using MINgo.Flight;
using UnityEngine;

namespace MINgo.Landing
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(ArcadeAircraftController))]
    public sealed class LandingStateMachine : MonoBehaviour
    {
        public float stableContactSeconds = 0.35f;

        private Rigidbody body;
        private ArcadeAircraftController aircraft;
        private LandingSample pendingSample;
        private bool hasPendingLanding;
        private float stableTimer;
        private Collider pendingSurface;

        public LandingContext LastContext { get; private set; } = LandingContext.None;
        public LandingOutcome LastOutcome { get; private set; } = LandingOutcome.None;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            aircraft = GetComponent<ArcadeAircraftController>();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!TryCreateSample(collision, out LandingSample sample, out Collider surface))
            {
                return;
            }

            LandingResult result = LandingClassifier.Classify(sample);
            if (result.Outcome == LandingOutcome.Crashed || result.Outcome == LandingOutcome.Submerged)
            {
                Resolve(result);
                return;
            }

            pendingSample = sample;
            pendingSurface = surface;
            hasPendingLanding = true;
            stableTimer = 0f;
        }

        private void OnCollisionStay(Collision collision)
        {
            if (!hasPendingLanding || !IsPendingSurface(collision))
            {
                return;
            }

            stableTimer += Time.fixedDeltaTime;
            if (stableTimer >= stableContactSeconds)
            {
                hasPendingLanding = false;
                Resolve(LandingClassifier.Classify(pendingSample));
            }
        }

        private void OnCollisionExit(Collision collision)
        {
            if (IsPendingSurface(collision))
            {
                hasPendingLanding = false;
                stableTimer = 0f;
                pendingSurface = null;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            SurfaceTag tag = other.GetComponentInParent<SurfaceTag>();
            if (tag == null || tag.kind != SurfaceKind.Water)
            {
                return;
            }

            Resolve(new LandingResult(LandingOutcome.Submerged, LandingContext.Submerged));
        }

        private bool TryCreateSample(Collision collision, out LandingSample sample, out Collider surface)
        {
            sample = default;
            surface = null;

            if (collision.contactCount == 0)
            {
                return false;
            }

            ContactPoint contact = collision.GetContact(0);
            surface = contact.otherCollider;
            SurfaceKind kind = ResolveSurfaceKind(surface);

            float verticalImpact = Mathf.Abs(Vector3.Dot(collision.relativeVelocity, contact.normal));
            float forwardSpeed = Mathf.Abs(Vector3.Dot(body.linearVelocity, transform.forward));
            float slope = Vector3.Angle(contact.normal, Vector3.up);

            sample = new LandingSample(
                kind,
                verticalImpact,
                forwardSpeed,
                SignedAbsAngle(transform.eulerAngles.x),
                SignedAbsAngle(transform.eulerAngles.z),
                slope,
                aircraft.CurrentState);

            return kind != SurfaceKind.Unknown;
        }

        private bool IsPendingSurface(Collision collision)
        {
            if (pendingSurface == null || collision.contactCount == 0)
            {
                return false;
            }

            for (int i = 0; i < collision.contactCount; i++)
            {
                if (collision.GetContact(i).otherCollider == pendingSurface)
                {
                    return true;
                }
            }

            return false;
        }

        private static SurfaceKind ResolveSurfaceKind(Collider surface)
        {
            SurfaceTag tag = surface == null ? null : surface.GetComponentInParent<SurfaceTag>();
            return tag == null ? SurfaceKind.Unknown : tag.kind;
        }

        private static float SignedAbsAngle(float eulerDegrees)
        {
            return Mathf.Abs(Mathf.DeltaAngle(0f, eulerDegrees));
        }

        private void Resolve(LandingResult result)
        {
            LastOutcome = result.Outcome;
            LastContext = result.Context;

            switch (result.Outcome)
            {
                case LandingOutcome.Clean:
                    aircraft.SetAircraftState(AircraftState.Landed);
                    break;
                case LandingOutcome.Rough:
                    aircraft.SetAircraftState(AircraftState.Landed);
                    break;
                case LandingOutcome.Damaged:
                    aircraft.SetAircraftState(AircraftState.Damaged);
                    break;
                case LandingOutcome.Crashed:
                    aircraft.SetAircraftState(AircraftState.Crashed);
                    break;
                case LandingOutcome.Submerged:
                    aircraft.SetAircraftState(AircraftState.Submerged);
                    break;
            }
        }
    }
}

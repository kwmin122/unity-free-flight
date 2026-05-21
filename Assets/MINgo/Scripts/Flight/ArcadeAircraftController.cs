using UnityEngine;

namespace MINgo.Flight
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class ArcadeAircraftController : MonoBehaviour
    {
        public float maxThrust = 85f;
        public float lift = 0.55f;
        public float pitchTorque = 35f;
        public float rollTorque = 55f;
        public float yawTorque = 18f;
        public float stabilization = 3.5f;
        public float autoLevel = 6f;
        public float maxLiftForce = 95f;
        public float speedDrag = 0.18f;
        public float groundBrake = 16f;
        public float takeoffSpeed = 22f;
        public float throttleChangeRate = 0.6f;

        private Rigidbody body;
        private bool hasGroundContact = true;

        public float Throttle01 { get; private set; }
        public float SpeedMetersPerSecond => body == null ? 0f : body.linearVelocity.magnitude;
        public float AltitudeMeters => Mathf.Max(0f, transform.position.y);
        public AircraftState CurrentState { get; private set; } = AircraftState.Grounded;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            body.useGravity = true;
            body.linearDamping = 0.08f;
            body.angularDamping = 1.15f;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }

        private void FixedUpdate()
        {
            FlightInputSnapshot input = FlightInputReader.ReadKeyboard();
            Throttle01 = Mathf.Clamp01(Throttle01 + input.ThrottleDelta * throttleChangeRate * Time.fixedDeltaTime);

            if (CurrentState == AircraftState.Crashed || CurrentState == AircraftState.Submerged)
            {
                Throttle01 = 0f;
                body.linearVelocity *= 0.98f;
                body.angularVelocity *= 0.98f;
                return;
            }

            float forwardSpeed = Vector3.Dot(body.linearVelocity, transform.forward);
            float speed01 = Mathf.Clamp01(Mathf.Abs(forwardSpeed) / takeoffSpeed);

            body.AddForce(transform.forward * (maxThrust * Throttle01), ForceMode.Force);

            float liftForce = Mathf.Min(Mathf.Max(0f, forwardSpeed * forwardSpeed) * lift * 0.08f * body.mass, maxLiftForce);
            body.AddForce(Vector3.up * liftForce, ForceMode.Force);
            body.AddForce(-body.linearVelocity * speedDrag, ForceMode.Force);

            float controlAuthority = Mathf.Lerp(0.35f, 1f, speed01);
            body.AddTorque(-transform.right * (input.Pitch * pitchTorque * controlAuthority), ForceMode.Force);
            body.AddTorque(-transform.forward * (input.Roll * rollTorque * controlAuthority), ForceMode.Force);
            body.AddTorque(transform.up * (input.Yaw * yawTorque * controlAuthority), ForceMode.Force);

            if (Mathf.Abs(input.Pitch) < 0.05f && Mathf.Abs(input.Roll) < 0.05f && Mathf.Abs(input.Yaw) < 0.05f)
            {
                body.AddTorque(-body.angularVelocity * stabilization, ForceMode.Force);
                Vector3 levelCorrection = Vector3.Cross(transform.up, Vector3.up);
                body.AddTorque(levelCorrection * autoLevel, ForceMode.Force);
            }

            if (input.Brake && hasGroundContact)
            {
                body.AddForce(-body.linearVelocity * groundBrake, ForceMode.Force);
            }

            if (!hasGroundContact || SpeedMetersPerSecond >= takeoffSpeed)
            {
                CurrentState = AircraftState.Flying;
            }
            else if (CurrentState != AircraftState.Landed && CurrentState != AircraftState.Damaged)
            {
                CurrentState = AircraftState.Grounded;
            }
        }

        public void SetAircraftState(AircraftState state)
        {
            CurrentState = state;
        }

        private void OnCollisionStay(Collision collision)
        {
            hasGroundContact = true;
        }

        private void OnCollisionExit(Collision collision)
        {
            hasGroundContact = false;
        }
    }
}

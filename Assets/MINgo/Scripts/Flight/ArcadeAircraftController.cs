using UnityEngine;

namespace MINgo.Flight
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class ArcadeAircraftController : MonoBehaviour
    {
        public float maxThrust = 85f;
        public float lift = 0.55f;
        public float zeroLiftAngle = -2f;
        public float liftSlope = 0.08f;
        public float stallAngle = 16f;
        public float maxLiftCoefficient = 1.2f;
        public float pitchTorque = 35f;
        public float rollTorque = 55f;
        public float yawTorque = 18f;
        public float stabilization = 3.5f;
        public float autoLevel = 6f;
        public float maxLiftForce = 95f;
        public float speedDrag = 0.012f;
        public float inducedDrag = 0.04f;
        public float groundBrake = 16f;
        public float takeoffSpeed = 22f;
        public float throttleChangeRate = 0.6f;

        private Rigidbody body;
        private bool hasGroundContact = true;

        public float Throttle01 { get; private set; }
        public float SpeedMetersPerSecond => body == null ? 0f : body.linearVelocity.magnitude;
        public float AltitudeMeters => Mathf.Max(0f, transform.position.y);
        public float AngleOfAttackDegrees { get; private set; }
        public float LiftCoefficient { get; private set; }
        public AircraftState CurrentState { get; private set; } = AircraftState.Grounded;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            body.useGravity = true;
            body.linearDamping = 0.01f;
            body.angularDamping = 0.9f;
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

            Vector3 velocity = body.linearVelocity;
            Vector3 localVelocity = transform.InverseTransformDirection(velocity);
            float forwardSpeed = Mathf.Max(0f, localVelocity.z);
            float speed01 = Mathf.Clamp01(forwardSpeed / takeoffSpeed);

            body.AddForce(transform.forward * (maxThrust * Throttle01), ForceMode.Force);

            AngleOfAttackDegrees = FlightAerodynamics.CalculateAngleOfAttackDegrees(localVelocity);
            LiftCoefficient = FlightAerodynamics.EvaluateLiftCoefficient(
                AngleOfAttackDegrees,
                zeroLiftAngle,
                liftSlope,
                stallAngle,
                maxLiftCoefficient);

            Vector3 liftForce = FlightAerodynamics.CalculateLiftForce(velocity, transform.right, LiftCoefficient, lift);
            if (liftForce.magnitude > maxLiftForce)
            {
                liftForce = liftForce.normalized * maxLiftForce;
            }

            body.AddForce(liftForce, ForceMode.Force);
            body.AddForce(FlightAerodynamics.CalculateDragForce(velocity, speedDrag, inducedDrag, LiftCoefficient), ForceMode.Force);

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

            CurrentState = ResolveMotionState(CurrentState, hasGroundContact, SpeedMetersPerSecond, takeoffSpeed);
        }

        public void SetAircraftState(AircraftState state)
        {
            CurrentState = state;
        }

        public static AircraftState ResolveMotionState(
            AircraftState currentState,
            bool hasGroundContact,
            float speedMetersPerSecond,
            float takeoffSpeed)
        {
            if (currentState == AircraftState.Crashed
                || currentState == AircraftState.Submerged
                || currentState == AircraftState.Damaged)
            {
                return currentState;
            }

            if (!hasGroundContact || speedMetersPerSecond >= takeoffSpeed)
            {
                return AircraftState.Flying;
            }

            return currentState == AircraftState.Landed ? AircraftState.Landed : AircraftState.Grounded;
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

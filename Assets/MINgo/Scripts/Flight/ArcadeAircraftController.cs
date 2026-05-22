using UnityEngine;

namespace MINgo.Flight
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class ArcadeAircraftController : MonoBehaviour
    {
        public float maxThrust = 52f;
        public float lift = 0.55f;
        public float zeroLiftAngle = -2f;
        public float liftSlope = 0.08f;
        public float stallAngle = 16f;
        public float maxLiftCoefficient = 1.2f;
        public float pitchTorque = 35f;
        public float rollTorque = 55f;
        public float yawTorque = 18f;
        public float stabilization = 6f;
        public float autoLevel = 12f;
        public float autoLevelRotationRate = 4f;
        public float assistedBankAngle = 22f;
        public float bankAssistResponse = 24f;
        public float turnYawAssist = 0.45f;
        public float takeoffAssistPitch = 0.3f;
        public float takeoffAssistStart01 = 0.82f;
        public float takeoffLiftAssist = 130f;
        public float maxLiftForce = 95f;
        public float speedDrag = 0.012f;
        public float inducedDrag = 0.04f;
        public float airbrakeDrag = 0.01f;
        public float slowdownDescentAcceleration = 18f;
        public float slowdownPitchDamping = 0.55f;
        public float idleCoastDrag = 0.006f;
        public float groundBrake = 16f;
        public float takeoffSpeed = 22f;
        public float throttleChangeRate = 3.2f;
        public float neutralThrottleReleaseRate = 1.4f;
        public float groundRunAcceleration = 7f;
        public float groundSteeringYawRateDegrees = 28f;
        public float groundSteeringFullSpeed = 12f;
        public bool acceptsInput = true;

        private Rigidbody body;
        private bool hasGroundContact = true;

        public float Throttle01 { get; private set; }
        public float SpeedMetersPerSecond => body == null ? 0f : body.linearVelocity.magnitude;
        public float ForwardSpeedMetersPerSecond
        {
            get
            {
                if (body == null)
                {
                    return 0f;
                }

                return Mathf.Max(0f, transform.InverseTransformDirection(body.linearVelocity).z);
            }
        }

        public float AltitudeMeters => Mathf.Max(0f, transform.position.y);
        public float AngleOfAttackDegrees { get; private set; }
        public float LiftCoefficient { get; private set; }
        public float RollDegrees { get; private set; }
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
            FlightInputSnapshot input = acceptsInput
                ? FlightInputReader.ReadKeyboard()
                : new FlightInputSnapshot(0f, 0f, 0f, 0f, false);
            Throttle01 = UpdateThrottleForGtaHold(
                Throttle01,
                input.ThrottleDelta,
                throttleChangeRate,
                neutralThrottleReleaseRate,
                Time.fixedDeltaTime);

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
            RollDegrees = FlightControlAssist.CalculateRollDegrees(transform.up, transform.forward);

            body.AddForce(transform.forward * (maxThrust * Throttle01), ForceMode.Force);
            if (hasGroundContact && Throttle01 > 0.05f)
            {
                body.AddForce(transform.forward * (groundRunAcceleration * Throttle01), ForceMode.Acceleration);
            }

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
            if (Throttle01 > 0.85f && forwardSpeed > takeoffSpeed * 0.72f && AltitudeMeters < 12f)
            {
                body.AddForce(Vector3.up * takeoffLiftAssist, ForceMode.Acceleration);
            }

            body.AddForce(FlightAerodynamics.CalculateDragForce(velocity, speedDrag, inducedDrag, LiftCoefficient), ForceMode.Force);
            body.AddForce(CalculateIdleCoastDrag(velocity, input.ThrottleDelta, hasGroundContact, idleCoastDrag), ForceMode.Force);
            if (input.ThrottleDelta < -0.05f && !hasGroundContact && velocity.sqrMagnitude > 1f)
            {
                body.AddForce(-velocity.normalized * (velocity.sqrMagnitude * airbrakeDrag), ForceMode.Force);
                body.AddForce(Vector3.down * slowdownDescentAcceleration, ForceMode.Acceleration);
                Vector3 localAngularVelocity = transform.InverseTransformDirection(body.angularVelocity);
                localAngularVelocity.x *= slowdownPitchDamping;
                body.angularVelocity = transform.TransformDirection(localAngularVelocity);
            }

            float controlAuthority = Mathf.Lerp(0.35f, 1f, speed01);
            FlightControlOutput controls = FlightControlAssist.CalculateAssistedControls(
                input,
                RollDegrees,
                forwardSpeed,
                takeoffSpeed,
                Throttle01,
                hasGroundContact,
                assistedBankAngle,
                bankAssistResponse,
                turnYawAssist,
                takeoffAssistPitch,
                takeoffAssistStart01);

            ApplyGroundSteering(input.Turn, forwardSpeed);
            body.AddTorque(-transform.right * (controls.Pitch * pitchTorque * controlAuthority), ForceMode.Force);
            body.AddTorque(-transform.forward * (controls.Roll * rollTorque * controlAuthority), ForceMode.Force);
            body.AddTorque(transform.up * (controls.Yaw * yawTorque * controlAuthority), ForceMode.Force);

            bool playerReleasedControls = Mathf.Abs(input.Pitch) < 0.05f
                && Mathf.Abs(input.Roll) < 0.05f
                && Mathf.Abs(input.Yaw) < 0.05f
                && Mathf.Abs(input.Turn) < 0.05f;
            if (playerReleasedControls)
            {
                body.AddTorque(-body.angularVelocity * stabilization, ForceMode.Force);
                Vector3 levelCorrection = Vector3.Cross(transform.up, Vector3.up);
                body.AddTorque(levelCorrection * autoLevel, ForceMode.Force);
                Vector3 localAngularVelocity = transform.InverseTransformDirection(body.angularVelocity);
                localAngularVelocity.z *= 0.35f;
                body.angularVelocity = transform.TransformDirection(localAngularVelocity);
                if (transform.forward.sqrMagnitude > 0.001f)
                {
                    Quaternion levelRotation = Quaternion.LookRotation(transform.forward, Vector3.up);
                    body.MoveRotation(Quaternion.Slerp(
                        body.rotation,
                        levelRotation,
                        Mathf.Clamp01(autoLevelRotationRate * Time.fixedDeltaTime)));
                }
            }

            if (input.Brake && hasGroundContact)
            {
                body.AddForce(-body.linearVelocity * groundBrake, ForceMode.Force);
            }

            CurrentState = ResolveMotionState(CurrentState, hasGroundContact, SpeedMetersPerSecond, takeoffSpeed);
        }

        private void ApplyGroundSteering(float turnInput, float forwardSpeed)
        {
            if (!hasGroundContact || Mathf.Abs(turnInput) < 0.05f || forwardSpeed < 0.75f)
            {
                return;
            }

            float speedBlend = Mathf.Clamp01(forwardSpeed / groundSteeringFullSpeed);
            float yawDelta = turnInput * groundSteeringYawRateDegrees * speedBlend * Time.fixedDeltaTime;
            body.MoveRotation(Quaternion.AngleAxis(yawDelta, Vector3.up) * body.rotation);

            Vector3 localVelocity = transform.InverseTransformDirection(body.linearVelocity);
            localVelocity.x *= 0.82f;
            body.linearVelocity = transform.TransformDirection(localVelocity);
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

        public static float UpdateThrottleForGtaHold(
            float currentThrottle,
            float throttleInput,
            float responseRate,
            float neutralReleaseRate,
            float deltaTime)
        {
            if (throttleInput > 0.05f)
            {
                return Mathf.MoveTowards(currentThrottle, 1f, responseRate * deltaTime);
            }

            if (throttleInput < -0.05f)
            {
                return Mathf.MoveTowards(currentThrottle, 0f, responseRate * deltaTime);
            }

            return Mathf.MoveTowards(currentThrottle, 0f, neutralReleaseRate * deltaTime);
        }

        public static Vector3 CalculateIdleCoastDrag(
            Vector3 velocity,
            float throttleInput,
            bool isGrounded,
            float dragFactor)
        {
            if (isGrounded || throttleInput > 0.05f || velocity.sqrMagnitude <= 1f || dragFactor <= 0f)
            {
                return Vector3.zero;
            }

            return -velocity.normalized * (velocity.sqrMagnitude * dragFactor);
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

using UnityEngine;

namespace MINgo.Vehicles
{
    public enum DriveMode
    {
        Coasting,
        Forward,
        Braking,
        Reverse
    }

    [RequireComponent(typeof(Rigidbody))]
    public sealed class ArcadeCarController : MonoBehaviour
    {
        public float acceleration = 34f;
        public float brakeAcceleration = 48f;
        public float reverseAcceleration = 18f;
        public float maxForwardSpeed = 38f;
        public float maxReverseSpeed = 10f;
        public float maxSteerDegrees = 32f;
        public float fullSteerSpeed = 6f;
        public float reducedSteerSpeed = 28f;
        public float lateralGrip = 8f;
        public float handbrakeGrip = 2.2f;
        public float reverseThreshold = 1.5f;
        public bool acceptsInput;

        private Rigidbody body;

        public float SpeedMetersPerSecond => body == null ? 0f : body.linearVelocity.magnitude;
        public DriveMode CurrentDriveMode { get; private set; }

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            body.mass = 950f;
            body.linearDamping = 0.08f;
            body.angularDamping = 1.8f;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }

        private void FixedUpdate()
        {
            VehicleInputSnapshot input = acceptsInput
                ? VehicleInputReader.ReadKeyboard()
                : new VehicleInputSnapshot(0f, 0f, false, false);

            Vector3 localVelocity = transform.InverseTransformDirection(body.linearVelocity);
            float forwardSpeed = localVelocity.z;
            CurrentDriveMode = ResolveDriveMode(input.Throttle, forwardSpeed, reverseThreshold);

            ApplyDriveForce(CurrentDriveMode, forwardSpeed);
            ApplySteering(input.Steer);
            ApplyLateralGrip(input.Handbrake);
        }

        public static DriveMode ResolveDriveMode(float throttleInput, float forwardSpeed, float reverseThreshold)
        {
            if (throttleInput > 0.05f)
            {
                return DriveMode.Forward;
            }

            if (throttleInput < -0.05f)
            {
                return forwardSpeed > reverseThreshold ? DriveMode.Braking : DriveMode.Reverse;
            }

            return DriveMode.Coasting;
        }

        public static float CalculateSteeringDegrees(
            float steerInput,
            float speedMetersPerSecond,
            float maxSteerDegrees,
            float fullSteerSpeed,
            float reducedSteerSpeed)
        {
            float speedBlend = Mathf.InverseLerp(fullSteerSpeed, reducedSteerSpeed, Mathf.Abs(speedMetersPerSecond));
            float speedScale = Mathf.Lerp(1f, 0.42f, speedBlend);
            return Mathf.Clamp(steerInput, -1f, 1f) * maxSteerDegrees * speedScale;
        }

        private void ApplyDriveForce(DriveMode mode, float forwardSpeed)
        {
            switch (mode)
            {
                case DriveMode.Forward:
                    if (forwardSpeed < maxForwardSpeed)
                    {
                        body.AddForce(transform.forward * acceleration, ForceMode.Acceleration);
                    }
                    break;
                case DriveMode.Braking:
                    body.AddForce(-transform.forward * brakeAcceleration, ForceMode.Acceleration);
                    break;
                case DriveMode.Reverse:
                    if (forwardSpeed > -maxReverseSpeed)
                    {
                        body.AddForce(-transform.forward * reverseAcceleration, ForceMode.Acceleration);
                    }
                    break;
            }
        }

        private void ApplySteering(float steerInput)
        {
            if (Mathf.Abs(steerInput) <= 0.05f || body.linearVelocity.sqrMagnitude < 0.25f)
            {
                return;
            }

            float steerDegrees = CalculateSteeringDegrees(
                steerInput,
                body.linearVelocity.magnitude,
                maxSteerDegrees,
                fullSteerSpeed,
                reducedSteerSpeed);
            float turnRadians = steerDegrees * Mathf.Deg2Rad * Time.fixedDeltaTime;
            body.MoveRotation(body.rotation * Quaternion.Euler(0f, turnRadians * Mathf.Rad2Deg, 0f));
        }

        private void ApplyLateralGrip(bool handbrake)
        {
            Vector3 localVelocity = transform.InverseTransformDirection(body.linearVelocity);
            float grip = handbrake ? handbrakeGrip : lateralGrip;
            localVelocity.x = Mathf.MoveTowards(localVelocity.x, 0f, grip * Time.fixedDeltaTime * Mathf.Max(1f, Mathf.Abs(localVelocity.x)));
            body.linearVelocity = transform.TransformDirection(localVelocity);
        }
    }
}

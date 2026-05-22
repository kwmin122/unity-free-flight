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
        public WheelCollider frontLeftWheel;
        public WheelCollider frontRightWheel;
        public WheelCollider rearLeftWheel;
        public WheelCollider rearRightWheel;
        public Transform frontLeftVisual;
        public Transform frontRightVisual;
        public Transform rearLeftVisual;
        public Transform rearRightVisual;
        public float motorTorque = 950f;
        public float reverseTorque = 420f;
        public float brakeTorque = 2600f;
        public float coastBrakeTorque = 260f;
        public float handbrakeTorque = 4200f;
        public float maxForwardSpeed = 34f;
        public float maxReverseSpeed = 9f;
        public float maxSteerDegrees = 28f;
        public float fullSteerSpeed = 5f;
        public float reducedSteerSpeed = 24f;
        public float reverseThreshold = 1.2f;
        public float downforce = 28f;
        public float antiRollForce = 6500f;
        public Vector3 centerOfMass = new Vector3(0f, -0.45f, 0.15f);
        public bool acceptsInput;

        private Rigidbody body;
        private WheelCollider[] driveWheels;
        private WheelCollider[] allWheels;

        public float SpeedMetersPerSecond => body == null ? 0f : body.linearVelocity.magnitude;
        public DriveMode CurrentDriveMode { get; private set; }
        public int GroundedWheelCount { get; private set; }
        public float RollDegrees => Mathf.DeltaAngle(0f, transform.eulerAngles.z);

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            body.mass = 950f;
            body.centerOfMass = centerOfMass;
            body.linearDamping = 0.04f;
            body.angularDamping = 1.2f;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            allWheels = new[] { frontLeftWheel, frontRightWheel, rearLeftWheel, rearRightWheel };
            driveWheels = allWheels;
        }

        private void FixedUpdate()
        {
            if (!HasRequiredWheels())
            {
                return;
            }

            VehicleInputSnapshot input = acceptsInput
                ? VehicleInputReader.ReadKeyboard()
                : new VehicleInputSnapshot(0f, 0f, false, false);

            Vector3 localVelocity = transform.InverseTransformDirection(body.linearVelocity);
            float forwardSpeed = localVelocity.z;
            CurrentDriveMode = ResolveDriveMode(input.Throttle, forwardSpeed, reverseThreshold);

            ApplyWheelControls(input, forwardSpeed);
            ApplyDownforce();
            ApplyAntiRoll(frontLeftWheel, frontRightWheel);
            ApplyAntiRoll(rearLeftWheel, rearRightWheel);
            UpdateGroundedWheelCount();
            UpdateWheelVisuals();
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

        public static float CalculateMotorTorque(
            DriveMode mode,
            float forwardSpeed,
            float maxForwardSpeed,
            float maxReverseSpeed,
            float motorTorque,
            float reverseTorque)
        {
            switch (mode)
            {
                case DriveMode.Forward:
                    if (forwardSpeed < maxForwardSpeed)
                    {
                        return motorTorque;
                    }
                    break;
                case DriveMode.Reverse:
                    if (forwardSpeed > -maxReverseSpeed)
                    {
                        return -reverseTorque;
                    }
                    break;
            }

            return 0f;
        }

        public static float CalculateBrakeTorque(
            DriveMode mode,
            bool handbrake,
            float brakeTorque,
            float coastBrakeTorque,
            float handbrakeTorque)
        {
            if (handbrake)
            {
                return handbrakeTorque;
            }

            if (mode == DriveMode.Braking)
            {
                return brakeTorque;
            }

            return mode == DriveMode.Coasting ? coastBrakeTorque : 0f;
        }

        private void ApplyWheelControls(VehicleInputSnapshot input, float forwardSpeed)
        {
            float steerDegrees = CalculateSteeringDegrees(
                input.Steer,
                body.linearVelocity.magnitude,
                maxSteerDegrees,
                fullSteerSpeed,
                reducedSteerSpeed);

            float driveTorque = CalculateMotorTorque(
                CurrentDriveMode,
                forwardSpeed,
                maxForwardSpeed,
                maxReverseSpeed,
                motorTorque,
                reverseTorque);
            float brakingTorque = CalculateBrakeTorque(
                CurrentDriveMode,
                input.Handbrake,
                brakeTorque,
                coastBrakeTorque,
                handbrakeTorque);

            frontLeftWheel.steerAngle = steerDegrees;
            frontRightWheel.steerAngle = steerDegrees;

            foreach (WheelCollider wheel in driveWheels)
            {
                wheel.motorTorque = driveTorque / driveWheels.Length;
            }

            foreach (WheelCollider wheel in allWheels)
            {
                wheel.brakeTorque = brakingTorque;
            }
        }

        private void ApplyDownforce()
        {
            float speed = body.linearVelocity.magnitude;
            if (speed > 1f)
            {
                body.AddForce(Vector3.down * (speed * speed * downforce), ForceMode.Force);
            }
        }

        private void ApplyAntiRoll(WheelCollider leftWheel, WheelCollider rightWheel)
        {
            float leftTravel = 1f;
            float rightTravel = 1f;
            bool leftGrounded = leftWheel.GetGroundHit(out WheelHit leftHit);
            bool rightGrounded = rightWheel.GetGroundHit(out WheelHit rightHit);

            if (leftGrounded)
            {
                leftTravel = (-leftWheel.transform.InverseTransformPoint(leftHit.point).y - leftWheel.radius) / leftWheel.suspensionDistance;
            }

            if (rightGrounded)
            {
                rightTravel = (-rightWheel.transform.InverseTransformPoint(rightHit.point).y - rightWheel.radius) / rightWheel.suspensionDistance;
            }

            float force = (leftTravel - rightTravel) * antiRollForce;
            if (leftGrounded)
            {
                body.AddForceAtPosition(leftWheel.transform.up * -force, leftWheel.transform.position);
            }

            if (rightGrounded)
            {
                body.AddForceAtPosition(rightWheel.transform.up * force, rightWheel.transform.position);
            }
        }

        private void UpdateGroundedWheelCount()
        {
            int grounded = 0;
            foreach (WheelCollider wheel in allWheels)
            {
                if (wheel.isGrounded)
                {
                    grounded++;
                }
            }

            GroundedWheelCount = grounded;
        }

        private void UpdateWheelVisuals()
        {
            UpdateWheelVisual(frontLeftWheel, frontLeftVisual);
            UpdateWheelVisual(frontRightWheel, frontRightVisual);
            UpdateWheelVisual(rearLeftWheel, rearLeftVisual);
            UpdateWheelVisual(rearRightWheel, rearRightVisual);
        }

        private static void UpdateWheelVisual(WheelCollider wheel, Transform visual)
        {
            if (wheel == null || visual == null)
            {
                return;
            }

            wheel.GetWorldPose(out Vector3 position, out Quaternion rotation);
            visual.position = position;
            visual.rotation = rotation * Quaternion.Euler(0f, 0f, 90f);
        }

        private bool HasRequiredWheels()
        {
            return frontLeftWheel != null
                && frontRightWheel != null
                && rearLeftWheel != null
                && rearRightWheel != null
                && allWheels != null
                && driveWheels != null;
        }
    }
}

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
        public float motorTorque = 1450f;
        public float reverseTorque = 950f;
        public float wheelMotorTorqueScale;
        public float brakeTorque = 2600f;
        public float coastBrakeTorque;
        public float handbrakeTorque = 4200f;
        public float driveAssistAcceleration = 92f;
        public float reverseAssistAcceleration = 75f;
        public float maxForwardSpeed = 20f;
        public float maxReverseSpeed = 9f;
        public float maxSteerDegrees = 28f;
        public float fullSteerSpeed = 5f;
        public float reducedSteerSpeed = 24f;
        public float reverseThreshold = 1f;
        public float reverseSteerScale = 0.65f;
        public float neutralCoastAcceleration = 1.1f;
        public float directionChangeBrakeAcceleration = 16f;
        public float steeringYawRateDegrees = 14f;
        public float reverseEngageDelay = 2f;
        public float handbrakeYawAcceleration = 200f;
        public float handbrakeYawRateDegrees = 30f;
        public float handbrakeMinimumSpeed = 8f;
        public float handbrakeMaximumAssistSpeed = 12f;
        public float groundStickAcceleration = 18f;
        public float downforce = 28f;
        public float antiRollForce = 6500f;
        public float lowGroundSupportHeight = 1.5f;
        public Vector3 centerOfMass = new Vector3(0f, -0.45f, 0.15f);
        public bool acceptsInput;

        private Rigidbody body;
        private WheelCollider[] driveWheels;
        private WheelCollider[] allWheels;
        private float reverseEngageTimer;

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
            body.sleepThreshold = 0f;
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
            WakeBodyForPlayerInput(input);

            Vector3 localVelocity = transform.InverseTransformDirection(body.linearVelocity);
            float forwardSpeed = localVelocity.z;
            CurrentDriveMode = ResolveDriveModeWithReverseDelay(input.Throttle, forwardSpeed);
            UpdateGroundedWheelCount();

            ApplyWheelControls(input, forwardSpeed);
            ApplyDriveAssist(CurrentDriveMode, forwardSpeed);
            ApplyDirectionChangeBrake(input, forwardSpeed);
            ApplyNeutralCoastAssist(input, forwardSpeed);
            ApplySpeedLimiter();
            ApplySteeringYawAssist(input, forwardSpeed);
            ApplyHandbrakeTurnAssist(input);
            ApplyGroundStickAssist();
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

        private DriveMode ResolveDriveModeWithReverseDelay(float throttleInput, float forwardSpeed)
        {
            DriveMode mode = ResolveDriveMode(throttleInput, forwardSpeed, reverseThreshold);
            if (throttleInput >= -0.05f)
            {
                reverseEngageTimer = 0f;
                return mode;
            }

            if (mode == DriveMode.Braking)
            {
                reverseEngageTimer = reverseEngageDelay;
                return mode;
            }

            if (mode == DriveMode.Reverse && reverseEngageTimer > 0f)
            {
                reverseEngageTimer = Mathf.Max(0f, reverseEngageTimer - Time.fixedDeltaTime);
                return DriveMode.Braking;
            }

            return mode;
        }

        private void ApplyWheelControls(VehicleInputSnapshot input, float forwardSpeed)
        {
            float steerInput = CurrentDriveMode == DriveMode.Reverse
                ? input.Steer * reverseSteerScale
                : input.Steer;
            float steerDegrees = CalculateSteeringDegrees(
                steerInput,
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
                reverseTorque) * wheelMotorTorqueScale;
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
                wheel.motorTorque = driveTorque;
            }

            foreach (WheelCollider wheel in allWheels)
            {
                wheel.brakeTorque = brakingTorque;
            }
        }

        private void WakeBodyForPlayerInput(VehicleInputSnapshot input)
        {
            if (Mathf.Abs(input.Throttle) > 0.05f || Mathf.Abs(input.Steer) > 0.05f || input.Handbrake)
            {
                body.WakeUp();
            }
        }

        private void ApplyDriveAssist(DriveMode mode, float forwardSpeed)
        {
            if (!HasGroundSupport())
            {
                return;
            }

            if (mode == DriveMode.Forward && forwardSpeed < maxForwardSpeed)
            {
                body.AddForce(transform.forward * driveAssistAcceleration, ForceMode.Acceleration);
            }
            else if (mode == DriveMode.Reverse && forwardSpeed > -maxReverseSpeed)
            {
                body.AddForce(-transform.forward * reverseAssistAcceleration, ForceMode.Acceleration);
            }
        }

        private void ApplyDirectionChangeBrake(VehicleInputSnapshot input, float forwardSpeed)
        {
            if (!HasGroundSupport())
            {
                return;
            }

            bool brakingForward = forwardSpeed > 0.5f && input.Throttle < -0.05f;
            bool brakingReverse = forwardSpeed < -0.5f && input.Throttle > 0.05f;
            if (!brakingForward && !brakingReverse)
            {
                return;
            }

            Vector3 localVelocity = transform.InverseTransformDirection(body.linearVelocity);
            float brakeDelta = Mathf.Sign(forwardSpeed) * directionChangeBrakeAcceleration * Time.fixedDeltaTime;
            if (Mathf.Abs(brakeDelta) > Mathf.Abs(localVelocity.z))
            {
                localVelocity.z = 0f;
            }
            else
            {
                localVelocity.z -= brakeDelta;
            }

            body.linearVelocity = transform.TransformDirection(localVelocity);
        }

        private void ApplyNeutralCoastAssist(VehicleInputSnapshot input, float forwardSpeed)
        {
            if (!HasGroundSupport() || Mathf.Abs(input.Throttle) > 0.05f || Mathf.Abs(forwardSpeed) < 0.5f)
            {
                return;
            }

            Vector3 localVelocity = transform.InverseTransformDirection(body.linearVelocity);
            float brakingDelta = Mathf.Sign(forwardSpeed) * neutralCoastAcceleration * Time.fixedDeltaTime;
            if (Mathf.Abs(brakingDelta) > Mathf.Abs(localVelocity.z))
            {
                localVelocity.z = 0f;
            }
            else
            {
                localVelocity.z -= brakingDelta;
            }

            body.linearVelocity = transform.TransformDirection(localVelocity);
        }

        private void ApplySpeedLimiter()
        {
            Vector3 localVelocity = transform.InverseTransformDirection(body.linearVelocity);
            float clampedForward = Mathf.Clamp(localVelocity.z, -maxReverseSpeed, maxForwardSpeed);
            if (Mathf.Approximately(clampedForward, localVelocity.z))
            {
                return;
            }

            localVelocity.z = clampedForward;
            body.linearVelocity = transform.TransformDirection(localVelocity);
        }

        private void ApplySteeringYawAssist(VehicleInputSnapshot input, float forwardSpeed)
        {
            if (input.Handbrake || !HasGroundSupport() || Mathf.Abs(input.Steer) < 0.05f)
            {
                return;
            }

            float speedBlend = Mathf.Clamp01(Mathf.Abs(forwardSpeed) / 8f);
            if (speedBlend <= 0f)
            {
                return;
            }

            float yawDelta = input.Steer * steeringYawRateDegrees * speedBlend * Time.fixedDeltaTime;
            body.MoveRotation(Quaternion.Euler(0f, yawDelta, 0f) * body.rotation);
        }

        private void ApplyHandbrakeTurnAssist(VehicleInputSnapshot input)
        {
            if (!input.Handbrake || !HasGroundSupport() || Mathf.Abs(input.Steer) < 0.05f)
            {
                return;
            }

            float speed = body.linearVelocity.magnitude;
            if (speed < handbrakeMinimumSpeed || speed > handbrakeMaximumAssistSpeed + 4f)
            {
                return;
            }

            float speedAssist = Mathf.InverseLerp(handbrakeMinimumSpeed, handbrakeMaximumAssistSpeed, speed);
            float yawAssist = Mathf.Max(0.8f, speedAssist);
            float yawDelta = input.Steer * handbrakeYawRateDegrees * yawAssist * Time.fixedDeltaTime;
            body.rotation = Quaternion.AngleAxis(yawDelta, Vector3.up) * body.rotation;
            body.AddTorque(Vector3.up * (input.Steer * handbrakeYawAcceleration * Mathf.Max(0.35f, speedAssist)), ForceMode.Acceleration);
        }

        private void ApplyGroundStickAssist()
        {
            if (HasGroundSupport() && body.linearVelocity.sqrMagnitude > 0.25f)
            {
                body.AddForce(Vector3.down * groundStickAcceleration, ForceMode.Acceleration);
            }
        }

        private bool HasGroundSupport()
        {
            if (GroundedWheelCount >= 2)
            {
                return true;
            }

            if (transform.position.y < lowGroundSupportHeight)
            {
                return true;
            }

            Ray ray = new Ray(transform.position + Vector3.up * 1.2f, Vector3.down);
            RaycastHit[] hits = Physics.RaycastAll(ray, 2.5f, ~0, QueryTriggerInteraction.Ignore);
            foreach (RaycastHit hit in hits)
            {
                if (!hit.collider.transform.IsChildOf(transform))
                {
                    return true;
                }
            }

            return false;
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

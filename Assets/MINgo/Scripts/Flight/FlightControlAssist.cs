using UnityEngine;

namespace MINgo.Flight
{
    public readonly struct FlightControlOutput
    {
        public FlightControlOutput(float pitch, float roll, float yaw)
        {
            Pitch = pitch;
            Roll = roll;
            Yaw = yaw;
        }

        public float Pitch { get; }
        public float Roll { get; }
        public float Yaw { get; }
    }

    public static class FlightControlAssist
    {
        public static float CalculateRollDegrees(Vector3 aircraftUp, Vector3 aircraftForward)
        {
            if (aircraftForward.sqrMagnitude < 0.0001f)
            {
                return 0f;
            }

            Vector3 projectedUp = Vector3.ProjectOnPlane(aircraftUp, aircraftForward.normalized);
            if (projectedUp.sqrMagnitude < 0.0001f)
            {
                return 0f;
            }

            return Vector3.SignedAngle(Vector3.up, projectedUp.normalized, aircraftForward.normalized);
        }

        public static FlightControlOutput CalculateAssistedControls(
            FlightInputSnapshot input,
            float currentRollDegrees,
            float forwardSpeed,
            float takeoffSpeed,
            float throttle01,
            bool hasGroundContact,
            float assistedBankAngleDegrees = 35f,
            float bankResponseDegrees = 24f,
            float turnYawAssist = 0.45f,
            float takeoffAssistPitch = 0.3f,
            float takeoffAssistStart01 = 0.82f,
            float stallGuardSpeed01 = 0.72f,
            float stallPitchLimit = 0.35f,
            float stallRecoveryPitch = -0.22f)
        {
            float turn = Mathf.Clamp(input.Turn, -1f, 1f);
            float manualRoll = Mathf.Clamp(input.Roll, -1f, 1f);
            float targetRoll = -turn * assistedBankAngleDegrees;
            float rollError = Mathf.DeltaAngle(currentRollDegrees, targetRoll);
            float response = Mathf.Max(1f, bankResponseDegrees);
            float assistedRoll = Mathf.Abs(turn) > 0.05f
                ? Mathf.Clamp(-rollError / response, -1f, 1f)
                : 0f;

            if (Mathf.Abs(manualRoll) > 0.05f)
            {
                assistedRoll = 0f;
            }

            float pitch = Mathf.Clamp(input.Pitch + CalculateTakeoffAssistPitch(
                input.Pitch,
                forwardSpeed,
                takeoffSpeed,
                throttle01,
                hasGroundContact,
                takeoffAssistPitch,
                takeoffAssistStart01), -1f, 1f);
            pitch = ApplyStallGuardPitch(
                pitch,
                input.Pitch,
                forwardSpeed,
                takeoffSpeed,
                hasGroundContact,
                stallGuardSpeed01,
                stallPitchLimit,
                stallRecoveryPitch);
            float roll = Mathf.Clamp(manualRoll + assistedRoll, -1f, 1f);
            float yaw = Mathf.Clamp(input.Yaw + turn * turnYawAssist, -1f, 1f);

            return new FlightControlOutput(pitch, roll, yaw);
        }

        private static float CalculateTakeoffAssistPitch(
            float pitchInput,
            float forwardSpeed,
            float takeoffSpeed,
            float throttle01,
            bool hasGroundContact,
            float takeoffAssistPitch,
            float takeoffAssistStart01)
        {
            if (!hasGroundContact || Mathf.Abs(pitchInput) > 0.05f || takeoffSpeed <= 0f)
            {
                return 0f;
            }

            float speed01 = Mathf.Clamp01(forwardSpeed / takeoffSpeed);
            float speedBlend = Mathf.InverseLerp(takeoffAssistStart01, 1f, speed01);
            float throttleBlend = Mathf.InverseLerp(0.65f, 1f, throttle01);
            return takeoffAssistPitch * speedBlend * throttleBlend;
        }

        private static float ApplyStallGuardPitch(
            float pitch,
            float pitchInput,
            float forwardSpeed,
            float takeoffSpeed,
            bool hasGroundContact,
            float stallGuardSpeed01,
            float stallPitchLimit,
            float stallRecoveryPitch)
        {
            if (hasGroundContact || takeoffSpeed <= 0f)
            {
                return pitch;
            }

            float speed01 = Mathf.Clamp01(forwardSpeed / takeoffSpeed);
            if (speed01 >= stallGuardSpeed01)
            {
                return pitch;
            }

            if (pitch > stallPitchLimit)
            {
                pitch = stallPitchLimit;
            }

            if (Mathf.Abs(pitchInput) <= 0.05f)
            {
                float recoveryBlend = Mathf.Clamp01((stallGuardSpeed01 - speed01) / Mathf.Max(0.001f, stallGuardSpeed01));
                pitch += stallRecoveryPitch * recoveryBlend;
            }

            return Mathf.Clamp(pitch, -1f, 1f);
        }
    }
}

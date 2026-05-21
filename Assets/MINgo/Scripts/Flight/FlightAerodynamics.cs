using UnityEngine;

namespace MINgo.Flight
{
    public static class FlightAerodynamics
    {
        private const float MinimumAirspeedSquared = 0.0001f;

        public static float CalculateAngleOfAttackDegrees(Vector3 localVelocity)
        {
            if (localVelocity.sqrMagnitude < MinimumAirspeedSquared)
            {
                return 0f;
            }

            float forwardSpeed = Mathf.Max(0.001f, Mathf.Abs(localVelocity.z));
            return Mathf.Atan2(-localVelocity.y, forwardSpeed) * Mathf.Rad2Deg;
        }

        public static float EvaluateLiftCoefficient(
            float angleOfAttackDegrees,
            float zeroLiftAngleDegrees,
            float liftSlopePerDegree,
            float stallAngleDegrees,
            float maxLiftCoefficient)
        {
            float coefficient = (angleOfAttackDegrees - zeroLiftAngleDegrees) * liftSlopePerDegree;
            coefficient = Mathf.Clamp(coefficient, -maxLiftCoefficient, maxLiftCoefficient);

            float stallStart = Mathf.Max(0.001f, Mathf.Abs(stallAngleDegrees));
            float stallAmount = Mathf.InverseLerp(stallStart, stallStart * 2.5f, Mathf.Abs(angleOfAttackDegrees));
            return coefficient * Mathf.Lerp(1f, 0.35f, stallAmount);
        }

        public static Vector3 CalculateLiftDirection(Vector3 velocity, Vector3 aircraftRight)
        {
            if (velocity.sqrMagnitude < MinimumAirspeedSquared || aircraftRight.sqrMagnitude < MinimumAirspeedSquared)
            {
                return Vector3.zero;
            }

            Vector3 liftDirection = Vector3.Cross(velocity.normalized, aircraftRight.normalized);
            return liftDirection.sqrMagnitude < MinimumAirspeedSquared ? Vector3.zero : liftDirection.normalized;
        }

        public static Vector3 CalculateLiftForce(Vector3 velocity, Vector3 aircraftRight, float liftCoefficient, float liftPower)
        {
            Vector3 liftDirection = CalculateLiftDirection(velocity, aircraftRight);
            if (liftDirection == Vector3.zero)
            {
                return Vector3.zero;
            }

            return liftDirection * (velocity.sqrMagnitude * liftCoefficient * liftPower);
        }

        public static Vector3 CalculateDragForce(Vector3 velocity, float dragCoefficient, float inducedDragCoefficient, float liftCoefficient)
        {
            if (velocity.sqrMagnitude < MinimumAirspeedSquared)
            {
                return Vector3.zero;
            }

            float effectiveDrag = Mathf.Max(0f, dragCoefficient + inducedDragCoefficient * liftCoefficient * liftCoefficient);
            return -velocity.normalized * (velocity.sqrMagnitude * effectiveDrag);
        }
    }
}

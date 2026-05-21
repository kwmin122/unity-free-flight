using MINgo.Flight;
using UnityEngine;

namespace MINgo.Landing
{
    public readonly struct LandingResult
    {
        public LandingResult(LandingOutcome outcome, LandingContext context)
        {
            Outcome = outcome;
            Context = context;
        }

        public LandingOutcome Outcome { get; }
        public LandingContext Context { get; }
    }

    public static class LandingClassifier
    {
        private const float CrashImpactSpeed = 12f;
        private const float CleanImpactSpeed = 5.5f;
        private const float CleanMinForwardSpeed = 8f;
        private const float CleanMaxForwardSpeed = 45f;
        private const float CleanPitchDegrees = 14f;
        private const float CleanRollDegrees = 16f;
        private const float CleanSlopeDegrees = 18f;
        private const float RoughPitchDegrees = 22f;
        private const float RoughRollDegrees = 28f;
        private const float RoughSlopeDegrees = 24f;
        private const float CrashSlopeDegrees = 38f;
        private const float AttitudeCrashImpactSpeed = 7f;

        public static LandingResult Classify(LandingSample sample)
        {
            if (sample.SurfaceKind == SurfaceKind.Water)
            {
                return new LandingResult(LandingOutcome.Submerged, LandingContext.Submerged);
            }

            float impact = Mathf.Abs(sample.VerticalImpactSpeed);
            float forwardSpeed = Mathf.Abs(sample.ForwardSpeed);
            float pitch = Mathf.Abs(sample.PitchDegrees);
            float roll = Mathf.Abs(sample.RollDegrees);
            float slope = Mathf.Abs(sample.GroundSlopeDegrees);

            if (impact > CrashImpactSpeed)
            {
                return new LandingResult(LandingOutcome.Crashed, SurfaceContext(sample.SurfaceKind));
            }

            bool extremeAttitude = pitch > RoughPitchDegrees || roll > RoughRollDegrees;
            bool extremeSlope = slope > CrashSlopeDegrees;
            if ((extremeAttitude || extremeSlope) && impact > AttitudeCrashImpactSpeed)
            {
                return new LandingResult(LandingOutcome.Crashed, SurfaceContext(sample.SurfaceKind));
            }

            bool clean = impact <= CleanImpactSpeed
                && forwardSpeed >= CleanMinForwardSpeed
                && forwardSpeed <= CleanMaxForwardSpeed
                && pitch <= CleanPitchDegrees
                && roll <= CleanRollDegrees
                && slope <= CleanSlopeDegrees;

            if (clean)
            {
                if (sample.AircraftState == AircraftState.Damaged)
                {
                    return new LandingResult(LandingOutcome.Damaged, LandingContext.EmergencyLanding);
                }

                return new LandingResult(LandingOutcome.Clean, SurfaceContext(sample.SurfaceKind));
            }

            return new LandingResult(LandingOutcome.Rough, LandingContext.RoughLanding);
        }

        private static LandingContext SurfaceContext(SurfaceKind kind)
        {
            return kind switch
            {
                SurfaceKind.Runway => LandingContext.RunwayLanding,
                SurfaceKind.Road => LandingContext.RoadLanding,
                SurfaceKind.Field => LandingContext.FieldLanding,
                SurfaceKind.Ridge => LandingContext.RidgeLanding,
                SurfaceKind.CanyonFloor => LandingContext.CanyonFloorLanding,
                SurfaceKind.Water => LandingContext.Submerged,
                _ => LandingContext.RoughLanding
            };
        }
    }
}

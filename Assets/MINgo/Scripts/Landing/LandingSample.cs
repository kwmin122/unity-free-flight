using MINgo.Flight;

namespace MINgo.Landing
{
    public readonly struct LandingSample
    {
        public LandingSample(
            SurfaceKind surfaceKind,
            float verticalImpactSpeed,
            float forwardSpeed,
            float pitchDegrees,
            float rollDegrees,
            float groundSlopeDegrees,
            AircraftState aircraftState)
        {
            SurfaceKind = surfaceKind;
            VerticalImpactSpeed = verticalImpactSpeed;
            ForwardSpeed = forwardSpeed;
            PitchDegrees = pitchDegrees;
            RollDegrees = rollDegrees;
            GroundSlopeDegrees = groundSlopeDegrees;
            AircraftState = aircraftState;
        }

        public SurfaceKind SurfaceKind { get; }
        public float VerticalImpactSpeed { get; }
        public float ForwardSpeed { get; }
        public float PitchDegrees { get; }
        public float RollDegrees { get; }
        public float GroundSlopeDegrees { get; }
        public AircraftState AircraftState { get; }
    }
}

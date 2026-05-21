using MINgo.Flight;
using MINgo.Landing;
using NUnit.Framework;

namespace MINgo.Tests
{
    public sealed class LandingClassifierTests
    {
        [Test]
        public void CleanRunwayLandingPasses()
        {
            LandingResult result = LandingClassifier.Classify(new LandingSample(
                SurfaceKind.Runway,
                verticalImpactSpeed: 2.5f,
                forwardSpeed: 27f,
                pitchDegrees: 4f,
                rollDegrees: 2f,
                groundSlopeDegrees: 1f,
                aircraftState: AircraftState.Flying));

            Assert.AreEqual(LandingOutcome.Clean, result.Outcome);
            Assert.AreEqual(LandingContext.RunwayLanding, result.Context);
        }

        [Test]
        public void RoadLandingLabelsAsRoad()
        {
            LandingResult result = LandingClassifier.Classify(new LandingSample(
                SurfaceKind.Road,
                verticalImpactSpeed: 3.5f,
                forwardSpeed: 24f,
                pitchDegrees: 5f,
                rollDegrees: 4f,
                groundSlopeDegrees: 3f,
                aircraftState: AircraftState.Flying));

            Assert.AreEqual(LandingOutcome.Clean, result.Outcome);
            Assert.AreEqual(LandingContext.RoadLanding, result.Context);
        }

        [Test]
        public void SteepRidgeImpactCrashes()
        {
            LandingResult result = LandingClassifier.Classify(new LandingSample(
                SurfaceKind.Ridge,
                verticalImpactSpeed: 8.5f,
                forwardSpeed: 18f,
                pitchDegrees: 6f,
                rollDegrees: 8f,
                groundSlopeDegrees: 42f,
                aircraftState: AircraftState.Flying));

            Assert.AreEqual(LandingOutcome.Crashed, result.Outcome);
        }

        [Test]
        public void WaterContactSubmerges()
        {
            LandingResult result = LandingClassifier.Classify(new LandingSample(
                SurfaceKind.Water,
                verticalImpactSpeed: 1f,
                forwardSpeed: 12f,
                pitchDegrees: 0f,
                rollDegrees: 0f,
                groundSlopeDegrees: 0f,
                aircraftState: AircraftState.Flying));

            Assert.AreEqual(LandingOutcome.Submerged, result.Outcome);
            Assert.AreEqual(LandingContext.Submerged, result.Context);
        }

        [Test]
        public void DamagedAircraftOnSurvivableFieldBecomesEmergencyLanding()
        {
            LandingResult result = LandingClassifier.Classify(new LandingSample(
                SurfaceKind.Field,
                verticalImpactSpeed: 3f,
                forwardSpeed: 19f,
                pitchDegrees: 5f,
                rollDegrees: 5f,
                groundSlopeDegrees: 4f,
                aircraftState: AircraftState.Damaged));

            Assert.AreEqual(LandingOutcome.Damaged, result.Outcome);
            Assert.AreEqual(LandingContext.EmergencyLanding, result.Context);
        }
    }
}

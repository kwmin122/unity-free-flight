using MINgo.Flight;
using NUnit.Framework;

namespace MINgo.Tests
{
    public sealed class AircraftControllerTests
    {
        [Test]
        public void ResolveMotionState_PreservesDamagedStateWhileAirborne()
        {
            AircraftState state = ArcadeAircraftController.ResolveMotionState(
                AircraftState.Damaged,
                hasGroundContact: false,
                speedMetersPerSecond: 40f,
                takeoffSpeed: 22f);

            Assert.AreEqual(AircraftState.Damaged, state);
        }

        [Test]
        public void ResolveMotionState_MarksHealthyAirborneAircraftAsFlying()
        {
            AircraftState state = ArcadeAircraftController.ResolveMotionState(
                AircraftState.Grounded,
                hasGroundContact: false,
                speedMetersPerSecond: 40f,
                takeoffSpeed: 22f);

            Assert.AreEqual(AircraftState.Flying, state);
        }
    }
}

using MINgo.Hazards;
using NUnit.Framework;

namespace MINgo.Tests
{
    public sealed class RestrictedAirspaceStateTests
    {
        [Test]
        public void EnteringOuterZoneReturnsWarning()
        {
            var state = new RestrictedAirspaceState();

            Assert.AreEqual(RestrictedAirspacePhase.Warning, state.Tick(isInsideOuterZone: true, isInsideDeepZone: false, 0f));
        }

        [Test]
        public void StayingInDeepZoneForThreeSecondsReturnsLocking()
        {
            var state = new RestrictedAirspaceState();

            state.Tick(isInsideOuterZone: true, isInsideDeepZone: true, 1.5f);
            RestrictedAirspacePhase phase = state.Tick(isInsideOuterZone: true, isInsideDeepZone: true, 1.5f);

            Assert.AreEqual(RestrictedAirspacePhase.Locking, phase);
        }

        [Test]
        public void StayingInDeepZoneForSixSecondsLaunchesMissile()
        {
            var state = new RestrictedAirspaceState();

            state.Tick(isInsideOuterZone: true, isInsideDeepZone: true, 3f);
            RestrictedAirspacePhase phase = state.Tick(isInsideOuterZone: true, isInsideDeepZone: true, 3f);

            Assert.AreEqual(RestrictedAirspacePhase.MissileLaunched, phase);
            Assert.IsTrue(state.HasActiveMissile);
        }

        [Test]
        public void LeavingBeforeLaunchReturnsEscapedAfterDelay()
        {
            var state = new RestrictedAirspaceState();

            state.Tick(isInsideOuterZone: true, isInsideDeepZone: true, 3f);
            state.Tick(isInsideOuterZone: false, isInsideDeepZone: false, 1.49f);

            Assert.AreEqual(RestrictedAirspacePhase.Escaped, state.Tick(isInsideOuterZone: false, isInsideDeepZone: false, 0.01f));
            Assert.IsFalse(state.HasActiveMissile);
        }

        [Test]
        public void LeavingAfterLaunchDoesNotCancelActiveMissile()
        {
            var state = new RestrictedAirspaceState();

            state.Tick(isInsideOuterZone: true, isInsideDeepZone: true, 6f);
            RestrictedAirspacePhase phase = state.Tick(isInsideOuterZone: false, isInsideDeepZone: false, 2f);

            Assert.AreEqual(RestrictedAirspacePhase.MissileLaunched, phase);
            Assert.IsTrue(state.HasActiveMissile);
        }
    }
}

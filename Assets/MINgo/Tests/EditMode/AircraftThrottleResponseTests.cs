using MINgo.Flight;
using NUnit.Framework;

namespace MINgo.Tests
{
    public sealed class AircraftThrottleResponseTests
    {
        [Test]
        public void UpdateThrottleForGtaHold_RampsQuicklyTowardFullPower()
        {
            float throttle = ArcadeAircraftController.UpdateThrottleForGtaHold(
                currentThrottle: 0f,
                throttleInput: 1f,
                responseRate: 3.2f,
                deltaTime: 0.25f);

            Assert.That(throttle, Is.GreaterThan(0.75f));
        }

        [Test]
        public void UpdateThrottleForGtaHold_RampsQuicklyTowardIdleWhenSIsHeld()
        {
            float throttle = ArcadeAircraftController.UpdateThrottleForGtaHold(
                currentThrottle: 1f,
                throttleInput: -1f,
                responseRate: 3.2f,
                deltaTime: 0.35f);

            Assert.That(throttle, Is.EqualTo(0f).Within(0.001f));
        }

        [Test]
        public void UpdateThrottleForGtaHold_KeepsThrottleWhenInputIsNeutral()
        {
            float throttle = ArcadeAircraftController.UpdateThrottleForGtaHold(
                currentThrottle: 0.42f,
                throttleInput: 0f,
                responseRate: 3.2f,
                deltaTime: 0.5f);

            Assert.That(throttle, Is.EqualTo(0.42f).Within(0.001f));
        }
    }
}

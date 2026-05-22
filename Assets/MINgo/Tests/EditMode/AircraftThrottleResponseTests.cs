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
                neutralReleaseRate: 1.4f,
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
                neutralReleaseRate: 1.4f,
                deltaTime: 0.35f);

            Assert.That(throttle, Is.EqualTo(0f).Within(0.001f));
        }

        [Test]
        public void UpdateThrottleForGtaHold_DecaysTowardIdleWhenInputIsNeutral()
        {
            float throttle = ArcadeAircraftController.UpdateThrottleForGtaHold(
                currentThrottle: 0.42f,
                throttleInput: 0f,
                responseRate: 3.2f,
                neutralReleaseRate: 1.4f,
                deltaTime: 0.2f);

            Assert.That(throttle, Is.LessThan(0.42f));
            Assert.That(throttle, Is.GreaterThan(0f));
        }

        [Test]
        public void CalculateIdleCoastDrag_ReturnsZeroWhilePowerIsHeld()
        {
            UnityEngine.Vector3 drag = ArcadeAircraftController.CalculateIdleCoastDrag(
                UnityEngine.Vector3.forward * 20f,
                throttleInput: 1f,
                isGrounded: false,
                dragFactor: 0.006f);

            Assert.That(drag, Is.EqualTo(UnityEngine.Vector3.zero));
        }

        [Test]
        public void CalculateIdleCoastDrag_OpposesVelocityWhenInputIsNeutralInAir()
        {
            UnityEngine.Vector3 drag = ArcadeAircraftController.CalculateIdleCoastDrag(
                UnityEngine.Vector3.forward * 20f,
                throttleInput: 0f,
                isGrounded: false,
                dragFactor: 0.006f);

            Assert.That(drag.z, Is.LessThan(0f));
            Assert.That(drag.x, Is.EqualTo(0f).Within(0.001f));
        }
    }
}

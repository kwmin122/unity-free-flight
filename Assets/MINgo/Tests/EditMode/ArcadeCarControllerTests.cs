using MINgo.Vehicles;
using NUnit.Framework;

namespace MINgo.Tests
{
    public sealed class ArcadeCarControllerTests
    {
        [Test]
        public void ResolveDriveMode_BrakesBeforeReverseWhenMovingForwardAndSHeld()
        {
            DriveMode mode = ArcadeCarController.ResolveDriveMode(
                throttleInput: -1f,
                forwardSpeed: 8f,
                reverseThreshold: 1.5f);

            Assert.That(mode, Is.EqualTo(DriveMode.Braking));
        }

        [Test]
        public void ResolveDriveMode_ReversesWhenNearlyStoppedAndSHeld()
        {
            DriveMode mode = ArcadeCarController.ResolveDriveMode(
                throttleInput: -1f,
                forwardSpeed: 0.4f,
                reverseThreshold: 1.5f);

            Assert.That(mode, Is.EqualTo(DriveMode.Reverse));
        }

        [Test]
        public void CalculateSteeringDegrees_ReducesSteeringAtSpeed()
        {
            float slow = ArcadeCarController.CalculateSteeringDegrees(
                steerInput: 1f,
                speedMetersPerSecond: 2f,
                maxSteerDegrees: 32f,
                fullSteerSpeed: 6f,
                reducedSteerSpeed: 28f);

            float fast = ArcadeCarController.CalculateSteeringDegrees(
                steerInput: 1f,
                speedMetersPerSecond: 24f,
                maxSteerDegrees: 32f,
                fullSteerSpeed: 6f,
                reducedSteerSpeed: 28f);

            Assert.That(slow, Is.GreaterThan(fast));
            Assert.That(fast, Is.GreaterThan(0f));
        }
    }
}

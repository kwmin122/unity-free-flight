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

        [Test]
        public void CalculateMotorTorque_CutsForwardTorqueAtMaxSpeed()
        {
            float torque = ArcadeCarController.CalculateMotorTorque(
                DriveMode.Forward,
                forwardSpeed: 40f,
                maxForwardSpeed: 34f,
                maxReverseSpeed: 9f,
                motorTorque: 950f,
                reverseTorque: 420f);

            Assert.That(torque, Is.EqualTo(0f).Within(0.001f));
        }

        [Test]
        public void CalculateBrakeTorque_UsesHandbrakeWhenHeld()
        {
            float torque = ArcadeCarController.CalculateBrakeTorque(
                DriveMode.Forward,
                handbrake: true,
                brakeTorque: 2600f,
                coastBrakeTorque: 260f,
                handbrakeTorque: 4200f);

            Assert.That(torque, Is.EqualTo(4200f).Within(0.001f));
        }

        [Test]
        public void CalculateBrakeTorque_UsesEngineBrakingWhenCoasting()
        {
            float torque = ArcadeCarController.CalculateBrakeTorque(
                DriveMode.Coasting,
                handbrake: false,
                brakeTorque: 2600f,
                coastBrakeTorque: 260f,
                handbrakeTorque: 4200f);

            Assert.That(torque, Is.EqualTo(260f).Within(0.001f));
        }
    }
}

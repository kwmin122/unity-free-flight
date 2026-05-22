using MINgo.Vehicles;
using NUnit.Framework;

namespace MINgo.Tests
{
    public sealed class VehicleInputReaderTests
    {
        [Test]
        public void CreateKeyboardSnapshot_MapsGtaStyleDrivingKeys()
        {
            VehicleInputSnapshot input = VehicleInputReader.CreateKeyboardSnapshot(
                accelerate: true,
                brakeOrReverse: true,
                steerLeft: true,
                steerRight: false,
                handbrake: true,
                switchVehicle: true);

            Assert.That(input.Throttle, Is.EqualTo(0f).Within(0.001f));
            Assert.That(input.Steer, Is.LessThan(0f));
            Assert.That(input.Handbrake, Is.True);
            Assert.That(input.SwitchVehicle, Is.True);
        }
    }
}

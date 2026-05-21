using MINgo.Flight;
using NUnit.Framework;

namespace MINgo.Tests
{
    public sealed class FlightInputReaderTests
    {
        [Test]
        public void CreateKeyboardSnapshot_TreatsSAsThrottleDownNotHardBrake()
        {
            FlightInputSnapshot input = FlightInputReader.CreateKeyboardSnapshot(
                pitchUp: false,
                pitchDown: false,
                rollLeft: false,
                rollRight: false,
                turnLeft: false,
                turnRight: false,
                throttleUp: false,
                throttleDown: true,
                brake: false);

            Assert.That(input.ThrottleDelta, Is.LessThan(0f));
            Assert.That(input.Brake, Is.False);
        }

        [Test]
        public void CreateKeyboardSnapshot_TreatsSpaceAsHardBrake()
        {
            FlightInputSnapshot input = FlightInputReader.CreateKeyboardSnapshot(
                pitchUp: false,
                pitchDown: false,
                rollLeft: false,
                rollRight: false,
                turnLeft: false,
                turnRight: false,
                throttleUp: false,
                throttleDown: false,
                brake: true);

            Assert.That(input.Brake, Is.True);
        }
    }
}

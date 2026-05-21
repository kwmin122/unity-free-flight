using MINgo.Flight;
using NUnit.Framework;
using UnityEngine;

namespace MINgo.Tests
{
    public sealed class FlightControlAssistTests
    {
        [Test]
        public void CalculateRollDegrees_ReportsSignedAircraftBank()
        {
            Quaternion roll = Quaternion.AngleAxis(25f, Vector3.forward);

            float rollDegrees = FlightControlAssist.CalculateRollDegrees(
                roll * Vector3.up,
                Vector3.forward);

            Assert.That(rollDegrees, Is.EqualTo(25f).Within(0.001f));
        }

        [Test]
        public void CalculateAssistedControls_TurnRequestHoldsTargetBank()
        {
            FlightInputSnapshot input = new FlightInputSnapshot(
                pitch: 0f,
                roll: 0f,
                yaw: 0f,
                turn: 1f,
                throttleDelta: 0f,
                brake: false);

            FlightControlOutput wingsLevel = FlightControlAssist.CalculateAssistedControls(
                input,
                currentRollDegrees: 0f,
                forwardSpeed: 35f,
                takeoffSpeed: 22f,
                throttle01: 0.8f,
                hasGroundContact: false);
            FlightControlOutput alreadyBanked = FlightControlAssist.CalculateAssistedControls(
                input,
                currentRollDegrees: -35f,
                forwardSpeed: 35f,
                takeoffSpeed: 22f,
                throttle01: 0.8f,
                hasGroundContact: false);

            Assert.That(wingsLevel.Roll, Is.GreaterThan(0.5f));
            Assert.That(wingsLevel.Yaw, Is.GreaterThan(0f));
            Assert.That(Mathf.Abs(alreadyBanked.Roll), Is.LessThan(Mathf.Abs(wingsLevel.Roll)));
        }

        [Test]
        public void CalculateAssistedControls_LevelsWingsWhenNoTurnInput()
        {
            FlightInputSnapshot input = new FlightInputSnapshot(
                pitch: 0f,
                roll: 0f,
                yaw: 0f,
                turn: 0f,
                throttleDelta: 0f,
                brake: false);

            FlightControlOutput output = FlightControlAssist.CalculateAssistedControls(
                input,
                currentRollDegrees: 24f,
                forwardSpeed: 30f,
                takeoffSpeed: 22f,
                throttle01: 0.6f,
                hasGroundContact: false);

            Assert.That(output.Roll, Is.GreaterThan(0f));
            Assert.That(output.Yaw, Is.EqualTo(0f).Within(0.001f));
        }

        [Test]
        public void CalculateAssistedControls_AddsTakeoffPitchWhenFastOnGround()
        {
            FlightInputSnapshot input = new FlightInputSnapshot(
                pitch: 0f,
                roll: 0f,
                yaw: 0f,
                turn: 0f,
                throttleDelta: 0f,
                brake: false);

            FlightControlOutput output = FlightControlAssist.CalculateAssistedControls(
                input,
                currentRollDegrees: 0f,
                forwardSpeed: 21f,
                takeoffSpeed: 22f,
                throttle01: 1f,
                hasGroundContact: true);

            Assert.That(output.Pitch, Is.GreaterThan(0f));
        }
    }
}

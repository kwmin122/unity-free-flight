using System.Collections;
using MINgo.Flight;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace MINgo.Tests
{
    public sealed class FlightPlayModeSmokeTests
    {
        [TearDown]
        public void TearDown()
        {
            FlightInputReader.ClearInputOverrideForTests();
        }

        [UnityTest]
        public IEnumerator HoldingThrottleAcceleratesTheSceneAircraft()
        {
            yield return LoadFreeFlightScene();
            ArcadeAircraftController aircraft = FindAircraft();
            Assert.That(aircraft, Is.Not.Null);

            FlightInputReader.SetInputOverrideForTests(new FlightInputSnapshot(
                pitch: 0f,
                roll: 0f,
                yaw: 0f,
                turn: 0f,
                throttleDelta: 1f,
                brake: false));

            for (int i = 0; i < 240; i++)
            {
                yield return new WaitForFixedUpdate();
            }

            Assert.That(aircraft.Throttle01, Is.GreaterThan(0.95f));
            Assert.That(aircraft.SpeedMetersPerSecond, Is.GreaterThan(12f));
            Assert.That(aircraft.CurrentState, Is.Not.EqualTo(AircraftState.Crashed));
            Assert.That(aircraft.CurrentState, Is.Not.EqualTo(AircraftState.Submerged));
        }

        [UnityTest]
        public IEnumerator TurnInputBanksThenReleaseRecoversTowardLevel()
        {
            yield return LoadFreeFlightScene();
            ArcadeAircraftController aircraft = FindAircraft();
            Assert.That(aircraft, Is.Not.Null);

            FlightInputReader.SetInputOverrideForTests(new FlightInputSnapshot(
                pitch: 0f,
                roll: 0f,
                yaw: 0f,
                turn: 0f,
                throttleDelta: 1f,
                brake: false));

            for (int i = 0; i < 220; i++)
            {
                yield return new WaitForFixedUpdate();
            }

            float initialHeading = aircraft.transform.eulerAngles.y;

            FlightInputReader.SetInputOverrideForTests(new FlightInputSnapshot(
                pitch: 0f,
                roll: 0f,
                yaw: 0f,
                turn: 1f,
                throttleDelta: 0f,
                brake: false));

            for (int i = 0; i < 120; i++)
            {
                yield return new WaitForFixedUpdate();
            }

            float bankAfterTurn = Mathf.Abs(aircraft.RollDegrees);
            float headingAfterTurn = aircraft.transform.eulerAngles.y;

            FlightInputReader.SetInputOverrideForTests(new FlightInputSnapshot(
                pitch: 0f,
                roll: 0f,
                yaw: 0f,
                turn: 0f,
                throttleDelta: 0f,
                brake: false));

            for (int i = 0; i < 160; i++)
            {
                yield return new WaitForFixedUpdate();
            }

            float bankAfterRelease = Mathf.Abs(aircraft.RollDegrees);

            Assert.That(bankAfterTurn, Is.GreaterThan(8f));
            Assert.That(Mathf.Abs(Mathf.DeltaAngle(initialHeading, headingAfterTurn)), Is.GreaterThan(5f));
            Assert.That(bankAfterRelease, Is.LessThan(bankAfterTurn));
            Assert.That(bankAfterRelease, Is.LessThan(8f));
            Assert.That(aircraft.CurrentState, Is.Not.EqualTo(AircraftState.Crashed));
            Assert.That(aircraft.CurrentState, Is.Not.EqualTo(AircraftState.Submerged));
        }

        private static IEnumerator LoadFreeFlightScene()
        {
            SceneManager.LoadScene("FreeFlightSandbox");
            yield return null;
            yield return new WaitForFixedUpdate();
        }

        private static ArcadeAircraftController FindAircraft()
        {
            return Object.FindFirstObjectByType<ArcadeAircraftController>();
        }
    }
}

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
            SceneManager.LoadScene("FreeFlightSandbox");
            yield return null;
            yield return new WaitForFixedUpdate();

            ArcadeAircraftController aircraft = Object.FindFirstObjectByType<ArcadeAircraftController>();
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
    }
}

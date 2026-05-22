using System.Collections;
using MINgo.Flight;
using MINgo.Vehicles;
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
            VehicleInputReader.ClearInputOverrideForTests();
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
        public IEnumerator CarAccelerationStaysGrounded()
        {
            yield return LoadFreeFlightScene();
            ArcadeCarController car = ActivateCarForTest();
            Assert.That(car, Is.Not.Null);

            VehicleInputReader.SetInputOverrideForTests(new VehicleInputSnapshot(
                throttle: 1f,
                steer: 0f,
                handbrake: false,
                switchVehicle: false));

            for (int i = 0; i < 220; i++)
            {
                yield return new WaitForFixedUpdate();
            }

            Assert.That(car.SpeedMetersPerSecond, Is.GreaterThan(4f));
            Assert.That(car.GroundedWheelCount, Is.GreaterThanOrEqualTo(3));
            Assert.That(car.transform.position.y, Is.LessThan(2.5f));
            Assert.That(Mathf.Abs(car.RollDegrees), Is.LessThan(18f));
        }

        [UnityTest]
        public IEnumerator CarForwardRightInputMovesAndTurns()
        {
            yield return LoadFreeFlightScene();
            ArcadeCarController car = ActivateCarForTest();
            Assert.That(car, Is.Not.Null);

            Vector3 startPosition = car.transform.position;
            float startYaw = car.transform.eulerAngles.y;

            VehicleInputReader.SetInputOverrideForTests(new VehicleInputSnapshot(
                throttle: 1f,
                steer: 1f,
                handbrake: false,
                switchVehicle: false));

            for (int i = 0; i < 180; i++)
            {
                yield return new WaitForFixedUpdate();
            }

            Assert.That(car.SpeedMetersPerSecond, Is.GreaterThan(6f));
            Assert.That(Vector3.Distance(startPosition, car.transform.position), Is.GreaterThan(10f));
            Assert.That(Mathf.Abs(Mathf.DeltaAngle(startYaw, car.transform.eulerAngles.y)), Is.GreaterThan(5f));
            Assert.That(car.GroundedWheelCount, Is.GreaterThanOrEqualTo(3));
        }

        [UnityTest]
        public IEnumerator CarReverseRightInputBacksUpAndTurns()
        {
            yield return LoadFreeFlightScene();
            ArcadeCarController car = ActivateCarForTest();
            Assert.That(car, Is.Not.Null);

            Vector3 startPosition = car.transform.position;
            Vector3 startForward = car.transform.forward;
            float startYaw = car.transform.eulerAngles.y;

            VehicleInputReader.SetInputOverrideForTests(new VehicleInputSnapshot(
                throttle: -1f,
                steer: 1f,
                handbrake: false,
                switchVehicle: false));

            for (int i = 0; i < 220; i++)
            {
                yield return new WaitForFixedUpdate();
            }

            float backwardTravel = Vector3.Dot(car.transform.position - startPosition, startForward);
            Assert.That(backwardTravel, Is.LessThan(-3f));
            Assert.That(car.SpeedMetersPerSecond, Is.GreaterThan(2f));
            Assert.That(Mathf.Abs(Mathf.DeltaAngle(startYaw, car.transform.eulerAngles.y)), Is.GreaterThan(4f));
            Assert.That(car.GroundedWheelCount, Is.GreaterThanOrEqualTo(3));
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

            float rollAfterTurn = aircraft.RollDegrees;
            float bankAfterTurn = Mathf.Abs(rollAfterTurn);
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

            float rollAfterRelease = aircraft.RollDegrees;
            float bankAfterRelease = Mathf.Abs(rollAfterRelease);

            Assert.That(bankAfterTurn, Is.GreaterThan(8f));
            Assert.That(Mathf.Abs(Mathf.DeltaAngle(initialHeading, headingAfterTurn)), Is.GreaterThan(5f));
            Assert.That(
                bankAfterRelease,
                Is.LessThan(bankAfterTurn),
                $"Expected release to reduce bank. rollAfterTurn={rollAfterTurn:0.00}, rollAfterRelease={rollAfterRelease:0.00}");
            Assert.That(
                bankAfterRelease,
                Is.LessThan(25f),
                $"Expected released controls to recover near level. rollAfterTurn={rollAfterTurn:0.00}, rollAfterRelease={rollAfterRelease:0.00}");
            Assert.That(aircraft.CurrentState, Is.Not.EqualTo(AircraftState.Crashed));
            Assert.That(aircraft.CurrentState, Is.Not.EqualTo(AircraftState.Submerged));
        }

        [UnityTest]
        public IEnumerator HoldingSlowdownInputAddsAirbrakeDragAfterThrottleCut()
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

            FlightInputReader.SetInputOverrideForTests(new FlightInputSnapshot(
                pitch: 0f,
                roll: 0f,
                yaw: 0f,
                turn: 0f,
                throttleDelta: -1f,
                brake: false));

            for (int i = 0; i < 120; i++)
            {
                yield return new WaitForFixedUpdate();
            }

            float idleStartSpeed = aircraft.SpeedMetersPerSecond;
            Assert.That(aircraft.Throttle01, Is.LessThan(0.05f));

            FlightInputReader.SetInputOverrideForTests(new FlightInputSnapshot(
                pitch: 0f,
                roll: 0f,
                yaw: 0f,
                turn: 0f,
                throttleDelta: 0f,
                brake: false));

            for (int i = 0; i < 120; i++)
            {
                yield return new WaitForFixedUpdate();
            }

            float idleFinalSpeed = aircraft.SpeedMetersPerSecond;

            yield return LoadFreeFlightScene();
            aircraft = FindAircraft();
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

            FlightInputReader.SetInputOverrideForTests(new FlightInputSnapshot(
                pitch: 0f,
                roll: 0f,
                yaw: 0f,
                turn: 0f,
                throttleDelta: -1f,
                brake: false));

            for (int i = 0; i < 120; i++)
            {
                yield return new WaitForFixedUpdate();
            }

            float airbrakeStartSpeed = aircraft.SpeedMetersPerSecond;

            for (int i = 0; i < 120; i++)
            {
                yield return new WaitForFixedUpdate();
            }

            float airbrakeFinalSpeed = aircraft.SpeedMetersPerSecond;

            Assert.That(idleStartSpeed, Is.GreaterThan(12f));
            Assert.That(airbrakeStartSpeed, Is.InRange(idleStartSpeed * 0.9f, idleStartSpeed * 1.1f));
            Assert.That(aircraft.Throttle01, Is.LessThan(0.05f));
            Assert.That(
                airbrakeFinalSpeed,
                Is.LessThan(idleFinalSpeed * 0.9f),
                $"Expected held slowdown to add airbrake drag. idle={idleFinalSpeed:0.00}, airbrake={airbrakeFinalSpeed:0.00}");
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

        private static ArcadeCarController ActivateCarForTest()
        {
            PlayerVehicleSwitcher switcher = Object.FindFirstObjectByType<PlayerVehicleSwitcher>();
            Assert.That(switcher, Is.Not.Null);
            switcher.startInAircraft = false;
            switcher.SetActiveVehicle(useAircraft: false);
            if (switcher.aircraft != null)
            {
                switcher.aircraft.acceptsInput = false;
            }

            if (switcher.car != null)
            {
                switcher.car.acceptsInput = true;
            }

            return switcher.car;
        }
    }
}

using System;
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

            yield return SimulateFixedFrames(100);

            float yawDelta = Mathf.Abs(Mathf.DeltaAngle(startYaw, car.transform.eulerAngles.y));
            Assert.That(car.SpeedMetersPerSecond, Is.GreaterThanOrEqualTo(7.5f));
            Assert.That(HorizontalDistance(startPosition, car.transform.position), Is.GreaterThan(8f));
            Assert.That(yawDelta, Is.GreaterThanOrEqualTo(20f));
            Assert.That(car.GroundedWheelCount, Is.GreaterThanOrEqualTo(3));
            Assert.That(Mathf.Abs(car.RollDegrees), Is.LessThan(12f));
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

            yield return SimulateFixedFrames(100);

            float backwardTravel = SignedForwardTravel(startPosition, car.transform.position, startForward);
            float yawDelta = Mathf.Abs(Mathf.DeltaAngle(startYaw, car.transform.eulerAngles.y));
            Assert.That(backwardTravel, Is.LessThanOrEqualTo(-4f));
            Assert.That(car.SpeedMetersPerSecond, Is.GreaterThan(2f));
            Assert.That(yawDelta, Is.GreaterThanOrEqualTo(10f));
            Assert.That(car.GroundedWheelCount, Is.GreaterThanOrEqualTo(3));
            Assert.That(Mathf.Abs(car.RollDegrees), Is.LessThan(12f));
        }

        [UnityTest]
        public IEnumerator CarForwardLaunchAndNeutralCoastMeetArcadeThresholds()
        {
            yield return LoadFreeFlightScene();
            ArcadeCarController car = ActivateCarForTest();
            Assert.That(car, Is.Not.Null);

            Vector3 startPosition = car.transform.position;
            int groundedSamples = 0;
            int totalSamples = 0;

            VehicleInputReader.SetInputOverrideForTests(new VehicleInputSnapshot(
                throttle: 1f,
                steer: 0f,
                handbrake: false,
                switchVehicle: false));

            yield return SimulateFixedFrames(100, () =>
            {
                groundedSamples += car.GroundedWheelCount >= 3 ? 1 : 0;
                totalSamples++;
            });
            float speedAfterTwoSeconds = car.SpeedMetersPerSecond;

            yield return SimulateFixedFrames(100, () =>
            {
                groundedSamples += car.GroundedWheelCount >= 3 ? 1 : 0;
                totalSamples++;
            });
            float speedAtRelease = car.SpeedMetersPerSecond;
            float forwardDistance = HorizontalDistance(startPosition, car.transform.position);

            VehicleInputReader.SetInputOverrideForTests(new VehicleInputSnapshot(
                throttle: 0f,
                steer: 0f,
                handbrake: false,
                switchVehicle: false));

            yield return SimulateFixedFrames(150, () =>
            {
                groundedSamples += car.GroundedWheelCount >= 3 ? 1 : 0;
                totalSamples++;
            });
            float speedAfterCoast = car.SpeedMetersPerSecond;
            float coastRatio = speedAfterCoast / Mathf.Max(speedAtRelease, 0.01f);

            Assert.That(speedAfterTwoSeconds, Is.GreaterThanOrEqualTo(7.5f));
            Assert.That(forwardDistance >= 24f || speedAtRelease >= 12f, Is.True,
                $"Forward launch was too weak. distance={forwardDistance:F2}, speed={speedAtRelease:F2}");
            Assert.That(coastRatio, Is.InRange(0.55f, 0.75f));
            Assert.That(GroundedRatio(groundedSamples, totalSamples), Is.GreaterThanOrEqualTo(0.95f));
            Assert.That(car.transform.position.y, Is.LessThan(2.5f));
            Assert.That(Mathf.Abs(car.RollDegrees), Is.LessThan(12f));
        }

        [UnityTest]
        public IEnumerator CarBrakesFromSpeedThenReversesFromLowSpeed()
        {
            yield return LoadFreeFlightScene();
            ArcadeCarController car = ActivateCarForTest();
            Assert.That(car, Is.Not.Null);

            VehicleInputReader.SetInputOverrideForTests(new VehicleInputSnapshot(
                throttle: 1f,
                steer: 0f,
                handbrake: false,
                switchVehicle: false));
            yield return SimulateFixedFrames(150);
            float brakeStartSpeed = car.SpeedMetersPerSecond;
            Assert.That(brakeStartSpeed, Is.GreaterThanOrEqualTo(8f));

            VehicleInputReader.SetInputOverrideForTests(new VehicleInputSnapshot(
                throttle: -1f,
                steer: 0f,
                handbrake: false,
                switchVehicle: false));
            yield return SimulateFixedFrames(100);
            Assert.That(car.SpeedMetersPerSecond, Is.LessThanOrEqualTo(brakeStartSpeed * 0.4f));

            yield return LoadFreeFlightScene();
            car = ActivateCarForTest();
            Assert.That(car.SpeedMetersPerSecond, Is.LessThanOrEqualTo(1.5f));

            Vector3 startPosition = car.transform.position;
            Vector3 startForward = car.transform.forward;
            VehicleInputReader.SetInputOverrideForTests(new VehicleInputSnapshot(
                throttle: -1f,
                steer: 0f,
                handbrake: false,
                switchVehicle: false));
            yield return SimulateFixedFrames(125);

            Assert.That(SignedForwardTravel(startPosition, car.transform.position, startForward), Is.LessThanOrEqualTo(-4f));
            Assert.That(car.GroundedWheelCount, Is.GreaterThanOrEqualTo(3));
            Assert.That(Mathf.Abs(car.RollDegrees), Is.LessThan(12f));
        }

        [UnityTest]
        public IEnumerator CarHandbrakeTurnRotatesWithoutFlipping()
        {
            yield return LoadFreeFlightScene();
            ArcadeCarController car = ActivateCarForTest();
            Assert.That(car, Is.Not.Null);

            VehicleInputReader.SetInputOverrideForTests(new VehicleInputSnapshot(
                throttle: 1f,
                steer: 0f,
                handbrake: false,
                switchVehicle: false));

            for (int i = 0; i < 180; i++)
            {
                yield return new WaitForFixedUpdate();
                if (car.SpeedMetersPerSecond >= 8f && car.SpeedMetersPerSecond <= 12f)
                {
                    break;
                }
            }

            float speedBeforeHandbrake = car.SpeedMetersPerSecond;
            Assert.That(speedBeforeHandbrake, Is.InRange(8f, 12f));
            float yawBeforeHandbrake = car.transform.eulerAngles.y;
            int groundedSamples = 0;
            int totalSamples = 0;

            VehicleInputReader.SetInputOverrideForTests(new VehicleInputSnapshot(
                throttle: 0f,
                steer: 1f,
                handbrake: true,
                switchVehicle: false));

            yield return SimulateFixedFrames(100, () =>
            {
                groundedSamples += car.GroundedWheelCount >= 2 ? 1 : 0;
                totalSamples++;
            });
            float yawDelta = Mathf.Abs(Mathf.DeltaAngle(yawBeforeHandbrake, car.transform.eulerAngles.y));

            Assert.That(yawDelta, Is.InRange(35f, 75f));
            Assert.That(GroundedRatio(groundedSamples, totalSamples), Is.GreaterThanOrEqualTo(0.95f));
            Assert.That(car.transform.position.y, Is.LessThan(2.5f));
            Assert.That(Mathf.Abs(car.RollDegrees), Is.LessThan(25f));
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

        private static IEnumerator SimulateFixedFrames(int frameCount)
        {
            for (int i = 0; i < frameCount; i++)
            {
                yield return new WaitForFixedUpdate();
            }
        }

        private static IEnumerator SimulateFixedFrames(int frameCount, Action onFrame)
        {
            for (int i = 0; i < frameCount; i++)
            {
                onFrame();
                yield return new WaitForFixedUpdate();
            }
        }

        private static float HorizontalDistance(Vector3 a, Vector3 b)
        {
            Vector3 delta = b - a;
            delta.y = 0f;
            return delta.magnitude;
        }

        private static float SignedForwardTravel(Vector3 startPosition, Vector3 endPosition, Vector3 startForward)
        {
            Vector3 delta = endPosition - startPosition;
            delta.y = 0f;
            Vector3 forward = startForward;
            forward.y = 0f;
            return Vector3.Dot(delta, forward.normalized);
        }

        private static float GroundedRatio(int groundedSamples, int totalSamples)
        {
            return totalSamples == 0 ? 0f : groundedSamples / (float)totalSamples;
        }

        private static ArcadeAircraftController FindAircraft()
        {
            return UnityEngine.Object.FindFirstObjectByType<ArcadeAircraftController>();
        }

        private static ArcadeCarController ActivateCarForTest()
        {
            PlayerVehicleSwitcher switcher = UnityEngine.Object.FindFirstObjectByType<PlayerVehicleSwitcher>();
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

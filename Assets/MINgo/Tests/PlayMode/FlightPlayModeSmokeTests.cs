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

            yield return WaitForAircraftAltitude(aircraft, 8f, 400);

            Assert.That(aircraft.Throttle01, Is.GreaterThan(0.95f));
            Assert.That(aircraft.ForwardSpeedMetersPerSecond, Is.GreaterThan(12f));
            Assert.That(aircraft.CurrentState, Is.Not.EqualTo(AircraftState.Crashed));
            Assert.That(aircraft.CurrentState, Is.Not.EqualTo(AircraftState.Submerged));
        }

        [UnityTest]
        public IEnumerator AircraftTakeoffMeetsArcadeThresholds()
        {
            yield return LoadFreeFlightScene();
            ArcadeAircraftController aircraft = FindAircraft();
            Assert.That(aircraft, Is.Not.Null);

            Vector3 startPosition = aircraft.transform.position;
            Vector3 startForward = aircraft.transform.forward;
            bool capturedTakeoff = false;
            float takeoffSpeed = 0f;
            float takeoffTravel = 0f;

            FlightInputReader.SetInputOverrideForTests(new FlightInputSnapshot(
                pitch: 0f,
                roll: 0f,
                yaw: 0f,
                turn: 0f,
                throttleDelta: 1f,
                brake: false));

            for (int i = 0; i < 400; i++)
            {
                yield return new WaitForFixedUpdate();
                if (!capturedTakeoff && aircraft.AltitudeMeters >= 8f)
                {
                    capturedTakeoff = true;
                    takeoffSpeed = aircraft.ForwardSpeedMetersPerSecond;
                    takeoffTravel = SignedForwardTravel(startPosition, aircraft.transform.position, startForward);
                }
            }

            Assert.That(capturedTakeoff, Is.True,
                $"Aircraft did not reach 8m in 8s. finalAltitude={aircraft.AltitudeMeters:F2}, finalSpeed={aircraft.SpeedMetersPerSecond:F2}");
            Assert.That(takeoffSpeed, Is.InRange(18f, 25f),
                $"Takeoff speed outside arcade band. speed={takeoffSpeed:F2}");
            Assert.That(takeoffTravel, Is.LessThanOrEqualTo(180f),
                $"Takeoff runway travel too long. travel={takeoffTravel:F2}");
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

            yield return SimulateFixedFrames(220);

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
            Assert.That(coastRatio, Is.InRange(0.55f, 0.75f),
                $"Coasting ratio outside arcade band. release={speedAtRelease:F2}, after={speedAfterCoast:F2}, ratio={coastRatio:F2}");
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

            Assert.That(yawDelta, Is.InRange(35f, 75f),
                $"Handbrake yaw outside arcade band. speed={speedBeforeHandbrake:F2}, yaw={yawDelta:F2}");
            Assert.That(GroundedRatio(groundedSamples, totalSamples), Is.GreaterThanOrEqualTo(0.95f));
            Assert.That(car.transform.position.y, Is.LessThan(2.5f));
            Assert.That(Mathf.Abs(car.RollDegrees), Is.LessThan(25f));
        }

        [UnityTest]
        public IEnumerator CarCameraTracksBehindActiveVehicle()
        {
            yield return LoadFreeFlightScene();
            ArcadeCarController car = ActivateCarForTest();
            ChaseCameraRig cameraRig = FindCameraRig();
            Camera camera = Camera.main;
            Assert.That(car, Is.Not.Null);
            Assert.That(cameraRig, Is.Not.Null);
            Assert.That(camera, Is.Not.Null);
            Assert.That(cameraRig.target, Is.EqualTo(car.transform));

            VehicleInputReader.SetInputOverrideForTests(new VehicleInputSnapshot(
                throttle: 1f,
                steer: 1f,
                handbrake: false,
                switchVehicle: false));

            float minDistance = float.PositiveInfinity;
            float maxDistance = 0f;
            float maxCenterOffset = 0f;
            bool cameraStayedBehind = true;

            yield return SimulateFixedAndRenderFrames(140, () =>
            {
                Vector3 toCamera = camera.transform.position - car.transform.position;
                float distance = toCamera.magnitude;
                minDistance = Mathf.Min(minDistance, distance);
                maxDistance = Mathf.Max(maxDistance, distance);

                Vector3 flatToCamera = Vector3.ProjectOnPlane(toCamera, Vector3.up).normalized;
                Vector3 flatForward = Vector3.ProjectOnPlane(car.transform.forward, Vector3.up).normalized;
                cameraStayedBehind &= Vector3.Dot(flatForward, flatToCamera) < -0.35f;

                Vector3 viewportPoint = camera.WorldToViewportPoint(car.transform.position);
                maxCenterOffset = Mathf.Max(
                    maxCenterOffset,
                    Mathf.Abs(viewportPoint.x - 0.5f) + Mathf.Abs(viewportPoint.y - 0.5f));
            });

            Assert.That(minDistance, Is.GreaterThanOrEqualTo(6f), $"Car camera entered target. minDistance={minDistance:F2}");
            Assert.That(maxDistance, Is.LessThanOrEqualTo(12f), $"Car camera drifted too far. maxDistance={maxDistance:F2}");
            Assert.That(cameraStayedBehind, Is.True, "Car camera did not remain behind the active car.");
            Assert.That(maxCenterOffset, Is.LessThanOrEqualTo(0.65f), $"Car left readable screen center. offset={maxCenterOffset:F2}");
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

            yield return WaitForAircraftAltitude(aircraft, 8f, 400);

            float initialHeading = aircraft.transform.eulerAngles.y;

            FlightInputReader.SetInputOverrideForTests(new FlightInputSnapshot(
                pitch: 0f,
                roll: 0f,
                yaw: 0f,
                turn: 1f,
                throttleDelta: 0f,
                brake: false));

            yield return SimulateFixedFrames(150);

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

            yield return SimulateFixedFrames(200);

            float rollAfterRelease = aircraft.RollDegrees;
            float bankAfterRelease = Mathf.Abs(rollAfterRelease);

            float headingDelta = Mathf.Abs(Mathf.DeltaAngle(initialHeading, headingAfterTurn));
            Assert.That(bankAfterTurn, Is.InRange(15f, 45f),
                $"Bank outside arcade turn band. rollAfterTurn={rollAfterTurn:F2}");
            Assert.That(headingDelta, Is.GreaterThanOrEqualTo(20f),
                $"Heading change too small for readable turn. headingDelta={headingDelta:F2}");
            Assert.That(bankAfterRelease, Is.LessThanOrEqualTo(10f),
                $"Expected released controls to recover near level. rollAfterTurn={rollAfterTurn:F2}, rollAfterRelease={rollAfterRelease:F2}");
            Assert.That(aircraft.CurrentState, Is.Not.EqualTo(AircraftState.Crashed));
            Assert.That(aircraft.CurrentState, Is.Not.EqualTo(AircraftState.Submerged));
        }

        [UnityTest]
        public IEnumerator AircraftSlowdownDropsSpeedByFifteenPercent()
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
            yield return SimulateFixedFrames(320);

            float speedBeforeSlowdown = aircraft.SpeedMetersPerSecond;
            FlightInputReader.SetInputOverrideForTests(new FlightInputSnapshot(
                pitch: 0f,
                roll: 0f,
                yaw: 0f,
                turn: 0f,
                throttleDelta: -1f,
                brake: false));
            yield return SimulateFixedFrames(150);

            Assert.That(aircraft.SpeedMetersPerSecond, Is.LessThanOrEqualTo(speedBeforeSlowdown * 0.85f),
                $"Slowdown too weak. before={speedBeforeSlowdown:F2}, after={aircraft.SpeedMetersPerSecond:F2}");
            Assert.That(aircraft.CurrentState, Is.Not.EqualTo(AircraftState.Crashed));
            Assert.That(aircraft.CurrentState, Is.Not.EqualTo(AircraftState.Submerged));
        }

        [UnityTest]
        public IEnumerator AircraftSlowdownCreatesMeasurablyMoreDescentThanIdle()
        {
            yield return LoadFreeFlightScene();
            ArcadeAircraftController idleAircraft = FindAircraft();
            Assert.That(idleAircraft, Is.Not.Null);

            FlightInputReader.SetInputOverrideForTests(new FlightInputSnapshot(
                pitch: 0f,
                roll: 0f,
                yaw: 0f,
                turn: 0f,
                throttleDelta: 1f,
                brake: false));
            yield return SimulateFixedFrames(320);
            FlightInputReader.SetInputOverrideForTests(new FlightInputSnapshot(
                pitch: 0f,
                roll: 0f,
                yaw: 0f,
                turn: 0f,
                throttleDelta: 0f,
                brake: false));
            yield return SimulateFixedFrames(60);
            float idleStartAltitude = idleAircraft.AltitudeMeters;
            yield return SimulateFixedFrames(150);
            float idleVerticalSpeed = (idleAircraft.AltitudeMeters - idleStartAltitude) / 3f;

            yield return LoadFreeFlightScene();
            ArcadeAircraftController brakingAircraft = FindAircraft();
            Assert.That(brakingAircraft, Is.Not.Null);

            FlightInputReader.SetInputOverrideForTests(new FlightInputSnapshot(
                pitch: 0f,
                roll: 0f,
                yaw: 0f,
                turn: 0f,
                throttleDelta: 1f,
                brake: false));
            yield return SimulateFixedFrames(320);
            FlightInputReader.SetInputOverrideForTests(new FlightInputSnapshot(
                pitch: 0f,
                roll: 0f,
                yaw: 0f,
                turn: 0f,
                throttleDelta: -1f,
                brake: false));
            yield return SimulateFixedFrames(60);
            float brakeStartAltitude = brakingAircraft.AltitudeMeters;
            yield return SimulateFixedFrames(150);
            float brakeVerticalSpeed = (brakingAircraft.AltitudeMeters - brakeStartAltitude) / 3f;

            Assert.That(brakeVerticalSpeed, Is.LessThanOrEqualTo(idleVerticalSpeed - 1.5f),
                $"Slowdown descent too weak. idleVertical={idleVerticalSpeed:F2}, brakeVertical={brakeVerticalSpeed:F2}");
            Assert.That(brakingAircraft.CurrentState, Is.Not.EqualTo(AircraftState.Crashed));
            Assert.That(brakingAircraft.CurrentState, Is.Not.EqualTo(AircraftState.Submerged));
        }

        [UnityTest]
        public IEnumerator AircraftRunwayApproachRemainsStableForEightSeconds()
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
            yield return SimulateFixedFrames(300);

            FlightInputReader.SetInputOverrideForTests(new FlightInputSnapshot(
                pitch: 0f,
                roll: 0f,
                yaw: 0f,
                turn: 0f,
                throttleDelta: -1f,
                brake: false));

            float maxRoll = 0f;
            float maxPitch = 0f;
            int rollSignFlips = 0;
            int lastRollSign = 0;
            float startAltitude = aircraft.AltitudeMeters;
            yield return SimulateFixedFrames(400, () =>
            {
                float roll = aircraft.RollDegrees;
                maxRoll = Mathf.Max(maxRoll, Mathf.Abs(roll));
                maxPitch = Mathf.Max(maxPitch, Mathf.Abs(SignedPitchDegrees(aircraft.transform)));

                int rollSign = Mathf.Abs(roll) < 3f ? 0 : Math.Sign(roll);
                if (rollSign != 0 && lastRollSign != 0 && rollSign != lastRollSign)
                {
                    rollSignFlips++;
                }

                if (rollSign != 0)
                {
                    lastRollSign = rollSign;
                }
            });

            float averageVerticalSpeed = (aircraft.AltitudeMeters - startAltitude) / 8f;
            Assert.That(aircraft.CurrentState, Is.Not.EqualTo(AircraftState.Crashed));
            Assert.That(aircraft.CurrentState, Is.Not.EqualTo(AircraftState.Submerged));
            Assert.That(maxRoll, Is.LessThanOrEqualTo(20f), $"Approach roll unstable. maxRoll={maxRoll:F2}");
            Assert.That(rollSignFlips, Is.LessThanOrEqualTo(3), $"Approach roll oscillated. flips={rollSignFlips}");
            Assert.That(maxPitch, Is.LessThanOrEqualTo(25f), $"Approach pitch unstable. maxPitch={maxPitch:F2}");
            Assert.That(averageVerticalSpeed, Is.GreaterThan(-18f), $"Approach descent runaway. vertical={averageVerticalSpeed:F2}");
        }

        [UnityTest]
        public IEnumerator AircraftCameraTracksCruiseAndTurn()
        {
            yield return LoadFreeFlightScene();
            ArcadeAircraftController aircraft = FindAircraft();
            ChaseCameraRig cameraRig = FindCameraRig();
            Camera camera = Camera.main;
            Assert.That(aircraft, Is.Not.Null);
            Assert.That(cameraRig, Is.Not.Null);
            Assert.That(camera, Is.Not.Null);
            Assert.That(cameraRig.target, Is.EqualTo(aircraft.transform));

            FlightInputReader.SetInputOverrideForTests(new FlightInputSnapshot(
                pitch: 0f,
                roll: 0f,
                yaw: 0f,
                turn: 0f,
                throttleDelta: 1f,
                brake: false));
            yield return WaitForAircraftAltitude(aircraft, 8f, 400);

            float cruiseMinDistance = float.PositiveInfinity;
            float cruiseMaxDistance = 0f;
            float dynamicMinDistance = float.PositiveInfinity;
            float dynamicMaxDistance = 0f;
            float maxCenterOffset = 0f;
            float minimumVisibleGroundRatio = 1f;
            bool aircraftStayedInFront = true;

            yield return SimulateFixedAndRenderFrames(100, () =>
            {
                SampleAircraftCamera(aircraft, camera, ref cruiseMinDistance, ref cruiseMaxDistance, ref maxCenterOffset, ref minimumVisibleGroundRatio, ref aircraftStayedInFront);
                dynamicMinDistance = Mathf.Min(dynamicMinDistance, Vector3.Distance(camera.transform.position, aircraft.transform.position));
                dynamicMaxDistance = Mathf.Max(dynamicMaxDistance, Vector3.Distance(camera.transform.position, aircraft.transform.position));
            });

            FlightInputReader.SetInputOverrideForTests(new FlightInputSnapshot(
                pitch: 0f,
                roll: 0f,
                yaw: 0f,
                turn: 1f,
                throttleDelta: 0f,
                brake: false));
            yield return SimulateFixedAndRenderFrames(150, () =>
            {
                float distance = Vector3.Distance(camera.transform.position, aircraft.transform.position);
                dynamicMinDistance = Mathf.Min(dynamicMinDistance, distance);
                dynamicMaxDistance = Mathf.Max(dynamicMaxDistance, distance);
                SampleAircraftCamera(aircraft, camera, ref dynamicMinDistance, ref dynamicMaxDistance, ref maxCenterOffset, ref minimumVisibleGroundRatio, ref aircraftStayedInFront);
            });

            Assert.That(cruiseMinDistance, Is.GreaterThanOrEqualTo(9f), $"Cruise camera too close. min={cruiseMinDistance:F2}");
            Assert.That(cruiseMaxDistance, Is.LessThanOrEqualTo(16f), $"Cruise camera too far. max={cruiseMaxDistance:F2}");
            Assert.That(dynamicMinDistance, Is.GreaterThanOrEqualTo(8f), $"Dynamic aircraft camera too close. min={dynamicMinDistance:F2}");
            Assert.That(dynamicMaxDistance, Is.LessThanOrEqualTo(18f), $"Dynamic aircraft camera too far. max={dynamicMaxDistance:F2}");
            Assert.That(aircraftStayedInFront, Is.True, "Aircraft camera looked away from the aircraft.");
            Assert.That(maxCenterOffset, Is.LessThanOrEqualTo(0.72f), $"Aircraft left readable screen center. offset={maxCenterOffset:F2}");
            Assert.That(minimumVisibleGroundRatio, Is.LessThan(0.96f), "Aircraft camera stayed in sky-only framing.");
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
                yield return new WaitForFixedUpdate();
                onFrame();
            }
        }

        private static IEnumerator SimulateFixedAndRenderFrames(int frameCount, Action onFrame)
        {
            for (int i = 0; i < frameCount; i++)
            {
                yield return new WaitForFixedUpdate();
                yield return null;
                onFrame();
            }
        }

        private static IEnumerator WaitForAircraftAltitude(ArcadeAircraftController aircraft, float altitudeMeters, int maxFrames)
        {
            for (int i = 0; i < maxFrames && aircraft.AltitudeMeters < altitudeMeters; i++)
            {
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

        private static float SignedPitchDegrees(Transform target)
        {
            return Mathf.DeltaAngle(0f, target.eulerAngles.x);
        }

        private static void SampleAircraftCamera(
            ArcadeAircraftController aircraft,
            Camera camera,
            ref float minDistance,
            ref float maxDistance,
            ref float maxCenterOffset,
            ref float minimumVisibleGroundRatio,
            ref bool aircraftStayedInFront)
        {
            float distance = Vector3.Distance(camera.transform.position, aircraft.transform.position);
            minDistance = Mathf.Min(minDistance, distance);
            maxDistance = Mathf.Max(maxDistance, distance);

            Vector3 viewportPoint = camera.WorldToViewportPoint(aircraft.transform.position);
            aircraftStayedInFront &= viewportPoint.z > 0f;
            maxCenterOffset = Mathf.Max(
                maxCenterOffset,
                Mathf.Abs(viewportPoint.x - 0.5f) + Mathf.Abs(viewportPoint.y - 0.5f));

            Vector3 forward = camera.transform.forward;
            float skyOnlyRatio = Mathf.InverseLerp(0.1f, 0.8f, forward.y);
            minimumVisibleGroundRatio = Mathf.Min(minimumVisibleGroundRatio, skyOnlyRatio);
        }

        private static ArcadeAircraftController FindAircraft()
        {
            return UnityEngine.Object.FindFirstObjectByType<ArcadeAircraftController>();
        }

        private static ChaseCameraRig FindCameraRig()
        {
            return UnityEngine.Object.FindFirstObjectByType<ChaseCameraRig>();
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

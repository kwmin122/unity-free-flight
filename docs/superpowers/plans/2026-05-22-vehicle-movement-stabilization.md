# Vehicle Movement Stabilization Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Lock the car and aircraft movement feel with measurable PlayMode thresholds before any Seoul world expansion work.

**Architecture:** Keep the current single-scene Unity vertical slice and existing runtime controllers. Add strict PlayMode tests first, then tune the car controller, aircraft controller, and scene defaults only where those tests prove a gap. Reuse `ChaseCameraRig`; do not introduce a second camera system.

**Tech Stack:** Unity 6000.3.11f1, C#, Unity Test Framework, NUnit, current `MINgo.Runtime`, `MINgo.EditMode`, and `MINgo.PlayMode` assemblies.

---

## File Structure

- Modify: `Assets/MINgo/Tests/PlayMode/FlightPlayModeSmokeTests.cs`
  - Owns end-to-end scene movement tests for car, aircraft, and camera.
- Modify: `Assets/MINgo/Scripts/Vehicles/ArcadeCarController.cs`
  - Owns car acceleration, coasting, reverse, handbrake yaw assist, and grounded stability.
- Modify: `Assets/MINgo/Scripts/Flight/ArcadeAircraftController.cs`
  - Owns aircraft takeoff, banked turn response, auto-level, slowdown, and descent assist.
- Modify: `Assets/MINgo/Editor/FreeFlightSceneBuilder.cs`
  - Owns scene default controller/camera values for generated `FreeFlightSandbox.unity`.
- Modify: `Assets/Scenes/FreeFlightSandbox.unity`
  - Regenerated scene from `FreeFlightSceneBuilder`.
- Create: `docs/superpowers/checkpoints/phase-23-vehicle-movement-stabilization.md`
  - Records root causes, tuning choices, and verification outputs.

## Commands

Use the Unity binary already used by this repo:

```bash
UNITY="/Applications/Unity/Hub/Editor/6000.3.11f1/Unity.app/Contents/MacOS/Unity"
PROJECT="/Users/a0000/Library/Mobile Documents/com~apple~CloudDocs/Desktop/dev/MINgo"
```

When the real editor is open, verify in a temp copy:

```bash
TMP="$(mktemp -d /tmp/MINgo-movement-verify.XXXXXX)"
rsync -a --delete \
  --exclude Library \
  --exclude Temp \
  --exclude Logs \
  --exclude UserSettings \
  --exclude Builds \
  --exclude .git \
  --exclude '*.csproj' \
  --exclude '*.slnx' \
  "$PROJECT/" "$TMP/"
```

---

### Task 1: Strengthen Car Movement PlayMode Tests

**Files:**
- Modify: `Assets/MINgo/Tests/PlayMode/FlightPlayModeSmokeTests.cs`

- [ ] **Step 1: Add helper methods for movement metrics**

Add these helpers near the bottom of `FlightPlayModeSmokeTests`, above `LoadFreeFlightScene()`:

```csharp
private static IEnumerator SimulateFixedFrames(int frameCount)
{
    for (int i = 0; i < frameCount; i++)
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
```

- [ ] **Step 2: Add forward launch and coasting test**

Add this test after `CarAccelerationStaysGrounded()`:

```csharp
[UnityTest]
public IEnumerator CarForwardLaunchAndNeutralCoastMeetThresholds()
{
    yield return LoadFreeFlightScene();
    ArcadeCarController car = ActivateCarForTest();
    Assert.That(car, Is.Not.Null);

    Vector3 startPosition = car.transform.position;
    VehicleInputReader.SetInputOverrideForTests(new VehicleInputSnapshot(
        throttle: 1f,
        steer: 0f,
        handbrake: false,
        switchVehicle: false));

    yield return SimulateFixedFrames(100);
    float speedAfterTwoSeconds = car.SpeedMetersPerSecond;

    yield return SimulateFixedFrames(100);
    float speedAtRelease = car.SpeedMetersPerSecond;
    float forwardDistance = HorizontalDistance(startPosition, car.transform.position);

    VehicleInputReader.SetInputOverrideForTests(new VehicleInputSnapshot(
        throttle: 0f,
        steer: 0f,
        handbrake: false,
        switchVehicle: false));

    yield return SimulateFixedFrames(150);
    float speedAfterCoast = car.SpeedMetersPerSecond;

    Assert.That(speedAfterTwoSeconds, Is.GreaterThanOrEqualTo(6f));
    Assert.That(forwardDistance, Is.GreaterThanOrEqualTo(14f));
    Assert.That(speedAfterCoast, Is.LessThanOrEqualTo(speedAtRelease * 0.6f));
    Assert.That(car.GroundedWheelCount, Is.GreaterThanOrEqualTo(3));
    Assert.That(car.transform.position.y, Is.LessThan(2.5f));
    Assert.That(Mathf.Abs(car.RollDegrees), Is.LessThan(18f));
}
```

- [ ] **Step 3: Add handbrake turn test**

Add this test after `CarReverseRightInputBacksUpAndTurns()`:

```csharp
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

    yield return SimulateFixedFrames(120);
    float yawBeforeHandbrake = car.transform.eulerAngles.y;

    VehicleInputReader.SetInputOverrideForTests(new VehicleInputSnapshot(
        throttle: 0f,
        steer: 1f,
        handbrake: true,
        switchVehicle: false));

    yield return SimulateFixedFrames(100);
    float yawDelta = Mathf.Abs(Mathf.DeltaAngle(yawBeforeHandbrake, car.transform.eulerAngles.y));

    Assert.That(yawDelta, Is.GreaterThanOrEqualTo(18f));
    Assert.That(car.GroundedWheelCount, Is.GreaterThanOrEqualTo(3));
    Assert.That(car.transform.position.y, Is.LessThan(2.5f));
    Assert.That(Mathf.Abs(car.RollDegrees), Is.LessThan(18f));
}
```

- [ ] **Step 4: Run the targeted car PlayMode tests and verify failures before implementation**

Run:

```bash
"$UNITY" -batchmode -nographics \
  -projectPath "$PROJECT" \
  -runTests -testPlatform PlayMode \
  -testFilter "MINgo.Tests.FlightPlayModeSmokeTests" \
  -testResults "Builds/TestResults/playmode-car-movement-red.xml" \
  -logFile "/tmp/mingo-playmode-car-movement-red.log"
```

Expected: at least one new test fails because neutral coasting or handbrake yaw is not yet tuned to the stricter thresholds.

- [ ] **Step 5: Commit the failing tests**

```bash
git add Assets/MINgo/Tests/PlayMode/FlightPlayModeSmokeTests.cs
git commit -m "test: define car movement thresholds"
```

---

### Task 2: Tune Car Movement To Pass The New Tests

**Files:**
- Modify: `Assets/MINgo/Scripts/Vehicles/ArcadeCarController.cs`
- Modify: `Assets/MINgo/Editor/FreeFlightSceneBuilder.cs`

- [ ] **Step 1: Add car tuning fields**

In `ArcadeCarController`, add these public fields next to the existing car motion fields:

```csharp
public float neutralCoastAcceleration = 8.5f;
public float handbrakeYawAcceleration = 95f;
public float handbrakeMinimumSpeed = 3.5f;
```

- [ ] **Step 2: Call coasting and handbrake assists from `FixedUpdate`**

In `FixedUpdate()`, after `ApplyDriveAssist(CurrentDriveMode, forwardSpeed);`, add:

```csharp
ApplyNeutralCoastAssist(input, forwardSpeed);
ApplyHandbrakeTurnAssist(input);
```

- [ ] **Step 3: Implement neutral coasting**

Add this private method below `ApplyDriveAssist`:

```csharp
private void ApplyNeutralCoastAssist(VehicleInputSnapshot input, float forwardSpeed)
{
    if (!HasGroundSupport() || Mathf.Abs(input.Throttle) > 0.05f || Mathf.Abs(forwardSpeed) < 0.5f)
    {
        return;
    }

    Vector3 localVelocity = transform.InverseTransformDirection(body.linearVelocity);
    float brakingDelta = Mathf.Sign(forwardSpeed) * neutralCoastAcceleration * Time.fixedDeltaTime;
    if (Mathf.Abs(brakingDelta) > Mathf.Abs(localVelocity.z))
    {
        localVelocity.z = 0f;
    }
    else
    {
        localVelocity.z -= brakingDelta;
    }

    body.linearVelocity = transform.TransformDirection(localVelocity);
}
```

- [ ] **Step 4: Implement handbrake yaw assist**

Add this private method below `ApplyNeutralCoastAssist`:

```csharp
private void ApplyHandbrakeTurnAssist(VehicleInputSnapshot input)
{
    if (!input.Handbrake || !HasGroundSupport() || Mathf.Abs(input.Steer) < 0.05f)
    {
        return;
    }

    if (body.linearVelocity.magnitude < handbrakeMinimumSpeed)
    {
        return;
    }

    body.AddTorque(Vector3.up * (input.Steer * handbrakeYawAcceleration), ForceMode.Acceleration);
}
```

- [ ] **Step 5: Update scene defaults**

In `FreeFlightSceneBuilder.CreatePlayerCar()`, after `controller.lowGroundSupportHeight = 1.5f;`, add:

```csharp
controller.neutralCoastAcceleration = 8.5f;
controller.handbrakeYawAcceleration = 95f;
controller.handbrakeMinimumSpeed = 3.5f;
```

- [ ] **Step 6: Regenerate the scene**

Run:

```bash
"$UNITY" -batchmode -nographics -quit \
  -projectPath "$PROJECT" \
  -executeMethod MINgo.EditorTools.FreeFlightSceneBuilder.RebuildScene \
  -logFile "/tmp/mingo-rebuild-scene-car-movement.log"
```

Expected: Unity exits with code `0` and updates `Assets/Scenes/FreeFlightSandbox.unity`.

- [ ] **Step 7: Verify targeted car tests pass**

Run:

```bash
"$UNITY" -batchmode -nographics \
  -projectPath "$PROJECT" \
  -runTests -testPlatform PlayMode \
  -testFilter "MINgo.Tests.FlightPlayModeSmokeTests" \
  -testResults "Builds/TestResults/playmode-car-movement-green.xml" \
  -logFile "/tmp/mingo-playmode-car-movement-green.log"
```

Expected: targeted car tests pass with `failed="0"`.

- [ ] **Step 8: Commit car implementation**

```bash
git add Assets/MINgo/Scripts/Vehicles/ArcadeCarController.cs Assets/MINgo/Editor/FreeFlightSceneBuilder.cs Assets/Scenes/FreeFlightSandbox.unity
git commit -m "fix: tune car coasting and handbrake movement"
```

---

### Task 3: Strengthen Aircraft Movement PlayMode Tests

**Files:**
- Modify: `Assets/MINgo/Tests/PlayMode/FlightPlayModeSmokeTests.cs`

- [ ] **Step 1: Add aircraft takeoff threshold test**

Add this test after `HoldingThrottleAcceleratesTheSceneAircraft()`:

```csharp
[UnityTest]
public IEnumerator AircraftTakeoffMeetsAltitudeAndSpeedThresholds()
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

    yield return SimulateFixedFrames(400);

    Assert.That(aircraft.AltitudeMeters, Is.GreaterThanOrEqualTo(8f));
    Assert.That(aircraft.SpeedMetersPerSecond, Is.GreaterThanOrEqualTo(14f));
    Assert.That(aircraft.CurrentState, Is.Not.EqualTo(AircraftState.Crashed));
    Assert.That(aircraft.CurrentState, Is.Not.EqualTo(AircraftState.Submerged));
}
```

- [ ] **Step 2: Add stricter turn recovery test**

Add this test after `TurnInputBanksThenReleaseRecoversTowardLevel()`:

```csharp
[UnityTest]
public IEnumerator AircraftBankedTurnAndAutoLevelMeetThresholds()
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

    yield return SimulateFixedFrames(260);
    float startHeading = aircraft.transform.eulerAngles.y;

    FlightInputReader.SetInputOverrideForTests(new FlightInputSnapshot(
        pitch: 0f,
        roll: 0f,
        yaw: 0f,
        turn: 1f,
        throttleDelta: 0f,
        brake: false));

    yield return SimulateFixedFrames(150);
    float bankDuringTurn = Mathf.Abs(aircraft.RollDegrees);
    float headingDelta = Mathf.Abs(Mathf.DeltaAngle(startHeading, aircraft.transform.eulerAngles.y));

    FlightInputReader.SetInputOverrideForTests(new FlightInputSnapshot(
        pitch: 0f,
        roll: 0f,
        yaw: 0f,
        turn: 0f,
        throttleDelta: 0f,
        brake: false));

    yield return SimulateFixedFrames(200);
    float bankAfterRelease = Mathf.Abs(aircraft.RollDegrees);

    Assert.That(bankDuringTurn, Is.InRange(15f, 45f));
    Assert.That(headingDelta, Is.GreaterThanOrEqualTo(8f));
    Assert.That(bankAfterRelease, Is.LessThanOrEqualTo(10f));
    Assert.That(aircraft.CurrentState, Is.Not.EqualTo(AircraftState.Crashed));
    Assert.That(aircraft.CurrentState, Is.Not.EqualTo(AircraftState.Submerged));
}
```

- [ ] **Step 3: Add slowdown/descent comparison test**

Add this test after `HoldingSlowdownInputAddsAirbrakeDragAfterThrottleCut()`:

```csharp
[UnityTest]
public IEnumerator AircraftSlowdownAddsDescentAssistComparedWithIdle()
{
    yield return LoadFreeFlightScene();
    ArcadeAircraftController idleAircraft = FindAircraft();
    Assert.That(idleAircraft, Is.Not.Null);

    FlightInputReader.SetInputOverrideForTests(new FlightInputSnapshot(0f, 0f, 0f, 0f, 1f, false));
    yield return SimulateFixedFrames(320);
    FlightInputReader.SetInputOverrideForTests(new FlightInputSnapshot(0f, 0f, 0f, 0f, -1f, false));
    yield return SimulateFixedFrames(120);
    FlightInputReader.SetInputOverrideForTests(new FlightInputSnapshot(0f, 0f, 0f, 0f, 0f, false));

    float idleStartAltitude = idleAircraft.AltitudeMeters;
    float idleStartSpeed = idleAircraft.SpeedMetersPerSecond;
    yield return SimulateFixedFrames(150);
    float idleAltitudeDelta = idleAircraft.AltitudeMeters - idleStartAltitude;
    float idleSpeedDelta = idleAircraft.SpeedMetersPerSecond - idleStartSpeed;

    yield return LoadFreeFlightScene();
    ArcadeAircraftController brakingAircraft = FindAircraft();
    Assert.That(brakingAircraft, Is.Not.Null);

    FlightInputReader.SetInputOverrideForTests(new FlightInputSnapshot(0f, 0f, 0f, 0f, 1f, false));
    yield return SimulateFixedFrames(320);
    FlightInputReader.SetInputOverrideForTests(new FlightInputSnapshot(0f, 0f, 0f, 0f, -1f, false));
    yield return SimulateFixedFrames(120);

    float brakeStartAltitude = brakingAircraft.AltitudeMeters;
    float brakeStartSpeed = brakingAircraft.SpeedMetersPerSecond;
    yield return SimulateFixedFrames(150);
    float brakeAltitudeDelta = brakingAircraft.AltitudeMeters - brakeStartAltitude;
    float brakeSpeedDelta = brakingAircraft.SpeedMetersPerSecond - brakeStartSpeed;

    Assert.That(brakeSpeedDelta, Is.LessThan(idleSpeedDelta - 1.0f));
    Assert.That(brakeAltitudeDelta, Is.LessThan(idleAltitudeDelta - 0.5f));
    Assert.That(brakingAircraft.CurrentState, Is.Not.EqualTo(AircraftState.Crashed));
    Assert.That(brakingAircraft.CurrentState, Is.Not.EqualTo(AircraftState.Submerged));
}
```

- [ ] **Step 4: Run targeted aircraft tests and verify failures before implementation**

Run:

```bash
"$UNITY" -batchmode -nographics \
  -projectPath "$PROJECT" \
  -runTests -testPlatform PlayMode \
  -testFilter "MINgo.Tests.FlightPlayModeSmokeTests" \
  -testResults "Builds/TestResults/playmode-aircraft-movement-red.xml" \
  -logFile "/tmp/mingo-playmode-aircraft-movement-red.log"
```

Expected: at least one aircraft test fails because takeoff, bank recovery, or descent assist is not yet tuned to the stricter thresholds.

- [ ] **Step 5: Commit the failing aircraft tests**

```bash
git add Assets/MINgo/Tests/PlayMode/FlightPlayModeSmokeTests.cs
git commit -m "test: define aircraft movement thresholds"
```

---

### Task 4: Tune Aircraft Movement To Pass The New Tests

**Files:**
- Modify: `Assets/MINgo/Scripts/Flight/ArcadeAircraftController.cs`
- Modify: `Assets/MINgo/Editor/FreeFlightSceneBuilder.cs`
- Modify: `Assets/Scenes/FreeFlightSandbox.unity`

- [ ] **Step 1: Add aircraft tuning fields**

In `ArcadeAircraftController`, add these fields next to the existing flight force fields:

```csharp
public float takeoffLiftAssist = 18f;
public float slowdownDescentAcceleration = 2.8f;
public float slowdownPitchDamping = 0.55f;
```

- [ ] **Step 2: Add takeoff lift assist**

In `FixedUpdate()`, after `body.AddForce(liftForce, ForceMode.Force);`, add:

```csharp
if (Throttle01 > 0.85f && forwardSpeed > takeoffSpeed * 0.55f && AltitudeMeters < 12f)
{
    body.AddForce(Vector3.up * takeoffLiftAssist, ForceMode.Acceleration);
}
```

- [ ] **Step 3: Add slowdown descent assist**

In the existing `if (input.ThrottleDelta < -0.05f && !hasGroundContact && velocity.sqrMagnitude > 1f)` block, after the current airbrake force, add:

```csharp
body.AddForce(Vector3.down * slowdownDescentAcceleration, ForceMode.Acceleration);
Vector3 localAngularVelocity = transform.InverseTransformDirection(body.angularVelocity);
localAngularVelocity.x *= slowdownPitchDamping;
body.angularVelocity = transform.TransformDirection(localAngularVelocity);
```

- [ ] **Step 4: Tune scene defaults**

In `FreeFlightSceneBuilder.CreateAircraft()`, after `controller.throttleChangeRate = 3.2f;`, add:

```csharp
controller.takeoffLiftAssist = 18f;
controller.slowdownDescentAcceleration = 2.8f;
controller.slowdownPitchDamping = 0.55f;
```

Also keep these existing defaults unchanged unless tests prove they fail:

```csharp
controller.stabilization = 4.5f;
controller.autoLevel = 6f;
controller.autoLevelRotationRate = 2f;
controller.assistedBankAngle = 22f;
controller.throttleChangeRate = 3.2f;
```

- [ ] **Step 5: Regenerate the scene**

Run:

```bash
"$UNITY" -batchmode -nographics -quit \
  -projectPath "$PROJECT" \
  -executeMethod MINgo.EditorTools.FreeFlightSceneBuilder.RebuildScene \
  -logFile "/tmp/mingo-rebuild-scene-aircraft-movement.log"
```

Expected: Unity exits with code `0` and updates `Assets/Scenes/FreeFlightSandbox.unity`.

- [ ] **Step 6: Verify targeted aircraft tests pass**

Run:

```bash
"$UNITY" -batchmode -nographics \
  -projectPath "$PROJECT" \
  -runTests -testPlatform PlayMode \
  -testFilter "MINgo.Tests.FlightPlayModeSmokeTests" \
  -testResults "Builds/TestResults/playmode-aircraft-movement-green.xml" \
  -logFile "/tmp/mingo-playmode-aircraft-movement-green.log"
```

Expected: targeted aircraft tests pass with `failed="0"`.

- [ ] **Step 7: Commit aircraft implementation**

```bash
git add Assets/MINgo/Scripts/Flight/ArcadeAircraftController.cs Assets/MINgo/Editor/FreeFlightSceneBuilder.cs Assets/Scenes/FreeFlightSandbox.unity
git commit -m "fix: tune aircraft takeoff and slowdown control"
```

---

### Task 5: Add Camera Follow Threshold Tests

**Files:**
- Modify: `Assets/MINgo/Tests/PlayMode/FlightPlayModeSmokeTests.cs`
- Modify if needed: `Assets/MINgo/Scripts/Flight/ChaseCameraRig.cs`
- Modify if needed: `Assets/MINgo/Editor/FreeFlightSceneBuilder.cs`
- Modify if needed: `Assets/Scenes/FreeFlightSandbox.unity`

- [ ] **Step 1: Add camera helpers**

Add these helper methods near the other private helpers:

```csharp
private static Camera FindMainCamera()
{
    Camera camera = Camera.main;
    Assert.That(camera, Is.Not.Null);
    return camera;
}

private static void AssertCameraDistanceToTarget(Camera camera, Transform target, float minDistance, float maxDistance)
{
    float distance = Vector3.Distance(camera.transform.position, target.position);
    Assert.That(distance, Is.InRange(minDistance, maxDistance));
}

private static void AssertCameraLooksTowardTarget(Camera camera, Transform target)
{
    Vector3 toTarget = (target.position - camera.transform.position).normalized;
    float alignment = Vector3.Dot(camera.transform.forward, toTarget);
    Assert.That(alignment, Is.GreaterThan(0.65f));
}
```

- [ ] **Step 2: Add active car camera test**

Add this test before the helper methods:

```csharp
[UnityTest]
public IEnumerator CarCameraTracksBehindActiveVehicle()
{
    yield return LoadFreeFlightScene();
    ArcadeCarController car = ActivateCarForTest();
    Camera camera = FindMainCamera();

    VehicleInputReader.SetInputOverrideForTests(new VehicleInputSnapshot(
        throttle: 1f,
        steer: 0.2f,
        handbrake: false,
        switchVehicle: false));

    yield return SimulateFixedFrames(160);

    AssertCameraDistanceToTarget(camera, car.transform, 6f, 12f);
    AssertCameraLooksTowardTarget(camera, car.transform);
    Vector3 carLocalCamera = car.transform.InverseTransformPoint(camera.transform.position);
    Assert.That(carLocalCamera.z, Is.LessThan(0f));
}
```

- [ ] **Step 3: Add active aircraft camera test**

Add this test after `CarCameraTracksBehindActiveVehicle()`:

```csharp
[UnityTest]
public IEnumerator AircraftCameraTracksCruiseAndTurn()
{
    yield return LoadFreeFlightScene();
    ArcadeAircraftController aircraft = FindAircraft();
    Camera camera = FindMainCamera();

    FlightInputReader.SetInputOverrideForTests(new FlightInputSnapshot(
        pitch: 0f,
        roll: 0f,
        yaw: 0f,
        turn: 0f,
        throttleDelta: 1f,
        brake: false));

    yield return SimulateFixedFrames(260);
    AssertCameraDistanceToTarget(camera, aircraft.transform, 7f, 11f);
    AssertCameraLooksTowardTarget(camera, aircraft.transform);

    FlightInputReader.SetInputOverrideForTests(new FlightInputSnapshot(
        pitch: 0f,
        roll: 0f,
        yaw: 0f,
        turn: 1f,
        throttleDelta: 0f,
        brake: false));

    yield return SimulateFixedFrames(120);
    AssertCameraDistanceToTarget(camera, aircraft.transform, 7f, 11f);
    AssertCameraLooksTowardTarget(camera, aircraft.transform);
}
```

- [ ] **Step 4: Run camera tests**

Run:

```bash
"$UNITY" -batchmode -nographics \
  -projectPath "$PROJECT" \
  -runTests -testPlatform PlayMode \
  -testFilter "MINgo.Tests.FlightPlayModeSmokeTests" \
  -testResults "Builds/TestResults/playmode-camera-movement.xml" \
  -logFile "/tmp/mingo-playmode-camera-movement.log"
```

Expected: tests pass. If either test fails, first tune `FreeFlightSceneBuilder` camera defaults:

```csharp
cameraRig.followDistance = 8f;
cameraRig.followHeight = 2.4f;
cameraRig.lookAhead = 14f;
cameraRig.smoothTime = 0.08f;
cameraRig.rotationSmooth = 8f;
```

If defaults cannot satisfy the test, change `ChaseCameraRig` in the smallest possible way and re-run the tests.

- [ ] **Step 5: Commit camera tests and any camera tuning**

```bash
git add Assets/MINgo/Tests/PlayMode/FlightPlayModeSmokeTests.cs Assets/MINgo/Scripts/Flight/ChaseCameraRig.cs Assets/MINgo/Editor/FreeFlightSceneBuilder.cs Assets/Scenes/FreeFlightSandbox.unity
git commit -m "test: lock active vehicle camera framing"
```

If `ChaseCameraRig.cs`, `FreeFlightSceneBuilder.cs`, or `Assets/Scenes/FreeFlightSandbox.unity` were not changed, omit them from `git add`.

---

### Task 6: Full Verification And Checkpoint

**Files:**
- Create: `docs/superpowers/checkpoints/phase-23-vehicle-movement-stabilization.md`

- [ ] **Step 1: Run full EditMode tests**

Run:

```bash
"$UNITY" -batchmode -nographics \
  -projectPath "$PROJECT" \
  -runTests -testPlatform EditMode \
  -testResults "Builds/TestResults/editmode-vehicle-movement.xml" \
  -logFile "/tmp/mingo-editmode-vehicle-movement.log"
```

Expected: XML root reports `result="Passed"` and `failed="0"`.

- [ ] **Step 2: Run full PlayMode tests**

Run:

```bash
"$UNITY" -batchmode -nographics \
  -projectPath "$PROJECT" \
  -runTests -testPlatform PlayMode \
  -testResults "Builds/TestResults/playmode-vehicle-movement.xml" \
  -logFile "/tmp/mingo-playmode-vehicle-movement.log"
```

Expected: XML root reports `result="Passed"` and `failed="0"`.

- [ ] **Step 3: Run macOS build**

Run:

```bash
"$UNITY" -batchmode -nographics -quit \
  -projectPath "$PROJECT" \
  -executeMethod MINgo.EditorTools.MINgoBuildPipeline.BuildMacOSPlayer \
  -logFile "/tmp/mingo-build-vehicle-movement.log"
```

Expected: log contains:

```text
Build Finished, Result: Success.
```

- [ ] **Step 4: Write checkpoint**

Create `docs/superpowers/checkpoints/phase-23-vehicle-movement-stabilization.md`:

```markdown
# Phase 23 Vehicle Movement Stabilization

Date: 2026-05-22

## Goal

Lock car and aircraft movement thresholds before Seoul simulator world expansion.

## Implemented

- Added numeric PlayMode thresholds for car forward launch, coasting, reverse, diagonal input, handbrake turning, and camera follow.
- Tuned `ArcadeCarController` for neutral coasting and handbrake yaw while preserving grounded stability.
- Added numeric PlayMode thresholds for aircraft takeoff, banked turning, auto-level recovery, slowdown/descent assist, and camera follow.
- Tuned `ArcadeAircraftController` for reliable easy takeoff and slowdown descent control.
- Regenerated `Assets/Scenes/FreeFlightSandbox.unity` from `FreeFlightSceneBuilder`.

## Verification

- EditMode: `Builds/TestResults/editmode-vehicle-movement.xml`, result `Passed`, failed `0`.
- PlayMode: `Builds/TestResults/playmode-vehicle-movement.xml`, result `Passed`, failed `0`.
- macOS build: `/tmp/mingo-build-vehicle-movement.log`, `Build Finished, Result: Success.`

## Manual Test

- Press Play in `Assets/Scenes/FreeFlightSandbox.unity`.
- Aircraft: hold `W` to take off, hold turn input, release controls, hold `S` to slow and descend.
- Vehicle switch: press `F` or `Tab`.
- Car: `W` accelerates, releasing `W` slows, `S` reverses, `W+D` and `S+D` arc correctly, `Space+D` handbrake-turns without flipping.
```

- [ ] **Step 5: Run diff hygiene**

Run:

```bash
git diff --check
```

Expected: no output and exit code `0`.

- [ ] **Step 6: Commit checkpoint and final verified state**

```bash
git add Assets/MINgo/Tests/PlayMode/FlightPlayModeSmokeTests.cs \
  Assets/MINgo/Scripts/Vehicles/ArcadeCarController.cs \
  Assets/MINgo/Scripts/Flight/ArcadeAircraftController.cs \
  Assets/MINgo/Scripts/Flight/ChaseCameraRig.cs \
  Assets/MINgo/Editor/FreeFlightSceneBuilder.cs \
  Assets/Scenes/FreeFlightSandbox.unity \
  docs/superpowers/checkpoints/phase-23-vehicle-movement-stabilization.md
git commit -m "chore: record vehicle movement stabilization verification"
```

If a listed file has no changes, omit it from `git add`.

- [ ] **Step 7: Push and verify remote**

Run:

```bash
git push origin main
git rev-parse HEAD
git ls-remote origin refs/heads/main
```

Expected: local `HEAD` SHA matches the remote `refs/heads/main` SHA.

---

## Self-Review

Spec coverage:

- Car forward launch, forward travel, coasting, reverse, diagonal input, handbrake, stability, and camera are covered by Tasks 1, 2, and 5.
- Aircraft takeoff, speed, banked turn, auto-level, slowdown, descent, landing-stability proxy, and camera are covered by Tasks 3, 4, and 5.
- Scene defaults and generated scene updates are covered by Tasks 2 and 4.
- Full EditMode, PlayMode, build, checkpoint, diff hygiene, commit, push, and remote verification are covered by Task 6.
- Seoul world, `imagegen` city assets, and map expansion are intentionally excluded and must be handled by a separate spec after this plan is green.

Placeholder scan:

- The plan contains no unresolved placeholder implementation steps and no references to undefined files.

Type consistency:

- Test code uses existing `FlightInputSnapshot`, `VehicleInputSnapshot`, `ArcadeAircraftController`, `ArcadeCarController`, `PlayerVehicleSwitcher`, and `ChaseCameraRig` names.
- New fields are explicitly introduced before scene defaults reference them.

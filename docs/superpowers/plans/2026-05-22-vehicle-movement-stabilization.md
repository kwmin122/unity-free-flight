# Vehicle Movement Stabilization Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Lock car and aircraft movement feel with measurable PlayMode thresholds before any Seoul world expansion work.

**Assumption:** This pass targets GTA-style arcade readability, not exact GTA handling data or real flight simulation. `W/S/A/D/Space` should feel familiar, responsive, and testable.

**Architecture:** Keep the current single-scene Unity vertical slice and existing runtime controllers. Add strict PlayMode tests first, then tune the car controller, aircraft controller, and scene defaults only where those tests prove a gap. Reuse `ChaseCameraRig`; do not introduce a second camera system.

**Research Inputs To Encode Into Tests:**

- GTA-style surface controls: `W/S` forward/reverse, `A/D` steering, `Space` handbrake. Source: https://gta.fandom.com/wiki/Controls_for_GTA_V
- Unity WheelCollider reference shape: speed-based steering, roughly `30` degrees low-speed steering and `10` degrees high-speed steering as a useful range. Source: https://docs.unity.cn/Manual/WheelColliderTutorial.html
- Arcade vehicle controller patterns: direction-change braking, handbrake as a dedicated yaw/traction event, stable camera follow. Source: https://nwhcoding.com/VehiclePhysics/manual/CarController.html
- GTA/FiveM handling perspective: previous thresholds were slow city-car smoke tests, so launch, turn, and handbrake thresholds must be materially higher. Source: https://xgamingserver.com/tools/fivem/vehicles
- C172 takeoff data is real-flight context only; this project keeps the aircraft arcade-first with fast takeoff and forgiving landing approach. Source: https://www.x-plane.com/manuals/C172_Pilot_Operating_Manual.pdf

**Tech Stack:** Unity 6000.3.11f1, C#, Unity Test Framework, NUnit, current `MINgo.Runtime`, `MINgo.EditMode`, and `MINgo.PlayMode` assemblies.

---

## File Structure

- Modify: `Assets/MINgo/Tests/PlayMode/FlightPlayModeSmokeTests.cs`
  - Owns end-to-end scene movement tests for car, aircraft, and camera.
- Modify: `Assets/MINgo/Scripts/Vehicles/ArcadeCarController.cs`
  - Owns car acceleration, coasting, braking, reverse, handbrake yaw assist, and grounded stability.
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

## Task 1: Strengthen Car Movement PlayMode Tests

**Files:**

- Modify: `Assets/MINgo/Tests/PlayMode/FlightPlayModeSmokeTests.cs`

- [ ] **Step 1: Add helper methods for strict movement metrics**

Add `using System;` if it is not already present, then add these helpers near the bottom of `FlightPlayModeSmokeTests`, above `LoadFreeFlightScene()`:

```csharp
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
```

- [ ] **Step 2: Add stricter forward launch and neutral coasting test**

Add this test after `CarAccelerationStaysGrounded()`:

```csharp
[UnityTest]
public IEnumerator CarForwardLaunchAndNeutralCoastMeetArcadeThresholds()
{
    yield return LoadFreeFlightScene();
    ArcadeCarController car = ActivateCarForTest();
    Assert.That(car, Is.Not.Null);

    Vector3 startPosition = car.transform.position;
    int groundedSamples = 0;
    int totalSamples = 0;

    VehicleInputReader.SetInputOverrideForTests(new VehicleInputSnapshot(1f, 0f, false, false));

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

    VehicleInputReader.SetInputOverrideForTests(new VehicleInputSnapshot(0f, 0f, false, false));

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
```

- [ ] **Step 3: Add braking and reverse transition test**

Add this test after the forward/coasting test:

```csharp
[UnityTest]
public IEnumerator CarBrakesFromSpeedThenReversesFromLowSpeed()
{
    yield return LoadFreeFlightScene();
    ArcadeCarController car = ActivateCarForTest();
    Assert.That(car, Is.Not.Null);

    VehicleInputReader.SetInputOverrideForTests(new VehicleInputSnapshot(1f, 0f, false, false));
    yield return SimulateFixedFrames(150);
    float brakeStartSpeed = car.SpeedMetersPerSecond;
    Assert.That(brakeStartSpeed, Is.GreaterThanOrEqualTo(8f));

    VehicleInputReader.SetInputOverrideForTests(new VehicleInputSnapshot(-1f, 0f, false, false));
    yield return SimulateFixedFrames(100);
    Assert.That(car.SpeedMetersPerSecond, Is.LessThanOrEqualTo(brakeStartSpeed * 0.4f));

    yield return LoadFreeFlightScene();
    car = ActivateCarForTest();
    Assert.That(car.SpeedMetersPerSecond, Is.LessThanOrEqualTo(1.5f));

    Vector3 startPosition = car.transform.position;
    Vector3 startForward = car.transform.forward;
    VehicleInputReader.SetInputOverrideForTests(new VehicleInputSnapshot(-1f, 0f, false, false));
    yield return SimulateFixedFrames(125);

    Assert.That(SignedForwardTravel(startPosition, car.transform.position, startForward), Is.LessThanOrEqualTo(-4f));
    Assert.That(car.GroundedWheelCount, Is.GreaterThanOrEqualTo(3));
    Assert.That(Mathf.Abs(car.RollDegrees), Is.LessThan(12f));
}
```

- [ ] **Step 4: Replace weak diagonal input checks with arcade thresholds**

Update the existing `CarForwardRightInputMovesAndTurns()` and `CarReverseRightInputBacksUpAndTurns()` tests:

```csharp
Assert.That(speed, Is.GreaterThanOrEqualTo(7.5f));
Assert.That(yawDelta, Is.GreaterThanOrEqualTo(20f));
```

For reverse diagonal, use a 2 second sample and assert:

```csharp
Assert.That(backwardTravel, Is.LessThanOrEqualTo(-4f));
Assert.That(yawDelta, Is.GreaterThanOrEqualTo(10f));
```

Both tests must preserve a grounded-wheel check of at least `3` wheels during normal driving.

- [ ] **Step 5: Add handbrake turn test**

Add this test after the diagonal input tests:

```csharp
[UnityTest]
public IEnumerator CarHandbrakeTurnRotatesWithoutFlipping()
{
    yield return LoadFreeFlightScene();
    ArcadeCarController car = ActivateCarForTest();
    Assert.That(car, Is.Not.Null);

    VehicleInputReader.SetInputOverrideForTests(new VehicleInputSnapshot(1f, 0f, false, false));

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

    VehicleInputReader.SetInputOverrideForTests(new VehicleInputSnapshot(0f, 1f, true, false));

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
```

- [ ] **Step 6: Run targeted car PlayMode tests and verify failures before implementation**

Run:

```bash
"$UNITY" -batchmode -nographics \
  -projectPath "$PROJECT" \
  -runTests -testPlatform PlayMode \
  -testFilter "MINgo.Tests.FlightPlayModeSmokeTests" \
  -testResults "Builds/TestResults/playmode-car-movement-red.xml" \
  -logFile "/tmp/mingo-playmode-car-movement-red.log"
```

Expected: at least one new or tightened test fails because launch, coasting, braking, diagonal yaw, or handbrake yaw is not yet tuned to the stricter thresholds.

- [ ] **Step 7: Commit the failing tests**

```bash
git add Assets/MINgo/Tests/PlayMode/FlightPlayModeSmokeTests.cs
git commit -m "test: tighten car movement thresholds"
```

---

## Task 2: Tune Car Movement To Pass The New Tests

**Files:**

- Modify: `Assets/MINgo/Scripts/Vehicles/ArcadeCarController.cs`
- Modify: `Assets/MINgo/Editor/FreeFlightSceneBuilder.cs`
- Modify: `Assets/Scenes/FreeFlightSandbox.unity`

- [ ] **Step 1: Replace loose steering fields and add car tuning fields**

In `ArcadeCarController`, replace the current `maxSteerDegrees`, `fullSteerSpeed`, and `reducedSteerSpeed` steering-field shape with explicit low/high steering fields, then add braking/coasting/handbrake fields next to the other car motion fields:

```csharp
public float lowSpeedSteerDegrees = 30f;
public float highSpeedSteerDegrees = 10f;
public float highSpeedSteerReference = 20f;
public float neutralCoastAcceleration = 5.5f;
public float directionChangeBrakeAcceleration = 14f;
public float handbrakeYawAcceleration = 130f;
public float handbrakeMinimumSpeed = 8f;
public float handbrakeMaximumAssistSpeed = 12f;
```

- [ ] **Step 2: Apply speed-based steering**

Where steering angle is assigned to WheelColliders, replace a single constant steering angle with:

```csharp
float speed01 = Mathf.Clamp01(Mathf.Abs(forwardSpeed) / Mathf.Max(highSpeedSteerReference, 0.01f));
float steerDegrees = Mathf.Lerp(lowSpeedSteerDegrees, highSpeedSteerDegrees, speed01);
float steerAngle = input.Steer * steerDegrees;
```

Preserve the current steering direction semantics. `W+D` must turn right; `S+D` must back up while yawing right in the test's measured direction.

- [ ] **Step 3: Call braking, coasting, and handbrake assists from `FixedUpdate`**

In `FixedUpdate()`, after the main drive assist and before angular stabilization, call:

```csharp
ApplyDirectionChangeBrake(input, forwardSpeed);
ApplyNeutralCoastAssist(input, forwardSpeed);
ApplyHandbrakeTurnAssist(input);
```

- [ ] **Step 4: Implement automatic braking before reverse**

Add this private method near the other drive helpers:

```csharp
private void ApplyDirectionChangeBrake(VehicleInputSnapshot input, float forwardSpeed)
{
    if (!HasGroundSupport())
    {
        return;
    }

    bool brakingForward = forwardSpeed > 0.5f && input.Throttle < -0.05f;
    bool brakingReverse = forwardSpeed < -0.5f && input.Throttle > 0.05f;
    if (!brakingForward && !brakingReverse)
    {
        return;
    }

    Vector3 localVelocity = transform.InverseTransformDirection(body.linearVelocity);
    float brakeDelta = Mathf.Sign(forwardSpeed) * directionChangeBrakeAcceleration * Time.fixedDeltaTime;
    if (Mathf.Abs(brakeDelta) > Mathf.Abs(localVelocity.z))
    {
        localVelocity.z = 0f;
    }
    else
    {
        localVelocity.z -= brakeDelta;
    }

    body.linearVelocity = transform.TransformDirection(localVelocity);
}
```

- [ ] **Step 5: Implement neutral coasting**

Add this private method below `ApplyDirectionChangeBrake`:

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

- [ ] **Step 6: Implement handbrake yaw assist**

Add this private method below `ApplyNeutralCoastAssist`:

```csharp
private void ApplyHandbrakeTurnAssist(VehicleInputSnapshot input)
{
    if (!input.Handbrake || !HasGroundSupport() || Mathf.Abs(input.Steer) < 0.05f)
    {
        return;
    }

    float speed = body.linearVelocity.magnitude;
    if (speed < handbrakeMinimumSpeed || speed > handbrakeMaximumAssistSpeed + 4f)
    {
        return;
    }

    float speedAssist = Mathf.InverseLerp(handbrakeMinimumSpeed, handbrakeMaximumAssistSpeed, speed);
    body.AddTorque(Vector3.up * (input.Steer * handbrakeYawAcceleration * Mathf.Max(0.35f, speedAssist)), ForceMode.Acceleration);
}
```

- [ ] **Step 7: Update scene defaults**

In `FreeFlightSceneBuilder.CreatePlayerCar()`, set:

```csharp
controller.lowSpeedSteerDegrees = 30f;
controller.highSpeedSteerDegrees = 10f;
controller.highSpeedSteerReference = 20f;
controller.neutralCoastAcceleration = 5.5f;
controller.directionChangeBrakeAcceleration = 14f;
controller.handbrakeYawAcceleration = 130f;
controller.handbrakeMinimumSpeed = 8f;
controller.handbrakeMaximumAssistSpeed = 12f;
```

- [ ] **Step 8: Regenerate the scene**

Run:

```bash
"$UNITY" -batchmode -nographics -quit \
  -projectPath "$PROJECT" \
  -executeMethod MINgo.EditorTools.FreeFlightSceneBuilder.RebuildScene \
  -logFile "/tmp/mingo-rebuild-scene-car-movement.log"
```

Expected: Unity exits with code `0` and updates `Assets/Scenes/FreeFlightSandbox.unity`.

- [ ] **Step 9: Verify targeted car tests pass**

Run:

```bash
"$UNITY" -batchmode -nographics \
  -projectPath "$PROJECT" \
  -runTests -testPlatform PlayMode \
  -testFilter "MINgo.Tests.FlightPlayModeSmokeTests" \
  -testResults "Builds/TestResults/playmode-car-movement-green.xml" \
  -logFile "/tmp/mingo-playmode-car-movement-green.log"
```

Expected: targeted car tests pass with `failed="0"`. If the failure is only numeric tuning, tune fields in the controller/scene defaults; do not weaken the tests.

- [ ] **Step 10: Commit car implementation**

```bash
git add Assets/MINgo/Scripts/Vehicles/ArcadeCarController.cs Assets/MINgo/Editor/FreeFlightSceneBuilder.cs Assets/Scenes/FreeFlightSandbox.unity
git commit -m "fix: tune arcade car handling"
```

---

## Task 3: Strengthen Aircraft Movement PlayMode Tests

**Files:**

- Modify: `Assets/MINgo/Tests/PlayMode/FlightPlayModeSmokeTests.cs`

- [ ] **Step 1: Add aircraft takeoff threshold test**

Add this test after `HoldingThrottleAcceleratesTheSceneAircraft()`:

```csharp
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

    FlightInputReader.SetInputOverrideForTests(new FlightInputSnapshot(0f, 0f, 0f, 0f, 1f, false));

    for (int i = 0; i < 400; i++)
    {
        yield return new WaitForFixedUpdate();
        if (!capturedTakeoff && aircraft.AltitudeMeters >= 8f)
        {
            capturedTakeoff = true;
            takeoffSpeed = aircraft.SpeedMetersPerSecond;
            takeoffTravel = SignedForwardTravel(startPosition, aircraft.transform.position, startForward);
        }
    }

    Assert.That(capturedTakeoff, Is.True);
    Assert.That(takeoffSpeed, Is.InRange(18f, 25f));
    Assert.That(takeoffTravel, Is.LessThanOrEqualTo(180f));
    Assert.That(aircraft.CurrentState, Is.Not.EqualTo(AircraftState.Crashed));
    Assert.That(aircraft.CurrentState, Is.Not.EqualTo(AircraftState.Submerged));
}
```

- [ ] **Step 2: Add stricter turn recovery test**

Update or add the aircraft turn test so it asserts:

```csharp
Assert.That(bankDuringTurn, Is.InRange(15f, 45f));
Assert.That(headingDelta, Is.GreaterThanOrEqualTo(20f));
Assert.That(bankAfterRelease, Is.LessThanOrEqualTo(10f));
```

The test setup should hold takeoff throttle for about `260` fixed frames, hold right turn for `150` fixed frames, then release controls for `200` fixed frames.

- [ ] **Step 3: Split slowdown and descent checks**

Add a direct slowdown test:

```csharp
[UnityTest]
public IEnumerator AircraftSlowdownDropsSpeedByFifteenPercent()
{
    yield return LoadFreeFlightScene();
    ArcadeAircraftController aircraft = FindAircraft();
    Assert.That(aircraft, Is.Not.Null);

    FlightInputReader.SetInputOverrideForTests(new FlightInputSnapshot(0f, 0f, 0f, 0f, 1f, false));
    yield return SimulateFixedFrames(320);

    float speedBeforeSlowdown = aircraft.SpeedMetersPerSecond;
    FlightInputReader.SetInputOverrideForTests(new FlightInputSnapshot(0f, 0f, 0f, 0f, -1f, false));
    yield return SimulateFixedFrames(150);

    Assert.That(aircraft.SpeedMetersPerSecond, Is.LessThanOrEqualTo(speedBeforeSlowdown * 0.85f));
    Assert.That(aircraft.CurrentState, Is.Not.EqualTo(AircraftState.Crashed));
    Assert.That(aircraft.CurrentState, Is.Not.EqualTo(AircraftState.Submerged));
}
```

Add a separate descent comparison test:

```csharp
[UnityTest]
public IEnumerator AircraftSlowdownCreatesMeasurablyMoreDescentThanIdle()
{
    yield return LoadFreeFlightScene();
    ArcadeAircraftController idleAircraft = FindAircraft();
    Assert.That(idleAircraft, Is.Not.Null);

    FlightInputReader.SetInputOverrideForTests(new FlightInputSnapshot(0f, 0f, 0f, 0f, 1f, false));
    yield return SimulateFixedFrames(320);
    FlightInputReader.SetInputOverrideForTests(new FlightInputSnapshot(0f, 0f, 0f, 0f, 0f, false));
    yield return SimulateFixedFrames(60);
    float idleStartVerticalSpeed = idleAircraft.VerticalSpeedMetersPerSecond;
    yield return SimulateFixedFrames(150);
    float idleEndVerticalSpeed = idleAircraft.VerticalSpeedMetersPerSecond;

    yield return LoadFreeFlightScene();
    ArcadeAircraftController brakingAircraft = FindAircraft();
    Assert.That(brakingAircraft, Is.Not.Null);

    FlightInputReader.SetInputOverrideForTests(new FlightInputSnapshot(0f, 0f, 0f, 0f, 1f, false));
    yield return SimulateFixedFrames(320);
    FlightInputReader.SetInputOverrideForTests(new FlightInputSnapshot(0f, 0f, 0f, 0f, -1f, false));
    yield return SimulateFixedFrames(60);
    float brakeStartVerticalSpeed = brakingAircraft.VerticalSpeedMetersPerSecond;
    yield return SimulateFixedFrames(150);
    float brakeEndVerticalSpeed = brakingAircraft.VerticalSpeedMetersPerSecond;

    float idleVerticalDelta = idleEndVerticalSpeed - idleStartVerticalSpeed;
    float brakeVerticalDelta = brakeEndVerticalSpeed - brakeStartVerticalSpeed;
    Assert.That(brakeVerticalDelta, Is.LessThanOrEqualTo(idleVerticalDelta - 1.5f));
}
```

If `VerticalSpeedMetersPerSecond` is not exposed yet, add a read-only property to `ArcadeAircraftController` rather than peeking into `Rigidbody` from the test.

- [ ] **Step 4: Add landing approach stability proxy**

Add this helper near the other test metric helpers if the file does not already have a pitch metric:

```csharp
private static float SignedPitchDegrees(Transform target)
{
    return Mathf.DeltaAngle(0f, target.eulerAngles.x);
}
```

Then add a test that flies level with low throttle for `8` seconds near the runway and samples:

- no `Crashed` or `Submerged` state
- max absolute roll `<= 20` degrees
- roll sign flips no more than `3` times after ignoring tiny roll values under `3` degrees
- no pitch spike above `25` degrees
- vertical speed at the end remains above `-18 m/s`

- [ ] **Step 5: Run targeted aircraft tests and verify failures before implementation**

Run:

```bash
"$UNITY" -batchmode -nographics \
  -projectPath "$PROJECT" \
  -runTests -testPlatform PlayMode \
  -testFilter "MINgo.Tests.FlightPlayModeSmokeTests" \
  -testResults "Builds/TestResults/playmode-aircraft-movement-red.xml" \
  -logFile "/tmp/mingo-playmode-aircraft-movement-red.log"
```

Expected: at least one aircraft test fails because takeoff speed/distance, heading change, slowdown, descent assist, or approach stability is not yet tuned to the stricter thresholds.

- [ ] **Step 6: Commit the failing aircraft tests**

```bash
git add Assets/MINgo/Tests/PlayMode/FlightPlayModeSmokeTests.cs Assets/MINgo/Scripts/Flight/ArcadeAircraftController.cs
git commit -m "test: tighten aircraft movement thresholds"
```

Omit `ArcadeAircraftController.cs` if no read-only vertical-speed property was needed.

---

## Task 4: Tune Aircraft Movement To Pass The New Tests

**Files:**

- Modify: `Assets/MINgo/Scripts/Flight/ArcadeAircraftController.cs`
- Modify: `Assets/MINgo/Editor/FreeFlightSceneBuilder.cs`
- Modify: `Assets/Scenes/FreeFlightSandbox.unity`

- [ ] **Step 1: Add aircraft tuning fields and read-only metric if needed**

In `ArcadeAircraftController`, add these fields next to the existing flight force fields:

```csharp
public float takeoffLiftAssist = 16f;
public float slowdownDescentAcceleration = 3.2f;
public float slowdownPitchDamping = 0.55f;
```

Do not duplicate the existing `turnYawAssist` field. Tune its default from `0.45f` to `0.65f`.

If tests need it, expose:

```csharp
public float VerticalSpeedMetersPerSecond => body != null ? body.linearVelocity.y : 0f;
```

- [ ] **Step 2: Add bounded takeoff lift assist**

In `FixedUpdate()`, after `body.AddForce(liftForce, ForceMode.Force);`, add:

```csharp
if (Throttle01 > 0.85f && forwardSpeed > takeoffSpeed * 0.55f && AltitudeMeters < 12f)
{
    body.AddForce(Vector3.up * takeoffLiftAssist, ForceMode.Acceleration);
}
```

If the takeoff speed test exceeds `25 m/s`, reduce thrust or assist timing in scene defaults; do not weaken the speed range.

- [ ] **Step 3: Add stronger readable heading assist for banked turns**

Where assisted turn/yaw is already applied, tune it so a `3` second turn produces at least `20` degrees of heading change while roll remains `15` to `45` degrees. Prefer field tuning first:

```csharp
controller.assistedBankAngle = 28f;
controller.turnYawAssist = 0.65f;
```

Only change controller logic if the existing fields cannot express the target.

- [ ] **Step 4: Add slowdown descent assist**

In the existing slowdown/airbrake block, after the current airbrake force, add:

```csharp
body.AddForce(Vector3.down * slowdownDescentAcceleration, ForceMode.Acceleration);
Vector3 localAngularVelocity = transform.InverseTransformDirection(body.angularVelocity);
localAngularVelocity.x *= slowdownPitchDamping;
body.angularVelocity = transform.TransformDirection(localAngularVelocity);
```

- [ ] **Step 5: Tune scene defaults**

In `FreeFlightSceneBuilder.CreateAircraft()`, set:

```csharp
controller.takeoffLiftAssist = 16f;
controller.slowdownDescentAcceleration = 3.2f;
controller.slowdownPitchDamping = 0.55f;
controller.assistedBankAngle = 28f;
controller.turnYawAssist = 0.65f;
controller.throttleChangeRate = 3.2f;
controller.stabilization = 4.5f;
controller.autoLevel = 6f;
controller.autoLevelRotationRate = 2f;
```

- [ ] **Step 6: Regenerate the scene**

Run:

```bash
"$UNITY" -batchmode -nographics -quit \
  -projectPath "$PROJECT" \
  -executeMethod MINgo.EditorTools.FreeFlightSceneBuilder.RebuildScene \
  -logFile "/tmp/mingo-rebuild-scene-aircraft-movement.log"
```

Expected: Unity exits with code `0` and updates `Assets/Scenes/FreeFlightSandbox.unity`.

- [ ] **Step 7: Verify targeted aircraft tests pass**

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

- [ ] **Step 8: Commit aircraft implementation**

```bash
git add Assets/MINgo/Scripts/Flight/ArcadeAircraftController.cs Assets/MINgo/Editor/FreeFlightSceneBuilder.cs Assets/Scenes/FreeFlightSandbox.unity
git commit -m "fix: tune arcade aircraft handling"
```

---

## Task 5: Add Camera Follow Threshold Tests

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

    VehicleInputReader.SetInputOverrideForTests(new VehicleInputSnapshot(1f, 0.2f, false, false));
    yield return SimulateFixedFrames(160);

    AssertCameraDistanceToTarget(camera, car.transform, 6f, 12f);
    AssertCameraLooksTowardTarget(camera, car.transform);
    Vector3 carLocalCamera = car.transform.InverseTransformPoint(camera.transform.position);
    Assert.That(carLocalCamera.z, Is.LessThan(0f));

    VehicleInputReader.SetInputOverrideForTests(new VehicleInputSnapshot(0f, 1f, true, false));
    yield return SimulateFixedFrames(80);
    AssertCameraDistanceToTarget(camera, car.transform, 6f, 12f);
    AssertCameraLooksTowardTarget(camera, car.transform);
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

    FlightInputReader.SetInputOverrideForTests(new FlightInputSnapshot(0f, 0f, 0f, 0f, 1f, false));
    yield return SimulateFixedFrames(260);
    AssertCameraDistanceToTarget(camera, aircraft.transform, 9f, 16f);
    AssertCameraLooksTowardTarget(camera, aircraft.transform);

    FlightInputReader.SetInputOverrideForTests(new FlightInputSnapshot(0f, 0f, 0f, 1f, 0f, false));
    yield return SimulateFixedFrames(120);
    AssertCameraDistanceToTarget(camera, aircraft.transform, 8f, 18f);
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
cameraRig.followDistance = 10.5f;
cameraRig.followHeight = 2.8f;
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

## Task 6: Full Verification And Checkpoint

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

- Added numeric PlayMode thresholds for car forward launch, forward travel, coasting, braking, reverse, diagonal input, handbrake turning, grounded stability, and camera follow.
- Tuned `ArcadeCarController` for speed-based steering, direction-change braking, neutral coasting, and handbrake yaw while preserving grounded stability.
- Added numeric PlayMode thresholds for aircraft takeoff speed/distance, banked turning, auto-level recovery, slowdown, descent assist, landing approach, and camera follow.
- Tuned `ArcadeAircraftController` for reliable easy takeoff, readable turn response, slowdown descent control, and stable approach.
- Regenerated `Assets/Scenes/FreeFlightSandbox.unity` from `FreeFlightSceneBuilder`.

## Verification

- EditMode: `Builds/TestResults/editmode-vehicle-movement.xml`, result `Passed`, failed `0`.
- PlayMode: `Builds/TestResults/playmode-vehicle-movement.xml`, result `Passed`, failed `0`.
- macOS build: `/tmp/mingo-build-vehicle-movement.log`, `Build Finished, Result: Success.`

## Manual Test

- Press Play in `Assets/Scenes/FreeFlightSandbox.unity`.
- Aircraft: hold `W` to take off, hold turn input, release controls, hold `S` to slow and descend.
- Vehicle switch: press `F` or `Tab`.
- Car: `W` accelerates, releasing `W` slows, `S` brakes then reverses, `W+D` and `S+D` arc correctly, `Space+D` handbrake-turns without flipping.
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

- Car forward launch, forward travel, coasting, braking, reverse, diagonal input, handbrake, grounded stability, and camera are covered by Tasks 1, 2, and 5.
- Aircraft takeoff altitude, speed, runway distance, banked turn, auto-level, slowdown, descent, landing-stability proxy, and camera are covered by Tasks 3, 4, and 5.
- Scene defaults and generated scene updates are covered by Tasks 2 and 4.
- Full EditMode, PlayMode, build, checkpoint, diff hygiene, commit, push, and remote verification are covered by Task 6.
- Seoul world, `imagegen` city assets, and map expansion are intentionally excluded and must be handled by a separate spec after this plan is green.

Placeholder scan:

- The plan contains no unresolved placeholder implementation steps and no references to undefined files.

Type consistency:

- Test code uses existing `FlightInputSnapshot`, `VehicleInputSnapshot`, `ArcadeAircraftController`, `ArcadeCarController`, `PlayerVehicleSwitcher`, and `ChaseCameraRig` names.
- New fields and any new read-only metric properties are explicitly introduced before scene defaults or tests reference them.

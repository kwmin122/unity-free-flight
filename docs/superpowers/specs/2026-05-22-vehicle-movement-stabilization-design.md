# Vehicle Movement Stabilization Design

Date: 2026-05-22

## Goal

Make the current aircraft and car controls stable enough to support a larger Seoul simulator map. This spec is only about movement feel and measurable control behavior. Seoul world layout, generated textures, landmarks, and map expansion are a separate spec.

## Product Bar

The player must be able to test the world without fighting the controls. The car should feel like an arcade open-world vehicle: `W` goes forward immediately, releasing `W` slows the car, `S` reverses at low speed, diagonal key combinations work, and the camera remains behind the active vehicle. The aircraft should be easy to take off, turn, slow down, descend, and approach a landing path without the camera drifting into useless sky-only framing.

## Scope

In scope:

- Car acceleration, coasting, reverse, diagonal input, handbrake turning, grounded stability, and camera following.
- Aircraft takeoff timing, banked turning, auto-level recovery, slowdown/descent behavior, landing approach stability, and camera following.
- PlayMode tests that encode numeric movement thresholds.
- Scene default values that make the default `FreeFlightSandbox` playable immediately.

Out of scope:

- Seoul map generation.
- `imagegen` city, roof, facade, bridge, or river textures.
- New 3D vehicle meshes.
- Mission systems, scoring, police/combat behavior, and traffic AI.

## Current Baseline

- `863705a fix: make car keyboard driving responsive` fixed the known car input failure path.
- Existing PlayMode tests cover `W`, `W+D`, `S+D`, grounded car motion, aircraft throttle, aircraft turning recovery, and aircraft slowdown drag.
- The current tests are useful but not strict enough for a production movement bar. They do not fully lock coasting, handbrake rotation, takeoff time, landing approach stability, and camera distance.

## Car Acceptance Criteria

All criteria are measured in PlayMode inside `Assets/Scenes/FreeFlightSandbox.unity` using test input overrides, not manual observation.

| Behavior | Input | Required result |
| --- | --- | --- |
| Forward launch | Hold `W` for 2 seconds | speed is at least `6 m/s` |
| Forward travel | Hold `W` for 4 seconds | travel distance is at least `14m` from start |
| Neutral coasting | Release `W` for 3 seconds after launch | speed drops by at least `40%` from release speed |
| Reverse transition | Hold `S` from stopped or low speed for 2.5 seconds | backward travel is at least `3m` |
| Forward diagonal | Hold `W+D` for 3 seconds | speed is at least `6 m/s`, yaw changes at least `5 degrees`, grounded wheels at least `3` |
| Reverse diagonal | Hold `S+D` for 3.5 seconds | backward travel is at least `3m`, yaw changes at least `4 degrees`, grounded wheels at least `3` |
| Handbrake turn | Hold `Space + D` for 2 seconds while moving | yaw changes at least `18 degrees`, roll stays under `18 degrees`, grounded wheels at least `3` |
| Stability | During all car movement tests | car height remains under `2.5m`, roll remains under `18 degrees` |
| Camera follow | Active car camera after switching | target distance stays between `6m` and `12m`, camera is behind the car, camera does not enter the chassis |

## Aircraft Acceptance Criteria

All criteria are measured in PlayMode inside `Assets/Scenes/FreeFlightSandbox.unity` using test input overrides.

| Behavior | Input | Required result |
| --- | --- | --- |
| Takeoff | Hold `W` for up to 8 seconds from runway start | altitude is at least `8m`, state is not `Crashed` or `Submerged` |
| Minimum climb speed | Same takeoff test | speed is at least `14 m/s` by the time altitude reaches `8m` |
| Banked turn | After takeoff, hold right turn for 3 seconds | absolute roll is between `15` and `45 degrees`, heading changes at least `8 degrees` |
| Auto-level recovery | Release controls for 4 seconds after banked turn | absolute roll returns to `10 degrees` or less |
| Slowdown | Hold slowdown for 3 seconds after cruise throttle | speed is lower than idle coast by at least `10%` |
| Descent assist | Hold slowdown while airborne for 3 seconds | altitude drops or vertical velocity becomes more downward than idle coast |
| Landing approach stability | Fly toward runway with low throttle and level controls for 5 seconds | state remains not `Crashed`, roll stays under `25 degrees`, pitch does not diverge into a stall-like climb |
| Camera follow | Active aircraft camera during cruise and turn | target distance stays between `7m` and `11m`, aircraft remains in front of the camera, camera does not show sky-only framing for the whole sample |

## Implementation Direction

Keep the controllers arcade-first. This is not a full simulation pass.

Car:

- Keep WheelColliders for contact, suspension, braking, and wheel pose.
- Keep grounded traction assist for keyboard-friendly launch and reverse.
- Add or tune explicit neutral coasting drag so releasing `W` visibly slows the car.
- Add handbrake yaw assist only when the car is moving and grounded.
- Keep road paint and markings visual-only so the car is not blocked by hidden colliders.

Aircraft:

- Keep the current hold-to-throttle model.
- Tune takeoff assist, lift limits, and ground contact state so takeoff is reliable from the default runway.
- Tune turn assist so turn input creates readable bank but auto-level recovers predictably.
- Make slowdown input cut throttle, add airbrake drag, and add a mild descent assist while airborne.
- Keep the chase camera close enough to read the vehicle silhouette.

Camera:

- Do not create separate camera systems for this pass.
- Reuse `ChaseCameraRig`.
- Tests may inspect camera and target positions directly.

## Files Expected To Change

- `Assets/MINgo/Scripts/Vehicles/ArcadeCarController.cs`
- `Assets/MINgo/Scripts/Flight/ArcadeAircraftController.cs`
- `Assets/MINgo/Scripts/Flight/ChaseCameraRig.cs` only if camera tests prove the existing rig cannot satisfy the distance/framing criteria.
- `Assets/MINgo/Editor/FreeFlightSceneBuilder.cs`
- `Assets/MINgo/Tests/PlayMode/FlightPlayModeSmokeTests.cs`
- `Assets/MINgo/Tests/EditMode/ArcadeCarControllerTests.cs`
- `Assets/MINgo/Tests/EditMode/AircraftControllerTests.cs` or focused existing flight tests if pure helper behavior changes.
- `docs/superpowers/checkpoints/phase-23-vehicle-movement-stabilization.md`

## Verification

The movement pass is complete only when all of these pass from a clean temp project or the real Unity project:

```bash
"/Applications/Unity/Hub/Editor/6000.3.11f1/Unity.app/Contents/MacOS/Unity" \
  -batchmode -nographics \
  -projectPath "<project-or-temp-copy>" \
  -runTests -testPlatform EditMode \
  -testResults "<project-or-temp-copy>/Builds/TestResults/editmode-vehicle-movement.xml" \
  -logFile "/tmp/mingo-editmode-vehicle-movement.log"

"/Applications/Unity/Hub/Editor/6000.3.11f1/Unity.app/Contents/MacOS/Unity" \
  -batchmode -nographics \
  -projectPath "<project-or-temp-copy>" \
  -runTests -testPlatform PlayMode \
  -testResults "<project-or-temp-copy>/Builds/TestResults/playmode-vehicle-movement.xml" \
  -logFile "/tmp/mingo-playmode-vehicle-movement.log"

"/Applications/Unity/Hub/Editor/6000.3.11f1/Unity.app/Contents/MacOS/Unity" \
  -batchmode -nographics -quit \
  -projectPath "<project-or-temp-copy>" \
  -executeMethod MINgo.EditorTools.MINgoBuildPipeline.BuildMacOSPlayer \
  -logFile "/tmp/mingo-build-vehicle-movement.log"
```

Expected:

- EditMode result is `Passed`.
- PlayMode result is `Passed`.
- Build log contains `Build Finished, Result: Success.`
- Manual Play Mode smoke path works: aircraft takeoff, switch to car with `F` or `Tab`, drive with `W/S/A/D`, handbrake with `Space`, switch back to aircraft, and camera follows the active vehicle.

## Failure Policy

- If a numeric test fails, tune the controller or scene defaults rather than weakening the test.
- If a test is flaky because of collision with world dressing, fix the world collision contract.
- If a test cannot express a subjective complaint, add a measurable proxy before implementing.
- Do not start the Seoul world implementation until this movement pass has its tests and build green.

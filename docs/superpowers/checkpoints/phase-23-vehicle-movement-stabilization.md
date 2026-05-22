# Phase 23 Vehicle Movement Stabilization Checkpoint

Date: 2026-05-22

## Goal

Lock the car, aircraft, and chase camera against measurable GTA-style arcade movement criteria before expanding the Seoul simulator map.

## Root Causes

- The previous car tests were smoke tests. They proved the car could move, but did not prove GTA-like launch, diagonal steering, reverse, coasting, handbrake rotation, or grounded stability.
- WheelCollider-only torque made keyboard driving too weak and too dependent on subtle traction behavior. The controller needed explicit arcade assists for launch, reverse, direction-change braking, neutral coasting, steering yaw, handbrake yaw, and ground stick.
- Aircraft total speed was the wrong takeoff metric after adding lift assist. The acceptance criterion needs forward airspeed at the takeoff moment, not full Rigidbody velocity including vertical lift.
- Aircraft slowdown previously only reduced speed. It did not reliably create a measurably more downward descent path than idle cruise.
- Camera behavior had no PlayMode guard. The chase rig could regress into too-close, too-far, off-center, or sky-only framing without a test failing.

## Changes

- Strengthened PlayMode movement tests for:
  - car launch, 4-second travel, neutral coasting, braking, reverse, diagonal steering, handbrake yaw, grounding, and roll limits
  - aircraft takeoff time, forward takeoff speed, runway travel, banked turn, auto-level, slowdown, descent assist, landing approach, and camera framing
  - active car camera distance, behind-target position, and screen readability
  - active aircraft camera cruise distance, dynamic turn distance, screen readability, and sky-only framing prevention
- Tuned `ArcadeCarController`:
  - deterministic steering yaw assist for readable `W+D` and `S+D`
  - stronger reverse assist
  - lower handbrake yaw assist to stay inside the 35-75 degree band
- Tuned `ArcadeAircraftController`:
  - lower forward thrust for 18-25 m/s takeoff forward speed
  - low-altitude takeoff lift assist for 8-second arcade takeoff
  - stronger auto-level and slowdown descent assist
  - exposed `ForwardSpeedMetersPerSecond` for tests and HUD-ready instrumentation
- Regenerated `Assets/Scenes/FreeFlightSandbox.unity` from `FreeFlightSceneBuilder` through the open Unity editor MCP menu item.

## Verification

Temp project:

- `/tmp/MINgo-movement-final.Ypq6qB`

Results:

- EditMode: `Passed`, `129/129`
- PlayMode: `Passed`, `14/14`
- macOS build: `Build Finished, Result: Success.`

Additional movement-only run:

- `/tmp/MINgo-camera-redgreen.iQ5CEI`
- PlayMode: `Passed`, `14/14`

Final scene YAML load check after trimming Unity-generated trailing whitespace:

- `/tmp/MINgo-scene-yaml-final.egjTej`
- PlayMode: `Passed`, `14/14`
- `git diff --check`: clean

## Manual Smoke Path

Use the open Unity editor scene after accepting/reloading the externally modified scene:

1. Press Play.
2. Aircraft: hold `W` to take off within roughly 8 seconds.
3. Aircraft: use `A/D` to bank and turn; release controls to auto-level.
4. Aircraft: hold `S` to slow and descend.
5. Switch vehicle with `F` or `Tab`.
6. Car: hold `W`, `W+D`, `S`, `S+D`, and `Space + A/D`.
7. Confirm the camera remains behind the active vehicle instead of showing sky-only framing.

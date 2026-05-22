# Phase 22 Checkpoint - WheelCollider Car And Reference Map Rebuild

Date: 2026-05-22

## Goal

Fix the car feeling unstable or airborne, research Unity 3D vehicle implementation and vehicle design sources, generate a map concept first with imagegen, then rebuild the current scene toward that concept.

## Implemented

- Generated and imported `Assets/MINgo/Art/Concepts/world-map-concept-v1.png`.
- Generated and imported `Assets/MINgo/Art/Concepts/car-reference-sheet-v1.png`.
- Rebuilt `ArcadeCarController` around four WheelColliders instead of direct Rigidbody steering.
- Added a single chassis collider and removed primitive colliders from car visual parts.
- Added lowered center of mass, tuned suspension/friction, downforce, anti-roll force, wheel pose sync, and coasting brake torque.
- Expanded the scene map with airport ring roads, city connector roads, downtown road details, marina/lighthouse roads, canyon hairpins, and mountain guardrails.
- Added research notes with more than 10 vehicle simulation sources and more than 10 vehicle design/asset pipeline sources.

## Verification

- Rebuilt `Assets/Scenes/FreeFlightSandbox.unity` in a clean temp project because the real project was open in the Unity editor.
- Full EditMode: 126/126 passed, including 56/56 MCP Unity package tests.
- Full PlayMode: 4/4 passed.
- macOS player build: success, output generated at `/tmp/MINgo-vehicle-final3.uYlHb8/Builds/macOS/MINgo.app`.
- Build log contains Unity Cloud Diagnostics symbol-upload auth warning for missing `USYM_UPLOAD_AUTH_TOKEN`; the local player build still finished with `Result: Success`.

## Manual Test Notes

- In Unity, choose **Reload** if prompted that `Assets/Scenes/FreeFlightSandbox.unity` changed on disk.
- Press Play.
- `F` or `Tab`: switch between aircraft and car.
- Car controls: `W` accelerate, `S` brake then reverse, `A/D` steer, `Space` handbrake.
- The car should stay grounded on the airport road/parking area and should slow under neutral input instead of continuing to pull forward.

## Follow-up Fix

After manual testing, the car still felt like it did not move or reverse clearly. The root causes were:

- The controller divided configured motor torque by wheel count, while the Unity WheelCollider examples apply motor torque per driven wheel.
- Thin visual road paint and parking stall cubes still had colliders, creating hidden curbs directly in front of the spawn lane.
- A stopped player car could remain effectively asleep under straight throttle, so explicit player input wake-up is required.
- Pure WheelCollider torque still did not provide reliable keyboard-friendly straight-line launch from rest, so a grounded traction assist with wheel/raycast support detection is needed for arcade drive feel.
- The car spawned in the parking lot facing a short path toward airport structures, so straight `W` could quickly collide and look like it did not move.
- WheelCollider motor torque was unreliable for the current blockout car because axis/sign behavior fought the keyboard traction assist; reverse tests showed `S+D` could travel forward.

The follow-up fix disables WheelCollider motor torque for the blockout car, keeps WheelColliders for suspension/contact/braking/visual wheel pose, converts road/parking/runway paint to visual-only objects, wakes the player car Rigidbody on active input, adds grounded traction assist, moves the car spawn to the open airport ring road, and adds tests for `W+D`, `S+D`, forward-right movement, reverse-right movement, and non-blocking road markings.

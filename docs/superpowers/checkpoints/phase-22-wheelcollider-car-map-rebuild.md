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

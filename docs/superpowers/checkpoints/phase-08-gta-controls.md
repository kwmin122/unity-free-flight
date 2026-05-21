# Phase 08 - GTA-Like Easy Flight Controls

Date: 2026-05-21

Goal:
- Make the aircraft easier to fly with a GTA-like keyboard profile.
- `W/S` changes throttle, `A/D` requests a turn, `Up/Down` controls pitch, `Q/E` gives manual roll, and `Space` brakes on the ground.
- The camera should stay behind the aircraft without inheriting full aircraft pitch/roll, so the horizon and ground stay readable.

Implementation:
- Added `FlightControlAssist` as a pure control mixer.
- Added assisted turn banking, yaw assist, automatic wing leveling, and takeoff pitch assist.
- Updated `FlightInputReader` so WASD is no longer sim-style pitch/roll.
- Updated `ChaseCameraRig` to use a horizon-stabilized chase forward vector.
- Updated HUD state text with throttle percent and a compact control hint.

Verification:
- RED: `FlightControlAssistTests` initially failed because `FlightControlAssist`, `FlightControlOutput`, and the `turn` input constructor did not exist.
- GREEN targeted: `/tmp/MINgo-test-run/Builds/TestResults/editmode-gta-controls-green-targeted.xml` passed 6/6 for `FlightControlAssistTests` and `ChaseCameraRigTests`.
- RED camera contract: `editmode-camera-red.xml` failed because the saved scene still used `followDistance: 6.7`, which made the aircraft occupy too much of the view.
- GREEN targeted after camera tuning and roll-sign correction: `/tmp/MINgo-test-run/Builds/TestResults/editmode-gta-controls-green-targeted-3.xml` passed 7/7.
- Full unfiltered EditMode in temp copy: `/tmp/MINgo-test-run/Builds/TestResults/editmode-gta-controls-temp-unfiltered-3.xml` passed 99/99, including 56/56 MCP Unity package tests.
- macOS temp build: `/tmp/mingo-build-gta-controls-temp-3.log` ended with `Build Finished, Result: Success.`
- GUI smoke: `/tmp/mingo-gta-controls-smoke-focused-2.png` showed HUD, runway, horizon, terrain, and a readable third-person aircraft silhouette after camera tuning. The later roll-sign correction was covered by targeted tests and final build.

Notes:
- The RED/GREEN and final loops ran in `/tmp/MINgo-test-run` because the Unity editor had the original project open, which blocks batchmode on the same project path.
- Build log still reports non-blocking Unity Cloud Diagnostics symbol upload failure because `USYM_UPLOAD_AUTH_TOKEN` is not configured.

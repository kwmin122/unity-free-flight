# Phase 16 Checkpoint: Close Chase Camera

Date: 2026-05-21

## Scope

- Move the camera closer to the GTA-style reference where the aircraft is the main visual object, not a tiny silhouette.
- Keep the horizon readable by preserving the existing world-up chase camera behavior.
- Keep this as a camera tuning pass only; do not change flight physics or input behavior.

## Implementation

- Tightened the scene camera contract in `FreeFlightSceneContractTests.SceneContainsChaseCameraAndHud`.
- Updated `ChaseCameraRig` defaults:
  - `followDistance`: `13.5` -> `8`
  - `followHeight`: `3.2` -> `2.4`
  - `lookAhead`: `20` -> `14`
  - `lookHeight`: `0.4` -> `0.25`
  - `pitchFollow`: `0.28` -> `0.22`
  - `speedPullback`: `4` -> `2`
  - `smoothTime`: `0.1` -> `0.08`
  - `rotationSmooth`: `6` -> `8`
  - `minFieldOfView`: `60` -> `55`
  - `maxFieldOfView`: `72` -> `66`
- Updated `FreeFlightSceneBuilder` to write the same values.
- Regenerated `Assets/Scenes/FreeFlightSandbox.unity` from the scene builder in a temp Unity project to verify the builder values.
- Kept the final committed scene diff limited to the serialized camera values so unrelated Unity scene ordering/whitespace churn does not obscure the change.

## Verification

- RED scene contract: `/tmp/MINgo-camera-red/Builds/TestResults/editmode-camera-red.xml`
  - 1 total, 0 passed, 1 failed.
  - Expected failure: existing scene `followDistance` was outside the new `7..9` close-chase range.
- GREEN targeted scene contract: `/tmp/MINgo-camera-green/Builds/TestResults/editmode-camera-green.xml`
  - 1 total, 1 passed, 0 failed.
- Full EditMode: `/tmp/MINgo-camera-verify/Builds/TestResults/editmode-camera-unfiltered.xml`
  - 105 total, 105 passed, 0 failed.
  - MCP Unity package tests included: 56 total, 56 passed, 0 failed.
- Full PlayMode: `/tmp/MINgo-camera-verify/Builds/TestResults/playmode-camera-unfiltered.xml`
  - 3 total, 3 passed, 0 failed.
- macOS build: `/tmp/mingo-build-camera.log`
  - `Build Finished, Result: Success.`
  - Build artifact exists at `/tmp/MINgo-camera-verify/Builds/macOS/MINgo.app`.
  - Non-blocking Unity Cloud Diagnostics symbol-upload warning remains because `USYM_UPLOAD_AUTH_TOKEN` is not configured.

## Notes

- This makes the default view substantially closer and less pulled back at speed.
- Manual Unity Play Mode is still required to judge subjective comfort, but the saved scene now has enforceable close-chase defaults.

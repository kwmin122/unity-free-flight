# Phase 13 Checkpoint: PlayMode Flight Smoke

Date: 2026-05-21

## Scope

- Add the first PlayMode coverage for the actual scene aircraft physics loop.
- Verify that holding throttle in `FreeFlightSandbox` drives the runtime `ArcadeAircraftController` through `FixedUpdate`, not only pure helper functions.
- Keep the test narrow: throttle acceleration and non-crash/non-submerge state.

## Implementation

- Added `Assets/MINgo/Tests/PlayMode/MINgo.PlayMode.asmdef`.
- Added `FlightPlayModeSmokeTests.HoldingThrottleAcceleratesTheSceneAircraft`.
- Added `FlightInputReader.SetInputOverrideForTests(...)` and `FlightInputReader.ClearInputOverrideForTests()` so PlayMode tests can drive the same runtime path without depending on OS keyboard events.

## Verification

- RED PlayMode: `/tmp/mingo-playmode-flight-smoke-red.log`
  - Expected compile failure because `FlightInputReader.ClearInputOverrideForTests` and `FlightInputReader.SetInputOverrideForTests` did not exist.
- GREEN targeted PlayMode: `/tmp/MINgo-playmode-green/Builds/TestResults/playmode-flight-smoke-green.xml`
  - 1 total, 1 passed, 0 failed.
- Full PlayMode: `/tmp/MINgo-playmode-verify/Builds/TestResults/playmode-flight-smoke-unfiltered.xml`
  - 1 total, 1 passed, 0 failed.
- Full EditMode after PlayMode addition: `/tmp/MINgo-playmode-verify/Builds/TestResults/editmode-after-playmode-smoke.xml`
  - 105 total, 105 passed, 0 failed.
  - MCP Unity package tests included: 56 total, 56 passed, 0 failed.
- macOS build: `/tmp/mingo-build-playmode-smoke.log`
  - `Build Finished, Result: Success.`
  - Build artifact exists at `/tmp/MINgo-playmode-verify/Builds/macOS/MINgo.app`.
  - Non-blocking Unity Cloud Diagnostics symbol-upload warning remains because `USYM_UPLOAD_AUTH_TOKEN` is not configured.

## Notes

- This is not a full feel test. It proves the scene aircraft accelerates under held throttle in PlayMode and avoids immediate crash/submerge failure states.
- Manual Unity Play remains required for camera feel, turn comfort, and landing feel.

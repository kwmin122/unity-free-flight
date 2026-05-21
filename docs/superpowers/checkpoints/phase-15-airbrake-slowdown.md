# Phase 15 Checkpoint: In-Air Slowdown Assist

Date: 2026-05-21

## Scope

- Make `S` useful as a beginner landing-approach slowdown control after throttle is already at zero.
- Keep `Space` as the hard brake and avoid changing the existing easy-control input contract.
- Keep the implementation focused on airborne drag only, not a broader flight-model rewrite.

## Implementation

- Replaced the first weak slowdown PlayMode test with `FlightPlayModeSmokeTests.HoldingSlowdownInputAddsAirbrakeDragAfterThrottleCut`.
- The test compares two equivalent scene flights after throttle reaches zero:
  - no further slowdown input.
  - continued held slowdown input.
- Added `ArcadeAircraftController.airbrakeDrag`.
- When airborne and `ThrottleDelta` is negative, the controller now adds velocity-opposing, speed-squared airbrake force.

## Verification

- Initial weak RED attempt:
  - 1 total, 1 passed, 0 failed.
  - This showed the original test only proved throttle cut plus natural drag, not extra airbrake behavior.
  - The weak result was intentionally replaced by the stricter RED run below.
- STRICT RED: `/tmp/MINgo-airbrake-red/Builds/TestResults/playmode-airbrake-red.xml`
  - 1 total, 0 passed, 1 failed.
  - Expected failure: `idle=31.80, airbrake=31.80`, proving held `S` did not add any slowdown after throttle was already zero.
- GREEN targeted: `/tmp/MINgo-airbrake-green/Builds/TestResults/playmode-airbrake-green.xml`
  - 1 total, 1 passed, 0 failed.
- Full PlayMode: `/tmp/MINgo-airbrake-verify/Builds/TestResults/playmode-airbrake-unfiltered.xml`
  - 3 total, 3 passed, 0 failed.
- Full EditMode: `/tmp/MINgo-airbrake-verify/Builds/TestResults/editmode-airbrake-unfiltered.xml`
  - 105 total, 105 passed, 0 failed.
  - MCP Unity package tests included: 56 total, 56 passed, 0 failed.
- macOS build: `/tmp/mingo-build-airbrake.log`
  - `Build Finished, Result: Success.`
  - Build artifact exists at `/tmp/MINgo-airbrake-verify/Builds/macOS/MINgo.app`.
  - Non-blocking Unity Cloud Diagnostics symbol-upload warning remains because `USYM_UPLOAD_AUTH_TOKEN` is not configured.

## Notes

- This should make final approach less frustrating because the player can bleed speed while staying airborne.
- Manual Unity Play Mode still needs to judge whether the airbrake value feels too weak or too aggressive in hand control.

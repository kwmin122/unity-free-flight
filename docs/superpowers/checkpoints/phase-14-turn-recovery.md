# Phase 14 Checkpoint: PlayMode Turn Recovery

Date: 2026-05-21

## Scope

- Prove that `A/D` turn input works in the actual PlayMode scene, not only in pure assist tests.
- Prove that releasing controls recovers toward level flight instead of leaving the aircraft stuck in a bank.
- Keep the change focused on beginner-friendly recovery after a normal turn.

## Implementation

- Added `FlightPlayModeSmokeTests.TurnInputBanksThenReleaseRecoversTowardLevel`.
- The test accelerates the scene aircraft, applies turn input, verifies bank and heading change, releases controls, and verifies bank returns below 8 degrees.
- Updated `ArcadeAircraftController` so stabilization/auto-level uses released player input as the condition, not zero assisted-control output.

## Verification

- Initial weak test: `/tmp/MINgo-turn-red/Builds/TestResults/playmode-turn-recovery-red.xml`
  - 1 total, 1 passed, 0 failed.
  - This showed the first contract only proved partial recovery and was too weak.
- STRICT RED: `/tmp/MINgo-turn-strict/Builds/TestResults/playmode-turn-recovery-strict.xml`
  - 1 total, 0 passed, 1 failed.
  - Expected failure: released bank remained `12.7605772` degrees, above the 8-degree beginner recovery target.
- GREEN targeted: `/tmp/MINgo-turn-green/Builds/TestResults/playmode-turn-recovery-green.xml`
  - 1 total, 1 passed, 0 failed.
- Full PlayMode: `/tmp/MINgo-turn-verify/Builds/TestResults/playmode-turn-recovery-unfiltered.xml`
  - 2 total, 2 passed, 0 failed.
- Full EditMode: `/tmp/MINgo-turn-verify/Builds/TestResults/editmode-after-turn-recovery.xml`
  - 105 total, 105 passed, 0 failed.
  - MCP Unity package tests included: 56 total, 56 passed, 0 failed.
- macOS build: `/tmp/mingo-build-turn-recovery.log`
  - `Build Finished, Result: Success.`
  - Build artifact exists at `/tmp/MINgo-turn-verify/Builds/macOS/MINgo.app`.
  - Non-blocking Unity Cloud Diagnostics symbol-upload warning remains because `USYM_UPLOAD_AUTH_TOKEN` is not configured.

## Notes

- This improves keyboard comfort after a turn. It does not replace manual Unity Editor feel testing for camera framing, route readability, or landing difficulty.

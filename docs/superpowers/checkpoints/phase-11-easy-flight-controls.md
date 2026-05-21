# Phase 11 Checkpoint: Beginner-Friendly Flight Controls

Date: 2026-05-21

## Scope

- Make the default controls less simulator-like and more open-world/GTA-like.
- Keep `A/D` as assisted turn, not direct raw roll.
- Add a stall guard so low-speed pitch-up is softened and no-input low-speed flight nudges nose-down recovery.
- Separate slowdown from hard brake: `S` lowers throttle, `Space` is the hard brake.
- Hide `Q/E` from the beginner HUD hint while keeping it available as advanced manual roll.

## Implementation

- Added `FlightInputReader.CreateKeyboardSnapshot(...)` so keyboard mapping can be tested without live Unity keyboard state.
- Updated `FlightInputReader.ReadKeyboard()` to map `S`/`Left Ctrl` to throttle down only and `Space` to brake.
- Extended `FlightControlAssist.CalculateAssistedControls(...)` with low-speed stall guard parameters.
- Updated `FlightHud.FormatControlHint()` to remove `Q/E` from the primary beginner hint.
- Added edit-mode tests for input mapping and stall guard behavior.
- Added `docs/superpowers/plans/2026-05-21-easy-flight-controls-rework-plan.md`.

## Verification

- RED assist/HUD: `/tmp/MINgo-easy-controls-red/Builds/TestResults/editmode-easy-controls-red.xml`
  - 20 total, 17 passed, 3 failed.
  - Expected failures:
    - `CalculateAssistedControls_AddsNoseDownRecoveryNearStall`
    - `CalculateAssistedControls_LimitsPitchUpNearStall`
    - `FormatControlHint_DescribesEasyFlightControls`
- RED input mapping: `/tmp/mingo-editmode-input-red.log`
  - Expected compile failure because `FlightInputReader.CreateKeyboardSnapshot(...)` did not exist yet.
- GREEN targeted: `/tmp/MINgo-input-green/Builds/TestResults/editmode-easy-controls-targeted.xml`
  - 22 total, 22 passed, 0 failed.
- Full unfiltered EditMode: `/tmp/MINgo-easy-controls-verify/Builds/TestResults/editmode-easy-controls-unfiltered.xml`
  - 104 total, 104 passed, 0 failed.
  - MCP Unity package tests included: 56 total, 56 passed, 0 failed.
- macOS build: `/tmp/mingo-build-easy-controls.log`
  - `Build Finished, Result: Success.`
  - Build artifact exists at `/tmp/MINgo-easy-controls-verify/Builds/macOS/MINgo.app`.
  - Non-blocking Unity Cloud Diagnostics symbol-upload warning remains because `USYM_UPLOAD_AUTH_TOKEN` is not configured.

## Notes

- No scene regeneration was required for this slice because the behavior changes are runtime code and HUD formatter changes, not serialized scene defaults.
- The first build command used an outdated method name from the new easy-controls plan. The plan was corrected to use `MINgo.EditorTools.MINgoBuildPipeline.BuildMacOSPlayer`.

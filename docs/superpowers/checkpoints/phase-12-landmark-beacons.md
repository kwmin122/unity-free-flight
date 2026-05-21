# Phase 12 Checkpoint: Travel Landmark Beacons

Date: 2026-05-21

## Scope

- Add readable visual landmarks that make the one-map sandbox easier to navigate.
- Keep the slice as greybox world composition, not a mission system.
- Cover airport, coast, canyon, and ridge destinations so the player has visible reasons to choose a direction after takeoff.

## Implementation

- Added scene contract test `SceneContainsReadableTravelLandmarkBeacons`.
- Added `FreeFlightSceneBuilder.CreateLandmarkBeacons()`.
- Added four primary beacon objects:
  - `Airport Beacon Tower`
  - `Coastal Lighthouse`
  - `Canyon Gate Beacon`
  - `Ridge Summit Beacon`
- Regenerated `Assets/Scenes/FreeFlightSandbox.unity` from the builder and copied it back into the project.

## Verification

- RED targeted: `/tmp/MINgo-beacons-red/Builds/TestResults/editmode-landmark-beacons-red.xml`
  - 1 total, 0 passed, 1 failed.
  - Expected failure: `SceneContainsReadableTravelLandmarkBeacons` could not find `Airport Beacon Tower`.
- GREEN targeted: `/tmp/MINgo-beacons-green/Builds/TestResults/editmode-landmark-beacons-green.xml`
  - 1 total, 1 passed, 0 failed.
- Full unfiltered EditMode: `/tmp/MINgo-beacons-verify/Builds/TestResults/editmode-landmark-beacons-unfiltered.xml`
  - 105 total, 105 passed, 0 failed.
  - MCP Unity package tests included: 56 total, 56 passed, 0 failed.
- macOS build: `/tmp/mingo-build-landmark-beacons.log`
  - `Build Finished, Result: Success.`
  - Build artifact exists at `/tmp/MINgo-beacons-verify/Builds/macOS/MINgo.app`.
  - Non-blocking Unity Cloud Diagnostics symbol-upload warning remains because `USYM_UPLOAD_AUTH_TOKEN` is not configured.

## Notes

- The beacons are intentionally primitive shapes for MVP readability.
- They are meant to make visible terrain feel like destinations before adding mission or scoring systems.

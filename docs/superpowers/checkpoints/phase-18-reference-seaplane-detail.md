# Phase 18 Checkpoint: Reference Seaplane Detail

Date: 2026-05-21

## Scope

- Improve the playable aircraft silhouette toward the generated seaplane reference without importing an unverified external mesh.
- Keep the change local to the scene builder, scene contract, and saved scene.
- Do not alter flight physics, controls, camera tuning, or landing classification.

## Implementation

- Added `FreeFlightSceneContractTests.SceneContainsReferenceDrivenSeaplaneDetails`.
- Raised the main wing into a high-wing position.
- Added a central high-wing pylon.
- Added front/rear wing struts on both sides.
- Added propeller hub plus horizontal and vertical blade blocks.
- Split visible aircraft detail from physics:
  - high wing, struts, wing tips, and propeller are visual-only without colliders.
  - hidden wing physics colliders keep the previous stable flight inertia and collision profile.
- Regenerated `Assets/Scenes/FreeFlightSandbox.unity` from `FreeFlightSceneBuilder`.

## Verification

- RED targeted scene contract: `/tmp/MINgo-seaplane-detail-red/Builds/TestResults/editmode-seaplane-detail-red.xml`
  - 1 total, 0 passed, 1 failed.
  - Expected failure: old scene had `Wing.localPosition.y = 0.0`, below the high-wing requirement.
- GREEN targeted scene contract: `/tmp/MINgo-seaplane-detail-green/Builds/TestResults/editmode-seaplane-detail-green.xml`
  - 1 total, 1 passed, 0 failed.
- Visual-only RED scene contract: `/tmp/MINgo-seaplane-visual-only-red/Builds/TestResults/editmode-seaplane-visual-only-red.xml`
  - 1 total, 0 passed, 1 failed.
  - Expected failure: visual wing still had a `BoxCollider`.
- Physics-collider RED scene contract: `/tmp/MINgo-seaplane-physics-collider-red/Builds/TestResults/editmode-seaplane-physics-collider-red.xml`
  - 1 total, 0 passed, 1 failed.
  - Expected failure: hidden `Wing Physics Collider` was missing.
- Physics-collider GREEN scene contract: `/tmp/MINgo-seaplane-physics-collider-green/Builds/TestResults/editmode-seaplane-physics-collider-green.xml`
  - 1 total, 1 passed, 0 failed.
- Full EditMode: `/tmp/MINgo-seaplane-detail-verify3/Builds/TestResults/editmode-seaplane-detail-unfiltered.xml`
  - 107 total, 107 passed, 0 failed.
  - MCP Unity package tests included: 56 total, 56 passed, 0 failed.
- Full PlayMode: `/tmp/MINgo-seaplane-detail-verify3/Builds/TestResults/playmode-seaplane-detail-unfiltered.xml`
  - 3 total, 3 passed, 0 failed.
- macOS build: `/tmp/mingo-build-seaplane-detail.log`
  - `Build Finished, Result: Success.`
  - Build artifact exists at `/tmp/MINgo-seaplane-detail-verify3/Builds/macOS/MINgo.app`.
  - Non-blocking Unity Cloud Diagnostics symbol-upload warning remains because `USYM_UPLOAD_AUTH_TOKEN` is not configured.

## Notes

- This is still a blockout, not the final Meshy/Tripo/ComfyUI mesh.
- The goal is a more readable tail-camera silhouette while preserving the known-good arcade flight loop.
- A first PlayMode run failed after adding visual detail because visible primitive colliders changed aircraft recovery behavior. The fix separates visible model detail from hidden physics colliders so future mesh swaps do not silently change handling.

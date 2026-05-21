# Phase 17 Checkpoint: Imagegen World Art Pass

Date: 2026-05-21

## Scope

- Apply generated PNG art to the greybox world without replacing the working flight loop.
- Add a stronger seaplane reference sheet for the later generated 3D aircraft mesh.
- Document the Meshy / Tripo / ComfyUI -> Blender -> Unity prefab pipeline with explicit import and verification rules.

## Implementation

- Added `Assets/MINgo/Art/Textures/world-material-atlas-v1.png`.
- Added `Assets/MINgo/Art/Textures/world-material-atlas-v1.md`.
- Added `Assets/MINgo/Art/Concepts/seaplane-reference-sheet-v2.png`.
- Added `Assets/MINgo/Art/Concepts/seaplane-reference-sheet-v2.md`.
- Updated `FreeFlightSceneBuilder` so landing surfaces and landmark blocks use atlas tiles through `AssetDatabase.LoadAssetAtPath`.
- Regenerated `Assets/Scenes/FreeFlightSandbox.unity` from the scene builder so the default scene is not visually empty greybox-only.
- Added `FreeFlightSceneContractTests.SceneAppliesGeneratedWorldMaterialAtlas`.
- Added `docs/asset-pipeline/ai-airplane-and-world-art.md`.

## Verification

- RED atlas scene contract: `/tmp/MINgo-art-red/Builds/TestResults/editmode-art-atlas-red.xml`
  - 1 total, 0 passed, 1 failed.
  - Expected failure: the old scene material did not reference `world-material-atlas-v1.png`.
- GREEN targeted atlas scene contract: `/tmp/MINgo-art-green/Builds/TestResults/editmode-art-atlas-green.xml`
  - 1 total, 1 passed, 0 failed.
- Post-whitespace-cleanup targeted atlas contract: `/tmp/MINgo-art-whitespace/Builds/TestResults/editmode-art-atlas-after-whitespace.xml`
  - 1 total, 1 passed, 0 failed.
- Full EditMode: `/tmp/MINgo-art-verify/Builds/TestResults/editmode-art-unfiltered.xml`
  - 106 total, 106 passed, 0 failed.
  - MCP Unity package tests included: 56 total, 56 passed, 0 failed.
- Full PlayMode: `/tmp/MINgo-art-verify/Builds/TestResults/playmode-art-unfiltered.xml`
  - 3 total, 3 passed, 0 failed.
- macOS build: `/tmp/mingo-build-art-quit.log`
  - `Build Finished, Result: Success.`
  - Build artifact exists at `/tmp/MINgo-art-build-quit/Builds/macOS/MINgo.app`.
  - Non-blocking Unity Cloud Diagnostics symbol-upload warning remains because `USYM_UPLOAD_AUTH_TOKEN` is not configured.

## Notes

- This pass intentionally uses original generated art and does not copy GTA branding, exact vehicle markings, logos, or UI.
- External 3D generation is documented as a next pipeline step. The repository does not claim Meshy, Tripo, or ComfyUI produced a finished mesh until an authenticated export is actually imported and verified.

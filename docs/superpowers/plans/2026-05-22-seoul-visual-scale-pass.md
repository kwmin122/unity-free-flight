# Seoul Visual Scale Pass Implementation Plan

> **For agentic workers:** follow Superpowers flow. This plan uses test-first scene contracts, then generated-scene implementation, then real Unity scene regeneration.

**Goal:** expand and texture the Seoul slice so it reads as a larger flight/driving city instead of a few isolated blocks.

**Architecture:** keep `FreeFlightSceneBuilder` as the source of truth. Add generated atlas creation, separate side/roof building meshes, and denser district extension helpers. Keep gameplay controllers unchanged unless tests reveal regressions.

## Files

- Modify: `Assets/MINgo/Tests/EditMode/FreeFlightSceneContractTests.cs`
- Modify: `Assets/MINgo/Editor/FreeFlightSceneBuilder.cs`
- Modify: `Assets/Scenes/FreeFlightSandbox.unity`
- Create: `Assets/MINgo/Art/Textures/seoul-generated-material-atlas-v1.png`
- Create: `Assets/MINgo/Art/Textures/seoul-generated-material-atlas-v1.md`
- Create: `docs/superpowers/checkpoints/phase-25-seoul-visual-scale-pass.md`

## Task 1: Add Failing Contracts

- [ ] Add tests for expanded map ground scale and camera far clip.
- [ ] Add tests for Seoul generated atlas existence and size.
- [ ] Add tests that representative Seoul buildings have distinct side and roof materials.
- [ ] Add tests for longer Hangang span and new west/east boundary landmarks.
- [ ] Add stricter Seoul density minimums.
- [ ] Run targeted EditMode test and confirm RED before implementation.

## Task 2: Implement Generated Seoul Visual Layer

- [ ] Generate a deterministic Seoul atlas PNG from editor code before scene creation.
- [ ] Add Seoul atlas material helper with tile offsets.
- [ ] Add custom cube mesh helper with side and roof submeshes.
- [ ] Route Seoul glass towers and apartment slabs through the side/roof helper.
- [ ] Expand ground size, camera far clip, Hangang river width, river roads, and park corridors.
- [ ] Add denser district blocks and west/east landmarks.

## Task 3: Regenerate and Verify

- [ ] Run builder in a temp copy and pass targeted EditMode.
- [ ] Run full EditMode, full PlayMode, and macOS build in a temp copy.
- [ ] Recompile and rebuild the real open Unity scene through MCP.
- [ ] Verify the committed scene without rerunning the builder.
- [ ] Commit and push.


# Seoul Visual Scale Pass Design

Date: 2026-05-22

## Goal

Push the current Seoul slice from a small recognizable blockout toward a larger flight simulator city: bigger map footprint, denser city fabric, and generated Seoul-specific textures that make roofs, facades, roads, river, and park surfaces read differently from the air.

## Product Direction

This is still a generated Unity vertical slice, not a finished GIS import. The player should feel that the map continues beyond the first river corridor, and that buildings are not identical grey cubes. From aircraft height, Seoul needs three things:

- a long Hangang corridor with bridge rhythm and river parks;
- district density around Yeouido, Jongno/Namsan, Gangnam, and Jamsil;
- roof/facade variation, because pilots see rooftops as much as street sides.

## In Scope

- Expand `Flight Reference Ground` and camera visibility so the world is not boxed into the previous small play area.
- Generate a deterministic Seoul material atlas PNG inside the Unity project.
- Use the generated atlas for new Seoul facades, rooftops, road surfaces, river water, parks, and landmark metal.
- Build Seoul-specific cube meshes with separate side and roof material slots.
- Increase district density enough that flying between landmarks feels like crossing city fabric.
- Add map boundary landmarks so west/east travel has visible targets.
- Add EditMode scene contract tests for the new visual scale guarantees.

## Out of Scope

- Exact OSM/GIS Seoul reconstruction.
- Paid heavy asset packs.
- Runtime traffic AI.
- Real GTA assets or copyrighted map extraction.
- Meshy/Tripo/Blender model import. That remains a later aircraft/vehicle art pipeline pass.

## Acceptance Criteria

Measured after regenerating `Assets/Scenes/FreeFlightSandbox.unity`:

- `Flight Reference Ground` is at least 4200m x 4200m in scene scale.
- Camera far clip is large enough for the expanded map.
- `Assets/MINgo/Art/Textures/seoul-generated-material-atlas-v1.png` exists and is at least 1024x1024.
- Representative Seoul buildings use at least two different materials: side facade and roof.
- Hangang west/east river surfaces span a long east-west corridor.
- Scene contains visible west/east Seoul boundary landmarks.
- Seoul object count increases substantially, with higher minimums for Gangnam, Yeouido, Jamsil, and Jongno.

## Implementation Notes

Do the art pass in code first because it is deterministic, testable, and survives scene regeneration. The generated atlas can later be replaced by hand-picked imagegen/ComfyUI assets without changing the scene contracts.


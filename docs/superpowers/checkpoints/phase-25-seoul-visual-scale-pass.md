# Phase 25 Seoul Visual Scale Pass Checkpoint

Date: 2026-05-22

## Goal

Make the Seoul map feel larger and less blockout-like while keeping the movement slice stable.

## Root Causes Addressed

- The previous Seoul slice was spatially recognizable, but the map footprint still felt small for free flight.
- Buildings used mostly single-material cube blocks, so rooftops and facades did not read differently from the air.
- Art quality was not regeneration-safe. Manually placed or ad-hoc assets would be easy to lose when rebuilding the Unity scene.
- A PlayMode regression exposed that the car handbrake turn still relied too much on WheelCollider torque and could undershoot the GTA-style yaw band.

## Changes

- Added a deterministic Seoul material atlas:
  - `Assets/MINgo/Art/Textures/seoul-generated-material-atlas-v1.png`
  - glass facade, concrete facade, apartment facade, rooftop equipment, road asphalt, Hangang water, park grass, landmark metal, palace roof.
- Expanded the generated map:
  - `Flight Reference Ground` is now 5200m x 5200m.
  - Camera far clip is now 6200m.
  - Hangang corridor spans over 2600m east-west.
  - Added west/east map boundary landmarks and extended riverside roads.
- Added custom Seoul building meshes with distinct side and roof material submeshes.
- Extended the roof/facade contract to named Seoul landmarks: `N Seoul Tower`, `Jamsil Lotte World Tower`, and `Seoul Jongno Palace Gate`.
- Added a shared mesh asset so every side/roof building reuses the same roof-split cube mesh instead of embedding unique meshes in the scene.
- Increased district fabric around Gangnam, Yeouido, Jamsil, and Jongno.
- Tuned car handbrake control:
  - restored torque assist to the stable value;
  - added a small direct yaw rotation only while handbrake is held, so the yaw band is deterministic without affecting normal driving.
- Regenerated `Assets/Scenes/FreeFlightSandbox.unity` through Unity MCP.

## Verification

Final verification used a temp copy of the current on-disk project without rerunning the builder.

- EditMode: 138/138 passed.
- PlayMode: 14/14 passed.
- macOS player build: exit 0.

Temp project: `/tmp/MINgo-seoul-visual-final4.BrfqJl`

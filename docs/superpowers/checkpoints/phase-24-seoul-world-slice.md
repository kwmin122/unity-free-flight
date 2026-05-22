# Phase 24 Seoul World Slice Checkpoint

Date: 2026-05-22

## Goal

Add a Seoul-inspired first world slice after the movement pass, centered on Hangang River navigation, district readability, and driveable river crossings.

## Source References Used

- Seoul Hangang overview: https://world.seoul.go.kr/service/amusement/hangang/overview/
- Hangang Parks overview: https://english.seoul.go.kr/service/amusement/hangang/hangang-parks/
- Accessible Hangang river destinations including Banpo/Jamsugyo, Sevitseom, Nodeul Island, and Yeouido Saetgang: https://english.seoul.go.kr/the-great-hangang-river/accessible-hangang-river/
- N Seoul Tower official overview: https://www.nseoultower.co.kr/eng/global/intro.asp
- Visit Seoul Seoul Sky page: https://visit.seoul.kr/en/places/seoul-sky
- Seoul tourist guidebook with Lotte World Tower/Jamsil context: https://world.seoul.go.kr/wp-content/uploads/2025/01/2025-Tourist-guidebookENG.pdf

## Root Cause

The previous map had airport, coast, city, mountain, and canyon play zones, but it did not read as Seoul. It also had too few visual anchors for long flight and car exploration. The player could test controls, but the world still felt like a generic greybox.

## What Changed

- Added `CreateSeoulWorldSlice()` to the scene builder.
- Added a central Hangang River axis:
  - `Hangang River West`
  - `Hangang River East`
  - north and south riverside parks
  - `Gangbyeonbuk-ro Riverside Road`
  - `Olympic-daero Riverside Road`
- Added driveable river crossings:
  - `Mapo Bridge Road`
  - `Banpo Bridge Road`
  - `Dongjak Bridge Road`
  - `Jamsil Bridge Road`
- Added landmark/district clusters:
  - Yeouido island, park, 63 Finance Tower, finance towers, National Assembly-like dome
  - Banpo/Nodeul/Sevitseom-inspired river zone
  - Namsan ridge and `N Seoul Tower`
  - Jongno/Gwanghwamun civic axis
  - Gangnam boulevard / Teheran-ro / COEX-like podium
  - Jamsil lake loop, stadium, and `Jamsil Lotte World Tower`
- Added reusable procedural detailing helpers:
  - Seoul road with lane markings
  - Seoul bridge with supports/guardrails
  - glass tower with window bands and rooftop details
  - apartment slab with balcony strips
  - tree rows for Hangang park edges
- Regenerated `Assets/Scenes/FreeFlightSandbox.unity` after forcing Unity script recompilation.

## Tests Added

EditMode tests in `FreeFlightSceneContractTests` now verify:

- required Seoul landmark objects exist
- water surfaces are trigger water surfaces
- bridge and riverside roads are playable road surfaces
- district density counts for Seoul/Gangnam/Yeouido/Jamsil/Jongno
- Jamsil and Namsan landmarks have readable scale

## Verification

RED run:

- `/tmp/MINgo-seoul-red.MxXPWr`
- `FreeFlightSceneContractTests`: `14 passed / 4 failed`
- Failure reason: Seoul objects did not exist yet.

Builder GREEN run:

- `/tmp/MINgo-seoul-green.dvIr3x`
- `FreeFlightSceneContractTests`: `18/18 Passed`

Full generated-builder verification:

- `/tmp/MINgo-seoul-final2.jx7yMj`
- EditMode: `133/133 Passed`
- PlayMode: `14/14 Passed`
- macOS build: `Build Finished, Result: Success.`

Final committed-scene verification:

- `/tmp/MINgo-seoul-scene-final.YJafD1`
- EditMode: `133/133 Passed`
- PlayMode: `14/14 Passed`
- macOS build: `Build Finished, Result: Success.`
- `git diff --check`: clean

## Operational Note

The first MCP scene rebuild after editing `FreeFlightSceneBuilder.cs` used the stale editor assembly. Root cause was running the menu item before Unity completed domain reload. The durable sequence is:

1. Send `recompile_scripts`.
2. Wait for zero-error response.
3. Send `MINgo/Rebuild Free Flight Sandbox Scene`.
4. Trim Unity YAML trailing whitespace.
5. Verify a temp copy without rerunning the builder, so the committed scene itself is tested.

## Manual Smoke Path

1. Reload the modified scene in Unity if prompted.
2. Press Play.
3. Fly east-west along Hangang River.
4. Use N Seoul Tower and Jamsil Lotte World Tower as skyline anchors.
5. Switch to the car with `F` or `Tab`.
6. Drive along the riverside roads and across Banpo/Mapo/Jamsil bridge roads.
7. Confirm camera and movement remain stable after the Seoul world density increase.

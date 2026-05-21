# Free Flight & Landing Sandbox Design

Date: 2026-05-21
Project: MINgo
Target: Unity 6000.3.11f1, PC build

## Summary

Build a GTA-inspired, simplified-realistic 3D free-flight and landing sandbox. The player flies a plane across one coastal mainland map with ocean, city edge, airport, roads, mountains, canyon landmarks, and a risky restricted airspace zone. There are no required missions in the MVP. The core loop is to take off, free fly, explore, treat visible terrain as landing challenges, land on any suitable ground, and take off again.

This game is a free-flight toy where visible terrain should feel like an invitation to try landing there.

## Goals

- Create a playable PC build, not only an Editor demo.
- Make flying feel immediately controllable, similar to arcade open-world aircraft handling.
- Provide a single broad map that feels travel-worthy without using world streaming.
- Make terrain itself create self-directed challenges: road landing, ridge landing, canyon floor landing, short takeoff, and emergency landing.
- Allow landing anywhere on land, with success determined by speed, attitude, vertical impact, and ground slope.
- Add one restricted airspace hazard that is optional for the player to enter and creates tension without turning the game into combat.
- Use AI-generated visual assets only where they help atmosphere and iteration speed.

## Non-Goals

- Multiplayer.
- Player weapons, dogfighting, base destruction, or combat progression.
- Walking, vehicle exit, or character controller gameplay.
- Traffic, pedestrians, or city simulation.
- World streaming.
- Realistic aircraft procedures, flaps, landing gear systems, or cockpit instrumentation.
- Water landing.
- Detailed destruction physics.
- A large set of production-grade assets in the first build.

## Game Shape

The game is a free-flight sandbox. The player starts at or near an airport, takes off, explores the coast, city edge, mountain ridge, and canyon, then lands wherever they choose. The player can take off again from suitable ground if enough speed and space are available.

The MVP intentionally avoids mandatory missions. Landmarks exist to create orientation, scale, and curiosity rather than explicit quest objectives.

The actual play motivation comes from self-assigned challenges. The map should make the player think: "Can I land on that road?", "Can I skim through that canyon?", "Can I touch down on that ridge?", or "Can I survive an emergency landing after entering the wrong airspace?"

## First Five Minutes

The intended first-session emotional curve is:

1. The player takes off quickly from the airport.
2. The ocean, city edge, and mountain/canyon region become visible and give the player clear directions to choose from.
3. Low flight near roads, beaches, fields, and ridges makes the player want to try landing somewhere other than the runway.
4. The landing result feels understandable: clean, rough, crash, submerged, or damaged.
5. The player sees another visible location and thinks, "Next time I should try landing there."

## World Design

The map is one coastal mainland region.

Core zones:

- Airport: start area, runway, hangar, control tower, and open flat ground.
- City edge: simplified low-rise buildings, roads, harbor-like structures, and recognizable silhouettes.
- Coastline: ocean boundary, beach, coastal road, and open flat areas for low flight and landing.
- Inland fields: open ground where arbitrary landing can be tested.
- Mountain ridge: altitude reference and visual boundary.
- Canyon route: a natural low-flying route through inland terrain.
- Restricted airspace: a military radar base or no-fly zone on one side of the mountain region.

The world is a single scene for the MVP. The ocean and mountain/canyon edges are used as natural boundaries so the map feels large without requiring streaming.

## Terrain As Landing Challenges

The map is not only scenery. Each region should create a different kind of landing or flight temptation:

- Airport: standard takeoff and landing, safe speed buildup, and orientation.
- Coastal road: low skimming, road landing attempts, and long flat recovery runs.
- City edge: tense silhouette flying near buildings without requiring dense traffic or pedestrians.
- Mountain ridge: altitude temptation, risky ridge-top touchdown, and steep-slope failure cases.
- Canyon: natural low-flight line and informal time-attack route without mandatory checkpoints.
- Inland fields: proof that arbitrary landings are allowed and recoverable.
- Beach and coastline: tempting flat space near water, with water contact causing submerged failure.

The MVP does not need a score system, but it must classify the core landing contexts that are already detectable from surface and terrain data: runway landing, road landing, field landing, ridge landing, canyon floor landing, rough landing, short takeoff, and emergency landing.

## Restricted Airspace

The mountain side of the map includes a restricted airspace zone that the player can optionally enter. This is an environmental hazard, not a combat system.

Behavior:

- A small military radar base marks the danger area.
- Entering the outer zone shows a restricted airspace warning on the HUD.
- Deeper entry starts a lock-on warning with sound and visual indicators.
- Remaining inside the danger zone long enough launches a missile.
- The missile has a visible smoke trail and enough delay to evade.
- The player can break lock or escape by using terrain: mountain ridges, canyon turns, low flight, or leaving the zone.
- The first missile hit puts the aircraft into Damaged state, creating an emergency landing opportunity. Crash is reserved for subsequent severe impacts, ground collisions, or water failure.

Rules:

- The player does not get weapons in the MVP.
- The military base cannot be destroyed in the MVP.
- The hazard exists to create forbidden-space curiosity, evasion, damage recovery, and emergency landing moments.

## Visual Direction

The visual style is simplified realism inspired by open-world driving/flying games. It should feel grounded and readable, but not chase photorealism.

Important art principles:

- Clear silhouettes over dense detail.
- Consistent material palette across AI, generated, and hand-built assets.
- Strong sky, haze, ocean, and lighting to sell scale.
- Low-to-mid complexity landmark assets.
- Avoid clutter that hurts flight readability.

## Asset Pipeline

Assets are split into three categories.

### Directly Built In Unity

- Terrain, ocean, runway, roads, and basic landforms.
- Flight and landing colliders.
- Aircraft control object and physics setup.
- Camera, lighting, fog, sky, build settings, and core prefabs.

### Image Generation

Use image generation for bitmap and reference assets:

- World concept image.
- Airport, city edge, coastline, and canyon mood boards.
- Decals such as runway numbers, warning signs, road markings, and airport signage.
- Texture references for asphalt, concrete, hangar doors, building walls, and terrain surfaces.
- Reference images for AI 3D generation.

Generated images used by the project must be copied into the workspace and tracked as project assets. Preview-only images may remain outside the Unity asset tree.

### AI 3D Generation

Use AI 3D generation for atmosphere and landmarks:

- Control tower.
- Hangars.
- Small low-rise buildings.
- Radio towers.
- Fuel tanks.
- Lighthouse or coastal beacon.
- Harbor structures.
- Canyon rocks and landmark props.

Do not use AI 3D generation for:

- The full terrain.
- The full city.
- The flight physics object or collision-critical aircraft model.
- Any asset whose collider, pivot, topology, or scale must be exact.

Every imported AI 3D asset must pass these checks:

- Unity scale is correct.
- Pivot is usable for placement.
- Collider can be simplified.
- Polycount is acceptable for PC build.
- Materials match the project style.
- License and generation source are recorded.

## Flight Controls

The flight model should feel GTA-like rather than like a strict simulator.

Inputs:

- Throttle up and down.
- Pitch up and down.
- Roll left and right.
- Yaw left and right.
- Ground brake or reverse thrust as a simple ground handling aid.

Behavior:

- Third-person chase camera.
- Camera slightly pulls back or smooths at speed.
- Mild auto-stabilization.
- Low-speed handling becomes weaker and the plane loses altitude, but the stall model stays readable and game-like.
- Takeoff is possible from the runway and from sufficiently long, flat ground.

## Landing Model

Landing is not restricted to predefined zones. Any land surface can be touched.

Landing evaluation uses:

- Vertical impact speed.
- Forward speed.
- Aircraft pitch and roll angle.
- Ground slope.
- Contact duration and stability.

Outcomes:

- Clean landing: stable touchdown on suitable ground.
- Rough landing: aircraft survives but enters damaged state or reduced handling.
- Crash: excessive impact, high angle, or unstable collision.
- Submerged: water contact causes failure state.

Runway, roads, and flat fields are naturally safer because their slope and surface are better. Steep hills and rough terrain are possible to touch but more dangerous.

## Player States

- Flying: airborne and under flight control.
- Grounded: wheels or body are in contact with land but landing quality is not yet resolved.
- Landed: aircraft has settled safely and can taxi or take off again.
- Damaged: aircraft survived contact but has reduced handling or needs restart/recovery.
- Submerged: aircraft touched water and entered failure state.
- Crashed: aircraft impact exceeded safe limits.

State transitions should be visible through simple HUD text and aircraft behavior.

## HUD

The MVP HUD should stay minimal:

- Speed.
- Altitude.
- Aircraft state.
- Context label for notable landings or danger states.
- Restricted airspace and lock-on warning when relevant.

Optional debug-only readouts may show landing quality, vertical speed, and surface slope during development.

## Build Target

The first build target is PC desktop. The primary local target is macOS because the current Unity install is on macOS. Windows build can be added later if the installed Unity modules support it.

## Testing And Verification

Required checks:

- Unity scene contains aircraft, camera, terrain, ocean, airport, city edge, mountain ridge, and canyon route.
- Aircraft can take off from the runway.
- Aircraft can free fly for at least one minute in a built player.
- Aircraft can land on runway, road, and open field.
- Aircraft gets damaged or crashes on unsafe impact.
- Aircraft enters submerged/failure state on water contact.
- Aircraft can take off again after a clean landing.
- HUD can label at least runway, road, field, ridge, canyon, submerged, and restricted airspace contexts.
- Restricted airspace warning, lock-on, missile launch, evasion, and damaged/emergency-landing flow work in the built player.
- PC build completes successfully.

Manual test surfaces:

- Runway.
- Road.
- Field.
- Sloped terrain.
- Canyon floor or ridge plateau.
- Water.
- Restricted airspace approach and escape path.

## MVP Boundary

The first version is successful if it proves the full loop:

1. Launch PC build.
2. Take off.
3. Fly over a visually distinct coastal mainland map.
4. Explore city edge, coast, mountains, and canyon.
5. Land on arbitrary land.
6. Recover or fail based on physical landing quality.
7. See a context label that makes the landing feel recognized.
8. Take off again after a clean landing.
9. Optionally enter restricted airspace, evade danger, and attempt emergency landing if damaged.

Anything that does not support this loop is deferred.

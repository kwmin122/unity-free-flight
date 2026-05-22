# A-to-B Playable Seoul Rebuild Design

Date: 2026-05-22

## Decision

The rebuild order is `A -> B`.

- `A`: fix the real control and takeoff problems first.
- `B`: after the control gate is credible, rebuild the Seoul world with stronger assets, roads, materials, textures, sprites, cubemaps, and 3D models.

Audio uses user-provided MP3 files. The game must map clips by filename and role. Do not depend on generated placeholder sounds as final audio.

## Product Bar

The first five minutes must feel like a playable Seoul open-world slice:

1. Start near a credible Seoul road/airport edge.
2. Drive a supercar forward, reverse, and diagonally without fighting controls.
3. Switch to aircraft.
4. Taxi, accelerate on the runway, and lift off gradually instead of popping upward.
5. Fly along the Hangang corridor toward Yeouido, Namsan, Gangnam, and Jamsil.
6. Hear distinct park/default music, car engine, aircraft takeoff, and aircraft cruise audio when the user-provided MP3 files are present.

This is not a generic blockout pass. It is a control-first playable Seoul slice.

## Current Root-Cause Findings

### Car

Current evidence:

- `ArcadeCarController` models reverse as `Braking` when moving forward and delays reverse through `reverseEngageDelay = 2f`.
- Reverse steer is reduced with `reverseSteerScale = 0.65f`.
- Steering yaw assist depends on existing speed with `Mathf.Abs(forwardSpeed) / 8f`, so from a stop or near-stop reverse diagonal steering feels late.
- Wheel motor torque is multiplied by `wheelMotorTorqueScale`, while most actual motion comes from custom force assist.

Root cause:

The car is not a clean GTA-style keyboard controller. It is a hybrid WheelCollider + force-assist controller with a delayed reverse state. That can pass simple movement tests but still feel wrong manually, especially for `S`, `S+A`, and `S+D`.

### Aircraft

Current evidence:

- `ArcadeAircraftController` applies strong upward acceleration through `takeoffLiftAssist = 130f` once throttle and speed thresholds are reached.
- `ResolveMotionState` returns `Flying` when speed reaches `takeoffSpeed`, even if ground contact still exists.
- There is no explicit ground taxi reverse behavior. In the air, `S` reduces throttle and applies airbrake/descent; on ground, it does not behave like a slow reverse taxi.

Root cause:

The aircraft takeoff is not being earned by runway roll, rotation, and lift buildup. It is helped by a strong vertical boost and an early state transition. That creates the user-visible "suddenly flies by itself" problem.

### Audio

Current evidence:

- `ProceduralFlightAudio` creates a sine-based engine loop at startup.
- The same procedural engine layer is used for aircraft engine character.
- There is no separate car engine controller or clip role mapping.

Root cause:

The sound system is placeholder synthesis, not vehicle-specific audio. It cannot convincingly distinguish park/default music, supercar engine, aircraft takeoff, and aircraft cruise.

### Seoul World

Current evidence:

- The scene has Hangang, Yeouido, Namsan, Gangnam, Jamsil, bridges, and districts.
- The world is still mostly procedural blocks and atlas-tinted cubes.
- The city exists by object names and test counts, but the player's front-facing route and road-level detail are weak.

Root cause:

The current map is a tested Seoul-inspired blockout, not a visually convincing Seoul simulator slice. It needs a stronger macro layout and asset layer, not just more cubes.

## A: Control Rebuild Requirements

### Car Controls

Target feel: GTA-like arcade keyboard driving.

Required behavior:

- `W`: immediate forward acceleration.
- `W+A` / `W+D`: forward diagonal movement and turning at the same time.
- `S` from stop: reverse starts within `0.35s`.
- `S+A` / `S+D` from stop: reverse diagonal movement and turning at the same time.
- `S` while moving forward: brake first, then enter reverse without a long dead delay.
- Releasing `W`: car slows naturally but does not instantly stop.
- Speed cap: readable Seoul city driving, not uncontrollable racing speed.
- Grass/park/road-edge transitions: no launching, bouncing, or violent roll.
- Handbrake: sharp but stable turn, no flipping.

Implementation direction:

- Keep WheelColliders for contact and suspension.
- Split the state machine into explicit `Forward`, `BrakeToStop`, `Reverse`, and `Coast`.
- Remove or reduce reverse delay; reverse from stopped must be immediate.
- Add reverse yaw assist that works even at low speed.
- Add surface-aware damping for road/grass/park transitions.
- Keep speed in a controllable open-world range before adding traffic or dense props.

### Aircraft Controls

Target feel: easy arcade aircraft with believable ground roll.

Required behavior:

- `W`: throttle increases and aircraft accelerates on runway.
- `A/D` on ground: taxi/ground steering works while accelerating.
- `S` on ground at low speed: slow reverse taxi or strong brake, depending speed.
- `S+A` / `S+D` on ground: reverse taxi diagonal movement works at low speed.
- Takeoff: no vertical pop. Aircraft must roll, reach takeoff speed, rotate, then climb.
- In air, `S` means throttle down / airbrake / descent, not backward flying.
- Aircraft does not enter `Flying` state only because speed crosses a number while still planted on the runway.

Implementation direction:

- Add explicit `GroundRoll`, `Rotate`, `LiftOff`, and `Airborne` phases.
- Replace the large upward takeoff boost with a smaller rotation/lift assist tied to angle of attack and runway speed.
- Require either loss of ground contact or a minimum altitude before `Flying` state is final.
- Add ground reverse/taxi behavior only below a safe low-speed threshold.
- Keep A/D easy: yaw/turn on ground, banked assist in air.

## B: Seoul World and Asset Rebuild Requirements

### Macro Map

The map should read as Seoul from the air and from the road.

Required structure:

- Hangang runs east-west and is the main navigation spine.
- The starting route must not face an empty area.
- West/center: airport/road entry and Yeouido finance island.
- Center: Banpo bridge, Nodeul Island, floating-island style landmarks, riverside roads.
- North: Namsan ridge, N Seoul Tower silhouette, Jongno/Gwanghwamun civic axis.
- South: Gangnam boulevard grid and Teheran-ro style road corridor.
- East: Jamsil, Seokchon Lake, Lotte World Tower silhouette.
- Roads must connect playable driving, not just decorate the scene.

### Asset Layers

Use all appropriate Unity asset types:

- `3D models`: supercar body, aircraft body improvements, bridges, landmark tower forms, road barriers, streetlights, signs, trees.
- `Textures`: facade atlases, road lane markings, rooftop detail, park paths, water detail.
- `Materials`: glass, concrete, asphalt, metal, park grass, water, night/emissive windows.
- `Sprites`: road signs, district labels, simple UI/map markers, lane arrows where mesh geometry would be wasteful.
- `Cubemap`: Seoul sky/atmosphere reflection and skyline mood for glass/water.
- `Prefabs`: repeatable street props, road segments, building families, bridge pieces, supercar, aircraft.

### Supercar

The car must stop looking like a generic block.

Required visual features:

- Low wedge/supercar silhouette.
- Wide stance.
- Distinct hood, cabin, rear engine deck, spoiler, headlights, taillights, wheel arches.
- Red or dark performance paint by default.
- Collider and WheelColliders remain functional; visual pieces must not create hidden collision problems.

### Seoul Visual Density

The scene must not be judged by object count alone.

Required visual cues:

- Front-facing spawn view contains road, buildings, signs, and a visible destination.
- Buildings have distinct roof and side treatment.
- Yeouido, Gangnam, Jongno, and Jamsil must have different skyline silhouettes.
- The river must have bridges, edge parks, and riverside roads.
- Road markings and signs must make the car route readable.

## Audio Requirements

The user will provide MP3 files. The project must map them by filename and role.

Expected roles:

- `park` / `default` / `bgm`: default park/world background music.
- `car` / `engine` / `supercar`: car driving engine loop.
- `takeoff` / `aircraft_takeoff` / `plane_takeoff`: aircraft takeoff clip.
- `cruise` / `aircraft_cruise` / `plane_move` / `flight`: aircraft in-flight movement loop.

Rules:

- Put provided files under `Assets/MINgo/Audio/Provided/`.
- Add an audio registry that scans known clip references or is populated by editor tooling.
- If a role is missing, the game must continue with silence or low-volume placeholder, not break play mode.
- Car audio follows car speed/throttle.
- Aircraft takeoff audio plays while grounded and accelerating through takeoff.
- Aircraft cruise audio crossfades after lift-off.
- Default BGM is always available when the `park/default/bgm` clip exists.

## Acceptance Criteria

### A Gate: Controls

PlayMode tests must prove:

- Car `S` from stop reverses within `0.35s`.
- Car `S+D` from stop moves backward and yaws at least `10 degrees` within `2s`.
- Car `W+D` moves forward and yaws at least `20 degrees` within `2s`.
- Car speed stays under a readable Seoul-city cap.
- Car crosses road-to-grass/park edges without height launch over `1.8m` or roll over `18 degrees`.
- Aircraft holds runway contact during early throttle roll.
- Aircraft does not reach `Flying` state before runway roll and lift-off criteria.
- Aircraft reaches a gradual climb after runway roll, with no sudden vertical velocity spike.
- Aircraft ground `S+D` performs low-speed reverse taxi diagonal movement.
- In-air `S` slows/descends but does not move the aircraft backward.

### B Gate: Seoul and Assets

EditMode and scene contract tests must prove:

- Spawn-forward view has road, city objects, and a visible destination marker.
- Seoul road network has connected driveable surfaces from start toward Hangang.
- At least one full car route crosses a bridge and reaches a south-river district.
- Yeouido, Namsan/Jongno, Gangnam, and Jamsil contain distinct asset families.
- Supercar prefab exists and uses multiple visual parts/materials.
- Seoul material set includes glass, asphalt, concrete, park, water, metal, and emissive/night-window material.
- Project contains generated or imported texture assets for roof/facade/road/water detail.
- Project contains a cubemap or sky/reflection asset used by glass/water materials.

### Audio Gate

EditMode or PlayMode tests must prove:

- Filename-to-role mapping finds expected role names when MP3 files are present.
- Missing MP3 roles do not throw exceptions.
- Car audio source is separate from aircraft audio sources.
- Aircraft takeoff and cruise use separate clips/sources.
- Default BGM is not tied to vehicle state.

## Source References To Use During Implementation

- Unity WheelCollider vehicle setup and API: https://docs.unity3d.com/Manual/WheelColliderTutorial.html
- Unity Rigidbody force modes: https://docs.unity3d.com/ScriptReference/Rigidbody.AddForce.html
- Unity AudioSource pitch/volume behavior: https://docs.unity3d.com/ScriptReference/AudioSource.html
- FAA Airplane Flying Handbook, takeoff and ground operations: https://www.faa.gov/regulations_policies/handbooks_manuals/aviation/airplane_handbook
- NASA lift / angle-of-attack basics: https://www.grc.nasa.gov/www/k-12/airplane/lift1.html
- Seoul Hangang official overview: https://world.seoul.go.kr/service/amusement/hangang/overview/
- Seoul Hangang Parks official overview: https://english.seoul.go.kr/service/amusement/hangang/hangang-parks/
- Seoul accessible Hangang official destinations: https://english.seoul.go.kr/the-great-hangang-river/accessible-hangang-river/
- N Seoul Tower official intro: https://www.nseoultower.co.kr/eng/global/intro.asp
- Visit Seoul / Seoul Sky as Lotte World Tower reference: https://visit.seoul.kr/en/places/seoul-sky

## Implementation Order

1. Add/strengthen failing PlayMode tests for car reverse, reverse diagonal, aircraft runway roll, aircraft gradual takeoff, and aircraft ground reverse taxi.
2. Fix car controller root causes.
3. Fix aircraft controller root causes.
4. Add MP3-role audio registry and runtime sources, without requiring MP3 files to exist yet.
5. Rebuild supercar visual prefab/scene generation.
6. Rebuild Seoul forward route, road network, skyline families, materials, textures, sprites, and cubemap usage.
7. Rebuild scene and run EditMode, PlayMode, and macOS build verification.

## Non-Goals

- Do not import copyrighted GTA assets.
- Do not block progress waiting for final MP3 files.
- Do not treat object count as enough evidence that the map feels like Seoul.
- Do not weaken tests to preserve old controller behavior.

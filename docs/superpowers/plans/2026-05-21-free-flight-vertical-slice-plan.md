# Free Flight Vertical Slice Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build the MINgo MVP as a sequence of playable vertical slices: takeoff, free flight, arbitrary landing, context feedback, restricted airspace danger, emergency landing, and repeatable PC build verification.

**Architecture:** Keep the game as one Unity scene for MVP, with small runtime scripts under `Assets/MINgo/Scripts`. Pure game-rule code handles landing classification and hazard state; MonoBehaviours adapt those rules to Unity physics, input, camera, HUD, and scene objects. Every phase must leave behind a playable build or a clearly blocked verification note.

**Tech Stack:** Unity 6000.3.11f1, C#, Unity Physics, Unity Input System 1.19.0, uGUI, Unity Test Framework 1.6.0, macOS Standalone build first.

---

## Source Spec

- `docs/superpowers/specs/2026-05-21-free-flight-sandbox-design.md`

## Planning Assumptions

- The GitHub repository for this project is `https://github.com/kwmin122/unity-free-flight`. If the local checkout has git initialized, every phase should end with a focused commit and a checkpoint note.
- Existing `Assets` content is minimal: `Assets/Scenes/SampleScene.unity` and `Assets/InputSystem_Actions.inputactions`.
- Do not build art first. Prove the toy-like loop before asset polish.
- Keep the first implementation in one scene: `Assets/Scenes/FreeFlightSandbox.unity`.
- Player weapons, base destruction, walking, traffic, multiplayer, and world streaming stay out of scope.

## Vertical Slice Policy

Each phase must answer one play question:

1. **Baseline:** Can a PC build launch a known scene?
2. **Flight Feel:** Is takeoff and one-minute free flight already pleasant?
3. **Landing Toy:** Does visible terrain feel like a landing challenge?
4. **World Shape:** Do coast, road, ridge, canyon, city edge, and airport create self-directed routes?
5. **Restricted Airspace:** Does the mountain danger zone create evasion and emergency landing without becoming combat?
6. **MVP Gate:** Does the full built player support takeoff, exploration, landing labels, damage/failure, and repeat play?

Do not advance a phase because the code compiles. Advance only when the phase's play question is answered in a built player or the blocker is documented.

## File Structure

Create these focused runtime areas:

- `Assets/MINgo/Scripts/Flight/`
  - `AircraftState.cs`: shared aircraft state enum.
  - `FlightInputReader.cs`: reads keyboard/gamepad input into a simple snapshot.
  - `ArcadeAircraftController.cs`: arcade aircraft physics, throttle, steering, stabilization, damage handling.
  - `ChaseCameraRig.cs`: third-person follow camera with speed pullback.
- `Assets/MINgo/Scripts/Landing/`
  - `SurfaceKind.cs`: surface categories for landing labels.
  - `SurfaceTag.cs`: MonoBehaviour marker for runway, road, field, ridge, canyon, water.
  - `LandingSample.cs`: immutable data used for classification.
  - `LandingClassifier.cs`: pure rule engine for clean, rough, damaged, crashed, submerged, context labels.
  - `LandingStateMachine.cs`: reads contacts and applies classifier results to the aircraft.
- `Assets/MINgo/Scripts/Hazards/`
  - `RestrictedAirspaceState.cs`: pure timing state for warning, lock-on, launch, and escape.
  - `RestrictedAirspaceZone.cs`: warning, lock-on, missile launch, and escape state.
  - `MissileThreat.cs`: visible evadable missile, damage on hit, timeout on miss.
- `Assets/MINgo/Scripts/UI/`
  - `FlightHud.cs`: speed, altitude, aircraft state, context label, restricted airspace warning.
- `Assets/MINgo/Scripts/World/`
  - `WorldBounds.cs`: simple water/failure boundary and optional reset volume.
- `Assets/MINgo/Editor/`
  - `FreeFlightSceneBuilder.cs`: editor-only builder that creates the greybox scene consistently.
  - `MINgoBuildPipeline.cs`: editor-only macOS build command.
- `Assets/MINgo/Tests/EditMode/`
  - `MINgo.EditMode.asmdef`: edit-mode test assembly.
  - `LandingClassifierTests.cs`: pure classification tests.
  - `RestrictedAirspaceStateTests.cs`: pure restricted-airspace timing tests.
- `docs/superpowers/checkpoints/`
- One checkpoint note per completed phase, plus a focused git commit when the local checkout has git initialized.

---

## Phase 0: Project Baseline And Buildable Scene

**Goal:** A known Unity scene exists, is listed in build settings, and can produce a macOS standalone build.

**Files:**
- Create: `Assets/MINgo/Editor/FreeFlightSceneBuilder.cs`
- Create: `Assets/MINgo/Editor/MINgoBuildPipeline.cs`
- Create: `Assets/Scenes/FreeFlightSandbox.unity` through the editor builder.
- Modify: `ProjectSettings/EditorBuildSettings.asset` through Unity APIs only.
- Create: `docs/superpowers/checkpoints/phase-00-baseline.md`

- [ ] **Step 1: Create the editor scene builder shell**

Create `Assets/MINgo/Editor/FreeFlightSceneBuilder.cs` with a menu command that creates `Assets/Scenes/FreeFlightSandbox.unity`, adds a directional light, a camera, a flat runway plane, and saves the scene.

```csharp
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MINgo.EditorTools
{
    public static class FreeFlightSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/FreeFlightSandbox.unity";

        [MenuItem("MINgo/Rebuild Free Flight Sandbox Scene")]
        public static void RebuildScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            RenderSettings.skybox = null;
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.62f, 0.72f, 0.78f);
            RenderSettings.fogDensity = 0.0025f;

            var lightObject = new GameObject("Sun");
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            lightObject.transform.rotation = Quaternion.Euler(48f, -35f, 0f);

            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            var camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.55f, 0.72f, 0.88f);
            cameraObject.transform.position = new Vector3(0f, 18f, -38f);
            cameraObject.transform.rotation = Quaternion.Euler(18f, 0f, 0f);

            GameObject runway = GameObject.CreatePrimitive(PrimitiveType.Cube);
            runway.name = "Runway";
            runway.transform.position = Vector3.zero;
            runway.transform.localScale = new Vector3(18f, 0.25f, 180f);
            runway.GetComponent<Renderer>().sharedMaterial = MakeMaterial("Runway_Mat", new Color(0.19f, 0.2f, 0.2f));

            EditorSceneManager.SaveScene(scene, ScenePath);
            AddSceneToBuildSettings(ScenePath);
        }

        private static Material MakeMaterial(string name, Color color)
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            material.name = name;
            material.color = color;
            return material;
        }

        private static void AddSceneToBuildSettings(string scenePath)
        {
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(scenePath, true)
            };
        }
    }
}
```

- [ ] **Step 2: Create the build pipeline command**

Create `Assets/MINgo/Editor/MINgoBuildPipeline.cs`.

```csharp
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace MINgo.EditorTools
{
    public static class MINgoBuildPipeline
    {
        private const string ScenePath = "Assets/Scenes/FreeFlightSandbox.unity";
        private const string BuildDirectory = "Builds/macOS";
        private const string BuildPath = BuildDirectory + "/MINgo.app";

        [MenuItem("MINgo/Build macOS Player")]
        public static void BuildMacOSPlayer()
        {
            Directory.CreateDirectory(BuildDirectory);

            var options = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = BuildPath,
                target = BuildTarget.StandaloneOSX,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new BuildFailedException($"MINgo macOS build failed: {report.summary.result}");
            }
        }
    }
}
```

- [ ] **Step 3: Run the scene builder in Unity**

Run Unity in batch mode:

```bash
"/Applications/Unity/Hub/Editor/6000.3.11f1/Unity.app/Contents/MacOS/Unity" \
  -batchmode \
  -projectPath "/Users/a0000/Library/Mobile Documents/com~apple~CloudDocs/Desktop/dev/MINgo" \
  -executeMethod MINgo.EditorTools.FreeFlightSceneBuilder.RebuildScene \
  -quit
```

Expected: Unity exits with code 0 and creates `Assets/Scenes/FreeFlightSandbox.unity`.

- [ ] **Step 4: Build macOS player**

```bash
"/Applications/Unity/Hub/Editor/6000.3.11f1/Unity.app/Contents/MacOS/Unity" \
  -batchmode \
  -projectPath "/Users/a0000/Library/Mobile Documents/com~apple~CloudDocs/Desktop/dev/MINgo" \
  -executeMethod MINgo.EditorTools.MINgoBuildPipeline.BuildMacOSPlayer \
  -quit
```

Expected: `Builds/macOS/MINgo.app` exists.

- [ ] **Step 5: Checkpoint**

Commit after verification:

```bash
git add Assets/MINgo/Editor Assets/Scenes/FreeFlightSandbox.unity ProjectSettings/EditorBuildSettings.asset
git commit -m "chore: add free flight build baseline"
```

Also create `docs/superpowers/checkpoints/phase-00-baseline.md`:

```markdown
# Phase 00 Baseline Checkpoint

Date: 2026-05-21
Result: PASS
Evidence:
- `Assets/Scenes/FreeFlightSandbox.unity` generated.
- `Builds/macOS/MINgo.app` generated.
- No gameplay implemented yet.
```

---

## Phase 1: Flight Feel Slice

**Goal:** The player can throttle up, take off, steer, and fly for at least one minute with an arcade third-person camera.

**Files:**
- Create: `Assets/MINgo/Scripts/Flight/AircraftState.cs`
- Create: `Assets/MINgo/Scripts/Flight/FlightInputReader.cs`
- Create: `Assets/MINgo/Scripts/Flight/ArcadeAircraftController.cs`
- Create: `Assets/MINgo/Scripts/Flight/ChaseCameraRig.cs`
- Modify: `Assets/MINgo/Editor/FreeFlightSceneBuilder.cs`
- Create: `docs/superpowers/checkpoints/phase-01-flight-feel.md`

- [ ] **Step 1: Define aircraft state**

```csharp
namespace MINgo.Flight
{
    public enum AircraftState
    {
        Flying,
        Grounded,
        Landed,
        Damaged,
        Submerged,
        Crashed
    }
}
```

- [ ] **Step 2: Add input reader**

Use the Input System directly so the first slice does not depend on generated input wrappers.

Controls:
- `W/S`: pitch down/up
- `A/D`: roll left/right
- `Q/E`: yaw left/right
- `Left Shift`: throttle up
- `Left Ctrl`: throttle down
- `Space`: brake

- [ ] **Step 3: Add arcade aircraft controller**

Implementation requirements:
- Require `Rigidbody`.
- Maintain `Throttle01`.
- Apply forward force based on throttle and current speed.
- Apply pitch, roll, and yaw torques.
- Apply mild stabilization when input is near zero.
- Weaken lift and steering at very low speed, but keep the stall readable.
- Expose `SpeedMetersPerSecond`, `AltitudeMeters`, and `CurrentState`.

Minimum tuning values for first pass:

```csharp
public float maxThrust = 85f;
public float lift = 0.55f;
public float pitchTorque = 35f;
public float rollTorque = 55f;
public float yawTorque = 18f;
public float stabilization = 3.5f;
public float groundBrake = 16f;
public float takeoffSpeed = 22f;
```

- [ ] **Step 4: Add chase camera**

Implementation requirements:
- Follow the aircraft transform.
- Base offset: `(0, 7, -18)`.
- Pull back up to 8 additional meters as speed increases.
- Smooth with `Vector3.SmoothDamp`.
- Look at a point 2 meters above aircraft center.

- [ ] **Step 5: Extend scene builder**

Update `FreeFlightSceneBuilder.RebuildScene()` to:
- Create an aircraft from simple primitives.
- Add `Rigidbody`.
- Add `ArcadeAircraftController`.
- Add `ChaseCameraRig` to the main camera.
- Place aircraft at `(0, 2, -65)` facing forward along the runway.

- [ ] **Step 6: Build and playtest**

Run the scene builder, build macOS player, launch the app, and test:
- Aircraft starts on/near runway.
- Throttle increases with `Left Shift`.
- Aircraft lifts off before runway end.
- Player can fly for one minute without immediate uncontrollable spin.
- Camera follows without clipping through the aircraft.

Tuning loop:
- Run at least one built-player playtest before changing tuning values.
- If takeoff fails, adjust only `maxThrust`, `lift`, or `takeoffSpeed`.
- If the aircraft spins or overreacts, adjust only `pitchTorque`, `rollTorque`, `yawTorque`, or `stabilization`.
- If the camera hides the aircraft or clips through it, adjust only the chase camera offset, pullback distance, or smoothing time.
- Do at most three tuning passes in Phase 1. If the aircraft still cannot pass the one-minute free-flight check, mark the checkpoint `BLOCKED` with the exact failing behavior instead of expanding scope.
- Record the final tuning values in `docs/superpowers/checkpoints/phase-01-flight-feel.md`.

- [ ] **Step 7: Checkpoint**

Create `docs/superpowers/checkpoints/phase-01-flight-feel.md`:

```markdown
# Phase 01 Flight Feel Checkpoint

Date: 2026-05-21
Result: PASS or BLOCKED
Build: `Builds/macOS/MINgo.app`
Playtest:
- Takeoff from runway:
- One-minute free flight:
- Camera readability:
- Main tuning concern:
```

---

## Phase 2: Landing And Repeat Takeoff Slice

**Goal:** The player can land on multiple surface types, receive a legible result, and take off again after a clean landing.

**Files:**
- Create: `Assets/MINgo/Scripts/Landing/SurfaceKind.cs`
- Create: `Assets/MINgo/Scripts/Landing/SurfaceTag.cs`
- Create: `Assets/MINgo/Scripts/Landing/LandingSample.cs`
- Create: `Assets/MINgo/Scripts/Landing/LandingClassifier.cs`
- Create: `Assets/MINgo/Scripts/Landing/LandingStateMachine.cs`
- Create: `Assets/MINgo/Tests/EditMode/MINgo.EditMode.asmdef`
- Create: `Assets/MINgo/Tests/EditMode/LandingClassifierTests.cs`
- Modify: `Assets/MINgo/Scripts/Flight/ArcadeAircraftController.cs`
- Modify: `Assets/MINgo/Editor/FreeFlightSceneBuilder.cs`
- Create: `docs/superpowers/checkpoints/phase-02-landing-loop.md`

- [ ] **Step 1: Write landing classifier tests first**

Create tests that lock the MVP rules:
- clean runway landing passes.
- road landing labels as road.
- steep ridge impact crashes.
- water contact submerges.
- damaged aircraft cleanly touching a field becomes emergency landing context.

Expected command:

```bash
"/Applications/Unity/Hub/Editor/6000.3.11f1/Unity.app/Contents/MacOS/Unity" \
  -batchmode \
  -projectPath "/Users/a0000/Library/Mobile Documents/com~apple~CloudDocs/Desktop/dev/MINgo" \
  -runTests \
  -testPlatform editmode \
  -testResults "Builds/TestResults/editmode-phase-02.xml" \
  -quit
```

Expected before implementation: tests fail because landing types do not exist yet.

- [ ] **Step 2: Implement landing rule types**

Required enums:

```csharp
namespace MINgo.Landing
{
    public enum SurfaceKind
    {
        Unknown,
        Runway,
        Road,
        Field,
        Ridge,
        CanyonFloor,
        Water
    }

    public enum LandingOutcome
    {
        None,
        Clean,
        Rough,
        Damaged,
        Crashed,
        Submerged
    }

    public enum LandingContext
    {
        None,
        RunwayLanding,
        RoadLanding,
        FieldLanding,
        RidgeLanding,
        CanyonFloorLanding,
        RoughLanding,
        ShortTakeoff,
        EmergencyLanding,
        Submerged,
        RestrictedAirspace
    }
}
```

- [ ] **Step 3: Implement `LandingClassifier` as pure code**

Rules for first pass:
- Water always returns `Submerged`.
- Vertical impact greater than `12 m/s` crashes.
- Absolute pitch greater than `22` degrees or roll greater than `28` degrees makes rough/crash depending on impact.
- Ground slope greater than `24` degrees makes rough; greater than `38` degrees crashes if vertical impact is above `7 m/s`.
- Clean landing requires vertical impact <= `5.5 m/s`, forward speed between `8` and `45 m/s`, pitch <= `14`, roll <= `16`, slope <= `18`.
- Damaged aircraft with a survivable field/road/runway landing gets `EmergencyLanding`.

- [ ] **Step 4: Add `SurfaceTag` markers**

`SurfaceTag` exposes:

```csharp
public SurfaceKind kind = SurfaceKind.Unknown;
public bool allowsShortTakeoff = true;
```

- [ ] **Step 5: Add `LandingStateMachine`**

Implementation requirements:
- Listen to `OnCollisionEnter`, `OnCollisionStay`, `OnCollisionExit`, and `OnTriggerEnter`.
- Derive surface kind from `SurfaceTag`.
- On `OnCollisionEnter`, capture the first contact normal and impact speed immediately using `Mathf.Abs(Vector3.Dot(collision.relativeVelocity, contact.normal))`; do not wait for `OnCollisionStay`, because the rigidbody may already have slowed down by then.
- Use `OnCollisionStay` only to confirm the aircraft has remained stable on the surface for at least `0.35` seconds before resolving a clean or rough landing.
- Use `OnTriggerEnter` for water volumes so ocean failure still works if the ocean is configured as a trigger.
- Use `LandingClassifier`.
- Apply state to `ArcadeAircraftController`.
- Keep the last context label for HUD.
- Allow throttle/takeoff again after clean landing.

- [ ] **Step 6: Extend scene builder with test surfaces**

Add named greybox surfaces:
- `Runway` with `SurfaceKind.Runway`.
- `Coastal Road` with `SurfaceKind.Road`.
- `Open Field` with `SurfaceKind.Field`.
- `Ridge Landing Shelf` with `SurfaceKind.Ridge`.
- `Canyon Floor` with `SurfaceKind.CanyonFloor`.
- `Ocean` with `SurfaceKind.Water` and trigger/collider for failure.

- [ ] **Step 7: Run tests and manual build**

Expected:
- EditMode tests pass.
- Built player supports runway landing, road landing, field landing, water failure.
- After clean landing, throttle can start a new takeoff roll.

- [ ] **Step 8: Checkpoint**

Create `docs/superpowers/checkpoints/phase-02-landing-loop.md` with:

```markdown
# Phase 02 Landing Loop Checkpoint

Date: 2026-05-21
Result: PASS or BLOCKED
Automated tests:
- EditMode landing classifier:
Built-player manual checks:
- Runway landing:
- Road landing:
- Field landing:
- Water failure:
- Clean landing to repeat takeoff:
```

---

## Phase 3: Greybox World Slice

**Goal:** The map gives the player visible reasons to choose a direction and attempt landings without any mission system.

**Files:**
- Modify: `Assets/MINgo/Editor/FreeFlightSceneBuilder.cs`
- Create: `Assets/MINgo/Scripts/World/WorldBounds.cs`
- Create: `docs/superpowers/checkpoints/phase-03-greybox-world.md`

- [ ] **Step 1: Expand the scene builder into a coastal mainland**

Add greybox regions with distinct silhouettes:
- Airport at the start.
- Ocean plane to one side.
- Beach strip and coastal road.
- Open inland fields.
- City edge as 20 to 35 low-rise cubes.
- Mountain ridge using large sloped cubes or terrain-like blockouts.
- Canyon route with two wall lines and a navigable floor.

Keep `FreeFlightSceneBuilder.cs` as a readable editor builder, not a runtime world generator. Split scene construction into private helper methods:
- `CreateAirport()`
- `CreateCoastline()`
- `CreateRoads()`
- `CreateCityEdge()`
- `CreateFields()`
- `CreateMountainRidge()`
- `CreateCanyonRoute()`
- `CreateLandingSurface(string name, SurfaceKind kind, Vector3 position, Vector3 scale, Color color)`

Do not add a separate procedural-generation system in Phase 3.

- [ ] **Step 2: Place landing opportunities intentionally**

Ensure the scene contains at least these attempt targets:
- Long runway.
- Straight road segment.
- Wide field.
- Ridge shelf.
- Canyon floor.
- Beach near water where water failure is tempting but avoidable.

- [ ] **Step 3: Add natural boundaries**

Use ocean, high mountains, and fog to make the map feel bounded without streaming.

- [ ] **Step 4: Add `WorldBounds`**

`WorldBounds` should:
- Detect aircraft below a water/failure height.
- Mark submerged if the aircraft enters an ocean trigger.
- Provide a reset key only for development, not as a core mechanic.

- [ ] **Step 5: Build and first-five-minutes playtest**

Manual pass criteria:
- Within 30 seconds of takeoff, at least three readable destinations are visible.
- The player can identify coast, city edge, mountain, and canyon without a minimap.
- There are at least five obvious landing temptations.
- The route from airport to canyon is legible.

- [ ] **Step 6: Checkpoint**

Create `docs/superpowers/checkpoints/phase-03-greybox-world.md`:

```markdown
# Phase 03 Greybox World Checkpoint

Date: 2026-05-21
Result: PASS or BLOCKED
First-five-minutes playtest:
- Visible directions after takeoff:
- Landing temptations found:
- Confusing or boring regions:
- Build path:
```

---

## Phase 4: HUD And Context Feedback Slice

**Goal:** The player understands aircraft state, speed, altitude, and why a landing counted as runway, road, field, ridge, canyon, rough, submerged, or emergency.

**Files:**
- Create: `Assets/MINgo/Scripts/UI/FlightHud.cs`
- Modify: `Assets/MINgo/Scripts/Landing/LandingStateMachine.cs`
- Modify: `Assets/MINgo/Editor/FreeFlightSceneBuilder.cs`
- Create: `docs/superpowers/checkpoints/phase-04-feedback.md`

- [ ] **Step 1: Add minimal HUD**

HUD fields:
- Speed in `m/s`.
- Altitude in meters.
- Aircraft state.
- Context label.
- Reserved restricted-warning text field, hidden until Phase 5.

Required labels:
- `Runway landing`
- `Road landing`
- `Field landing`
- `Ridge landing`
- `Canyon floor landing`
- `Rough landing`
- `Emergency landing`
- `Submerged`

- [ ] **Step 2: Connect HUD to controller and landing state**

`FlightHud` reads:
- `ArcadeAircraftController.SpeedMetersPerSecond`
- `ArcadeAircraftController.AltitudeMeters`
- `ArcadeAircraftController.CurrentState`
- `LandingStateMachine.LastContext`

- [ ] **Step 3: Keep HUD minimal**

Do not add a mission panel, score panel, minimap, or tutorial overlay in this phase. The HUD exists only to confirm state and landing recognition.

- [ ] **Step 4: Build and label playtest**

Manual checks:
- Land on runway and see runway label.
- Land on road and see road label.
- Touch water and see submerged/failure.
- Damage state is visible.
- Labels disappear or settle after a readable delay instead of spamming.

- [ ] **Step 5: Checkpoint**

Create `docs/superpowers/checkpoints/phase-04-feedback.md`.

---

## Phase 5: Restricted Airspace Hazard Slice

**Goal:** The mountain-side restricted airspace creates forbidden-space curiosity, lock-on pressure, evasion, damage, and emergency landing without adding player weapons or base destruction.

**Files:**
- Create: `Assets/MINgo/Scripts/Hazards/RestrictedAirspaceState.cs`
- Create: `Assets/MINgo/Scripts/Hazards/RestrictedAirspaceZone.cs`
- Create: `Assets/MINgo/Scripts/Hazards/MissileThreat.cs`
- Create: `Assets/MINgo/Tests/EditMode/RestrictedAirspaceStateTests.cs`
- Modify: `Assets/MINgo/Scripts/Flight/ArcadeAircraftController.cs`
- Modify: `Assets/MINgo/Scripts/UI/FlightHud.cs`
- Modify: `Assets/MINgo/Editor/FreeFlightSceneBuilder.cs`
- Create: `docs/superpowers/checkpoints/phase-05-restricted-airspace.md`

- [ ] **Step 1: Write restricted airspace timing tests first**

Test these state transitions:
- Entering outer zone returns `Warning`.
- Staying in deep zone for `3` seconds returns `Locking`.
- Staying in deep zone for `6` seconds returns `MissileLaunched`.
- Leaving before launch returns `Escaped` after `1.5` seconds.
- Leaving after launch does not cancel the active missile.

Expected before implementation: tests fail because `RestrictedAirspaceState` does not exist yet.

- [ ] **Step 2: Add restricted airspace state machine**

States:
- `Outside`
- `Warning`
- `Locking`
- `MissileLaunched`
- `Escaped`

First-pass timing:
- Outer warning starts immediately on trigger enter.
- Lock-on starts after `3` seconds inside the deep zone.
- Missile launches after `6` seconds inside the deep zone.
- Leaving the zone before launch clears lock-on after `1.5` seconds.

- [ ] **Step 3: Add visible missile threat**

Requirements:
- Missile is a sphere/capsule with red material.
- Trail renderer creates visible smoke/track.
- Missile turns toward aircraft with capped turn rate, not perfect homing.
- Missile times out after `10` seconds.
- Hit radius applies `Damaged` on first hit.
- If aircraft is already damaged, severe impact can crash through existing landing/collision rules.

First-pass missile tuning:
```csharp
public float missileSpeed = 55f;
public float maxTurnDegreesPerSecond = 65f;
public float missileLifetimeSeconds = 10f;
public float hitRadiusMeters = 8f;
```

Evasion rule: the missile must be visibly dangerous but beatable. In manual testing, the player must be able to evade at least one missile by leaving the zone, diving behind the ridge, or turning through the canyon before this phase can pass.

- [ ] **Step 4: Build military base marker**

Scene builder creates:
- Simple radar dish greybox prop.
- A few low military buildings.
- Warning-colored boundary posts.
- Restricted trigger volume on mountain side.
- Deep lock-on trigger nested inside outer warning zone.

- [ ] **Step 5: Connect HUD warnings**

HUD states:
- `Restricted airspace`
- `Lock-on`
- `Missile launched`
- `Escaped`
- `Emergency landing`

- [ ] **Step 6: Manual evasion playtest**

Pass criteria:
- Entering the zone is optional.
- Warning appears before missile launch.
- Missile has visible trajectory and can be evaded.
- Terrain helps evasion: ridge/canyon/low flight.
- First hit creates damaged/emergency landing opportunity.
- No weapons are added.
- If the missile hits in all three manual escape attempts, reduce `maxTurnDegreesPerSecond` or `missileSpeed` before proceeding.

- [ ] **Step 7: Checkpoint**

Create `docs/superpowers/checkpoints/phase-05-restricted-airspace.md`.

---

## Phase 6: MVP Gate, Tuning, And Build Verification

**Goal:** The complete MVP loop works in a built player and is ready for a focused review.

**Files:**
- Modify only files required by failed checks.
- Create: `docs/superpowers/checkpoints/phase-06-mvp-gate.md`

- [ ] **Step 1: Run edit-mode tests**

```bash
"/Applications/Unity/Hub/Editor/6000.3.11f1/Unity.app/Contents/MacOS/Unity" \
  -batchmode \
  -projectPath "/Users/a0000/Library/Mobile Documents/com~apple~CloudDocs/Desktop/dev/MINgo" \
  -runTests \
  -testPlatform editmode \
  -testResults "Builds/TestResults/editmode-mvp.xml" \
  -quit
```

Expected: landing classifier tests pass.

- [ ] **Step 2: Build macOS player**

```bash
"/Applications/Unity/Hub/Editor/6000.3.11f1/Unity.app/Contents/MacOS/Unity" \
  -batchmode \
  -projectPath "/Users/a0000/Library/Mobile Documents/com~apple~CloudDocs/Desktop/dev/MINgo" \
  -executeMethod MINgo.EditorTools.MINgoBuildPipeline.BuildMacOSPlayer \
  -quit
```

Expected: `Builds/macOS/MINgo.app` exists and launches.

- [ ] **Step 3: Run MVP manual script**

Manual script:

1. Launch built player.
2. Take off from runway.
3. Fly over ocean/coast.
4. Fly toward city edge.
5. Fly toward mountain/canyon.
6. Land on runway.
7. Take off again.
8. Land on road.
9. Take off again if space allows.
10. Land on field.
11. Touch water and confirm submerged/failure.
12. Enter restricted airspace.
13. Escape once without hit.
14. Enter again, allow missile hit, confirm damaged.
15. Attempt emergency landing on field/road.

- [ ] **Step 4: Tune only blockers**

Allowed tuning:
- Aircraft thrust, lift, torque, stabilization.
- Camera offset/smoothing.
- Landing classifier thresholds.
- Missile speed/turn rate/timing.
- Surface placement and scale.

Not allowed in MVP gate:
- New mission system.
- New aircraft models.
- Weapon systems.
- Dense city details.
- Traffic or pedestrians.

- [ ] **Step 5: Write final checkpoint**

Create `docs/superpowers/checkpoints/phase-06-mvp-gate.md`:

```markdown
# Phase 06 MVP Gate Checkpoint

Date: 2026-05-21
Result: PASS or BLOCKED
Build:
- Path:
- Launch result:
Automated tests:
- EditMode:
Manual MVP loop:
- Takeoff:
- One-minute free flight:
- Runway landing:
- Road landing:
- Field landing:
- Water failure:
- Re-takeoff after clean landing:
- Restricted warning:
- Lock-on:
- Missile evasion:
- Damaged emergency landing:
Open issues:
- None, or list exact blocker.
```

---

## Execution Prompts

Use one prompt per phase. Do not ask an agent to implement multiple phases unless the previous phase checkpoint exists.

### Phase 0 Prompt

```text
Implement Phase 0 from docs/superpowers/plans/2026-05-21-free-flight-vertical-slice-plan.md.

Scope:
- Add the editor scene builder.
- Add the macOS build pipeline command.
- Generate FreeFlightSandbox.unity.
- Verify a macOS build exists.

Do not implement flight controls, landing, HUD, world layout, or hazards.
Use `UnityEditor.Build.BuildFailedException`; do not leave the build pipeline with unresolved editor namespaces.
Use karpathy-guidelines: smallest complete change, surgical edits, concrete verification.
End with the checkpoint file and exact build evidence.
```

### Phase 1 Prompt

```text
Implement Phase 1 from docs/superpowers/plans/2026-05-21-free-flight-vertical-slice-plan.md.

Scope:
- Add arcade aircraft input/controller.
- Add chase camera.
- Extend scene builder to spawn the aircraft.
- Build and playtest takeoff plus one-minute flight.
- Run the Phase 1 tuning loop and record final tuning values.

Do not implement landing classification, HUD, world expansion, or restricted airspace.
End with the Phase 1 checkpoint and tuning notes.
```

### Phase 2 Prompt

```text
Implement Phase 2 from docs/superpowers/plans/2026-05-21-free-flight-vertical-slice-plan.md.

Scope:
- Write landing classifier tests first.
- Implement surface tags, landing classifier, and landing state machine.
- Capture landing impact on `OnCollisionEnter`; use `OnCollisionStay` only for settled contact.
- Handle ocean failure through `OnTriggerEnter` if water is configured as a trigger.
- Add runway, road, field, ridge, canyon, and water test surfaces.
- Verify clean landing, rough/crash, submerged, and repeat takeoff.

Do not expand the full world or add restricted airspace.
End with tests, built-player checks, and checkpoint evidence.
```

### Phase 3 Prompt

```text
Implement Phase 3 from docs/superpowers/plans/2026-05-21-free-flight-vertical-slice-plan.md.

Scope:
- Expand the greybox map into airport, coast, road, field, city edge, ridge, and canyon.
- Keep everything simple and readable.
- Keep `FreeFlightSceneBuilder.cs` split into private helper methods; do not create a procedural world system.
- Ensure the first five minutes create self-directed landing temptations.

Do not add art polish, missions, combat, or missile hazards.
End with a built-player first-five-minutes checkpoint.
```

### Phase 4 Prompt

```text
Implement Phase 4 from docs/superpowers/plans/2026-05-21-free-flight-vertical-slice-plan.md.

Scope:
- Add the minimal HUD.
- Show speed, altitude, aircraft state, and landing context labels.
- Verify runway, road, field, ridge, canyon, rough, emergency, and submerged labels.

Do not add score, minimap, tutorial overlay, or mission UI.
End with label verification evidence.
```

### Phase 5 Prompt

```text
Implement Phase 5 from docs/superpowers/plans/2026-05-21-free-flight-vertical-slice-plan.md.

Scope:
- Add restricted airspace on the mountain side.
- Add warning, lock-on, missile launch, evasion, damaged state, and emergency landing flow.
- Keep it as an environmental hazard, not combat.
- Tune the missile so it is visibly dangerous but beatable through ridge, canyon, or zone-exit evasion.

Do not add player weapons, base destruction, dogfighting, or combat progression.
End with evasion and damaged-emergency-landing checkpoint evidence.
```

### Phase 6 Prompt

```text
Implement Phase 6 from docs/superpowers/plans/2026-05-21-free-flight-vertical-slice-plan.md.

Scope:
- Run final edit-mode tests.
- Build macOS player.
- Execute the full MVP manual script.
- Tune only blockers that prevent the MVP loop.

Do not add new features during the MVP gate.
End with phase-06-mvp-gate.md and exact pass/blocker evidence.
```

---

## Self-Review

Spec coverage:
- Playable PC build: Phase 0 and Phase 6.
- GTA-like arcade flight: Phase 1.
- Arbitrary landings and repeat takeoff: Phase 2.
- Coastal mainland with airport, city edge, coast, fields, ridge, canyon: Phase 3.
- Landing context labels: Phase 4.
- Restricted airspace, lock-on, missile, evasion, damaged emergency landing: Phase 5.
- Full MVP loop verification: Phase 6.

Placeholder scan:
- No unresolved planning-marker strings remain.
- Every phase has a concrete goal, files, scope exclusions, and verification.

Scope check:
- Weapons, traffic, pedestrians, walking, streaming, dense art, and combat progression are explicitly excluded.
- The plan stays vertical-slice oriented and does not split into technical layers such as "all physics first" or "all art first."

Execution note:
- The local project should be connected to `kwmin122/unity-free-flight`. Each phase should leave both a checkpoint markdown file and a focused commit after verification.

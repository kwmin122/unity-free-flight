# Easy Flight Controls Rework Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make the default aircraft controls feel like an open-world GTA-style vehicle: easy takeoff, simple WASD turning, readable camera, and optional advanced pitch/roll for players who want it.

**Architecture:** Keep the existing Rigidbody flight model and scene builder. Rework the player-facing layer first: input mapping, assisted control output, camera target behavior, HUD hints, and tests. Do not replace the aircraft physics with a full simulator until the easy-control loop is fun.

**Tech Stack:** Unity 6000.3.11f1, C#, Unity Physics Rigidbody, Unity Input System 1.19.0, uGUI, Unity Test Framework 1.6.0.

---

## Sources Used

- Rockstar GTAV PC controls article: PC flying defaults to keyboard/numpad, mouse override is supported, flying controls are rebindable, and vehicle control settings are adjusted while in a vehicle.
- Rockstar Support vehicle-control article: vehicle control configuration appears while inside a vehicle.
- Unity Cinemachine Third Person Follow docs: third-person camera should follow a target rig, and camera aim can be decoupled by using an invisible target object instead of the model directly.
- Unity Input System keyboard docs: direct key reads are valid, but simultaneous key limitations exist, so the default control scheme should avoid awkward multi-key chords.
- Unity Rigidbody torque support article: torque can behave unexpectedly around inertia tensors, so the easy controller should prefer target-rate correction and damping over piling raw torque on every axis.
- Vazgriz Unity flight simulator article and repo: fake as much as possible, use simple hand-tuned formulas, expose AOA/drag/G/energy later, and keep accessibility between Ace Combat and serious sims.
- gasgiant Aircraft-Physics repo: per-surface aerodynamics is a validated future path, but too deep for this MVP control pass.
- Existing MINgo research doc: `docs/research/2026-05-21-aircraft-sim-source-integration.md`.

## Diagnosis

The current project already moved toward GTA-like controls, but it still feels too hard because the player is asked to manage too many aircraft-specific ideas too early.

Current issues:

- `W/S` changes throttle, but `S` also acts as brake through `FlightInputReader`, so a beginner can accidentally kill speed while trying to descend or slow down.
- `A/D` requests an assisted bank, but the player still has to understand speed, pitch, takeoff timing, and recovery.
- Pitch is on `Up/Down`, so the first takeoff still depends on non-obvious aircraft controls instead of simple vehicle controls.
- `Q/E` manual roll exists in the main hint, making the default control surface feel more sim-like than open-world.
- Camera is better than before, but still follows aircraft/velocity state directly rather than a true player-intent target.
- There is no explicit stall guard, cruise assist, landing assist, or "basic mode versus advanced mode" boundary.

Target feel:

- A beginner should be able to hold `W`, take off, turn with `A/D`, release keys, and not immediately spiral or stall.
- Advanced inputs should exist, but they should not be required in the first five minutes.
- The camera should communicate "where I am going" more than "what the aircraft's raw physics state is doing."

## Control Model

Default controls:

- `W`: accelerate toward safe cruise throttle. On runway, this also enables takeoff assist.
- `S`: slow down / airbrake. On ground, also brake. In air, it should not instantly dump all control authority.
- `A/D` or `Left/Right`: turn left/right. This is a turn request, not direct roll.
- `Mouse X` or optional `Left/Right`: later camera/heading assist, not required in first pass.
- `Up/Down`: optional pitch trim / climb-dive request.
- `Q/E`: optional advanced roll, hidden from primary HUD hint.
- `Space`: hard ground brake and optional stronger airbrake.
- `R`: reset.

Assist rules:

- If airborne with no pitch input, slowly return toward a safe shallow climb or level attitude.
- If speed falls near stall threshold, limit pitch-up input and add gentle nose-down recovery.
- If the player holds `A/D`, target a safe bank angle and yaw rate instead of raw roll torque.
- If the player releases `A/D`, auto-level wings.
- During takeoff, keep roll nearly level and add nose-up only after enough forward speed.
- Near ground, reduce aggressive bank/pitch unless the player is clearly holding advanced controls.
- Do not add weapons or mission systems in this pass.

## Files

- Modify: `Assets/MINgo/Scripts/Flight/FlightInputReader.cs`
- Modify: `Assets/MINgo/Scripts/Flight/FlightControlAssist.cs`
- Modify: `Assets/MINgo/Scripts/Flight/ArcadeAircraftController.cs`
- Modify: `Assets/MINgo/Scripts/Flight/ChaseCameraRig.cs`
- Modify: `Assets/MINgo/Scripts/UI/FlightHud.cs`
- Modify: `Assets/MINgo/Editor/FreeFlightSceneBuilder.cs`
- Modify: `Assets/MINgo/Tests/EditMode/FlightControlAssistTests.cs`
- Modify: `Assets/MINgo/Tests/EditMode/ChaseCameraRigTests.cs`
- Modify: `Assets/MINgo/Tests/EditMode/FlightHudTests.cs`
- Modify: `Assets/MINgo/Tests/EditMode/FreeFlightSceneContractTests.cs`
- Modify: `README.md`
- Create: `docs/superpowers/checkpoints/phase-11-easy-flight-controls.md`

## Task 1: Lock The New Easy-Control Contract

- [ ] Add tests in `FlightControlAssistTests` for the beginner contract.

Required test cases:

- `CalculateAssistedControls_AutoLevelsWingsWhenTurnReleased`: with current roll at 30 degrees and no turn input, output roll correction returns toward level.
- `CalculateAssistedControls_LimitsPitchUpNearStall`: with low forward speed and pitch-up input, pitch output is capped below full input.
- `CalculateAssistedControls_AddsNoseDownRecoveryNearStall`: with low forward speed and no pitch input, output nudges nose down.
- `CalculateAssistedControls_UsesGentleTakeoffAssistOnlyWhenFastEnough`: on ground below takeoff threshold, no large pitch-up; near takeoff speed with high throttle, positive pitch assist.
- `CalculateAssistedControls_TurnRequestProducesBankAndYawWithoutManualRoll`: `A/D` equivalent creates coordinated bank/yaw and does not require `Q/E`.

- [ ] Add a HUD test asserting the primary hint says `W/S throttle`, `A/D turn`, `Up/Down pitch`, `Space brake`, `R reset`, and does not advertise `Q/E` in the beginner line.

- [ ] Add or update camera tests so the chase forward stays horizon-readable even during steep pitch and does not inherit full roll.

Run targeted RED:

```bash
"/Applications/Unity/Hub/Editor/6000.3.11f1/Unity.app/Contents/MacOS/Unity" -batchmode -projectPath "/tmp/MINgo-easy-controls-red" -runTests -testPlatform EditMode -assemblyNames MINgo.EditMode -testFilter MINgo.Tests.FlightControlAssistTests -testResults "Builds/TestResults/editmode-easy-controls-red.xml" -logFile "/tmp/mingo-editmode-easy-controls-red.log"
```

Expected: new tests fail before implementation.

## Task 2: Separate Beginner Input From Advanced Roll

- [ ] Update `FlightInputSnapshot` only if needed; keep the public shape small.
- [ ] Change `FlightInputReader.ReadKeyboard()` so `S` means slow/airbrake intent, but hard brake is primarily `Space`.
- [ ] Keep `Q/E` manual roll active internally, but treat it as advanced override.
- [ ] Keep arrow keys as alternatives, but avoid requiring them for basic takeoff and turning.

Acceptance:

- Holding `W` and `A/D` is enough to take off and turn.
- A player does not need `Q/E` or rudder knowledge to recover from normal turns.

## Task 3: Add Beginner Flight Assist

- [ ] Extend `FlightControlAssist.CalculateAssistedControls()` with low-speed guard and level-flight recovery parameters.
- [ ] Keep the implementation pure and testable; do not read Unity input or scene objects inside the assist function.
- [ ] Prefer target bank / target pitch / target yaw-rate correction over larger raw torque constants.

Suggested new tunables:

- `safeCruiseThrottle = 0.72f`
- `stallGuardSpeed01 = 0.72f`
- `stallPitchLimit = 0.35f`
- `stallRecoveryPitch = -0.22f`
- `levelPitchAssist = 0.12f`
- `nearGroundBankLimitDegrees = 18f`

Acceptance:

- No-input airborne state drifts toward stable flight.
- Low-speed pitch-up is softened before the aircraft stalls hard.
- Turn input still feels responsive but does not roll indefinitely.

## Task 4: Tune Controller Authority For Arcade Stability

- [ ] In `ArcadeAircraftController`, reduce direct roll/pitch aggressiveness if tests or playtest show oscillation.
- [ ] Add speed-aware angular damping if needed, instead of increasing static damping globally.
- [ ] Keep lift/drag model intact unless a specific control issue proves it must change.
- [ ] Tune scene builder inspector defaults so the saved scene matches code defaults.

Acceptance:

- From runway: hold `W` for takeoff, then `A/D` turns without spiral.
- Release input: aircraft stabilizes into readable forward flight.
- Hold `S` in air: aircraft slows but remains recoverable.

## Task 5: Make The Camera Follow Intent, Not Raw Roll

- [ ] Update `ChaseCameraRig` to optionally use an invisible camera target transform or an internal stabilized heading.
- [ ] Keep world-up orientation so the horizon remains readable.
- [ ] Slightly enlarge aircraft in frame if it still feels distant: reduce `followDistance` only after checking it does not hide terrain.
- [ ] Keep speed-based FOV, but avoid excessive pullback at normal speeds.

Acceptance:

- The aircraft stays near center/back view like the GTA reference.
- The horizon and ground are visible during takeoff, turn, climb, and descent.
- Camera does not roll with the plane.

## Task 6: Update HUD And Scene Contract

- [ ] Change beginner HUD hint to avoid exposing `Q/E` as a required control.
- [ ] Add a secondary advanced hint only if needed later; do not clutter the main HUD now.
- [ ] Regenerate `Assets/Scenes/FreeFlightSandbox.unity` from `FreeFlightSceneBuilder`.
- [ ] Update `FreeFlightSceneContractTests` for camera/controller default values that matter to the easy-control contract.

Acceptance:

- Player sees only the controls they need first.
- Saved scene and builder defaults do not drift.

## Task 7: Verification

Because the Unity Editor is usually open on the original project, run batchmode tests on a temp copy.

Temp copy pattern:

```bash
rm -rf /tmp/MINgo-easy-controls-verify
rsync -a --exclude Library --exclude Temp --exclude Logs --exclude Builds --exclude .git \
  "/Users/a0000/Library/Mobile Documents/com~apple~CloudDocs/Desktop/dev/MINgo/" \
  /tmp/MINgo-easy-controls-verify/
```

Targeted tests:

```bash
"/Applications/Unity/Hub/Editor/6000.3.11f1/Unity.app/Contents/MacOS/Unity" -batchmode -projectPath "/tmp/MINgo-easy-controls-verify" -runTests -testPlatform EditMode -assemblyNames MINgo.EditMode -testFilter MINgo.Tests.FlightControlAssistTests -testResults "Builds/TestResults/editmode-easy-controls-targeted.xml" -logFile "/tmp/mingo-editmode-easy-controls-targeted.log"
```

Full unfiltered EditMode:

```bash
"/Applications/Unity/Hub/Editor/6000.3.11f1/Unity.app/Contents/MacOS/Unity" -batchmode -projectPath "/tmp/MINgo-easy-controls-verify" -runTests -testPlatform EditMode -testResults "Builds/TestResults/editmode-easy-controls-unfiltered.xml" -logFile "/tmp/mingo-editmode-easy-controls-unfiltered.log"
```

Build:

```bash
"/Applications/Unity/Hub/Editor/6000.3.11f1/Unity.app/Contents/MacOS/Unity" -batchmode -projectPath "/tmp/MINgo-easy-controls-verify" -executeMethod MINgo.EditorTools.MINgoBuildPipeline.BuildMacOSPlayer -quit -logFile "/tmp/mingo-build-easy-controls.log"
```

Manual Unity Play Mode smoke:

- Press Play in `Assets/Scenes/FreeFlightSandbox.unity`.
- Hold `W`: aircraft accelerates and takes off without needing pitch keys.
- Press `A/D`: aircraft turns and returns toward level after release.
- Press `S`: aircraft slows but is recoverable.
- Press `Space` on ground: aircraft brakes.
- Press `R`: aircraft resets.
- Confirm the plane is visually large enough and the horizon stays readable.

## Commit Plan

Use small commits:

1. `test: lock easy flight control contract`
2. `feat: add beginner flight assist`
3. `feat: stabilize chase camera for easy flight`
4. `docs: record easy flight controls checkpoint`

Do not proceed to landmark/world additions until this control pass feels playable.

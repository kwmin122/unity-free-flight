# Aircraft Sim Source Integration

Date: 2026-05-21

Purpose: use the supplied community sources as implementation input for MINgo, not as a detached summary.

Copyright boundary: do not paste full blog text, full subtitles, or copied source code into this repo. Convert the material into our own implementation checklist, code structure, tuning targets, and verification criteria.

## Sources Checked

- Naver intro/index: https://blog.naver.com/wjoh0315/222245521019
- Naver #0 theory post linked by the intro: https://blog.naver.com/wjoh0315/222111157068
- Naver #1 project setup: https://blog.naver.com/wjoh0315/222123440070
- Naver #2 script setup linked by the intro: https://blog.naver.com/wjoh0315/222129351767
- Naver #3 programming linked by the intro: https://blog.naver.com/wjoh0315/222132555948
- YouTube reference from the supplied links: https://www.youtube.com/watch?v=p3jDJ9FtTyM
- GitHub repo linked by the Naver intro: https://github.com/wjoh0315/AerocraftSimulator
- YouTube reference supplied on 2026-05-21: https://www.youtube.com/watch?v=7vAHo2B1zLc
- Original blog post linked from that video: https://vazgriz.com/346/flight-simulator-in-unity3d-part-1/
- GitHub repo linked from that video/blog: https://github.com/vazgriz/FlightSim
- Playable reference linked from that video: https://vazgriz.itch.io/flight-sim-part-1

YouTube captions were checked from the available English subtitle track. Local inspection converted it into 147 timestamped caption rows for review, but the full transcript is not committed.

The 7vAHo2B1zLc video had no manual subtitle track available. Local inspection used the English automatic caption track and converted it into 601 timestamped caption rows for review. The full auto transcript is not committed. The linked `vazgriz/FlightSim` repo was also checked at tag `part-1`; it is MIT licensed, but MINgo still uses it as a reference pattern rather than copying architecture or assets.

## Source To Implementation Map

### Intro / Series Index

Use it as the source coverage checklist:

- Theory is not optional background. It defines the terms our code should expose: thrust, lift, drag, aerodynamic moment, airspeed dependence, and stall-like control loss.
- Setup is not only assets. It defines world scale, model hierarchy, UI, audio, and missile/hazard expectations.
- Script setup argues for separating developer-facing aircraft parameters from runtime force results and enum-like state.
- Programming sections include force calculation, force application, debug visualization, control-surface visuals, camera feel, missiles, UI, and audio.
- The linked GitHub repo is reference material only. It has no clearly detected license in the shallow clone, so do not copy code or assets wholesale.

### #0 Aerodynamics Theory

Apply as design rules:

- Thrust: our controller should treat thrust as forward force from the aircraft nose direction, not a generic world-space push.
- Lift: lift should be tied to airflow and angle of attack, not just upward force forever. The current MVP may stay arcade, but the parameter names and debug output must leave room for angle-of-attack behavior.
- Drag: drag should oppose relative airflow and grow with speed. Current linear drag is only a temporary stabilizer; a later slice should use an airspeed-squared component.
- Moment: rotation should come from torque-like behavior and control authority, not instant transform rotation.
- Coefficients: the material repeatedly frames lift, drag, and moment as coefficient-driven. Our eventual implementation should keep coefficients isolated from input handling.
- Scope decision: do not chase NASA-grade data now. Use simple curves or formulas first, then tune from playtest.

### #1 Project Setup

Apply as production setup rules:

- Aircraft model: imported aircraft assets often include extra lights/cameras or open parts. When we replace the blockout aircraft, import cleanup is mandatory.
- Aircraft hierarchy: control surfaces need their own pivot parents if we want aileron/elevator/rudder visuals. Do not rotate mesh children around the wrong origin.
- Missile model: for our game, use this as restricted-airspace incoming missile/hazard reference, not as player weapon scope.
- Missile effects: smoke/explosion/audio are important to readability, but they belong after flight/landing works.
- Terrain: fast aircraft need a large play area. Use the setup post's 2000 x 2000 terrain scale as the minimum greybox target for early playtests.
- UI: speed readout and reticle are useful debug/readability tools. They should become our HUD slice, not a mission panel.
- Audio: engine sound should respond to aircraft speed/throttle; explosion/lock sounds belong to the restricted-airspace hazard.

### #2 Script Setup

Apply as code organization rules:

- Keep aircraft tuning parameters grouped and inspectable.
- Keep force results as structured values, not loose unrelated locals once the physics grows.
- Keep enum/state definitions explicit: grounded, flying, landed, rough, crashed, submerged, restricted-airspace warning/lock/damaged.
- Do not mirror the original monolithic controller. Split our project into small scripts: input, flight physics, camera, landing classifier, HUD, hazard controller, audio.

### #3 Programming

Apply as concrete implementation patterns:

- Aerodynamics:
  - Compute relative airflow from aircraft velocity and angular velocity.
  - Convert airflow into local aircraft space.
  - Derive angle of attack from local airflow.
  - Calculate lift, drag, and moment separately before summing.
- Force application:
  - Apply forces in `FixedUpdate`.
  - Apply torque separately from force.
  - Scale control response down as speed grows, otherwise the aircraft feels twitchy.
  - Increase angular damping/drag with speed to prevent uncontrolled spin.
- Debug:
  - Add gizmo/debug lines for thrust, lift, drag, and moment when we start tuning real aerodynamics.
  - HUD speed is not cosmetic; it verifies whether camera, lift, and landing classification are behaving.
- Control visuals:
  - Aileron/rudder/elevator animation should come after stable flight and landing. The pivots must be correct first.
- Camera:
  - Use a separate camera anchor concept, not a bare child camera.
  - Predict the target position using velocity and fixed timestep.
  - Smooth rotation with spherical interpolation.
  - Increase camera field of view with speed.
  - This directly replaces the earlier distant, flat-heading camera that made the aircraft tiny against empty sky.
- Missile:
  - Use forward target points, transform-based target tracking, smoke trail, collision, and explosion only for environmental incoming missiles.
  - Do not add player firing or weapon mode switching in the MVP.
- UI/audio:
  - Speed readout, targeting/warning indicator, engine pitch, lock warning, and explosion audio are part of the feel layer.

### YouTube Captions: Realistic Aircraft Physics For Games

Apply all major timestamp blocks:

- 00:00-00:59: An arcade controller can be better for a game, but realistic concepts still improve feel. MINgo stays arcade-first with selective realistic signals.
- 01:00-01:43: Use the four-force framing: gravity, thrust, lift, drag. Our current controller already has gravity, thrust, lift, and drag placeholders.
- 01:43-02:21: Break aircraft behavior into surfaces/elements later. MVP may use one body, but the architecture should not block per-surface forces.
- 02:21-04:00: Airflow speed, density, wing surface, and chord explain force scale. Future `FlightAerodynamics` should expose these parameters.
- 04:00-04:42: Coefficients depend on angle of attack, Reynolds number, and Mach number. For this project, use angle-of-attack curves only.
- 04:47-05:25: Lift rises at low angle, drag grows, and stall reduces lift/control. Add stall-like behavior as a gameplay feel feature before deep simulation.
- 05:48-06:27: Control surfaces affect coefficient curves. Our control input should eventually modify coefficient behavior, not only add direct torque.
- 06:35-07:15: Useful surface parameters include lift slope, skin friction, zero-lift angle, stall angle, flap fraction, and aspect ratio. Keep these as future tuning names.
- 07:17-08:33: Apply force and torque to a Rigidbody, including torque from force application offset. Future aircraft surfaces should use this.
- 08:33-09:49: Physics update jitter can happen because forces update only once per physics tick. If MINgo jitters after real aerodynamics, add a cheap prediction/substep pass before changing global physics timestep.

### YouTube Captions: Creating A Flight Simulator In Unity3D Part 1

Apply every major timestamp block as implementation constraints:

- 00:00-01:20: The target feel sits between Ace Combat/Project Wingman arcade and DCS-level simulation. MINgo should remain accessible, but the aircraft must create reasons to manage speed, altitude, and turn energy. Do not bury this under mission complexity.
- 01:22-02:06: Treat forces as imbalances: thrust against drag, lift against gravity, and turning from force/torque imbalance. Keep this readable in code rather than directly rotating transforms.
- 02:06-03:21: Lift depends on speed and angle of attack. The current arcade lift can stay for MVP, but Phase 6+ should replace it with local-velocity angle-of-attack measurement and a curve-driven lift coefficient.
- 03:23-03:39: Flaps are a low-speed takeoff/landing tool because they add lift and drag. This becomes a later landing-feel control, not a Phase 2 requirement.
- 03:36-03:57: Induced drag makes hard turns cost speed. This matters for canyon/ridge routes because repeated low-altitude turns should naturally create energy pressure.
- 03:54-05:25: Aileron, rudder, and elevator/stabilator visuals should eventually come from separate pivoted control-surface nodes. Current direct torque is acceptable while the blockout aircraft is still a toy.
- 05:41-07:22: High-speed turning needs a G limiter so control does not scale forever with speed. Future advanced flight should cap effective input by predicted G load, with different pitch-up and pitch-down limits.
- 07:24-07:45: The implementation principle is to fake as much as possible with simple formulas and hand-tuned parameters. This supports the current MINgo direction: small scripts, tunable forces, and playtest-driven values.
- 07:47-08:20: The update loop should measure state in the aircraft local frame first: world velocity, local velocity, local angular velocity, pitch AOA, yaw AOA, and G force from velocity delta.
- 08:20-08:33: Thrust is throttle `0..1` multiplied by max thrust and applied as Rigidbody force. Phase 1 already follows this pattern.
- 08:42-10:23: Do not rely on Unity Rigidbody drag for final flight feel. Use velocity squared times hand-tuned directional drag coefficients, with extra forward drag when airbrakes or flaps are active.
- 10:27-13:53: Lift can ignore air density and surface area for this game. Use velocity squared, an AOA coefficient curve, a lift-power tuning scalar, and induced drag based on the squared lift coefficient. Project velocity onto the lift plane so sideways flow does not create fake wing lift.
- 13:58-15:45: Steering can be faked by applying torque directly to the center of mass. Convert player input into target angular velocity, limit by turn acceleration, and reduce steering power at low speed.
- 15:46-16:06: If induced drag is missing, the aircraft can turn without paying speed. This is the exact failure mode to avoid once MINgo adds canyon time-attack style routes.
- 16:06-19:12: G limiting is predictive: estimate future G from angular velocity crossed with velocity, derive directional G limits, and scale player input by a `0..1` limiter value rather than abruptly clamping controls.
- 19:18-20:15: Energy management is kinetic speed plus potential altitude. Drag and induced drag remove speed; climbing stores energy as altitude; diving turns altitude back into speed; engines are the only net energy source.
- 20:17-20:27: Corner speed emerges where low-speed control authority and high-speed G limit meet. MINgo should tune toward a readable sweet spot instead of equally strong turning at all speeds.
- 20:32-21:12: Low induced drag makes a loose arcade aircraft. Higher induced drag makes a fast straight-line aircraft that bleeds speed in turns. This is a future tuning lever for making the mountain/canyon route feel dangerous.
- 21:17-21:32: The linked repo and playable build are reference material. Use the patterns, not the asset package, because MINgo must keep its own one-scene free-landing sandbox scope.

### Vazgriz Blog / Repo Reference

Use observed patterns as validated implementation guidance:

- The blog explicitly names the target systems we should eventually model: drag, lift, angle of attack, induced drag, G-force/G limiter, corner speed, and energy management.
- The repo groups aircraft tuning by lift, steering, drag, and misc landing/graphics fields. MINgo should preserve that grouping when `ArcadeAircraftController` grows or when a separate `FlightAerodynamics` script is introduced.
- The repo measures `Velocity`, `LocalVelocity`, `LocalAngularVelocity`, AOA, yaw AOA, and local G force before applying forces. MINgo should add these debug outputs before replacing the arcade lift model.
- The repo applies thrust, lift, steering, drag, and angular drag in `FixedUpdate`. This matches MINgo's current physics loop and confirms that Phase 2 landing should not move physics to `Update`.
- The repo's steering is intentionally fake and stable: target angular velocity plus acceleration-limited correction. This is the right direction for MINgo; a full per-surface torque sim is unnecessary for MVP.
- The repo has landing gear colliders and braking material, but no free-surface landing classifier. MINgo's Phase 2 should therefore add our own surface-tag classification instead of borrowing a crash-only landing model.
- The repo HUD uses speed, AOA, G-force, boresight, and velocity marker. MINgo Phase 4 should start with speed/altitude/state/context, then add AOA/G/velocity marker only after the flight model exposes those values.

### GitHub Reference Repo

Use observed patterns only:

- It confirms the blog's design: one controller handles force, camera, missiles, UI, audio, and gizmos.
- It confirms the camera recipe: camera anchor, velocity prediction, rotation smoothing, speed-based FOV.
- It confirms the HUD recipe: speed text and reticle target projection.
- It confirms the hazard recipe: missile start velocity inherits aircraft velocity, target transform tracking, smoke/explosion, and temporary effect cleanup.
- It also shows why we should not copy the architecture directly: it is a single large controller and contains implementation details that are too coupled for our MVP.

## Changes To Apply In MINgo

Immediate Phase 1:

- Camera must be close tail chase, not distant observation.
- Camera position uses predicted aircraft position.
- Camera rotation follows aircraft rotation smoothly.
- Camera FOV widens with speed.
- Early world must show a ground/horizon reference so flight is readable.

Phase 2-3:

- Treat every visible surface as a landing test surface.
- Build at least a 2000 x 2000 greybox play area.
- Add runway, road, field, ridge, canyon, water, and city-edge markers with surface tags.

Phase 4:

- Add speed/altitude/state HUD.
- Add optional reticle or center mark for flight readability.
- Add debug output for thrust/lift/drag/moment once forces are richer.

Phase 5:

- Restricted airspace uses incoming missile hazard only.
- Player cannot fire missiles in MVP.
- Add lock warning, smoke trail, avoidable incoming missile, damage/emergency-landing state.

Phase 6+:

- Replace blockout aircraft with a cleaned imported/generative model.
- Add pivoted control surface visuals.
- Add afterburner/engine audio tied to throttle/speed.
- Replace simple lift/drag with angle-of-attack coefficient curves when the arcade loop is already fun.

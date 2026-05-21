# Phase 05 Restricted Airspace Checkpoint

Date: 2026-05-21
Result: PASS for restricted-airspace hazard slice.

Automated tests:
- RED: `RestrictedAirspaceStateTests` failed to compile because `MINgo.Hazards` did not exist.
- GREEN: `MINgo.EditMode` targeted run passed 27/27.
- Result file: `Builds/TestResults/editmode-phase-05-green-2.xml`.

Implemented hazard loop:
- `RestrictedAirspaceState` handles `Outside`, `Warning`, `Locking`, `MissileLaunched`, and `Escaped`.
- Outer-zone entry shows `Restricted airspace`.
- Deep-zone dwell reaches `Lock-on` at 3 seconds and `Missile launched` at 6 seconds.
- Leaving before launch resolves to `Escaped` after 1.5 seconds.
- Leaving after launch keeps an active missile alive.

Implemented world objects:
- Mountain-side `Restricted Airspace` root.
- Hidden outer and deep trigger boxes.
- Radar dish, barracks, hangar, warning boundary posts, and missile launch point.
- Visible missile threat with trail, capped turn rate, 10-second timeout, and first-hit damage.

HUD connection:
- `FlightHud` warning text supports `Restricted airspace`, `Lock-on`, `Missile launched`, and `Escaped`.

Scope held:
- No player weapons.
- No base destruction.
- No combat mission chain.

Manual check proxy:
- Scene contains `Restricted Airspace`, `Restricted Outer Zone`, `Restricted Deep Lock Zone`, `Missile Launch Point`, and `Radar Dish`.
- `ArcadeAircraftController.ResolveMotionState` preserves `Damaged` while airborne so missile hits can create an emergency landing opportunity.

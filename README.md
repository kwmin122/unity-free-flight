# Unity Free Flight

Unity 6000.3 free-flight and landing sandbox prototype.

The MVP is planned as vertical slices:

1. Buildable Unity baseline.
2. Arcade takeoff and one-minute free flight.
3. Arbitrary landing classification and repeat takeoff.
4. Greybox coastal world with airport, roads, city edge, ridge, and canyon.
5. Minimal HUD with landing context labels.
6. Restricted airspace hazard with lock-on, missile evasion, damage, and emergency landing.
7. Built-player MVP verification.
8. GTA-like easy flight controls: W/S throttle, A/D assisted turn, Up/Down pitch, Q/E manual roll, R reset.
9. Seaplane blockout silhouette with pontoons, struts, cockpit canopy, and red-white reference colors.
10. Center flight reticle for GTA-like forward reference.

Primary docs:

- Spec: `docs/superpowers/specs/2026-05-21-free-flight-sandbox-design.md`
- Plan: `docs/superpowers/plans/2026-05-21-free-flight-vertical-slice-plan.md`

Open `Assets/Scenes/FreeFlightSandbox.unity` in Unity before pressing Play. If the scene is empty, run `MINgo > Rebuild Free Flight Sandbox Scene`.

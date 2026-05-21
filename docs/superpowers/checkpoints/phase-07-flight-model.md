# Phase 07 Flight Model Checkpoint

Date: 2026-05-21
Result: PASS for source-driven flight model upgrade.

Sources applied:
- Unity Rigidbody force application and Unity 6 `linearVelocity`.
- NASA lift equation framing: airspeed squared, lift coefficient, and reference surface simplification.
- JSBSim frame/reference concepts: local velocity, relative wind, angle of attack, lift and drag directions.
- Unity aircraft reference implementations for one-body arcade-first aerodynamics.

Implemented:
- Added `FlightAerodynamics` as pure rule code for AOA, lift coefficient, lift direction, lift force, and drag force.
- Replaced world-up lift in `ArcadeAircraftController` with aircraft/right-axis lift, so bank angle now affects lift direction.
- Added speed-squared drag and lift-coefficient-driven induced drag.
- Exposed runtime `AngleOfAttackDegrees` and `LiftCoefficient` for later HUD/debug tuning.
- Rebuilt `FreeFlightSandbox.unity` so the `Player Aircraft` uses `speedDrag: 0.012` and `inducedDrag: 0.04`.

TDD evidence:
- RED: `Builds/TestResults/editmode-flight-aero-red.xml` did not generate because compilation failed as expected; `/tmp/mingo-editmode-flight-aero-red.log` shows missing `FlightAerodynamics`.
- GREEN targeted: `Builds/TestResults/editmode-flight-aero-green.xml` passed 36/36 for `MINgo.EditMode`.

Verification:
- Full EditMode unfiltered: `Builds/TestResults/editmode-flight-aero-final.xml` passed 92/92, including MCP Unity package tests 56/56.
- macOS build: `/tmp/mingo-build-flight-aero-final.log` contains `Build Finished, Result: Success.`
- GUI launch smoke: built player opened with chase camera, HUD, visible horizon/ground reference, and aircraft in frame.

Open tuning:
- Full route playtest still needs human input for exact feel: takeoff roll, road landing, ridge landing, restricted-airspace evasion, and damaged emergency landing.
- Next flight feel pass should tune values only after a hand playtest, not add new systems.

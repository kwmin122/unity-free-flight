# Phase 21 Checkpoint: Easy Controls, Car, Map Pass

Date: 2026-05-22

## Scope

- Fix the user-reported aircraft issue where releasing W still preserved throttle.
- Keep the aircraft accessible with GTA-style keyboard intent: W power, S brake/slow, A/D turn, separate pitch.
- Add a first drivable car and vehicle switching.
- Improve the world with more readable road/landing routes and ground traversal targets.
- Document at least 10 searched references each for controls, map/world quality, and car implementation.

## Root Cause

`ArcadeAircraftController.UpdateThrottleForGtaHold` intentionally preserved the current throttle when throttle input was neutral. That matched a throttle-lever simulation, but it contradicted the requested GTA/easy keyboard feel. For this prototype, neutral input should glide and bleed power instead of holding full engine command forever.

## Changes

- Aircraft throttle now releases toward idle when W/S are not held.
- Added idle coast drag while airborne and not holding power.
- Added `acceptsInput` gates so only the active vehicle reads controls.
- Added `VehicleInputReader`, `ArcadeCarController`, and `PlayerVehicleSwitcher`.
- Added `F`/`Tab` switching between plane and car, with chase camera retargeting.
- Added parking lot, parking stalls, downtown boulevard, bridge, beach ramp, and mountain switchback roads.
- Updated HUD hint to name the easier control scheme.
- Expanded the research note with 12 control references, 12 map/world references, and 12 car references.

## Verification

- TDD red observed first for the throttle release and idle drag API before implementation.
- Targeted EditMode tests passed:
  - `AircraftThrottleResponseTests` 5/5
  - `ArcadeCarControllerTests` 3/3
  - `VehicleInputReaderTests` 1/1
  - `FreeFlightSceneContractTests` 12/12
- Full unfiltered EditMode passed: `/tmp/MINgo-editmode-full-3.xml`, 122/122 including 56/56 MCP Unity package tests.
- Full PlayMode passed: `/tmp/MINgo-playmode-full.xml`, 3/3.
- macOS player build succeeded: `Builds/macOS/MINgo.app`, 106 MB.

Note: Unity still logs a non-blocking Cloud Diagnostics symbol upload auth warning during build, but the player build exits successfully.

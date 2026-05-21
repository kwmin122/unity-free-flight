# Phase 01 Flight Feel Checkpoint

Date: 2026-05-21
Result: PASS
Build: `Builds/macOS/MINgo.app`

Evidence:
- Scene builder batch command exited with code 0 after adding the aircraft and chase camera.
- macOS build command exited with code 0.
- `Assets/Scenes/FreeFlightSandbox.unity` contains `Player Aircraft`.
- `Player Aircraft` has `ArcadeAircraftController`.
- `Main Camera` has `ChaseCameraRig`.
- Built app launched as `Builds/macOS/MINgo.app/Contents/MacOS/MINgo`.
- Smoke playtest screenshot confirmed the aircraft visible behind the runway.
- Throttle smoke test moved the aircraft away from the runway and into flight view.

Playtest:
- Takeoff from runway: PASS in smoke test.
- One-minute free flight: PASS. Built player stayed running through a 60-second throttle smoke, and the camera kept a readable close chase view.
- Camera readability: PASS after community-source retune to close tail chase.
- Main tuning concern: Future phases need HUD/debug readouts to make exact speed, altitude, and state easier to verify.
- Screenshot evidence:
  - `/tmp/mingo_close_chase_final_start.png`
  - `/tmp/mingo_close_chase_final_flight.png`
  - `/tmp/mingo_close_chase_final_60s.png`

Final tuning values:
- `maxThrust = 85`
- `lift = 0.55`
- `pitchTorque = 35`
- `rollTorque = 55`
- `yawTorque = 18`
- `stabilization = 3.5`
- `autoLevel = 6`
- `groundBrake = 16`
- `takeoffSpeed = 22`
- Camera source integration: close tail chase, predicted target position, smoothed aircraft rotation, speed-based FOV.

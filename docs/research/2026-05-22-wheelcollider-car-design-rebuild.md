# WheelCollider Car And Map Rebuild Notes

Date: 2026-05-22

Purpose: rebuild MINgo's car after playtest feedback that the first version felt hard to control and could fly off the road. This note records the sources used and the exact implementation decisions so the car remains a grounded Unity 3D vehicle, not a loose physics block.

## Diagnosis

- The first car used direct Rigidbody force/rotation with primitive visual colliders still mixed into the body.
- That made the car sensitive to small bumps, side contacts, and stacked colliders, so it could hop or feel airborne.
- The proper Unity vehicle baseline is four WheelColliders for tire contact/suspension, one simple chassis collider for the body, a low center of mass, and separate visual wheel meshes that follow `GetWorldPose`.

## Vehicle Simulation Sources

1. Unity Manual, WheelCollider vehicle tutorial: https://docs.unity3d.com/Manual/WheelColliderTutorial.html
2. Unity Manual, WheelCollider component reference: https://docs.unity3d.com/Manual/class-WheelCollider.html
3. Unity Scripting API, WheelCollider: https://docs.unity3d.com/ScriptReference/WheelCollider.html
4. Unity Scripting API, WheelCollider.GetWorldPose: https://docs.unity3d.com/ScriptReference/WheelCollider.GetWorldPose.html
5. Unity Scripting API, WheelCollider.GetGroundHit: https://docs.unity3d.com/ScriptReference/WheelCollider.GetGroundHit.html
6. Unity Scripting API, WheelFrictionCurve: https://docs.unity3d.com/ScriptReference/WheelFrictionCurve.html
7. Unity Scripting API, WheelCollider.forceAppPointDistance: https://docs.unity3d.com/ScriptReference/WheelCollider-forceAppPointDistance.html
8. Unity Scripting API, Rigidbody.centerOfMass: https://docs.unity3d.com/ScriptReference/Rigidbody-centerOfMass.html
9. Yarsa Labs, basic WheelCollider vehicle controller: https://blog.yarsalabs.com/unity-basic-vehicle-controller-wheel-colliders-tutorial/
10. SharpCoder, basic car controls tutorial for Unity: https://www.sharpcoderblog.com/blog/basic-car-controls-tutorial-for-unity
11. VionixStudio, Unity car controller using WheelCollider physics: https://vionixstudio.com/2022/10/13/unity-car-controller-using-wheel-collider-physics/
12. Unity Scripting API, WheelCollider.ConfigureVehicleSubsteps: https://docs.unity3d.com/ScriptReference/WheelCollider.ConfigureVehicleSubsteps.html

## Vehicle Design And Asset Pipeline Sources

1. Unity Manual, importing models: https://docs.unity3d.com/Manual/ImportingModelFiles.html
2. Unity Manual, FBX model import settings: https://docs.unity3d.com/Manual/FBXImporter-Model.html
3. Unity Manual, mesh colliders: https://docs.unity3d.com/Manual/mesh-colliders-introduction.html
4. Unity Manual, primitive and compound colliders: https://docs.unity3d.com/Manual/collider-types-introduction.html
5. Blender Manual, FBX import/export: https://docs.blender.org/manual/en/latest/addons/import_export/scene_fbx.html
6. Blender Manual, object origin: https://docs.blender.org/manual/en/latest/scene_layout/object/origin.html
7. Unity Manual, prefabs: https://docs.unity3d.com/Manual/Prefabs.html
8. Unity Manual, LOD Group: https://docs.unity3d.com/Manual/class-LODGroup.html
9. Unity Manual, asset workflow baseline: https://docs.unity3d.com/Manual/AssetWorkflow.html
10. BlenderNation, 3D modeling a low-poly car in Blender: https://www.blendernation.com/2024/06/14/full-tutorial-3d-modeling-a-low-poly-car-in-blender/
11. BlenderNation, low-poly vehicles tutorial: https://www.blendernation.com/2019/09/02/low-poly-vehicles-easy-blender-2-8-tutorial/
12. B3D101, low-poly car tutorial: https://b3d101.org/en/2.8/Learn/Blender2.8/car.html
13. GameDev Academy, low-poly 3D assets in Blender: https://gamedevacademy.org/blender-tutorial/
14. 80 Level, vehicle art production articles archive: https://80.lv/articles/vehicle/

## Imagegen Concepts Created

- World map concept: `Assets/MINgo/Art/Concepts/world-map-concept-v1.png`
  - Top-down original island layout with airport, downtown, coastal highway, marina, mountain switchbacks, canyon, lighthouse, and restricted radar base.
- Car reference sheet: `Assets/MINgo/Art/Concepts/car-reference-sheet-v1.png`
  - Red/white compact sport utility car with front, side, top, rear, and three-quarter views.

## Implementation Decisions Applied

- Car root:
  - One `Rigidbody`, mass 950.
  - One `BoxCollider` for the chassis.
  - Center of mass lowered to reduce rollovers and hopping.
- Wheel physics:
  - Four child `WheelCollider` objects named `Wheel Collider FL/FR/RL/RR`.
  - Tuned radius, spring, damper, suspension distance, force app point, forward friction, and sideways friction.
  - All visual wheel cylinders have their primitive colliders removed.
  - Visual wheels sync to physics wheels through `WheelCollider.GetWorldPose`.
- Handling:
  - `W` applies motor torque until max speed.
  - Releasing `W` applies light coasting/engine braking so the car does not keep accelerating or feel uncontrolled.
  - `S` brakes while moving forward at speed, then reverses below a forgiving low-speed threshold so the car behaves like a keyboard/GTA-style vehicle instead of feeling stuck.
  - Reverse steering is damped so holding `S+D` produces a controllable backward arc instead of immediately spinning the car around.
  - Steering angle reduces with speed.
  - Downforce and anti-roll force keep the car planted.
  - Player input wakes the Rigidbody and disables sleep threshold for the player car so a stopped vehicle always responds to `W`, `S`, and combined steering input.
  - The controller uses a grounded traction assist in addition to WheelCollider suspension. WheelColliders still handle contact, suspension, friction, and wheel pose, but keyboard play needs strong direct acceleration while the car is supported by wheels, by low ground height, or by a downward road raycast so `W` responds immediately from rest without assisting high airborne motion.
  - `W+D` and `S+D` must keep throttle/reverse and steering as separate simultaneous inputs, matching the common Unity vehicle pattern of applying vertical input to motor/brake torque and horizontal input to wheel steer angle in the same physics step.
  - WheelCollider motor torque is scaled to zero for the current blockout vehicle. The previous total-torque division and wheel-axis sign behavior made straight and reverse input unreliable. The WheelColliders remain responsible for contact, suspension, brake torque, friction, and visual wheel pose while grounded traction assist provides predictable keyboard drive.
  - Road paint, parking stall lines, runway threshold marks, and road center stripes are visual-only objects with no collider. They must not act like hidden curbs that block the car from driving straight.
  - The player car starts on the open airport ring road, facing along the road, so initial `W` input has enough runway to demonstrate movement before the player reaches any obstacle.
- Map:
  - The generated map concept was translated into actual scene objects: airport ring road, city connector, downtown roads, marina access road, lighthouse road, canyon hairpin roads, mountain switchback guardrails, and the existing airport/downtown/coast/bridge landmarks.

## Verification Criteria

- EditMode car math tests cover brake-before-reverse, reverse threshold, speed-sensitive steering, max-speed torque cut, handbrake torque, and coasting brake torque.
- Scene contract tests verify four WheelColliders, collider-free car visual parts, generated concept assets, and the expanded road loop.
- PlayMode car smoke test drives forward for several seconds and asserts speed gain, at least three grounded wheels, low vertical displacement, and limited roll angle.

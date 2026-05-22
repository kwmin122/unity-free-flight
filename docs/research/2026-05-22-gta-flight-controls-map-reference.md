# GTA Flight Controls And Map Reference Notes

Date: 2026-05-22

Purpose: ground the current MINgo control and map pass in observed GTA-style behavior without copying GTA assets, names, layout, logos, or proprietary art.

## Flight Controls

- GTA Wiki's GTA V controls table separates normal vehicles from aircraft:
  - Land/water vehicles: W/S are forward/backward acceleration.
  - Aircraft: W/S are aircraft throttle on/off.
  - A/D are aircraft yaw left/right.
  - Numpad keys handle roll and pitch.
- Sportskeeda's beginner PC flying guide describes the practical feel: hold W on land to move the plane forward for takeoff, use pitch input to lift, and press S after touchdown to stop.

Implementation decision:

- Do not make S a full air reverse. That reads like a car, but it is wrong for the aircraft fantasy.
- Make W feel immediate by ramping throttle rapidly toward full power while held.
- Make S feel useful by rapidly cutting throttle toward idle and applying the existing airbrake slowdown while airborne.
- Keep Space as the hard ground brake.

## Map Composition

Useful GTA-style world ingredients from the referenced map/landmark sources:

- A main airport near the southern urban edge.
- A downtown financial skyline with tall office towers.
- A western beach district with pier/boardwalk energy.
- A marina/harbor edge with boats and piers.
- Freeways and overpasses that read clearly from the air.
- Rural mountains, rivers, military/restricted areas, and large natural landmarks.

Implementation decision:

- Keep MINgo as one original map, not a GTA replica.
- Build readable silhouettes for flight: runway markings, terminal, glass towers, rooftop helipad, boardwalk, marina pier, boats, freeway overpass, palms, tree clusters, plaza sculpture.
- Treat every large shape as a navigation cue or landing temptation, not just decoration.

## Sources

- GTA Wiki, Controls for GTA V and GTA Online: https://gta.fandom.com/wiki/Controls_for_GTA_V
- Sportskeeda, Beginner's guide to flying planes in GTA 5 on PC: https://www.sportskeeda.com/gta/beginner-s-guide-flying-planes-gta-5-pc
- GTABase, GTA 5 Map & Locations Guide: https://www.gtabase.com/grand-theft-auto-v/map-locations/
- GTA V Wiki, Landmarks: https://gta5wiki.com/places/landmarks/
- Grand Theft Wiki, GTA V areas template: https://www.grandtheftwiki.com/Template%3AGtav_areas

## 2026-05-22 Follow-up Search Set

User request: driving is too hard, releasing W should not keep full power forever, the map looks poor, and cars should be added. Search target was at least 10 references per area.

### Controls / Flight Feel Sources

1. Rockstar Games, GTAV PC settings and control customization: https://www.rockstargames.com/newswire/article/51974aa3a724o2/rockstar-game-tips-tailoring-your-settings-and-controls-in
2. Rockstar Support, camera perspective default control: https://support.rockstargames.com/articles/4SLMPiZCrVUVcuL71dBD29/changing-camera-perspective-to-1st-person-in-gtav
3. GTA Wiki, aircraft controls table: https://gta.fandom.com/wiki/Controls_for_GTA_V
4. Unity Learn, Plane Programming challenge: https://learn.unity.com/tutorial/challenge-1-plane-programming
5. Unity Scripting API, `Rigidbody.AddForce`: https://docs.unity.cn/ScriptReference/Rigidbody.AddForce.html
6. Unity Scripting API, `Rigidbody.linearVelocity`: https://docs.unity.cn/6000.1/Documentation/ScriptReference/Rigidbody-linearVelocity.html
7. Unity Input System, `Key` enum / keyboard layout semantics: https://docs.unity.cn/Packages/com.unity.inputsystem%401.7/api/UnityEngine.InputSystem.Key.html
8. Unity Camera documentation, field of view and framing: https://docs.unity.com/en-us/unity-studio/develop/gameobjects/camera
9. Sharp Coder, Unity aeroplane controller overview: https://www.sharpcoderblog.com/blog/aeroplane-controller-for-unity
10. Catlike Coding, orbit camera and control readability: https://catlikecoding.com/unity/tutorials/movement/orbit-camera/
11. Vazgriz, flight simulator in Unity reference: https://vazgriz.com/346/flight-simulator-in-unity3d-part-1/
12. Vazgriz, FlightSim reference repo: https://github.com/vazgriz/FlightSim

Control conclusions applied:

- Keep aircraft as throttle-driven, not transform-teleport movement.
- `W` ramps throttle up, but neutral input now releases throttle toward idle instead of preserving the previous setting forever.
- `S` remains useful as throttle cut / braking, not an airborne reverse gear.
- Use `Space` for a stronger ground brake / hard brake because it is readable and avoids overloading S.
- Keep close third-person chase camera and speed FOV so the aircraft reads like a vehicle, not a tiny object in empty sky.

### Map / World Quality Sources

1. Unity Manual, world building: https://docs.unity.cn/2022.3/Documentation/Manual/CreatingEnvironments.html
2. Unity Learn, Terrain Editor: https://learn.unity.com/tutorial/working-with-the-terrain-editor-1
3. Unity Learn, Introduction to Terrain Editor: https://learn.unity.com/project/introduction-to-terrain-editor
4. EasyRoads3D terrain/road integration notes: https://www.unityterraintools.com/tutorials/terrain.php
5. ModDB, recreating a city environment in Unity: https://www.moddb.com/tutorials/recreating-a-real-life-city-environment-in-unity-an-indie-approach
6. GameDev Academy, open-world level design and landmark routing: https://gamedevacademy.org/level-design-open-world-tutorial/
7. Generalist Programmer, Unity procedural terrain generation: https://generalistprogrammer.com/game-design-development/unity-3d-procedural-terrain-generation/
8. UhiyamaLab, Unity terrain guide for large natural environments: https://uhiyama-lab.com/en/notes/unity/unity-terrain-guide/
9. GTABase, GTA 5 map/location categories: https://www.gtabase.com/grand-theft-auto-v/map-locations/
10. GTA V Wiki, landmarks: https://gta5wiki.com/places/landmarks/
11. GTA Wiki, Grand Theft Auto V world overview: https://gta.fandom.com/wiki/Grand_Theft_Auto_V
12. Wikipedia, GTA V development world research summary: https://en.wikipedia.org/wiki/Development_of_Grand_Theft_Auto_V

Map conclusions applied:

- The MVP should use big readable silhouettes before expensive art: skyline, airport, road loop, coastal route, bridge, mountain route, beach ramp, and parking area.
- Roads are gameplay surfaces, not just decoration. They create takeoff/landing and future car routes.
- Landmarks must be visible from the air and also usable as challenge prompts: bridge pass, road landing, beach ramp, mountain switchback, downtown edge, airport parking lot.
- This remains an original map inspired by open-world composition principles, not a copied GTA map.

### Car / Driving Sources

1. Unity Manual, create a vehicle with Wheel Colliders: https://docs.unity.cn/2020.3/Documentation/Manual/WheelColliderTutorial.html
2. Unity Manual, Wheel Collider component reference: https://docs.unity3d.com/Manual/class-WheelCollider.html
3. Unity Scripting API, `Rigidbody.AddForce`: https://docs.unity.cn/ScriptReference/Rigidbody.AddForce.html
4. Unity Scripting API, `Rigidbody.linearVelocity`: https://docs.unity.cn/6000.1/Documentation/ScriptReference/Rigidbody-linearVelocity.html
5. Yarsa Labs, basic WheelCollider vehicle controller: https://blog.yarsalabs.com/unity-basic-vehicle-controller-wheel-colliders-tutorial/
6. Sharp Coder, Unity car controls tutorial: https://www.sharpcoderblog.com/blog/basic-car-controls-tutorial-for-unity
7. VionixStudio, Unity car controller using WheelCollider physics: https://vionixstudio.com/2022/10/13/unity-car-controller-using-wheel-collider-physics/
8. Doofah, arcade bouncy vehicle physics tutorial: https://www.doofah.com/tutorials/unity/bouncy-vehicle-tutorial/
9. Yarsa Labs, arcade car controller force/friction idea: https://blog.yarsalabs.com/basic-2d-arcade-car-controller-in-unity/
10. Wayline, Rigidbody friction primer: https://www.wayline.io/blog/how-to-add-friction-to-a-rigidbody-in-unity
11. Unity Discussions, Unity 6 vehicle force behavior caution: https://discussions.unity.com/t/rigidbody-addforceatposition-not-working-after-updating-to-unity-6/1649536
12. Stack Overflow, Unity 6 `velocity` rename context: https://stackoverflow.com/questions/79437377/why-is-rigidbody-velocity-not-working-in-unity-version-6

Car conclusions applied:

- Superseded the first direct-Rigidbody arcade car after playtest feedback that it felt unstable and could leave the ground.
- Rebuilt the player car around four Unity WheelColliders, a single chassis collider, lowered Rigidbody center of mass, downforce, anti-roll force, tuned suspension, tuned tire friction, and visual wheel pose sync.
- `W` accelerates, neutral input applies light coasting/engine braking, and `S` brakes while moving forward before becoming reverse below a low speed threshold.
- Steering scales down at speed to prevent instant spin.
- Keep the `F`/`Tab` vehicle switcher so the chase camera retargets between plane and car.

# AI Airplane And World Art Pipeline

Date: 2026-05-21

Purpose: make the current imagegen assets usable in Unity now, and define the exact next pipeline for replacing the blockout aircraft with a generated 3D model.

## Current Project Assets

- World atlas: `Assets/MINgo/Art/Textures/world-material-atlas-v1.png`
- World atlas notes: `Assets/MINgo/Art/Textures/world-material-atlas-v1.md`
- Aircraft reference v1: `Assets/MINgo/Art/Concepts/seaplane-reference-sheet-v1.png`
- Aircraft reference v2: `Assets/MINgo/Art/Concepts/seaplane-reference-sheet-v2.png`
- Aircraft reference v2 notes: `Assets/MINgo/Art/Concepts/seaplane-reference-sheet-v2.md`

The current Unity scene already applies the atlas to the major playable surfaces: ocean, runway, road, open field, beach strip, mountain ridge, canyon, city edge blocks, and tree-line landmarks.

## Imagegen Prompts Used

World atlas:

```text
Create a clean 3x3 texture atlas for a stylized modern coastal open-world flight game. Top row: ocean water, asphalt road, airport runway. Middle row: green field grass, pale beach sand, gray mountain rock. Bottom row: red canyon rock, modern city building facade, dense tree canopy. Each tile should be square, seamless-looking, readable from a distance, no text, no logos, no copyrighted game branding, polished GTA-like open-world vibe but original.
```

Seaplane reference v2:

```text
Create a polished multi-view concept reference sheet for an original red-and-white civilian seaplane for a Unity free-flight landing game. Include front view, side view, top view, and rear three-quarter view on one clean white sheet. High wing, twin floats, visible wing struts, cockpit canopy, sturdy landing-friendly proportions, no text, no logos, no copied game branding.
```

## Meshy Path

Use this path when speed matters more than local-only generation.

1. Upload `seaplane-reference-sheet-v2.png` into Meshy Image to 3D.
2. If available, split or crop the sheet into 2-4 aligned views and use Multi-view. Front, side, rear/back, and top views are preferred.
3. Generate the model.
4. Export `GLB` for fastest Unity inspection, and `FBX` when animation/pivot cleanup is needed.
5. Download all texture maps if the export exposes them.
6. Move raw files into `Assets/Models/Airplane/Raw/`.
7. Open in Blender before prefab creation.

Why: Meshy documents Image-to-3D model URLs for `glb`, `fbx`, and `obj`, and its multi-view guidance recommends 2-4 photos/views for better full-geometry accuracy.

## Tripo Path

Use this path when Meshy output is poor or a second generator is needed.

1. Upload the same reference sheet or cropped views into Tripo image-to-model / multiview-to-model.
2. Convert/export to `GLB` first.
3. Use conversion options only when needed:
   - `quad` for auto-retopology.
   - `face_limit` around `10000-30000` for MVP.
   - `texture_format: PNG` for Unity-oriented FBX exports.
   - `pivot_to_center_bottom` only for props; for aircraft, verify the center of mass manually in Blender.
4. Move raw files into `Assets/Models/Airplane/Raw/`.

Why: Tripo's conversion API exposes remesh, face limit, texture format, scale, pivot, UV packing, baking, and export orientation controls. Those are exactly the cleanup knobs needed before Unity import.

## ComfyUI / Hunyuan3D Path

Use this path when local-first generation is preferred and the machine has a working ComfyUI 3D stack.

1. Load a Hunyuan3D-2 single-view or multi-view workflow in ComfyUI.
2. Use cropped reference views from `seaplane-reference-sheet-v2.png`.
3. Queue the workflow.
4. Collect generated `.glb` output from `ComfyUI/output/mesh`.
5. Import into Blender for the same cleanup pass below.

Boundary: this repository currently stores the input images and Unity-side contract. It does not claim that Meshy, Tripo, or local ComfyUI generated a finished aircraft mesh in this session because no authenticated generator export was verified here.

## Blender Cleanup Checklist

Before a generated mesh becomes a Unity prefab:

- Delete extra cameras, lights, ground planes, labels, or backing cards.
- Verify scale: one Unity unit should equal roughly one meter.
- Set forward axis so the nose points along Unity `+Z`.
- Put origin/pivot near the aircraft center of mass, not at the bottom of a float.
- Keep the floats, wing, tail, and fuselage symmetrical.
- Fix visibly warped wings, bent tail planes, or melted cockpit geometry.
- Merge or rename material slots into readable names: fuselage, wing, floats, canopy, propeller, trim.
- Reduce mesh density if it is too heavy for a wide open world.
- Export a cleaned `.fbx` or `.glb` to `Assets/Models/Airplane/Clean/`.

## Unity Import Checklist

1. Put the cleaned model under `Assets/Models/Airplane/Clean/`.
2. Create materials under `Assets/Models/Airplane/Materials/`.
3. Create a prefab under `Assets/MINgo/Prefabs/`.
4. Add colliders:
   - fuselage: capsule or box collider
   - wings: thin box colliders
   - floats: two box/capsule colliders for landing and water contact
5. Keep the existing Rigidbody and `ArcadeAircraftController` on the player root.
6. Swap visual children only first; do not change flight physics in the same commit.
7. Run:
   - targeted scene contract
   - full EditMode
   - full PlayMode
   - macOS build

## Acceptance Criteria For The First Mesh Commit

- The aircraft appears in Play Mode from the default scene without manual hierarchy work.
- The camera still frames the aircraft from behind, not from inside or far above.
- The aircraft collider can land on runway, road, field, ridge, canyon floor, and water.
- No generated asset keeps source-provider logos, copied GTA marks, random text, or broken reference-sheet remnants.
- The repo includes the raw generator export, cleaned export, prefab, and a short asset provenance note.

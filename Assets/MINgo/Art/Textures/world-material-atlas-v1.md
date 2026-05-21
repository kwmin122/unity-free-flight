# World Material Atlas v1

Purpose: first generated visual layer for the one-map free landing sandbox.

Generated source:
- Tool: built-in imagegen
- Saved asset: `Assets/MINgo/Art/Textures/world-material-atlas-v1.png`
- Prompt target: a 3x3 atlas with ocean, road, runway, grass, beach sand, mountain rock, canyon rock, city facade, and tree canopy tiles.

Tile order:

| Index | Tile | Unity use |
| --- | --- | --- |
| 0 | Ocean | `SurfaceKind.Water` |
| 1 | Road | `SurfaceKind.Road` |
| 2 | Runway | `SurfaceKind.Runway`, airport apron |
| 3 | Grass | `SurfaceKind.Field`, reference ground |
| 4 | Sand | beach emergency strip |
| 5 | Mountain | ridge walls, radar base terrain |
| 6 | Canyon | canyon walls and canyon floor |
| 7 | Building | city blocks, hangars, towers, barracks |
| 8 | Trees | tree-line landmarks |

Implementation:
- `FreeFlightSceneBuilder` loads this texture with `AssetDatabase.LoadAssetAtPath`.
- Each generated material uses `1/3 x 1/3` texture scale and per-tile offset.
- `FreeFlightSceneContractTests.SceneAppliesGeneratedWorldMaterialAtlas` protects the scene contract.

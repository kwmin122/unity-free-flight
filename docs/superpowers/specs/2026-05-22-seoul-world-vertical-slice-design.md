# Seoul World Vertical Slice Design

Date: 2026-05-22

## Goal

Turn the current generic open-world test map into a Seoul-inspired flight and driving sandbox where the player can navigate by the Hangang River, recognizable districts, and large landmark silhouettes.

## Scope

This phase is not the final full Seoul simulator. It is the first Seoul map slice that makes the world readable from the air and usable by car.

In scope:

- A wide east-west Hangang River axis through the center of the map.
- Driveable bridge and riverside road loop across the river.
- Seoul district clusters that are visually distinct from the air:
  - Yeouido financial island and park edge.
  - Banpo / Nodeul river landmark zone.
  - Namsan ridge with N Seoul Tower silhouette.
  - Gangnam / Samseong modern grid.
  - Jamsil / Lotte World Tower / Seokchon Lake zone.
  - Jongno / Gwanghwamun civic axis as a lower, denser north-side cluster.
- More detailed procedural building dressing: rooftop caps, window strips, glass towers, podium blocks, river park trees, bridge supports, and landmark beacons.
- EditMode tests proving the Seoul slice exists, has enough density, has driveable roads/bridges, and keeps water as a trigger.
- Regenerated `Assets/Scenes/FreeFlightSandbox.unity`.

Out of scope:

- Exact GIS/OSM mesh import.
- Traffic AI.
- Real copyrighted GTA assets.
- Meshy/Tripo/Blender model pipeline.
- Final imagegen facade pack for every building. This phase can use the existing atlas and procedural geometry; a later visual pass can replace materials with generated Seoul-specific PNG atlases.

## Source References

- Seoul Metropolitan Government describes Hangang as a central landmark and river corridor through Seoul, with bridges, parks, islands, and ecological/riverfront spaces: https://world.seoul.go.kr/service/amusement/hangang/overview/
- Seoul Metropolitan Government describes Hangang Parks as 11 parks with distinct scenery and facilities: https://english.seoul.go.kr/service/amusement/hangang/hangang-parks/
- Seoul Metropolitan Government highlights Banpo/Jamsugyo, Sevitseom, Nodeul Island, Yeouido Saetgang, and other river destinations as core Hangang places: https://english.seoul.go.kr/the-great-hangang-river/accessible-hangang-river/
- N Seoul Tower official overview states it has been a public Seoul attraction since 1980 and sits with the nature of Namsan: https://www.nseoultower.co.kr/eng/global/intro.asp
- Visit Seoul describes Seoul Sky as the observation deck at the 123-story Lotte World Tower: https://visit.seoul.kr/en/places/seoul-sky
- Seoul tourist guide material places Lotte World Tower in Jamsil and describes it as 123 floors and 555m tall: https://world.seoul.go.kr/wp-content/uploads/2025/01/2025-Tourist-guidebookENG.pdf

## Design Direction

Use an inspired miniature Seoul rather than exact scale. Exact coordinates are not necessary yet; what matters for the player is spatial legibility:

- The river runs east-west across the map center.
- North of the river: Namsan, Jongno/Gwanghwamun, lower civic blocks.
- West/center river: Yeouido island, finance towers, park edge, 63 Building-like tower.
- Center river: Banpo bridge, Nodeul Island, Sevitseom-like floating islands.
- South of the river: Gangnam boulevard grid and Samseong tower cluster.
- East/southeast: Jamsil lake loop and a very tall Lotte World Tower-like landmark.

The player should be able to fly over the river and instantly understand where to go next: tower, island, bridge, mountain, city grid, lake.

## Gameplay Requirements

- The river must create a visual route, not just blue filler.
- Bridges must be driveable road surfaces, not decorative-only blocks.
- At least one north-south bridge route must connect car travel between airport/city and Gangnam/Jamsil.
- Yeouido and Hangang park edges must give emergency landing/low flight targets.
- Namsan and Lotte/Jamsil landmarks must be visible from cruise altitude.
- The city must not be only three isolated cubes. The first slice should add enough density to read as a city grid from the air.

## Acceptance Criteria

Measured by EditMode scene contract tests after regenerating `FreeFlightSandbox.unity`:

- Scene contains named Seoul landmarks:
  - `Hangang River West`
  - `Hangang River East`
  - `Yeouido Island Park`
  - `Yeouido 63 Finance Tower`
  - `Banpo Bridge Road`
  - `Nodeul Island Park`
  - `Namsan Ridge`
  - `N Seoul Tower`
  - `Gangnam Boulevard`
  - `Jamsil Lotte World Tower`
  - `Seokchon Lake East`
- Water surfaces named `Hangang River West` and `Hangang River East` are trigger colliders.
- Bridge road objects have `SurfaceTag.kind == SurfaceKind.Road`.
- The Seoul slice has at least:
  - 70 objects with names beginning with `Seoul`
  - 20 objects with names containing `Gangnam`
  - 12 objects with names containing `Yeouido`
  - 10 objects with names containing `Jamsil`
  - 8 objects with names containing `Jongno`
- `Jamsil Lotte World Tower` is the tallest new tower and at least 180m tall in game scale.
- `N Seoul Tower` sits above the Namsan ridge and is at least 70m tall in game scale.
- At least four driveable Seoul bridge/river-road surfaces exist.

## Implementation Strategy

Keep the current generated-scene architecture:

- Add `CreateSeoulWorldSlice()` to `FreeFlightSceneBuilder`.
- Call it after base ground and before old generic city/mountain content so the existing MVP loops remain intact.
- Add small helper methods for reusable Seoul buildings:
  - detailed glass tower
  - apartment slab
  - road with lane markings
  - bridge with supports
  - park tree rows
  - landmark tower
- Do not import heavy packages in this phase.
- Do not split `FreeFlightSceneBuilder` yet; it is already the current pattern, and this slice can be added surgically.

## Verification

Run from a temp copy while the real Unity editor is open:

```bash
UNITY="/Applications/Unity/Hub/Editor/6000.3.11f1/Unity.app/Contents/MacOS/Unity"
PROJECT="/Users/a0000/Library/Mobile Documents/com~apple~CloudDocs/Desktop/dev/MINgo"
TMP="$(mktemp -d /tmp/MINgo-seoul-world.XXXXXX)"
rsync -a --delete --exclude Library --exclude Temp --exclude Logs --exclude UserSettings --exclude Builds --exclude .git --exclude '*.csproj' --exclude '*.slnx' "$PROJECT/" "$TMP/"
"$UNITY" -batchmode -nographics -quit -projectPath "$TMP" -executeMethod MINgo.EditorTools.FreeFlightSceneBuilder.RebuildScene -logFile "$TMP/rebuild.log"
"$UNITY" -batchmode -nographics -projectPath "$TMP" -runTests -testPlatform EditMode -testResults "$TMP/Builds/TestResults/editmode.xml" -logFile "$TMP/editmode.log"
"$UNITY" -batchmode -nographics -projectPath "$TMP" -runTests -testPlatform PlayMode -testResults "$TMP/Builds/TestResults/playmode.xml" -logFile "$TMP/playmode.log"
"$UNITY" -batchmode -nographics -quit -projectPath "$TMP" -executeMethod MINgo.EditorTools.MINgoBuildPipeline.BuildMacOSPlayer -logFile "$TMP/build.log"
```

Expected:

- EditMode passes.
- PlayMode movement tests still pass.
- macOS build succeeds.

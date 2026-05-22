# Seoul World Vertical Slice Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a Seoul-inspired Hangang-centered world slice to `FreeFlightSandbox.unity` without regressing vehicle movement.

**Architecture:** Keep the current generated-scene pattern in `FreeFlightSceneBuilder`. Add one Seoul slice entrypoint, reusable procedural-detail helpers, and EditMode contract tests that prove the scene contains readable landmarks, district density, driveable roads/bridges, and trigger water.

**Tech Stack:** Unity 6000.3.11f1, C#, Unity Test Framework, NUnit, current `MINgo.EditorTools`, `MINgo.Landing`, and generated `FreeFlightSandbox.unity`.

---

## File Structure

- Modify: `Assets/MINgo/Tests/EditMode/FreeFlightSceneContractTests.cs`
  - Add Seoul-specific scene contract tests before implementation.
- Modify: `Assets/MINgo/Editor/FreeFlightSceneBuilder.cs`
  - Add `CreateSeoulWorldSlice()` and small helper methods for detailed buildings, bridges, roads, parks, and landmarks.
- Modify: `Assets/Scenes/FreeFlightSandbox.unity`
  - Regenerate with the open Unity editor MCP menu item or batch builder.
- Create: `docs/superpowers/checkpoints/phase-24-seoul-world-slice.md`
  - Record root causes, design choices, and verification.

## Task 1: Add Failing Seoul Scene Contract Tests

**Files:**

- Modify: `Assets/MINgo/Tests/EditMode/FreeFlightSceneContractTests.cs`

- [ ] **Step 1: Add tests for landmarks, district density, and road/water contracts**

Insert after `SceneContainsVehicleSandboxRoadLoop()`:

```csharp
[Test]
public void SceneContainsSeoulHangangLandmarkSlice()
{
    string[] objectNames = Object.FindObjectsByType<Transform>(FindObjectsSortMode.None)
        .Select(transform => transform.name)
        .ToArray();

    Assert.That(objectNames, Does.Contain("Hangang River West"));
    Assert.That(objectNames, Does.Contain("Hangang River East"));
    Assert.That(objectNames, Does.Contain("Yeouido Island Park"));
    Assert.That(objectNames, Does.Contain("Yeouido 63 Finance Tower"));
    Assert.That(objectNames, Does.Contain("Banpo Bridge Road"));
    Assert.That(objectNames, Does.Contain("Nodeul Island Park"));
    Assert.That(objectNames, Does.Contain("Namsan Ridge"));
    Assert.That(objectNames, Does.Contain("N Seoul Tower"));
    Assert.That(objectNames, Does.Contain("Gangnam Boulevard"));
    Assert.That(objectNames, Does.Contain("Jamsil Lotte World Tower"));
    Assert.That(objectNames, Does.Contain("Seokchon Lake East"));
}

[Test]
public void SceneContainsDenseSeoulDistrictClusters()
{
    string[] objectNames = Object.FindObjectsByType<Transform>(FindObjectsSortMode.None)
        .Select(transform => transform.name)
        .ToArray();

    Assert.That(objectNames.Count(name => name.StartsWith("Seoul")), Is.GreaterThanOrEqualTo(70));
    Assert.That(objectNames.Count(name => name.Contains("Gangnam")), Is.GreaterThanOrEqualTo(20));
    Assert.That(objectNames.Count(name => name.Contains("Yeouido")), Is.GreaterThanOrEqualTo(12));
    Assert.That(objectNames.Count(name => name.Contains("Jamsil")), Is.GreaterThanOrEqualTo(10));
    Assert.That(objectNames.Count(name => name.Contains("Jongno")), Is.GreaterThanOrEqualTo(8));
}

[Test]
public void SeoulWaterAndBridgeContractsArePlayable()
{
    AssertWaterTrigger("Hangang River West");
    AssertWaterTrigger("Hangang River East");
    AssertRoadSurface("Banpo Bridge Road");
    AssertRoadSurface("Mapo Bridge Road");
    AssertRoadSurface("Jamsil Bridge Road");
    AssertRoadSurface("Olympic-daero Riverside Road");
    AssertRoadSurface("Gangbyeonbuk-ro Riverside Road");
}

[Test]
public void SeoulLandmarksHaveReadableScale()
{
    Assert.That(GameObject.Find("Jamsil Lotte World Tower").transform.localScale.y, Is.GreaterThanOrEqualTo(180f));
    Assert.That(GameObject.Find("N Seoul Tower").transform.position.y, Is.GreaterThanOrEqualTo(95f));
    Assert.That(GameObject.Find("N Seoul Tower").transform.localScale.y, Is.GreaterThanOrEqualTo(70f));
}
```

- [ ] **Step 2: Add test helpers**

Add near the bottom before `AssertVisualOnlyPart`:

```csharp
private static void AssertWaterTrigger(string objectName)
{
    GameObject water = GameObject.Find(objectName);
    Assert.That(water, Is.Not.Null, objectName);
    Assert.That(water.GetComponent<SurfaceTag>(), Is.Not.Null, objectName);
    Assert.That(water.GetComponent<SurfaceTag>().kind, Is.EqualTo(SurfaceKind.Water), objectName);
    Assert.That(water.GetComponent<Collider>().isTrigger, Is.True, objectName);
}

private static void AssertRoadSurface(string objectName)
{
    GameObject road = GameObject.Find(objectName);
    Assert.That(road, Is.Not.Null, objectName);
    Assert.That(road.GetComponent<SurfaceTag>(), Is.Not.Null, objectName);
    Assert.That(road.GetComponent<SurfaceTag>().kind, Is.EqualTo(SurfaceKind.Road), objectName);
    Assert.That(road.GetComponent<Collider>().isTrigger, Is.False, objectName);
}
```

- [ ] **Step 3: Run EditMode and verify RED**

Use a temp copy if the real editor is open:

```bash
UNITY="/Applications/Unity/Hub/Editor/6000.3.11f1/Unity.app/Contents/MacOS/Unity"
PROJECT="/Users/a0000/Library/Mobile Documents/com~apple~CloudDocs/Desktop/dev/MINgo"
TMP="$(mktemp -d /tmp/MINgo-seoul-red.XXXXXX)"
mkdir -p "$TMP/Builds/TestResults"
rsync -a --delete --exclude Library --exclude Temp --exclude Logs --exclude UserSettings --exclude Builds --exclude .git --exclude '*.csproj' --exclude '*.slnx' "$PROJECT/" "$TMP/"
"$UNITY" -batchmode -nographics -projectPath "$TMP" -runTests -testPlatform EditMode -testFilter "MINgo.Tests.FreeFlightSceneContractTests" -testResults "$TMP/Builds/TestResults/editmode-red.xml" -logFile "$TMP/editmode-red.log"
```

Expected: FAIL because the named Seoul objects do not exist yet.

- [ ] **Step 4: Commit failing tests**

```bash
git add Assets/MINgo/Tests/EditMode/FreeFlightSceneContractTests.cs
git commit -m "test: require seoul world slice"
```

## Task 2: Implement Seoul Slice in the Scene Builder

**Files:**

- Modify: `Assets/MINgo/Editor/FreeFlightSceneBuilder.cs`

- [ ] **Step 1: Call the new Seoul builder entrypoint**

In `RebuildScene()`, add this call after ground creation and before the existing old map sections:

```csharp
CreateSeoulWorldSlice();
```

- [ ] **Step 2: Add `CreateSeoulWorldSlice()` and district helpers**

Add methods near existing world creation methods:

```csharp
private static void CreateSeoulWorldSlice()
{
    CreateSeoulHangangAxis();
    CreateYeouidoDistrict();
    CreateBanpoNodeulZone();
    CreateNamsanJongnoDistrict();
    CreateGangnamDistrict();
    CreateJamsilDistrict();
}
```

Implement helper bodies with named objects matching the tests. Use `CreateLandingSurface` for roads/water/park emergency landing surfaces and `CreateBlock` / `CreateVisualBlock` for details.

- [ ] **Step 3: Add reusable helpers**

Add helper methods for:

```csharp
private static GameObject CreateSeoulRoad(string name, Vector3 position, Vector3 scale, float yawDegrees)
private static GameObject CreateSeoulBridge(string name, Vector3 position, Vector3 scale, float yawDegrees)
private static void CreateSeoulGlassTower(string name, Vector3 position, Vector3 scale, Color color)
private static void CreateSeoulApartmentSlab(string name, Vector3 position, Vector3 scale, Color color)
private static void CreateSeoulTreeRow(string name, Vector3 start, int count, Vector3 step)
```

Keep helpers deterministic and small; no random values.

- [ ] **Step 4: Run builder and EditMode**

```bash
UNITY="/Applications/Unity/Hub/Editor/6000.3.11f1/Unity.app/Contents/MacOS/Unity"
PROJECT="/Users/a0000/Library/Mobile Documents/com~apple~CloudDocs/Desktop/dev/MINgo"
TMP="$(mktemp -d /tmp/MINgo-seoul-green.XXXXXX)"
mkdir -p "$TMP/Builds/TestResults"
rsync -a --delete --exclude Library --exclude Temp --exclude Logs --exclude UserSettings --exclude Builds --exclude .git --exclude '*.csproj' --exclude '*.slnx' "$PROJECT/" "$TMP/"
"$UNITY" -batchmode -nographics -quit -projectPath "$TMP" -executeMethod MINgo.EditorTools.FreeFlightSceneBuilder.RebuildScene -logFile "$TMP/rebuild.log"
"$UNITY" -batchmode -nographics -projectPath "$TMP" -runTests -testPlatform EditMode -testFilter "MINgo.Tests.FreeFlightSceneContractTests" -testResults "$TMP/Builds/TestResults/editmode-green.xml" -logFile "$TMP/editmode-green.log"
```

Expected: PASS for `FreeFlightSceneContractTests`.

- [ ] **Step 5: Commit implementation**

```bash
git add Assets/MINgo/Editor/FreeFlightSceneBuilder.cs
git commit -m "feat: add seoul world slice"
```

## Task 3: Regenerate Scene and Verify Full Project

**Files:**

- Modify: `Assets/Scenes/FreeFlightSandbox.unity`
- Create: `docs/superpowers/checkpoints/phase-24-seoul-world-slice.md`

- [ ] **Step 1: Regenerate the real scene**

If Unity editor is open with MCP running:

```bash
node - <<'NODE'
const ws = new WebSocket('ws://localhost:8090/McpUnity?clientName=CodexSeoulSceneRebuild');
const timer = setTimeout(() => { console.error('timeout'); process.exit(2); }, 20000);
ws.addEventListener('open', () => {
  ws.send(JSON.stringify({ id: 'rebuild-seoul-scene', method: 'execute_menu_item', params: { menuPath: 'MINgo/Rebuild Free Flight Sandbox Scene' } }));
});
ws.addEventListener('message', (event) => {
  console.log(event.data.toString());
  clearTimeout(timer);
  ws.close();
});
ws.addEventListener('error', (event) => {
  console.error('websocket error', event.message || event.type || event);
  clearTimeout(timer);
  process.exit(1);
});
ws.addEventListener('close', () => process.exit(0));
NODE
```

- [ ] **Step 2: Trim Unity YAML trailing whitespace**

```bash
perl -pi -e 's/[ \t]+$//' Assets/Scenes/FreeFlightSandbox.unity
git diff --check
```

Expected: no output from `git diff --check`.

- [ ] **Step 3: Run full verification**

```bash
UNITY="/Applications/Unity/Hub/Editor/6000.3.11f1/Unity.app/Contents/MacOS/Unity"
PROJECT="/Users/a0000/Library/Mobile Documents/com~apple~CloudDocs/Desktop/dev/MINgo"
TMP="$(mktemp -d /tmp/MINgo-seoul-final.XXXXXX)"
mkdir -p "$TMP/Builds/TestResults"
rsync -a --delete --exclude Library --exclude Temp --exclude Logs --exclude UserSettings --exclude Builds --exclude .git --exclude '*.csproj' --exclude '*.slnx' "$PROJECT/" "$TMP/"
"$UNITY" -batchmode -nographics -projectPath "$TMP" -runTests -testPlatform EditMode -testResults "$TMP/Builds/TestResults/editmode.xml" -logFile "$TMP/editmode.log"
"$UNITY" -batchmode -nographics -projectPath "$TMP" -runTests -testPlatform PlayMode -testResults "$TMP/Builds/TestResults/playmode.xml" -logFile "$TMP/playmode.log"
"$UNITY" -batchmode -nographics -quit -projectPath "$TMP" -executeMethod MINgo.EditorTools.MINgoBuildPipeline.BuildMacOSPlayer -logFile "$TMP/build.log"
```

Expected:

- EditMode passed.
- PlayMode passed.
- Build log contains `Build Finished, Result: Success.`

- [ ] **Step 4: Write checkpoint**

Create `docs/superpowers/checkpoints/phase-24-seoul-world-slice.md` with:

- source references used
- what Seoul landmarks/districts were added
- verification temp path and results
- manual smoke path

- [ ] **Step 5: Commit and push**

```bash
git add Assets/Scenes/FreeFlightSandbox.unity docs/superpowers/checkpoints/phase-24-seoul-world-slice.md
git commit -m "chore: regenerate seoul world scene"
git push origin main
```

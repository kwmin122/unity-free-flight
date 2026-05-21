using UnityEditor;
using UnityEditor.SceneManagement;
using MINgo.Flight;
using MINgo.Hazards;
using MINgo.Landing;
using MINgo.UI;
using MINgo.World;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MINgo.EditorTools
{
    public static class FreeFlightSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/FreeFlightSandbox.unity";

        [MenuItem("MINgo/Rebuild Free Flight Sandbox Scene")]
        public static void RebuildScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            RenderSettings.skybox = MakeProceduralSkybox();
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.62f, 0.72f, 0.78f);
            RenderSettings.fogDensity = 0.0025f;
            RenderSettings.ambientLight = new Color(0.6f, 0.68f, 0.74f);

            var lightObject = new GameObject("Sun");
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            lightObject.transform.rotation = Quaternion.Euler(48f, -35f, 0f);

            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            var camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.Skybox;
            camera.fieldOfView = 62f;
            camera.nearClipPlane = 0.05f;
            camera.farClipPlane = 2600f;
            cameraObject.transform.position = new Vector3(0f, 18f, -38f);
            cameraObject.transform.rotation = Quaternion.Euler(18f, 0f, 0f);

            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "Flight Reference Ground";
            ground.transform.position = new Vector3(0f, -0.22f, 500f);
            ground.transform.localScale = new Vector3(2200f, 0.08f, 2200f);
            ground.GetComponent<Renderer>().sharedMaterial = MakeMaterial("Flight_Reference_Ground_Mat", new Color(0.36f, 0.52f, 0.4f));

            CreateAirport();
            CreateCoastline();
            CreateRoads();
            CreateFields();
            CreateCityEdge();
            CreateMountainRidge();
            CreateCanyonRoute();

            GameObject aircraft = CreateAircraft();
            var cameraRig = cameraObject.AddComponent<ChaseCameraRig>();
            cameraRig.target = aircraft.transform;
            CreateWorldBounds(aircraft);
            FlightHud hud = CreateHud(aircraft);
            CreateRestrictedAirspace(aircraft, hud);

            EditorSceneManager.SaveScene(scene, ScenePath);
            AddSceneToBuildSettings(ScenePath);
        }

        private static void CreateAirport()
        {
            CreateLandingSurface("Runway", SurfaceKind.Runway, Vector3.zero, new Vector3(18f, 0.25f, 220f), new Color(0.19f, 0.2f, 0.2f));
            CreateBlock("Runway Centerline", new Vector3(0f, 0.16f, 10f), new Vector3(1.2f, 0.03f, 170f), new Color(0.92f, 0.9f, 0.78f));
            CreateBlock("Airport Apron", new Vector3(-34f, 0f, -40f), new Vector3(58f, 0.16f, 58f), new Color(0.25f, 0.26f, 0.25f));
            CreateBlock("Hangar West", new Vector3(-72f, 6f, -70f), new Vector3(34f, 12f, 24f), new Color(0.48f, 0.52f, 0.54f));
            CreateBlock("Hangar East", new Vector3(-70f, 5f, -20f), new Vector3(28f, 10f, 20f), new Color(0.42f, 0.47f, 0.5f));
            CreateBlock("Control Tower", new Vector3(36f, 20f, -30f), new Vector3(8f, 40f, 8f), new Color(0.7f, 0.75f, 0.72f));
        }

        private static void CreateCoastline()
        {
            GameObject ocean = CreateLandingSurface("Ocean", SurfaceKind.Water, new Vector3(650f, -0.05f, 520f), new Vector3(680f, 1f, 1500f), new Color(0.12f, 0.33f, 0.58f));
            ocean.GetComponent<Collider>().isTrigger = true;
            CreateLandingSurface("Beach Emergency Strip", SurfaceKind.Field, new Vector3(310f, 0.03f, 330f), new Vector3(44f, 0.14f, 300f), new Color(0.76f, 0.68f, 0.48f));
            CreateBlock("Beach Sand Band", new Vector3(355f, -0.01f, 520f), new Vector3(95f, 0.1f, 1120f), new Color(0.69f, 0.62f, 0.43f));
        }

        private static void CreateRoads()
        {
            CreateLandingSurface("Coastal Road", SurfaceKind.Road, new Vector3(210f, 0.04f, 360f), new Vector3(10f, 0.16f, 560f), new Color(0.12f, 0.13f, 0.14f));
            GameObject airportRoad = CreateLandingSurface("Airport Service Road", SurfaceKind.Road, new Vector3(83f, 0.04f, 70f), new Vector3(12f, 0.16f, 230f), new Color(0.1f, 0.11f, 0.12f));
            airportRoad.transform.rotation = Quaternion.Euler(0f, -22f, 0f);
        }

        private static void CreateFields()
        {
            CreateLandingSurface("Open Field", SurfaceKind.Field, new Vector3(-115f, 0.02f, 260f), new Vector3(150f, 0.14f, 150f), new Color(0.28f, 0.48f, 0.27f));
            CreateLandingSurface("Long Meadow", SurfaceKind.Field, new Vector3(-220f, 0.02f, 165f), new Vector3(95f, 0.14f, 250f), new Color(0.34f, 0.55f, 0.3f));
            CreateBlock("Field Tree Line", new Vector3(-40f, 5f, 360f), new Vector3(12f, 10f, 170f), new Color(0.14f, 0.28f, 0.15f));
        }

        private static void CreateCityEdge()
        {
            for (int row = 0; row < 5; row++)
            {
                for (int col = 0; col < 5; col++)
                {
                    float height = 10f + ((row * 3 + col * 5) % 20);
                    Vector3 position = new Vector3(32f + col * 24f, height * 0.5f, 390f + row * 28f);
                    Vector3 scale = new Vector3(14f + (col % 2) * 5f, height, 12f + (row % 2) * 6f);
                    CreateBlock("City Edge Block " + row + "-" + col, position, scale, new Color(0.46f, 0.49f, 0.5f));
                }
            }

            CreateBlock("City Edge Marker Tower", new Vector3(166f, 31f, 515f), new Vector3(14f, 62f, 14f), new Color(0.56f, 0.58f, 0.6f));
        }

        private static void CreateMountainRidge()
        {
            for (int i = 0; i < 7; i++)
            {
                GameObject ridgeBlock = CreateBlock(
                    "Mountain Ridge Wall " + i,
                    new Vector3(-420f + i * 58f, 45f + i % 3 * 7f, 650f + i * 28f),
                    new Vector3(90f, 90f + i % 2 * 22f, 72f),
                    new Color(0.36f, 0.34f, 0.31f));
                ridgeBlock.transform.rotation = Quaternion.Euler(0f, 18f, -18f + i % 3 * 10f);
            }

            GameObject shelf = CreateLandingSurface("Ridge Landing Shelf", SurfaceKind.Ridge, new Vector3(-255f, 24f, 555f), new Vector3(105f, 0.3f, 48f), new Color(0.39f, 0.37f, 0.32f));
            shelf.transform.rotation = Quaternion.Euler(0f, 8f, 12f);
        }

        private static void CreateCanyonRoute()
        {
            CreateLandingSurface("Canyon Floor", SurfaceKind.CanyonFloor, new Vector3(250f, 0.03f, 690f), new Vector3(58f, 0.14f, 330f), new Color(0.46f, 0.34f, 0.24f));
            for (int i = 0; i < 6; i++)
            {
                CreateBlock("Canyon Left Wall " + i, new Vector3(198f, 28f, 555f + i * 58f), new Vector3(36f, 56f, 50f), new Color(0.42f, 0.31f, 0.23f));
                CreateBlock("Canyon Right Wall " + i, new Vector3(302f, 31f, 555f + i * 58f), new Vector3(40f, 62f, 50f), new Color(0.39f, 0.29f, 0.22f));
            }
        }

        private static void CreateWorldBounds(GameObject aircraft)
        {
            var boundsObject = new GameObject("World Bounds");
            var bounds = boundsObject.AddComponent<WorldBounds>();
            bounds.aircraft = aircraft.GetComponent<ArcadeAircraftController>();
            bounds.waterFailureHeight = -2f;
            bounds.resetPosition = new Vector3(0f, 2f, -65f);
            bounds.resetEulerAngles = Vector3.zero;
        }

        private static GameObject CreateAircraft()
        {
            var aircraft = new GameObject("Player Aircraft");
            aircraft.transform.position = new Vector3(0f, 2f, -65f);
            aircraft.transform.rotation = Quaternion.identity;

            var body = aircraft.AddComponent<Rigidbody>();
            body.mass = 4f;
            body.useGravity = true;

            CreateAircraftPart("Fuselage", aircraft.transform, new Vector3(0f, 0f, 0f), new Vector3(1.4f, 0.8f, 5.6f), new Color(0.85f, 0.87f, 0.82f));
            CreateAircraftPart("Wing", aircraft.transform, new Vector3(0f, 0f, -0.2f), new Vector3(8.5f, 0.18f, 1.3f), new Color(0.72f, 0.76f, 0.74f));
            CreateAircraftPart("Tail", aircraft.transform, new Vector3(0f, 0.55f, -2.45f), new Vector3(3.2f, 0.16f, 0.85f), new Color(0.66f, 0.7f, 0.72f));
            CreateAircraftPart("Nose", aircraft.transform, new Vector3(0f, 0.05f, 2.95f), new Vector3(0.9f, 0.55f, 0.9f), new Color(0.95f, 0.48f, 0.36f));

            aircraft.AddComponent<ArcadeAircraftController>();
            aircraft.AddComponent<LandingStateMachine>();
            return aircraft;
        }

        private static FlightHud CreateHud(GameObject aircraft)
        {
            var canvasObject = new GameObject("Flight HUD");
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;

            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObject.AddComponent<GraphicRaycaster>();

            var hud = canvasObject.AddComponent<FlightHud>();
            hud.aircraft = aircraft.GetComponent<ArcadeAircraftController>();
            hud.landing = aircraft.GetComponent<LandingStateMachine>();
            hud.speedText = CreateHudText("Speed", canvasObject.transform, new Vector2(28f, -28f), 22, Color.white);
            hud.altitudeText = CreateHudText("Altitude", canvasObject.transform, new Vector2(28f, -58f), 22, Color.white);
            hud.stateText = CreateHudText("State", canvasObject.transform, new Vector2(28f, -88f), 22, Color.white);
            hud.contextText = CreateHudText("Landing Context", canvasObject.transform, new Vector2(28f, -132f), 28, new Color(1f, 0.91f, 0.58f));
            hud.warningText = CreateHudText("Restricted Warning", canvasObject.transform, new Vector2(0f, -96f), 30, new Color(1f, 0.42f, 0.32f));
            hud.warningText.alignment = TextAnchor.UpperCenter;
            hud.warningText.rectTransform.anchorMin = new Vector2(0.5f, 1f);
            hud.warningText.rectTransform.anchorMax = new Vector2(0.5f, 1f);
            hud.warningText.rectTransform.pivot = new Vector2(0.5f, 1f);
            hud.warningText.rectTransform.sizeDelta = new Vector2(720f, 42f);
            hud.warningText.enabled = false;
            return hud;
        }

        private static Text CreateHudText(string name, Transform parent, Vector2 anchoredPosition, int fontSize, Color color)
        {
            var textObject = new GameObject(name);
            textObject.transform.SetParent(parent, false);

            var text = textObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.color = color;
            text.alignment = TextAnchor.UpperLeft;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.text = string.Empty;

            RectTransform rect = text.rectTransform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(520f, 36f);

            return text;
        }

        private static void CreateRestrictedAirspace(GameObject aircraft, FlightHud hud)
        {
            GameObject root = new GameObject("Restricted Airspace");
            root.transform.position = new Vector3(-470f, 48f, 790f);

            GameObject outer = CreateTriggerBox(
                "Restricted Outer Zone",
                root.transform,
                Vector3.zero,
                new Vector3(330f, 170f, 290f),
                new Color(0.75f, 0.58f, 0.18f, 0.22f));

            GameObject deep = CreateTriggerBox(
                "Restricted Deep Lock Zone",
                root.transform,
                new Vector3(0f, -2f, 0f),
                new Vector3(145f, 96f, 135f),
                new Color(0.9f, 0.18f, 0.12f, 0.3f));

            Transform spawn = CreateBlock(
                "Missile Launch Point",
                new Vector3(-500f, 55f, 790f),
                new Vector3(4f, 4f, 8f),
                new Color(0.65f, 0.12f, 0.08f)).transform;

            var zone = root.AddComponent<RestrictedAirspaceZone>();
            zone.aircraft = aircraft.GetComponent<ArcadeAircraftController>();
            zone.hud = hud;
            zone.outerZone = outer.GetComponent<Collider>();
            zone.deepZone = deep.GetComponent<Collider>();
            zone.missileSpawnPoint = spawn;

            CreateMilitaryBaseMarkers();
        }

        private static void CreateMilitaryBaseMarkers()
        {
            CreateBlock("Restricted Base Barracks", new Vector3(-458f, 38f, 762f), new Vector3(34f, 10f, 24f), new Color(0.25f, 0.29f, 0.28f));
            CreateBlock("Restricted Base Hangar", new Vector3(-512f, 40f, 824f), new Vector3(50f, 14f, 34f), new Color(0.31f, 0.33f, 0.32f));
            CreateBlock("Radar Tower", new Vector3(-476f, 58f, 795f), new Vector3(8f, 34f, 8f), new Color(0.44f, 0.46f, 0.43f));

            GameObject dish = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            dish.name = "Radar Dish";
            dish.transform.position = new Vector3(-476f, 78f, 795f);
            dish.transform.rotation = Quaternion.Euler(18f, -35f, 0f);
            dish.transform.localScale = new Vector3(22f, 4f, 22f);
            dish.GetComponent<Renderer>().sharedMaterial = MakeMaterial("Radar_Dish_Mat", new Color(0.58f, 0.6f, 0.56f));

            for (int i = 0; i < 8; i++)
            {
                float angle = i * 45f * Mathf.Deg2Rad;
                Vector3 position = new Vector3(-470f + Mathf.Cos(angle) * 165f, 10f, 790f + Mathf.Sin(angle) * 145f);
                CreateBlock("Restricted Boundary Post " + i, position, new Vector3(3f, 20f, 3f), new Color(0.95f, 0.72f, 0.12f));
            }
        }

        private static GameObject CreateTriggerBox(string name, Transform parent, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = name;
            box.transform.SetParent(parent, false);
            box.transform.localPosition = localPosition;
            box.transform.localScale = localScale;
            box.GetComponent<Collider>().isTrigger = true;
            Renderer renderer = box.GetComponent<Renderer>();
            renderer.sharedMaterial = MakeMaterial(name.Replace(" ", "_") + "_Mat", color);
            renderer.enabled = false;
            return box;
        }

        private static void CreateAircraftPart(string name, Transform parent, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject part = GameObject.CreatePrimitive(PrimitiveType.Cube);
            part.name = name;
            part.transform.SetParent(parent);
            part.transform.localPosition = localPosition;
            part.transform.localRotation = Quaternion.identity;
            part.transform.localScale = localScale;
            part.GetComponent<Renderer>().sharedMaterial = MakeMaterial(name + "_Mat", color);
        }

        private static GameObject CreateLandingSurface(string name, SurfaceKind kind, Vector3 position, Vector3 scale, Color color)
        {
            GameObject surface = GameObject.CreatePrimitive(PrimitiveType.Cube);
            surface.name = name;
            surface.transform.position = position;
            surface.transform.localScale = scale;
            surface.GetComponent<Renderer>().sharedMaterial = MakeMaterial(name.Replace(" ", "_") + "_Mat", color);
            surface.AddComponent<SurfaceTag>().kind = kind;
            return surface;
        }

        private static GameObject CreateBlock(string name, Vector3 position, Vector3 scale, Color color)
        {
            GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.name = name;
            block.transform.position = position;
            block.transform.localScale = scale;
            block.GetComponent<Renderer>().sharedMaterial = MakeMaterial(name.Replace(" ", "_") + "_Mat", color);
            return block;
        }

        private static Material MakeMaterial(string name, Color color)
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            material.name = name;
            material.color = color;
            return material;
        }

        private static Material MakeProceduralSkybox()
        {
            var material = new Material(Shader.Find("Skybox/Procedural"));
            material.name = "Procedural_Skybox_Mat";
            material.SetFloat("_SunSize", 0.04f);
            material.SetFloat("_AtmosphereThickness", 0.8f);
            material.SetColor("_SkyTint", new Color(0.55f, 0.7f, 0.9f));
            material.SetColor("_GroundColor", new Color(0.45f, 0.5f, 0.48f));
            return material;
        }

        private static void AddSceneToBuildSettings(string scenePath)
        {
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(scenePath, true)
            };
        }
    }
}

using UnityEditor;
using UnityEditor.SceneManagement;
using MINgo.Audio;
using MINgo.Flight;
using MINgo.Hazards;
using MINgo.Landing;
using MINgo.UI;
using MINgo.Vehicles;
using MINgo.World;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MINgo.EditorTools
{
    public static class FreeFlightSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/FreeFlightSandbox.unity";
        private const string WorldMaterialAtlasPath = "Assets/MINgo/Art/Textures/world-material-atlas-v1.png";

        private enum WorldAtlasTile
        {
            Ocean,
            Road,
            Runway,
            Grass,
            Sand,
            Mountain,
            Canyon,
            Building,
            Trees
        }

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
            cameraObject.AddComponent<AudioListener>();

            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "Flight Reference Ground";
            ground.transform.position = new Vector3(0f, -0.22f, 500f);
            ground.transform.localScale = new Vector3(2200f, 0.08f, 2200f);
            ground.GetComponent<Renderer>().sharedMaterial = MakeAtlasMaterial(
                "Flight_Reference_Ground_Mat",
                new Color(0.36f, 0.52f, 0.4f),
                WorldAtlasTile.Grass);

            CreateAirport();
            CreateCoastline();
            CreateRoads();
            CreateFields();
            CreateCityEdge();
            CreateMountainRidge();
            CreateCanyonRoute();
            CreateLandmarkBeacons();

            GameObject aircraft = CreateAircraft();
            GameObject car = CreatePlayerCar();
            var cameraRig = cameraObject.AddComponent<ChaseCameraRig>();
            cameraRig.target = aircraft.transform;
            cameraRig.followDistance = 8f;
            cameraRig.followHeight = 2.4f;
            cameraRig.lookAhead = 14f;
            cameraRig.lookHeight = 0.25f;
            cameraRig.pitchFollow = 0.22f;
            cameraRig.speedPullback = 2f;
            cameraRig.pullbackAtSpeed = 65f;
            cameraRig.smoothTime = 0.08f;
            cameraRig.rotationSmooth = 8f;
            cameraRig.minFieldOfView = 55f;
            cameraRig.maxFieldOfView = 66f;
            cameraRig.fieldOfViewAtSpeed = 85f;
            CreateWorldBounds(aircraft);
            FlightHud hud = CreateHud(aircraft);
            CreateRestrictedAirspace(aircraft, hud);
            CreateFlightAudio(aircraft);
            CreateVehicleSwitcher(aircraft, car, cameraRig);

            EditorSceneManager.SaveScene(scene, ScenePath);
            AddSceneToBuildSettings(ScenePath);
        }

        private static void CreateAirport()
        {
            CreateLandingSurface("Runway", SurfaceKind.Runway, Vector3.zero, new Vector3(18f, 0.25f, 220f), new Color(0.19f, 0.2f, 0.2f));
            CreateVisualBlock("Runway Centerline", new Vector3(0f, 0.16f, 10f), new Vector3(1.2f, 0.03f, 170f), new Color(0.92f, 0.9f, 0.78f));
            CreateVisualBlock("Runway Threshold Marking North", new Vector3(0f, 0.19f, 92f), new Vector3(14f, 0.03f, 2f), new Color(0.95f, 0.93f, 0.84f));
            CreateVisualBlock("Runway Threshold Marking South", new Vector3(0f, 0.19f, -92f), new Vector3(14f, 0.03f, 2f), new Color(0.95f, 0.93f, 0.84f));
            CreateBlock("Airport Apron", new Vector3(-34f, 0f, -40f), new Vector3(58f, 0.16f, 58f), new Color(0.25f, 0.26f, 0.25f));
            CreateBlock("Hangar West", new Vector3(-72f, 6f, -70f), new Vector3(34f, 12f, 24f), new Color(0.48f, 0.52f, 0.54f));
            CreateBlock("Hangar East", new Vector3(-70f, 5f, -20f), new Vector3(28f, 10f, 20f), new Color(0.42f, 0.47f, 0.5f));
            CreateBlock("Control Tower", new Vector3(36f, 20f, -30f), new Vector3(8f, 40f, 8f), new Color(0.7f, 0.75f, 0.72f));
            CreateBlock("Airport Glass Terminal", new Vector3(-42f, 6f, 28f), new Vector3(48f, 12f, 16f), new Color(0.42f, 0.55f, 0.62f));
            CreateBlock("Airport Terminal Window Strip", new Vector3(-42f, 9f, 36.3f), new Vector3(44f, 3f, 0.5f), new Color(0.14f, 0.28f, 0.42f));
            CreateLandingSurface("Airport Parking Lot", SurfaceKind.Road, new Vector3(-118f, 0.05f, 28f), new Vector3(62f, 0.16f, 48f), new Color(0.11f, 0.12f, 0.12f));
            CreateVisualBlock("Airport Parking Stall 0", new Vector3(-134f, 0.18f, 18f), new Vector3(2f, 0.03f, 18f), new Color(0.95f, 0.92f, 0.68f));
            CreateVisualBlock("Airport Parking Stall 1", new Vector3(-122f, 0.18f, 18f), new Vector3(2f, 0.03f, 18f), new Color(0.95f, 0.92f, 0.68f));
            CreateVisualBlock("Airport Parking Stall 2", new Vector3(-110f, 0.18f, 18f), new Vector3(2f, 0.03f, 18f), new Color(0.95f, 0.92f, 0.68f));
        }

        private static void CreateCoastline()
        {
            GameObject ocean = CreateLandingSurface("Ocean", SurfaceKind.Water, new Vector3(650f, -0.05f, 520f), new Vector3(680f, 1f, 1500f), new Color(0.12f, 0.33f, 0.58f));
            ocean.GetComponent<Collider>().isTrigger = true;
            CreateLandingSurface("Beach Emergency Strip", SurfaceKind.Field, new Vector3(310f, 0.03f, 330f), new Vector3(44f, 0.14f, 300f), new Color(0.76f, 0.68f, 0.48f));
            CreateBlock("Beach Sand Band", new Vector3(355f, -0.01f, 520f), new Vector3(95f, 0.1f, 1120f), new Color(0.69f, 0.62f, 0.43f));
            CreateLandingSurface("Beach Boardwalk", SurfaceKind.Road, new Vector3(335f, 0.08f, 300f), new Vector3(8f, 0.18f, 260f), new Color(0.55f, 0.42f, 0.27f));
            CreateLandingSurface("Marina Pier Main", SurfaceKind.Road, new Vector3(455f, 0.08f, 430f), new Vector3(120f, 0.18f, 8f), new Color(0.48f, 0.38f, 0.26f));
            CreateBlock("Marina Pier Finger 0", new Vector3(500f, 0.11f, 402f), new Vector3(8f, 0.16f, 48f), new Color(0.47f, 0.37f, 0.25f));
            CreateBlock("Marina Pier Finger 1", new Vector3(532f, 0.11f, 458f), new Vector3(8f, 0.16f, 48f), new Color(0.47f, 0.37f, 0.25f));
            CreateBlock("Marina White Yacht", new Vector3(526f, 1.1f, 402f), new Vector3(22f, 2.2f, 5f), new Color(0.88f, 0.9f, 0.86f));
            CreateBlock("Marina Red Speedboat", new Vector3(555f, 0.9f, 459f), new Vector3(14f, 1.8f, 4f), new Color(0.75f, 0.12f, 0.12f));
            CreatePalmTree("Coastal Palm 0", new Vector3(328f, 0.2f, 210f));
            CreatePalmTree("Coastal Palm 1", new Vector3(330f, 0.2f, 265f));
            CreatePalmTree("Coastal Palm 2", new Vector3(330f, 0.2f, 342f));
            CreatePalmTree("Coastal Palm 3", new Vector3(330f, 0.2f, 410f));
        }

        private static void CreateRoads()
        {
            CreateLandingSurface("Coastal Road", SurfaceKind.Road, new Vector3(210f, 0.04f, 360f), new Vector3(10f, 0.16f, 560f), new Color(0.12f, 0.13f, 0.14f));
            GameObject airportRoad = CreateLandingSurface("Airport Service Road", SurfaceKind.Road, new Vector3(83f, 0.04f, 70f), new Vector3(12f, 0.16f, 230f), new Color(0.1f, 0.11f, 0.12f));
            airportRoad.transform.rotation = Quaternion.Euler(0f, -22f, 0f);
            GameObject overpass = CreateLandingSurface("Freeway Overpass", SurfaceKind.Road, new Vector3(95f, 8f, 540f), new Vector3(18f, 0.28f, 220f), new Color(0.13f, 0.14f, 0.15f));
            overpass.transform.rotation = Quaternion.Euler(0f, 34f, 0f);
            CreateBlock("Freeway Overpass Support 0", new Vector3(58f, 4f, 474f), new Vector3(5f, 8f, 5f), new Color(0.42f, 0.43f, 0.4f));
            CreateBlock("Freeway Overpass Support 1", new Vector3(92f, 4f, 532f), new Vector3(5f, 8f, 5f), new Color(0.42f, 0.43f, 0.4f));
            CreateBlock("Freeway Overpass Support 2", new Vector3(126f, 4f, 590f), new Vector3(5f, 8f, 5f), new Color(0.42f, 0.43f, 0.4f));
            CreateBlock("Coastal Road Lane Stripe 0", new Vector3(210f, 0.16f, 220f), new Vector3(1f, 0.03f, 70f), new Color(0.9f, 0.86f, 0.55f));
            CreateBlock("Coastal Road Lane Stripe 1", new Vector3(210f, 0.16f, 390f), new Vector3(1f, 0.03f, 70f), new Color(0.9f, 0.86f, 0.55f));
            CreateLandingSurface("Downtown Boulevard", SurfaceKind.Road, new Vector3(95f, 0.06f, 430f), new Vector3(126f, 0.18f, 16f), new Color(0.1f, 0.105f, 0.11f));
            CreateLandingSurface("Coastal Highway Bridge", SurfaceKind.Road, new Vector3(298f, 3.2f, 435f), new Vector3(92f, 0.24f, 14f), new Color(0.15f, 0.16f, 0.17f));
            CreateLandingSurface("Beach Ramp", SurfaceKind.Road, new Vector3(270f, 1.4f, 372f), new Vector3(58f, 0.2f, 12f), new Color(0.18f, 0.17f, 0.15f)).transform.rotation = Quaternion.Euler(0f, 22f, 8f);
            CreateRoadSegment("Airport Ring Road South", new Vector3(-75f, 0.06f, -118f), new Vector3(170f, 0.18f, 11f), 0f);
            CreateRoadSegment("Airport Ring Road East", new Vector3(26f, 0.06f, -18f), new Vector3(11f, 0.18f, 190f), 0f);
            CreateRoadSegment("City Connector Road", new Vector3(34f, 0.07f, 250f), new Vector3(13f, 0.18f, 250f), -18f);
            CreateRoadSegment("Downtown Roundabout North", new Vector3(95f, 0.08f, 456f), new Vector3(70f, 0.18f, 12f), 0f);
            CreateRoadSegment("Downtown Roundabout South", new Vector3(95f, 0.08f, 404f), new Vector3(70f, 0.18f, 12f), 0f);
            CreateRoadSegment("Marina Access Road", new Vector3(410f, 0.06f, 486f), new Vector3(11f, 0.18f, 145f), 72f);
            CreateRoadSegment("Lighthouse Coastal Road", new Vector3(430f, 0.06f, 220f), new Vector3(12f, 0.18f, 170f), -30f);
            for (int i = 0; i < 4; i++)
            {
                GameObject switchback = CreateLandingSurface(
                    "Mountain Switchback Road " + i,
                    SurfaceKind.Road,
                    new Vector3(-285f + i * 48f, 9f + i * 5f, 440f + i * 42f),
                    new Vector3(80f, 0.18f, 12f),
                    new Color(0.12f, 0.115f, 0.105f));
                switchback.transform.rotation = Quaternion.Euler(0f, i % 2 == 0 ? 34f : -28f, 6f + i * 2f);
                CreateGuardrail("Mountain Switchback Guardrail " + i + " A", switchback.transform.position + new Vector3(0f, 0.85f, -7f), new Vector3(80f, 1.2f, 0.7f), switchback.transform.eulerAngles.y);
                CreateGuardrail("Mountain Switchback Guardrail " + i + " B", switchback.transform.position + new Vector3(0f, 0.85f, 7f), new Vector3(80f, 1.2f, 0.7f), switchback.transform.eulerAngles.y);
            }
        }

        private static void CreateFields()
        {
            CreateLandingSurface("Open Field", SurfaceKind.Field, new Vector3(-115f, 0.02f, 260f), new Vector3(150f, 0.14f, 150f), new Color(0.28f, 0.48f, 0.27f));
            CreateLandingSurface("Long Meadow", SurfaceKind.Field, new Vector3(-220f, 0.02f, 165f), new Vector3(95f, 0.14f, 250f), new Color(0.34f, 0.55f, 0.3f));
            CreateBlock("Field Tree Line", new Vector3(-40f, 5f, 360f), new Vector3(12f, 10f, 170f), new Color(0.14f, 0.28f, 0.15f));
            CreateTreeCluster("Meadow Tree Cluster 0", new Vector3(-180f, 0.1f, 320f));
            CreateTreeCluster("Meadow Tree Cluster 1", new Vector3(-72f, 0.1f, 228f));
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
            CreateModernDowntown();
        }

        private static void CreateModernDowntown()
        {
            CreateBlock("Downtown Glass Tower 0", new Vector3(92f, 36f, 462f), new Vector3(18f, 72f, 18f), new Color(0.32f, 0.48f, 0.58f));
            CreateBlock("Downtown Glass Tower 0 Window Band A", new Vector3(92f, 50f, 471.3f), new Vector3(16f, 2f, 0.5f), new Color(0.12f, 0.23f, 0.32f));
            CreateBlock("Downtown Glass Tower 0 Window Band B", new Vector3(92f, 34f, 471.3f), new Vector3(16f, 2f, 0.5f), new Color(0.12f, 0.23f, 0.32f));
            CreateBlock("Downtown Rooftop Helipad", new Vector3(92f, 72.4f, 462f), new Vector3(20f, 0.6f, 20f), new Color(0.12f, 0.14f, 0.15f));
            CreateBlock("Downtown Helipad Marking", new Vector3(92f, 72.8f, 462f), new Vector3(12f, 0.08f, 1.4f), new Color(0.92f, 0.9f, 0.76f));
            CreateBlock("Downtown Glass Tower 1", new Vector3(132f, 48f, 438f), new Vector3(22f, 96f, 16f), new Color(0.28f, 0.42f, 0.54f));
            CreateBlock("Downtown Glass Tower 2", new Vector3(158f, 31f, 472f), new Vector3(16f, 62f, 22f), new Color(0.36f, 0.48f, 0.5f));
            CreateBlock("Downtown Luxury Condo", new Vector3(62f, 24f, 430f), new Vector3(26f, 48f, 18f), new Color(0.62f, 0.63f, 0.58f));
            CreateBlock("City Plaza", new Vector3(96f, 0.09f, 410f), new Vector3(70f, 0.18f, 42f), new Color(0.34f, 0.35f, 0.33f));
            CreateBlock("City Plaza Sculpture", new Vector3(96f, 9f, 410f), new Vector3(4f, 18f, 4f), new Color(0.86f, 0.82f, 0.68f));
            CreateBlock("City Plaza Sculpture Crossbar", new Vector3(96f, 15f, 410f), new Vector3(20f, 3f, 3f), new Color(0.86f, 0.82f, 0.68f));
            CreateBlock("City Retail Pod 0", new Vector3(65f, 3f, 396f), new Vector3(16f, 6f, 10f), new Color(0.45f, 0.42f, 0.38f));
            CreateBlock("City Retail Pod 1", new Vector3(127f, 3f, 396f), new Vector3(16f, 6f, 10f), new Color(0.46f, 0.43f, 0.39f));
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
            for (int i = 0; i < 5; i++)
            {
                GameObject road = CreateLandingSurface(
                    "Canyon Hairpin Road " + i,
                    SurfaceKind.Road,
                    new Vector3(250f + (i % 2 == 0 ? -24f : 24f), 1.1f + i * 0.4f, 555f + i * 62f),
                    new Vector3(72f, 0.18f, 10f),
                    new Color(0.13f, 0.12f, 0.11f));
                road.transform.rotation = Quaternion.Euler(0f, i % 2 == 0 ? -31f : 29f, 0f);
            }

            for (int i = 0; i < 6; i++)
            {
                CreateBlock("Canyon Left Wall " + i, new Vector3(198f, 28f, 555f + i * 58f), new Vector3(36f, 56f, 50f), new Color(0.42f, 0.31f, 0.23f));
                CreateBlock("Canyon Right Wall " + i, new Vector3(302f, 31f, 555f + i * 58f), new Vector3(40f, 62f, 50f), new Color(0.39f, 0.29f, 0.22f));
            }
        }

        private static void CreateLandmarkBeacons()
        {
            CreateBeaconTower("Airport Beacon Tower", new Vector3(58f, 0f, -92f), 34f, new Color(0.88f, 0.18f, 0.14f), new Color(1f, 0.95f, 0.45f));
            CreateBeaconTower("Coastal Lighthouse", new Vector3(386f, 0f, 185f), 44f, new Color(0.9f, 0.86f, 0.74f), new Color(0.25f, 0.52f, 0.95f));
            CreateBeaconTower("Canyon Gate Beacon", new Vector3(250f, 0f, 520f), 38f, new Color(0.92f, 0.48f, 0.18f), new Color(1f, 0.76f, 0.24f));
            CreateBeaconTower("Ridge Summit Beacon", new Vector3(-255f, 25f, 555f), 30f, new Color(0.86f, 0.28f, 0.16f), new Color(1f, 0.82f, 0.28f));

            CreateBlock("Airport Beacon Base Stripe", new Vector3(58f, 18f, -92f), new Vector3(8f, 4f, 8f), new Color(0.95f, 0.95f, 0.9f));
            CreateBlock("Coastal Lighthouse Red Band", new Vector3(386f, 23f, 185f), new Vector3(12f, 5f, 12f), new Color(0.8f, 0.12f, 0.1f));
            CreateBlock("Canyon Gate Left Marker", new Vector3(226f, 10f, 520f), new Vector3(5f, 20f, 5f), new Color(0.95f, 0.68f, 0.18f));
            CreateBlock("Canyon Gate Right Marker", new Vector3(274f, 10f, 520f), new Vector3(5f, 20f, 5f), new Color(0.95f, 0.68f, 0.18f));
            CreateBlock("Ridge Summit Landing Flag", new Vector3(-235f, 45f, 552f), new Vector3(18f, 5f, 4f), new Color(0.95f, 0.76f, 0.2f));
        }

        private static void CreateBeaconTower(string name, Vector3 basePosition, float height, Color bodyColor, Color lightColor)
        {
            CreateBlock(name, basePosition + Vector3.up * (height * 0.5f), new Vector3(6f, height, 6f), bodyColor);
            CreateBlock(name + " Light", basePosition + Vector3.up * (height + 3f), new Vector3(14f, 6f, 14f), lightColor);
            CreateBlock(name + " Cap", basePosition + Vector3.up * (height + 7f), new Vector3(18f, 2.5f, 18f), new Color(0.12f, 0.13f, 0.12f));
        }

        private static void CreatePalmTree(string name, Vector3 basePosition)
        {
            CreateCylinderBlock(name + " Trunk", basePosition + new Vector3(0f, 5f, 0f), new Vector3(0.8f, 5f, 0.8f), new Color(0.46f, 0.31f, 0.18f), WorldAtlasTile.Trees);
            CreateBlock(name + " Crown A", basePosition + new Vector3(0f, 10.6f, 0f), new Vector3(8f, 1.4f, 2f), new Color(0.12f, 0.34f, 0.16f));
            CreateBlock(name + " Crown B", basePosition + new Vector3(0f, 10.7f, 0f), new Vector3(2f, 1.4f, 8f), new Color(0.12f, 0.34f, 0.16f));
            CreateBlock(name + " Crown C", basePosition + new Vector3(0f, 10.4f, 0f), new Vector3(6f, 1.1f, 6f), new Color(0.1f, 0.28f, 0.12f));
        }

        private static void CreateTreeCluster(string name, Vector3 basePosition)
        {
            for (int i = 0; i < 3; i++)
            {
                Vector3 offset = new Vector3(i * 5f, 0f, (i % 2) * 6f);
                CreateCylinderBlock(name + " Trunk " + i, basePosition + offset + new Vector3(0f, 3.2f, 0f), new Vector3(1.1f, 3.2f, 1.1f), new Color(0.34f, 0.23f, 0.13f), WorldAtlasTile.Trees);
                CreateSphereBlock(name + " Canopy " + i, basePosition + offset + new Vector3(0f, 8f, 0f), new Vector3(7f, 5f, 7f), new Color(0.1f, 0.31f, 0.14f), WorldAtlasTile.Trees);
            }
        }

        private static GameObject CreateRoadSegment(string name, Vector3 position, Vector3 scale, float yawDegrees)
        {
            GameObject road = CreateLandingSurface(name, SurfaceKind.Road, position, scale, new Color(0.1f, 0.105f, 0.11f));
            road.transform.rotation = Quaternion.Euler(0f, yawDegrees, 0f);

            GameObject stripe = CreateVisualBlock(name + " Center Stripe", position + Vector3.up * 0.12f, new Vector3(Mathf.Max(1f, scale.x * 0.05f), 0.03f, Mathf.Max(1f, scale.z * 0.72f)), new Color(0.92f, 0.86f, 0.52f));
            stripe.transform.rotation = road.transform.rotation;
            return road;
        }

        private static void CreateGuardrail(string name, Vector3 position, Vector3 scale, float yawDegrees)
        {
            GameObject rail = CreateBlock(name, position, scale, new Color(0.72f, 0.73f, 0.68f));
            rail.transform.rotation = Quaternion.Euler(0f, yawDegrees, 0f);
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

        private static void CreateFlightAudio(GameObject aircraft)
        {
            var audioObject = new GameObject("Flight Audio Rig");
            audioObject.transform.position = aircraft.transform.position;
            audioObject.transform.SetParent(aircraft.transform, true);

            var audio = audioObject.AddComponent<ProceduralFlightAudio>();
            audio.aircraft = aircraft.GetComponent<ArcadeAircraftController>();
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
            CreateAircraftVisualPart("Wing", aircraft.transform, new Vector3(0f, 0.72f, -0.2f), new Vector3(8.5f, 0.18f, 1.3f), new Color(0.72f, 0.76f, 0.74f));
            CreateAircraftVisualPart("Left Wing Tip Red", aircraft.transform, new Vector3(-4.5f, 0.72f, -0.2f), new Vector3(0.7f, 0.2f, 1.35f), new Color(0.88f, 0.12f, 0.1f));
            CreateAircraftVisualPart("Right Wing Tip Red", aircraft.transform, new Vector3(4.5f, 0.72f, -0.2f), new Vector3(0.7f, 0.2f, 1.35f), new Color(0.88f, 0.12f, 0.1f));
            CreateAircraftPhysicsCollider("Wing Physics Collider", aircraft.transform, new Vector3(0f, 0f, -0.2f), new Vector3(8.5f, 0.18f, 1.3f));
            CreateAircraftPhysicsCollider("Left Wing Tip Physics Collider", aircraft.transform, new Vector3(-4.5f, 0f, -0.2f), new Vector3(0.7f, 0.2f, 1.35f));
            CreateAircraftPhysicsCollider("Right Wing Tip Physics Collider", aircraft.transform, new Vector3(4.5f, 0f, -0.2f), new Vector3(0.7f, 0.2f, 1.35f));
            CreateAircraftVisualPart("High Wing Pylon", aircraft.transform, new Vector3(0f, 0.38f, 0.05f), new Vector3(0.3f, 0.72f, 0.82f), new Color(0.8f, 0.82f, 0.78f));
            CreateAircraftVisualPart("Left Wing Strut Front", aircraft.transform, new Vector3(-1.9f, 0.0f, 0.88f), new Vector3(0.12f, 1.65f, 0.12f), new Color(0.55f, 0.57f, 0.56f), new Vector3(0f, 0f, -28f));
            CreateAircraftVisualPart("Right Wing Strut Front", aircraft.transform, new Vector3(1.9f, 0.0f, 0.88f), new Vector3(0.12f, 1.65f, 0.12f), new Color(0.55f, 0.57f, 0.56f), new Vector3(0f, 0f, 28f));
            CreateAircraftVisualPart("Left Wing Strut Rear", aircraft.transform, new Vector3(-1.9f, 0.0f, -1.18f), new Vector3(0.12f, 1.65f, 0.12f), new Color(0.55f, 0.57f, 0.56f), new Vector3(0f, 0f, -28f));
            CreateAircraftVisualPart("Right Wing Strut Rear", aircraft.transform, new Vector3(1.9f, 0.0f, -1.18f), new Vector3(0.12f, 1.65f, 0.12f), new Color(0.55f, 0.57f, 0.56f), new Vector3(0f, 0f, 28f));
            CreateAircraftPart("Tail", aircraft.transform, new Vector3(0f, 0.55f, -2.45f), new Vector3(3.2f, 0.16f, 0.85f), new Color(0.66f, 0.7f, 0.72f));
            CreateAircraftPart("Tail Vertical Fin", aircraft.transform, new Vector3(0f, 1.05f, -2.6f), new Vector3(0.24f, 1.2f, 0.85f), new Color(0.74f, 0.78f, 0.78f));
            CreateAircraftPart("Tail Fin Red Stripe", aircraft.transform, new Vector3(0f, 1.34f, -2.58f), new Vector3(0.28f, 0.24f, 0.9f), new Color(0.88f, 0.12f, 0.1f));
            CreateAircraftPart("Nose", aircraft.transform, new Vector3(0f, 0.05f, 2.95f), new Vector3(0.9f, 0.55f, 0.9f), new Color(0.95f, 0.48f, 0.36f));
            CreateAircraftVisualPart("Propeller Hub", aircraft.transform, new Vector3(0f, 0.05f, 3.46f), new Vector3(0.42f, 0.42f, 0.2f), new Color(0.08f, 0.08f, 0.08f));
            CreateAircraftVisualPart("Propeller Blade Horizontal", aircraft.transform, new Vector3(0f, 0.05f, 3.58f), new Vector3(2.15f, 0.12f, 0.08f), new Color(0.12f, 0.12f, 0.12f));
            CreateAircraftVisualPart("Propeller Blade Vertical", aircraft.transform, new Vector3(0f, 0.05f, 3.59f), new Vector3(0.12f, 2.15f, 0.08f), new Color(0.12f, 0.12f, 0.12f));
            CreateAircraftPart("Cockpit Canopy", aircraft.transform, new Vector3(0f, 0.5f, 0.95f), new Vector3(1.0f, 0.3f, 0.95f), new Color(0.12f, 0.18f, 0.24f));
            CreateAircraftPart("Left Pontoon", aircraft.transform, new Vector3(-1.55f, -0.82f, 0.05f), new Vector3(0.55f, 0.32f, 4.8f), new Color(0.82f, 0.84f, 0.8f));
            CreateAircraftPart("Right Pontoon", aircraft.transform, new Vector3(1.55f, -0.82f, 0.05f), new Vector3(0.55f, 0.32f, 4.8f), new Color(0.82f, 0.84f, 0.8f));
            CreateAircraftPart("Left Pontoon Red Tip", aircraft.transform, new Vector3(-1.55f, -0.82f, 2.35f), new Vector3(0.58f, 0.34f, 0.42f), new Color(0.88f, 0.12f, 0.1f));
            CreateAircraftPart("Right Pontoon Red Tip", aircraft.transform, new Vector3(1.55f, -0.82f, 2.35f), new Vector3(0.58f, 0.34f, 0.42f), new Color(0.88f, 0.12f, 0.1f));
            CreateAircraftPart("Float Cross Strut Front", aircraft.transform, new Vector3(0f, -0.45f, 1.35f), new Vector3(3.45f, 0.1f, 0.12f), new Color(0.55f, 0.57f, 0.56f));
            CreateAircraftPart("Float Cross Strut Rear", aircraft.transform, new Vector3(0f, -0.45f, -1.45f), new Vector3(3.45f, 0.1f, 0.12f), new Color(0.55f, 0.57f, 0.56f));
            CreateAircraftPart("Left Float Strut Front", aircraft.transform, new Vector3(-0.78f, -0.45f, 1.35f), new Vector3(0.12f, 0.85f, 0.12f), new Color(0.55f, 0.57f, 0.56f));
            CreateAircraftPart("Right Float Strut Front", aircraft.transform, new Vector3(0.78f, -0.45f, 1.35f), new Vector3(0.12f, 0.85f, 0.12f), new Color(0.55f, 0.57f, 0.56f));
            CreateAircraftPart("Left Float Strut Rear", aircraft.transform, new Vector3(-0.78f, -0.45f, -1.45f), new Vector3(0.12f, 0.85f, 0.12f), new Color(0.55f, 0.57f, 0.56f));
            CreateAircraftPart("Right Float Strut Rear", aircraft.transform, new Vector3(0.78f, -0.45f, -1.45f), new Vector3(0.12f, 0.85f, 0.12f), new Color(0.55f, 0.57f, 0.56f));

            var controller = aircraft.AddComponent<ArcadeAircraftController>();
            controller.stabilization = 4.5f;
            controller.autoLevel = 6f;
            controller.autoLevelRotationRate = 2f;
            controller.assistedBankAngle = 22f;
            controller.throttleChangeRate = 3.2f;
            aircraft.AddComponent<LandingStateMachine>();
            return aircraft;
        }

        private static GameObject CreatePlayerCar()
        {
            var car = new GameObject("Player Car");
            car.transform.position = new Vector3(-145f, 0.18f, -118f);
            car.transform.rotation = Quaternion.Euler(0f, 90f, 0f);

            var body = car.AddComponent<Rigidbody>();
            body.mass = 950f;
            body.useGravity = true;
            body.centerOfMass = new Vector3(0f, -0.45f, 0.15f);

            var chassis = car.AddComponent<BoxCollider>();
            chassis.center = new Vector3(0f, 0.62f, 0f);
            chassis.size = new Vector3(2.15f, 1.05f, 4.35f);

            CreateCarPart("Car Body", car.transform, new Vector3(0f, 0.55f, 0f), new Vector3(2.2f, 0.62f, 4.35f), new Color(0.88f, 0.08f, 0.06f));
            CreateCarPart("Car Cabin", car.transform, new Vector3(0f, 1.02f, -0.3f), new Vector3(1.72f, 0.72f, 1.75f), new Color(0.08f, 0.12f, 0.15f));
            CreateCarPart("Car White Roof Panel", car.transform, new Vector3(0f, 1.41f, -0.34f), new Vector3(1.62f, 0.08f, 1.82f), new Color(0.92f, 0.92f, 0.87f));
            CreateCarPart("Car Hood White Stripe", car.transform, new Vector3(0f, 0.9f, 1.16f), new Vector3(0.48f, 0.08f, 1.55f), new Color(0.96f, 0.96f, 0.9f));
            CreateCarPart("Car Front Bumper", car.transform, new Vector3(0f, 0.43f, 2.27f), new Vector3(2.18f, 0.28f, 0.22f), new Color(0.05f, 0.055f, 0.06f));
            CreateCarPart("Car Rear Bumper", car.transform, new Vector3(0f, 0.43f, -2.27f), new Vector3(2.18f, 0.28f, 0.22f), new Color(0.05f, 0.055f, 0.06f));
            CreateCarPart("Car Front Grille", car.transform, new Vector3(0f, 0.65f, 2.41f), new Vector3(1.35f, 0.28f, 0.08f), new Color(0.02f, 0.02f, 0.025f));
            CreateCarPart("Car Left Headlight", car.transform, new Vector3(-0.73f, 0.75f, 2.43f), new Vector3(0.45f, 0.18f, 0.06f), new Color(0.92f, 0.94f, 0.86f));
            CreateCarPart("Car Right Headlight", car.transform, new Vector3(0.73f, 0.75f, 2.43f), new Vector3(0.45f, 0.18f, 0.06f), new Color(0.92f, 0.94f, 0.86f));
            CreateCarPart("Car Left Tail Light", car.transform, new Vector3(-0.78f, 0.72f, -2.43f), new Vector3(0.42f, 0.18f, 0.06f), new Color(0.95f, 0.05f, 0.04f));
            CreateCarPart("Car Right Tail Light", car.transform, new Vector3(0.78f, 0.72f, -2.43f), new Vector3(0.42f, 0.18f, 0.06f), new Color(0.95f, 0.05f, 0.04f));
            CreateCarPart("Car Left Side Skirt", car.transform, new Vector3(-1.14f, 0.42f, 0f), new Vector3(0.12f, 0.22f, 3.7f), new Color(0.04f, 0.045f, 0.05f));
            CreateCarPart("Car Right Side Skirt", car.transform, new Vector3(1.14f, 0.42f, 0f), new Vector3(0.12f, 0.22f, 3.7f), new Color(0.04f, 0.045f, 0.05f));

            Transform frontLeftVisual = CreateCarWheel("Front Left Wheel Visual", car.transform, new Vector3(-1.17f, 0.36f, 1.38f));
            Transform frontRightVisual = CreateCarWheel("Front Right Wheel Visual", car.transform, new Vector3(1.17f, 0.36f, 1.38f));
            Transform rearLeftVisual = CreateCarWheel("Rear Left Wheel Visual", car.transform, new Vector3(-1.17f, 0.36f, -1.38f));
            Transform rearRightVisual = CreateCarWheel("Rear Right Wheel Visual", car.transform, new Vector3(1.17f, 0.36f, -1.38f));

            WheelCollider frontLeftWheel = CreateWheelCollider("Wheel Collider FL", car.transform, new Vector3(-1.17f, 0.36f, 1.38f));
            WheelCollider frontRightWheel = CreateWheelCollider("Wheel Collider FR", car.transform, new Vector3(1.17f, 0.36f, 1.38f));
            WheelCollider rearLeftWheel = CreateWheelCollider("Wheel Collider RL", car.transform, new Vector3(-1.17f, 0.36f, -1.38f));
            WheelCollider rearRightWheel = CreateWheelCollider("Wheel Collider RR", car.transform, new Vector3(1.17f, 0.36f, -1.38f));

            var controller = car.AddComponent<ArcadeCarController>();
            controller.acceptsInput = false;
            controller.motorTorque = 1450f;
            controller.reverseTorque = 950f;
            controller.wheelMotorTorqueScale = 0f;
            controller.driveAssistAcceleration = 34f;
            controller.reverseAssistAcceleration = 34f;
            controller.reverseThreshold = 4.5f;
            controller.lowGroundSupportHeight = 1.5f;
            controller.frontLeftWheel = frontLeftWheel;
            controller.frontRightWheel = frontRightWheel;
            controller.rearLeftWheel = rearLeftWheel;
            controller.rearRightWheel = rearRightWheel;
            controller.frontLeftVisual = frontLeftVisual;
            controller.frontRightVisual = frontRightVisual;
            controller.rearLeftVisual = rearLeftVisual;
            controller.rearRightVisual = rearRightVisual;
            return car;
        }

        private static void CreateVehicleSwitcher(GameObject aircraft, GameObject car, ChaseCameraRig cameraRig)
        {
            var switcherObject = new GameObject("Player Vehicle Switcher");
            var switcher = switcherObject.AddComponent<PlayerVehicleSwitcher>();
            switcher.aircraft = aircraft.GetComponent<ArcadeAircraftController>();
            switcher.car = car.GetComponent<ArcadeCarController>();
            switcher.cameraRig = cameraRig;
            switcher.startInAircraft = true;
        }

        private static void CreateCarPart(string name, Transform parent, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject part = GameObject.CreatePrimitive(PrimitiveType.Cube);
            part.name = name;
            part.transform.SetParent(parent);
            part.transform.localPosition = localPosition;
            part.transform.localRotation = Quaternion.identity;
            part.transform.localScale = localScale;
            part.GetComponent<Renderer>().sharedMaterial = MakeAtlasMaterial(name + "_Mat", color, WorldAtlasTile.Building);
            Object.DestroyImmediate(part.GetComponent<Collider>());
        }

        private static Transform CreateCarWheel(string name, Transform parent, Vector3 localPosition)
        {
            GameObject wheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            wheel.name = name;
            wheel.transform.SetParent(parent);
            wheel.transform.localPosition = localPosition;
            wheel.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            wheel.transform.localScale = new Vector3(0.36f, 0.2f, 0.36f);
            wheel.GetComponent<Renderer>().sharedMaterial = MakeMaterial(name + "_Mat", new Color(0.05f, 0.05f, 0.05f));
            Object.DestroyImmediate(wheel.GetComponent<Collider>());
            CreateCarWheelHub(name + " Hub", parent, localPosition);
            return wheel.transform;
        }

        private static void CreateCarWheelHub(string name, Transform parent, Vector3 localPosition)
        {
            GameObject hub = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            hub.name = name;
            hub.transform.SetParent(parent);
            hub.transform.localPosition = localPosition;
            hub.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            hub.transform.localScale = new Vector3(0.2f, 0.22f, 0.2f);
            hub.GetComponent<Renderer>().sharedMaterial = MakeMaterial(name + "_Mat", new Color(0.42f, 0.43f, 0.42f));
            Object.DestroyImmediate(hub.GetComponent<Collider>());
        }

        private static WheelCollider CreateWheelCollider(string name, Transform parent, Vector3 localPosition)
        {
            var wheelObject = new GameObject(name);
            wheelObject.transform.SetParent(parent);
            wheelObject.transform.localPosition = localPosition;
            wheelObject.transform.localRotation = Quaternion.identity;
            var wheel = wheelObject.AddComponent<WheelCollider>();
            wheel.mass = 24f;
            wheel.radius = 0.36f;
            wheel.wheelDampingRate = 0.8f;
            wheel.suspensionDistance = 0.18f;
            wheel.forceAppPointDistance = 0.22f;

            JointSpring spring = wheel.suspensionSpring;
            spring.spring = 28000f;
            spring.damper = 4200f;
            spring.targetPosition = 0.55f;
            wheel.suspensionSpring = spring;

            WheelFrictionCurve forward = wheel.forwardFriction;
            forward.extremumSlip = 0.35f;
            forward.extremumValue = 1f;
            forward.asymptoteSlip = 0.8f;
            forward.asymptoteValue = 0.65f;
            forward.stiffness = 1.25f;
            wheel.forwardFriction = forward;

            WheelFrictionCurve sideways = wheel.sidewaysFriction;
            sideways.extremumSlip = 0.25f;
            sideways.extremumValue = 1f;
            sideways.asymptoteSlip = 0.55f;
            sideways.asymptoteValue = 0.72f;
            sideways.stiffness = 1.35f;
            wheel.sidewaysFriction = sideways;

            return wheel;
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
            hud.reticleText = CreateReticleText(canvasObject.transform);
            hud.warningText = CreateHudText("Restricted Warning", canvasObject.transform, new Vector2(0f, -96f), 30, new Color(1f, 0.42f, 0.32f));
            hud.warningText.alignment = TextAnchor.UpperCenter;
            hud.warningText.rectTransform.anchorMin = new Vector2(0.5f, 1f);
            hud.warningText.rectTransform.anchorMax = new Vector2(0.5f, 1f);
            hud.warningText.rectTransform.pivot = new Vector2(0.5f, 1f);
            hud.warningText.rectTransform.sizeDelta = new Vector2(720f, 42f);
            hud.warningText.enabled = false;
            return hud;
        }

        private static Text CreateReticleText(Transform parent)
        {
            Text reticle = CreateHudText("Flight Reticle", parent, Vector2.zero, 34, new Color(1f, 1f, 1f, 0.8f));
            reticle.text = "+";
            reticle.alignment = TextAnchor.MiddleCenter;
            reticle.raycastTarget = false;

            RectTransform rect = reticle.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, 0f);
            rect.sizeDelta = new Vector2(52f, 52f);

            return reticle;
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

        private static GameObject CreateAircraftPart(string name, Transform parent, Vector3 localPosition, Vector3 localScale, Color color)
        {
            return CreateAircraftPart(name, parent, localPosition, localScale, color, Vector3.zero);
        }

        private static GameObject CreateAircraftPart(string name, Transform parent, Vector3 localPosition, Vector3 localScale, Color color, Vector3 localEulerAngles)
        {
            GameObject part = GameObject.CreatePrimitive(PrimitiveType.Cube);
            part.name = name;
            part.transform.SetParent(parent);
            part.transform.localPosition = localPosition;
            part.transform.localRotation = Quaternion.Euler(localEulerAngles);
            part.transform.localScale = localScale;
            part.GetComponent<Renderer>().sharedMaterial = MakeMaterial(name + "_Mat", color);
            return part;
        }

        private static void CreateAircraftVisualPart(string name, Transform parent, Vector3 localPosition, Vector3 localScale, Color color)
        {
            CreateAircraftVisualPart(name, parent, localPosition, localScale, color, Vector3.zero);
        }

        private static void CreateAircraftVisualPart(string name, Transform parent, Vector3 localPosition, Vector3 localScale, Color color, Vector3 localEulerAngles)
        {
            GameObject part = CreateAircraftPart(name, parent, localPosition, localScale, color, localEulerAngles);
            Object.DestroyImmediate(part.GetComponent<Collider>());
        }

        private static void CreateAircraftPhysicsCollider(string name, Transform parent, Vector3 localPosition, Vector3 localScale)
        {
            GameObject part = CreateAircraftPart(name, parent, localPosition, localScale, new Color(0f, 0f, 0f, 0f));
            part.GetComponent<Renderer>().enabled = false;
        }

        private static GameObject CreateLandingSurface(string name, SurfaceKind kind, Vector3 position, Vector3 scale, Color color)
        {
            GameObject surface = GameObject.CreatePrimitive(PrimitiveType.Cube);
            surface.name = name;
            surface.transform.position = position;
            surface.transform.localScale = scale;
            surface.GetComponent<Renderer>().sharedMaterial = MakeAtlasMaterial(
                name.Replace(" ", "_") + "_Mat",
                color,
                GetSurfaceTile(kind));
            surface.AddComponent<SurfaceTag>().kind = kind;
            return surface;
        }

        private static GameObject CreateBlock(string name, Vector3 position, Vector3 scale, Color color)
        {
            GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.name = name;
            block.transform.position = position;
            block.transform.localScale = scale;
            block.GetComponent<Renderer>().sharedMaterial = MakeAtlasMaterial(
                name.Replace(" ", "_") + "_Mat",
                color,
                GetBlockTile(name));
            return block;
        }

        private static GameObject CreateVisualBlock(string name, Vector3 position, Vector3 scale, Color color)
        {
            GameObject block = CreateBlock(name, position, scale, color);
            Object.DestroyImmediate(block.GetComponent<Collider>());
            return block;
        }

        private static GameObject CreateCylinderBlock(string name, Vector3 position, Vector3 scale, Color color, WorldAtlasTile tile)
        {
            GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            block.name = name;
            block.transform.position = position;
            block.transform.localScale = scale;
            block.GetComponent<Renderer>().sharedMaterial = MakeAtlasMaterial(
                name.Replace(" ", "_") + "_Mat",
                color,
                tile);
            return block;
        }

        private static GameObject CreateSphereBlock(string name, Vector3 position, Vector3 scale, Color color, WorldAtlasTile tile)
        {
            GameObject block = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            block.name = name;
            block.transform.position = position;
            block.transform.localScale = scale;
            block.GetComponent<Renderer>().sharedMaterial = MakeAtlasMaterial(
                name.Replace(" ", "_") + "_Mat",
                color,
                tile);
            return block;
        }

        private static WorldAtlasTile GetSurfaceTile(SurfaceKind kind)
        {
            return kind switch
            {
                SurfaceKind.Runway => WorldAtlasTile.Runway,
                SurfaceKind.Road => WorldAtlasTile.Road,
                SurfaceKind.Field => WorldAtlasTile.Grass,
                SurfaceKind.Ridge => WorldAtlasTile.Mountain,
                SurfaceKind.CanyonFloor => WorldAtlasTile.Canyon,
                SurfaceKind.Water => WorldAtlasTile.Ocean,
                _ => WorldAtlasTile.Grass
            };
        }

        private static WorldAtlasTile GetBlockTile(string name)
        {
            if (name.Contains("City") || name.Contains("Hangar") || name.Contains("Tower") || name.Contains("Barracks") || name.Contains("Terminal") || name.Contains("Downtown") || name.Contains("Condo") || name.Contains("Retail") || name.Contains("Window"))
            {
                return WorldAtlasTile.Building;
            }

            if (name.Contains("Tree") || name.Contains("Palm") || name.Contains("Crown") || name.Contains("Canopy"))
            {
                return WorldAtlasTile.Trees;
            }

            if (name.Contains("Canyon"))
            {
                return WorldAtlasTile.Canyon;
            }

            if (name.Contains("Mountain") || name.Contains("Ridge") || name.Contains("Radar"))
            {
                return WorldAtlasTile.Mountain;
            }

            if (name.Contains("Road") || name.Contains("Runway") || name.Contains("Apron") || name.Contains("Freeway") || name.Contains("Boardwalk") || name.Contains("Pier") || name.Contains("Parking") || name.Contains("Stripe") || name.Contains("Bridge") || name.Contains("Guardrail"))
            {
                return WorldAtlasTile.Runway;
            }

            if (name.Contains("Beach") || name.Contains("Sand"))
            {
                return WorldAtlasTile.Sand;
            }

            return WorldAtlasTile.Grass;
        }

        private static Material MakeAtlasMaterial(string name, Color color, WorldAtlasTile tile)
        {
            Material material = MakeMaterial(name, color);
            Texture2D atlas = AssetDatabase.LoadAssetAtPath<Texture2D>(WorldMaterialAtlasPath);
            if (atlas == null)
            {
                return material;
            }

            Vector2 scale = new Vector2(1f / 3f, 1f / 3f);
            Vector2 offset = GetAtlasOffset(tile);

            if (material.HasProperty("_BaseMap"))
            {
                material.SetTexture("_BaseMap", atlas);
                material.SetTextureScale("_BaseMap", scale);
                material.SetTextureOffset("_BaseMap", offset);
                material.SetColor("_BaseColor", color);
            }

            if (material.HasProperty("_MainTex"))
            {
                material.SetTexture("_MainTex", atlas);
                material.SetTextureScale("_MainTex", scale);
                material.SetTextureOffset("_MainTex", offset);
            }

            material.mainTexture = atlas;
            material.mainTextureScale = scale;
            material.mainTextureOffset = offset;
            return material;
        }

        private static Vector2 GetAtlasOffset(WorldAtlasTile tile)
        {
            int index = (int)tile;
            int column = index % 3;
            int topRow = index / 3;
            int unityRow = 2 - topRow;
            return new Vector2(column / 3f, unityRow / 3f);
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

using System.Collections.Generic;
using System.IO;
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
        private const string SeoulMaterialAtlasPath = "Assets/MINgo/Art/Textures/seoul-generated-material-atlas-v1.png";
        private const string SeoulBoxMeshPath = "Assets/MINgo/Art/Meshes/seoul-box-roof-split.asset";

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

        private enum SeoulAtlasTile
        {
            GlassFacade,
            ConcreteFacade,
            ApartmentFacade,
            RoofMechanical,
            RoadAsphalt,
            RiverWater,
            ParkGrass,
            LandmarkMetal,
            PalaceRoof
        }

        [MenuItem("MINgo/Rebuild Free Flight Sandbox Scene")]
        public static void RebuildScene()
        {
            EnsureSeoulMaterialAtlasAsset();
            EnsureSeoulBoxMeshAsset();

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            RenderSettings.skybox = MakeProceduralSkybox();
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.62f, 0.72f, 0.78f);
            RenderSettings.fogDensity = 0.0014f;
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
            camera.farClipPlane = 6200f;
            cameraObject.transform.position = new Vector3(0f, 18f, -38f);
            cameraObject.transform.rotation = Quaternion.Euler(18f, 0f, 0f);
            cameraObject.AddComponent<AudioListener>();

            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "Flight Reference Ground";
            ground.transform.position = new Vector3(0f, -0.04f, 500f);
            ground.transform.localScale = new Vector3(5200f, 0.08f, 5200f);
            ground.GetComponent<Renderer>().sharedMaterial = MakeAtlasMaterial(
                "Flight_Reference_Ground_Mat",
                new Color(0.36f, 0.52f, 0.4f),
                WorldAtlasTile.Grass);

            CreateSeoulWorldSlice();
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

        private static void CreateSeoulWorldSlice()
        {
            CreateSeoulHangangAxis();
            CreateYeouidoDistrict();
            CreateBanpoNodeulZone();
            CreateNamsanJongnoDistrict();
            CreateGangnamDistrict();
            CreateJamsilDistrict();
            CreateExpandedSeoulFabric();
        }

        private static void EnsureSeoulMaterialAtlasAsset()
        {
            string absoluteAtlasPath = Path.Combine(Application.dataPath, SeoulMaterialAtlasPath.Substring("Assets/".Length));
            string directory = Path.GetDirectoryName(absoluteAtlasPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            const int atlasSize = 1024;
            Texture2D atlas = new Texture2D(atlasSize, atlasSize, TextureFormat.RGBA32, false)
            {
                name = "seoul-generated-material-atlas-v1"
            };

            PaintSeoulAtlasTile(atlas, SeoulAtlasTile.GlassFacade, new Color(0.22f, 0.42f, 0.55f));
            PaintSeoulAtlasTile(atlas, SeoulAtlasTile.ConcreteFacade, new Color(0.58f, 0.6f, 0.57f));
            PaintSeoulAtlasTile(atlas, SeoulAtlasTile.ApartmentFacade, new Color(0.64f, 0.64f, 0.58f));
            PaintSeoulAtlasTile(atlas, SeoulAtlasTile.RoofMechanical, new Color(0.15f, 0.16f, 0.16f));
            PaintSeoulAtlasTile(atlas, SeoulAtlasTile.RoadAsphalt, new Color(0.1f, 0.105f, 0.11f));
            PaintSeoulAtlasTile(atlas, SeoulAtlasTile.RiverWater, new Color(0.08f, 0.3f, 0.52f));
            PaintSeoulAtlasTile(atlas, SeoulAtlasTile.ParkGrass, new Color(0.16f, 0.42f, 0.22f));
            PaintSeoulAtlasTile(atlas, SeoulAtlasTile.LandmarkMetal, new Color(0.64f, 0.72f, 0.76f));
            PaintSeoulAtlasTile(atlas, SeoulAtlasTile.PalaceRoof, new Color(0.42f, 0.18f, 0.12f));
            atlas.Apply();

            File.WriteAllBytes(absoluteAtlasPath, atlas.EncodeToPNG());
            Object.DestroyImmediate(atlas);
            AssetDatabase.ImportAsset(SeoulMaterialAtlasPath, ImportAssetOptions.ForceUpdate);

            if (AssetImporter.GetAtPath(SeoulMaterialAtlasPath) is TextureImporter importer)
            {
                importer.textureType = TextureImporterType.Default;
                importer.sRGBTexture = true;
                importer.maxTextureSize = 2048;
                importer.mipmapEnabled = true;
                importer.SaveAndReimport();
            }
        }

        private static void EnsureSeoulBoxMeshAsset()
        {
            if (AssetDatabase.LoadAssetAtPath<Mesh>(SeoulBoxMeshPath) != null)
            {
                return;
            }

            string absoluteMeshPath = Path.Combine(Application.dataPath, SeoulBoxMeshPath.Substring("Assets/".Length));
            string directory = Path.GetDirectoryName(absoluteMeshPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            Mesh mesh = CreateBoxMeshWithRoofSubmesh("Seoul_Box_Roof_Split_Mesh");
            AssetDatabase.CreateAsset(mesh, SeoulBoxMeshPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(SeoulBoxMeshPath, ImportAssetOptions.ForceUpdate);
        }

        private static void PaintSeoulAtlasTile(Texture2D atlas, SeoulAtlasTile tile, Color baseColor)
        {
            const int columns = 3;
            int tileSize = atlas.width / columns;
            int index = (int)tile;
            int column = index % columns;
            int topRow = index / columns;
            int startX = column * tileSize;
            int startY = (columns - 1 - topRow) * tileSize;

            for (int y = 0; y < tileSize; y++)
            {
                for (int x = 0; x < tileSize; x++)
                {
                    float u = x / (float)(tileSize - 1);
                    float v = y / (float)(tileSize - 1);
                    float noise = Mathf.PerlinNoise((startX + x) * 0.027f, (startY + y) * 0.031f);
                    Color color = baseColor * Mathf.Lerp(0.82f, 1.16f, noise);

                    color = tile switch
                    {
                        SeoulAtlasTile.GlassFacade => PaintGlassFacade(color, u, v),
                        SeoulAtlasTile.ConcreteFacade => PaintConcreteFacade(color, u, v),
                        SeoulAtlasTile.ApartmentFacade => PaintApartmentFacade(color, u, v),
                        SeoulAtlasTile.RoofMechanical => PaintRoofMechanical(color, u, v),
                        SeoulAtlasTile.RoadAsphalt => PaintRoadAsphalt(color, u, v),
                        SeoulAtlasTile.RiverWater => PaintRiverWater(color, u, v),
                        SeoulAtlasTile.ParkGrass => PaintParkGrass(color, u, v),
                        SeoulAtlasTile.LandmarkMetal => PaintLandmarkMetal(color, u, v),
                        SeoulAtlasTile.PalaceRoof => PaintPalaceRoof(color, u, v),
                        _ => color
                    };

                    color.a = 1f;
                    atlas.SetPixel(startX + x, startY + y, color);
                }
            }
        }

        private static Color PaintGlassFacade(Color color, float u, float v)
        {
            bool mullion = Mathf.Repeat(u * 12f, 1f) < 0.08f || Mathf.Repeat(v * 18f, 1f) < 0.06f;
            bool brightWindow = Mathf.Repeat(u * 12f + v * 2f, 1f) > 0.72f;
            return mullion
                ? color * 0.42f
                : Color.Lerp(color, new Color(0.72f, 0.88f, 0.96f), brightWindow ? 0.45f : 0.16f);
        }

        private static Color PaintConcreteFacade(Color color, float u, float v)
        {
            bool joint = Mathf.Repeat(u * 6f, 1f) < 0.035f || Mathf.Repeat(v * 10f, 1f) < 0.035f;
            bool window = Mathf.Repeat(u * 9f, 1f) > 0.58f && Mathf.Repeat(v * 12f, 1f) > 0.48f;
            Color jointColor = color * 0.58f;
            Color windowColor = new Color(0.12f, 0.2f, 0.25f);
            return joint ? jointColor : (window ? Color.Lerp(color, windowColor, 0.72f) : color);
        }

        private static Color PaintApartmentFacade(Color color, float u, float v)
        {
            bool balconyRail = Mathf.Repeat(v * 14f, 1f) < 0.08f;
            bool window = Mathf.Repeat(u * 8f, 1f) > 0.52f && Mathf.Repeat(v * 14f, 1f) > 0.32f;
            Color balconyColor = new Color(0.82f, 0.84f, 0.78f);
            Color windowColor = new Color(0.16f, 0.22f, 0.25f);
            return balconyRail ? Color.Lerp(color, balconyColor, 0.55f) : (window ? Color.Lerp(color, windowColor, 0.55f) : color);
        }

        private static Color PaintRoofMechanical(Color color, float u, float v)
        {
            bool vent = (u > 0.15f && u < 0.33f && v > 0.2f && v < 0.36f)
                || (u > 0.58f && u < 0.78f && v > 0.58f && v < 0.78f);
            bool helipad = Mathf.Abs(u - 0.5f) < 0.035f || Mathf.Abs(v - 0.5f) < 0.035f;
            return vent
                ? new Color(0.32f, 0.34f, 0.34f)
                : (helipad ? Color.Lerp(color, new Color(0.88f, 0.86f, 0.62f), 0.34f) : color);
        }

        private static Color PaintRoadAsphalt(Color color, float u, float v)
        {
            bool center = Mathf.Abs(v - 0.5f) < 0.018f && Mathf.Repeat(u * 8f, 1f) > 0.45f;
            bool shoulder = v < 0.08f || v > 0.92f;
            return center
                ? new Color(0.92f, 0.84f, 0.42f)
                : (shoulder ? color * 0.65f : color);
        }

        private static Color PaintRiverWater(Color color, float u, float v)
        {
            float wave = Mathf.Sin((u * 18f + v * 7f) * Mathf.PI) * 0.5f + 0.5f;
            return Color.Lerp(color, new Color(0.62f, 0.82f, 0.92f), wave * 0.22f);
        }

        private static Color PaintParkGrass(Color color, float u, float v)
        {
            bool path = Mathf.Abs(v - (0.35f + Mathf.Sin(u * Mathf.PI * 2f) * 0.08f)) < 0.035f;
            bool treeDot = Mathf.Repeat(u * 9f, 1f) < 0.12f && Mathf.Repeat(v * 7f, 1f) < 0.12f;
            return path
                ? new Color(0.42f, 0.36f, 0.24f)
                : (treeDot ? new Color(0.06f, 0.24f, 0.09f) : color);
        }

        private static Color PaintLandmarkMetal(Color color, float u, float v)
        {
            bool rib = Mathf.Repeat(u * 10f, 1f) < 0.12f;
            bool highlight = u > 0.42f && u < 0.5f;
            return rib ? color * 0.58f : (highlight ? Color.Lerp(color, Color.white, 0.28f) : color);
        }

        private static Color PaintPalaceRoof(Color color, float u, float v)
        {
            bool ridge = Mathf.Repeat(u * 5f, 1f) < 0.07f;
            bool dancheong = Mathf.Repeat(v * 7f, 1f) < 0.08f;
            return ridge
                ? new Color(0.2f, 0.08f, 0.06f)
                : (dancheong ? Color.Lerp(color, new Color(0.06f, 0.32f, 0.22f), 0.55f) : color);
        }

        private static void CreateSeoulHangangAxis()
        {
            GameObject westRiver = CreateLandingSurface("Hangang River West", SurfaceKind.Water, new Vector3(-760f, -0.07f, 1080f), new Vector3(1480f, 0.7f, 150f), new Color(0.09f, 0.33f, 0.58f));
            westRiver.GetComponent<Collider>().isTrigger = true;
            GameObject eastRiver = CreateLandingSurface("Hangang River East", SurfaceKind.Water, new Vector3(760f, -0.07f, 1080f), new Vector3(1480f, 0.7f, 150f), new Color(0.08f, 0.31f, 0.55f));
            eastRiver.GetComponent<Collider>().isTrigger = true;

            CreateLandingSurface("Seoul Hangang North Park", SurfaceKind.Field, new Vector3(0f, 0.02f, 1174f), new Vector3(2950f, 0.14f, 52f), new Color(0.2f, 0.45f, 0.25f));
            CreateLandingSurface("Seoul Hangang South Park", SurfaceKind.Field, new Vector3(0f, 0.02f, 986f), new Vector3(2950f, 0.14f, 52f), new Color(0.18f, 0.42f, 0.23f));
            CreateSeoulRoad("Gangbyeonbuk-ro Riverside Road", new Vector3(0f, 0.1f, 1226f), new Vector3(3020f, 0.18f, 16f), 0f);
            CreateSeoulRoad("Olympic-daero Riverside Road", new Vector3(0f, 0.1f, 934f), new Vector3(3020f, 0.18f, 16f), 0f);
            CreateSeoulRoad("Seoul West Riverside Expressway", new Vector3(-900f, 0.12f, 1254f), new Vector3(1180f, 0.18f, 18f), 0f);
            CreateSeoulRoad("Seoul East Riverside Expressway", new Vector3(900f, 0.12f, 906f), new Vector3(1180f, 0.18f, 18f), 0f);

            CreateSeoulBridge("Mapo Bridge Road", new Vector3(-190f, 5.4f, 1080f), new Vector3(18f, 0.28f, 260f), 0f);
            CreateSeoulBridge("Banpo Bridge Road", new Vector3(130f, 5.8f, 1080f), new Vector3(20f, 0.28f, 280f), 0f);
            CreateSeoulBridge("Dongjak Bridge Road", new Vector3(300f, 5.6f, 1080f), new Vector3(18f, 0.28f, 255f), 0f);
            CreateSeoulBridge("Jamsil Bridge Road", new Vector3(610f, 5.8f, 1080f), new Vector3(18f, 0.28f, 265f), 0f);

            CreateSeoulTreeRow("Seoul Hangang North Tree Row", new Vector3(-1320f, 0.2f, 1194f), 56, new Vector3(48f, 0f, 0f));
            CreateSeoulTreeRow("Seoul Hangang South Tree Row", new Vector3(-1320f, 0.2f, 966f), 56, new Vector3(48f, 0f, 0f));
        }

        private static void CreateYeouidoDistrict()
        {
            CreateLandingSurface("Yeouido Island Park", SurfaceKind.Field, new Vector3(-210f, 0.05f, 1078f), new Vector3(210f, 0.22f, 74f), new Color(0.22f, 0.48f, 0.24f));
            CreateSeoulRoad("Seoul Yeouido Ring Road North", new Vector3(-210f, 0.16f, 1123f), new Vector3(220f, 0.18f, 10f), 0f);
            CreateSeoulRoad("Seoul Yeouido Ring Road South", new Vector3(-210f, 0.16f, 1033f), new Vector3(220f, 0.18f, 10f), 0f);
            CreateSeoulRoad("Seoul Yeouido Finance Avenue", new Vector3(-210f, 0.17f, 1078f), new Vector3(14f, 0.18f, 92f), 0f);

            CreateSeoulGlassTower("Yeouido 63 Finance Tower", new Vector3(-318f, 76f, 1068f), new Vector3(18f, 152f, 18f), new Color(0.73f, 0.63f, 0.38f));
            CreateSeoulGlassTower("Seoul Yeouido IFC Tower 0", new Vector3(-260f, 54f, 1110f), new Vector3(22f, 108f, 18f), new Color(0.32f, 0.48f, 0.58f));
            CreateSeoulGlassTower("Seoul Yeouido IFC Tower 1", new Vector3(-235f, 42f, 1110f), new Vector3(18f, 84f, 18f), new Color(0.28f, 0.42f, 0.52f));
            CreateSeoulGlassTower("Seoul Yeouido Securities Tower", new Vector3(-175f, 40f, 1112f), new Vector3(20f, 80f, 22f), new Color(0.38f, 0.48f, 0.54f));
            CreateSeoulApartmentSlab("Seoul Yeouido Riverside Apartment 0", new Vector3(-140f, 18f, 1038f), new Vector3(36f, 36f, 14f), new Color(0.62f, 0.63f, 0.58f));
            CreateSeoulApartmentSlab("Seoul Yeouido Riverside Apartment 1", new Vector3(-105f, 20f, 1118f), new Vector3(38f, 40f, 14f), new Color(0.58f, 0.6f, 0.58f));
            CreateBlock("Seoul Yeouido National Assembly Dome", new Vector3(-325f, 14f, 1120f), new Vector3(34f, 10f, 28f), new Color(0.64f, 0.62f, 0.55f));
            CreateSphereBlock("Seoul Yeouido National Assembly Roof", new Vector3(-325f, 23f, 1120f), new Vector3(17f, 7f, 14f), new Color(0.42f, 0.55f, 0.48f), WorldAtlasTile.Building);
            CreateBlock("Seoul Yeouido Broadcast Mast", new Vector3(-95f, 43f, 1060f), new Vector3(5f, 86f, 5f), new Color(0.58f, 0.6f, 0.62f));
        }

        private static void CreateBanpoNodeulZone()
        {
            CreateLandingSurface("Nodeul Island Park", SurfaceKind.Field, new Vector3(72f, 0.08f, 1082f), new Vector3(84f, 0.18f, 38f), new Color(0.28f, 0.52f, 0.27f));
            CreateBlock("Seoul Nodeul Performance Hall", new Vector3(68f, 6f, 1080f), new Vector3(30f, 12f, 18f), new Color(0.44f, 0.42f, 0.38f));
            CreateLandingSurface("Seoul Sevit Floating Island 0", SurfaceKind.Field, new Vector3(160f, 0.11f, 1058f), new Vector3(32f, 0.18f, 18f), new Color(0.52f, 0.58f, 0.52f));
            CreateLandingSurface("Seoul Sevit Floating Island 1", SurfaceKind.Field, new Vector3(190f, 0.11f, 1092f), new Vector3(26f, 0.18f, 16f), new Color(0.5f, 0.56f, 0.5f));
            CreateBlock("Seoul Banpo Fountain Light 0", new Vector3(118f, 9f, 949f), new Vector3(4f, 18f, 4f), new Color(0.6f, 0.78f, 0.96f));
            CreateBlock("Seoul Banpo Fountain Light 1", new Vector3(142f, 9f, 1211f), new Vector3(4f, 18f, 4f), new Color(0.6f, 0.78f, 0.96f));
            CreateSeoulRoad("Seoul Banpo Riverside Access Road", new Vector3(188f, 0.16f, 965f), new Vector3(105f, 0.18f, 10f), -22f);
        }

        private static void CreateNamsanJongnoDistrict()
        {
            GameObject ridge = CreateBlock("Namsan Ridge", new Vector3(5f, 42f, 1360f), new Vector3(210f, 84f, 120f), new Color(0.24f, 0.37f, 0.24f));
            ridge.transform.rotation = Quaternion.Euler(0f, -12f, 0f);
            CreateBlock("N Seoul Tower", new Vector3(0f, 132f, 1360f), new Vector3(10f, 78f, 10f), new Color(0.82f, 0.84f, 0.78f));
            CreateBlock("N Seoul Tower Observatory", new Vector3(0f, 176f, 1360f), new Vector3(26f, 10f, 26f), new Color(0.18f, 0.22f, 0.26f));
            CreateBlock("N Seoul Tower Antenna", new Vector3(0f, 209f, 1360f), new Vector3(3f, 58f, 3f), new Color(0.9f, 0.9f, 0.86f));

            CreateSeoulRoad("Seoul Jongno Gwanghwamun Axis", new Vector3(-155f, 0.14f, 1340f), new Vector3(16f, 0.18f, 190f), 0f);
            CreateBlock("Seoul Jongno Gwanghwamun Plaza", new Vector3(-155f, 0.2f, 1440f), new Vector3(52f, 0.2f, 74f), new Color(0.46f, 0.45f, 0.4f));
            CreateBlock("Seoul Jongno Palace Gate", new Vector3(-155f, 13f, 1484f), new Vector3(42f, 18f, 14f), new Color(0.48f, 0.22f, 0.15f));
            CreateBlock("Seoul Jongno City Hall", new Vector3(-248f, 16f, 1312f), new Vector3(48f, 32f, 28f), new Color(0.5f, 0.55f, 0.56f));

            for (int i = 0; i < 10; i++)
            {
                float x = -305f + (i % 5) * 34f;
                float z = 1248f + (i / 5) * 42f;
                float h = 18f + (i % 4) * 5f;
                CreateSeoulApartmentSlab("Seoul Jongno Lowrise Block " + i, new Vector3(x, h * 0.5f, z), new Vector3(24f, h, 20f), new Color(0.56f, 0.56f, 0.52f));
            }
        }

        private static void CreateGangnamDistrict()
        {
            CreateSeoulRoad("Gangnam Boulevard", new Vector3(250f, 0.16f, 805f), new Vector3(420f, 0.18f, 18f), 0f);
            CreateSeoulRoad("Seoul Gangnam Teheran-ro", new Vector3(250f, 0.18f, 760f), new Vector3(390f, 0.18f, 14f), 0f);
            CreateSeoulRoad("Seoul Gangnam North-South Road 0", new Vector3(110f, 0.17f, 820f), new Vector3(12f, 0.18f, 170f), 0f);
            CreateSeoulRoad("Seoul Gangnam North-South Road 1", new Vector3(230f, 0.17f, 820f), new Vector3(12f, 0.18f, 170f), 0f);
            CreateSeoulRoad("Seoul Gangnam North-South Road 2", new Vector3(350f, 0.17f, 820f), new Vector3(12f, 0.18f, 170f), 0f);

            for (int i = 0; i < 24; i++)
            {
                int row = i / 6;
                int col = i % 6;
                float height = 30f + ((i * 17) % 70);
                Vector3 position = new Vector3(70f + col * 54f, height * 0.5f, 705f + row * 55f);
                Vector3 scale = new Vector3(22f + (i % 3) * 4f, height, 20f + (i % 2) * 8f);
                if (i % 3 == 0)
                {
                    CreateSeoulGlassTower("Seoul Gangnam Glass Office " + i, position, scale, new Color(0.28f, 0.42f, 0.52f));
                }
                else
                {
                    CreateSeoulApartmentSlab("Seoul Gangnam Mixed Use Block " + i, position, scale, new Color(0.55f, 0.58f, 0.56f));
                }
            }

            CreateBlock("Seoul Gangnam COEX Podium", new Vector3(420f, 10f, 850f), new Vector3(88f, 20f, 48f), new Color(0.44f, 0.45f, 0.44f));
            CreateBlock("Seoul Gangnam Trade Tower", new Vector3(472f, 58f, 846f), new Vector3(20f, 116f, 20f), new Color(0.3f, 0.43f, 0.5f));
        }

        private static void CreateJamsilDistrict()
        {
            CreateLandingSurface("Seokchon Lake East", SurfaceKind.Water, new Vector3(620f, -0.04f, 790f), new Vector3(108f, 0.4f, 52f), new Color(0.07f, 0.27f, 0.48f)).GetComponent<Collider>().isTrigger = true;
            CreateLandingSurface("Seokchon Lake West", SurfaceKind.Water, new Vector3(520f, -0.04f, 790f), new Vector3(90f, 0.4f, 48f), new Color(0.08f, 0.29f, 0.5f)).GetComponent<Collider>().isTrigger = true;
            CreateSeoulRoad("Seoul Jamsil Lake Ring North", new Vector3(570f, 0.15f, 830f), new Vector3(220f, 0.18f, 10f), 0f);
            CreateSeoulRoad("Seoul Jamsil Lake Ring South", new Vector3(570f, 0.15f, 742f), new Vector3(220f, 0.18f, 10f), 0f);
            CreateSeoulRoad("Seoul Jamsil Sports Road", new Vector3(675f, 0.15f, 875f), new Vector3(135f, 0.18f, 12f), 20f);

            CreateBlock("Jamsil Lotte World Tower", new Vector3(585f, 105f, 850f), new Vector3(20f, 210f, 20f), new Color(0.66f, 0.78f, 0.84f));
            CreateBlock("Seoul Jamsil Lotte Tower Crown", new Vector3(585f, 218f, 850f), new Vector3(14f, 22f, 14f), new Color(0.78f, 0.86f, 0.9f));
            CreateBlock("Seoul Jamsil Lotte Mall Podium", new Vector3(545f, 12f, 845f), new Vector3(84f, 24f, 52f), new Color(0.5f, 0.48f, 0.43f));

            for (int i = 0; i < 12; i++)
            {
                float height = 22f + (i % 5) * 8f;
                Vector3 position = new Vector3(492f + (i % 4) * 52f, height * 0.5f, 690f + (i / 4) * 48f);
                CreateSeoulApartmentSlab("Seoul Jamsil Apartment Cluster " + i, position, new Vector3(32f, height, 16f), new Color(0.6f, 0.61f, 0.57f));
            }

            CreateBlock("Seoul Jamsil Stadium Bowl", new Vector3(720f, 9f, 806f), new Vector3(90f, 18f, 68f), new Color(0.56f, 0.58f, 0.56f));
            CreateBlock("Seoul Jamsil Stadium Field", new Vector3(720f, 18.2f, 806f), new Vector3(62f, 1f, 42f), new Color(0.12f, 0.42f, 0.14f));
        }

        private static void CreateExpandedSeoulFabric()
        {
            CreateSeoulGlassTower("Seoul Map West Boundary Landmark", new Vector3(-1540f, 64f, 1110f), new Vector3(34f, 128f, 30f), new Color(0.36f, 0.48f, 0.56f));
            CreateSeoulGlassTower("Seoul Map East Boundary Landmark", new Vector3(1560f, 72f, 930f), new Vector3(36f, 144f, 34f), new Color(0.42f, 0.56f, 0.62f));
            CreateSeoulRoad("Seoul West Boundary Arterial", new Vector3(-1450f, 0.16f, 1038f), new Vector3(260f, 0.18f, 14f), -18f);
            CreateSeoulRoad("Seoul East Boundary Arterial", new Vector3(1450f, 0.16f, 982f), new Vector3(270f, 0.18f, 14f), 22f);

            CreateGangnamExtendedBlocks();
            CreateYeouidoExtendedBlocks();
            CreateJamsilExtendedBlocks();
            CreateJongnoExtendedBlocks();
        }

        private static void CreateGangnamExtendedBlocks()
        {
            for (int i = 0; i < 54; i++)
            {
                int row = i / 9;
                int col = i % 9;
                float height = 24f + ((i * 19) % 82);
                Vector3 position = new Vector3(80f + col * 58f, height * 0.5f, 590f + row * 48f);
                Vector3 scale = new Vector3(22f + (i % 4) * 3f, height, 18f + (i % 3) * 5f);
                if (i % 4 == 0)
                {
                    CreateSeoulGlassTower("Seoul Gangnam Extended Glass Block " + i, position, scale, new Color(0.26f, 0.4f, 0.52f));
                }
                else
                {
                    CreateSeoulApartmentSlab("Seoul Gangnam Extended Mixed Block " + i, position, scale, new Color(0.57f, 0.59f, 0.55f));
                }
            }
        }

        private static void CreateYeouidoExtendedBlocks()
        {
            for (int i = 0; i < 24; i++)
            {
                int row = i / 6;
                int col = i % 6;
                float height = 22f + ((i * 23) % 92);
                Vector3 position = new Vector3(-430f + col * 46f, height * 0.5f, 1010f + row * 50f);
                Vector3 scale = new Vector3(18f + (i % 3) * 4f, height, 16f + (i % 2) * 6f);
                if (i % 3 == 0)
                {
                    CreateSeoulGlassTower("Seoul Yeouido Extended Finance Block " + i, position, scale, new Color(0.34f, 0.48f, 0.58f));
                }
                else
                {
                    CreateSeoulApartmentSlab("Seoul Yeouido Extended Riverside Block " + i, position, scale, new Color(0.61f, 0.62f, 0.57f));
                }
            }
        }

        private static void CreateJamsilExtendedBlocks()
        {
            for (int i = 0; i < 30; i++)
            {
                int row = i / 6;
                int col = i % 6;
                float height = 20f + ((i * 13) % 58);
                Vector3 position = new Vector3(500f + col * 56f, height * 0.5f, 620f + row * 46f);
                Vector3 scale = new Vector3(28f, height, 16f + (i % 3) * 4f);
                CreateSeoulApartmentSlab("Seoul Jamsil Extended Apartment Block " + i, position, scale, new Color(0.62f, 0.63f, 0.58f));
            }
        }

        private static void CreateJongnoExtendedBlocks()
        {
            for (int i = 0; i < 28; i++)
            {
                int row = i / 7;
                int col = i % 7;
                float height = 14f + ((i * 7) % 30);
                Vector3 position = new Vector3(-430f + col * 44f, height * 0.5f, 1255f + row * 42f);
                Vector3 scale = new Vector3(22f + (i % 2) * 6f, height, 18f + (i % 3) * 3f);
                CreateSeoulApartmentSlab("Seoul Jongno Extended Lowrise Block " + i, position, scale, new Color(0.58f, 0.57f, 0.52f));
            }

            CreateSeoulBoxWithFaces(
                "Seoul Jongno Palace Roof Pavilion",
                new Vector3(-155f, 25f, 1510f),
                new Vector3(54f, 14f, 28f),
                new Color(0.44f, 0.22f, 0.15f),
                SeoulAtlasTile.ConcreteFacade,
                new Color(0.38f, 0.12f, 0.09f),
                SeoulAtlasTile.PalaceRoof);
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
            CreateLandingSurface("Open Field", SurfaceKind.Field, new Vector3(-115f, 0.01f, 260f), new Vector3(150f, 0.04f, 150f), new Color(0.28f, 0.48f, 0.27f));
            CreateLandingSurface("Long Meadow", SurfaceKind.Field, new Vector3(-220f, 0.01f, 165f), new Vector3(95f, 0.04f, 250f), new Color(0.34f, 0.55f, 0.3f));
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

        private static GameObject CreateSeoulRoad(string name, Vector3 position, Vector3 scale, float yawDegrees)
        {
            GameObject road = CreateLandingSurface(name, SurfaceKind.Road, position, scale, new Color(0.1f, 0.105f, 0.11f));
            road.transform.rotation = Quaternion.Euler(0f, yawDegrees, 0f);

            bool eastWest = scale.x >= scale.z;
            Vector3 stripeScale = eastWest
                ? new Vector3(Mathf.Max(1f, scale.x * 0.7f), 0.03f, 0.9f)
                : new Vector3(0.9f, 0.03f, Mathf.Max(1f, scale.z * 0.7f));
            GameObject stripe = CreateVisualBlock(name + " Lane Marking", position + Vector3.up * 0.12f, stripeScale, new Color(0.9f, 0.84f, 0.48f));
            stripe.transform.rotation = road.transform.rotation;
            return road;
        }

        private static GameObject CreateSeoulBridge(string name, Vector3 position, Vector3 scale, float yawDegrees)
        {
            GameObject bridge = CreateSeoulRoad(name, position, scale, yawDegrees);
            float supportSpacing = Mathf.Max(42f, Mathf.Max(scale.x, scale.z) * 0.28f);
            bool northSouth = scale.z >= scale.x;
            Vector3 supportAxis = northSouth
                ? bridge.transform.forward
                : bridge.transform.right;

            for (int i = -1; i <= 1; i++)
            {
                Vector3 supportPosition = position + supportAxis * (supportSpacing * i);
                CreateBlock(name + " Support " + (i + 1), new Vector3(supportPosition.x, 2.2f, supportPosition.z), new Vector3(5f, 4.4f, 5f), new Color(0.48f, 0.49f, 0.47f));
            }

            CreateGuardrail(name + " West Guardrail", position + bridge.transform.right * -((scale.x * 0.5f) + 1.2f) + Vector3.up * 0.85f, new Vector3(1.4f, 1.2f, Mathf.Max(12f, scale.z)), yawDegrees);
            CreateGuardrail(name + " East Guardrail", position + bridge.transform.right * ((scale.x * 0.5f) + 1.2f) + Vector3.up * 0.85f, new Vector3(1.4f, 1.2f, Mathf.Max(12f, scale.z)), yawDegrees);
            return bridge;
        }

        private static void CreateSeoulGlassTower(string name, Vector3 position, Vector3 scale, Color color)
        {
            CreateSeoulBoxWithFaces(
                name,
                position,
                scale,
                color,
                SeoulAtlasTile.GlassFacade,
                new Color(0.12f, 0.14f, 0.15f),
                SeoulAtlasTile.RoofMechanical);
            float topY = position.y + scale.y * 0.5f;
            float frontZ = position.z + scale.z * 0.51f;
            float rightX = position.x + scale.x * 0.51f;
            float bandHeight = Mathf.Max(1.2f, scale.y * 0.035f);

            for (int i = 0; i < 4; i++)
            {
                float y = position.y - scale.y * 0.28f + i * scale.y * 0.18f;
                CreateVisualBlock(name + " Window Band Front " + i, new Vector3(position.x, y, frontZ), new Vector3(scale.x * 0.86f, bandHeight, 0.45f), new Color(0.1f, 0.22f, 0.32f));
                CreateVisualBlock(name + " Window Band Right " + i, new Vector3(rightX, y, position.z), new Vector3(0.45f, bandHeight, scale.z * 0.82f), new Color(0.1f, 0.22f, 0.32f));
            }

            CreateBlock(name + " Roof Cap", new Vector3(position.x, topY + 0.7f, position.z), new Vector3(scale.x * 1.05f, 1.2f, scale.z * 1.05f), new Color(0.12f, 0.14f, 0.15f));
            CreateVisualBlock(name + " Rooftop Mechanical", new Vector3(position.x + scale.x * 0.18f, topY + 2.1f, position.z - scale.z * 0.18f), new Vector3(scale.x * 0.3f, 1.8f, scale.z * 0.25f), new Color(0.18f, 0.19f, 0.2f));
        }

        private static void CreateSeoulApartmentSlab(string name, Vector3 position, Vector3 scale, Color color)
        {
            CreateSeoulBoxWithFaces(
                name,
                position,
                scale,
                color,
                SeoulAtlasTile.ApartmentFacade,
                new Color(0.28f, 0.29f, 0.28f),
                SeoulAtlasTile.RoofMechanical);
            float frontZ = position.z + scale.z * 0.51f;
            int stripCount = Mathf.Clamp(Mathf.RoundToInt(scale.y / 14f), 2, 6);
            for (int i = 0; i < stripCount; i++)
            {
                float y = position.y - scale.y * 0.35f + i * (scale.y * 0.7f / Mathf.Max(1, stripCount - 1));
                CreateVisualBlock(name + " Balcony Strip " + i, new Vector3(position.x, y, frontZ), new Vector3(scale.x * 0.82f, 0.9f, 0.38f), new Color(0.2f, 0.27f, 0.3f));
            }

            CreateVisualBlock(name + " Rooftop Line", new Vector3(position.x, position.y + scale.y * 0.5f + 0.35f, position.z), new Vector3(scale.x * 0.94f, 0.7f, scale.z * 0.9f), new Color(0.32f, 0.33f, 0.32f));
        }

        private static GameObject CreateSeoulBoxWithFaces(
            string name,
            Vector3 position,
            Vector3 scale,
            Color sideColor,
            SeoulAtlasTile sideTile,
            Color roofColor,
            SeoulAtlasTile roofTile)
        {
            var box = new GameObject(name);
            box.transform.position = position;
            box.transform.localScale = scale;

            var meshFilter = box.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(SeoulBoxMeshPath)
                ?? CreateBoxMeshWithRoofSubmesh("Seoul_Box_Roof_Split_Mesh");

            var renderer = box.AddComponent<MeshRenderer>();
            renderer.sharedMaterials = new[]
            {
                MakeSeoulAtlasMaterial(name.Replace(" ", "_") + "_Facade_Mat", sideColor, sideTile),
                MakeSeoulAtlasMaterial(name.Replace(" ", "_") + "_Roof_Mat", roofColor, roofTile)
            };

            box.AddComponent<BoxCollider>();
            return box;
        }

        private static Mesh CreateBoxMeshWithRoofSubmesh(string name)
        {
            var vertices = new[]
            {
                new Vector3(-0.5f, -0.5f, 0.5f), new Vector3(-0.5f, 0.5f, 0.5f), new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0.5f, -0.5f, 0.5f),
                new Vector3(0.5f, -0.5f, -0.5f), new Vector3(0.5f, 0.5f, -0.5f), new Vector3(-0.5f, 0.5f, -0.5f), new Vector3(-0.5f, -0.5f, -0.5f),
                new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(-0.5f, 0.5f, -0.5f), new Vector3(-0.5f, 0.5f, 0.5f), new Vector3(-0.5f, -0.5f, 0.5f),
                new Vector3(0.5f, -0.5f, 0.5f), new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0.5f, 0.5f, -0.5f), new Vector3(0.5f, -0.5f, -0.5f),
                new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(-0.5f, -0.5f, 0.5f), new Vector3(0.5f, -0.5f, 0.5f), new Vector3(0.5f, -0.5f, -0.5f),
                new Vector3(-0.5f, 0.5f, 0.5f), new Vector3(-0.5f, 0.5f, -0.5f), new Vector3(0.5f, 0.5f, -0.5f), new Vector3(0.5f, 0.5f, 0.5f)
            };
            var uvs = new[]
            {
                new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(1f, 0f),
                new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(1f, 0f),
                new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(1f, 0f),
                new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(1f, 0f),
                new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(1f, 0f),
                new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(1f, 0f)
            };
            var sideTriangles = new List<int>(30);
            for (int face = 0; face < 5; face++)
            {
                int start = face * 4;
                sideTriangles.Add(start);
                sideTriangles.Add(start + 1);
                sideTriangles.Add(start + 2);
                sideTriangles.Add(start);
                sideTriangles.Add(start + 2);
                sideTriangles.Add(start + 3);
            }

            var roofTriangles = new[] { 20, 21, 22, 20, 22, 23 };
            var mesh = new Mesh
            {
                name = name,
                vertices = vertices,
                uv = uvs,
                subMeshCount = 2
            };
            mesh.SetTriangles(sideTriangles, 0);
            mesh.SetTriangles(roofTriangles, 1);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static void CreateSeoulTreeRow(string name, Vector3 start, int count, Vector3 step)
        {
            for (int i = 0; i < count; i++)
            {
                Vector3 basePosition = start + step * i;
                CreateCylinderBlock(name + " " + i + " Trunk", basePosition + new Vector3(0f, 3.5f, 0f), new Vector3(0.8f, 3.5f, 0.8f), new Color(0.34f, 0.22f, 0.12f), WorldAtlasTile.Trees);
                CreateSphereBlock(name + " " + i + " Canopy", basePosition + new Vector3(0f, 8f, 0f), new Vector3(6f, 4f, 6f), new Color(0.1f, 0.32f, 0.15f), WorldAtlasTile.Trees);
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
            aircraft.transform.position = new Vector3(0f, 1.12f, -65f);
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
            controller.maxThrust = 52f;
            controller.stabilization = 6f;
            controller.autoLevel = 12f;
            controller.autoLevelRotationRate = 4f;
            controller.assistedBankAngle = 22f;
            controller.turnYawAssist = 0.45f;
            controller.takeoffLiftAssist = 130f;
            controller.slowdownDescentAcceleration = 18f;
            controller.slowdownPitchDamping = 0.55f;
            controller.throttleChangeRate = 3.2f;
            controller.groundRunAcceleration = 7f;
            controller.groundSteeringYawRateDegrees = 28f;
            controller.groundSteeringFullSpeed = 12f;
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
            controller.coastBrakeTorque = 0f;
            controller.driveAssistAcceleration = 92f;
            controller.reverseAssistAcceleration = 75f;
            controller.maxForwardSpeed = 20f;
            controller.maxSteerDegrees = 32f;
            controller.fullSteerSpeed = 5f;
            controller.reducedSteerSpeed = 22f;
            controller.reverseThreshold = 1f;
            controller.reverseSteerScale = 0.65f;
            controller.neutralCoastAcceleration = 1.1f;
            controller.directionChangeBrakeAcceleration = 16f;
            controller.steeringYawRateDegrees = 14f;
            controller.handbrakeYawAcceleration = 200f;
            controller.reverseEngageDelay = 2f;
            controller.handbrakeYawRateDegrees = 30f;
            controller.handbrakeMinimumSpeed = 8f;
            controller.handbrakeMaximumAssistSpeed = 12f;
            controller.groundStickAcceleration = 18f;
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
            surface.GetComponent<Renderer>().sharedMaterial = IsSeoulWorldName(name)
                ? MakeSeoulAtlasMaterial(name.Replace(" ", "_") + "_Mat", color, GetSeoulSurfaceTile(kind))
                : MakeAtlasMaterial(
                    name.Replace(" ", "_") + "_Mat",
                    color,
                    GetSurfaceTile(kind));
            surface.AddComponent<SurfaceTag>().kind = kind;
            return surface;
        }

        private static GameObject CreateBlock(string name, Vector3 position, Vector3 scale, Color color)
        {
            if (IsSeoulWorldName(name) && ShouldUseSeoulFaceSplitBlock(name, scale))
            {
                return CreateSeoulBoxWithFaces(
                    name,
                    position,
                    scale,
                    color,
                    GetSeoulBlockTile(name),
                    GetSeoulRoofColor(name),
                    GetSeoulRoofTile(name));
            }

            GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.name = name;
            block.transform.position = position;
            block.transform.localScale = scale;
            block.GetComponent<Renderer>().sharedMaterial = IsSeoulWorldName(name)
                ? MakeSeoulAtlasMaterial(name.Replace(" ", "_") + "_Mat", color, GetSeoulBlockTile(name))
                : MakeAtlasMaterial(
                    name.Replace(" ", "_") + "_Mat",
                    color,
                    GetBlockTile(name));
            return block;
        }

        private static bool ShouldUseSeoulFaceSplitBlock(string name, Vector3 scale)
        {
            if (scale.y < 6f || name.Contains("Ridge"))
            {
                return false;
            }

            return !name.Contains("Road")
                && !name.Contains("Bridge")
                && !name.Contains("Expressway")
                && !name.Contains("Park")
                && !name.Contains("Plaza")
                && !name.Contains("Lake")
                && !name.Contains("River")
                && !name.Contains("Marking")
                && !name.Contains("Strip")
                && !name.Contains("Line")
                && !name.Contains("Fountain");
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
            block.GetComponent<Renderer>().sharedMaterial = IsSeoulWorldName(name)
                ? MakeSeoulAtlasMaterial(name.Replace(" ", "_") + "_Mat", color, GetSeoulBlockTile(name))
                : MakeAtlasMaterial(
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
            block.GetComponent<Renderer>().sharedMaterial = IsSeoulWorldName(name)
                ? MakeSeoulAtlasMaterial(name.Replace(" ", "_") + "_Mat", color, GetSeoulBlockTile(name))
                : MakeAtlasMaterial(
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

        private static SeoulAtlasTile GetSeoulSurfaceTile(SurfaceKind kind)
        {
            return kind switch
            {
                SurfaceKind.Road => SeoulAtlasTile.RoadAsphalt,
                SurfaceKind.Water => SeoulAtlasTile.RiverWater,
                SurfaceKind.Field => SeoulAtlasTile.ParkGrass,
                _ => SeoulAtlasTile.ConcreteFacade
            };
        }

        private static WorldAtlasTile GetBlockTile(string name)
        {
            if (name.Contains("City") || name.Contains("Hangar") || name.Contains("Tower") || name.Contains("Barracks") || name.Contains("Terminal") || name.Contains("Downtown") || name.Contains("Condo") || name.Contains("Retail") || name.Contains("Window") || name.Contains("Seoul") || name.Contains("Apartment") || name.Contains("Finance") || name.Contains("Lotte") || name.Contains("Gangnam") || name.Contains("Yeouido") || name.Contains("Jamsil") || name.Contains("Jongno"))
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

        private static SeoulAtlasTile GetSeoulBlockTile(string name)
        {
            if (name.Contains("Road") || name.Contains("Bridge") || name.Contains("Expressway") || name.Contains("Marking"))
            {
                return SeoulAtlasTile.RoadAsphalt;
            }

            if (name.Contains("Park") || name.Contains("Tree") || name.Contains("Canopy") || name.Contains("Field"))
            {
                return SeoulAtlasTile.ParkGrass;
            }

            if (name.Contains("River") || name.Contains("Lake") || name.Contains("Floating Island"))
            {
                return SeoulAtlasTile.RiverWater;
            }

            if (name.Contains("Tower") || name.Contains("Landmark") || name.Contains("Mast") || name.Contains("Antenna"))
            {
                return SeoulAtlasTile.LandmarkMetal;
            }

            if (name.Contains("Palace") || name.Contains("Gate"))
            {
                return SeoulAtlasTile.PalaceRoof;
            }

            if (name.Contains("Apartment") || name.Contains("Riverside") || name.Contains("Lowrise"))
            {
                return SeoulAtlasTile.ApartmentFacade;
            }

            return SeoulAtlasTile.ConcreteFacade;
        }

        private static SeoulAtlasTile GetSeoulRoofTile(string name)
        {
            if (name.Contains("Palace") || name.Contains("Gate"))
            {
                return SeoulAtlasTile.PalaceRoof;
            }

            if (name.Contains("Tower") || name.Contains("Landmark") || name.Contains("Mast") || name.Contains("Antenna"))
            {
                return SeoulAtlasTile.RoofMechanical;
            }

            return SeoulAtlasTile.RoofMechanical;
        }

        private static Color GetSeoulRoofColor(string name)
        {
            if (name.Contains("Palace") || name.Contains("Gate"))
            {
                return new Color(0.34f, 0.12f, 0.09f);
            }

            if (name.Contains("Tower") || name.Contains("Landmark"))
            {
                return new Color(0.16f, 0.18f, 0.19f);
            }

            return new Color(0.28f, 0.29f, 0.28f);
        }

        private static bool IsSeoulWorldName(string name)
        {
            return name.Contains("Seoul")
                || name.Contains("Hangang")
                || name.Contains("Yeouido")
                || name.Contains("Banpo")
                || name.Contains("Nodeul")
                || name.Contains("Namsan")
                || name.Contains("Gangnam")
                || name.Contains("Jamsil")
                || name.Contains("Jongno")
                || name.Contains("Seokchon")
                || name.Contains("Mapo Bridge")
                || name.Contains("Dongjak Bridge")
                || name.Contains("Olympic-daero")
                || name.Contains("Gangbyeonbuk-ro")
                || name.Contains("N Seoul Tower");
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

        private static Material MakeSeoulAtlasMaterial(string name, Color color, SeoulAtlasTile tile)
        {
            Material material = MakeMaterial(name, color);
            Texture2D atlas = AssetDatabase.LoadAssetAtPath<Texture2D>(SeoulMaterialAtlasPath);
            if (atlas == null)
            {
                return material;
            }

            Vector2 scale = new Vector2(1f / 3f, 1f / 3f);
            Vector2 offset = GetSeoulAtlasOffset(tile);

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

        private static Vector2 GetSeoulAtlasOffset(SeoulAtlasTile tile)
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

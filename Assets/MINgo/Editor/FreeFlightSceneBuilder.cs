using UnityEditor;
using UnityEditor.SceneManagement;
using MINgo.Flight;
using MINgo.Landing;
using UnityEngine;
using UnityEngine.SceneManagement;

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
            camera.farClipPlane = 1800f;
            cameraObject.transform.position = new Vector3(0f, 18f, -38f);
            cameraObject.transform.rotation = Quaternion.Euler(18f, 0f, 0f);

            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "Flight Reference Ground";
            ground.transform.position = new Vector3(0f, -0.22f, 500f);
            ground.transform.localScale = new Vector3(2000f, 0.08f, 2000f);
            ground.GetComponent<Renderer>().sharedMaterial = MakeMaterial("Flight_Reference_Ground_Mat", new Color(0.36f, 0.52f, 0.4f));

            CreateLandingSurface("Runway", SurfaceKind.Runway, Vector3.zero, new Vector3(18f, 0.25f, 180f), new Color(0.19f, 0.2f, 0.2f));
            CreateLandingSurface("Coastal Road", SurfaceKind.Road, new Vector3(70f, 0.02f, 230f), new Vector3(10f, 0.16f, 320f), new Color(0.12f, 0.13f, 0.14f));
            CreateLandingSurface("Open Field", SurfaceKind.Field, new Vector3(-95f, 0.01f, 260f), new Vector3(120f, 0.14f, 130f), new Color(0.28f, 0.48f, 0.27f));
            GameObject ridge = CreateLandingSurface("Ridge Landing Shelf", SurfaceKind.Ridge, new Vector3(-260f, 22f, 520f), new Vector3(95f, 0.3f, 46f), new Color(0.39f, 0.37f, 0.32f));
            ridge.transform.rotation = Quaternion.Euler(0f, 8f, 12f);
            CreateLandingSurface("Canyon Floor", SurfaceKind.CanyonFloor, new Vector3(250f, 0.03f, 610f), new Vector3(58f, 0.14f, 260f), new Color(0.46f, 0.34f, 0.24f));
            GameObject ocean = CreateLandingSurface("Ocean", SurfaceKind.Water, new Vector3(610f, -0.05f, 520f), new Vector3(620f, 1f, 1400f), new Color(0.12f, 0.33f, 0.58f));
            ocean.GetComponent<Collider>().isTrigger = true;

            GameObject aircraft = CreateAircraft();
            var cameraRig = cameraObject.AddComponent<ChaseCameraRig>();
            cameraRig.target = aircraft.transform;

            EditorSceneManager.SaveScene(scene, ScenePath);
            AddSceneToBuildSettings(ScenePath);
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

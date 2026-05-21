using UnityEditor;
using UnityEditor.SceneManagement;
using MINgo.Flight;
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

            GameObject runway = GameObject.CreatePrimitive(PrimitiveType.Cube);
            runway.name = "Runway";
            runway.transform.position = Vector3.zero;
            runway.transform.localScale = new Vector3(18f, 0.25f, 180f);
            runway.GetComponent<Renderer>().sharedMaterial = MakeMaterial("Runway_Mat", new Color(0.19f, 0.2f, 0.2f));

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

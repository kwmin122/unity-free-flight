using System.Linq;
using MINgo.Audio;
using MINgo.Flight;
using MINgo.Hazards;
using MINgo.Landing;
using MINgo.UI;
using MINgo.Vehicles;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace MINgo.Tests
{
    public sealed class FreeFlightSceneContractTests
    {
        private const string ScenePath = "Assets/Scenes/FreeFlightSandbox.unity";
        private const string WorldAtlasPath = "Assets/MINgo/Art/Textures/world-material-atlas-v1.png";
        private const string SeoulAtlasPath = "Assets/MINgo/Art/Textures/seoul-generated-material-atlas-v1.png";
        private const string WorldMapConceptPath = "Assets/MINgo/Art/Concepts/world-map-concept-v1.png";
        private const string CarReferencePath = "Assets/MINgo/Art/Concepts/car-reference-sheet-v1.png";

        [OneTimeSetUp]
        public void OpenScene()
        {
            EditorSceneManager.OpenScene(ScenePath);
        }

        [Test]
        public void SceneContainsPlayableAircraftRig()
        {
            GameObject aircraft = GameObject.Find("Player Aircraft");

            Assert.That(aircraft, Is.Not.Null);
            Assert.That(aircraft.GetComponent<Rigidbody>(), Is.Not.Null);
            Assert.That(aircraft.GetComponent<ArcadeAircraftController>(), Is.Not.Null);
            Assert.That(aircraft.GetComponent<LandingStateMachine>(), Is.Not.Null);
        }

        [Test]
        public void SceneContainsChaseCameraAndHud()
        {
            ChaseCameraRig cameraRig = Object.FindAnyObjectByType<ChaseCameraRig>();
            FlightHud hud = Object.FindAnyObjectByType<FlightHud>();

            Assert.That(cameraRig, Is.Not.Null);
            Assert.That(cameraRig.target, Is.Not.Null);
            Assert.That(cameraRig.lookAhead, Is.GreaterThan(0f));
            Assert.That(cameraRig.followDistance, Is.InRange(7f, 9f));
            Assert.That(cameraRig.followHeight, Is.InRange(2f, 2.8f));
            Assert.That(cameraRig.speedPullback, Is.LessThanOrEqualTo(2.5f));
            Assert.That(cameraRig.minFieldOfView, Is.LessThanOrEqualTo(56f));
            Assert.That(cameraRig.maxFieldOfView, Is.LessThanOrEqualTo(68f));
            Assert.That(cameraRig.pitchFollow, Is.LessThanOrEqualTo(0.4f));
            Assert.That(hud, Is.Not.Null);
            Assert.That(GameObject.Find("Flight Reticle"), Is.Not.Null);
        }

        [Test]
        public void SceneContainsLandingSurfacesForMvpLoop()
        {
            SurfaceKind[] surfaceKinds = Object.FindObjectsByType<SurfaceTag>(FindObjectsSortMode.None)
                .Select(surface => surface.kind)
                .Distinct()
                .ToArray();

            Assert.That(surfaceKinds, Does.Contain(SurfaceKind.Runway));
            Assert.That(surfaceKinds, Does.Contain(SurfaceKind.Road));
            Assert.That(surfaceKinds, Does.Contain(SurfaceKind.Field));
            Assert.That(surfaceKinds, Does.Contain(SurfaceKind.Ridge));
            Assert.That(surfaceKinds, Does.Contain(SurfaceKind.CanyonFloor));
            Assert.That(surfaceKinds, Does.Contain(SurfaceKind.Water));
        }

        [Test]
        public void SceneContainsConfiguredRestrictedAirspace()
        {
            RestrictedAirspaceZone zone = Object.FindAnyObjectByType<RestrictedAirspaceZone>();

            Assert.That(zone, Is.Not.Null);
            Assert.That(zone.aircraft, Is.Not.Null);
            Assert.That(zone.hud, Is.Not.Null);
            Assert.That(zone.outerZone, Is.Not.Null);
            Assert.That(zone.deepZone, Is.Not.Null);
            Assert.That(zone.missileSpawnPoint, Is.Not.Null);
        }

        [Test]
        public void SceneContainsSeaplaneBlockoutSilhouette()
        {
            string[] objectNames = Object.FindObjectsByType<Transform>(FindObjectsSortMode.None)
                .Select(transform => transform.name)
                .ToArray();

            Assert.That(objectNames, Does.Contain("Left Pontoon"));
            Assert.That(objectNames, Does.Contain("Right Pontoon"));
            Assert.That(objectNames, Does.Contain("Float Cross Strut Front"));
            Assert.That(objectNames, Does.Contain("Float Cross Strut Rear"));
            Assert.That(objectNames, Does.Contain("Cockpit Canopy"));
            Assert.That(objectNames, Does.Contain("Tail Vertical Fin"));
            Assert.That(objectNames, Does.Contain("Left Wing Tip Red"));
            Assert.That(objectNames, Does.Contain("Right Wing Tip Red"));
        }

        [Test]
        public void SceneContainsReferenceDrivenSeaplaneDetails()
        {
            Transform aircraft = GameObject.Find("Player Aircraft").transform;

            Assert.That(aircraft.Find("Wing").localPosition.y, Is.GreaterThan(0.45f));
            Assert.That(aircraft.Find("High Wing Pylon"), Is.Not.Null);
            Assert.That(aircraft.Find("Left Wing Strut Front"), Is.Not.Null);
            Assert.That(aircraft.Find("Right Wing Strut Front"), Is.Not.Null);
            Assert.That(aircraft.Find("Left Wing Strut Rear"), Is.Not.Null);
            Assert.That(aircraft.Find("Right Wing Strut Rear"), Is.Not.Null);
            Assert.That(aircraft.Find("Propeller Hub"), Is.Not.Null);
            Assert.That(aircraft.Find("Propeller Blade Horizontal"), Is.Not.Null);
            Assert.That(aircraft.Find("Propeller Blade Vertical"), Is.Not.Null);
            AssertVisualOnlyPart(aircraft, "Wing");
            AssertVisualOnlyPart(aircraft, "Left Wing Tip Red");
            AssertVisualOnlyPart(aircraft, "Right Wing Tip Red");
            AssertVisualOnlyPart(aircraft, "High Wing Pylon");
            AssertVisualOnlyPart(aircraft, "Left Wing Strut Front");
            AssertVisualOnlyPart(aircraft, "Right Wing Strut Front");
            AssertVisualOnlyPart(aircraft, "Left Wing Strut Rear");
            AssertVisualOnlyPart(aircraft, "Right Wing Strut Rear");
            AssertVisualOnlyPart(aircraft, "Propeller Hub");
            AssertVisualOnlyPart(aircraft, "Propeller Blade Horizontal");
            AssertVisualOnlyPart(aircraft, "Propeller Blade Vertical");
            AssertPhysicsOnlyPart(aircraft, "Wing Physics Collider");
            AssertPhysicsOnlyPart(aircraft, "Left Wing Tip Physics Collider");
            AssertPhysicsOnlyPart(aircraft, "Right Wing Tip Physics Collider");
        }

        [Test]
        public void SceneContainsReadableTravelLandmarkBeacons()
        {
            string[] objectNames = Object.FindObjectsByType<Transform>(FindObjectsSortMode.None)
                .Select(transform => transform.name)
                .ToArray();

            Assert.That(objectNames, Does.Contain("Airport Beacon Tower"));
            Assert.That(objectNames, Does.Contain("Coastal Lighthouse"));
            Assert.That(objectNames, Does.Contain("Canyon Gate Beacon"));
            Assert.That(objectNames, Does.Contain("Ridge Summit Beacon"));
        }

        [Test]
        public void SceneContainsModernOpenWorldMapDressing()
        {
            string[] objectNames = Object.FindObjectsByType<Transform>(FindObjectsSortMode.None)
                .Select(transform => transform.name)
                .ToArray();

            Assert.That(objectNames, Does.Contain("Downtown Glass Tower 0"));
            Assert.That(objectNames, Does.Contain("Downtown Rooftop Helipad"));
            Assert.That(objectNames, Does.Contain("Freeway Overpass"));
            Assert.That(objectNames, Does.Contain("Beach Boardwalk"));
            Assert.That(objectNames, Does.Contain("Marina Pier Main"));
            Assert.That(objectNames, Does.Contain("Coastal Palm 0 Trunk"));
            Assert.That(objectNames, Does.Contain("City Plaza Sculpture"));
            Assert.That(objectNames, Does.Contain("Runway Threshold Marking North"));
        }

        [Test]
        public void SceneContainsDriveableCarAndVehicleSwitcher()
        {
            GameObject car = GameObject.Find("Player Car");
            PlayerVehicleSwitcher switcher = Object.FindAnyObjectByType<PlayerVehicleSwitcher>();
            WheelCollider[] wheelColliders = car == null
                ? new WheelCollider[0]
                : car.GetComponentsInChildren<WheelCollider>();

            Assert.That(car, Is.Not.Null);
            Assert.That(car.GetComponent<Rigidbody>(), Is.Not.Null);
            Assert.That(car.GetComponent<BoxCollider>(), Is.Not.Null);
            Assert.That(car.GetComponent<ArcadeCarController>(), Is.Not.Null);
            Assert.That(wheelColliders, Has.Length.EqualTo(4));
            Assert.That(car.transform.Find("Wheel Collider FL"), Is.Not.Null);
            Assert.That(car.transform.Find("Wheel Collider FR"), Is.Not.Null);
            Assert.That(car.transform.Find("Wheel Collider RL"), Is.Not.Null);
            Assert.That(car.transform.Find("Wheel Collider RR"), Is.Not.Null);
            AssertCarVisualOnlyPart(car.transform, "Car Body");
            AssertCarVisualOnlyPart(car.transform, "Car Cabin");
            AssertCarVisualOnlyPart(car.transform, "Front Left Wheel Visual");
            AssertCarVisualOnlyPart(car.transform, "Front Right Wheel Visual");
            Assert.That(switcher, Is.Not.Null);
            Assert.That(switcher.aircraft, Is.Not.Null);
            Assert.That(switcher.car, Is.Not.Null);
            Assert.That(switcher.cameraRig, Is.Not.Null);
            Assert.That(switcher.cameraRig.target.name, Is.EqualTo("Player Aircraft"));
        }

        [Test]
        public void SceneContainsVehicleSandboxRoadLoop()
        {
            string[] objectNames = Object.FindObjectsByType<Transform>(FindObjectsSortMode.None)
                .Select(transform => transform.name)
                .ToArray();

            Assert.That(objectNames, Does.Contain("Airport Parking Lot"));
            Assert.That(objectNames, Does.Contain("Airport Parking Stall 0"));
            Assert.That(objectNames, Does.Contain("Downtown Boulevard"));
            Assert.That(objectNames, Does.Contain("Mountain Switchback Road 0"));
            Assert.That(objectNames, Does.Contain("Coastal Highway Bridge"));
            Assert.That(objectNames, Does.Contain("Beach Ramp"));
        }

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

        [Test]
        public void SceneUsesExpandedSeoulMapScale()
        {
            GameObject ground = GameObject.Find("Flight Reference Ground");
            Camera camera = Camera.main;

            Assert.That(ground, Is.Not.Null);
            Assert.That(ground.transform.localScale.x, Is.GreaterThanOrEqualTo(4200f));
            Assert.That(ground.transform.localScale.z, Is.GreaterThanOrEqualTo(4200f));
            Assert.That(camera, Is.Not.Null);
            Assert.That(camera.farClipPlane, Is.GreaterThanOrEqualTo(5200f));
            Assert.That(GameObject.Find("Seoul Map West Boundary Landmark"), Is.Not.Null);
            Assert.That(GameObject.Find("Seoul Map East Boundary Landmark"), Is.Not.Null);
        }

        [Test]
        public void SeoulHangangCorridorSpansExpandedMap()
        {
            GameObject west = GameObject.Find("Hangang River West");
            GameObject east = GameObject.Find("Hangang River East");

            Assert.That(west, Is.Not.Null);
            Assert.That(east, Is.Not.Null);

            float minX = Mathf.Min(
                west.transform.position.x - west.transform.localScale.x * 0.5f,
                east.transform.position.x - east.transform.localScale.x * 0.5f);
            float maxX = Mathf.Max(
                west.transform.position.x + west.transform.localScale.x * 0.5f,
                east.transform.position.x + east.transform.localScale.x * 0.5f);

            Assert.That(maxX - minX, Is.GreaterThanOrEqualTo(2600f));
            AssertRoadSurface("Seoul West Riverside Expressway");
            AssertRoadSurface("Seoul East Riverside Expressway");
        }

        [Test]
        public void ProjectContainsGeneratedSeoulMaterialAtlas()
        {
            Texture2D atlas = AssetDatabase.LoadAssetAtPath<Texture2D>(SeoulAtlasPath);

            Assert.That(atlas, Is.Not.Null);
            Assert.That(atlas.width, Is.GreaterThanOrEqualTo(1024));
            Assert.That(atlas.height, Is.GreaterThanOrEqualTo(1024));
        }

        [Test]
        public void SeoulBuildingsUseDistinctRoofAndSideMaterials()
        {
            Texture2D atlas = AssetDatabase.LoadAssetAtPath<Texture2D>(SeoulAtlasPath);

            Assert.That(atlas, Is.Not.Null);
            AssertRoofAndFacadeMaterials("Seoul Gangnam Glass Office 0", atlas);
            AssertRoofAndFacadeMaterials("Seoul Yeouido IFC Tower 0", atlas);
            AssertRoofAndFacadeMaterials("Seoul Jamsil Apartment Cluster 0", atlas);
            AssertRoofAndFacadeMaterials("Seoul Map East Boundary Landmark", atlas);
            AssertRoofAndFacadeMaterials("N Seoul Tower", atlas);
            AssertRoofAndFacadeMaterials("Jamsil Lotte World Tower", atlas);
            AssertRoofAndFacadeMaterials("Seoul Jongno Palace Gate", atlas);
        }

        [Test]
        public void SceneContainsSeoulDensityForFlightTravel()
        {
            string[] objectNames = Object.FindObjectsByType<Transform>(FindObjectsSortMode.None)
                .Select(transform => transform.name)
                .ToArray();

            Assert.That(objectNames.Count(name => name.StartsWith("Seoul")), Is.GreaterThanOrEqualTo(220));
            Assert.That(objectNames.Count(name => name.Contains("Gangnam")), Is.GreaterThanOrEqualTo(70));
            Assert.That(objectNames.Count(name => name.Contains("Yeouido")), Is.GreaterThanOrEqualTo(30));
            Assert.That(objectNames.Count(name => name.Contains("Jamsil")), Is.GreaterThanOrEqualTo(35));
            Assert.That(objectNames.Count(name => name.Contains("Jongno")), Is.GreaterThanOrEqualTo(25));
        }

        [Test]
        public void RoadAndRunwayPaintDoesNotBlockVehicles()
        {
            AssertVisualOnlyWorldMarking("Airport Parking Stall 0");
            AssertVisualOnlyWorldMarking("Airport Parking Stall 1");
            AssertVisualOnlyWorldMarking("Airport Parking Stall 2");
            AssertVisualOnlyWorldMarking("Runway Centerline");
            AssertVisualOnlyWorldMarking("Runway Threshold Marking North");
            AssertVisualOnlyWorldMarking("Airport Ring Road South Center Stripe");
        }

        [Test]
        public void OpenGrassFieldsAreFlushEnoughForVehicleTravel()
        {
            AssertLowFieldLip("Open Field");
            AssertLowFieldLip("Long Meadow");
        }

        [Test]
        public void SceneContainsProceduralFlightAudioRig()
        {
            ProceduralFlightAudio audio = Object.FindAnyObjectByType<ProceduralFlightAudio>();

            Assert.That(audio, Is.Not.Null);
            Assert.That(audio.aircraft, Is.Not.Null);
            Assert.That(GameObject.Find("Flight Audio Rig"), Is.Not.Null);
        }

        [Test]
        public void SceneAppliesGeneratedWorldMaterialAtlas()
        {
            Texture2D atlas = AssetDatabase.LoadAssetAtPath<Texture2D>(WorldAtlasPath);

            Assert.That(atlas, Is.Not.Null);
            AssertUsesAtlas("Ocean", atlas);
            AssertUsesAtlas("Coastal Road", atlas);
            AssertUsesAtlas("Runway", atlas);
            AssertUsesAtlas("Open Field", atlas);
            AssertUsesAtlas("Beach Emergency Strip", atlas);
            AssertUsesAtlas("Mountain Ridge Wall 0", atlas);
            AssertUsesAtlas("Canyon Left Wall 0", atlas);
            AssertUsesAtlas("City Edge Block 0-0", atlas);
            AssertUsesAtlas("Field Tree Line", atlas);
        }

        [Test]
        public void ProjectContainsImagegenMapAndCarReferenceAssets()
        {
            Texture2D mapConcept = AssetDatabase.LoadAssetAtPath<Texture2D>(WorldMapConceptPath);
            Texture2D carReference = AssetDatabase.LoadAssetAtPath<Texture2D>(CarReferencePath);

            Assert.That(mapConcept, Is.Not.Null);
            Assert.That(mapConcept.width, Is.GreaterThanOrEqualTo(1024));
            Assert.That(carReference, Is.Not.Null);
            Assert.That(carReference.width, Is.GreaterThanOrEqualTo(1024));
        }

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

        private static void AssertUsesAtlas(string objectName, Texture2D atlas)
        {
            GameObject sceneObject = GameObject.Find(objectName);
            Assert.That(sceneObject, Is.Not.Null, objectName);

            Renderer renderer = sceneObject.GetComponent<Renderer>();
            Assert.That(renderer, Is.Not.Null, objectName);
            Assert.That(renderer.sharedMaterial, Is.Not.Null, objectName);

            Material material = renderer.sharedMaterial;
            Texture texture = material.HasProperty("_BaseMap")
                ? material.GetTexture("_BaseMap")
                : material.mainTexture;
            Assert.That(texture, Is.SameAs(atlas), objectName);
        }

        private static void AssertRoofAndFacadeMaterials(string objectName, Texture2D atlas)
        {
            GameObject sceneObject = GameObject.Find(objectName);
            Assert.That(sceneObject, Is.Not.Null, objectName);

            Renderer renderer = sceneObject.GetComponent<Renderer>();
            Assert.That(renderer, Is.Not.Null, objectName);
            Assert.That(renderer.sharedMaterials, Has.Length.GreaterThanOrEqualTo(2), objectName);
            Assert.That(renderer.sharedMaterials[0], Is.Not.Null, objectName + " side material");
            Assert.That(renderer.sharedMaterials[1], Is.Not.Null, objectName + " roof material");
            Assert.That(renderer.sharedMaterials[0], Is.Not.SameAs(renderer.sharedMaterials[1]), objectName);
            Assert.That(GetMaterialTexture(renderer.sharedMaterials[0]), Is.SameAs(atlas), objectName + " side atlas");
            Assert.That(GetMaterialTexture(renderer.sharedMaterials[1]), Is.SameAs(atlas), objectName + " roof atlas");
        }

        private static Texture GetMaterialTexture(Material material)
        {
            return material.HasProperty("_BaseMap")
                ? material.GetTexture("_BaseMap")
                : material.mainTexture;
        }

        private static void AssertVisualOnlyPart(Transform aircraft, string childName)
        {
            Transform part = aircraft.Find(childName);
            Assert.That(part, Is.Not.Null, childName);
            Assert.That(part.GetComponent<Collider>(), Is.Null, childName);
        }

        private static void AssertPhysicsOnlyPart(Transform aircraft, string childName)
        {
            Transform part = aircraft.Find(childName);
            Assert.That(part, Is.Not.Null, childName);
            Assert.That(part.GetComponent<Collider>(), Is.Not.Null, childName);
            Assert.That(part.GetComponent<Renderer>().enabled, Is.False, childName);
        }

        private static void AssertCarVisualOnlyPart(Transform car, string childName)
        {
            Transform part = car.Find(childName);
            Assert.That(part, Is.Not.Null, childName);
            Assert.That(part.GetComponent<Collider>(), Is.Null, childName);
        }

        private static void AssertVisualOnlyWorldMarking(string objectName)
        {
            GameObject marking = GameObject.Find(objectName);
            Assert.That(marking, Is.Not.Null, objectName);
            Assert.That(marking.GetComponent<Renderer>(), Is.Not.Null, objectName);
            Assert.That(marking.GetComponent<Collider>(), Is.Null, objectName);
        }

        private static void AssertLowFieldLip(string objectName)
        {
            GameObject field = GameObject.Find(objectName);
            Assert.That(field, Is.Not.Null, objectName);
            Assert.That(field.GetComponent<SurfaceTag>().kind, Is.EqualTo(SurfaceKind.Field), objectName);
            float top = field.transform.position.y + field.transform.localScale.y * 0.5f;
            Assert.That(top, Is.LessThanOrEqualTo(0.035f),
                $"{objectName} has a raised cube lip that can kick WheelColliders. top={top:F3}");
        }
    }
}

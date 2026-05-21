using System.Linq;
using MINgo.Flight;
using MINgo.Hazards;
using MINgo.Landing;
using MINgo.UI;
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
    }
}

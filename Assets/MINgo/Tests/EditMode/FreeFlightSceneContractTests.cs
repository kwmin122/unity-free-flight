using System.Linq;
using MINgo.Flight;
using MINgo.Hazards;
using MINgo.Landing;
using MINgo.UI;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace MINgo.Tests
{
    public sealed class FreeFlightSceneContractTests
    {
        private const string ScenePath = "Assets/Scenes/FreeFlightSandbox.unity";

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
            Assert.That(hud, Is.Not.Null);
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
    }
}

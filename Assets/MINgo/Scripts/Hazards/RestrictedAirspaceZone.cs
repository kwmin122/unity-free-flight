using MINgo.Flight;
using MINgo.UI;
using UnityEngine;

namespace MINgo.Hazards
{
    public sealed class RestrictedAirspaceZone : MonoBehaviour
    {
        public ArcadeAircraftController aircraft;
        public FlightHud hud;
        public Collider outerZone;
        public Collider deepZone;
        public Transform missileSpawnPoint;
        public float missileSpeed = 55f;
        public float maxTurnDegreesPerSecond = 65f;
        public float missileLifetimeSeconds = 10f;
        public float hitRadiusMeters = 8f;

        private readonly RestrictedAirspaceState state = new RestrictedAirspaceState();
        private MissileThreat activeMissile;

        public RestrictedAirspacePhase CurrentPhase => state.CurrentPhase;

        private void Update()
        {
            if (aircraft == null || outerZone == null || deepZone == null)
            {
                return;
            }

            Vector3 position = aircraft.transform.position;
            bool isInsideOuter = Contains(outerZone, position);
            bool isInsideDeep = Contains(deepZone, position);

            RestrictedAirspacePhase phase = state.Tick(isInsideOuter, isInsideDeep, Time.deltaTime);
            if (hud != null)
            {
                hud.SetRestrictedWarning(FlightHud.FormatRestrictedWarning(phase));
            }

            if (phase == RestrictedAirspacePhase.MissileLaunched && activeMissile == null)
            {
                activeMissile = MissileThreat.Spawn(
                    missileSpawnPoint == null ? transform.position : missileSpawnPoint.position,
                    aircraft,
                    missileSpeed,
                    maxTurnDegreesPerSecond,
                    missileLifetimeSeconds,
                    hitRadiusMeters);
                activeMissile.OnThreatEnded += HandleMissileEnded;
            }
        }

        private void HandleMissileEnded(MissileThreat threat)
        {
            if (activeMissile == threat)
            {
                activeMissile = null;
                state.ResolveMissile();
            }
        }

        private static bool Contains(Collider zone, Vector3 position)
        {
            return zone.bounds.Contains(position);
        }
    }
}

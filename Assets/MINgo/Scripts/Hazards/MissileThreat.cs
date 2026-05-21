using System;
using MINgo.Flight;
using UnityEngine;

namespace MINgo.Hazards
{
    public sealed class MissileThreat : MonoBehaviour
    {
        public ArcadeAircraftController target;
        public float missileSpeed = 55f;
        public float maxTurnDegreesPerSecond = 65f;
        public float missileLifetimeSeconds = 10f;
        public float hitRadiusMeters = 8f;

        private float ageSeconds;
        private bool hasEnded;

        public event Action<MissileThreat> OnThreatEnded;

        public static MissileThreat Spawn(
            Vector3 position,
            ArcadeAircraftController target,
            float speed,
            float maxTurnDegreesPerSecond,
            float lifetimeSeconds,
            float hitRadiusMeters)
        {
            GameObject missileObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            missileObject.name = "Missile Threat";
            missileObject.transform.position = position;
            Vector3 initialDirection = target == null ? Vector3.forward : target.transform.position - position;
            if (initialDirection.sqrMagnitude < 0.001f)
            {
                initialDirection = Vector3.forward;
            }

            missileObject.transform.rotation = target == null
                ? Quaternion.identity
                : Quaternion.LookRotation(initialDirection.normalized, Vector3.up);
            missileObject.transform.localScale = new Vector3(0.8f, 0.8f, 2.8f);

            var renderer = missileObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                var material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
                material.name = "Missile_Threat_Mat";
                material.color = new Color(0.95f, 0.12f, 0.06f);
                renderer.sharedMaterial = material;
            }

            Collider collider = missileObject.GetComponent<Collider>();
            if (collider != null)
            {
                collider.isTrigger = true;
            }

            var trail = missileObject.AddComponent<TrailRenderer>();
            trail.time = 1.25f;
            trail.startWidth = 0.6f;
            trail.endWidth = 0.05f;
            trail.material = new Material(Shader.Find("Sprites/Default"));
            trail.startColor = new Color(0.88f, 0.88f, 0.82f, 0.8f);
            trail.endColor = new Color(0.55f, 0.55f, 0.55f, 0f);

            var missile = missileObject.AddComponent<MissileThreat>();
            missile.target = target;
            missile.missileSpeed = speed;
            missile.maxTurnDegreesPerSecond = maxTurnDegreesPerSecond;
            missile.missileLifetimeSeconds = lifetimeSeconds;
            missile.hitRadiusMeters = hitRadiusMeters;
            return missile;
        }

        private void Update()
        {
            ageSeconds += Time.deltaTime;
            if (ageSeconds >= missileLifetimeSeconds || target == null)
            {
                EndThreat();
                return;
            }

            Vector3 toTarget = target.transform.position - transform.position;
            if (toTarget.sqrMagnitude <= hitRadiusMeters * hitRadiusMeters)
            {
                ApplyHit();
                EndThreat();
                return;
            }

            Quaternion desiredRotation = Quaternion.LookRotation(toTarget.normalized, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, desiredRotation, maxTurnDegreesPerSecond * Time.deltaTime);
            transform.position += transform.forward * (missileSpeed * Time.deltaTime);
        }

        private void ApplyHit()
        {
            if (target.CurrentState == AircraftState.Damaged)
            {
                target.SetAircraftState(AircraftState.Crashed);
            }
            else if (target.CurrentState != AircraftState.Crashed && target.CurrentState != AircraftState.Submerged)
            {
                target.SetAircraftState(AircraftState.Damaged);
            }
        }

        private void EndThreat()
        {
            if (hasEnded)
            {
                return;
            }

            hasEnded = true;
            OnThreatEnded?.Invoke(this);
            Destroy(gameObject);
        }
    }
}

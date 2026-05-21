using MINgo.Flight;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MINgo.World
{
    public sealed class WorldBounds : MonoBehaviour
    {
        public ArcadeAircraftController aircraft;
        public float waterFailureHeight = -2f;
        public Vector3 resetPosition = new Vector3(0f, 2f, -65f);
        public Vector3 resetEulerAngles;

        private Rigidbody aircraftBody;

        public static bool IsBelowFailureHeight(Vector3 position, float failureHeight)
        {
            return position.y < failureHeight;
        }

        private void Awake()
        {
            if (aircraft == null)
            {
                aircraft = FindFirstObjectByType<ArcadeAircraftController>();
            }

            aircraftBody = aircraft == null ? null : aircraft.GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            if (aircraft == null)
            {
                return;
            }

            if (IsBelowFailureHeight(aircraft.transform.position, waterFailureHeight))
            {
                aircraft.SetAircraftState(AircraftState.Submerged);
            }
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard.rKey.wasPressedThisFrame)
            {
                ResetAircraft();
            }
        }

        private void ResetAircraft()
        {
            if (aircraft == null)
            {
                return;
            }

            aircraft.transform.SetPositionAndRotation(resetPosition, Quaternion.Euler(resetEulerAngles));
            if (aircraftBody != null)
            {
                aircraftBody.linearVelocity = Vector3.zero;
                aircraftBody.angularVelocity = Vector3.zero;
            }

            aircraft.SetAircraftState(AircraftState.Grounded);
        }
    }
}

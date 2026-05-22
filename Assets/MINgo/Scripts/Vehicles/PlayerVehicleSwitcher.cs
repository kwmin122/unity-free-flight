using MINgo.Flight;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MINgo.Vehicles
{
    public sealed class PlayerVehicleSwitcher : MonoBehaviour
    {
        public ArcadeAircraftController aircraft;
        public ArcadeCarController car;
        public ChaseCameraRig cameraRig;
        public bool startInAircraft = true;

        private bool usingAircraft = true;

        public bool UsingAircraft => usingAircraft;

        private void Start()
        {
            SetActiveVehicle(startInAircraft);
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && (keyboard.fKey.wasPressedThisFrame || keyboard.tabKey.wasPressedThisFrame))
            {
                SetActiveVehicle(!usingAircraft);
            }
        }

        public void SetActiveVehicle(bool useAircraft)
        {
            usingAircraft = useAircraft;

            if (aircraft != null)
            {
                aircraft.acceptsInput = useAircraft;
            }

            if (car != null)
            {
                car.acceptsInput = !useAircraft;
            }

            if (cameraRig != null)
            {
                Transform target = useAircraft
                    ? aircraft == null ? null : aircraft.transform
                    : car == null ? null : car.transform;
                if (target != null)
                {
                    cameraRig.SetTarget(target, snap: true);
                }
            }
        }
    }
}

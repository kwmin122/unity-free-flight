using UnityEngine.InputSystem;

namespace MINgo.Vehicles
{
    public readonly struct VehicleInputSnapshot
    {
        public VehicleInputSnapshot(float throttle, float steer, bool handbrake, bool switchVehicle)
        {
            Throttle = throttle;
            Steer = steer;
            Handbrake = handbrake;
            SwitchVehicle = switchVehicle;
        }

        public float Throttle { get; }
        public float Steer { get; }
        public bool Handbrake { get; }
        public bool SwitchVehicle { get; }
    }

    public static class VehicleInputReader
    {
        private static VehicleInputSnapshot? inputOverride;

        public static VehicleInputSnapshot ReadKeyboard()
        {
            if (inputOverride.HasValue)
            {
                return inputOverride.Value;
            }

            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return new VehicleInputSnapshot(0f, 0f, false, false);
            }

            return CreateKeyboardSnapshot(
                accelerate: keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed,
                brakeOrReverse: keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed,
                steerLeft: keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed,
                steerRight: keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed,
                handbrake: keyboard.spaceKey.isPressed,
                switchVehicle: keyboard.fKey.wasPressedThisFrame || keyboard.tabKey.wasPressedThisFrame);
        }

        public static VehicleInputSnapshot CreateKeyboardSnapshot(
            bool accelerate,
            bool brakeOrReverse,
            bool steerLeft,
            bool steerRight,
            bool handbrake,
            bool switchVehicle)
        {
            float throttle = 0f;
            if (accelerate)
            {
                throttle += 1f;
            }

            if (brakeOrReverse)
            {
                throttle -= 1f;
            }

            float steer = 0f;
            if (steerLeft)
            {
                steer -= 1f;
            }

            if (steerRight)
            {
                steer += 1f;
            }

            return new VehicleInputSnapshot(throttle, steer, handbrake, switchVehicle);
        }

        public static void SetInputOverrideForTests(VehicleInputSnapshot input)
        {
            inputOverride = input;
        }

        public static void ClearInputOverrideForTests()
        {
            inputOverride = null;
        }
    }
}

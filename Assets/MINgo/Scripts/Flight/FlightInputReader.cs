using UnityEngine.InputSystem;

namespace MINgo.Flight
{
    public readonly struct FlightInputSnapshot
    {
        public FlightInputSnapshot(float pitch, float roll, float yaw, float throttleDelta, bool brake)
        {
            Pitch = pitch;
            Roll = roll;
            Yaw = yaw;
            ThrottleDelta = throttleDelta;
            Brake = brake;
        }

        public float Pitch { get; }
        public float Roll { get; }
        public float Yaw { get; }
        public float ThrottleDelta { get; }
        public bool Brake { get; }
    }

    public static class FlightInputReader
    {
        public static FlightInputSnapshot ReadKeyboard()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return new FlightInputSnapshot(0f, 0f, 0f, 0f, false);
            }

            float pitch = 0f;
            if (keyboard.wKey.isPressed)
            {
                pitch -= 1f;
            }

            if (keyboard.sKey.isPressed)
            {
                pitch += 1f;
            }

            float roll = 0f;
            if (keyboard.aKey.isPressed)
            {
                roll -= 1f;
            }

            if (keyboard.dKey.isPressed)
            {
                roll += 1f;
            }

            float yaw = 0f;
            if (keyboard.qKey.isPressed)
            {
                yaw -= 1f;
            }

            if (keyboard.eKey.isPressed)
            {
                yaw += 1f;
            }

            float throttleDelta = 0f;
            if (keyboard.leftShiftKey.isPressed || keyboard.upArrowKey.isPressed)
            {
                throttleDelta += 1f;
            }

            if (keyboard.leftCtrlKey.isPressed || keyboard.downArrowKey.isPressed)
            {
                throttleDelta -= 1f;
            }

            return new FlightInputSnapshot(pitch, roll, yaw, throttleDelta, keyboard.spaceKey.isPressed);
        }
    }
}

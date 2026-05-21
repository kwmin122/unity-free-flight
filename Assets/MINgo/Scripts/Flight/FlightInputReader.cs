using UnityEngine.InputSystem;

namespace MINgo.Flight
{
    public readonly struct FlightInputSnapshot
    {
        public FlightInputSnapshot(float pitch, float roll, float yaw, float throttleDelta, bool brake)
            : this(pitch, roll, yaw, 0f, throttleDelta, brake)
        {
        }

        public FlightInputSnapshot(float pitch, float roll, float yaw, float turn, float throttleDelta, bool brake)
        {
            Pitch = pitch;
            Roll = roll;
            Yaw = yaw;
            Turn = turn;
            ThrottleDelta = throttleDelta;
            Brake = brake;
        }

        public float Pitch { get; }
        public float Roll { get; }
        public float Yaw { get; }
        public float Turn { get; }
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
            if (keyboard.upArrowKey.isPressed)
            {
                pitch -= 1f;
            }

            if (keyboard.downArrowKey.isPressed)
            {
                pitch += 1f;
            }

            float roll = 0f;
            if (keyboard.qKey.isPressed)
            {
                roll -= 1f;
            }

            if (keyboard.eKey.isPressed)
            {
                roll += 1f;
            }

            float yaw = 0f;
            float turn = 0f;
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
            {
                turn -= 1f;
            }

            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
            {
                turn += 1f;
            }

            float throttleDelta = 0f;
            if (keyboard.wKey.isPressed || keyboard.leftShiftKey.isPressed)
            {
                throttleDelta += 1f;
            }

            if (keyboard.sKey.isPressed || keyboard.leftCtrlKey.isPressed)
            {
                throttleDelta -= 1f;
            }

            return new FlightInputSnapshot(pitch, roll, yaw, turn, throttleDelta, keyboard.spaceKey.isPressed || keyboard.sKey.isPressed);
        }
    }
}

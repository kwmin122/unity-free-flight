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
        private static FlightInputSnapshot? inputOverride;

        public static FlightInputSnapshot ReadKeyboard()
        {
            if (inputOverride.HasValue)
            {
                return inputOverride.Value;
            }

            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return new FlightInputSnapshot(0f, 0f, 0f, 0f, false);
            }

            return CreateKeyboardSnapshot(
                pitchUp: keyboard.upArrowKey.isPressed,
                pitchDown: keyboard.downArrowKey.isPressed,
                rollLeft: keyboard.qKey.isPressed,
                rollRight: keyboard.eKey.isPressed,
                turnLeft: keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed,
                turnRight: keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed,
                throttleUp: keyboard.wKey.isPressed || keyboard.leftShiftKey.isPressed,
                throttleDown: keyboard.sKey.isPressed || keyboard.leftCtrlKey.isPressed,
                brake: keyboard.spaceKey.isPressed);
        }

        public static FlightInputSnapshot CreateKeyboardSnapshot(
            bool pitchUp,
            bool pitchDown,
            bool rollLeft,
            bool rollRight,
            bool turnLeft,
            bool turnRight,
            bool throttleUp,
            bool throttleDown,
            bool brake)
        {
            float pitch = 0f;
            if (pitchUp)
            {
                pitch -= 1f;
            }

            if (pitchDown)
            {
                pitch += 1f;
            }

            float roll = 0f;
            if (rollLeft)
            {
                roll -= 1f;
            }

            if (rollRight)
            {
                roll += 1f;
            }

            float yaw = 0f;
            float turn = 0f;
            if (turnLeft)
            {
                turn -= 1f;
            }

            if (turnRight)
            {
                turn += 1f;
            }

            float throttleDelta = 0f;
            if (throttleUp)
            {
                throttleDelta += 1f;
            }

            if (throttleDown)
            {
                throttleDelta -= 1f;
            }

            return new FlightInputSnapshot(pitch, roll, yaw, turn, throttleDelta, brake);
        }

        public static void SetInputOverrideForTests(FlightInputSnapshot input)
        {
            inputOverride = input;
        }

        public static void ClearInputOverrideForTests()
        {
            inputOverride = null;
        }
    }
}

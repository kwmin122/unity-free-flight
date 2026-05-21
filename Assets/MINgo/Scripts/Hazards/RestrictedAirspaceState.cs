namespace MINgo.Hazards
{
    public enum RestrictedAirspacePhase
    {
        Outside,
        Warning,
        Locking,
        MissileLaunched,
        Escaped
    }

    public sealed class RestrictedAirspaceState
    {
        private const float LockSeconds = 3f;
        private const float LaunchSeconds = 6f;
        private const float EscapeSeconds = 1.5f;
        private const float EscapedHoldSeconds = 2f;

        private float deepZoneSeconds;
        private float outsideSeconds;
        private float escapedSeconds;

        public RestrictedAirspacePhase CurrentPhase { get; private set; } = RestrictedAirspacePhase.Outside;
        public bool HasActiveMissile { get; private set; }

        public RestrictedAirspacePhase Tick(bool isInsideOuterZone, bool isInsideDeepZone, float deltaSeconds)
        {
            float dt = deltaSeconds < 0f ? 0f : deltaSeconds;

            if (HasActiveMissile)
            {
                CurrentPhase = RestrictedAirspacePhase.MissileLaunched;
                return CurrentPhase;
            }

            if (isInsideDeepZone)
            {
                outsideSeconds = 0f;
                escapedSeconds = 0f;
                deepZoneSeconds += dt;

                if (deepZoneSeconds >= LaunchSeconds)
                {
                    HasActiveMissile = true;
                    CurrentPhase = RestrictedAirspacePhase.MissileLaunched;
                }
                else if (deepZoneSeconds >= LockSeconds)
                {
                    CurrentPhase = RestrictedAirspacePhase.Locking;
                }
                else
                {
                    CurrentPhase = RestrictedAirspacePhase.Warning;
                }

                return CurrentPhase;
            }

            if (isInsideOuterZone)
            {
                deepZoneSeconds = 0f;
                outsideSeconds = 0f;
                escapedSeconds = 0f;
                CurrentPhase = RestrictedAirspacePhase.Warning;
                return CurrentPhase;
            }

            if (CurrentPhase == RestrictedAirspacePhase.Warning || CurrentPhase == RestrictedAirspacePhase.Locking)
            {
                outsideSeconds += dt;
                if (outsideSeconds >= EscapeSeconds)
                {
                    deepZoneSeconds = 0f;
                    escapedSeconds = 0f;
                    CurrentPhase = RestrictedAirspacePhase.Escaped;
                }
            }
            else if (CurrentPhase == RestrictedAirspacePhase.Escaped)
            {
                deepZoneSeconds = 0f;
                escapedSeconds += dt;
                if (escapedSeconds >= EscapedHoldSeconds)
                {
                    outsideSeconds = 0f;
                    CurrentPhase = RestrictedAirspacePhase.Outside;
                }
            }
            else
            {
                deepZoneSeconds = 0f;
                outsideSeconds = 0f;
                escapedSeconds = 0f;
                CurrentPhase = RestrictedAirspacePhase.Outside;
            }

            return CurrentPhase;
        }

        public void ResolveMissile()
        {
            HasActiveMissile = false;
            deepZoneSeconds = 0f;
            outsideSeconds = 0f;
            escapedSeconds = 0f;
            CurrentPhase = RestrictedAirspacePhase.Outside;
        }
    }
}

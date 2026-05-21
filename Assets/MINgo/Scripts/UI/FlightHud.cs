using MINgo.Hazards;
using MINgo.Flight;
using MINgo.Landing;
using UnityEngine;
using UnityEngine.UI;

namespace MINgo.UI
{
    public sealed class FlightHud : MonoBehaviour
    {
        public ArcadeAircraftController aircraft;
        public LandingStateMachine landing;
        public Text speedText;
        public Text altitudeText;
        public Text stateText;
        public Text contextText;
        public Text warningText;
        public float contextVisibleSeconds = 4f;

        private LandingContext lastContext = LandingContext.None;
        private float contextTimer;
        private string warningMessage = string.Empty;

        private void Update()
        {
            if (aircraft == null)
            {
                return;
            }

            SetText(speedText, "Speed " + FormatSpeed(aircraft.SpeedMetersPerSecond));
            SetText(altitudeText, "Altitude " + FormatAltitude(aircraft.AltitudeMeters));
            SetText(stateText, "State " + aircraft.CurrentState + "  Throttle " + Mathf.RoundToInt(aircraft.Throttle01 * 100f) + "%");

            LandingContext currentContext = landing == null ? LandingContext.None : landing.LastContext;
            UpdateContext(currentContext, Time.deltaTime);
            UpdateWarningText();
        }

        public void SetRestrictedWarning(string message)
        {
            warningMessage = message ?? string.Empty;
            UpdateWarningText();
        }

        public static string FormatSpeed(float metersPerSecond)
        {
            return Mathf.Max(0f, metersPerSecond).ToString("0") + " m/s";
        }

        public static string FormatAltitude(float meters)
        {
            return Mathf.Max(0f, meters).ToString("0") + " m";
        }

        public static string FormatContextLabel(LandingContext context)
        {
            return context switch
            {
                LandingContext.RunwayLanding => "Runway landing",
                LandingContext.RoadLanding => "Road landing",
                LandingContext.FieldLanding => "Field landing",
                LandingContext.RidgeLanding => "Ridge landing",
                LandingContext.CanyonFloorLanding => "Canyon floor landing",
                LandingContext.RoughLanding => "Rough landing",
                LandingContext.EmergencyLanding => "Emergency landing",
                LandingContext.Submerged => "Submerged",
                LandingContext.RestrictedAirspace => "Restricted airspace",
                _ => string.Empty
            };
        }

        public static string FormatRestrictedWarning(RestrictedAirspacePhase phase)
        {
            return phase switch
            {
                RestrictedAirspacePhase.Warning => "Restricted airspace",
                RestrictedAirspacePhase.Locking => "Lock-on",
                RestrictedAirspacePhase.MissileLaunched => "Missile launched",
                RestrictedAirspacePhase.Escaped => "Escaped",
                _ => string.Empty
            };
        }

        private void UpdateContext(LandingContext currentContext, float deltaTime)
        {
            if (currentContext != LandingContext.None && currentContext != lastContext)
            {
                lastContext = currentContext;
                contextTimer = contextVisibleSeconds;
            }

            if (contextTimer > 0f)
            {
                contextTimer -= deltaTime;
                SetText(contextText, FormatContextLabel(lastContext));
            }
            else
            {
                SetText(contextText, FormatControlHint());
            }
        }

        public static string FormatControlHint()
        {
            return "W/S throttle  A/D turn  Up/Down pitch  Q/E roll  Space brake";
        }

        private void UpdateWarningText()
        {
            SetText(warningText, warningMessage);
            if (warningText != null)
            {
                warningText.enabled = !string.IsNullOrEmpty(warningMessage);
            }
        }

        private static void SetText(Text target, string value)
        {
            if (target != null)
            {
                target.text = value;
            }
        }
    }
}

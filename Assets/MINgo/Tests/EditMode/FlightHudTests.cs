using MINgo.Hazards;
using MINgo.Landing;
using MINgo.UI;
using NUnit.Framework;

namespace MINgo.Tests
{
    public sealed class FlightHudTests
    {
        [TestCase(LandingContext.RunwayLanding, "Runway landing")]
        [TestCase(LandingContext.RoadLanding, "Road landing")]
        [TestCase(LandingContext.FieldLanding, "Field landing")]
        [TestCase(LandingContext.RidgeLanding, "Ridge landing")]
        [TestCase(LandingContext.CanyonFloorLanding, "Canyon floor landing")]
        [TestCase(LandingContext.RoughLanding, "Rough landing")]
        [TestCase(LandingContext.EmergencyLanding, "Emergency landing")]
        [TestCase(LandingContext.Submerged, "Submerged")]
        public void FormatContextLabel_ReturnsReadableLandingLabel(LandingContext context, string expected)
        {
            Assert.AreEqual(expected, FlightHud.FormatContextLabel(context));
        }

        [Test]
        public void FormatContextLabel_HidesEmptyContext()
        {
            Assert.AreEqual(string.Empty, FlightHud.FormatContextLabel(LandingContext.None));
        }

        [TestCase(RestrictedAirspacePhase.Warning, "Restricted airspace")]
        [TestCase(RestrictedAirspacePhase.Locking, "Lock-on")]
        [TestCase(RestrictedAirspacePhase.MissileLaunched, "Missile launched")]
        [TestCase(RestrictedAirspacePhase.Escaped, "Escaped")]
        public void FormatRestrictedWarning_ReturnsHudWarning(RestrictedAirspacePhase phase, string expected)
        {
            Assert.AreEqual(expected, FlightHud.FormatRestrictedWarning(phase));
        }

        [Test]
        public void FormatControlHint_DescribesEasyFlightControls()
        {
            string hint = FlightHud.FormatControlHint();

            Assert.That(hint, Does.Contain("W"));
            Assert.That(hint, Does.Contain("S"));
            Assert.That(hint, Does.Contain("A/D"));
            Assert.That(hint, Does.Not.Contain("Q/E"));
            Assert.That(hint, Does.Contain("Space"));
        }
    }
}

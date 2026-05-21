using MINgo.Flight;
using NUnit.Framework;
using UnityEngine;

namespace MINgo.Tests
{
    public sealed class FlightAerodynamicsTests
    {
        [Test]
        public void CalculateAngleOfAttack_ReturnsPositiveWhenNoseIsAboveVelocity()
        {
            float angleOfAttack = FlightAerodynamics.CalculateAngleOfAttackDegrees(new Vector3(0f, -1f, 10f));

            Assert.That(angleOfAttack, Is.GreaterThan(0f));
        }

        [Test]
        public void CalculateLiftDirection_BanksWithAircraftRightAxis()
        {
            Vector3 bankedRight = Quaternion.AngleAxis(45f, Vector3.forward) * Vector3.right;

            Vector3 liftDirection = FlightAerodynamics.CalculateLiftDirection(Vector3.forward, bankedRight);

            Assert.That(liftDirection.x, Is.LessThan(0f));
            Assert.That(liftDirection.y, Is.GreaterThan(0f));
            Assert.That(liftDirection.magnitude, Is.EqualTo(1f).Within(0.001f));
        }

        [Test]
        public void CalculateLiftForce_UsesAirspeedSquared()
        {
            Vector3 lowSpeedLift = FlightAerodynamics.CalculateLiftForce(Vector3.forward * 10f, Vector3.right, 0.5f, 0.1f);
            Vector3 highSpeedLift = FlightAerodynamics.CalculateLiftForce(Vector3.forward * 20f, Vector3.right, 0.5f, 0.1f);

            Assert.That(highSpeedLift.magnitude / lowSpeedLift.magnitude, Is.EqualTo(4f).Within(0.001f));
        }

        [Test]
        public void EvaluateLiftCoefficient_ReducesAfterStall()
        {
            float normalLift = FlightAerodynamics.EvaluateLiftCoefficient(10f, -2f, 0.08f, 16f, 1.2f);
            float stalledLift = FlightAerodynamics.EvaluateLiftCoefficient(28f, -2f, 0.08f, 16f, 1.2f);

            Assert.That(stalledLift, Is.LessThan(normalLift));
        }

        [Test]
        public void CalculateDragForce_AddsInducedDragWhenLiftCoefficientIncreases()
        {
            Vector3 baseDrag = FlightAerodynamics.CalculateDragForce(Vector3.forward * 20f, 0.01f, 0.04f, 0f);
            Vector3 inducedDrag = FlightAerodynamics.CalculateDragForce(Vector3.forward * 20f, 0.01f, 0.04f, 1f);

            Assert.That(inducedDrag.magnitude, Is.GreaterThan(baseDrag.magnitude));
        }
    }
}

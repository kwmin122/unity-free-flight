using MINgo.Flight;
using NUnit.Framework;
using UnityEngine;

namespace MINgo.Tests
{
    public sealed class ChaseCameraRigTests
    {
        [Test]
        public void CalculateChaseForward_DampensSteepPitchTowardHorizon()
        {
            Vector3 steepAircraftForward = Quaternion.AngleAxis(-65f, Vector3.right) * Vector3.forward;

            Vector3 chaseForward = ChaseCameraRig.CalculateChaseForward(
                steepAircraftForward,
                velocity: Vector3.zero,
                pitchFollow: 0.35f);

            Assert.That(chaseForward.y, Is.GreaterThan(0f));
            Assert.That(chaseForward.y, Is.LessThan(steepAircraftForward.y));
        }

        [Test]
        public void CalculateChaseForward_FallsBackToAircraftHeadingWhenVelocityIsSlow()
        {
            Vector3 aircraftForward = Quaternion.Euler(0f, 35f, 0f) * Vector3.forward;

            Vector3 chaseForward = ChaseCameraRig.CalculateChaseForward(
                aircraftForward,
                velocity: Vector3.forward,
                pitchFollow: 0.35f);

            Assert.That(Vector3.Dot(chaseForward, aircraftForward.normalized), Is.GreaterThan(0.9f));
        }
    }
}

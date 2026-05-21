using MINgo.World;
using NUnit.Framework;
using UnityEngine;

namespace MINgo.Tests
{
    public sealed class WorldBoundsTests
    {
        [Test]
        public void PositionBelowFailureHeightIsOutOfBounds()
        {
            Assert.IsTrue(WorldBounds.IsBelowFailureHeight(new Vector3(0f, -2.1f, 0f), -2f));
        }

        [Test]
        public void PositionAtOrAboveFailureHeightIsInBounds()
        {
            Assert.IsFalse(WorldBounds.IsBelowFailureHeight(new Vector3(0f, -2f, 0f), -2f));
            Assert.IsFalse(WorldBounds.IsBelowFailureHeight(new Vector3(0f, 1f, 0f), -2f));
        }
    }
}

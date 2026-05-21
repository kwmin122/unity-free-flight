using MINgo.Audio;
using NUnit.Framework;
using UnityEngine;

namespace MINgo.Tests
{
    public sealed class ProceduralFlightAudioTests
    {
        [Test]
        public void CreateEngineLoop_ReturnsNonSilentLoopingClip()
        {
            AudioClip clip = ProceduralFlightAudio.CreateEngineLoop("Test Engine", 22050, 0.25f, 70f, 0.5f);
            var samples = new float[clip.samples * clip.channels];

            bool read = clip.GetData(samples, 0);

            Assert.That(read, Is.True);
            Assert.That(clip.loadType, Is.EqualTo(AudioClipLoadType.DecompressOnLoad));
            Assert.That(clip.samples, Is.GreaterThan(0));
            Assert.That(samples, Has.Some.Not.EqualTo(0f));
        }

        [Test]
        public void CreateAmbientMusicLoop_ReturnsStereoClip()
        {
            AudioClip clip = ProceduralFlightAudio.CreateAmbientMusicLoop("Test Music", 22050, 0.5f, 0.35f);

            Assert.That(clip.channels, Is.EqualTo(2));
            Assert.That(clip.length, Is.GreaterThan(0.45f));
        }
    }
}

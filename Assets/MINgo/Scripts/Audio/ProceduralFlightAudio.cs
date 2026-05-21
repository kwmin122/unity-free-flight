using MINgo.Flight;
using UnityEngine;

namespace MINgo.Audio
{
    public sealed class ProceduralFlightAudio : MonoBehaviour
    {
        public ArcadeAircraftController aircraft;
        public AudioSource engineSource;
        public AudioSource windSource;
        public AudioSource musicSource;
        public float masterVolume = 0.65f;

        private void Awake()
        {
            engineSource = EnsureSource(engineSource, "Engine Audio Source", spatialBlend: 0.4f);
            windSource = EnsureSource(windSource, "Wind Audio Source", spatialBlend: 0.25f);
            musicSource = EnsureSource(musicSource, "Music Audio Source", spatialBlend: 0f);
        }

        private void Start()
        {
            if (engineSource.clip == null)
            {
                engineSource.clip = CreateEngineLoop("Procedural Engine Loop", 44100, 1f, 80f, 0.55f);
            }

            if (windSource.clip == null)
            {
                windSource.clip = CreateWindLoop("Procedural Wind Loop", 44100, 1f, 0.28f);
            }

            if (musicSource.clip == null)
            {
                musicSource.clip = CreateAmbientMusicLoop("Procedural Ambient Music Loop", 44100, 8f, 0.35f);
            }

            PlayLoop(engineSource);
            PlayLoop(windSource);
            PlayLoop(musicSource);
        }

        private void Update()
        {
            if (aircraft == null)
            {
                return;
            }

            float throttle = aircraft.Throttle01;
            float speed01 = Mathf.Clamp01(aircraft.SpeedMetersPerSecond / 45f);

            engineSource.volume = masterVolume * Mathf.Lerp(0.18f, 0.72f, throttle);
            engineSource.pitch = Mathf.Lerp(0.82f, 1.55f, throttle);
            windSource.volume = masterVolume * Mathf.Lerp(0.02f, 0.32f, speed01);
            windSource.pitch = Mathf.Lerp(0.72f, 1.25f, speed01);
            musicSource.volume = masterVolume * 0.16f;
        }

        public static AudioClip CreateEngineLoop(string name, int sampleRate, float durationSeconds, float baseFrequency, float volume)
        {
            int sampleCount = Mathf.Max(1, Mathf.RoundToInt(sampleRate * durationSeconds));
            var data = new float[sampleCount];
            for (int i = 0; i < sampleCount; i++)
            {
                float t = i / (float)sampleRate;
                float engine = Mathf.Sin(Mathf.PI * 2f * baseFrequency * t)
                    + Mathf.Sin(Mathf.PI * 2f * baseFrequency * 2f * t) * 0.35f
                    + Mathf.Sin(Mathf.PI * 2f * baseFrequency * 3f * t) * 0.18f;
                float pulse = 0.78f + 0.22f * Mathf.Sin(Mathf.PI * 2f * 12f * t);
                data[i] = Mathf.Clamp(engine * pulse * volume * 0.55f, -1f, 1f);
            }

            AudioClip clip = AudioClip.Create(name, sampleCount, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        public static AudioClip CreateWindLoop(string name, int sampleRate, float durationSeconds, float volume)
        {
            int sampleCount = Mathf.Max(1, Mathf.RoundToInt(sampleRate * durationSeconds));
            var data = new float[sampleCount];
            for (int i = 0; i < sampleCount; i++)
            {
                float t = i / (float)sampleRate;
                float gust = Mathf.Sin(Mathf.PI * 2f * 0.7f * t) * 0.35f + Mathf.Sin(Mathf.PI * 2f * 2.1f * t) * 0.12f;
                float hiss = Mathf.Sin(Mathf.PI * 2f * 640f * t) * 0.16f + Mathf.Sin(Mathf.PI * 2f * 920f * t) * 0.08f;
                data[i] = Mathf.Clamp((gust + hiss) * volume, -1f, 1f);
            }

            AudioClip clip = AudioClip.Create(name, sampleCount, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        public static AudioClip CreateAmbientMusicLoop(string name, int sampleRate, float durationSeconds, float volume)
        {
            int frameCount = Mathf.Max(1, Mathf.RoundToInt(sampleRate * durationSeconds));
            var data = new float[frameCount * 2];
            for (int i = 0; i < frameCount; i++)
            {
                float t = i / (float)sampleRate;
                float swell = 0.65f + 0.35f * Mathf.Sin(Mathf.PI * 2f * 0.125f * t);
                float chord = Mathf.Sin(Mathf.PI * 2f * 55f * t) * 0.45f
                    + Mathf.Sin(Mathf.PI * 2f * 82.5f * t) * 0.3f
                    + Mathf.Sin(Mathf.PI * 2f * 110f * t) * 0.22f;
                float shimmer = Mathf.Sin(Mathf.PI * 2f * 220f * t) * 0.08f;
                float sample = Mathf.Clamp((chord + shimmer) * swell * volume * 0.45f, -1f, 1f);
                data[i * 2] = sample;
                data[i * 2 + 1] = sample * 0.92f;
            }

            AudioClip clip = AudioClip.Create(name, frameCount, 2, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioSource EnsureSource(AudioSource source, string name, float spatialBlend)
        {
            if (source != null)
            {
                return source;
            }

            var sourceObject = new GameObject(name);
            sourceObject.transform.SetParent(transform, false);
            source = sourceObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = true;
            source.spatialBlend = spatialBlend;
            source.rolloffMode = AudioRolloffMode.Linear;
            source.minDistance = 5f;
            source.maxDistance = 250f;
            return source;
        }

        private static void PlayLoop(AudioSource source)
        {
            source.loop = true;
            if (!source.isPlaying)
            {
                source.Play();
            }
        }
    }
}

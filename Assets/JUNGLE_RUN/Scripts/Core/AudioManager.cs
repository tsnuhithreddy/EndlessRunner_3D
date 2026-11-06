using System.Collections.Generic;
using UnityEngine;

namespace JungleRun
{
    /// <summary>Self-contained audio manager. Procedural clips are generated when no imported clips are assigned.</summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource bgmSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource collisionSource;
        [Header("Default Mix")]
        [Range(0f, 1f)] [SerializeField] private float masterVolume = 1f;
        [Range(0f, 1f)] [SerializeField] private float musicVolume = 0.7f;
        [Range(0f, 1f)] [SerializeField] private float sfxVolume = 0.8f;

        private readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();
        private const int SampleRate = 22050;

        public float MasterVolume => masterVolume;
        public float MusicVolume => musicVolume;
        public float SfxVolume => sfxVolume;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            AudioSource[] sources = GetComponents<AudioSource>();
            if (bgmSource == null) bgmSource = sources.Length > 0 ? sources[0] : gameObject.AddComponent<AudioSource>();
            if (sfxSource == null) sfxSource = sources.Length > 1 ? sources[1] : gameObject.AddComponent<AudioSource>();
            if (collisionSource == null) collisionSource = gameObject.AddComponent<AudioSource>();
            collisionSource.playOnAwake = false;
            collisionSource.loop = false;
            collisionSource.spatialBlend = 0f;
            collisionSource.priority = 0;
            collisionSource.ignoreListenerPause = true;
            bgmSource.loop = true;
            bgmSource.playOnAwake = false;
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;

            masterVolume = PlayerPrefs.GetFloat("Jungle_MasterVolume", 1f);
            musicVolume = PlayerPrefs.GetFloat("Jungle_MusicVolume", 0.7f);
            sfxVolume = PlayerPrefs.GetFloat("Jungle_SfxVolume", 0.8f);
            LoadAuthoredSounds();
            GenerateBuiltInSounds();
            ApplyVolumes();
        }

        private void LoadAuthoredSounds()
        {
            // Imported WAV clips provide fuller, less synthetic sounds than the
            // procedural fallback. Keep fallbacks so the project still runs if
            // an audio asset is accidentally removed.
            string[] names = { "CoinCollect", "GemCollect", "PlayerCrash", "ButtonSelect", "Powerup", "JungleExpedition" };
            foreach (string clipName in names)
            {
                AudioClip loaded = Resources.Load<AudioClip>("Audio/" + clipName);
                if (loaded != null) clips[clipName] = loaded;
            }
            bgmSource.spatialBlend = 0f;
            bgmSource.dopplerLevel = 0f;
            sfxSource.spatialBlend = 0f;
            sfxSource.dopplerLevel = 0f;
            sfxSource.playOnAwake = false;
            sfxSource.ignoreListenerPause = true;
        }

        private void GenerateBuiltInSounds()
        {
            if (!clips.ContainsKey("CoinCollect")) clips["CoinCollect"] = MakeTone("CoinCollect", 0.24f, 1050f, 1650f, 0.22f, true);
            if (!clips.ContainsKey("GemCollect")) clips["GemCollect"] = MakeTone("GemCollect", 0.48f, 620f, 1850f, 0.30f, true);
            if (!clips.ContainsKey("PlayerCrash")) clips["PlayerCrash"] = MakeTone("PlayerCrash", 0.42f, 150f, 48f, 0.34f, false);
            if (!clips.ContainsKey("ButtonSelect")) clips["ButtonSelect"] = MakeTone("ButtonSelect", 0.075f, 720f, 980f, 0.16f, true);
            if (!clips.ContainsKey("Powerup")) clips["Powerup"] = MakeTone("Powerup", 0.34f, 440f, 1320f, 0.22f, true);
            if (!clips.ContainsKey("JungleExpedition")) clips["JungleExpedition"] = MakeJungleAmbience(14f);
        }

        private AudioClip MakeTone(string name, float duration, float startHz, float endHz, float amplitude, bool bright)
        {
            int count = Mathf.CeilToInt(SampleRate * duration);
            float[] data = new float[count];
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)SampleRate;
                float p = t / duration;
                float hz = Mathf.Lerp(startHz, endHz, p);
                float envelope = Mathf.Sin(Mathf.PI * p);
                float fundamental = Mathf.Sin(2f * Mathf.PI * hz * t);
                float overtone = bright ? 0.35f * Mathf.Sin(2f * Mathf.PI * hz * 2.01f * t) : 0.08f * Mathf.Sin(2f * Mathf.PI * hz * 0.5f * t);
                data[i] = (fundamental + overtone) * envelope * amplitude;
            }
            AudioClip clip = AudioClip.Create(name, count, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip MakeJungleAmbience(float duration)
        {
            int count = Mathf.CeilToInt(SampleRate * duration);
            float[] data = new float[count];
            System.Random random = new System.Random(21);
            float[] chirpStarts = { 1.2f, 4.6f, 8.1f, 11.5f };
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)SampleRate;
                float wind = 0.025f * Mathf.Sin(2f * Mathf.PI * 110f * t)
                           + 0.012f * Mathf.Sin(2f * Mathf.PI * 165f * t);
                float insects = 0.008f * Mathf.Sin(2f * Mathf.PI * (2600f + 120f * Mathf.Sin(t * 0.7f)) * t);

                // A soft original pentatonic marimba motif and hand-drum pulse
                // gives the ambience a gentle adventure-music feel.
                float[] melody = { 293.66f, 349.23f, 440f, 523.25f, 440f, 392f, 329.63f };
                float noteTime = t % 0.5f;
                int noteIndex = ((int)(t / 0.5f)) % melody.Length;
                float noteEnvelope = Mathf.Exp(-noteTime * 5.5f);
                float melodyTone = 0.022f * noteEnvelope * Mathf.Sin(2f * Mathf.PI * melody[noteIndex] * noteTime);
                float beatTime = t % 0.875f;
                float drum = 0.045f * Mathf.Exp(-beatTime * 13f) * Mathf.Sin(2f * Mathf.PI * (68f - beatTime * 24f) * beatTime);
                float birds = 0f;
                for (int c = 0; c < chirpStarts.Length; c++)
                {
                    float local = t - chirpStarts[c];
                    if (local >= 0f && local < 0.42f)
                    {
                        float env = Mathf.Sin(Mathf.PI * local / 0.42f);
                        float freq = 1900f + 700f * Mathf.Sin(local * 16f);
                        birds += 0.045f * env * Mathf.Sin(2f * Mathf.PI * freq * local);
                    }
                }
                float noise = ((float)random.NextDouble() * 2f - 1f) * 0.003f;
                data[i] = wind + insects + birds + melodyTone + drum + noise;
            }
            AudioClip clip = AudioClip.Create("JungleExpedition", count, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private void ApplyVolumes()
        {
            AudioListener.volume = masterVolume;
            if (bgmSource != null) bgmSource.volume = musicVolume;
        }

        public void SetMasterVolume(float value)
        {
            masterVolume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat("Jungle_MasterVolume", masterVolume);
            ApplyVolumes();
        }

        public void SetMusicVolume(float value)
        {
            musicVolume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat("Jungle_MusicVolume", musicVolume);
            ApplyVolumes();
        }

        public void SetSfxVolume(float value)
        {
            sfxVolume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat("Jungle_SfxVolume", sfxVolume);
        }

        public void PlayBGM(string trackName)
        {
            if (bgmSource == null) return;
            AudioClip clip = clips.ContainsKey(trackName) ? clips[trackName] : clips["JungleExpedition"];
            if (bgmSource.clip == clip && bgmSource.isPlaying) return;
            bgmSource.clip = clip;
            bgmSource.loop = true;
            bgmSource.volume = musicVolume;
            bgmSource.Play();
        }

        public void StopBGM() { if (bgmSource != null) bgmSource.Stop(); }
        public void PauseBGM() { if (bgmSource != null && bgmSource.isPlaying) bgmSource.Pause(); }
        public void ResumeBGM() { if (bgmSource != null) bgmSource.UnPause(); }

        public void PlaySFX(string sfxName)
        {
            if (!clips.TryGetValue(sfxName, out AudioClip clip) || clip == null) return;
            if (sfxVolume <= 0.001f) return;

            // A dedicated 2D source keeps the impact audible even while the
            // gameplay camera/player is being disabled during the death flow.
            if (sfxName == "PlayerCrash" && collisionSource != null)
            {
                collisionSource.Stop();
                collisionSource.volume = sfxVolume;
                collisionSource.PlayOneShot(clip, 1f);
                return;
            }

            if (sfxSource != null)
            {
                sfxSource.volume = 1f;
                sfxSource.PlayOneShot(clip, sfxVolume);
            }
        }
    }
}

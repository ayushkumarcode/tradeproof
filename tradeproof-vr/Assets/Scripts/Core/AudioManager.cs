using UnityEngine;

namespace TradeProof.Core
{
    public class AudioManager : MonoBehaviour
    {
        private static AudioManager _instance;
        public static AudioManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("AudioManager");
                    _instance = go.AddComponent<AudioManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource uiSource;

        [Header("Sound Effects")]
        [SerializeField] private AudioClip correctSound;
        [SerializeField] private AudioClip incorrectSound;
        [SerializeField] private AudioClip snapSound;
        [SerializeField] private AudioClip grabSound;
        [SerializeField] private AudioClip releaseSound;
        [SerializeField] private AudioClip hintSound;
        [SerializeField] private AudioClip timerTickSound;
        [SerializeField] private AudioClip timerEndSound;
        [SerializeField] private AudioClip badgeEarnedSound;
        [SerializeField] private AudioClip buttonClickSound;
        [SerializeField] private AudioClip taskCompleteSound;
        [SerializeField] private AudioClip taskFailSound;

        [Header("Settings")]
        [SerializeField] private float sfxVolume = 1.0f;
        [SerializeField] private float musicVolume = 0.5f;
        [SerializeField] private float uiVolume = 0.8f;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);

            SetupAudioSources();
            LoadVolumeSettings();
        }

        private void SetupAudioSources()
        {
            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
                sfxSource.spatialBlend = 0f; // 2D for UI sounds
            }

            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.playOnAwake = false;
                musicSource.loop = true;
                musicSource.spatialBlend = 0f;
            }

            if (uiSource == null)
            {
                uiSource = gameObject.AddComponent<AudioSource>();
                uiSource.playOnAwake = false;
                uiSource.spatialBlend = 0f;
            }
        }

        private void LoadVolumeSettings()
        {
            sfxVolume = PlayerPrefs.GetFloat("TradeProof_SFXVolume", 1.0f);
            musicVolume = PlayerPrefs.GetFloat("TradeProof_MusicVolume", 0.5f);
            uiVolume = PlayerPrefs.GetFloat("TradeProof_UIVolume", 0.8f);

            sfxSource.volume = sfxVolume;
            musicSource.volume = musicVolume;
            uiSource.volume = uiVolume;
        }

        public void SaveVolumeSettings()
        {
            PlayerPrefs.SetFloat("TradeProof_SFXVolume", sfxVolume);
            PlayerPrefs.SetFloat("TradeProof_MusicVolume", musicVolume);
            PlayerPrefs.SetFloat("TradeProof_UIVolume", uiVolume);
            PlayerPrefs.Save();
        }

        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            sfxSource.volume = sfxVolume;
        }

        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            musicSource.volume = musicVolume;
        }

        public void SetUIVolume(float volume)
        {
            uiVolume = Mathf.Clamp01(volume);
            uiSource.volume = uiVolume;
        }

        // --- SFX Playback ---

        private void PlaySFX(AudioClip clip)
        {
            if (clip != null && sfxSource != null)
            {
                sfxSource.PlayOneShot(clip, sfxVolume);
            }
        }

        private void PlayUI(AudioClip clip)
        {
            if (clip != null && uiSource != null)
            {
                uiSource.PlayOneShot(clip, uiVolume);
            }
        }

        public void PlayCorrectSound()
        {
            if (correctSound != null)
            {
                PlaySFX(correctSound);
            }
            else
            {
                PlayGeneratedTone(880f, 0.15f, sfxSource); // A5 note — pleasant
            }
        }

        public void PlayIncorrectSound()
        {
            if (incorrectSound != null)
            {
                PlaySFX(incorrectSound);
            }
            else
            {
                PlayGeneratedTone(220f, 0.3f, sfxSource); // A3 note — low warning
            }
        }

        public void PlaySnapSound()
        {
            if (snapSound != null)
            {
                PlaySFX(snapSound);
            }
            else
            {
                PlayGeneratedTone(1200f, 0.05f, sfxSource); // Quick high tick
            }
        }

        public void PlayGrabSound()
        {
            if (grabSound != null)
                PlaySFX(grabSound);
        }

        public void PlayReleaseSound()
        {
            if (releaseSound != null)
                PlaySFX(releaseSound);
        }

        public void PlayHintSound()
        {
            if (hintSound != null)
            {
                PlayUI(hintSound);
            }
            else
            {
                PlayGeneratedTone(660f, 0.2f, uiSource);
            }
        }

        public void PlayTimerTickSound()
        {
            if (timerTickSound != null)
                PlaySFX(timerTickSound);
        }

        public void PlayTimerEndSound()
        {
            if (timerEndSound != null)
            {
                PlaySFX(timerEndSound);
            }
            else
            {
                PlayGeneratedTone(440f, 0.5f, sfxSource);
            }
        }

        public void PlayBadgeEarnedSound()
        {
            if (badgeEarnedSound != null)
            {
                PlaySFX(badgeEarnedSound);
            }
            else
            {
                // Play ascending notes
                StartCoroutine(PlayBadgeJingle());
            }
        }

        public void PlayButtonClick()
        {
            if (buttonClickSound != null)
            {
                PlayUI(buttonClickSound);
            }
            else
            {
                PlayGeneratedTone(1000f, 0.05f, uiSource);
            }
        }

        public void PlayTaskComplete()
        {
            if (taskCompleteSound != null)
            {
                PlaySFX(taskCompleteSound);
            }
            else
            {
                StartCoroutine(PlayCompleteJingle());
            }
        }

        public void PlayTaskFail()
        {
            if (taskFailSound != null)
            {
                PlaySFX(taskFailSound);
            }
            else
            {
                PlayGeneratedTone(200f, 0.5f, sfxSource);
            }
        }

        // --- Spatial Audio ---

        public void PlaySpatialSound(AudioClip clip, Vector3 position, float maxDistance = 5f)
        {
            if (clip == null) return;

            GameObject tempAudio = new GameObject("TempSpatialAudio");
            tempAudio.transform.position = position;

            AudioSource source = tempAudio.AddComponent<AudioSource>();
            source.clip = clip;
            source.spatialBlend = 1f; // Fully 3D
            source.maxDistance = maxDistance;
            source.rolloffMode = AudioRolloffMode.Linear;
            source.volume = sfxVolume;
            source.Play();

            Destroy(tempAudio, clip.length + 0.1f);
        }

        // --- Procedural Audio (fallback when no clips assigned) ---

        private void PlayGeneratedTone(float frequency, float duration, AudioSource source)
        {
            int sampleRate = 44100;
            int sampleCount = Mathf.CeilToInt(sampleRate * duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float envelope = 1f - (t / duration); // Linear fade out
                samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * envelope * 0.3f;
            }

            AudioClip generatedClip = AudioClip.Create("GeneratedTone", sampleCount, 1, sampleRate, false);
            generatedClip.SetData(samples, 0);

            if (source != null)
            {
                source.PlayOneShot(generatedClip);
            }
        }

        private System.Collections.IEnumerator PlayBadgeJingle()
        {
            float[] notes = { 523f, 659f, 784f, 1047f }; // C5, E5, G5, C6
            foreach (float note in notes)
            {
                PlayGeneratedTone(note, 0.15f, sfxSource);
                yield return new WaitForSeconds(0.12f);
            }
        }

        private System.Collections.IEnumerator PlayCompleteJingle()
        {
            float[] notes = { 440f, 554f, 659f }; // A4, C#5, E5 (A major chord arpeggiated)
            foreach (float note in notes)
            {
                PlayGeneratedTone(note, 0.2f, sfxSource);
                yield return new WaitForSeconds(0.15f);
            }
        }
    }
}

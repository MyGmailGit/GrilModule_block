using Unity.Mathematics;
using UnityEngine;
using DG.Tweening;


namespace Watermelon
{
    [StaticUnload]
    [RequireComponent(typeof(AudioSource))]
    public class MusicSource : MonoBehaviour
    {
        private const float DEFAULT_FADE_DURATION = 0.3f;

        private static MusicSource defaultMusicSource;

        private static MusicSource activeMusicSource;
        public static MusicSource ActiveMusicSource => activeMusicSource;

        [SerializeField] bool activateAutomatically = false;

        private AudioSource[] audioSource;
        public AudioSource[] AudioSource => audioSource;

        private Tween fadeTweenCase;

        private float volumeMultiplier = 1.0f;

        private int nowPlayIdx = -1;

        private void Start()
        {
            if (activateAutomatically)
            {
                Init();
                // Activate();
            }
        }

        public void Init()
        {
            activeMusicSource = this;

            audioSource = GetComponents<AudioSource>();
            foreach (var it in audioSource)
            {
                it.loop = true;
                it.playOnAwake = false;
                it.volume = AudioController.GetVolume(AudioType.Music) * volumeMultiplier;
                volumeMultiplier = it.volume;
                it.Stop();
            }

            AudioController.VolumeChanged += OnVolumeChanged;
        }

        public void Unload()
        {
            AudioController.VolumeChanged -= OnVolumeChanged;
        }

        private void OnDestroy()
        {
            AudioController.VolumeChanged -= OnVolumeChanged;
        }

        public void SetAsDefault()
        {
            defaultMusicSource = this;
        }

        public void StopAll()
        {
            foreach (var it in audioSource)
            {
                it.volume = 0.0f;
                it.Stop();
            }
        }

        public void Activate(int i = 0)
        {
            // if (activeMusicSource == this) return;

            // if (activeMusicSource != null)
            // {
            //     StopAll();
            //     // activeMusicSource.audioSource.volume = 0.0f;
            //     // activeMusicSource.audioSource.Stop();
            // }

            if (i == nowPlayIdx) return;

            Fade(0.0f, DEFAULT_FADE_DURATION, 0, () =>
            {
                if (nowPlayIdx >= 0 && nowPlayIdx < audioSource.Length)
                {
                    audioSource[nowPlayIdx].Stop();
                }

                i = math.clamp(i, 0, audioSource.Length - 1);
                nowPlayIdx = i;
                audioSource[i].Play();
                Fade(1.0f, DEFAULT_FADE_DURATION);
            });

        }

        public void SetVolume(float volume)
        {
            if (nowPlayIdx >= 0 && nowPlayIdx < audioSource.Length)
            {
                audioSource[nowPlayIdx].volume = volume * AudioController.GetVolume(AudioType.Music) * volumeMultiplier;
            }
        }

        public void Fade(float value, float duration, float delay = 0, SimpleCallback onComplete = null)
        {
            fadeTweenCase.Kill();
            if (nowPlayIdx < 0 || nowPlayIdx >= audioSource.Length) { onComplete.Invoke(); return; }

            fadeTweenCase = DOVirtual.Float(audioSource[nowPlayIdx].volume, value, duration, (value) =>
            {
                audioSource[nowPlayIdx].volume = value * AudioController.GetVolume(AudioType.Music) * volumeMultiplier;
            }).SetDelay(delay).OnComplete(() => { onComplete?.Invoke(); });
        }

        private void OnVolumeChanged(AudioType audioType, float volume)
        {
            if (audioType != AudioType.Music) return;

            if (nowPlayIdx >= 0 && nowPlayIdx < audioSource.Length)
            {
                audioSource[nowPlayIdx].volume = volume * volumeMultiplier;
            }
        }

        public bool IsActive()
        {
            return activeMusicSource == this;
        }

        // public static void ActivateDefault()
        // {
        //     if (activeMusicSource == defaultMusicSource) return;

        //     defaultMusicSource.Activate();
        // }

        private static void UnloadStatic()
        {
            defaultMusicSource = null;
            activeMusicSource = null;
        }
    }
}
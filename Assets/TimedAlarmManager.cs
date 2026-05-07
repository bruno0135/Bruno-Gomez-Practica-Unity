using UnityEngine;

namespace BrunoGomez
{
    /// <summary>
    /// Plays a specific alarm sound after a set amount of time has passed.
    /// </summary>
    public class TimedAlarmManager : MonoBehaviour
    {
        [Header("Alarm Settings")]
        [Tooltip("The alarm clip to play (edr-synth-alarm-01-169969)")]
        public AudioClip alarmClip;
        
        [Tooltip("Delay in seconds before the alarm plays (e.g. 120 for 2 minutes)")]
        public float delayInSeconds = 120f;
        
        [Tooltip("Volume of the alarm (0-1)")]
        [Range(0f, 1f)]
        public float volume = 0.7f;
        
        [Tooltip("Should the alarm loop?")]
        public bool loop = false;

        [Header("3D Sound Settings")]
        [Tooltip("3D spatial blend (0=2D, 1=3D)")]
        [Range(0f, 1f)]
        public float spatialBlend = 1.0f;
        
        [Tooltip("Minimum distance for 3D sound")]
        public float minDistance = 1f;
        
        [Tooltip("Maximum distance for 3D sound")]
        public float maxDistance = 20f;

        [Header("Optional")]
        public UnityEngine.Audio.AudioMixerGroup mixerGroup;

        private AudioSource audioSource;
        private float timer;
        private bool hasPlayed = false;

        void Start()
        {
            // Reset timer
            timer = 0f;
            hasPlayed = false;

            // Prepare AudioSource
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.clip = alarmClip;
            audioSource.volume = volume;
            audioSource.loop = loop;
            audioSource.spatialBlend = spatialBlend;
            audioSource.minDistance = minDistance;
            audioSource.maxDistance = maxDistance;
            audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
            
            if (mixerGroup != null)
                audioSource.outputAudioMixerGroup = mixerGroup;
        }

        void Update()
        {
            if (hasPlayed && !loop) return;

            if (!hasPlayed)
            {
                timer += Time.deltaTime;

                if (timer >= delayInSeconds)
                {
                    PlayAlarm();
                }
            }
        }

        private void PlayAlarm()
        {
            if (alarmClip == null)
            {
                Debug.LogWarning("[TimedAlarmManager] No alarm clip assigned!");
                hasPlayed = true; // Avoid spamming warning
                return;
            }

            audioSource.Play();
            hasPlayed = true;
            Debug.Log($"[TimedAlarmManager] Alarm '{alarmClip.name}' started playing after {delayInSeconds} seconds.");
        }
    }
}

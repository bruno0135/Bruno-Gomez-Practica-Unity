using UnityEngine;

namespace BrunoGomez
{
    /// <summary>
    /// Universal static loop emitter for 3D or 2D sounds.
    /// Automatically pauses playback when the player is far away to save CPU.
    ///
    /// SETUP in Unity:
    /// 1. Add to any static object (machines, vents, pipes, etc.)
    /// 2. Assign an audio clip
    /// 3. Set spatialBlend (0 = 2D global, 1 = 3D positional)
    /// 4. Configure distances and optional mixer group
    /// </summary>
    public class StaticLoopEmitter : MonoBehaviour
    {
        [Header("Audio")]
        [Tooltip("Audio clip to loop")]
        public AudioClip loopClip;

        [Range(0f, 1f)]
        public float volume = 0.5f;

        [Tooltip("0 = 2D (heard everywhere), 1 = 3D (positional)")]
        [Range(0f, 1f)]
        public float spatialBlend = 1.0f;

        [Header("3D Settings")]
        public float minDistance = 1f;
        public float maxDistance = 15f;
        public AudioRolloffMode rolloff = AudioRolloffMode.Logarithmic;

        [Header("Optimization")]
        [Tooltip("Multiplier of maxDistance beyond which playback is paused")]
        public float pauseDistanceMultiplier = 2.5f;
        [Tooltip("How often to check distance (seconds)")]
        public float checkInterval = 0.5f;

        [Header("Optional")]
        public UnityEngine.Audio.AudioMixerGroup mixerGroup;

        private AudioSource audioSource;
        private Transform listenerTransform;
        private float checkTimer;
        private bool isPaused;

        void Start()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();

            audioSource.clip = loopClip;
            audioSource.loop = true;
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = spatialBlend;
            audioSource.volume = volume;
            audioSource.minDistance = minDistance;
            audioSource.maxDistance = maxDistance;
            audioSource.rolloffMode = rolloff;
            audioSource.dopplerLevel = 0f;

            if (mixerGroup != null)
                audioSource.outputAudioMixerGroup = mixerGroup;

            if (Camera.main != null)
                listenerTransform = Camera.main.transform;

            if (loopClip != null)
            {
                audioSource.Play();
                Debug.Log($"[StaticLoop] {gameObject.name}: Playing '{loopClip.name}' (spatial: {spatialBlend})");
            }
            else
            {
                Debug.LogWarning($"[StaticLoop] {gameObject.name}: No clip assigned!");
            }
        }

        void Update()
        {
            // Only check distance periodically for performance
            checkTimer -= Time.deltaTime;
            if (checkTimer > 0f) return;
            checkTimer = checkInterval;

            // Skip optimization for 2D sounds (always audible)
            if (spatialBlend < 0.1f) return;

            if (listenerTransform == null)
            {
                if (Camera.main != null) listenerTransform = Camera.main.transform;
                else return;
            }

            float dist = Vector3.Distance(transform.position, listenerTransform.position);
            float pauseDist = maxDistance * pauseDistanceMultiplier;

            if (dist > pauseDist && !isPaused)
            {
                audioSource.Pause();
                isPaused = true;
            }
            else if (dist <= pauseDist && isPaused)
            {
                audioSource.UnPause();
                isPaused = false;
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.2f, 0.8f, 0.2f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, minDistance);
            Gizmos.color = new Color(0.8f, 0.2f, 0.2f, 0.2f);
            Gizmos.DrawWireSphere(transform.position, maxDistance);
            Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.1f);
            Gizmos.DrawWireSphere(transform.position, maxDistance * pauseDistanceMultiplier);
        }
#endif
    }
}

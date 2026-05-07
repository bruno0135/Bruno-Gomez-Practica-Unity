using UnityEngine;

namespace BrunoGomez
{
    /// <summary>
    /// Synchronizes sound effects with animation states.
    /// Define sound events tied to specific animation states and normalized times.
    ///
    /// SETUP in Unity:
    /// 1. Add this component to any GameObject with an Animator
    /// 2. Define sound events in the array (clip, state name, trigger time)
    /// 3. Optionally assign an AudioMixerGroup
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class AnimationSoundSync : MonoBehaviour
    {
        [System.Serializable]
        public class AnimSoundEvent
        {
            [Tooltip("Name of the animation state to watch")]
            public string stateName;
            [Tooltip("Normalized time (0-1) at which to play the sound")]
            [Range(0f, 1f)]
            public float triggerTime = 0.5f;
            [Tooltip("Audio clip to play")]
            public AudioClip clip;
            [Tooltip("Volume of this sound")]
            [Range(0f, 1f)]
            public float volume = 0.7f;
            [Tooltip("3D spatial blend (0=2D, 1=3D)")]
            [Range(0f, 1f)]
            public float spatialBlend = 1.0f;

            // Internal: prevent re-triggering in the same loop
            [HideInInspector] public bool triggered;
            [HideInInspector] public int stateHash;
        }

        [Header("Sound Events")]
        public AnimSoundEvent[] soundEvents;

        [Header("Settings")]
        [Tooltip("Animator layer to monitor")]
        public int animatorLayer = 0;

        [Tooltip("Optional Audio Mixer Group")]
        public UnityEngine.Audio.AudioMixerGroup mixerGroup;

        private Animator animator;
        private AudioSource audioSource;

        void Start()
        {
            animator = GetComponent<Animator>();

            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            audioSource.playOnAwake = false;

            if (mixerGroup != null)
                audioSource.outputAudioMixerGroup = mixerGroup;

            // Pre-hash state names for performance
            if (soundEvents != null)
            {
                for (int i = 0; i < soundEvents.Length; i++)
                {
                    soundEvents[i].stateHash = Animator.StringToHash(soundEvents[i].stateName);
                }
            }
        }

        void Update()
        {
            if (animator == null || soundEvents == null) return;

            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(animatorLayer);

            for (int i = 0; i < soundEvents.Length; i++)
            {
                AnimSoundEvent evt = soundEvents[i];
                if (evt.clip == null) continue;

                bool isInState = stateInfo.shortNameHash == evt.stateHash;

                if (isInState)
                {
                    float normalizedTime = stateInfo.normalizedTime % 1f;

                    // Trigger when we cross the trigger time threshold
                    if (normalizedTime >= evt.triggerTime && !evt.triggered)
                    {
                        audioSource.spatialBlend = evt.spatialBlend;
                        audioSource.PlayOneShot(evt.clip, evt.volume);
                        evt.triggered = true;
                    }

                    // Reset trigger when animation loops back past the trigger point
                    if (normalizedTime < evt.triggerTime)
                    {
                        evt.triggered = false;
                    }
                }
                else
                {
                    evt.triggered = false;
                }
            }
        }
    }
}

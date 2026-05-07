using UnityEngine;
using UnityEngine.Audio;

namespace BrunoGomez
{
    /// <summary>
    /// Vertical music system with synchronized layers that can be blended in/out.
    /// All layers play simultaneously but at different volumes for seamless transitions.
    ///
    /// SETUP in Unity:
    /// 1. Create an empty GameObject "MusicManager"
    /// 2. Add this component
    /// 3. Assign AudioClips for each layer (base, tension, action)
    /// 4. Optionally assign AudioMixerGroups for each layer
    /// 5. Use SetLayerTarget() or triggers to control layers
    /// </summary>
    public class VerticalMusicManager : MonoBehaviour
    {
        [System.Serializable]
        public class MusicLayer
        {
            public string layerName = "Layer";
            public AudioClip clip;
            [Range(0f, 1f)]
            public float targetVolume = 0f;
            [Range(0f, 1f)]
            public float maxVolume = 1f;
            public AudioMixerGroup mixerGroup;

            [HideInInspector] public AudioSource source;
            [HideInInspector] public float currentVolume;
        }

        [Header("Music Layers")]
        [Tooltip("Define each music layer (base, tension, action, etc.)")]
        public MusicLayer[] layers;

        [Header("Transition Settings")]
        [Tooltip("Speed of volume crossfade between layers")]
        public float fadeSpeed = 2f;

        [Header("Sync Settings")]
        [Tooltip("Schedule all layers to start at the same DSP time")]
        public bool syncOnStart = true;

        private bool initialized = false;

        void Start()
        {
            if (layers == null || layers.Length == 0) return;

            // Schedule a start time slightly in the future for perfect sync
            double startTime = AudioSettings.dspTime + 0.5;

            for (int i = 0; i < layers.Length; i++)
            {
                MusicLayer layer = layers[i];
                if (layer.clip == null) continue;

                // Create AudioSource for each layer
                layer.source = gameObject.AddComponent<AudioSource>();
                layer.source.clip = layer.clip;
                layer.source.loop = true;
                layer.source.playOnAwake = false;
                layer.source.spatialBlend = 0f; // 2D music
                layer.source.volume = layer.targetVolume;
                layer.source.priority = 0; // High priority
                layer.currentVolume = layer.targetVolume;

                if (layer.mixerGroup != null)
                    layer.source.outputAudioMixerGroup = layer.mixerGroup;

                // Schedule synchronized start
                if (syncOnStart)
                    layer.source.PlayScheduled(startTime);
                else
                    layer.source.Play();
            }

            initialized = true;
            Debug.Log($"[VerticalMusic] Started {layers.Length} layers" +
                      (syncOnStart ? $" (synced at DSP {startTime:F3})" : ""));
        }

        void Update()
        {
            if (!initialized) return;

            // Smoothly fade each layer towards its target volume
            for (int i = 0; i < layers.Length; i++)
            {
                MusicLayer layer = layers[i];
                if (layer.source == null) continue;

                layer.currentVolume = Mathf.MoveTowards(
                    layer.currentVolume,
                    layer.targetVolume,
                    fadeSpeed * Time.deltaTime
                );
                layer.source.volume = layer.currentVolume;
            }
        }

        /// <summary>
        /// Set the target volume for a specific layer by index.
        /// </summary>
        public void SetLayerTarget(int layerIndex, float target)
        {
            if (layers != null && layerIndex >= 0 && layerIndex < layers.Length)
            {
                layers[layerIndex].targetVolume = Mathf.Clamp(target, 0f, layers[layerIndex].maxVolume);
            }
        }

        /// <summary>
        /// Set the target volume for a specific layer by name.
        /// </summary>
        public void SetLayerTarget(string layerName, float target)
        {
            if (layers == null) return;
            for (int i = 0; i < layers.Length; i++)
            {
                if (layers[i].layerName == layerName)
                {
                    layers[i].targetVolume = Mathf.Clamp(target, 0f, layers[i].maxVolume);
                    return;
                }
            }
        }

        /// <summary>
        /// Activate only one layer (solo mode). All others fade to 0.
        /// </summary>
        public void SoloLayer(int layerIndex)
        {
            if (layers == null) return;
            for (int i = 0; i < layers.Length; i++)
            {
                layers[i].targetVolume = (i == layerIndex) ? layers[i].maxVolume : 0f;
            }
        }

        /// <summary>
        /// Fade all layers to zero (silence).
        /// </summary>
        public void FadeOutAll()
        {
            if (layers == null) return;
            for (int i = 0; i < layers.Length; i++)
                layers[i].targetVolume = 0f;
        }

        /// <summary>
        /// Fade all layers to their max volume.
        /// </summary>
        public void FadeInAll()
        {
            if (layers == null) return;
            for (int i = 0; i < layers.Length; i++)
                layers[i].targetVolume = layers[i].maxVolume;
        }
    }
}

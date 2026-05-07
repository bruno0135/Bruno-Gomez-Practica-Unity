using UnityEngine;

namespace BrunoGomez
{
    /// <summary>
    /// Simulates Computer_loop.wav playing from a small radio speaker.
    /// Uses band-pass filtering (High-Pass + Low-Pass) and subtle distortion
    /// to emulate a cheap, tiny speaker. Includes integrated raycast occlusion
    /// that further muffles and attenuates the sound when walls block line of sight.
    ///
    /// SETUP in Unity:
    /// 1. Add this component to each Wall_Console prefab instance
    /// 2. Drag Computer_loop.wav into the "Computer Loop Clip" field
    /// 3. Optionally assign an AudioMixerGroup (e.g. SFX > Machines)
    /// 4. Adjust parameters in the Inspector as desired
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class ScreenSound : MonoBehaviour
    {
        [Header("=== Audio Clip ===")]
        [Tooltip("The Computer_loop audio clip to play")]
        public AudioClip computerLoopClip;

        [Header("=== Radio Speaker Settings ===")]
        [Tooltip("Volume of the radio speaker (0-1)")]
        [Range(0f, 1f)]
        public float volume = 0.15f;

        [Tooltip("High-Pass cutoff to remove bass (simulates tiny speaker)")]
        public float highPassCutoff = 800f;

        [Tooltip("Low-Pass cutoff to remove high treble (simulates speaker limit)")]
        public float lowPassCutoff = 4000f;

        [Tooltip("Subtle distortion amount (simulates speaker saturation)")]
        [Range(0f, 0.8f)]
        public float distortionAmount = 0.15f;

        [Header("=== 3D Spatialization ===")]
        [Tooltip("Distance at which the sound starts to fade out")]
        public float minDistance = 0.5f;

        [Tooltip("Maximum distance at which the sound can be heard")]
        public float maxDistance = 8.0f;

        [Header("=== Raycast Occlusion ===")]
        [Tooltip("Enable raycast occlusion (sound muffles behind walls)")]
        public bool enableOcclusion = true;

        [Tooltip("Layers that block the sound")]
        public LayerMask obstacleLayers = -1;

        [Tooltip("Low-Pass frequency when fully occluded")]
        public float occludedCutoff = 350f;

        [Tooltip("Volume multiplier when occluded (0-1)")]
        [Range(0f, 1f)]
        public float occludedVolumeMultiplier = 0.3f;

        [Tooltip("Volume reduction per additional wall hit")]
        [Range(0f, 0.5f)]
        public float volumeReductionPerWall = 0.15f;

        [Tooltip("How fast the occlusion filter transitions")]
        public float occlusionSmoothSpeed = 8f;

        [Header("=== Optional ===")]
        [Tooltip("Audio Mixer Group to route this sound to (e.g. SFX/Machines)")]
        public UnityEngine.Audio.AudioMixerGroup mixerGroup;

        // Components
        private AudioSource audioSource;
        private AudioHighPassFilter highPassFilter;
        private AudioLowPassFilter lowPassFilter;
        private AudioDistortionFilter distortionFilter;

        // Occlusion state
        private Transform listenerTransform;
        private float targetLowPassCutoff;
        private float targetVolume;
        private float baseVolume;

        void Start()
        {
            // --- AudioSource Setup ---
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }

            audioSource.clip = computerLoopClip;
            audioSource.loop = true;
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1.0f;       // Full 3D
            audioSource.volume = volume;
            audioSource.minDistance = minDistance;
            audioSource.maxDistance = maxDistance;
            audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
            audioSource.dopplerLevel = 0f;          // No Doppler for static source
            audioSource.spread = 60f;               // Narrow spread like a small speaker

            if (mixerGroup != null)
            {
                audioSource.outputAudioMixerGroup = mixerGroup;
            }

            // --- Radio Speaker Filters ---
            // High-Pass: Removes bass to simulate a tiny speaker that can't reproduce low frequencies
            highPassFilter = GetComponent<AudioHighPassFilter>();
            if (highPassFilter == null)
            {
                highPassFilter = gameObject.AddComponent<AudioHighPassFilter>();
            }
            highPassFilter.cutoffFrequency = highPassCutoff;
            highPassFilter.highpassResonanceQ = 1.0f;

            // Low-Pass: Removes high treble to simulate speaker frequency limit
            lowPassFilter = GetComponent<AudioLowPassFilter>();
            if (lowPassFilter == null)
            {
                lowPassFilter = gameObject.AddComponent<AudioLowPassFilter>();
            }
            lowPassFilter.cutoffFrequency = lowPassCutoff;
            lowPassFilter.lowpassResonanceQ = 1.5f; // Slight resonance for "speaker character"

            // Distortion: Subtle saturation like a cheap speaker at high volume
            distortionFilter = GetComponent<AudioDistortionFilter>();
            if (distortionFilter == null)
            {
                distortionFilter = gameObject.AddComponent<AudioDistortionFilter>();
            }
            distortionFilter.distortionLevel = distortionAmount;

            // --- Occlusion Setup ---
            baseVolume = volume;
            targetLowPassCutoff = lowPassCutoff;
            targetVolume = volume;

            if (Camera.main != null)
            {
                listenerTransform = Camera.main.transform;
            }

            // --- Start Playing ---
            if (computerLoopClip != null)
            {
                audioSource.Play();
                Debug.Log($"[ScreenSound] {gameObject.name}: Radio speaker started (Band-pass: {highPassCutoff}Hz-{lowPassCutoff}Hz, Distortion: {distortionAmount})");
            }
            else
            {
                Debug.LogWarning($"[ScreenSound] {gameObject.name}: No Computer_loop clip assigned!");
            }
        }

        void Update()
        {
            if (!enableOcclusion) return;
            if (audioSource == null || !audioSource.isPlaying) return;

            // Find listener if not cached
            if (listenerTransform == null)
            {
                if (Camera.main != null) listenerTransform = Camera.main.transform;
                else return;
            }

            // --- Distance-based optimization: skip raycast if player is far away ---
            float distToListener = Vector3.Distance(transform.position, listenerTransform.position);
            if (distToListener > maxDistance * 2f)
            {
                // Too far to matter, ensure we're at occluded state to save processing
                targetLowPassCutoff = occludedCutoff;
                targetVolume = 0f;
            }
            else
            {
                // --- Raycast Occlusion with multi-wall support ---
                Vector3 direction = listenerTransform.position - transform.position;
                float distance = direction.magnitude;

                RaycastHit[] hits = Physics.RaycastAll(transform.position, direction.normalized, distance, obstacleLayers, QueryTriggerInteraction.Ignore);

                // Count how many obstacles are between us and the listener
                int wallCount = 0;
                foreach (RaycastHit hit in hits)
                {
                    // Ignore hits on the listener itself or children
                    if (hit.transform == listenerTransform || hit.transform.IsChildOf(listenerTransform))
                        continue;
                    // Ignore hits on ourselves
                    if (hit.transform == transform || hit.transform.IsChildOf(transform))
                        continue;
                    wallCount++;
                }

                if (wallCount > 0)
                {
                    // Occluded: muffle and reduce volume
                    targetLowPassCutoff = occludedCutoff;
                    float reduction = Mathf.Clamp01(1f - (wallCount * volumeReductionPerWall));
                    targetVolume = baseVolume * Mathf.Min(occludedVolumeMultiplier, reduction);
                }
                else
                {
                    // Clear line of sight: restore radio speaker sound
                    targetLowPassCutoff = lowPassCutoff;
                    targetVolume = baseVolume;
                }
            }

            // --- Smooth transitions ---
            float lerpSpeed = Time.deltaTime * occlusionSmoothSpeed;
            lowPassFilter.cutoffFrequency = Mathf.Lerp(lowPassFilter.cutoffFrequency, targetLowPassCutoff, lerpSpeed);
            audioSource.volume = Mathf.Lerp(audioSource.volume, targetVolume, lerpSpeed);
        }

        /// <summary>
        /// Allows external scripts to change the volume at runtime.
        /// </summary>
        public void SetVolume(float newVolume)
        {
            baseVolume = Mathf.Clamp01(newVolume);
            targetVolume = baseVolume;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            // Draw min/max distance spheres
            Gizmos.color = new Color(0f, 1f, 0.5f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, minDistance);

            Gizmos.color = new Color(1f, 0.5f, 0f, 0.2f);
            Gizmos.DrawWireSphere(transform.position, maxDistance);

            // Draw occlusion ray if in play mode
            if (Application.isPlaying && listenerTransform != null)
            {
                bool isOccluded = targetLowPassCutoff < lowPassCutoff;
                Gizmos.color = isOccluded ? Color.red : Color.green;
                Gizmos.DrawLine(transform.position, listenerTransform.position);

                // Label
                UnityEditor.Handles.color = isOccluded ? Color.red : Color.green;
                UnityEditor.Handles.Label(transform.position + Vector3.up * 0.5f,
                    isOccluded ? "OCCLUDED" : "CLEAR");
            }
        }
#endif
    }
}

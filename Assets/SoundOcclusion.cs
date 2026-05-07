using UnityEngine;

namespace BrunoGomez
{
    /// <summary>
    /// Raycast-based sound occlusion system. Attach to any AudioSource to automatically
    /// muffle and attenuate the sound when walls/obstacles block line of sight to the listener.
    /// Supports multi-wall occlusion using RaycastAll.
    ///
    /// SETUP in Unity:
    /// 1. Add this component to any GameObject that already has an AudioSource
    /// 2. Configure obstacle layers and occlusion parameters
    /// 3. The script automatically adds an AudioLowPassFilter if one doesn't exist
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class SoundOcclusion : MonoBehaviour
    {
        [Header("Occlusion Settings")]
        [Tooltip("The layers that will block the sound (e.g. Default, World)")]
        public LayerMask obstacleLayers = -1;

        [Tooltip("Frequency when the sound is fully occluded (lower = more muffled)")]
        public float minCutoffFreq = 500f;

        [Tooltip("Frequency when the sound is clear")]
        public float maxCutoffFreq = 22000f;

        [Tooltip("How fast the filter transitions")]
        public float smoothSpeed = 10f;

        [Header("Volume Attenuation")]
        [Tooltip("Enable volume reduction when occluded")]
        public bool attenuateVolume = true;

        [Tooltip("Volume multiplier per wall hit (stacks multiplicatively)")]
        [Range(0.1f, 1f)]
        public float volumeMultiplierPerWall = 0.5f;

        [Tooltip("Minimum volume when fully occluded")]
        [Range(0f, 1f)]
        public float minimumVolume = 0.05f;

        [Header("Multi-Wall Support")]
        [Tooltip("Maximum number of walls to consider for occlusion")]
        [Range(1, 5)]
        public int maxWallsToConsider = 3;

        // Internal state
        private AudioSource audioSource;
        private AudioLowPassFilter lowPassFilter;
        private Transform listenerTransform;
        private float targetCutoff;
        private float targetVolume;
        private float originalVolume;
        private bool volumeInitialized;

        void Start()
        {
            audioSource = GetComponent<AudioSource>();

            // Ensure we have a Low Pass Filter
            lowPassFilter = GetComponent<AudioLowPassFilter>();
            if (lowPassFilter == null)
            {
                lowPassFilter = gameObject.AddComponent<AudioLowPassFilter>();
            }

            // Find the listener (usually the main camera)
            if (Camera.main != null)
            {
                listenerTransform = Camera.main.transform;
            }

            targetCutoff = maxCutoffFreq;
            lowPassFilter.cutoffFrequency = maxCutoffFreq;
        }

        void Update()
        {
            if (audioSource == null) return;

            // Cache original volume on first frame the source is playing
            if (!volumeInitialized && audioSource.isPlaying)
            {
                originalVolume = audioSource.volume;
                targetVolume = originalVolume;
                volumeInitialized = true;
            }

            if (listenerTransform == null)
            {
                if (Camera.main != null) listenerTransform = Camera.main.transform;
                else return;
            }

            // Raycast direction: From this object to the listener
            Vector3 direction = listenerTransform.position - transform.position;
            float distance = direction.magnitude;

            // Use RaycastAll for multi-wall support
            RaycastHit[] hits = Physics.RaycastAll(transform.position, direction.normalized, distance, obstacleLayers, QueryTriggerInteraction.Ignore);

            // Count valid obstacles (ignore listener and self)
            int wallCount = 0;
            foreach (RaycastHit hit in hits)
            {
                if (hit.transform == listenerTransform || hit.transform.IsChildOf(listenerTransform))
                    continue;
                if (hit.transform == transform || hit.transform.IsChildOf(transform))
                    continue;

                wallCount++;
                if (wallCount >= maxWallsToConsider) break;
            }

            if (wallCount > 0)
            {
                // Interpolate cutoff based on wall count
                float occlusionFactor = Mathf.Clamp01((float)wallCount / maxWallsToConsider);
                targetCutoff = Mathf.Lerp(maxCutoffFreq, minCutoffFreq, occlusionFactor);

                // Volume attenuation
                if (attenuateVolume && volumeInitialized)
                {
                    float volMult = Mathf.Pow(volumeMultiplierPerWall, wallCount);
                    targetVolume = Mathf.Max(originalVolume * volMult, minimumVolume);
                }
            }
            else
            {
                targetCutoff = maxCutoffFreq;
                if (attenuateVolume && volumeInitialized)
                {
                    targetVolume = originalVolume;
                }
            }

            // Smooth transitions
            float lerpSpeed = Time.deltaTime * smoothSpeed;
            lowPassFilter.cutoffFrequency = Mathf.Lerp(lowPassFilter.cutoffFrequency, targetCutoff, lerpSpeed);

            if (attenuateVolume && volumeInitialized)
            {
                audioSource.volume = Mathf.Lerp(audioSource.volume, targetVolume, lerpSpeed);
            }
        }

        /// <summary>
        /// Call this if the original volume changes at runtime (e.g. from a mixer).
        /// </summary>
        public void RefreshOriginalVolume()
        {
            if (audioSource != null)
            {
                originalVolume = audioSource.volume;
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (listenerTransform != null)
            {
                bool isOccluded = targetCutoff < maxCutoffFreq;
                Gizmos.color = isOccluded ? Color.red : Color.green;
                Gizmos.DrawLine(transform.position, listenerTransform.position);

                if (isOccluded)
                {
                    UnityEditor.Handles.color = Color.red;
                    UnityEditor.Handles.Label(transform.position + Vector3.up * 0.3f,
                        $"Occluded (Cutoff: {lowPassFilter.cutoffFrequency:F0}Hz)");
                }
            }
        }
#endif
    }
}

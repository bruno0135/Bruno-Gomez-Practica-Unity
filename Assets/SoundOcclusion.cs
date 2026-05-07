using UnityEngine;

namespace BrunoGomez
{
    [RequireComponent(typeof(AudioSource))]
    public class SoundOcclusion : MonoBehaviour
    {
        [Header("Occlusion Settings")]
        [Tooltip("The layers that will block the sound (e.g. Default, World)")]
        public LayerMask obstacleLayers = -1; 
        
        [Tooltip("Frequency when the sound is occluded (lower = more muffled)")]
        public float minCutoffFreq = 500f;    
        
        [Tooltip("Frequency when the sound is clear")]
        public float maxCutoffFreq = 22000f;  
        
        [Tooltip("How fast the filter transitions")]
        public float smoothSpeed = 10f;       

        private AudioSource audioSource;
        private AudioLowPassFilter lowPassFilter;
        private Transform listenerTransform;
        private float targetCutoff;

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
            if (listenerTransform == null)
            {
                if (Camera.main != null) listenerTransform = Camera.main.transform;
                else return;
            }

            // Raycast direction: From this object to the listener
            Vector3 direction = listenerTransform.position - transform.position;
            float distance = direction.magnitude;

            RaycastHit hit;
            // Raycast check
            if (Physics.Raycast(transform.position, direction, out hit, distance, obstacleLayers, QueryTriggerInteraction.Ignore))
            {
                // If we hit something that is NOT the listener or a child of it, it's an obstacle
                if (hit.transform != listenerTransform && !hit.transform.IsChildOf(listenerTransform))
                {
                    targetCutoff = minCutoffFreq;
                }
                else
                {
                    targetCutoff = maxCutoffFreq;
                }
            }
            else
            {
                targetCutoff = maxCutoffFreq;
            }

            // Smooth transition
            lowPassFilter.cutoffFrequency = Mathf.Lerp(lowPassFilter.cutoffFrequency, targetCutoff, Time.deltaTime * smoothSpeed);
        }
        
        private void OnDrawGizmosSelected()
        {
            if (listenerTransform != null)
            {
                Gizmos.color = targetCutoff < maxCutoffFreq ? Color.red : Color.green;
                Gizmos.DrawLine(transform.position, listenerTransform.position);
            }
        }
    }
}

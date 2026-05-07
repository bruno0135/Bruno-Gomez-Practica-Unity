using UnityEngine;
using UnityEngine.Audio;

namespace BrunoGomez
{
    /// <summary>
    /// Audio zone trigger for interior/exterior transitions using AudioMixer snapshots.
    /// SETUP: Add a trigger collider, assign enter/exit snapshots, ensure Player has "Player" tag.
    /// </summary>
    public class AudioZoneTrigger : MonoBehaviour
    {
        [Header("Mixer Snapshots")]
        [Tooltip("Snapshot when player ENTERS (e.g. Interior)")]
        public AudioMixerSnapshot enterSnapshot;
        [Tooltip("Snapshot when player EXITS (e.g. Exterior)")]
        public AudioMixerSnapshot exitSnapshot;

        [Header("Transition")]
        public float enterTransitionTime = 1.0f;
        public float exitTransitionTime = 1.5f;

        [Header("Weighted Blend (Optional)")]
        public bool useWeightedBlend = false;
        public AudioMixerSnapshot[] blendSnapshots;
        public float[] blendWeights;

        [Header("Debug")]
        public bool showDebug = true;

        private bool playerInside = false;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player") || playerInside) return;
            playerInside = true;

            if (useWeightedBlend && blendSnapshots != null && blendSnapshots.Length > 0
                && blendWeights != null && blendWeights.Length == blendSnapshots.Length)
            {
                blendSnapshots[0].audioMixer.TransitionToSnapshots(blendSnapshots, blendWeights, enterTransitionTime);
                if (showDebug) Debug.Log($"[AudioZone] {gameObject.name}: Weighted blend ({enterTransitionTime}s)");
            }
            else if (enterSnapshot != null)
            {
                enterSnapshot.TransitionTo(enterTransitionTime);
                if (showDebug) Debug.Log($"[AudioZone] {gameObject.name}: ENTER → {enterSnapshot.name} ({enterTransitionTime}s)");
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player") || !playerInside) return;
            playerInside = false;

            if (exitSnapshot != null)
            {
                exitSnapshot.TransitionTo(exitTransitionTime);
                if (showDebug) Debug.Log($"[AudioZone] {gameObject.name}: EXIT → {exitSnapshot.name} ({exitTransitionTime}s)");
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            BoxCollider box = GetComponent<BoxCollider>();
            if (box != null)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.color = playerInside ? new Color(0, 0.8f, 1, 0.15f) : new Color(1, 0.8f, 0, 0.08f);
                Gizmos.DrawCube(box.center, box.size);
                Gizmos.color = playerInside ? new Color(0, 0.8f, 1, 0.5f) : new Color(1, 0.8f, 0, 0.3f);
                Gizmos.DrawWireCube(box.center, box.size);
            }
        }
#endif
    }
}

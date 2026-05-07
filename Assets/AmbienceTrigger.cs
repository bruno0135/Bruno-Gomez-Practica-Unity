using UnityEngine;
using UnityEngine.Audio;

namespace BrunoGomez
{
    public class AmbienceTrigger : MonoBehaviour
    {
        [Header("Mixer Settings")]
        [Tooltip("The Snapshot from the Audio Mixer to activate when entering this trigger")]
        public AudioMixerSnapshot snapshotToActivate;
        
        [Tooltip("How long the transition to the new snapshot should take")]
        public float transitionTime = 1.5f;

        private void OnTriggerEnter(Collider other)
        {
            // Ensure the object has the "Player" tag
            if (other.CompareTag("Player"))
            {
                if (snapshotToActivate != null)
                {
                    Debug.Log($"[Ambience] Transitioning to: {snapshotToActivate.name}");
                    snapshotToActivate.TransitionTo(transitionTime);
                }
            }
        }
    }
}

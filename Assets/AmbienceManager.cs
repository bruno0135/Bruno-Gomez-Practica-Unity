using UnityEngine;

namespace BrunoGomez
{
    /// <summary>
    /// Global ambience sound manager. Plays Ambiant_Loop.wav as a 2D background sound
    /// throughout the entire scene.
    /// 
    /// SETUP in Unity:
    /// 1. Create an empty GameObject in the scene (e.g. "AmbienceManager")
    /// 2. Add this component to it
    /// 3. Drag the Ambiant_Loop.wav audio clip into the "Ambience Clip" field
    /// 4. Adjust volume as desired
    /// </summary>
    public class AmbienceManager : MonoBehaviour
    {
        [Header("Ambience Settings")]
        [Tooltip("The ambient loop audio clip (Ambiant_Loop.wav)")]
        public AudioClip ambienceClip;
        
        [Tooltip("Volume of the ambient sound (0-1)")]
        [Range(0f, 1f)]
        public float volume = 0.25f;
        
        [Header("Optional")]
        [Tooltip("Audio Mixer Group to route the ambience to")]
        public UnityEngine.Audio.AudioMixerGroup mixerGroup;
        
        private AudioSource audioSource;

        void Start()
        {
            // Create AudioSource
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            
            // Configure as 2D global sound (heard everywhere equally)
            audioSource.clip = ambienceClip;
            audioSource.loop = true;
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; // Full 2D - heard everywhere
            audioSource.volume = volume;
            audioSource.priority = 0; // Highest priority so it never gets culled
            
            if (mixerGroup != null)
            {
                audioSource.outputAudioMixerGroup = mixerGroup;
            }
            
            if (ambienceClip != null)
            {
                audioSource.Play();
                Debug.Log("[AmbienceManager] Ambient loop started playing.");
            }
            else
            {
                Debug.LogWarning("[AmbienceManager] No ambience clip assigned!");
            }
        }
    }
}

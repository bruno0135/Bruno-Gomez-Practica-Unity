using UnityEngine;

namespace BrunoGomez
{
    [RequireComponent(typeof(AudioSource))]
    public class TimedSoundTrigger : MonoBehaviour
    {
        [Header("Configuración de Tiempo")]
        [Tooltip("Segundos que deben pasar antes de sonar")]
        public float delay = 60f;
        
        [Tooltip("¿Debe sonar cada 60 segundos (bucle) o solo una vez?")]
        public bool repeatEveryInterval = true;

        [Header("Audio")]
        public AudioClip audioClip;
        
        [Range(0f, 1f)]
        public float volume = 0.05f;

        [Header("3D Settings")]
        [Range(0f, 1f)]
        public float spatialBlend = 1.0f;
        public float minDistance = 1f;
        public float maxDistance = 20f;

        [Header("Output")]
        public UnityEngine.Audio.AudioMixerGroup mixerGroup;

        private AudioSource audioSource;
        private float timer;
        private bool hasPlayed = false;

        void Start()
        {
            // Configurar el AudioSource automáticamente
            audioSource = GetComponent<AudioSource>();
            audioSource.clip = audioClip;
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.spatialBlend = spatialBlend;
            audioSource.minDistance = minDistance;
            audioSource.maxDistance = maxDistance;
            audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
            
            if (mixerGroup != null)
                audioSource.outputAudioMixerGroup = mixerGroup;
            
            // Seteamos el volumen del componente a 1 para que el volumen del script sea el valor real
            audioSource.volume = 1f; 
            
            timer = delay;
        }

        void Update()
        {
            if (hasPlayed && !repeatEveryInterval) return;

            timer -= Time.deltaTime;

            if (timer <= 0f)
            {
                PlaySound();
                
                if (repeatEveryInterval)
                {
                    timer = delay; // Reiniciar cuenta atrás
                }
                else
                {
                    hasPlayed = true;
                }
            }
        }

        void PlaySound()
        {
            if (audioClip != null)
            {
                audioSource.PlayOneShot(audioClip, volume);
                Debug.Log($"[TimedSound] Sonando en {gameObject.name} después de {delay} segundos.");
            }
            else
            {
                Debug.LogWarning($"[TimedSound] {gameObject.name} no tiene un AudioClip asignado.");
            }
        }

    }
}

using UnityEngine;

namespace BrunoGomez
{
    [RequireComponent(typeof(AudioSource))]
    public class TimedSoundTrigger : MonoBehaviour
    {
        [Header("Configuración de Tiempo")]
        [Tooltip("Segundos que deben pasar antes de sonar")]
        public float delay = 20f;
        
        [Tooltip("¿Debe sonar cada 20 segundos (bucle) o solo una vez?")]
        public bool repeatEveryInterval = false;

        [Header("Audio")]
        public AudioClip audioClip;
        
        [Range(0f, 1f)]
        public float volume = 0.3f;

        private AudioSource audioSource;
        private float timer;
        private bool hasPlayed = false;

        void Start()
        {
            // Configurar el AudioSource automáticamente
            audioSource = GetComponent<AudioSource>();
            audioSource.clip = audioClip;
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; // Modo 2D (se oye en todo el nivel)
            audioSource.volume = volume;
            
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

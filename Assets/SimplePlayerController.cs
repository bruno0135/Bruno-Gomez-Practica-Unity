using UnityEngine;

namespace BrunoGomez
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(Animator))]
    public class SimplePlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        public float walkSpeed = 2.0f;
        public float runSpeed = 5.0f;
        public float rotationSpeed = 10.0f;
        public float gravity = -9.81f;
        public float jumpHeight = 1.5f;

        [Header("Camera Settings")]
        public Transform cameraTransform;
        public float lookSensitivity = 2.0f;
        public float minPitch = -30f;
        public float maxPitch = 60f;
        public float cameraDistance = 3.0f;
        public Vector3 cameraOffset = new Vector3(0, 1.5f, 0);

        [Header("Footstep Audio")]
        [Tooltip("Array of footstep audio clips (Player_Footstep_01 to _10)")]
        public AudioClip[] footstepClips;
        
        [Tooltip("Sound played when landing after a jump or fall")]
        public AudioClip landingClip;
        
        [Tooltip("Volume for footstep sounds")]
        [Range(0f, 1f)]
        public float footstepVolume = 0.5f;
        
        [Tooltip("Time interval between footsteps when walking")]
        public float walkStepInterval = 0.5f;
        
        [Tooltip("Time interval between footsteps when running")]
        public float runStepInterval = 0.3f;
        
        [Tooltip("Optional Audio Mixer Group for footsteps")]
        public UnityEngine.Audio.AudioMixerGroup footstepMixerGroup;

        private CharacterController controller;
        private Animator animator;
        private Vector3 velocity;
        private bool isGrounded;
        private float yaw;
        private float pitch;

        // Animation IDs
        private int animIDSpeed;
        private int animIDGrounded;
        private int animIDJump;
        private int animIDFreeFall;
        private int animIDMotionSpeed;

        // Footstep audio
        private AudioSource footstepAudioSource;
        private float footstepTimer;
        private int lastFootstepIndex = -1;

        void Start()
        {
            controller = GetComponent<CharacterController>();
            animator = GetComponent<Animator>();

            // If no camera assigned, try to find Main Camera
            if (cameraTransform == null && Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
            }

            // Assign Animation IDs
            animIDSpeed = Animator.StringToHash("Speed");
            animIDGrounded = Animator.StringToHash("Grounded");
            animIDJump = Animator.StringToHash("Jump");
            animIDFreeFall = Animator.StringToHash("FreeFall");
            animIDMotionSpeed = Animator.StringToHash("MotionSpeed");

            // Setup footstep AudioSource
            footstepAudioSource = gameObject.AddComponent<AudioSource>();
            footstepAudioSource.playOnAwake = false;
            footstepAudioSource.spatialBlend = 1.0f; // 3D sound
            footstepAudioSource.minDistance = 1f;
            footstepAudioSource.maxDistance = 15f;
            footstepAudioSource.volume = footstepVolume;
            if (footstepMixerGroup != null)
            {
                footstepAudioSource.outputAudioMixerGroup = footstepMixerGroup;
            }

            // Lock cursor
            Cursor.lockState = CursorLockMode.Locked;
        }

        void Update()
        {
            if (cameraTransform == null)
            {
                if (Camera.main != null) cameraTransform = Camera.main.transform;
                else return;
            }

            // --- GROUNDED CHECK ---
            // Small offset to the sphere to make it more reliable
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y + 0.1f, transform.position.z);
            bool wasGrounded = isGrounded;
            isGrounded = Physics.CheckSphere(spherePosition, 0.2f, -1, QueryTriggerInteraction.Ignore);

            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2f;
            }

            // Reset Jump parameter when landing + play landing sound
            if (isGrounded && !wasGrounded)
            {
                if (animator != null)
                {
                    animator.SetBool(animIDJump, false);
                    animator.SetBool(animIDFreeFall, false);
                }
                
                // Play landing sound
                if (landingClip != null && footstepAudioSource != null)
                {
                    footstepAudioSource.PlayOneShot(landingClip, footstepVolume);
                }
            }

            // --- INPUT HANDLING ---
            float horizontal = 0;
            float vertical = 0;
            bool isRunning = false;
            bool jumpPressed = false;

            if (UnityEngine.InputSystem.Keyboard.current != null)
            {
                var kb = UnityEngine.InputSystem.Keyboard.current;
                if (kb.wKey.isPressed) vertical += 1;
                if (kb.sKey.isPressed) vertical -= 1;
                if (kb.aKey.isPressed) horizontal -= 1;
                if (kb.dKey.isPressed) horizontal += 1;
                
                isRunning = kb.leftShiftKey.isPressed;
                jumpPressed = kb.spaceKey.wasPressedThisFrame;
            }

            Vector3 direction = new Vector3(horizontal, 0, vertical).normalized;
            float currentSpeed = isRunning ? runSpeed : walkSpeed;
            bool isMoving = direction.magnitude >= 0.1f;

            if (isMoving)
            {
                float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref rotationSpeed, 0.1f);
                transform.rotation = Quaternion.Euler(0, angle, 0);

                Vector3 moveDirection = Quaternion.Euler(0, targetAngle, 0) * Vector3.forward;
                controller.Move(moveDirection.normalized * currentSpeed * Time.deltaTime);
                
                if (animator != null)
                {
                    float speedValue = isRunning ? 6.0f : 2.0f;
                    animator.SetFloat(animIDSpeed, speedValue, 0.1f, Time.deltaTime);
                    animator.SetFloat(animIDMotionSpeed, 1.0f);
                }
                
                // --- FOOTSTEP AUDIO ---
                if (isGrounded)
                {
                    float stepInterval = isRunning ? runStepInterval : walkStepInterval;
                    footstepTimer -= Time.deltaTime;
                    
                    if (footstepTimer <= 0f)
                    {
                        PlayFootstep();
                        footstepTimer = stepInterval;
                    }
                }
            }
            else
            {
                if (animator != null) 
                {
                    animator.SetFloat(animIDSpeed, 0, 0.1f, Time.deltaTime);
                    animator.SetFloat(animIDMotionSpeed, 1.0f);
                }
                
                // Reset footstep timer when not moving
                footstepTimer = 0f;
            }

            // Jump
            if (jumpPressed && isGrounded)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                if (animator != null) animator.SetBool(animIDJump, true);
            }

            // Gravity
            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);

            // Animator Updates
            if (animator != null)
            {
                animator.SetBool(animIDGrounded, isGrounded);
                if (!isGrounded && velocity.y < -1f) animator.SetBool(animIDFreeFall, true);
            }
        }

        /// <summary>
        /// Plays a random footstep sound, avoiding repeating the same one twice in a row.
        /// </summary>
        private void PlayFootstep()
        {
            if (footstepClips == null || footstepClips.Length == 0 || footstepAudioSource == null) return;
            
            // Pick a random clip, avoiding the last one played
            int index;
            if (footstepClips.Length > 1)
            {
                do
                {
                    index = Random.Range(0, footstepClips.Length);
                } while (index == lastFootstepIndex);
            }
            else
            {
                index = 0;
            }
            
            lastFootstepIndex = index;
            
            if (footstepClips[index] != null)
            {
                footstepAudioSource.PlayOneShot(footstepClips[index], footstepVolume);
            }
        }

        void LateUpdate()
        {
            if (cameraTransform == null) return;

            float mouseX = 0;
            float mouseY = 0;

            if (UnityEngine.InputSystem.Mouse.current != null)
            {
                var mouse = UnityEngine.InputSystem.Mouse.current;
                Vector2 mouseDelta = mouse.delta.ReadValue() * 0.1f;
                mouseX = mouseDelta.x * lookSensitivity;
                mouseY = mouseDelta.y * lookSensitivity;
            }

            yaw += mouseX;
            pitch -= mouseY;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
            Vector3 headPosition = transform.position + cameraOffset;
            Vector3 position = headPosition - (rotation * Vector3.forward * cameraDistance);

            cameraTransform.position = position;
            cameraTransform.rotation = rotation;
        }
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(new Vector3(transform.position.x, transform.position.y + 0.1f, transform.position.z), 0.2f);
        }
    }
}

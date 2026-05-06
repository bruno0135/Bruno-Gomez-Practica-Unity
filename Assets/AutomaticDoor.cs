using UnityEngine;

namespace BrunoGomez
{
    public class AutomaticDoor : MonoBehaviour
    {
        [Header("Door Settings")]
        public float interactionRange = 3.5f;
        public float openSpeed = 4.0f;
        public Vector3 openOffset = new Vector3(1.2f, 0, 0);
        public bool isVerticalDoor = false;
        
        [Header("Status")]
        public bool isOpen = false;
        
        [Header("Components")]
        public Transform leftPanel;
        public Transform rightPanel;
        public Animator doorAnimator;
        
        [Header("UI Settings")]
        public GameObject interactHint;
        
        private Vector3 leftClosedPos;
        private Vector3 rightClosedPos;
        private Transform playerTransform;

        void Start()
        {
            FindPlayer();

            // 1. Try to find panels in children
            SearchForPanels(transform);
            
            // 2. If not found, try to find in parent (siblings)
            if ((leftPanel == null || rightPanel == null) && transform.parent != null)
            {
                SearchForPanels(transform.parent);
            }
            
            // 3. If still no panels found, warn the user
            if (leftPanel == null && rightPanel == null)
            {
                Debug.LogError($"[Door] {gameObject.name} could NOT find any door panels! Please assign 'Left Panel' and 'Right Panel' manually in the Inspector.");
            }
            
            // Safety check: Don't let the script move itself
            if (leftPanel == this.transform) leftPanel = null;
            if (rightPanel == this.transform) rightPanel = null;

            // Setup Animator
            doorAnimator = GetComponent<Animator>();
            if (doorAnimator == null) doorAnimator = GetComponentInChildren<Animator>();
            if (doorAnimator == null && transform.parent != null) doorAnimator = transform.parent.GetComponentInChildren<Animator>();

            // Save initial positions
            if (leftPanel != null) {
                leftClosedPos = leftPanel.localPosition;
                Debug.Log($"[Door] {gameObject.name} -> Left Panel Ready: {leftPanel.name}");
            }
            if (rightPanel != null) {
                rightClosedPos = rightPanel.localPosition;
                Debug.Log($"[Door] {gameObject.name} -> Right Panel Ready: {rightPanel.name}");
            }

            // Ensure physics doesn't block it if we are moving it manually
            SetupKinematic(this.transform);
            if (leftPanel != null) SetupKinematic(leftPanel);
            if (rightPanel != null) SetupKinematic(rightPanel);

            if (interactHint != null) interactHint.SetActive(false);
        }

        private void SetupKinematic(Transform t)
        {
            Rigidbody rb = t.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;
            
            // Also check children of panels
            foreach (Rigidbody childRb in t.GetComponentsInChildren<Rigidbody>())
            {
                childRb.isKinematic = true;
            }
        }

        private void SearchForPanels(Transform root)
        {
            foreach (Transform child in root)
            {
                string n = child.name.ToLower();
                // Specific check for user's requested names: "door left_01" and "door_right_01"
                if (leftPanel == null && (n.Contains("left_01") || n.Contains("left") || n.Contains("_01"))) 
                    leftPanel = child;
                
                if (rightPanel == null && (n.Contains("right_01") || n.Contains("right") || n.Contains("_02"))) 
                    rightPanel = child;
            }
        }

        void Update()
        {
            if (playerTransform == null) 
            {
                FindPlayer();
                return;
            }

            float distance = Vector3.Distance(transform.position, playerTransform.position);
            bool isNear = distance <= interactionRange;

            // Update UI Hint
            if (interactHint != null)
            {
                interactHint.SetActive(isNear && !isOpen);
            }

            // Handle Interaction: Press 'E' to toggle (open/close) when near
            if (isNear && UnityEngine.InputSystem.Keyboard.current != null)
            {
                if (UnityEngine.InputSystem.Keyboard.current.eKey.wasPressedThisFrame)
                {
                    Debug.Log($"[Door] {gameObject.name} interaction pressed.");
                    ToggleDoor();
                }
            }

            // Auto-Close logic: If player moves away, ensure the door is closed
            // We DON'T sync during auto-close to avoid infinite loops if multiple doors are nearby
            if (distance > interactionRange + 0.5f && isOpen)
            {
                Debug.Log($"[Door] {gameObject.name} auto-closing because player is far.");
                SetState(false);
            }

            ApplyMovement();
        }

        private void FindPlayer()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerTransform = player.transform;
        }

        public void ToggleDoor()
        {
            bool newState = !isOpen;
            SetState(newState);
            SyncNearbyDoors(newState);
        }

        private void SyncNearbyDoors(bool open)
        {
            // Updated obsolete method to FindObjectsByType
            AutomaticDoor[] allDoors = Object.FindObjectsByType<AutomaticDoor>(FindObjectsSortMode.None);
            foreach (var other in allDoors)
            {
                if (other == this) continue;
                if (Vector3.Distance(transform.position, other.transform.position) < 2.5f)
                {
                    if (other.isOpen != open) other.SetState(open);
                }
            }
        }

        public void SetState(bool open)
        {
            if (isOpen == open) return;
            
            isOpen = open;
            Debug.Log($"[Door] {gameObject.name} state changed to: {(isOpen ? "OPEN" : "CLOSED")}");

            if (doorAnimator != null)
            {
                bool hasParam = false;
                if (doorAnimator.runtimeAnimatorController != null)
                {
                    foreach (AnimatorControllerParameter param in doorAnimator.parameters)
                    {
                        if (param.name == "Open" || param.name == "character_nearby")
                        {
                            doorAnimator.SetBool(param.name, isOpen);
                            hasParam = true;
                        }
                    }
                }
                
                // AGGRESSIVE: If animator is present but has no "Open" param, disable it to allow manual movement
                if (!hasParam && doorAnimator.enabled)
                {
                    Debug.LogWarning($"[Door] Disabling Animator on {gameObject.name} because it has no 'Open' parameter. Manual movement will take over.");
                    doorAnimator.enabled = false;
                }
            }
        }

        void ApplyMovement()
        {
            // Skip manual movement ONLY if we have a WORKING animator with a controller
            if (doorAnimator != null && doorAnimator.enabled && doorAnimator.runtimeAnimatorController != null) return;

            float step = Time.deltaTime * openSpeed;

            if (isVerticalDoor)
            {
                if (leftPanel != null)
                {
                    Vector3 target = isOpen ? leftClosedPos + new Vector3(0, 2.5f, 0) : leftClosedPos;
                    leftPanel.localPosition = Vector3.Lerp(leftPanel.localPosition, target, step);
                }
                if (rightPanel != null && rightPanel != leftPanel)
                {
                    Vector3 target = isOpen ? rightClosedPos + new Vector3(0, 2.5f, 0) : rightClosedPos;
                    rightPanel.localPosition = Vector3.Lerp(rightPanel.localPosition, target, step);
                }
            }
            else
            {
                if (leftPanel != null)
                {
                    Vector3 target = isOpen ? leftClosedPos - openOffset : leftClosedPos;
                    leftPanel.localPosition = Vector3.Lerp(leftPanel.localPosition, target, step);
                }
                if (rightPanel != null && rightPanel != leftPanel)
                {
                    Vector3 target = isOpen ? rightClosedPos + openOffset : rightClosedPos;
                    rightPanel.localPosition = Vector3.Lerp(rightPanel.localPosition, target, step);
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = isOpen ? Color.green : Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactionRange);
        }
    }
}

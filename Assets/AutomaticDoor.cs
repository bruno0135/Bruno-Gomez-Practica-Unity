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
        
        private Vector3 leftClosedPos;
        private Vector3 rightClosedPos;
        private Transform playerTransform;

        void Start()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerTransform = player.transform;

            // 1. Try to find panels in children
            foreach (Transform child in transform)
            {
                string n = child.name.ToLower();
                if (leftPanel == null && (n.Contains("left") || n.Contains("_01"))) leftPanel = child;
                if (rightPanel == null && (n.Contains("right") || n.Contains("_02"))) rightPanel = child;
                if (n.Contains("ver") || n.Contains("doorway")) isVerticalDoor = true;
            }
            
            // 2. If no panels found, this object IS the panel
            if (leftPanel == null && rightPanel == null)
            {
                leftPanel = this.transform;
                string n = name.ToLower();
                if (n.Contains("right") || n.Contains("_02")) { 
                    // It's a right panel, move it the other way
                    openOffset = -openOffset; 
                }
                if (n.Contains("ver")) isVerticalDoor = true;
            }

            doorAnimator = GetComponent<Animator>();
            if (doorAnimator == null) doorAnimator = GetComponentInChildren<Animator>();

            if (leftPanel != null) leftClosedPos = leftPanel.localPosition;
            if (rightPanel != null) rightClosedPos = rightPanel.localPosition;
            
            gameObject.isStatic = false;
        }

        void Update()
        {
            if (playerTransform == null) return;

            float distance = Vector3.Distance(transform.position, playerTransform.position);
            bool isNear = distance <= interactionRange;

            if (isNear && UnityEngine.InputSystem.Keyboard.current.eKey.wasPressedThisFrame)
            {
                ToggleDoor();
                
                // SYNC: Find sibling doors (like the other half of a double door) and toggle them too
                AutomaticDoor[] allDoors = FindObjectsOfType<AutomaticDoor>();
                foreach (var other in allDoors)
                {
                    if (other == this) continue;
                    // If the other door is very close to this one (same doorway)
                    if (Vector3.Distance(transform.position, other.transform.position) < 2.0f)
                    {
                        other.SetState(this.isOpen);
                    }
                }
            }

            ApplyMovement();
        }

        public void ToggleDoor()
        {
            SetState(!isOpen);
        }

        public void SetState(bool open)
        {
            isOpen = open;
            if (doorAnimator != null)
            {
                doorAnimator.SetBool("Open", isOpen);
                doorAnimator.SetBool("character_nearby", isOpen);
            }
        }

        void ApplyMovement()
        {
            if (doorAnimator != null && doorAnimator.enabled) return;

            if (isVerticalDoor && leftPanel != null)
            {
                Vector3 target = isOpen ? leftClosedPos + new Vector3(0, 2.5f, 0) : leftClosedPos;
                leftPanel.localPosition = Vector3.Lerp(leftPanel.localPosition, target, Time.deltaTime * openSpeed);
            }
            else
            {
                if (leftPanel != null)
                {
                    Vector3 target = isOpen ? leftClosedPos - openOffset : leftClosedPos;
                    leftPanel.localPosition = Vector3.Lerp(leftPanel.localPosition, target, Time.deltaTime * openSpeed);
                }
                if (rightPanel != null && rightPanel != leftPanel)
                {
                    Vector3 target = isOpen ? rightClosedPos + openOffset : rightClosedPos;
                    rightPanel.localPosition = Vector3.Lerp(rightPanel.localPosition, target, Time.deltaTime * openSpeed);
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

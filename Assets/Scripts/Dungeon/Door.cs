using UnityEngine;

namespace PixelGame
{
    /// <summary>
    /// Door that blocks passage between rooms
    /// </summary>
    public class Door : MonoBehaviour
    {
        [Header("Door Settings")]
        [SerializeField] private bool isLocked = true;
        [SerializeField] private bool startsOpen = false;

        [Header("Components")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Collider2D doorCollider;
        [SerializeField] private Animator animator;

        [Header("Visual")]
        [SerializeField] private Sprite openSprite;
        [SerializeField] private Sprite closedSprite;

        private bool isOpen = false;

        private void Start()
        {
            if (doorCollider == null)
            {
                doorCollider = GetComponent<Collider2D>();
            }

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            if (startsOpen)
            {
                Open();
            }
            else
            {
                Close();
            }
        }

        public void Open()
        {
            if (isOpen) return;

            isOpen = true;
            isLocked = false;

            // Visual changes
            if (spriteRenderer != null && openSprite != null)
            {
                spriteRenderer.sprite = openSprite;
            }

            // Disable collision
            if (doorCollider != null)
            {
                doorCollider.enabled = false;
            }

            // Animation
            if (animator != null)
            {
                animator.SetTrigger("Open");
            }

            // Sound effect
            GameEvents.PlaySoundEffect("DoorOpen", transform.position);
        }

        public void Close()
        {
            if (!isOpen && isLocked) return;

            isOpen = false;
            isLocked = true;

            // Visual changes
            if (spriteRenderer != null && closedSprite != null)
            {
                spriteRenderer.sprite = closedSprite;
            }

            // Enable collision
            if (doorCollider != null)
            {
                doorCollider.enabled = true;
            }

            // Animation
            if (animator != null)
            {
                animator.SetTrigger("Close");
            }

            // Sound effect
            GameEvents.PlaySoundEffect("DoorClose", transform.position);
        }

        public void Toggle()
        {
            if (isOpen)
            {
                Close();
            }
            else
            {
                Open();
            }
        }

        public bool IsOpen() => isOpen;
        public bool IsLocked() => isLocked;

        public void Unlock()
        {
            isLocked = false;
        }

        public void Lock()
        {
            isLocked = true;
        }
    }
}

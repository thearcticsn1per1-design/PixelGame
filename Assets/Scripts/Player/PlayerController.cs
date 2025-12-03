using UnityEngine;
using System.Collections;

namespace PixelGame
{
    /// <summary>
    /// Main player controller handling movement, combat, and input
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(PlayerStats))]
    public class PlayerController : MonoBehaviour, IDamageable
    {
        [Header("Components")]
        [SerializeField] private Transform weaponPivot;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Animator animator;

        [Header("Movement Settings")]
        [SerializeField] private float dashSpeed = 15f;
        [SerializeField] private float dashDuration = 0.2f;
        [SerializeField] private float baseDashCooldown = 1f;

        [Header("Combat Settings")]
        [SerializeField] private LayerMask pickupLayer;
        [SerializeField] private float basePickupRange = 2f;

        [Header("Invincibility")]
        [SerializeField] private float invincibilityDuration = 0.5f;
        [SerializeField] private float flashInterval = 0.1f;

        // Components
        private Rigidbody2D rb;
        private PlayerStats stats;
        private InventorySystem inventory;
        private WeaponBase currentWeapon;

        // Input
        private Vector2 moveInput;
        private Vector2 aimDirection;
        private bool attackInput;
        private bool dashInput;
        private bool interactInput;

        // State
        private bool isDashing = false;
        private bool canDash = true;
        private bool isInvincible = false;
        private bool isDead = false;

        // Character class
        private CharacterClass characterClass;

        #region Unity Lifecycle

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            stats = GetComponent<PlayerStats>();
            inventory = GetComponent<InventorySystem>();

            if (weaponPivot == null)
            {
                // Create weapon pivot if not assigned
                GameObject pivotObj = new GameObject("WeaponPivot");
                pivotObj.transform.SetParent(transform);
                pivotObj.transform.localPosition = Vector3.zero;
                weaponPivot = pivotObj.transform;
            }
        }

        private void Start()
        {
            stats.Initialize();
            SubscribeToEvents();
        }

        private void Update()
        {
            if (isDead) return;

            HandleInput();
            HandleAiming();
            HandleAttack();
            HandlePickups();

            // Apply regeneration
            stats.ApplyRegeneration(Time.deltaTime);

            UpdateAnimations();
        }

        private void FixedUpdate()
        {
            if (isDead) return;

            HandleMovement();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initialize player with character class data
        /// </summary>
        public void Initialize(CharacterClass classData)
        {
            characterClass = classData;

            // Apply class stats
            ApplyClassStats();

            // Equip starting weapon
            if (classData.startingWeapon != null)
            {
                EquipWeapon(classData.startingWeapon);
            }

            // Apply starting traits
            if (classData.startingTraits != null)
            {
                foreach (var trait in classData.startingTraits)
                {
                    trait?.Apply(stats);
                }
            }
        }

        private void ApplyClassStats()
        {
            if (characterClass == null) return;

            stats.baseMaxHealth += characterClass.healthBonus;
            stats.baseDamage += characterClass.damageBonus;
            stats.baseMoveSpeed += characterClass.moveSpeedBonus;
            stats.baseArmor += characterClass.armorBonus;

            // Apply class-specific modifiers
            foreach (var modifier in characterClass.statModifiers)
            {
                if (modifier.isMultiplicative)
                {
                    stats.AddMultiplierModifier(modifier.statType, modifier.value);
                }
                else
                {
                    stats.AddFlatModifier(modifier.statType, modifier.value);
                }
            }

            stats.Initialize();
        }

        private void SubscribeToEvents()
        {
            stats.OnHealthChanged += HandleHealthChanged;
        }

        private void UnsubscribeFromEvents()
        {
            stats.OnHealthChanged -= HandleHealthChanged;
        }

        #endregion

        #region Input Handling

        private void HandleInput()
        {
            // Movement input (WASD or Arrow keys)
            moveInput = new Vector2(
                Input.GetAxisRaw("Horizontal"),
                Input.GetAxisRaw("Vertical")
            );

            // Attack input
            attackInput = Input.GetMouseButton(0); // Left click

            // Dash input
            if (Input.GetKeyDown(KeyCode.Space) && canDash && !isDashing)
            {
                dashInput = true;
            }

            // Interact input
            if (Input.GetKeyDown(KeyCode.E))
            {
                interactInput = true;
            }
        }

        private void HandleAiming()
        {
            // Aim toward mouse position
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;

            aimDirection = (mousePos - transform.position).normalized;

            if (weaponPivot != null && aimDirection.sqrMagnitude > 0.01f)
            {
                // Rotate weapon pivot to aim direction
                float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
                weaponPivot.rotation = Quaternion.Euler(0, 0, angle);

                // Flip weapon sprite if aiming left
                if (weaponPivot.localScale.y > 0 && aimDirection.x < 0)
                {
                    weaponPivot.localScale = new Vector3(weaponPivot.localScale.x, -1, 1);
                }
                else if (weaponPivot.localScale.y < 0 && aimDirection.x > 0)
                {
                    weaponPivot.localScale = new Vector3(weaponPivot.localScale.x, 1, 1);
                }
            }

            // Flip player sprite based on movement or aim direction
            if (spriteRenderer != null)
            {
                if (moveInput.x != 0)
                {
                    spriteRenderer.flipX = moveInput.x < 0;
                }
                else if (aimDirection.x != 0)
                {
                    spriteRenderer.flipX = aimDirection.x < 0;
                }
            }
        }

        #endregion

        #region Movement

        private void HandleMovement()
        {
            if (isDashing)
            {
                // Dashing movement handled by coroutine
                return;
            }

            // Normal movement
            float moveSpeed = stats.GetStat(StatType.MoveSpeed);
            Vector2 velocity = moveInput.normalized * moveSpeed;

            rb.linearVelocity = velocity;

            // Handle dash
            if (dashInput)
            {
                dashInput = false;
                StartCoroutine(PerformDash());
            }
        }

        private IEnumerator PerformDash()
        {
            isDashing = true;
            canDash = false;

            // Dash direction (use input or aim direction)
            Vector2 dashDir = moveInput.normalized;
            if (dashDir.sqrMagnitude < 0.1f)
            {
                dashDir = aimDirection;
            }

            // Become temporarily invincible during dash
            bool wasInvincible = isInvincible;
            isInvincible = true;

            // Dash movement
            float elapsed = 0f;
            while (elapsed < dashDuration)
            {
                rb.linearVelocity = dashDir * dashSpeed;
                elapsed += Time.deltaTime;
                yield return null;
            }

            rb.linearVelocity = Vector2.zero;
            isDashing = false;
            isInvincible = wasInvincible;

            // Dash cooldown
            float dashCooldown = baseDashCooldown / stats.GetStat(StatType.AttackSpeed);
            yield return new WaitForSeconds(dashCooldown);
            canDash = true;
        }

        #endregion

        #region Combat

        private void HandleAttack()
        {
            if (currentWeapon != null && attackInput)
            {
                currentWeapon.Attack(aimDirection);
            }
        }

        /// <summary>
        /// Equip a weapon
        /// </summary>
        public void EquipWeapon(WeaponData weaponData)
        {
            // Destroy current weapon
            if (currentWeapon != null)
            {
                Destroy(currentWeapon.gameObject);
            }

            // Instantiate new weapon
            if (weaponData != null && weaponData.weaponPrefab != null)
            {
                GameObject weaponObj = Instantiate(weaponData.weaponPrefab, weaponPivot);
                weaponObj.transform.localPosition = weaponData.weaponOffset;
                currentWeapon = weaponObj.GetComponent<WeaponBase>();

                if (currentWeapon != null)
                {
                    currentWeapon.Initialize(this, stats);
                }
            }
        }

        /// <summary>
        /// Get reference to current weapon
        /// </summary>
        public WeaponBase GetCurrentWeapon()
        {
            return currentWeapon;
        }

        #endregion

        #region Damage System

        public void TakeDamage(float damage, bool isCritical = false, GameObject damageSource = null)
        {
            if (isDead || isInvincible) return;

            // Check for dodge
            if (stats.RollDodge())
            {
                // Dodged!
                GameEvents.PlaySoundEffect("Dodge", transform.position);
                return;
            }

            // Apply damage reduction
            float damageReduction = stats.CalculateDamageReduction();
            float finalDamage = damage * (1f - damageReduction);

            // Apply damage
            stats.ModifyHealth(-finalDamage);

            // Trigger events
            GameEvents.PlayerDamaged(finalDamage, damageSource);
            GameEvents.DamageDealt(gameObject, finalDamage, isCritical);

            // Start invincibility frames
            StartCoroutine(InvincibilityFrames());

            // Check for death
            if (stats.currentHealth <= 0 && !isDead)
            {
                Die();
            }

            // Visual feedback
            StartCoroutine(DamageFlash());
        }

        public void Die()
        {
            if (isDead) return;

            isDead = true;
            rb.linearVelocity = Vector2.zero;

            // Trigger death events
            GameEvents.PlayerDeath();

            // Death animation
            if (animator != null)
            {
                animator.SetTrigger("Death");
            }

            // Disable components
            if (currentWeapon != null)
            {
                currentWeapon.gameObject.SetActive(false);
            }

            // Game over after delay
            StartCoroutine(GameOverDelay());
        }

        private IEnumerator GameOverDelay()
        {
            yield return new WaitForSeconds(2f);
            GameManager.Instance.ChangeState(GameState.GameOver);
        }

        public bool IsAlive()
        {
            return !isDead;
        }

        public float GetHealthPercentage()
        {
            return stats.GetHealthPercentage();
        }

        private IEnumerator InvincibilityFrames()
        {
            isInvincible = true;
            yield return new WaitForSeconds(invincibilityDuration);
            isInvincible = false;
        }

        private IEnumerator DamageFlash()
        {
            if (spriteRenderer == null) yield break;

            Color originalColor = spriteRenderer.color;
            float elapsed = 0f;

            while (elapsed < invincibilityDuration)
            {
                spriteRenderer.color = Color.Lerp(Color.red, originalColor, (elapsed / invincibilityDuration));
                elapsed += flashInterval;
                yield return new WaitForSeconds(flashInterval);
            }

            spriteRenderer.color = originalColor;
        }

        #endregion

        #region Pickups

        private void HandlePickups()
        {
            float pickupRange = basePickupRange + stats.GetStat(StatType.PickupRange);

            Collider2D[] pickups = Physics2D.OverlapCircleAll(transform.position, pickupRange, pickupLayer);

            foreach (var pickup in pickups)
            {
                IPickupable pickupable = pickup.GetComponent<IPickupable>();
                if (pickupable != null)
                {
                    pickupable.Pickup(this);
                }
            }
        }

        #endregion

        #region Animations

        private void UpdateAnimations()
        {
            if (animator == null) return;

            // Set movement parameters
            animator.SetFloat("Speed", moveInput.magnitude);
            animator.SetBool("IsDashing", isDashing);
        }

        #endregion

        #region Event Handlers

        private void HandleHealthChanged(float currentHealth, float maxHealth)
        {
            GameEvents.PlayerHealthChanged(currentHealth, maxHealth);
        }

        #endregion

        #region Gizmos

        private void OnDrawGizmosSelected()
        {
            // Draw pickup range
            float pickupRange = basePickupRange;
            if (Application.isPlaying && stats != null)
            {
                pickupRange += stats.GetStat(StatType.PickupRange);
            }

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, pickupRange);
        }

        #endregion
    }
}

using UnityEngine;
using System.Collections;

namespace PixelGame
{
    /// <summary>
    /// Main player controller handling movement, combat, and input
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
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

        // State
        private bool isDashing = false;
        private bool canDash = true;
        private bool isInvincible = false;
        private bool isDead = false;

        // Character class
        private CharacterClass characterClass;

        // --- ADDED: Direction enum + lastDirection tracking ---
        private enum Direction
        {
            North, NorthEast, East, SouthEast,
            South, SouthWest, West, NorthWest
        }

        private Direction lastDirection = Direction.South;

        // --- ADDED: Convert movement vector to direction ---
        private Direction GetDirectionFromInput(Vector2 input)
        {
            if (input == Vector2.zero)
                return lastDirection;

            float angle = Mathf.Atan2(input.y, input.x) * Mathf.Rad2Deg;
            if (angle < 0) angle += 360f;

            if (angle >= 337.5f || angle < 22.5f) return Direction.East;
            if (angle < 67.5f) return Direction.NorthEast;
            if (angle < 112.5f) return Direction.North;
            if (angle < 157.5f) return Direction.NorthWest;
            if (angle < 202.5f) return Direction.West;
            if (angle < 247.5f) return Direction.SouthWest;
            if (angle < 292.5f) return Direction.South;
            return Direction.SouthEast;
        }

        // --- ADDED: Clear all direction animator bools ---
        private void ClearDirectionBools()
        {
            animator.SetBool("MoveNorth", false);
            animator.SetBool("MoveNorthEast", false);
            animator.SetBool("MoveEast", false);
            animator.SetBool("MoveSouthEast", false);
            animator.SetBool("MoveSouth", false);
            animator.SetBool("MoveSouthWest", false);
            animator.SetBool("MoveWest", false);
            animator.SetBool("MoveNorthWest", false);

            animator.SetBool("isNorth", false);
            animator.SetBool("isNorthEast", false);
            animator.SetBool("isEast", false);
            animator.SetBool("isSouthEast", false);
            animator.SetBool("isSouth", false);
            animator.SetBool("isSouthWest", false);
            animator.SetBool("isWest", false);
            animator.SetBool("isNorthWest", false);
        }

        // --- ADDED: Set correct direction bools ---
        private void SetDirectionBools(Direction dir, bool isMoving)
        {
            switch (dir)
            {
                case Direction.North:
                    animator.SetBool("isNorth", true);
                    if (isMoving) animator.SetBool("MoveNorth", true);
                    break;
                case Direction.NorthEast:
                    animator.SetBool("isNorthEast", true);
                    if (isMoving) animator.SetBool("MoveNorthEast", true);
                    break;
                case Direction.East:
                    animator.SetBool("isEast", true);
                    if (isMoving) animator.SetBool("MoveEast", true);
                    break;
                case Direction.SouthEast:
                    animator.SetBool("isSouthEast", true);
                    if (isMoving) animator.SetBool("MoveSouthEast", true);
                    break;
                case Direction.South:
                    animator.SetBool("isSouth", true);
                    if (isMoving) animator.SetBool("MoveSouth", true);
                    break;
                case Direction.SouthWest:
                    animator.SetBool("isSouthWest", true);
                    if (isMoving) animator.SetBool("MoveSouthWest", true);
                    break;
                case Direction.West:
                    animator.SetBool("isWest", true);
                    if (isMoving) animator.SetBool("MoveWest", true);
                    break;
                case Direction.NorthWest:
                    animator.SetBool("isNorthWest", true);
                    if (isMoving) animator.SetBool("MoveNorthWest", true);
                    break;
            }
        }

        #region Unity Lifecycle

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            stats = GetComponent<PlayerStats>();
            inventory = GetComponent<InventorySystem>();

            if (weaponPivot == null)
            {
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

            // Check if animator is assigned and has correct parameters
            if (animator != null)
            {
                Debug.Log("PlayerController: Animator component found!");

                // List all parameters for debugging
                foreach (var param in animator.parameters)
                {
                    Debug.Log($"Animator Parameter: {param.name} ({param.type})");
                }
            }
            else
            {
                Debug.LogError("PlayerController: No Animator component assigned! Animations will not work.");
            }
        }

        private void Update()
        {
            if (isDead) return;

            HandleInput();
            HandleAiming();
            HandleAttack();
            HandlePickups();

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

        public void Initialize(CharacterClass classData)
        {
            Debug.Log($"PlayerController.Initialize: Starting initialization with class: {classData?.className}");
            characterClass = classData;

            ApplyClassStats();

            if (classData.startingWeapon != null)
            {
                Debug.Log($"PlayerController.Initialize: Starting weapon found: {classData.startingWeapon.weaponName}");
                EquipWeapon(classData.startingWeapon);
            }
            else
            {
                Debug.LogWarning("PlayerController.Initialize: No starting weapon assigned to character class!");
            }

            if (classData.startingTraits != null)
            {
                foreach (var trait in classData.startingTraits)
                {
                    trait?.Apply(stats);
                }
            }

            Debug.Log("PlayerController.Initialize: Initialization complete");
        }

        private void ApplyClassStats()
        {
            if (characterClass == null) return;

            stats.baseMaxHealth += characterClass.healthBonus;
            stats.baseDamage += characterClass.damageBonus;
            stats.baseMoveSpeed += characterClass.moveSpeedBonus;
            stats.baseArmor += characterClass.armorBonus;

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
            moveInput = new Vector2(
                Input.GetAxisRaw("Horizontal"),
                Input.GetAxisRaw("Vertical")
            );

            attackInput = Input.GetMouseButton(0);

            if (Input.GetKeyDown(KeyCode.Space) && canDash && !isDashing)
            {
                dashInput = true;
            }
        }

        private void HandleAiming()
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;

            aimDirection = (mousePos - transform.position).normalized;

            if (weaponPivot != null && aimDirection.sqrMagnitude > 0.01f)
            {
                float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
                weaponPivot.rotation = Quaternion.Euler(0, 0, angle);

                if (weaponPivot.localScale.y > 0 && aimDirection.x < 0)
                {
                    weaponPivot.localScale = new Vector3(weaponPivot.localScale.x, -1, 1);
                }
                else if (weaponPivot.localScale.y < 0 && aimDirection.x > 0)
                {
                    weaponPivot.localScale = new Vector3(weaponPivot.localScale.x, 1, 1);
                }
            }

            // Note: 8-directional sprites don't need flipping - they have sprites for each direction
            // If using simple 2D sprites without 8-directional animations, uncomment the flip code below:
            /*
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
            */
        }

        #endregion

        #region Movement

        private void HandleMovement()
        {
            if (isDashing)
            {
                return;
            }

            float moveSpeed = stats.GetStat(StatType.MoveSpeed);
            Vector2 velocity = moveInput.normalized * moveSpeed;

            rb.linearVelocity = velocity;

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

            Vector2 dashDir = moveInput.normalized;
            if (dashDir.sqrMagnitude < 0.1f)
            {
                dashDir = aimDirection;
            }

            bool wasInvincible = isInvincible;
            isInvincible = true;

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

        public void EquipWeapon(WeaponData weaponData)
        {
            Debug.Log($"PlayerController.EquipWeapon: Called with weapon: {weaponData?.weaponName}");

            if (currentWeapon != null)
            {
                Debug.Log("PlayerController.EquipWeapon: Destroying previous weapon");
                Destroy(currentWeapon.gameObject);
            }

            if (weaponData != null && weaponData.weaponPrefab != null)
            {
                Debug.Log($"PlayerController.EquipWeapon: Instantiating weapon prefab at weaponPivot");
                Debug.Log($"PlayerController.EquipWeapon: WeaponPivot = {weaponPivot?.name}, Position = {weaponPivot?.position}");

                GameObject weaponObj = GameObject.Instantiate(weaponData.weaponPrefab, weaponPivot);
                weaponObj.transform.localPosition = weaponData.weaponOffset;

                Debug.Log($"PlayerController.EquipWeapon: Weapon spawned: {weaponObj.name}");

                currentWeapon = weaponObj.GetComponent<WeaponBase>();

                if (currentWeapon != null)
                {
                    Debug.Log($"PlayerController.EquipWeapon: WeaponBase component found, initializing...");
                    currentWeapon.Initialize(this, stats);
                    Debug.Log("PlayerController.EquipWeapon: Weapon equipped successfully!");
                }
                else
                {
                    Debug.LogError($"PlayerController.EquipWeapon: No WeaponBase component on {weaponObj.name}!");
                }
            }
            else
            {
                if (weaponData == null)
                    Debug.LogError("PlayerController.EquipWeapon: WeaponData is null!");
                else if (weaponData.weaponPrefab == null)
                    Debug.LogError($"PlayerController.EquipWeapon: WeaponData '{weaponData.weaponName}' has no weaponPrefab assigned!");
            }
        }

        public WeaponBase GetCurrentWeapon()
        {
            return currentWeapon;
        }

        #endregion

        #region Damage System

        public void TakeDamage(float damage, bool isCritical = false, GameObject damageSource = null)
        {
            if (isDead || isInvincible) return;

            if (stats.RollDodge())
            {
                GameEvents.PlaySoundEffect("Dodge", transform.position);
                return;
            }

            float damageReduction = stats.CalculateDamageReduction();
            float finalDamage = damage * (1f - damageReduction);

            stats.ModifyHealth(-finalDamage);

            GameEvents.PlayerDamaged(finalDamage, damageSource);
            GameEvents.DamageDealt(gameObject, finalDamage, isCritical);

            StartCoroutine(InvincibilityFrames());

            if (stats.currentHealth <= 0 && !isDead)
            {
                Die();
            }

            StartCoroutine(DamageFlash());
        }

        public void Die()
        {
            if (isDead) return;

            isDead = true;
            rb.linearVelocity = Vector2.zero;

            GameEvents.PlayerDeath();

            if (animator != null)
            {
                animator.SetTrigger("Death");
            }

            if (currentWeapon != null)
            {
                currentWeapon.gameObject.SetActive(false);
            }

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
            if (animator == null)
            {
                Debug.LogWarning("PlayerController: Animator is NULL! Please assign an Animator component in the Inspector.");
                return;
            }

            // 8-directional animation handling
            Vector2 input = moveInput.normalized;
            bool isMoving = input.sqrMagnitude > 0.1f && !isDashing;

            // Set walking flag
            animator.SetBool("isWalking", isMoving);

            // Determine facing direction
            Direction dir = GetDirectionFromInput(input);
            if (isMoving)
            {
                lastDirection = dir;
                Debug.Log($"Moving {dir} - isWalking: {isMoving}, MoveInput: {moveInput}");
            }

            // Clear all direction bools
            ClearDirectionBools();

            // Set new direction
            SetDirectionBools(lastDirection, isMoving);
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

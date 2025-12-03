using UnityEngine;

namespace PixelGame
{
    /// <summary>
    /// Base class for all weapons in the game
    /// </summary>
    public abstract class WeaponBase : MonoBehaviour
    {
        [Header("Weapon Info")]
        public WeaponData weaponData;

        [Header("Visual")]
        [SerializeField] protected SpriteRenderer spriteRenderer;
        [SerializeField] protected Animator animator;

        // References
        protected PlayerController player;
        protected PlayerStats playerStats;
        protected Transform firePoint;

        // Attack timing
        protected float lastAttackTime;
        protected bool isAttacking = false;

        /// <summary>
        /// Initialize weapon with player reference
        /// </summary>
        public virtual void Initialize(PlayerController playerController, PlayerStats stats)
        {
            player = playerController;
            playerStats = stats;

            // Create fire point if not exists
            if (firePoint == null)
            {
                GameObject fp = new GameObject("FirePoint");
                fp.transform.SetParent(transform);
                fp.transform.localPosition = weaponData != null ? weaponData.firePointOffset : Vector3.right;
                firePoint = fp.transform;
            }

            OnInitialize();
        }

        /// <summary>
        /// Override for weapon-specific initialization
        /// </summary>
        protected virtual void OnInitialize() { }

        /// <summary>
        /// Attempt to attack
        /// </summary>
        public void Attack(Vector2 direction)
        {
            if (!CanAttack()) return;

            lastAttackTime = Time.time;
            isAttacking = true;

            PerformAttack(direction);

            // Play attack animation
            if (animator != null)
            {
                animator.SetTrigger("Attack");
            }

            // Play attack sound
            if (weaponData != null && !string.IsNullOrEmpty(weaponData.attackSoundName))
            {
                GameEvents.PlaySoundEffect(weaponData.attackSoundName, transform.position);
            }
        }

        /// <summary>
        /// Perform the actual attack logic (override in derived classes)
        /// </summary>
        protected abstract void PerformAttack(Vector2 direction);

        /// <summary>
        /// Check if weapon can attack
        /// </summary>
        protected virtual bool CanAttack()
        {
            if (weaponData == null) return false;

            float attackSpeed = GetAttackSpeed();
            float cooldown = 1f / attackSpeed;

            return Time.time >= lastAttackTime + cooldown;
        }

        /// <summary>
        /// Get effective attack speed
        /// </summary>
        protected float GetAttackSpeed()
        {
            if (weaponData == null || playerStats == null) return 1f;

            float baseSpeed = weaponData.attackSpeed;
            float statSpeed = playerStats.GetStat(StatType.AttackSpeed);

            return baseSpeed * statSpeed;
        }

        /// <summary>
        /// Calculate final weapon damage
        /// </summary>
        protected DamageInfo CalculateDamage()
        {
            if (weaponData == null || playerStats == null)
            {
                return new DamageInfo { damage = 10f, isCritical = false };
            }

            // Base damage from weapon
            float baseDamage = weaponData.baseDamage;

            // Apply weapon type scaling
            switch (weaponData.weaponType)
            {
                case WeaponType.Melee:
                    baseDamage += playerStats.GetStat(StatType.PhysicalDamage);
                    break;
                case WeaponType.Ranged:
                    baseDamage += playerStats.GetStat(StatType.PhysicalDamage) * 0.5f;
                    break;
                case WeaponType.Magic:
                    baseDamage += playerStats.GetStat(StatType.MagicDamage);
                    break;
            }

            // Apply general damage stat
            float damageMultiplier = 1f + playerStats.GetStat(StatType.Damage) / 100f;
            float finalDamage = baseDamage * damageMultiplier;

            // Check for critical hit
            bool isCritical = playerStats.RollCritical();
            if (isCritical)
            {
                float critMultiplier = playerStats.GetStat(StatType.CritMultiplier);
                finalDamage *= critMultiplier;
            }

            return new DamageInfo
            {
                damage = finalDamage,
                isCritical = isCritical,
                damageType = weaponData.weaponType
            };
        }

        /// <summary>
        /// Spawn a projectile
        /// </summary>
        protected Projectile SpawnProjectile(Vector2 direction, Vector3 spawnPosition)
        {
            if (weaponData.projectilePrefab == null) return null;

            GameObject projObj = Instantiate(weaponData.projectilePrefab, spawnPosition, Quaternion.identity);
            Projectile projectile = projObj.GetComponent<Projectile>();

            if (projectile != null)
            {
                DamageInfo damageInfo = CalculateDamage();

                ProjectileData projData = new ProjectileData
                {
                    damage = damageInfo.damage,
                    speed = weaponData.projectileSpeed * (1f + playerStats.GetStat(StatType.ProjectileSpeed) / 100f),
                    lifetime = weaponData.projectileLifetime,
                    pierceCount = weaponData.pierceCount + Mathf.RoundToInt(playerStats.GetStat(StatType.PierceCount)),
                    size = 1f + playerStats.GetStat(StatType.ProjectileSize) / 100f,
                    isCritical = damageInfo.isCritical,
                    isPlayerProjectile = true
                };

                projectile.Initialize(projData, direction);

                GameEvents.ProjectileSpawned(projectile, true);
            }

            return projectile;
        }

        /// <summary>
        /// Apply visual feedback for attack
        /// </summary>
        protected void ApplyAttackFeedback()
        {
            // Screen shake, particles, etc.
            // To be implemented with VFX system
        }

        #region Unity Lifecycle

        protected virtual void Update()
        {
            if (isAttacking && animator != null)
            {
                // Reset attacking flag when animation completes
                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                if (stateInfo.normalizedTime >= 1f && !stateInfo.IsName("Attack"))
                {
                    isAttacking = false;
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// Weapon type enumeration
    /// </summary>
    public enum WeaponType
    {
        Melee,
        Ranged,
        Magic
    }

    /// <summary>
    /// Damage information structure
    /// </summary>
    public struct DamageInfo
    {
        public float damage;
        public bool isCritical;
        public WeaponType damageType;
    }
}

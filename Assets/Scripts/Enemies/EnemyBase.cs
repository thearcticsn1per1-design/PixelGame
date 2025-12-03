using UnityEngine;
using System.Collections;

namespace PixelGame
{
    /// <summary>
    /// Base class for all enemies in the game
    /// Handles health, AI, movement, and loot drops
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public abstract class EnemyBase : MonoBehaviour, IDamageable
    {
        [Header("Enemy Stats")]
        [SerializeField] protected float maxHealth = 50f;
        [SerializeField] protected float moveSpeed = 3f;
        [SerializeField] protected float damage = 10f;
        [SerializeField] protected int xpValue = 10;
        [SerializeField] protected int goldValue = 5;

        [Header("AI Settings")]
        [SerializeField] protected float detectionRange = 10f;
        [SerializeField] protected float attackRange = 1.5f;
        [SerializeField] protected float attackCooldown = 1f;

        [Header("Knockback")]
        [SerializeField] protected float knockbackResistance = 0f; // 0-1, higher = more resistant
        [SerializeField] protected float knockbackDuration = 0.2f;

        [Header("Drops")]
        [SerializeField] protected LootTable lootTable;

        [Header("Visual")]
        [SerializeField] protected SpriteRenderer spriteRenderer;
        [SerializeField] protected Animator animator;

        // Components
        protected Rigidbody2D rb;
        protected Transform player;

        // State
        protected float currentHealth;
        protected EnemyState currentState = EnemyState.Idle;
        protected bool isDead = false;
        protected bool isKnockedBack = false;

        // Combat
        protected float lastAttackTime;

        // Invincibility (for damage flashing)
        private bool isFlashing = false;
        private Color originalColor;

        protected enum EnemyState
        {
            Idle,
            Patrolling,
            Chasing,
            Attacking,
            Fleeing,
            Stunned,
            Dead
        }

        #region Unity Lifecycle

        protected virtual void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            currentHealth = maxHealth;

            if (spriteRenderer != null)
            {
                originalColor = spriteRenderer.color;
            }
        }

        protected virtual void Start()
        {
            FindPlayer();
        }

        protected virtual void Update()
        {
            if (isDead || isKnockedBack) return;

            UpdateAI();
            UpdateAnimations();
        }

        protected virtual void FixedUpdate()
        {
            if (isDead || isKnockedBack) return;

            UpdateMovement();
        }

        #endregion

        #region AI

        protected void FindPlayer()
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }

        protected abstract void UpdateAI();

        protected virtual void UpdateMovement()
        {
            // Override in derived classes for specific movement behaviors
        }

        protected float GetDistanceToPlayer()
        {
            if (player == null)
            {
                FindPlayer();
                return float.MaxValue;
            }

            return Vector2.Distance(transform.position, player.position);
        }

        protected Vector2 GetDirectionToPlayer()
        {
            if (player == null) return Vector2.zero;

            return (player.position - transform.position).normalized;
        }

        protected bool IsPlayerInRange(float range)
        {
            return GetDistanceToPlayer() <= range;
        }

        protected void MoveTowardsPlayer(float speed)
        {
            Vector2 direction = GetDirectionToPlayer();
            rb.velocity = direction * speed;

            // Flip sprite based on movement direction
            if (spriteRenderer != null && direction.x != 0)
            {
                spriteRenderer.flipX = direction.x < 0;
            }
        }

        protected void MoveAwayFromPlayer(float speed)
        {
            Vector2 direction = -GetDirectionToPlayer();
            rb.velocity = direction * speed;

            // Flip sprite
            if (spriteRenderer != null && direction.x != 0)
            {
                spriteRenderer.flipX = direction.x < 0;
            }
        }

        protected bool CanAttack()
        {
            return Time.time >= lastAttackTime + attackCooldown;
        }

        #endregion

        #region Damage System

        public virtual void TakeDamage(float damage, bool isCritical = false, GameObject damageSource = null)
        {
            if (isDead) return;

            currentHealth -= damage;

            // Visual feedback
            StartCoroutine(DamageFlash());

            // Knockback
            if (damageSource != null && knockbackResistance < 1f)
            {
                Vector2 knockbackDir = (transform.position - damageSource.transform.position).normalized;
                ApplyKnockback(knockbackDir, damage * (1f - knockbackResistance));
            }

            // Trigger events
            GameEvents.DamageDealt(gameObject, damage, isCritical);

            // Check for death
            if (currentHealth <= 0)
            {
                Die();
            }
        }

        protected virtual void ApplyKnockback(Vector2 direction, float force)
        {
            if (isKnockedBack) return;

            StartCoroutine(KnockbackCoroutine(direction, force));
        }

        private IEnumerator KnockbackCoroutine(Vector2 direction, float force)
        {
            isKnockedBack = true;
            rb.velocity = Vector2.zero;
            rb.AddForce(direction * force, ForceMode2D.Impulse);

            yield return new WaitForSeconds(knockbackDuration);

            isKnockedBack = false;
            rb.velocity = Vector2.zero;
        }

        public virtual void Die()
        {
            if (isDead) return;

            isDead = true;
            currentState = EnemyState.Dead;
            rb.velocity = Vector2.zero;

            // Disable collider
            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;

            // Award XP
            if (player != null)
            {
                PlayerStats stats = player.GetComponent<PlayerStats>();
                if (stats != null)
                {
                    stats.AddXP(xpValue);
                }
            }

            // Drop loot
            DropLoot();

            // Trigger events
            GameEvents.EnemyKilled(this);

            // Death animation and cleanup
            StartCoroutine(DeathSequence());
        }

        private IEnumerator DeathSequence()
        {
            // Play death animation
            if (animator != null)
            {
                animator.SetTrigger("Death");
                yield return new WaitForSeconds(0.5f);
            }

            // Fade out
            if (spriteRenderer != null)
            {
                float fadeTime = 0.3f;
                float elapsed = 0f;

                while (elapsed < fadeTime)
                {
                    Color color = spriteRenderer.color;
                    color.a = Mathf.Lerp(1f, 0f, elapsed / fadeTime);
                    spriteRenderer.color = color;

                    elapsed += Time.deltaTime;
                    yield return null;
                }
            }

            // Destroy or return to pool
            Destroy(gameObject);
        }

        public bool IsAlive()
        {
            return !isDead;
        }

        public float GetHealthPercentage()
        {
            return maxHealth > 0 ? currentHealth / maxHealth : 0;
        }

        #endregion

        #region Loot System

        protected virtual void DropLoot()
        {
            // Drop gold
            if (goldValue > 0)
            {
                // Would spawn gold pickup here
                // For now, directly add to player
                GameManager.Instance.AddGold(goldValue);
            }

            // Drop items from loot table
            if (lootTable != null)
            {
                lootTable.RollLoot(transform.position);
            }
        }

        #endregion

        #region Visual Effects

        private IEnumerator DamageFlash()
        {
            if (spriteRenderer == null || isFlashing) yield break;

            isFlashing = true;

            // Flash red
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);

            // Return to original color
            spriteRenderer.color = originalColor;

            isFlashing = false;
        }

        #endregion

        #region Animations

        protected virtual void UpdateAnimations()
        {
            if (animator == null) return;

            animator.SetFloat("Speed", rb.velocity.magnitude);
            animator.SetBool("IsAttacking", currentState == EnemyState.Attacking);
        }

        #endregion

        #region Gizmos

        protected virtual void OnDrawGizmosSelected()
        {
            // Detection range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);

            // Attack range
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }

        #endregion
    }
}

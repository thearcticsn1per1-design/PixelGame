using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelGame
{
    /// <summary>
    /// Projectile class for ranged and magic attacks
    /// Supports piercing, homing, chaining, and explosions
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class Projectile : MonoBehaviour
    {
        [Header("Visual")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private TrailRenderer trailRenderer;
        [SerializeField] private GameObject hitEffectPrefab;
        [SerializeField] private GameObject explosionEffectPrefab;

        // Components
        private Rigidbody2D rb;

        // Projectile data
        private ProjectileData data;
        private Vector2 direction;

        // State
        private float lifetime;
        private int remainingPierces;
        private HashSet<Collider2D> hitEnemies = new HashSet<Collider2D>();
        private bool hasExploded = false;

        // Homing
        private bool isHoming = false;
        private float homingStrength = 0f;
        private Transform target;

        // Chaining
        private bool canChain = false;
        private int remainingChains = 0;
        private float chainRange = 5f;
        private LayerMask enemyLayer;

        // Explosion
        private bool hasExplosion = false;
        private float explosionRadius = 0f;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0; // No gravity for projectiles

            enemyLayer = LayerMask.GetMask("Enemy");
        }

        /// <summary>
        /// Initialize projectile with data
        /// </summary>
        public void Initialize(ProjectileData projData, Vector2 dir)
        {
            data = projData;
            direction = dir.normalized;
            lifetime = 0f;
            remainingPierces = data.pierceCount;
            hasExploded = false;
            hitEnemies.Clear();

            // Set velocity
            rb.linearVelocity = direction * data.speed;

            // Rotate sprite to face direction
            if (spriteRenderer != null)
            {
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0, 0, angle);
            }

            // Apply size
            transform.localScale = Vector3.one * data.size;

            // Enable/disable trail
            if (trailRenderer != null)
            {
                trailRenderer.enabled = data.isPlayerProjectile;
                trailRenderer.Clear();
            }
        }

        private void Update()
        {
            // Lifetime check
            lifetime += Time.deltaTime;
            if (lifetime >= data.lifetime)
            {
                DestroyProjectile();
                return;
            }

            // Homing behavior
            if (isHoming)
            {
                UpdateHoming();
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            HandleCollision(collision);
        }

        private void HandleCollision(Collider2D collision)
        {
            // Check if hit wall
            if (collision.CompareTag("Wall"))
            {
                OnHitWall();
                return;
            }

            // Check if hit appropriate target
            bool isValidTarget = false;

            if (data.isPlayerProjectile && collision.CompareTag("Enemy"))
            {
                isValidTarget = true;
            }
            else if (!data.isPlayerProjectile && collision.CompareTag("Player"))
            {
                isValidTarget = true;
            }

            if (isValidTarget && !hitEnemies.Contains(collision))
            {
                OnHitTarget(collision);
            }
        }

        private void OnHitTarget(Collider2D target)
        {
            hitEnemies.Add(target);

            // Deal damage
            IDamageable damageable = target.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(data.damage, data.isCritical, gameObject);
            }

            // Spawn hit effect
            SpawnHitEffect(transform.position);

            // Check for chain
            if (canChain && remainingChains > 0)
            {
                ChainToNearbyEnemy(target);
                remainingChains--;
            }

            // Check for explosion
            if (hasExplosion && !hasExploded)
            {
                Explode();
                hasExploded = true;
            }

            // Check for piercing
            if (remainingPierces > 0)
            {
                remainingPierces--;
            }
            else
            {
                DestroyProjectile();
            }
        }

        private void OnHitWall()
        {
            // Spawn hit effect
            SpawnHitEffect(transform.position);

            // Explode if has explosion
            if (hasExplosion && !hasExploded)
            {
                Explode();
            }

            DestroyProjectile();
        }

        #region Special Effects

        /// <summary>
        /// Enable homing behavior
        /// </summary>
        public void EnableHoming(float strength)
        {
            isHoming = true;
            homingStrength = strength;
            FindNearestTarget();
        }

        private void UpdateHoming()
        {
            // Find target if lost
            if (target == null || !target.gameObject.activeInHierarchy)
            {
                FindNearestTarget();
            }

            // Steer toward target
            if (target != null)
            {
                Vector2 desiredDirection = (target.position - transform.position).normalized;
                Vector2 newDirection = Vector2.Lerp(direction, desiredDirection, homingStrength * Time.deltaTime);
                newDirection.Normalize();

                direction = newDirection;
                rb.linearVelocity = direction * data.speed;

                // Update rotation
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0, 0, angle);
            }
        }

        private void FindNearestTarget()
        {
            Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, 15f, enemyLayer);

            float nearestDistance = float.MaxValue;
            Transform nearestEnemy = null;

            foreach (var enemy in enemies)
            {
                if (hitEnemies.Contains(enemy)) continue;

                float distance = Vector2.Distance(transform.position, enemy.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestEnemy = enemy.transform;
                }
            }

            target = nearestEnemy;
        }

        /// <summary>
        /// Enable chain lightning effect
        /// </summary>
        public void EnableChaining(int chainCount, float range)
        {
            canChain = true;
            remainingChains = chainCount;
            chainRange = range;
        }

        private void ChainToNearbyEnemy(Collider2D previousTarget)
        {
            Collider2D[] enemies = Physics2D.OverlapCircleAll(previousTarget.transform.position, chainRange, enemyLayer);

            Transform closestEnemy = null;
            float closestDistance = float.MaxValue;

            foreach (var enemy in enemies)
            {
                if (enemy == previousTarget || hitEnemies.Contains(enemy)) continue;

                float distance = Vector2.Distance(previousTarget.transform.position, enemy.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = enemy.transform;
                }
            }

            if (closestEnemy != null)
            {
                // Spawn chain lightning visual
                SpawnChainEffect(previousTarget.transform.position, closestEnemy.position);

                // Deal damage to chained enemy
                IDamageable damageable = closestEnemy.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(data.damage * 0.7f, false, gameObject); // Reduced damage
                }

                hitEnemies.Add(closestEnemy.GetComponent<Collider2D>());
            }
        }

        /// <summary>
        /// Enable explosion on impact
        /// </summary>
        public void EnableExplosion(float radius)
        {
            hasExplosion = true;
            explosionRadius = radius;
        }

        private void Explode()
        {
            // Spawn explosion effect
            if (explosionEffectPrefab != null)
            {
                GameObject explosion = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
                Destroy(explosion, 2f);
            }

            // Damage all enemies in radius
            Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, explosionRadius, enemyLayer);

            foreach (var enemy in enemies)
            {
                IDamageable damageable = enemy.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    float explosionDamage = data.damage * 0.8f; // 80% of projectile damage
                    damageable.TakeDamage(explosionDamage, false, gameObject);
                }
            }

            GameEvents.PlaySoundEffect("Explosion", transform.position);
        }

        #endregion

        #region Visual Effects

        private void SpawnHitEffect(Vector3 position)
        {
            if (hitEffectPrefab != null)
            {
                GameObject effect = Instantiate(hitEffectPrefab, position, Quaternion.identity);
                Destroy(effect, 1f);
            }

            GameEvents.PlaySoundEffect("ProjectileHit", position);
        }

        private void SpawnChainEffect(Vector3 start, Vector3 end)
        {
            // Create a line renderer for chain lightning
            GameObject chainObj = new GameObject("ChainLightning");
            chainObj.transform.position = start;

            LineRenderer line = chainObj.AddComponent<LineRenderer>();
            line.startWidth = 0.1f;
            line.endWidth = 0.1f;
            line.positionCount = 2;
            line.SetPosition(0, start);
            line.SetPosition(1, end);

            // Set material/color (would need a material assigned)
            line.startColor = Color.cyan;
            line.endColor = Color.white;

            Destroy(chainObj, 0.2f);

            GameEvents.PlaySoundEffect("ChainLightning", start);
        }

        #endregion

        private void DestroyProjectile()
        {
            // Return to pool instead of destroying (if pooling is active)
            if (ObjectPool.Instance != null)
            {
                ObjectPool.Instance.ReturnToPool(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (hasExplosion)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, explosionRadius);
            }

            if (canChain)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(transform.position, chainRange);
            }
        }
    }

    /// <summary>
    /// Projectile configuration data
    /// </summary>
    [System.Serializable]
    public struct ProjectileData
    {
        public float damage;
        public float speed;
        public float lifetime;
        public int pierceCount;
        public float size;
        public bool isCritical;
        public bool isPlayerProjectile;
    }
}

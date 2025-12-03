using UnityEngine;

namespace PixelGame
{
    /// <summary>
    /// Ranged enemy that shoots projectiles at the player
    /// Tries to maintain distance from player
    /// </summary>
    public class RangedEnemy : EnemyBase
    {
        [Header("Ranged Specific")]
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private Transform firePoint;
        [SerializeField] private float projectileSpeed = 8f;
        [SerializeField] private float projectileDamage = 15f;
        [SerializeField] private float optimalDistance = 6f; // Preferred distance from player
        [SerializeField] private float minDistance = 3f; // Too close, back away

        protected override void Awake()
        {
            base.Awake();

            if (firePoint == null)
            {
                GameObject fp = new GameObject("FirePoint");
                fp.transform.SetParent(transform);
                fp.transform.localPosition = Vector3.right * 0.5f;
                firePoint = fp.transform;
            }
        }

        protected override void UpdateAI()
        {
            float distanceToPlayer = GetDistanceToPlayer();

            switch (currentState)
            {
                case EnemyState.Idle:
                    if (IsPlayerInRange(detectionRange))
                    {
                        currentState = EnemyState.Chasing;
                    }
                    break;

                case EnemyState.Chasing:
                    if (!IsPlayerInRange(detectionRange))
                    {
                        currentState = EnemyState.Idle;
                    }
                    else if (distanceToPlayer <= attackRange)
                    {
                        currentState = EnemyState.Attacking;
                    }
                    else if (distanceToPlayer < minDistance)
                    {
                        currentState = EnemyState.Fleeing;
                    }
                    break;

                case EnemyState.Attacking:
                    if (distanceToPlayer > attackRange)
                    {
                        currentState = EnemyState.Chasing;
                    }
                    else if (distanceToPlayer < minDistance)
                    {
                        currentState = EnemyState.Fleeing;
                    }
                    else if (CanAttack())
                    {
                        ShootAtPlayer();
                    }
                    break;

                case EnemyState.Fleeing:
                    if (distanceToPlayer >= optimalDistance)
                    {
                        currentState = EnemyState.Attacking;
                    }
                    break;
            }

            // Always face player
            FacePlayer();
        }

        protected override void UpdateMovement()
        {
            float distanceToPlayer = GetDistanceToPlayer();

            switch (currentState)
            {
                case EnemyState.Idle:
                    rb.linearVelocity = Vector2.zero;
                    break;

                case EnemyState.Chasing:
                    // Move toward player if too far
                    if (distanceToPlayer > optimalDistance)
                    {
                        MoveTowardsPlayer(moveSpeed);
                    }
                    else
                    {
                        rb.linearVelocity = Vector2.zero;
                    }
                    break;

                case EnemyState.Attacking:
                    // Strafe or stay still while attacking
                    rb.linearVelocity = rb.linearVelocity * 0.3f;
                    break;

                case EnemyState.Fleeing:
                    // Back away from player
                    MoveAwayFromPlayer(moveSpeed);
                    break;
            }
        }

        private void ShootAtPlayer()
        {
            if (projectilePrefab == null || player == null) return;

            lastAttackTime = Time.time;

            Vector2 direction = GetDirectionToPlayer();

            // Spawn projectile
            GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
            Projectile projectile = proj.GetComponent<Projectile>();

            if (projectile != null)
            {
                ProjectileData data = new ProjectileData
                {
                    damage = projectileDamage,
                    speed = projectileSpeed,
                    lifetime = 5f,
                    pierceCount = 0,
                    size = 1f,
                    isCritical = false,
                    isPlayerProjectile = false
                };

                projectile.Initialize(data, direction);
            }

            // Play shoot animation
            if (animator != null)
            {
                animator.SetTrigger("Shoot");
            }
        }

        private void FacePlayer()
        {
            if (player == null || spriteRenderer == null) return;

            Vector2 direction = GetDirectionToPlayer();
            spriteRenderer.flipX = direction.x < 0;
        }
    }
}

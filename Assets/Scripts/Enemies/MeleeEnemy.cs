using UnityEngine;

namespace PixelGame
{
    /// <summary>
    /// Melee enemy that chases and attacks player in close range
    /// </summary>
    public class MeleeEnemy : EnemyBase
    {
        [Header("Melee Specific")]
        [SerializeField] private float lungeSpeed = 8f;
        [SerializeField] private float lungeDuration = 0.3f;
        [SerializeField] private bool canLunge = true;

        private bool isLunging = false;
        private float lungeEndTime;

        protected override void UpdateAI()
        {
            float distanceToPlayer = GetDistanceToPlayer();

            // State machine
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
                        rb.velocity = Vector2.zero;
                    }
                    else if (IsPlayerInRange(attackRange))
                    {
                        currentState = EnemyState.Attacking;
                    }
                    break;

                case EnemyState.Attacking:
                    if (!IsPlayerInRange(attackRange * 1.5f))
                    {
                        currentState = EnemyState.Chasing;
                    }
                    else if (CanAttack())
                    {
                        PerformAttack();
                    }
                    break;
            }

            // Handle lunging
            if (isLunging)
            {
                if (Time.time >= lungeEndTime)
                {
                    isLunging = false;
                    rb.velocity = Vector2.zero;
                }
            }
        }

        protected override void UpdateMovement()
        {
            if (isLunging) return; // Don't update movement during lunge

            switch (currentState)
            {
                case EnemyState.Idle:
                    rb.velocity = Vector2.zero;
                    break;

                case EnemyState.Chasing:
                    MoveTowardsPlayer(moveSpeed);
                    break;

                case EnemyState.Attacking:
                    // Slow down when attacking
                    rb.velocity = rb.velocity * 0.5f;
                    break;
            }
        }

        private void PerformAttack()
        {
            lastAttackTime = Time.time;

            if (canLunge)
            {
                // Lunge toward player
                Vector2 lungeDirection = GetDirectionToPlayer();
                rb.velocity = lungeDirection * lungeSpeed;
                isLunging = true;
                lungeEndTime = Time.time + lungeDuration;
            }

            // Check if hit player
            if (IsPlayerInRange(attackRange))
            {
                DamagePlayer();
            }
        }

        private void DamagePlayer()
        {
            if (player == null) return;

            IDamageable damageable = player.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage, false, gameObject);
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Player") && isLunging)
            {
                DamagePlayer();
            }
        }
    }
}

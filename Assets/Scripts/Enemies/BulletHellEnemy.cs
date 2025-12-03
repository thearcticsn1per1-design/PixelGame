using UnityEngine;
using System.Collections;

namespace PixelGame
{
    /// <summary>
    /// Bullet-hell enemy that fires complex projectile patterns
    /// </summary>
    public class BulletHellEnemy : EnemyBase
    {
        [Header("Bullet Hell Specific")]
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private BulletPattern bulletPattern;
        [SerializeField] private float stationaryDistance = 7f; // Stay at this distance
        [SerializeField] private float patternCooldown = 3f;

        private float lastPatternTime;
        private bool isPerformingPattern = false;

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
                    else if (Mathf.Abs(distanceToPlayer - stationaryDistance) < 1f)
                    {
                        currentState = EnemyState.Attacking;
                    }
                    break;

                case EnemyState.Attacking:
                    if (!IsPlayerInRange(detectionRange))
                    {
                        currentState = EnemyState.Idle;
                    }
                    else if (!isPerformingPattern && Time.time >= lastPatternTime + patternCooldown)
                    {
                        StartCoroutine(ExecuteBulletPattern());
                    }
                    break;
            }
        }

        protected override void UpdateMovement()
        {
            float distanceToPlayer = GetDistanceToPlayer();

            switch (currentState)
            {
                case EnemyState.Idle:
                    rb.velocity = Vector2.zero;
                    break;

                case EnemyState.Chasing:
                    // Move to optimal distance
                    if (distanceToPlayer > stationaryDistance + 1f)
                    {
                        MoveTowardsPlayer(moveSpeed);
                    }
                    else if (distanceToPlayer < stationaryDistance - 1f)
                    {
                        MoveAwayFromPlayer(moveSpeed);
                    }
                    else
                    {
                        rb.velocity = Vector2.zero;
                    }
                    break;

                case EnemyState.Attacking:
                    // Stay relatively stationary while attacking
                    rb.velocity = Vector2.zero;
                    break;
            }
        }

        private IEnumerator ExecuteBulletPattern()
        {
            isPerformingPattern = true;
            lastPatternTime = Time.time;

            if (bulletPattern != null)
            {
                yield return StartCoroutine(bulletPattern.Execute(transform.position, damage, projectilePrefab));
            }

            isPerformingPattern = false;
        }
    }
}

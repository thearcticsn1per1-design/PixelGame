using UnityEngine;
using System.Collections;

namespace PixelGame
{
    /// <summary>
    /// Scriptable and component-based bullet pattern system
    /// Creates various bullet-hell attack patterns
    /// </summary>
    [System.Serializable]
    public class BulletPattern : MonoBehaviour
    {
        [Header("Pattern Settings")]
        [SerializeField] private PatternType patternType = PatternType.Circle;
        [SerializeField] private int projectileCount = 8;
        [SerializeField] private float projectileSpeed = 5f;
        [SerializeField] private float projectileLifetime = 5f;

        [Header("Pattern Specific")]
        [SerializeField] private float spreadAngle = 360f; // For circle/arc patterns
        [SerializeField] private float spiralRotationSpeed = 45f; // Degrees per wave
        [SerializeField] private int waveCount = 3; // For spiral/wave patterns
        [SerializeField] private float delayBetweenWaves = 0.2f;

        [Header("Advanced")]
        [SerializeField] private bool rotatePattern = false;
        [SerializeField] private float rotationSpeed = 30f; // Degrees per second
        [SerializeField] private AnimationCurve speedCurve = AnimationCurve.Linear(0, 1, 1, 1);

        private float currentRotation = 0f;

        public enum PatternType
        {
            Circle,        // 360-degree burst
            Arc,           // Spread in an arc
            Spiral,        // Rotating spiral
            Wave,          // Sequential waves
            Cross,         // Plus shape
            X,             // X shape
            Random,        // Random directions
            TargetedSpread // Aimed at player with spread
        }

        /// <summary>
        /// Execute the bullet pattern
        /// </summary>
        public IEnumerator Execute(Vector3 origin, float damage, GameObject projectilePrefab)
        {
            if (projectilePrefab == null)
            {
                Debug.LogWarning("BulletPattern: No projectile prefab assigned!");
                yield break;
            }

            switch (patternType)
            {
                case PatternType.Circle:
                    SpawnCirclePattern(origin, damage, projectilePrefab);
                    break;

                case PatternType.Arc:
                    SpawnArcPattern(origin, damage, projectilePrefab);
                    break;

                case PatternType.Spiral:
                    yield return SpawnSpiralPattern(origin, damage, projectilePrefab);
                    break;

                case PatternType.Wave:
                    yield return SpawnWavePattern(origin, damage, projectilePrefab);
                    break;

                case PatternType.Cross:
                    SpawnCrossPattern(origin, damage, projectilePrefab);
                    break;

                case PatternType.X:
                    SpawnXPattern(origin, damage, projectilePrefab);
                    break;

                case PatternType.Random:
                    SpawnRandomPattern(origin, damage, projectilePrefab);
                    break;

                case PatternType.TargetedSpread:
                    SpawnTargetedSpreadPattern(origin, damage, projectilePrefab);
                    break;
            }
        }

        #region Pattern Implementations

        private void SpawnCirclePattern(Vector3 origin, float damage, GameObject prefab)
        {
            float angleStep = 360f / projectileCount;

            for (int i = 0; i < projectileCount; i++)
            {
                float angle = (i * angleStep) + currentRotation;
                Vector2 direction = AngleToVector(angle);

                SpawnProjectile(origin, direction, damage, prefab);
            }

            UpdateRotation();
        }

        private void SpawnArcPattern(Vector3 origin, float damage, GameObject prefab)
        {
            float angleStep = spreadAngle / (projectileCount - 1);
            float startAngle = -spreadAngle / 2f + currentRotation;

            // Get direction to player for aiming
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            float baseAngle = 0f;

            if (player != null)
            {
                Vector2 toPlayer = (player.transform.position - origin).normalized;
                baseAngle = VectorToAngle(toPlayer);
            }

            for (int i = 0; i < projectileCount; i++)
            {
                float angle = baseAngle + startAngle + (i * angleStep);
                Vector2 direction = AngleToVector(angle);

                SpawnProjectile(origin, direction, damage, prefab);
            }

            UpdateRotation();
        }

        private IEnumerator SpawnSpiralPattern(Vector3 origin, float damage, GameObject prefab)
        {
            for (int wave = 0; wave < waveCount; wave++)
            {
                SpawnCirclePattern(origin, damage, prefab);
                currentRotation += spiralRotationSpeed;

                if (wave < waveCount - 1)
                {
                    yield return new WaitForSeconds(delayBetweenWaves);
                }
            }
        }

        private IEnumerator SpawnWavePattern(Vector3 origin, float damage, GameObject prefab)
        {
            for (int wave = 0; wave < waveCount; wave++)
            {
                SpawnArcPattern(origin, damage, prefab);

                if (wave < waveCount - 1)
                {
                    yield return new WaitForSeconds(delayBetweenWaves);
                }
            }
        }

        private void SpawnCrossPattern(Vector3 origin, float damage, GameObject prefab)
        {
            // Four cardinal directions
            Vector2[] directions = new Vector2[]
            {
                Vector2.up,
                Vector2.down,
                Vector2.left,
                Vector2.right
            };

            foreach (var direction in directions)
            {
                SpawnProjectile(origin, direction, damage, prefab);
            }
        }

        private void SpawnXPattern(Vector3 origin, float damage, GameObject prefab)
        {
            // Four diagonal directions
            Vector2[] directions = new Vector2[]
            {
                new Vector2(1, 1).normalized,
                new Vector2(1, -1).normalized,
                new Vector2(-1, 1).normalized,
                new Vector2(-1, -1).normalized
            };

            foreach (var direction in directions)
            {
                SpawnProjectile(origin, direction, damage, prefab);
            }
        }

        private void SpawnRandomPattern(Vector3 origin, float damage, GameObject prefab)
        {
            for (int i = 0; i < projectileCount; i++)
            {
                float angle = Random.Range(0f, 360f);
                Vector2 direction = AngleToVector(angle);

                SpawnProjectile(origin, direction, damage, prefab);
            }
        }

        private void SpawnTargetedSpreadPattern(Vector3 origin, float damage, GameObject prefab)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;

            Vector2 toPlayer = (player.transform.position - origin).normalized;
            float targetAngle = VectorToAngle(toPlayer);

            float angleStep = spreadAngle / (projectileCount - 1);
            float startAngle = targetAngle - (spreadAngle / 2f);

            for (int i = 0; i < projectileCount; i++)
            {
                float angle = startAngle + (i * angleStep);
                Vector2 direction = AngleToVector(angle);

                SpawnProjectile(origin, direction, damage, prefab);
            }
        }

        #endregion

        #region Helper Methods

        private void SpawnProjectile(Vector3 origin, Vector2 direction, float damage, GameObject prefab)
        {
            GameObject proj = Instantiate(prefab, origin, Quaternion.identity);
            Projectile projectile = proj.GetComponent<Projectile>();

            if (projectile != null)
            {
                ProjectileData data = new ProjectileData
                {
                    damage = damage,
                    speed = projectileSpeed,
                    lifetime = projectileLifetime,
                    pierceCount = 0,
                    size = 1f,
                    isCritical = false,
                    isPlayerProjectile = false
                };

                projectile.Initialize(data, direction);
            }
        }

        private Vector2 AngleToVector(float angleDegrees)
        {
            float radians = angleDegrees * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
        }

        private float VectorToAngle(Vector2 vector)
        {
            return Mathf.Atan2(vector.y, vector.x) * Mathf.Rad2Deg;
        }

        private void UpdateRotation()
        {
            if (rotatePattern)
            {
                currentRotation += rotationSpeed * Time.deltaTime;
                currentRotation %= 360f;
            }
        }

        #endregion

        #region Gizmos

        private void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying) return;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, projectileSpeed);
        }

        #endregion
    }
}

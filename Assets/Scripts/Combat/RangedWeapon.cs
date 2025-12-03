using UnityEngine;

namespace PixelGame
{
    /// <summary>
    /// Ranged weapon that fires projectiles
    /// </summary>
    public class RangedWeapon : WeaponBase
    {
        [Header("Ranged Settings")]
        [SerializeField] private int baseProjectileCount = 1;
        [SerializeField] private float spreadAngle = 0f; // Degrees of spread between projectiles

        protected override void PerformAttack(Vector2 direction)
        {
            if (weaponData == null || weaponData.projectilePrefab == null) return;

            // Calculate total projectile count
            int projectileCount = baseProjectileCount + Mathf.RoundToInt(playerStats.GetStat(StatType.ProjectileCount));

            if (projectileCount == 1)
            {
                // Single projectile
                SpawnProjectile(direction, firePoint.position);
            }
            else
            {
                // Multiple projectiles with spread
                FireMultipleProjectiles(direction, projectileCount);
            }

            // Apply screen shake or other feedback
            ApplyAttackFeedback();
        }

        private void FireMultipleProjectiles(Vector2 direction, int count)
        {
            float totalSpread = spreadAngle * (count - 1);
            float startAngle = -totalSpread / 2f;

            for (int i = 0; i < count; i++)
            {
                float angle = startAngle + (spreadAngle * i);
                Vector2 projDirection = RotateVector(direction, angle);

                SpawnProjectile(projDirection, firePoint.position);
            }
        }

        private Vector2 RotateVector(Vector2 vector, float degrees)
        {
            float radians = degrees * Mathf.Deg2Rad;
            float cos = Mathf.Cos(radians);
            float sin = Mathf.Sin(radians);

            return new Vector2(
                vector.x * cos - vector.y * sin,
                vector.x * sin + vector.y * cos
            );
        }
    }
}

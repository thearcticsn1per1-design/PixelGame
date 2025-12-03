using UnityEngine;
using System.Collections;

namespace PixelGame
{
    /// <summary>
    /// Magic weapon that fires spell projectiles with special effects
    /// </summary>
    public class MagicWeapon : WeaponBase
    {
        [Header("Magic Settings")]
        [SerializeField] private float manaCost = 10f;
        [SerializeField] private bool homingProjectiles = false;
        [SerializeField] private float homingStrength = 5f;
        [SerializeField] private int baseProjectileCount = 1;
        [SerializeField] private float spreadAngle = 15f;

        [Header("Special Effects")]
        [SerializeField] private bool chainLightning = false;
        [SerializeField] private bool explosiveImpact = false;
        [SerializeField] private float explosionRadius = 2f;

        protected override bool CanAttack()
        {
            if (!base.CanAttack()) return false;

            // Check mana cost
            if (manaCost > 0 && playerStats != null)
            {
                return playerStats.HasMana(manaCost);
            }

            return true;
        }

        protected override void PerformAttack(Vector2 direction)
        {
            if (weaponData == null || weaponData.projectilePrefab == null) return;

            // Consume mana
            if (manaCost > 0 && playerStats != null)
            {
                playerStats.SpendMana(manaCost);
            }

            // Calculate projectile count with magic scaling
            int projectileCount = baseProjectileCount + Mathf.RoundToInt(playerStats.GetStat(StatType.ProjectileCount));

            if (projectileCount == 1)
            {
                // Single magic projectile
                Projectile proj = SpawnProjectile(direction, firePoint.position);
                ApplyMagicEffects(proj);
            }
            else
            {
                // Multiple magic projectiles
                FireMultipleMagicProjectiles(direction, projectileCount);
            }

            // Spawn casting effect
            SpawnCastingEffect();
            ApplyAttackFeedback();
        }

        private void FireMultipleMagicProjectiles(Vector2 direction, int count)
        {
            float totalSpread = spreadAngle * (count - 1);
            float startAngle = -totalSpread / 2f;

            for (int i = 0; i < count; i++)
            {
                float angle = startAngle + (spreadAngle * i);
                Vector2 projDirection = RotateVector(direction, angle);

                Projectile proj = SpawnProjectile(projDirection, firePoint.position);
                ApplyMagicEffects(proj);
            }
        }

        private void ApplyMagicEffects(Projectile projectile)
        {
            if (projectile == null) return;

            // Apply homing
            if (homingProjectiles)
            {
                projectile.EnableHoming(homingStrength);
            }

            // Apply chain lightning
            if (chainLightning)
            {
                int chainCount = 3 + Mathf.RoundToInt(playerStats.GetStat(StatType.ChainCount));
                projectile.EnableChaining(chainCount, 5f);
            }

            // Apply explosive impact
            if (explosiveImpact)
            {
                float aoeBonus = playerStats.GetStat(StatType.AreaOfEffect) / 100f;
                projectile.EnableExplosion(explosionRadius * (1f + aoeBonus));
            }
        }

        private void SpawnCastingEffect()
        {
            if (weaponData != null && weaponData.castEffectPrefab != null)
            {
                GameObject effect = Instantiate(weaponData.castEffectPrefab, firePoint.position, Quaternion.identity);
                effect.transform.SetParent(transform);
                Destroy(effect, 1f);
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

using UnityEngine;

namespace PixelGame
{
    /// <summary>
    /// Interface for any entity that can take damage
    /// </summary>
    public interface IDamageable
    {
        /// <summary>
        /// Apply damage to this entity
        /// </summary>
        /// <param name="damage">Amount of damage to apply</param>
        /// <param name="isCritical">Whether this is a critical hit</param>
        /// <param name="damageSource">GameObject that dealt the damage</param>
        void TakeDamage(float damage, bool isCritical = false, GameObject damageSource = null);

        /// <summary>
        /// Kill this entity immediately
        /// </summary>
        void Die();

        /// <summary>
        /// Check if entity is currently alive
        /// </summary>
        bool IsAlive();

        /// <summary>
        /// Get current health percentage (0-1)
        /// </summary>
        float GetHealthPercentage();
    }
}

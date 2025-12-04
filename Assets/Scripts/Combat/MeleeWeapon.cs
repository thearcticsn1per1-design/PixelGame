using UnityEngine;
using System.Collections;

namespace PixelGame
{
    /// <summary>
    /// Melee weapon that hits enemies in a swing arc
    /// </summary>
    public class MeleeWeapon : WeaponBase
    {
        [Header("Melee Settings")]
        [SerializeField] private float swingArc = 90f;
        [SerializeField] private LayerMask enemyLayer;
        [SerializeField] private Transform hitboxCenter;

        private Collider2D weaponCollider;

        protected override void OnInitialize()
        {
            // Set up weapon collider if not exists
            weaponCollider = GetComponent<Collider2D>();
            if (weaponCollider == null)
            {
                CircleCollider2D collider = gameObject.AddComponent<CircleCollider2D>();
                collider.isTrigger = true;
                collider.radius = weaponData != null ? weaponData.range : 1f;
                weaponCollider = collider;
            }

            // Disable collider initially
            weaponCollider.enabled = false;

            if (hitboxCenter == null)
            {
                hitboxCenter = firePoint;
            }
        }

        protected override void PerformAttack(Vector2 direction)
        {
            StartCoroutine(MeleeAttackCoroutine(direction));
        }

        private IEnumerator MeleeAttackCoroutine(Vector2 direction)
        {
            Debug.Log("MeleeWeapon: Attack started!");

            // Enable hitbox for a short duration
            weaponCollider.enabled = true;

            // Detect enemies in range
            Vector3 attackPosition = hitboxCenter != null ? hitboxCenter.position : transform.position;
            float attackRange = weaponData != null ? weaponData.range : 1.5f;

            // Add AOE bonus
            if (playerStats != null)
            {
                attackRange *= (1f + playerStats.GetStat(StatType.AreaOfEffect) / 100f);
            }

            // ENHANCED DEBUGGING FOR LAYER DETECTION
            Debug.Log($"MeleeWeapon: LayerMask value = {enemyLayer.value}");
            Debug.Log($"MeleeWeapon: Attack position = {attackPosition}, Range = {attackRange}");

            // Test 1: Check with specified layer mask
            Collider2D[] hits = Physics2D.OverlapCircleAll(attackPosition, attackRange, enemyLayer);
            Debug.Log($"MeleeWeapon: Found {hits.Length} colliders with enemyLayer mask");

            // Test 2: Check what's actually there (any layer)
            Collider2D[] allHits = Physics2D.OverlapCircleAll(attackPosition, attackRange);
            Debug.Log($"MeleeWeapon: Found {allHits.Length} total colliders in range (all layers)");

            // Log details about ALL colliders found
            for (int i = 0; i < allHits.Length; i++)
            {
                int objLayer = allHits[i].gameObject.layer;
                string layerName = LayerMask.LayerToName(objLayer);
                Debug.Log($"  - Collider {i}: {allHits[i].name}, Layer: {layerName} ({objLayer})");
            }

            DamageInfo damageInfo = CalculateDamage();
            Debug.Log($"MeleeWeapon: Damage = {damageInfo.damage}, Crit = {damageInfo.isCritical}");

            foreach (var hit in hits)
            {
                // Check if enemy is within swing arc
                if (IsInSwingArc(hit.transform.position, direction))
                {
                    Debug.Log($"MeleeWeapon: Enemy {hit.name} is in swing arc!");
                    IDamageable damageable = hit.GetComponent<IDamageable>();
                    if (damageable != null && damageable.IsAlive())
                    {
                        Debug.Log($"MeleeWeapon: Dealing {damageInfo.damage} damage to {hit.name}");
                        damageable.TakeDamage(damageInfo.damage, damageInfo.isCritical, player.gameObject);

                        // Apply knockback
                        ApplyKnockback(hit.gameObject, direction);

                        // Spawn hit effect
                        SpawnHitEffect(hit.transform.position);
                    }
                    else
                    {
                        Debug.LogWarning($"MeleeWeapon: {hit.name} has no IDamageable component or is dead!");
                    }
                }
                else
                {
                    Debug.Log($"MeleeWeapon: Enemy {hit.name} is NOT in swing arc");
                }
            }

            // Wait for swing duration
            yield return new WaitForSeconds(0.2f);

            // Disable hitbox
            weaponCollider.enabled = false;
        }

        private bool IsInSwingArc(Vector3 targetPosition, Vector2 attackDirection)
        {
            if (attackDirection.sqrMagnitude < 0.01f) return true; // No direction = hit all

            Vector3 attackPos = hitboxCenter != null ? hitboxCenter.position : transform.position;
            Vector2 toTarget = (targetPosition - attackPos).normalized;

            float angle = Vector2.Angle(attackDirection, toTarget);
            return angle <= swingArc / 2f;
        }

        private void ApplyKnockback(GameObject target, Vector2 direction)
        {
            Rigidbody2D rb = target.GetComponent<Rigidbody2D>();
            if (rb != null && weaponData != null)
            {
                float knockbackForce = weaponData.knockbackForce;
                rb.AddForce(direction.normalized * knockbackForce, ForceMode2D.Impulse);
            }
        }

        private void SpawnHitEffect(Vector3 position)
        {
            if (weaponData != null && weaponData.hitEffectPrefab != null)
            {
                GameObject effect = Instantiate(weaponData.hitEffectPrefab, position, Quaternion.identity);
                Destroy(effect, 1f);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (weaponData == null || hitboxCenter == null) return;

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(hitboxCenter.position, weaponData.range);

            // Draw swing arc
            Vector3 forward = hitboxCenter.right;
            Vector3 arcStart = Quaternion.Euler(0, 0, -swingArc / 2f) * forward * weaponData.range;
            Vector3 arcEnd = Quaternion.Euler(0, 0, swingArc / 2f) * forward * weaponData.range;

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(hitboxCenter.position, hitboxCenter.position + arcStart);
            Gizmos.DrawLine(hitboxCenter.position, hitboxCenter.position + arcEnd);
        }
    }
}

using UnityEngine;

namespace PixelGame
{
    /// <summary>
    /// ScriptableObject containing weapon configuration data
    /// </summary>
    [CreateAssetMenu(fileName = "New Weapon", menuName = "PixelGame/Weapon Data")]
    public class WeaponData : ScriptableObject
    {
        [Header("Basic Info")]
        public string weaponName = "New Weapon";
        [TextArea(3, 5)]
        public string description = "Weapon description here...";
        public Sprite icon;
        public WeaponType weaponType = WeaponType.Melee;
        public WeaponRarity rarity = WeaponRarity.Common;

        [Header("Weapon Prefab")]
        public GameObject weaponPrefab;
        public Vector3 weaponOffset = Vector3.right * 0.5f;
        public Vector3 firePointOffset = Vector3.right;

        [Header("Stats")]
        public float baseDamage = 10f;
        public float attackSpeed = 1f; // Attacks per second
        public float range = 1.5f;

        [Header("Projectile (for Ranged/Magic)")]
        public GameObject projectilePrefab;
        public float projectileSpeed = 10f;
        public float projectileLifetime = 3f;
        public int pierceCount = 0;

        [Header("Special Properties")]
        public float knockbackForce = 5f;
        public float critChanceBonus = 0f;
        public float critDamageBonus = 0f;

        [Header("Effects")]
        public GameObject hitEffectPrefab;
        public GameObject castEffectPrefab;
        public string attackSoundName = "WeaponSwing";

        [Header("Requirements")]
        public int levelRequirement = 1;
        public bool isUnlocked = true;
    }

    public enum WeaponRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }
}

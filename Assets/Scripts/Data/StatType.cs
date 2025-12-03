namespace PixelGame
{
    /// <summary>
    /// All possible character stat types in the game
    /// </summary>
    public enum StatType
    {
        // Health and Survivability
        MaxHealth,
        HealthRegen,
        Armor,
        DodgeChance,

        // Damage
        Damage,
        PhysicalDamage,
        MagicDamage,
        CritChance,
        CritMultiplier,

        // Attack Speed and Cooldowns
        AttackSpeed,
        CooldownReduction,

        // Movement
        MoveSpeed,
        DashCooldown,
        DashDistance,

        // Resources
        MaxMana,
        ManaRegen,

        // Special
        Lifesteal,
        AreaOfEffect,
        ProjectileSpeed,
        ProjectileSize,
        ProjectileCount,
        PierceCount,
        ChainCount,

        // Pickup and Gold
        PickupRange,
        GoldFind,
        ExperienceGain,

        // Damage Reduction
        DamageReduction,
        PhysicalResistance,
        MagicResistance,

        // Status Effect
        BurnChance,
        FreezeChance,
        PoisonChance,
        StunChance,

        // Other
        Luck,
        Thorns
    }
}

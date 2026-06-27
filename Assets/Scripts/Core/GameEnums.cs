namespace SurvivalMoba.Core
{
    /// <summary>
    /// High-level game flow states driven by <see cref="GameManager"/>.
    /// </summary>
    public enum GameState
    {
        Loading,
        Playing,
        Paused,
        LevelUp,
        Victory,
        Defeat
    }

    /// <summary>
    /// Logical category of an enemy. Used by spawning, scoring and boss handling.
    /// </summary>
    public enum EnemyType
    {
        Grunt,
        Runner,
        Brute,
        Boss
    }

    /// <summary>
    /// How an ability is aimed / targeted when cast. Matches the plan's targeting list.
    /// </summary>
    public enum TargetingType
    {
        InstantCast,
        LineSkillshot,
        ConeSkillshot,
        AreaPlacement,
        Dash,
        SelfBuff,
        TargetedAutoAim
    }

    /// <summary>
    /// What an ability actually does when it resolves.
    /// </summary>
    public enum AbilityEffectType
    {
        Projectile,
        PiercingLine,
        AreaOverTime,
        Dash,
        MeteorStorm,
        SelfBuff
    }

    /// <summary>
    /// Upgrade effect categories. <see cref="UpgradeManager"/> maps each to a stat change.
    /// </summary>
    public enum UpgradeEffectType
    {
        AbilityDamage,
        AttackSpeed,
        MoveSpeed,
        MaxHealth,
        CooldownReduction,
        ExtraProjectile,
        ProjectilePierce,
        FrostRadius,
        DashCooldown,
        UltimateCooldown
    }

    /// <summary>
    /// Rarity tiers, used for upgrade weighting and UI colouring.
    /// </summary>
    public enum Rarity
    {
        Common,
        Rare,
        Epic,
        Legendary
    }

    /// <summary>
    /// Which faction a combatant belongs to. Keeps projectiles from hitting their owner's team.
    /// </summary>
    public enum Faction
    {
        Player,
        Enemy
    }
}

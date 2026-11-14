namespace Rebellion.Tactical.Data
{
    /// <summary>
    /// Weapon types available in tactical combat.
    /// </summary>
    public enum TacticalWeaponType
    {
        LaserCannon,
        IonCannon,
        Torpedo,
    }

    /// <summary>
    /// Side designation for ships in a tactical battle.
    /// </summary>
    public enum TacticalShipSide
    {
        Attacker,
        Defender,
    }

    /// <summary>
    /// Operational status of a ship in tactical combat.
    /// </summary>
    public enum TacticalShipStatus
    {
        Active,
        Disabled,
        Destroyed,
    }

    /// <summary>
    /// Battle speed settings for tactical combat.
    /// </summary>
    public enum TacticalBattleSpeed
    {
        RealTime,
        Fast,
        VeryFast,
        Paused,
    }

    /// <summary>
    /// Outcome of a tactical battle.
    /// </summary>
    public enum TacticalBattleOutcome
    {
        Victory,
        Defeat,
        Withdrawal,
        Stalemate,
    }

    /// <summary>
    /// Types of ship subsystems that can be individually damaged.
    /// </summary>
    public enum TacticalSubsystemType
    {
        /// <summary>
        /// Damaged first; reduces shield capacity/regen.
        /// </summary>
        ShieldGenerator,

        /// <summary>
        /// Reduces speed when damaged.
        /// </summary>
        SublightEngines,

        /// <summary>
        /// Increases recharge time when damaged.
        /// </summary>
        Weapons,

        /// <summary>
        /// Required for hyperspace escape.
        /// </summary>
        Hyperdrive,

        /// <summary>
        /// Reduces target agility when locked (certain ships).
        /// </summary>
        TractorBeam,

        /// <summary>
        /// Prevents enemy hyperspace escape (certain ships).
        /// </summary>
        GravityWellProjector,
    }

    /// <summary>
    /// Operational status of a subsystem.
    /// </summary>
    public enum TacticalSubsystemStatus
    {
        /// <summary>100% functionality.</summary>
        Operational,

        /// <summary>Reduced functionality (partial damage).</summary>
        Damaged,

        /// <summary>0% functionality (destroyed).</summary>
        Offline,
    }

    /// <summary>
    /// Targeting strategy for weapon systems.
    /// </summary>
    public enum TacticalTargetingStrategy
    {
        Nearest,
        Weakest,
        Strongest,
        PlayerSelected,
    }
}
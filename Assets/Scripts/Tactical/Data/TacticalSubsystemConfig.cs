using System.Collections.Generic;

namespace Rebellion.Tactical.Data
{
    /// <summary>
    /// Configuration for a single ship subsystem. Each subsystem represents a percentage
    /// of the ship's actual stats. When damaged, the corresponding stat is reduced proportionally.
    /// </summary>
    public sealed class TacticalSubsystemConfig
    {
        /// <summary>The type of subsystem.</summary>
        public TacticalSubsystemType Type { get; }

        /// <summary>Maximum health of this subsystem.</summary>
        public int MaxHealth { get; }

        public TacticalSubsystemConfig(TacticalSubsystemType type, int maxHealth)
        {
            Type = type;
            MaxHealth = maxHealth;
        }

        /// <summary>
        /// Creates the subsystem set for a capital ship based on its strategic stats.
        /// Subsystems are only included if the ship has the corresponding capability.
        /// </summary>
        public static Dictionary<TacticalSubsystemType, TacticalSubsystemConfig> CreateCapitalShipSubsystems(
            int maxShieldStrength,
            int shieldRechargeRate,
            int sublightSpeed,
            int maneuverability,
            int weaponRecharge,
            int hyperdrive,
            int tractorBeamPower,
            int tractorBeamRange,
            bool hasGravityWell
        )
        {
            var subsystems = new Dictionary<TacticalSubsystemType, TacticalSubsystemConfig>
            {
                // Core subsystems always present on capital ships
                { TacticalSubsystemType.ShieldGenerator, new TacticalSubsystemConfig(TacticalSubsystemType.ShieldGenerator, maxShieldStrength) },
                { TacticalSubsystemType.SublightEngines, new TacticalSubsystemConfig(TacticalSubsystemType.SublightEngines, sublightSpeed * 5) },
                { TacticalSubsystemType.Weapons, new TacticalSubsystemConfig(TacticalSubsystemType.Weapons, 100) },
                { TacticalSubsystemType.Hyperdrive, new TacticalSubsystemConfig(TacticalSubsystemType.Hyperdrive, hyperdrive * 10) },
            };

            // Optional subsystems only if ship has the capability
            if (tractorBeamPower > 0)
            {
                subsystems[TacticalSubsystemType.TractorBeam] =
                    new TacticalSubsystemConfig(TacticalSubsystemType.TractorBeam, tractorBeamPower);
            }

            if (hasGravityWell)
            {
                subsystems[TacticalSubsystemType.GravityWellProjector] =
                    new TacticalSubsystemConfig(TacticalSubsystemType.GravityWellProjector, 100);
            }

            return subsystems;
        }

        /// <summary>
        /// Starfighters have no subsystems — they are simple hull-and-weapons units.
        /// </summary>
        public static Dictionary<TacticalSubsystemType, TacticalSubsystemConfig> CreateStarfighterSubsystems()
        {
            return new Dictionary<TacticalSubsystemType, TacticalSubsystemConfig>();
        }
    }
}
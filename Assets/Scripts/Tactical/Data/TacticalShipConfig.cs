using System.Collections.Generic;
using System.Linq;

namespace Rebellion.Tactical.Data
{
    /// <summary>
    /// Immutable configuration for a ship in tactical combat.
    /// Maps strategic-layer CapitalShip or Starfighter stats to tactical parameters.
    /// </summary>
    public sealed class TacticalShipConfig
    {
        /// <summary>Display name of the ship class.</summary>
        public string Name { get; }

        /// <summary>Maximum hull strength.</summary>
        public int MaxHull { get; }

        /// <summary>Maximum shield strength.</summary>
        public int MaxShields { get; }

        /// <summary>Shield recharge rate per tick.</summary>
        public int ShieldRechargeRate { get; }

        /// <summary>Base speed in units/sec.</summary>
        public float Speed { get; }

        /// <summary>Base agility/turn rate.</summary>
        public float Agility { get; }

        /// <summary>Turn rate in radians/sec.</summary>
        public float TurnRate { get; }

        /// <summary>Base weapon recharge time in seconds.</summary>
        public int WeaponRecharge { get; }

        /// <summary>Hyperdrive rating.</summary>
        public int Hyperdrive { get; }

        /// <summary>Tractor beam power.</summary>
        public int TractorBeamPower { get; }

        /// <summary>Tractor beam range.</summary>
        public int TractorBeamRange { get; }

        /// <summary>Whether the ship has a gravity well projector.</summary>
        public bool HasGravityWell { get; }

        /// <summary>Damage control rating (repair rate).</summary>
        public int DamageControl { get; }

        /// <summary>Weapons mounted on this ship.</summary>
        public IReadOnlyList<TacticalWeaponConfig> Weapons { get; }

        /// <summary>Subsystems for capital ships. Empty for starfighters.</summary>
        public IReadOnlyDictionary<TacticalSubsystemType, TacticalSubsystemConfig> Subsystems { get; }

        /// <summary>Whether this is a starfighter.</summary>
        public bool IsStarfighter { get; }

        /// <summary>Visual/collision size.</summary>
        public float Size { get; }

        private TacticalShipConfig(
            string name,
            int maxHull,
            int maxShields,
            int shieldRechargeRate,
            float speed,
            float agility,
            float turnRate,
            int weaponRecharge,
            int hyperdrive,
            int tractorBeamPower,
            int tractorBeamRange,
            bool hasGravityWell,
            int damageControl,
            IReadOnlyList<TacticalWeaponConfig> weapons,
            IReadOnlyDictionary<TacticalSubsystemType, TacticalSubsystemConfig> subsystems,
            bool isStarfighter,
            float size
        )
        {
            Name = name;
            MaxHull = maxHull;
            MaxShields = maxShields;
            ShieldRechargeRate = shieldRechargeRate;
            Speed = speed;
            Agility = agility;
            TurnRate = turnRate;
            WeaponRecharge = weaponRecharge;
            Hyperdrive = hyperdrive;
            TractorBeamPower = tractorBeamPower;
            TractorBeamRange = tractorBeamRange;
            HasGravityWell = hasGravityWell;
            DamageControl = damageControl;
            Weapons = weapons ?? new TacticalWeaponConfig[0];
            Subsystems = subsystems ?? new Dictionary<TacticalSubsystemType, TacticalSubsystemConfig>();
            IsStarfighter = isStarfighter;
            Size = size;
        }

        /// <summary>
        /// Creates a capital ship configuration from strategic stats.
        /// </summary>
        public static TacticalShipConfig CreateCapitalShip(
            string name,
            int hull,
            int shields,
            int speed,
            int agility,
            IReadOnlyList<TacticalWeaponConfig> weapons = null,
            Dictionary<TacticalSubsystemType, TacticalSubsystemConfig> subsystems = null,
            int shieldRechargeRate = 0,
            int weaponRecharge = 5,
            int hyperdrive = 8,
            int tractorBeamPower = 0,
            int tractorBeamRange = 0,
            bool hasGravityWell = false,
            int damageControl = 0
        )
        {
            var subsystemDict = subsystems ??
                TacticalSubsystemConfig.CreateCapitalShipSubsystems(
                    shields, shieldRechargeRate, speed, agility,
                    weaponRecharge, hyperdrive, tractorBeamPower,
                    tractorBeamRange, hasGravityWell);

            return new TacticalShipConfig(
                name, hull, shields, shieldRechargeRate,
                speed, agility, 0.5f, weaponRecharge, hyperdrive,
                tractorBeamPower, tractorBeamRange, hasGravityWell,
                damageControl, weapons ?? new TacticalWeaponConfig[0],
                subsystemDict, false, 100f);
        }

        /// <summary>
        /// Creates a starfighter configuration from strategic stats.
        /// </summary>
        public static TacticalShipConfig CreateStarfighter(
            string name,
            int hull,
            int shields,
            int speed,
            int agility,
            IReadOnlyList<TacticalWeaponConfig> weapons = null
        )
        {
            return new TacticalShipConfig(
                name, hull, shields, 0,
                speed, agility, 1.5f, 3, 0,
                0, 0, false, 0,
                weapons ?? new TacticalWeaponConfig[0],
                new Dictionary<TacticalSubsystemType, TacticalSubsystemConfig>(),
                true, 20f);
        }
    }
}
using System.Collections.Generic;
using Rebellion.Game;
using Rebellion.Game.Units;
using Rebellion.Tactical.Data;

namespace Rebellion.Tactical.Integration
{
    /// <summary>
    /// Bridge between strategic layer and tactical combat.
    /// Converts strategic fleets to tactical configs and applies results back.
    /// </summary>
    public sealed class TacticalBattleBridge
    {
        /// <summary>
        /// Creates a tactical battle config from strategic fleets.
        /// </summary>
        public TacticalBattleConfig CreateBattleConfig(
            Fleet attackerFleet,
            Fleet defenderFleet,
            string planetName)
        {
            var attackerShips = new List<TacticalShipConfig>();
            var defenderShips = new List<TacticalShipConfig>();

            // Convert attacker fleet ships
            foreach (var ship in attackerFleet.GetChildren<CapitalShip>())
            {
                attackerShips.Add(ConvertCapitalShip(ship));
            }

            // Convert defender fleet ships
            foreach (var ship in defenderFleet.GetChildren<CapitalShip>())
            {
                defenderShips.Add(ConvertCapitalShip(ship));
            }

            return new TacticalBattleConfig(
                attackerShips,
                defenderShips,
                planetName,
                0, // Random seed will be set by caller
                5000f,
                2000f);
        }

        /// <summary>
        /// Converts a strategic CapitalShip to a tactical ship config.
        /// </summary>
        public TacticalShipConfig ConvertCapitalShip(CapitalShip ship)
        {
            var weapons = new List<TacticalWeaponConfig>();

            // Convert primary weapons
            if (ship.PrimaryWeapons != null)
            {
                foreach (var weaponEntry in ship.PrimaryWeapons)
                {
                    int power = GetWeaponPower(weaponEntry.Value);
                    int range = GetWeaponRange(weaponEntry.Value);

                    switch (weaponEntry.Key)
                    {
                        case PrimaryWeaponType.Turbolaser:
                            weapons.Add(TacticalWeaponConfig.CreateLaserCannon(power, range));
                            break;
                        case PrimaryWeaponType.IonCannon:
                            weapons.Add(TacticalWeaponConfig.CreateIonCannon(power, range));
                            break;
                        case PrimaryWeaponType.LaserCannon:
                            weapons.Add(TacticalWeaponConfig.CreateLaserCannon(power, range));
                            break;
                    }
                }
            }

            var subsystems = TacticalSubsystemConfig.CreateCapitalShipSubsystems(
                ship.MaxShieldStrength,
                ship.ShieldRechargeRate,
                ship.SublightSpeed,
                ship.Maneuverability,
                ship.WeaponRecharge,
                ship.Hyperdrive,
                ship.TractorBeamPower,
                ship.TractorBeamnRange,
                ship.HasGravityWell);

            return TacticalShipConfig.CreateCapitalShip(
                ship.GetDisplayName(),
                ship.MaxHullStrength,
                ship.MaxShieldStrength,
                ship.SublightSpeed,
                ship.Maneuverability,
                weapons,
                subsystems,
                ship.ShieldRechargeRate,
                ship.WeaponRecharge,
                ship.Hyperdrive,
                ship.TractorBeamPower,
                ship.TractorBeamnRange,
                ship.HasGravityWell,
                ship.DamageControl);
        }

        /// <summary>
        /// Converts a strategic Starfighter to a tactical ship config.
        /// </summary>
        public TacticalShipConfig ConvertStarfighter(Starfighter fighter)
        {
            var weapons = new List<TacticalWeaponConfig>();

            if (fighter.LaserCannon > 0)
                weapons.Add(TacticalWeaponConfig.CreateLaserCannon(fighter.LaserCannon, fighter.LaserRange));

            if (fighter.IonCannon > 0)
                weapons.Add(TacticalWeaponConfig.CreateIonCannon(fighter.IonCannon, fighter.IonRange));

            if (fighter.Torpedoes > 0)
                weapons.Add(TacticalWeaponConfig.CreateTorpedo(fighter.Torpedoes, fighter.TorpedoRange));

            return TacticalShipConfig.CreateStarfighter(
                fighter.GetDisplayName(),
                fighter.MaxSquadronSize,
                fighter.ShieldStrength,
                fighter.SublightSpeed,
                fighter.Agility,
                weapons);
        }

        /// <summary>
        /// Applies tactical battle results back to the strategic layer.
        /// </summary>
        public void ApplyResult(GameRoot game, TacticalBattleResult result)
        {
            // TODO: Implement result application to strategic layer
            // This will need to:
            // 1. Find destroyed ships and remove them from fleets
            // 2. Apply hull damage to surviving ships
            // 3. Record squadron losses
            // 4. Handle withdrawals
        }

        private static int GetWeaponPower(int[] weaponValues)
        {
            if (weaponValues == null) return 0;
            int power = 0;
            foreach (var value in weaponValues)
                power += value;
            return power;
        }

        private static int GetWeaponRange(int[] weaponValues)
        {
            if (weaponValues == null) return 0;
            int range = 0;
            foreach (var value in weaponValues)
                range += value;
            return range;
        }
    }
}
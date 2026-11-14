using System.Collections.Generic;
using System.Linq;
using Rebellion.Tactical.Data;
using Rebellion.Tactical.Simulation;

namespace Rebellion.Gameplay
{
    /// <summary>
    /// Bridge between strategic layer and tactical combat.
    /// Converts strategic fleets to tactical configs and applies results back.
    /// </summary>
    public sealed class TacticalBattleBridge
    {
        /// <summary>
        /// Creates a tactical battle config from strategic capital ships.
        /// </summary>
        public TacticalBattleConfig CreateBattleConfig(
            IReadOnlyList<CapitalShip> attackerShips,
            IReadOnlyList<CapitalShip> defenderShips,
            string planetName,
            int randomSeed)
        {
            var attackerConfigs = attackerShips
                .Select(ship => ConvertCapitalShip(ship))
                .ToList();

            var defenderConfigs = defenderShips
                .Select(ship => ConvertCapitalShip(ship))
                .ToList();

            return new TacticalBattleConfig(
                attackerConfigs,
                defenderConfigs,
                planetName,
                randomSeed,
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

                    if (power <= 0) continue;

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
        /// Runs a headless tactical battle simulation.
        /// </summary>
        public TacticalBattleResult RunHeadlessBattle(TacticalBattleConfig config)
        {
            var simulation = new TacticalBattleSimulation(config);
            var targetingSystem = new TacticalTargetingSystem();
            var damageSystem = new TacticalDamageSystem(simulation.Random);
            var weaponSystem = new TacticalWeaponSystem(targetingSystem, damageSystem);
            var repairSystem = new TacticalRepairSystem();
            var aiController = new TacticalAIController(targetingSystem, weaponSystem);

            const float deltaTime = 1f / 60f;
            const int maxTicks = 6000; // Max 100 seconds of simulation

            for (int tick = 0; tick < maxTicks && !simulation.IsBattleOver; tick++)
            {
                simulation.Update(deltaTime);

                // AI controls all ships in headless mode
                foreach (var ship in simulation.AllShips)
                {
                    if (ship.IsDestroyed) continue;

                    var enemies = ship.Side == TacticalShipSide.Attacker
                        ? simulation.DefenderShips
                        : simulation.AttackerShips;

                    aiController.Update(ship, enemies, deltaTime);
                    weaponSystem.UpdateCooldowns(ship, deltaTime);

                    var shots = weaponSystem.TryFireWeapons(ship, enemies, deltaTime);
                    foreach (var shot in shots)
                    {
                        if (!shot.IsProjectile)
                            damageSystem.ApplyDamage(shot.Target, shot.Damage);
                    }
                }

                // Repair
                foreach (var ship in simulation.AllShips)
                {
                    if (!ship.IsDestroyed)
                        repairSystem.Update(ship, deltaTime);
                }
            }

            return simulation.FinalizeResult();
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
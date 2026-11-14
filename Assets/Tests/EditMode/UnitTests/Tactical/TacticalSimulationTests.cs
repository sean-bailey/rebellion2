using System.Collections.Generic;
using NUnit.Framework;
using Rebellion.Tactical.Data;
using Rebellion.Tactical.Simulation;

namespace Rebellion.Tests.EditMode.UnitTests.Tactical
{
    /// <summary>
    /// Unit tests for the tactical simulation core.
    /// </summary>
    public sealed class TacticalSimulationTests
    {
        [Test]
        public void ShipEntity_StartsAtFullHealth()
        {
            var config = TacticalShipConfig.CreateCapitalShip("Test", 100, 50, 10, 5);
            var entity = new TacticalShipEntity("ship-1", config, TacticalShipSide.Attacker);

            Assert.AreEqual(100, entity.Hull);
            Assert.AreEqual(50, entity.Shields);
            Assert.AreEqual(TacticalShipStatus.Active, entity.Status);
            Assert.IsFalse(entity.IsDestroyed);
        }

        [Test]
        public void ShipEntity_CapitalShipHasSubsystems()
        {
            var subsystems = TacticalSubsystemConfig.CreateCapitalShipSubsystems(
                500, 10, 20, 10, 5, 8, 0, 0, false);
            var config = TacticalShipConfig.CreateCapitalShip(
                "Test", 1000, 500, 20, 10, new List<TacticalWeaponConfig>(), subsystems);
            var entity = new TacticalShipEntity("ship-1", config, TacticalShipSide.Attacker);

            Assert.AreEqual(4, entity.Subsystems.Count);

            foreach (var subsystem in entity.Subsystems.Values)
                Assert.AreEqual(subsystem.Config.MaxHealth, subsystem.Health);
        }

        [Test]
        public void ShipEntity_EffectiveSpeedScalesWithEngineDamage()
        {
            var subsystems = TacticalSubsystemConfig.CreateCapitalShipSubsystems(
                500, 10, 20, 10, 5, 8, 0, 0, false);
            var config = TacticalShipConfig.CreateCapitalShip(
                "Test", 1000, 500, 20, 10, new List<TacticalWeaponConfig>(), subsystems);
            var entity = new TacticalShipEntity("ship-1", config, TacticalShipSide.Attacker);

            // Damage engines to 50%
            var engines = entity.Subsystems[TacticalSubsystemType.SublightEngines];
            engines.Health = engines.Config.MaxHealth / 2;

            Assert.AreEqual(10f, entity.EffectiveSpeed);
        }

        [Test]
        public void ShipEntity_StarfighterHasNoSubsystems()
        {
            var config = TacticalShipConfig.CreateStarfighter("X-Wing", 100, 0, 20, 15);
            var entity = new TacticalShipEntity("ship-1", config, TacticalShipSide.Attacker);

            Assert.AreEqual(0, entity.Subsystems.Count);
            Assert.AreEqual(20f, entity.EffectiveSpeed);
        }

        [Test]
        public void DamageSystem_ShieldsAbsorbDamageFirst()
        {
            var system = new TacticalDamageSystem();
            var ship = CreateTestCapitalShip();

            system.ApplyDamage(ship, 300);
            Assert.AreEqual(1000, ship.Hull);
            Assert.AreEqual(200, ship.Shields);
        }

        [Test]
        public void DamageSystem_ShipDestroyedAtZeroHull()
        {
            var system = new TacticalDamageSystem();
            var ship = CreateTestCapitalShip();

            system.ApplyDamage(ship, 20000);
            Assert.IsTrue(ship.IsDestroyed);
            Assert.AreEqual(TacticalShipStatus.Destroyed, ship.Status);
        }

        [Test]
        public void DamageSystem_StarfighterTakesDirectHullDamage()
        {
            var system = new TacticalDamageSystem();
            var ship = CreateTestStarfighter();

            system.ApplyDamage(ship, 30);
            Assert.AreEqual(70, ship.Hull);
        }

        [Test]
        public void Simulation_InitializesShipsFromConfig()
        {
            var config = CreateTestBattleConfig();
            var sim = new TacticalBattleSimulation(config);

            Assert.AreEqual(config.AttackerShips.Count, sim.AttackerShips.Count);
            Assert.AreEqual(config.DefenderShips.Count, sim.DefenderShips.Count);
        }

        [Test]
        public void Simulation_IsDeterministic()
        {
            var config = CreateTestBattleConfig();
            var sim1 = new TacticalBattleSimulation(config);
            var sim2 = new TacticalBattleSimulation(config);

            for (int i = 0; i < 60; i++)
            {
                sim1.Update(1f / 60f);
                sim2.Update(1f / 60f);
            }

            // All ships should be in identical states
            for (int i = 0; i < sim1.AllShips.Count; i++)
            {
                Assert.AreEqual(
                    sim1.AllShips[i].Position.x,
                    sim2.AllShips[i].Position.x,
                    0.001f);
                Assert.AreEqual(
                    sim1.AllShips[i].Position.y,
                    sim2.AllShips[i].Position.y,
                    0.001f);
                Assert.AreEqual(
                    sim1.AllShips[i].Position.z,
                    sim2.AllShips[i].Position.z,
                    0.001f);
            }
        }

        [Test]
        public void Simulation_DetectsBattleEnd()
        {
            var config = CreateTestBattleConfig();
            var sim = new TacticalBattleSimulation(config);

            // Destroy all defender ships
            foreach (var ship in sim.DefenderShips)
                ship.TakeDamage(9999, 9999);

            sim.Update(1f);
            Assert.IsTrue(sim.IsBattleOver);
            Assert.AreEqual(TacticalBattleOutcome.Victory, sim.Outcome);
        }

        [Test]
        public void RepairSystem_CapitalShipRepairs()
        {
            var system = new TacticalRepairSystem();
            var ship = CreateTestCapitalShip(damageControl: 10);

            // Damage hull
            ship.TakeDamage(50, 0);
            int hullBefore = ship.Hull;

            system.Update(ship, 1f);

            Assert.Greater(ship.Hull, hullBefore);
        }

        [Test]
        public void RepairSystem_StarfighterDoesNotRepair()
        {
            var system = new TacticalRepairSystem();
            var ship = CreateTestStarfighter();

            ship.TakeDamage(50, 0);
            int hullBefore = ship.Hull;

            system.Update(ship, 10f);

            Assert.AreEqual(hullBefore, ship.Hull);
        }

        [Test]
        public void TargetingSystem_FindsNearestEnemy()
        {
            var system = new TacticalTargetingSystem();
            var ship = CreateTestShip();
            var enemy1 = CreateEnemyShip(new Unity.Mathematics.float3(100, 0, 0));
            var enemy2 = CreateEnemyShip(new Unity.Mathematics.float3(50, 0, 0));

            var target = system.FindNearestEnemy(ship, new[] { enemy1, enemy2 });
            Assert.AreEqual(enemy2, target);
        }

        [Test]
        public void SpeedController_DefaultsToRealTime()
        {
            var controller = new Rebellion.Tactical.UI.TacticalSpeedController();
            Assert.AreEqual(TacticalBattleSpeed.RealTime, controller.CurrentSpeed);
            Assert.AreEqual(1f, controller.TimeScale);
        }

        [Test]
        public void SpeedController_CanPause()
        {
            var controller = new Rebellion.Tactical.UI.TacticalSpeedController();
            controller.SetSpeed(TacticalBattleSpeed.Paused);
            Assert.AreEqual(0f, controller.TimeScale);
        }

        private static TacticalShipEntity CreateTestCapitalShip(int damageControl = 0)
        {
            var subsystems = TacticalSubsystemConfig.CreateCapitalShipSubsystems(
                500, 10, 20, 10, 5, 8, 0, 0, false);
            var config = TacticalShipConfig.CreateCapitalShip(
                "Test", 1000, 500, 20, 10, new List<TacticalWeaponConfig>(),
                subsystems, damageControl: damageControl);
            return new TacticalShipEntity("ship-1", config, TacticalShipSide.Attacker);
        }

        private static TacticalShipEntity CreateTestStarfighter()
        {
            var config = TacticalShipConfig.CreateStarfighter("X-Wing", 100, 0, 20, 15);
            return new TacticalShipEntity("ship-1", config, TacticalShipSide.Attacker);
        }

        private static TacticalShipEntity CreateTestShip()
        {
            var config = TacticalShipConfig.CreateCapitalShip("Test", 100, 50, 10, 5);
            return new TacticalShipEntity("ship-1", config, TacticalShipSide.Attacker);
        }

        private static TacticalShipEntity CreateEnemyShip(Unity.Mathematics.float3 position)
        {
            var config = TacticalShipConfig.CreateCapitalShip("Enemy", 100, 50, 10, 5);
            var entity = new TacticalShipEntity("enemy-1", config, TacticalShipSide.Defender);
            entity.Position = position;
            return entity;
        }

        private static TacticalBattleConfig CreateTestBattleConfig()
        {
            var attackerConfig = TacticalShipConfig.CreateCapitalShip("Attacker", 1000, 500, 20, 10);
            var defenderConfig = TacticalShipConfig.CreateCapitalShip("Defender", 1000, 500, 20, 10);

            return new TacticalBattleConfig(
                new List<TacticalShipConfig> { attackerConfig },
                new List<TacticalShipConfig> { defenderConfig },
                "Test Planet",
                12345);
        }
    }
}
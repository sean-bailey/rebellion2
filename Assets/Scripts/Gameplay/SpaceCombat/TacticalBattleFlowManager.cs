using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Rebellion.Game;
using Rebellion.Game.Galaxy;
using Rebellion.Game.Units;
using Rebellion.Tactical.Data;
using Rebellion.Tactical.Presentation;

namespace Rebellion.Gameplay
{
    /// <summary>
    /// Manages the flow between strategic and tactical battle scenes.
    /// Handles scene transitions and battle result application.
    /// Persists across scene loads via DontDestroyOnLoad.
    /// </summary>
    public sealed class TacticalBattleFlowManager : MonoBehaviour, ITacticalBattleFlowManager
    {
        private const string TacticalSceneName = "TacticalBattle";
        private const string StrategySceneName = "StrategyView";

        [NonSerialized] private SpaceCombatDecision _pendingDecision;
        [NonSerialized] private GameRoot _game;
        [NonSerialized] private SpaceCombatCommands _spaceCombatCommands;

        /// <summary>
        /// Starts a tactical battle from the strategic layer.
        /// Stores the decision and loads the tactical scene.
        /// </summary>
        public static void StartTacticalBattle(
            GameRoot game,
            SpaceCombatCommands spaceCombatCommands,
            SpaceCombatDecision decision)
        {
            var manager = FindObjectOfType<TacticalBattleFlowManager>();
            if (manager == null)
            {
                var managerGO = new GameObject("TacticalBattleFlowManager");
                manager = managerGO.AddComponent<TacticalBattleFlowManager>();
                DontDestroyOnLoad(managerGO);
            }

            manager._game = game;
            manager._spaceCombatCommands = spaceCombatCommands;
            manager._pendingDecision = decision;

            // Load the tactical battle scene
            SceneManager.LoadSceneAsync(TacticalSceneName);
        }

        /// <summary>
        /// Called when the tactical battle scene is loaded.
        /// Initializes the battle with the stored config.
        /// </summary>
        public void OnTacticalSceneLoaded()
        {
            if (_pendingDecision == null || _game == null)
                return;

            var bridge = new TacticalBattleBridge();

            var attackerFleets = GetFleets(_pendingDecision.AttackerFleetInstanceIDs);
            var defenderFleets = GetFleets(_pendingDecision.DefenderFleetInstanceIDs);

            var attackerShips = attackerFleets
                .SelectMany(fleet => fleet.GetChildren<CapitalShip>())
                .Where(ship => ship.ManufacturingStatus == ManufacturingStatus.Complete)
                .ToList();

            var defenderShips = defenderFleets
                .SelectMany(fleet => fleet.GetChildren<CapitalShip>())
                .Where(ship => ship.ManufacturingStatus == ManufacturingStatus.Complete)
                .ToList();

            var planet = _game.GetSceneNodeByInstanceID<Planet>(
                _pendingDecision.PlanetInstanceID);
            string planetName = planet?.GetDisplayName() ?? "Unknown";

            var battleConfig = bridge.CreateBattleConfig(
                attackerShips,
                defenderShips,
                planetName,
                _game.Random.NextInt(0, int.MaxValue));

            // Find the TacticalBattleEntry and start the battle
            var entry = FindObjectOfType<TacticalBattleEntry>();
            if (entry != null)
            {
                entry.StartTacticalBattle(battleConfig);
            }
        }

        /// <summary>
        /// Called when the tactical battle ends.
        /// Returns to the strategic scene and applies results.
        /// </summary>
        public void OnTacticalBattleComplete(TacticalBattleResult result)
        {
            if (_pendingDecision == null || _game == null)
                return;

            ApplyBattleResults(result);

            // Clear the pending decision
            _pendingDecision = null;
            _game = null;
            _spaceCombatCommands = null;

            // Return to strategic scene
            SceneManager.LoadSceneAsync(StrategySceneName);
        }

        private void ApplyBattleResults(TacticalBattleResult result)
        {
            foreach (var destroyedRecord in result.DestroyedShips)
            {
                var ship = FindShipById(destroyedRecord.ShipId);
                if (ship != null)
                {
                    ship.CurrentHullStrength = 0;
                }
            }

            foreach (var damagedRecord in result.DamagedShips)
            {
                var ship = FindShipById(damagedRecord.ShipId);
                if (ship != null)
                {
                    ship.CurrentHullStrength = Math.Max(
                        0,
                        ship.CurrentHullStrength - damagedRecord.HullDamage);
                }
            }
        }

        private List<Fleet> GetFleets(IEnumerable<string> fleetInstanceIds)
        {
            return (fleetInstanceIds ?? Enumerable.Empty<string>())
                .Select(id => _game.GetSceneNodeByInstanceID<Fleet>(id))
                .Where(fleet => fleet != null)
                .ToList();
        }

        private CapitalShip FindShipById(string shipId)
        {
            if (_game == null)
                return null;

            if (shipId.StartsWith("attacker-"))
            {
                int index = int.Parse(shipId.Substring("attacker-".Length));
                var fleets = GetFleets(_pendingDecision.AttackerFleetInstanceIDs);
                var ships = fleets
                    .SelectMany(fleet => fleet.GetChildren<CapitalShip>())
                    .Where(ship => ship.ManufacturingStatus == ManufacturingStatus.Complete)
                    .ToList();
                if (index >= 0 && index < ships.Count)
                    return ships[index];
            }
            else if (shipId.StartsWith("defender-"))
            {
                int index = int.Parse(shipId.Substring("defender-".Length));
                var fleets = GetFleets(_pendingDecision.DefenderFleetInstanceIDs);
                var ships = fleets
                    .SelectMany(fleet => fleet.GetChildren<CapitalShip>())
                    .Where(ship => ship.ManufacturingStatus == ManufacturingStatus.Complete)
                    .ToList();
                if (index >= 0 && index < ships.Count)
                    return ships[index];
            }

            return null;
        }
    }
}

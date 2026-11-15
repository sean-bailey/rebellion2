using System.Collections.Generic;
using UnityEngine;
using Rebellion.Tactical.Data;
using Rebellion.Tactical.Simulation;
using Rebellion.Tactical.AI;

namespace Rebellion.Tactical.Presentation
{
    /// <summary>
    /// State machine for the tactical battle lifecycle.
    /// </summary>
    public enum TacticalBattleStateEnum
    {
        Idle,
        Running,
        Complete,
    }

    /// <summary>
    /// MonoBehaviour that manages the tactical battle lifecycle in Unity.
    /// Coordinates simulation, rendering, input, and UI.
    /// </summary>
    public sealed class TacticalBattleCoordinator : MonoBehaviour
    {
        private TacticalBattleStateEnum _state;
        private TacticalBattleSimulation _simulation;
        private TacticalSpeedController _speedController;
        private TacticalDamageSystem _damageSystem;
        private TacticalRepairSystem _repairSystem;
        private TacticalWeaponSystem _weaponSystem;
        private TacticalTargetingSystem _targetingSystem;
        private TacticalAIController _aiController;

        private List<TacticalProjectile> _projectiles = new List<TacticalProjectile>();
        private TacticalBattleSceneBuilder _sceneBuilder;

        /// <summary>Current battle state.</summary>
        public TacticalBattleStateEnum State => _state;

        /// <summary>The active simulation.</summary>
        public TacticalBattleSimulation Simulation => _simulation;

        /// <summary>Speed controller.</summary>
        public TacticalSpeedController SpeedController => _speedController;

        /// <summary>Event raised when the battle completes.</summary>
        public event System.Action<TacticalBattleResult> BattleComplete;

        private void Start()
        {
            _sceneBuilder = GetComponent<TacticalBattleSceneBuilder>();
        }

        /// <summary>
        /// Starts a tactical battle with the given config.
        /// </summary>
        public void StartBattle(TacticalBattleConfig config)
        {
            if (_state != TacticalBattleStateEnum.Idle)
                return;

            _simulation = new TacticalBattleSimulation(config);
            _speedController = new TacticalSpeedController();
            _targetingSystem = new TacticalTargetingSystem();
            _damageSystem = new TacticalDamageSystem(_simulation.Random);
            _repairSystem = new TacticalRepairSystem();
            _weaponSystem = new TacticalWeaponSystem(_targetingSystem, _damageSystem);
            _aiController = new TacticalAIController(_targetingSystem, _weaponSystem);

            _state = TacticalBattleStateEnum.Running;
        }

        /// <summary>
        /// Ends the battle and returns the result.
        /// </summary>
        public TacticalBattleResult EndBattle()
        {
            if (_state != TacticalBattleStateEnum.Running)
                return null;

            _state = TacticalBattleStateEnum.Complete;
            var result = _simulation.FinalizeResult();
            BattleComplete?.Invoke(result);
            return result;
        }

        /// <summary>
        /// Pauses the battle.
        /// </summary>
        public void Pause()
        {
            _speedController?.SetSpeed(TacticalBattleSpeed.Paused);
        }

        /// <summary>
        /// Resumes the battle at real-time speed.
        /// </summary>
        public void Resume()
        {
            _speedController?.SetSpeed(TacticalBattleSpeed.RealTime);
        }

        /// <summary>
        /// Unity Update loop — drives the simulation.
        /// </summary>
        private void Update()
        {
            if (_state != TacticalBattleStateEnum.Running || _simulation == null)
                return;

            float deltaTime = Time.deltaTime * _speedController.TimeScale;

            // Update simulation movement
            _simulation.Update(deltaTime);

            // Update AI ships (defenders)
            foreach (var ship in _simulation.DefenderShips)
            {
                if (!ship.IsDestroyed)
                    _aiController.Update(ship, _simulation.AttackerShips, deltaTime);
            }

            // Update weapons
            foreach (var ship in _simulation.AllShips)
            {
                if (ship.IsDestroyed) continue;
                _weaponSystem.UpdateCooldowns(ship, deltaTime);
            }

            // Fire weapons
            foreach (var ship in _simulation.AllShips)
            {
                if (ship.IsDestroyed) continue;
                var enemies = ship.Side == TacticalShipSide.Attacker
                    ? _simulation.DefenderShips
                    : _simulation.AttackerShips;
                var shots = _weaponSystem.TryFireWeapons(ship, enemies, deltaTime);
                foreach (var shot in shots)
                {
                    if (shot.IsProjectile)
                        _projectiles.Add(new TacticalProjectile(shot));
                    else
                        _damageSystem.ApplyDamage(shot.Target, shot.Damage);
                }
            }

            // Update projectiles
            _weaponSystem.UpdateProjectiles(_projectiles, _simulation.AllShips, deltaTime);
            _projectiles.RemoveAll(p => p.IsDestroyed);

            // Update repairs
            foreach (var ship in _simulation.AllShips)
            {
                if (!ship.IsDestroyed)
                    _repairSystem.Update(ship, deltaTime);
            }

            // Update ship renderers
            if (_sceneBuilder != null)
            {
                _sceneBuilder.UpdateShipRenderers();
            }

            // Check battle end
            if (_simulation.IsBattleOver)
            {
                EndBattle();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;

namespace Rebellion.Tactical.Simulation
{
    /// <summary>
    /// Core deterministic battle simulation loop. Pure C#, no Unity dependencies.
    /// Runs at fixed timestep for consistent behavior across frame rates.
    /// </summary>
    public sealed class TacticalBattleSimulation
    {
        private readonly List<TacticalShipEntity> _allShips = new List<TacticalShipEntity>();
        private readonly TacticalMovementSystem _movementSystem = new TacticalMovementSystem();
        private readonly System.Random _random;

        /// <summary>All ships in the battle.</summary>
        public IReadOnlyList<TacticalShipEntity> AllShips => _allShips;

        /// <summary>Attacker-side ships.</summary>
        public IReadOnlyList<TacticalShipEntity> AttackerShips { get; }

        /// <summary>Defender-side ships.</summary>
        public IReadOnlyList<TacticalShipEntity> DefenderShips { get; }

        /// <summary>Elapsed simulation time.</summary>
        public float ElapsedTime { get; private set; }

        /// <summary>Whether the battle has ended.</summary>
        public bool IsBattleOver { get; private set; }

        /// <summary>Battle outcome.</summary>
        public TacticalBattleOutcome Outcome { get; private set; }

        /// <summary>Battle result record.</summary>
        public TacticalBattleResult Result { get; private set; }

        /// <summary>RNG for deterministic simulation.</summary>
        public System.Random Random => _random;

        public TacticalBattleSimulation(TacticalBattleConfig config)
        {
            _random = new System.Random(config.RandomSeed);

            var attackerList = new List<TacticalShipEntity>();
            var defenderList = new List<TacticalShipEntity>();

            // Position attackers on one side, defenders on the other
            for (int i = 0; i < config.AttackerShips.Count; i++)
            {
                var configShip = config.AttackerShips[i];
                var entity = new TacticalShipEntity(
                    $"attacker-{i}", configShip, TacticalShipSide.Attacker);

                // Spread ships across the attacker line
                float spread = (i - (config.AttackerShips.Count - 1) / 2f) * 300f;
                entity.Position = new float3(-config.StartingDistance, spread, 0);
                entity.Orientation = quaternion.identity; // Facing +X

                attackerList.Add(entity);
                _allShips.Add(entity);
            }

            for (int i = 0; i < config.DefenderShips.Count; i++)
            {
                var configShip = config.DefenderShips[i];
                var entity = new TacticalShipEntity(
                    $"defender-{i}", configShip, TacticalShipSide.Defender);

                // Spread ships across the defender line
                float spread = (i - (config.DefenderShips.Count - 1) / 2f) * 300f;
                entity.Position = new float3(config.StartingDistance, spread, 0);
                entity.Orientation = quaternion.Euler(0, 180f, 0); // Facing -X

                defenderList.Add(entity);
                _allShips.Add(entity);
            }

            AttackerShips = attackerList;
            DefenderShips = defenderList;
        }

        /// <summary>
        /// Advances the simulation by deltaTime seconds.
        /// </summary>
        public void Update(float deltaTime)
        {
            if (IsBattleOver) return;

            ElapsedTime += deltaTime;

            foreach (var ship in _allShips)
            {
                if (!ship.IsDestroyed)
                    _movementSystem.Update(ship, deltaTime);
            }

            CheckBattleEnd();
        }

        private void CheckBattleEnd()
        {
            bool attackersAlive = AttackerShips.Any(s => !s.IsDestroyed);
            bool defendersAlive = DefenderShips.Any(s => !s.IsDestroyed);

            if (!attackersAlive && !defendersAlive)
            {
                IsBattleOver = true;
                Outcome = TacticalBattleOutcome.Stalemate;
            }
            else if (!attackersAlive)
            {
                IsBattleOver = true;
                Outcome = TacticalBattleOutcome.Defeat;
            }
            else if (!defendersAlive)
            {
                IsBattleOver = true;
                Outcome = TacticalBattleOutcome.Victory;
            }
        }

        /// <summary>
        /// Finalizes the battle result.
        /// </summary>
        public TacticalBattleResult FinalizeResult()
        {
            Result = new TacticalBattleResult(Outcome);
            Result.DurationSeconds = ElapsedTime;

            foreach (var ship in _allShips)
            {
                if (ship.IsDestroyed)
                {
                    Result.RecordDestroyedShip(ship.Id, ship.Side);
                }
                else
                {
                    int hullDamage = ship.Config.MaxHull - ship.Hull;
                    int shieldDamage = ship.Config.MaxShields - ship.Shields;
                    if (hullDamage > 0 || shieldDamage > 0)
                    {
                        Result.RecordDamagedShip(ship.Id, ship.Side, hullDamage, shieldDamage);
                    }
                }
            }

            return Result;
        }
    }
}
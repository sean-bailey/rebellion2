using System.Collections.Generic;

namespace Rebellion.Tactical.Data
{
    /// <summary>
    /// Configuration for a tactical battle instance.
    /// </summary>
    public sealed class TacticalBattleConfig
    {
        /// <summary>Ships on the attacker side.</summary>
        public IReadOnlyList<TacticalShipConfig> AttackerShips { get; }

        /// <summary>Ships on the defender side.</summary>
        public IReadOnlyList<TacticalShipConfig> DefenderShips { get; }

        /// <summary>Name of the planet/system where battle occurs.</summary>
        public string PlanetName { get; }

        /// <summary>RNG seed for deterministic simulation.</summary>
        public int RandomSeed { get; }

        /// <summary>Radius of the battlefield in world units.</summary>
        public float BattlefieldRadius { get; }

        /// <summary>Initial distance between opposing fleets.</summary>
        public float StartingDistance { get; }

        public TacticalBattleConfig(
            IReadOnlyList<TacticalShipConfig> attackerShips,
            IReadOnlyList<TacticalShipConfig> defenderShips,
            string planetName = null,
            int randomSeed = 0,
            float battlefieldRadius = 5000f,
            float startingDistance = 2000f
        )
        {
            AttackerShips = attackerShips ?? new TacticalShipConfig[0];
            DefenderShips = defenderShips ?? new TacticalShipConfig[0];
            PlanetName = planetName ?? "Unknown";
            RandomSeed = randomSeed;
            BattlefieldRadius = battlefieldRadius;
            StartingDistance = startingDistance;
        }
    }
}
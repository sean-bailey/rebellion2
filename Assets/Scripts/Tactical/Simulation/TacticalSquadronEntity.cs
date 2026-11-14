using System.Collections.Generic;
using System.Linq;

namespace Rebellion.Tactical.Simulation
{
    /// <summary>
    /// Represents a starfighter squadron of 12 fighters.
    /// Damaging a squadron reduces its fighter count, and its attack and
    /// defense stats scale proportionally with remaining fighters.
    /// Squadrons do NOT repair in battle — lost fighters are gone.
    /// </summary>
    public sealed class TacticalSquadronEntity
    {
        /// <summary>Full squadron size.</summary>
        public const int FullSquadronSize = 12;

        /// <summary>Unique squadron identifier.</summary>
        public string Id { get; }

        /// <summary>Squadron configuration.</summary>
        public TacticalShipConfig Config { get; }

        /// <summary>Side this squadron belongs to.</summary>
        public TacticalShipSide Side { get; }

        /// <summary>All fighters in the squadron.</summary>
        public List<TacticalShipEntity> Fighters { get; }

        /// <summary>Alive fighters only.</summary>
        public IReadOnlyList<TacticalShipEntity> AliveFighters =>
            Fighters.Where(f => !f.IsDestroyed).ToList();

        /// <summary>Count of alive fighters.</summary>
        public int AliveFighterCount => AliveFighters.Count;

        /// <summary>Whether all fighters are destroyed.</summary>
        public bool IsEmpty => AliveFighters.Count == 0;

        /// <summary>Total hull of all alive fighters.</summary>
        public int TotalHull => AliveFighters.Sum(f => f.Hull);

        /// <summary>
        /// Attack multiplier based on remaining fighters (0.0 to 1.0).
        /// Used to scale the squadron's effective attack power.
        /// </summary>
        public float EffectiveAttackMultiplier =>
            (float)AliveFighterCount / FullSquadronSize;

        public TacticalSquadronEntity(string id, TacticalShipConfig config, TacticalShipSide side)
        {
            Id = id;
            Config = config;
            Side = side;
            Fighters = new List<TacticalShipEntity>(FullSquadronSize);
            for (int i = 0; i < FullSquadronSize; i++)
            {
                Fighters.Add(new TacticalShipEntity(
                    $"{id}-fighter-{i}", config, side));
            }
        }

        /// <summary>
        /// Updates all alive fighters. No repair for starfighters.
        /// </summary>
        public void Update(float deltaTime, TacticalMovementSystem movementSystem)
        {
            foreach (var fighter in Fighters)
            {
                if (!fighter.IsDestroyed)
                    movementSystem.Update(fighter, deltaTime);
            }
        }
    }
}
using System.Collections.Generic;
using System.Linq;

namespace Rebellion.Tactical.Simulation
{
    /// <summary>
    /// Handles damage application with the layered damage model:
    /// shields → shield generator → other subsystems (weighted random) → hull.
    /// </summary>
    public sealed class TacticalDamageSystem
    {
        private readonly System.Random _random;

        // Subsystem damage weights (higher = more likely to be hit)
        private static readonly Dictionary<TacticalSubsystemType, int> SubsystemWeights =
            new Dictionary<TacticalSubsystemType, int>
            {
                { TacticalSubsystemType.SublightEngines, 3 },
                { TacticalSubsystemType.Weapons, 3 },
                { TacticalSubsystemType.Hyperdrive, 2 },
                { TacticalSubsystemType.TractorBeam, 1 },
                { TacticalSubsystemType.GravityWellProjector, 1 },
            };

        public TacticalDamageSystem(System.Random random = null)
        {
            _random = random ?? new System.Random();
        }

        /// <summary>
        /// Applies damage to a ship using the layered damage model.
        /// </summary>
        public void ApplyDamage(TacticalShipEntity ship, int damage)
        {
            if (ship.IsDestroyed) return;

            // Phase 1: Shields absorb damage
            if (ship.Shields > 0)
            {
                int shieldDamage = System.Math.Min(ship.Shields, damage);
                ship.Shields -= shieldDamage;
                damage -= shieldDamage;
                ship.ShieldRegenTimer = 0; // Reset regen on hit
            }

            if (damage <= 0) return;

            // Starfighters: remaining damage goes straight to hull
            if (ship.Config.IsStarfighter)
            {
                ApplyHullDamage(ship, damage);
                return;
            }

            // Phase 2: Shield generator takes damage (priority target)
            if (ship.Subsystems.TryGetValue(TacticalSubsystemType.ShieldGenerator, out var shieldGen))
            {
                if (shieldGen.Health > 0)
                {
                    int absorbed = shieldGen.ApplyDamage(damage);
                    damage -= absorbed;
                    if (damage <= 0) return;
                }
            }

            // Phase 3: Other subsystems take damage (weighted random)
            if (damage > 0)
            {
                damage = ApplySubsystemDamage(ship, damage);
            }

            // Phase 4: Hull takes remaining damage
            if (damage > 0)
            {
                ApplyHullDamage(ship, damage);
            }
        }

        private int ApplySubsystemDamage(TacticalShipEntity ship, int damage)
        {
            // Collect operational subsystems (excluding shield generator)
            var targets = ship.Subsystems.Values
                .Where(s => s.Config.Type != TacticalSubsystemType.ShieldGenerator && s.Health > 0)
                .ToList();

            if (targets.Count == 0) return damage;

            // Calculate total weight
            int totalWeight = targets.Sum(t =>
                SubsystemWeights.TryGetValue(t.Config.Type, out int w) ? w : 1);

            while (damage > 0 && targets.Any(t => t.Health > 0))
            {
                // Weighted random selection
                int roll = _random.Next(totalWeight);
                TacticalSubsystemState selected = null;
                int cumulative = 0;
                foreach (var target in targets)
                {
                    if (target.Health <= 0) continue;
                    int weight = SubsystemWeights.TryGetValue(target.Config.Type, out int w) ? w : 1;
                    cumulative += weight;
                    if (roll < cumulative)
                    {
                        selected = target;
                        break;
                    }
                }

                if (selected == null) break;

                int absorbed = selected.ApplyDamage(damage);
                damage -= absorbed;

                // Recalculate weights if subsystem went offline
                if (selected.Health <= 0)
                {
                    totalWeight -= SubsystemWeights.TryGetValue(selected.Config.Type, out int w) ? w : 1;
                    if (totalWeight <= 0) break;
                }
            }

            return damage;
        }

        private void ApplyHullDamage(TacticalShipEntity ship, int damage)
        {
            ship.Hull = System.Math.Max(0, ship.Hull - damage);
            if (ship.Hull <= 0)
            {
                ship.Status = TacticalShipStatus.Destroyed;
            }
        }

        /// <summary>
        /// Applies ion damage that disables the ship.
        /// </summary>
        public void ApplyIonDamage(TacticalShipEntity ship, float duration)
        {
            if (ship.IsDestroyed) return;
            ship.Disable(duration);
        }
    }
}
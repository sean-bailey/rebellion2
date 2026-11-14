using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;

namespace Rebellion.Tactical.Simulation
{
    /// <summary>
    /// Target selection logic for weapon systems.
    /// </summary>
    public sealed class TacticalTargetingSystem
    {
        /// <summary>
        /// Finds the nearest enemy ship to the given ship.
        /// </summary>
        public TacticalShipEntity FindNearestEnemy(
            TacticalShipEntity ship, IEnumerable<TacticalShipEntity> enemies)
        {
            TacticalShipEntity nearest = null;
            float nearestDistance = float.MaxValue;

            foreach (var enemy in enemies)
            {
                if (enemy.IsDestroyed) continue;
                float distance = math.distance(ship.Position, enemy.Position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = enemy;
                }
            }

            return nearest;
        }

        /// <summary>
        /// Finds an enemy target within weapon range.
        /// </summary>
        public TacticalShipEntity FindTargetInRange(
            TacticalShipEntity ship,
            IEnumerable<TacticalShipEntity> enemies,
            TacticalWeaponConfig weapon)
        {
            TacticalShipEntity target = null;
            float bestDistance = float.MaxValue;

            foreach (var enemy in enemies)
            {
                if (enemy.IsDestroyed) continue;
                float distance = math.distance(ship.Position, enemy.Position);
                if (distance <= weapon.Range && distance < bestDistance)
                {
                    bestDistance = distance;
                    target = enemy;
                }
            }

            return target;
        }

        /// <summary>
        /// Finds the weakest enemy target (lowest hull percentage).
        /// </summary>
        public TacticalShipEntity FindWeakestTarget(
            TacticalShipEntity ship, IEnumerable<TacticalShipEntity> enemies)
        {
            TacticalShipEntity weakest = null;
            float lowestHullPct = float.MaxValue;

            foreach (var enemy in enemies)
            {
                if (enemy.IsDestroyed) continue;
                float hullPct = (float)enemy.Hull / enemy.Config.MaxHull;
                if (hullPct < lowestHullPct)
                {
                    lowestHullPct = hullPct;
                    weakest = enemy;
                }
            }

            return weakest;
        }

        /// <summary>
        /// Finds a target using the specified strategy.
        /// </summary>
        public TacticalShipEntity FindTarget(
            TacticalShipEntity ship,
            IEnumerable<TacticalShipEntity> enemies,
            TacticalWeaponConfig weapon,
            TacticalTargetingStrategy strategy)
        {
            switch (strategy)
            {
                case TacticalTargetingStrategy.Nearest:
                    return FindNearestEnemy(ship, enemies);
                case TacticalTargetingStrategy.Weakest:
                    return FindWeakestTarget(ship, enemies);
                case TacticalTargetingStrategy.Strongest:
                    return enemies
                        .Where(e => !e.IsDestroyed)
                        .OrderByDescending(e => e.Hull)
                        .FirstOrDefault();
                case TacticalTargetingStrategy.PlayerSelected:
                    return FindTargetInRange(ship, enemies, weapon);
                default:
                    return null;
            }
        }
    }
}
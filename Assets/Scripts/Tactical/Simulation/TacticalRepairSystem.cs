using System.Linq;

namespace Rebellion.Tactical.Simulation
{
    /// <summary>
    /// Handles ship repair based on damage control rating.
    /// Capital ships repair subsystems first, then hull.
    /// Starfighters do NOT repair in battle.
    /// </summary>
    public sealed class TacticalRepairSystem
    {
        /// <summary>
        /// Updates repair for a ship over deltaTime seconds.
        /// </summary>
        public void Update(TacticalShipEntity ship, float deltaTime)
        {
            if (ship.IsDestroyed) return;
            if (ship.Config.IsStarfighter) return; // Starfighters don't repair
            if (ship.Config.DamageControl <= 0) return;

            float hullPercentage = (float)ship.Hull / ship.Config.MaxHull;

            // Repair rate is reduced when heavily damaged
            float repairModifier = hullPercentage < 0.5f ? 0.5f : 1.0f;
            float repairAmount = ship.Config.DamageControl * repairModifier * deltaTime;

            // Phase 1: Repair subsystems first
            if (repairAmount > 0)
            {
                foreach (var subsystem in ship.Subsystems.Values)
                {
                    if (repairAmount <= 0) break;
                    if (subsystem.Health < subsystem.Config.MaxHealth)
                    {
                        int repaired = subsystem.Repair((int)repairAmount);
                        repairAmount -= repaired;
                    }
                }
            }

            // Phase 2: Repair hull
            if (repairAmount > 0 && ship.Hull < ship.Config.MaxHull)
            {
                int hullRepaired = (int)System.Math.Min(repairAmount, ship.Config.MaxHull - ship.Hull);
                ship.Hull += hullRepaired;
            }
        }
    }
}
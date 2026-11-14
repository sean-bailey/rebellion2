using Unity.Mathematics;

namespace Rebellion.Tactical.Simulation
{
    /// <summary>
    /// Handles ship movement physics. Deterministic, fixed-timestep safe.
    /// </summary>
    public sealed class TacticalMovementSystem
    {
        /// <summary>
        /// Updates a ship's position based on its current speed and orientation.
        /// </summary>
        public void Update(TacticalShipEntity ship, float deltaTime)
        {
            if (ship.IsDestroyed) return;

            // Disabled ships decelerate but don't stop instantly
            if (ship.Status == TacticalShipStatus.Disabled)
            {
                ship.CurrentSpeed = math.Max(0, ship.CurrentSpeed - ship.Config.Speed * 0.5f * deltaTime);
            }

            // Apply drag
            ship.CurrentSpeed = math.Max(0, ship.CurrentSpeed - 1f * deltaTime);

            // Update position
            float3 forward = math.mul(ship.Orientation, float3.forward);
            ship.Position += forward * ship.CurrentSpeed * deltaTime;

            // Update disable timer
            if (ship.DisableTimer > 0)
            {
                ship.DisableTimer -= deltaTime;
                if (ship.DisableTimer <= 0)
                    ship.Status = TacticalShipStatus.Active;
            }

            // Shield regeneration
            int maxShields = ship.EffectiveMaxShields;
            if (ship.Shields < maxShields && maxShields > 0)
            {
                ship.ShieldRegenTimer += deltaTime;
                if (ship.ShieldRegenTimer >= 1f)
                {
                    ship.Shields = math.Min(maxShields, ship.Shields + ship.EffectiveShieldRechargeRate);
                    ship.ShieldRegenTimer = 0;
                }
            }
        }
    }
}
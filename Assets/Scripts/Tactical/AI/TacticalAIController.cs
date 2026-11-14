using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;

namespace Rebellion.Tactical.AI
{
    /// <summary>
    /// AI controller for enemy ships in tactical combat.
    /// Ships move toward targets, maintain weapon range, and fire when ready.
    /// </summary>
    public sealed class TacticalAIController
    {
        private readonly TacticalTargetingSystem _targetingSystem;
        private readonly TacticalWeaponSystem _weaponSystem;

        public TacticalAIController(TacticalTargetingSystem targetingSystem, TacticalWeaponSystem weaponSystem)
        {
            _targetingSystem = targetingingSystem;
            _weaponSystem = weaponSystem;
        }

        /// <summary>
        /// Updates AI behavior for a ship.
        /// </summary>
        public void Update(
            TacticalShipEntity ship,
            IEnumerable<TacticalShipEntity> enemies,
            float deltaTime)
        {
            if (ship.IsDestroyed) return;

            var enemiesList = enemies.ToList();
            var target = SelectTarget(ship, enemiesList);
            if (target == null) return;

            UpdateMovement(ship, target, deltaTime);
            UpdateWeapons(ship, target, deltaTime);
        }

        private TacticalShipEntity SelectTarget(
            TacticalShipEntity ship, IEnumerable<TacticalShipEntity> enemies)
        {
            // Prefer weakest target in range
            var inRange = enemies
                .Where(e => !e.IsDestroyed)
                .Where(e => math.distance(ship.Position, e.Position) <= 1500f);

            var candidates = inRange as List<TacticalShipEntity> ?? inRange.ToList();
            if (candidates.Count == 0)
                return _targetingSystem.FindNearestEnemy(ship, enemies);

            // Pick weakest
            return candidates.OrderBy(e => (float)e.Hull / e.Config.MaxHull).FirstOrDefault();
        }

        private void UpdateMovement(
            TacticalShipEntity ship, TacticalShipEntity target, float deltaTime)
        {
            if (ship.Status == TacticalShipStatus.Disabled) return;

            float distance = math.distance(ship.Position, target.Position);

            // Determine optimal range based on weapons
            float optimalRange = 1000f;
            if (ship.Config.Weapons.Count > 0)
                optimalRange = ship.Config.Weapons.Max(w => w.Range) * 0.7f;

            // Move toward or away from target
            if (distance > optimalRange * 1.2f)
            {
                // Too far, approach
                float3 direction = math.normalize(target.Position - ship.Position);
                ship.Position += direction * ship.EffectiveSpeed * deltaTime;
                ship.Orientation = math.quaternionLookRotation(direction);
            }
            else if (distance < optimalRange * 0.5f)
            {
                // Too close, retreat
                float3 direction = math.normalize(ship.Position - target.Position);
                ship.Position += direction * ship.EffectiveSpeed * 0.5f * deltaTime;
                ship.Orientation = math.quaternionLookRotation(-direction);
            }
            else
            {
                // In optimal range, strafe
                float3 forward = math.normalize(target.Position - ship.Position);
                float3 right = math.cross(forward, float3.up);
                ship.Position += right * ship.EffectiveSpeed * 0.3f * deltaTime;
                ship.Orientation = math.quaternionLookRotation(-forward);
            }
        }

        private void UpdateWeapons(
            TacticalShipEntity ship, TacticalShipEntity target, float deltaTime)
        {
            _weaponSystem.UpdateCooldowns(ship, deltaTime);
        }
    }
}
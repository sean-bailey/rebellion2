using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;

namespace Rebellion.Tactical.Simulation
{
    /// <summary>
    /// Handles weapon firing, cooldowns, and projectile management.
    /// </summary>
    public sealed class TacticalWeaponSystem
    {
        private readonly TacticalTargetingSystem _targetingSystem;
        private readonly TacticalDamageSystem _damageSystem;

        public TacticalWeaponSystem(TacticalTargetingSystem targetingSystem, TacticalDamageSystem damageSystem)
        {
            _targetingSystem = targetingSystem;
            _damageSystem = damageSystem;
        }

        /// <summary>
        /// Attempts to fire all ready weapons at available targets.
        /// Returns shots that were fired this tick.
        /// </summary>
        public List<TacticalWeaponShot> TryFireWeapons(
            TacticalShipEntity ship,
            IEnumerable<TacticalShipEntity> enemies,
            float deltaTime)
        {
            var shots = new List<TacticalWeaponShot>();

            if (ship.IsDestroyed || ship.Status == TacticalShipStatus.Disabled)
                return shots;

            foreach (var weapon in ship.Config.Weapons)
            {
                // Check cooldown
                if (!ship.WeaponCooldowns.TryGetValue(weapon.Type, out float cooldown))
                    continue;

                if (cooldown > 0)
                    continue;

                // Find target in range
                var target = _targetingSystem.FindTargetInRange(ship, enemies, weapon);
                if (target == null)
                    continue;

                // Fire!
                ship.WeaponCooldowns[weapon.Type] = ship.EffectiveWeaponRecharge;

                shots.Add(new TacticalWeaponShot(
                    ship, target, weapon.Type, weapon.Damage,
                    weapon.ProjectileSpeed > 0,
                    ship.Position,
                    math.normalize(target.Position - ship.Position),
                    weapon.ProjectileSpeed));
            }

            return shots;
        }

        /// <summary>
        /// Updates weapon cooldowns for a ship.
        /// </summary>
        public void UpdateCooldowns(TacticalShipEntity ship, float deltaTime)
        {
            foreach (var kvp in ship.WeaponCooldowns)
            {
                if (kvp.Value > 0)
                    ship.WeaponCooldowns[kvp.Key] = System.Math.Max(0, kvp.Value - deltaTime);
            }
        }

        /// <summary>
        /// Updates projectile positions and checks for collisions.
        /// </summary>
        public void UpdateProjectiles(
            List<TacticalProjectile> projectiles,
            IEnumerable<TacticalShipEntity> allShips,
            float deltaTime)
        {
            foreach (var projectile in projectiles)
            {
                if (projectile.IsDestroyed) continue;

                // Move projectile
                projectile.Position += projectile.Velocity * deltaTime;
                projectile.Lifetime -= deltaTime;

                if (projectile.Lifetime <= 0)
                {
                    projectile.IsDestroyed = true;
                    continue;
                }

                // Check collision with target
                if (projectile.Target != null && !projectile.Target.IsDestroyed)
                {
                    float distance = math.distance(projectile.Position, projectile.Target.Position);
                    float hitRadius = projectile.Target.Config.Size * 0.5f;
                    if (distance <= hitRadius)
                    {
                        _damageSystem.ApplyDamage(projectile.Target, projectile.Damage);
                        projectile.IsDestroyed = true;
                    }
                }
            }
        }
    }

    /// <summary>
    /// A weapon shot fired by a ship.
    /// </summary>
    public sealed class TacticalWeaponShot
    {
        public TacticalShipEntity FiringShip { get; }
        public TacticalShipEntity Target { get; }
        public TacticalWeaponType WeaponType { get; }
        public int Damage { get; }
        public bool IsProjectile { get; }
        public float3 StartPosition { get; }
        public float3 Direction { get; }
        public float ProjectileSpeed { get; }

        public TacticalWeaponShot(
            TacticalShipEntity firingShip,
            TacticalShipEntity target,
            TacticalWeaponType weaponType,
            int damage,
            bool isProjectile,
            float3 startPosition,
            float3 direction,
            float projectileSpeed)
        {
            FiringShip = firingShip;
            Target = target;
            WeaponType = weaponType;
            Damage = damage;
            IsProjectile = isProjectile;
            StartPosition = startPosition;
            Direction = direction;
            ProjectileSpeed = projectileSpeed;
        }
    }

    /// <summary>
    /// A projectile in flight.
    /// </summary>
    public sealed class TacticalProjectile
    {
        public float3 Position { get; set; }
        public float3 Velocity { get; }
        public TacticalShipEntity FiringShip { get; }
        public TacticalShipEntity Target { get; }
        public TacticalWeaponType WeaponType { get; }
        public int Damage { get; }
        public float Lifetime { get; set; }
        public bool IsDestroyed { get; set; }

        public TacticalProjectile(TacticalWeaponShot shot)
        {
            Position = shot.StartPosition;
            Velocity = shot.Direction * shot.ProjectileSpeed;
            FiringShip = shot.FiringShip;
            Target = shot.Target;
            WeaponType = shot.WeaponType;
            Damage = shot.Damage;
            Lifetime = 30f; // Max 30 seconds flight time
            IsDestroyed = false;
        }
    }
}
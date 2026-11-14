using System.Collections.Generic;
using Unity.Mathematics;

namespace Rebellion.Tactical.Simulation
{
    /// <summary>
    /// Represents a single ship in tactical combat. Tracks position, orientation,
    /// hull/shield health, subsystem states, and weapon cooldowns.
    /// Pure C# — no Unity dependencies.
    /// </summary>
    public sealed class TacticalShipEntity
    {
        /// <summary>Unique ship identifier.</summary>
        public string Id { get; }

        /// <summary>Ship configuration.</summary>
        public TacticalShipConfig Config { get; }

        /// <summary>Side this ship belongs to.</summary>
        public TacticalShipSide Side { get; }

        /// <summary>Current operational status.</summary>
        public TacticalShipStatus Status { get; private set; }

        /// <summary>World position.</summary>
        public float3 Position { get; set; }

        /// <summary>World orientation.</summary>
        public quaternion Orientation { get; set; }

        /// <summary>Current velocity vector.</summary>
        public float3 Velocity { get; private set; }

        /// <summary>Current speed magnitude.</summary>
        public float CurrentSpeed { get; private set; }

        /// <summary>Current hull strength.</summary>
        public int Hull { get; private set; }

        /// <summary>Current shield strength.</summary>
        public int Shields { get; private set; }

        /// <summary>Remaining disable timer (ion damage).</summary>
        public float DisableTimer { get; private set; }

        /// <summary>Shield regeneration timer.</summary>
        public float ShieldRegenTimer { get; private set; }

        /// <summary>Subsystem states (capital ships only).</summary>
        public Dictionary<TacticalSubsystemType, TacticalSubsystemState> Subsystems { get; }

        /// <summary>Weapon cooldown timers.</summary>
        public Dictionary<TacticalWeaponType, float> WeaponCooldowns { get; }

        /// <summary>Whether this ship is destroyed.</summary>
        public bool IsDestroyed => Hull <= 0;

        public TacticalShipEntity(string id, TacticalShipConfig config, TacticalShipSide side)
        {
            Id = id;
            Config = config;
            Side = side;
            Status = TacticalShipStatus.Active;
            Hull = config.MaxHull;
            Shields = config.MaxShields;
            Orientation = quaternion.identity;

            Subsystems = new Dictionary<TacticalSubsystemType, TacticalSubsystemState>();
            foreach (var kvp in config.Subsystems)
                Subsystems[kvp.Key] = new TacticalSubsystemState(kvp.Value);

            WeaponCooldowns = new Dictionary<TacticalWeaponType, float>();
            foreach (var weapon in config.Weapons)
                WeaponCooldowns[weapon.Type] = 0f;
        }

        /// <summary>
        /// Effective speed after subsystem damage.
        /// </summary>
        public float EffectiveSpeed
        {
            get
            {
                if (!Subsystems.TryGetValue(TacticalSubsystemType.SublightEngines, out var engines))
                    return Config.Speed;
                return Config.Speed * engines.OperationalPercentage;
            }
        }

        /// <summary>
        /// Effective agility after subsystem damage.
        /// </summary>
        public float EffectiveAgility
        {
            get
            {
                if (!Subsystems.TryGetValue(TacticalSubsystemType.SublightEngines, out var engines))
                    return Config.Agility;
                return Config.Agility * engines.OperationalPercentage;
            }
        }

        /// <summary>
        /// Effective max shields after shield generator damage.
        /// </summary>
        public int EffectiveMaxShields
        {
            get
            {
                if (!Subsystems.TryGetValue(TacticalSubsystemType.ShieldGenerator, out var generator))
                    return Config.MaxShields;
                return (int)(Config.MaxShields * generator.OperationalPercentage);
            }
        }

        /// <summary>
        /// Effective shield recharge rate after shield generator damage.
        /// </summary>
        public int EffectiveShieldRechargeRate
        {
            get
            {
                if (!Subsystems.TryGetValue(TacticalSubsystemType.ShieldGenerator, out var generator))
                    return Config.ShieldRechargeRate;
                return (int)(Config.ShieldRechargeRate * generator.OperationalPercentage);
            }
        }

        /// <summary>
        /// Effective weapon recharge time after weapons subsystem damage.
        /// Higher value = slower firing.
        /// </summary>
        public float EffectiveWeaponRecharge
        {
            get
            {
                if (!Subsystems.TryGetValue(TacticalSubsystemType.Weapons, out var weapons))
                    return Config.WeaponRecharge;
                float pct = weapons.OperationalPercentage;
                return pct > 0 ? Config.WeaponRecharge / pct : float.MaxValue;
            }
        }

        /// <summary>
        /// Whether the hyperdrive is operational (required for hyperspace escape).
        /// </summary>
        public bool HyperdriveOperational
        {
            get
            {
                if (!Subsystems.TryGetValue(TacticalSubsystemType.Hyperdrive, out var hyperdrive))
                    return true;
                return hyperdrive.Status != TacticalSubsystemStatus.Offline;
            }
        }

        /// <summary>
        /// Whether the gravity well projector is active.
        /// </summary>
        public bool GravityWellActive
        {
            get
            {
                if (!Subsystems.TryGetValue(TacticalSubsystemType.GravityWellProjector, out var gw))
                    return false;
                return gw.Status != TacticalSubsystemStatus.Offline;
            }
        }

        /// <summary>
        /// Effective tractor beam power after subsystem damage.
        /// </summary>
        public int EffectiveTractorBeamPower
        {
            get
            {
                if (!Subsystems.TryGetValue(TacticalSubsystemType.TractorBeam, out var beam))
                    return Config.TractorBeamPower;
                return (int)(Config.TractorBeamPower * beam.OperationalPercentage);
            }
        }

        /// <summary>
        /// Updates ship timers and status.
        /// </summary>
        public void Update(float deltaTime)
        {
            if (IsDestroyed) return;

            // Update disable timer
            if (DisableTimer > 0)
            {
                DisableTimer -= deltaTime;
                if (DisableTimer <= 0)
                    Status = TacticalShipStatus.Active;
            }

            // Shield regeneration
            int maxShields = EffectiveMaxShields;
            if (Shields < maxShields && maxShields > 0)
            {
                ShieldRegenTimer += deltaTime;
                if (ShieldRegenTimer >= 1f)
                {
                    Shields = System.Math.Min(maxShields, Shields + EffectiveShieldRechargeRate);
                    ShieldRegenTimer = 0;
                }
            }
        }

        /// <summary>
        /// Applies thrust in the ship's forward direction.
        /// </summary>
        public void ApplyThrust(float amount)
        {
            if (IsDestroyed || Status == TacticalShipStatus.Disabled) return;
            CurrentSpeed = System.Math.Min(EffectiveSpeed, CurrentSpeed + amount * EffectiveSpeed);
        }

        /// <summary>
        /// Applies orientation changes.
        /// </summary>
        public void ApplyTurn(float yaw, float pitch, float roll)
        {
            if (IsDestroyed) return;
            Orientation = math.mul(Orientation, quaternion.Euler(yaw, pitch, roll));
        }

        /// <summary>
        /// Disables the ship for a duration (ion damage).
        /// </summary>
        public void Disable(float duration)
        {
            if (IsDestroyed) return;
            Status = TacticalShipStatus.Disabled;
            DisableTimer = duration;
        }

        /// <summary>
        /// Takes hull/shield damage directly.
        /// </summary>
        public void TakeDamage(int hullDamage, int shieldDamage)
        {
            if (IsDestroyed) return;

            if (shieldDamage > 0 && Shields > 0)
            {
                int absorbed = System.Math.Min(Shields, shieldDamage);
                Shields -= absorbed;
                hullDamage += shieldDamage - absorbed;
            }

            Hull = System.Math.Max(0, Hull - hullDamage);
            if (Hull <= 0)
                Status = TacticalShipStatus.Destroyed;
        }
    }
}
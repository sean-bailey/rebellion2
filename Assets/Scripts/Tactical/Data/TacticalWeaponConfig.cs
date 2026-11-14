namespace Rebellion.Tactical.Data
{
    /// <summary>
    /// Immutable configuration for a single weapon type in tactical combat.
    /// Maps strategic-layer weapon stats to tactical combat parameters.
    /// </summary>
    public sealed class TacticalWeaponConfig
    {
        /// <summary>Weapon type.</summary>
        public TacticalWeaponType Type { get; }

        /// <summary>Damage per hit.</summary>
        public int Damage { get; }

        /// <summary>Effective range in world units.</summary>
        public float Range { get; }

        /// <summary>Seconds between shots.</summary>
        public float RechargeTime { get; }

        /// <summary>Projectile speed in units/sec. Zero for instant-hit weapons.</summary>
        public float ProjectileSpeed { get; }

        /// <summary>Disable duration in seconds. Zero for non-ion weapons.</summary>
        public float DisableDuration { get; }

        /// <summary>Accuracy factor 0.0 to 1.0.</summary>
        public float Accuracy { get; }

        public TacticalWeaponConfig(
            TacticalWeaponType type,
            int damage,
            float range,
            float rechargeTime,
            float projectileSpeed,
            float disableDuration,
            float accuracy
        )
        {
            Type = type;
            Damage = damage;
            Range = range;
            RechargeTime = rechargeTime;
            ProjectileSpeed = projectileSpeed;
            DisableDuration = disableDuration;
            Accuracy = accuracy;
        }

        /// <summary>
        /// Creates a laser cannon config from strategic stats.
        /// </summary>
        /// <param name="power">Weapon power from strategic data.</param>
        /// <param name="range">Weapon range from strategic data.</param>
        public static TacticalWeaponConfig CreateLaserCannon(int power, int range)
        {
            return new TacticalWeaponConfig(
                TacticalWeaponType.LaserCannon,
                power,
                range * 10f,
                2.5f,
                0f, // Instant hit
                0f,
                0.95f
            );
        }

        /// <summary>
        /// Creates an ion cannon config from strategic stats.
        /// </summary>
        /// <param name="power">Weapon power from strategic data.</param>
        /// <param name="range">Weapon range from strategic data.</param>
        public static TacticalWeaponConfig CreateIonCannon(int power, int range)
        {
            return new TacticalWeaponConfig(
                TacticalWeaponType.IonCannon,
                power,
                range * 10f,
                4.0f,
                0f, // Instant hit
                5.0f, // Disable duration
                0.85f
            );
        }

        /// <summary>
        /// Creates a torpedo config from strategic stats.
        /// </summary>
        /// <param name="power">Weapon power from strategic data.</param>
        /// <param name="range">Weapon range from strategic data.</param>
        public static TacticalWeaponConfig CreateTorpedo(int power, int range)
        {
            return new TacticalWeaponConfig(
                TacticalWeaponType.Torpedo,
                power,
                range * 10f,
                6.0f,
                150f, // Projectile speed
                0f,
                0.7f
            );
        }
    }
}
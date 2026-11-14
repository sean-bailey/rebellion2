namespace Rebellion.Tactical.Simulation
{
    /// <summary>
    /// Tracks the current health of a single subsystem on a ship.
    /// Health ranges from 0 (offline) to Config.MaxHealth (fully operational).
    /// </summary>
    public sealed class TacticalSubsystemState
    {
        /// <summary>Configuration for this subsystem.</summary>
        public TacticalSubsystemConfig Config { get; }

        /// <summary>Current health of the subsystem.</summary>
        public int Health { get; set; }

        public TacticalSubsystemState(TacticalSubsystemConfig config)
        {
            Config = config;
            Health = config.MaxHealth;
        }

        /// <summary>
        /// Returns the operational percentage (0.0 to 1.0).
        /// </summary>
        public float OperationalPercentage =>
            Config.MaxHealth > 0 ? (float)Health / Config.MaxHealth : 0f;

        /// <summary>
        /// Current status derived from health.
        /// </summary>
        public TacticalSubsystemStatus Status
        {
            get
            {
                if (Health <= 0) return TacticalSubsystemStatus.Offline;
                if (Health < Config.MaxHealth) return TacticalSubsystemStatus.Damaged;
                return TacticalSubsystemStatus.Operational;
            }
        }

        /// <summary>
        /// Applies damage to this subsystem. Returns the amount of damage absorbed.
        /// </summary>
        public int ApplyDamage(int damage)
        {
            int absorbed = System.Math.Min(Health, damage);
            Health -= absorbed;
            return absorbed;
        }

        /// <summary>
        /// Repairs this subsystem. Returns the amount of health restored.
        /// </summary>
        public int Repair(int amount)
        {
            int restored = System.Math.Min(Config.MaxHealth - Health, amount);
            Health += restored;
            return restored;
        }
    }
}
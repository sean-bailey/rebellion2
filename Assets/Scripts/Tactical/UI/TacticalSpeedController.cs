namespace Rebellion.Tactical.UI
{
    /// <summary>
    /// Controls battle speed (real-time, fast, very fast, paused).
    /// </summary>
    public sealed class TacticalSpeedController
    {
        /// <summary>Current speed setting.</summary>
        public TacticalBattleSpeed CurrentSpeed { get; private set; }

        /// <summary>Time scale multiplier.</summary>
        public float TimeScale { get; private set; }

        public TacticalSpeedController()
        {
            CurrentSpeed = TacticalBattleSpeed.RealTime;
            TimeScale = 1f;
        }

        /// <summary>
        /// Sets the battle speed.
        /// </summary>
        public void SetSpeed(TacticalBattleSpeed speed)
        {
            CurrentSpeed = speed;
            switch (speed)
            {
                case TacticalBattleSpeed.Paused:
                    TimeScale = 0f;
                    break;
                case TacticalBattleSpeed.RealTime:
                    TimeScale = 1f;
                    break;
                case TacticalBattleSpeed.Fast:
                    TimeScale = 2f;
                    break;
                case TacticalBattleSpeed.VeryFast:
                    TimeScale = 4f;
                    break;
            }
        }

        /// <summary>
        /// Cycles through speed settings.
        /// </summary>
        public void CycleSpeed()
        {
            switch (CurrentSpeed)
            {
                case TacticalBattleSpeed.Paused:
                    SetSpeed(TacticalBattleSpeed.RealTime);
                    break;
                case TacticalBattleSpeed.RealTime:
                    SetSpeed(TacticalBattleSpeed.Fast);
                    break;
                case TacticalBattleSpeed.Fast:
                    SetSpeed(TacticalBattleSpeed.VeryFast);
                    break;
                case TacticalBattleSpeed.VeryFast:
                    SetSpeed(TacticalBattleSpeed.Paused);
                    break;
            }
        }
    }
}
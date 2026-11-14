using System.Collections.Generic;

namespace Rebellion.Tactical.Data
{
    /// <summary>
    /// Result of a tactical battle, tracking all losses and damage.
    /// </summary>
    public sealed class TacticalBattleResult
    {
        /// <summary>Outcome of the battle.</summary>
        public TacticalBattleOutcome Outcome { get; }

        /// <summary>Ships destroyed during the battle.</summary>
        public List<DestroyedShipRecord> DestroyedShips { get; }

        /// <summary>Ships that took damage but survived.</summary>
        public List<DamagedShipRecord> DamagedShips { get; }

        /// <summary>Subsystem damage records.</summary>
        public List<SubsystemDamageRecord> SubsystemDamage { get; }

        /// <summary>Squadron fighter losses.</summary>
        public List<SquadronLossRecord> SquadronLosses { get; }

        /// <summary>Ships that withdrew from battle.</summary>
        public List<WithdrawnShipRecord> WithdrawnShips { get; }

        /// <summary>Duration of the battle in seconds.</summary>
        public float DurationSeconds { get; set; }

        public TacticalBattleResult(TacticalBattleOutcome outcome)
        {
            Outcome = outcome;
            DestroyedShips = new List<DestroyedShipRecord>();
            DamagedShips = new List<DamagedShipRecord>();
            SubsystemDamage = new List<SubsystemDamageRecord>();
            SquadronLosses = new List<SquadronLossRecord>();
            WithdrawnShips = new List<WithdrawnShipRecord>();
        }

        /// <summary>Records a destroyed ship.</summary>
        public void RecordDestroyedShip(string shipId, TacticalShipSide side)
        {
            DestroyedShips.Add(new DestroyedShipRecord(shipId, side));
        }

        /// <summary>Records damage to a surviving ship.</summary>
        public void RecordDamagedShip(string shipId, TacticalShipSide side, int hullDamage, int shieldDamage)
        {
            DamagedShips.Add(new DamagedShipRecord(shipId, side, hullDamage, shieldDamage));
        }

        /// <summary>Records damage to a specific subsystem.</summary>
        public void RecordSubsystemDamage(
            string shipId,
            TacticalShipSide side,
            TacticalSubsystemType subsystem,
            int healthLost)
        {
            SubsystemDamage.Add(new SubsystemDamageRecord(shipId, side, subsystem, healthLost));
        }

        /// <summary>Records fighter losses from a squadron.</summary>
        public void RecordSquadronFighterLosses(string squadronId, TacticalShipSide side, int fightersLost)
        {
            SquadronLosses.Add(new SquadronLossRecord(squadronId, side, fightersLost));
        }

        /// <summary>Records a ship that withdrew.</summary>
        public void RecordWithdrawnShip(string shipId, TacticalShipSide side)
        {
            WithdrawnShips.Add(new WithdrawnShipRecord(shipId, side));
        }
    }

    /// <summary>Record of a destroyed ship.</summary>
    public sealed class DestroyedShipRecord
    {
        public string ShipId { get; }
        public TacticalShipSide Side { get; }

        public DestroyedShipRecord(string shipId, TacticalShipSide side)
        {
            ShipId = shipId;
            Side = side;
        }
    }

    /// <summary>Record of damage to a surviving ship.</summary>
    public sealed class DamagedShipRecord
    {
        public string ShipId { get; }
        public TacticalShipSide Side { get; }
        public int HullDamage { get; }
        public int ShieldDamage { get; }

        public DamagedShipRecord(string shipId, TacticalShipSide side, int hullDamage, int shieldDamage)
        {
            ShipId = shipId;
            Side = side;
            HullDamage = hullDamage;
            ShieldDamage = shieldDamage;
        }
    }

    /// <summary>Record of damage to a specific subsystem.</summary>
    public sealed class SubsystemDamageRecord
    {
        public string ShipId { get; }
        public TacticalShipSide Side { get; }
        public TacticalSubsystemType Subsystem { get; }
        public int HealthLost { get; }

        public SubsystemDamageRecord(
            string shipId, TacticalShipSide side,
            TacticalSubsystemType subsystem, int healthLost)
        {
            ShipId = shipId;
            Side = side;
            Subsystem = subsystem;
            HealthLost = healthLost;
        }
    }

    /// <summary>Record of fighter losses from a squadron.</summary>
    public sealed class SquadronLossRecord
    {
        public string SquadronId { get; }
        public TacticalShipSide Side { get; }
        public int FightersLost { get; }

        public SquadronLossRecord(string squadronId, TacticalShipSide side, int fightersLost)
        {
            SquadronId = squadronId;
            Side = side;
            FightersLost = fightersLost;
        }
    }

    /// <summary>Record of a ship that withdrew.</summary>
    public sealed class WithdrawnShipRecord
    {
        public string ShipId { get; }
        public TacticalShipSide Side { get; }

        public WithdrawnShipRecord(string shipId, TacticalShipSide side)
        {
            ShipId = shipId;
            Side = side;
        }
    }
}
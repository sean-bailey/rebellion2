using UnityEngine;
using Rebellion.Tactical.Data;

namespace Rebellion.Tactical.Presentation
{
    /// <summary>
    /// Entry point for tactical battles from the strategic layer.
    /// Attaches to a GameObject in the tactical battle scene.
    /// Handles scene setup and battle initialization.
    /// </summary>
    public sealed class TacticalBattleEntry : MonoBehaviour
    {
        private TacticalBattleSceneBuilder _sceneBuilder;
        private TacticalBattleCoordinator _coordinator;
        private ITacticalBattleFlowManager _flowManager;

        private void Start()
        {
            // Get or create components
            _sceneBuilder = GetComponent<TacticalBattleSceneBuilder>();
            if (_sceneBuilder == null)
                _sceneBuilder = gameObject.AddComponent<TacticalBattleSceneBuilder>();

            _coordinator = GetComponent<TacticalBattleCoordinator>();
            if (_coordinator == null)
                _coordinator = gameObject.AddComponent<TacticalBattleCoordinator>();

            // Find the flow manager (persists across scenes) via the GameObject name
            var flowManagerGO = GameObject.Find("TacticalBattleFlowManager");
            _flowManager = flowManagerGO?.GetComponent<ITacticalBattleFlowManager>();

            // Wire up battle completion callback
            if (_coordinator != null)
            {
                _coordinator.BattleComplete += OnBattleComplete;
            }

            // Notify the flow manager that the scene is loaded
            if (_flowManager != null)
            {
                _flowManager.OnTacticalSceneLoaded();
            }
        }

        /// <summary>
        /// Starts a tactical battle with the given config.
        /// This is called from the strategic layer when tactical combat is chosen.
        /// </summary>
        public void StartTacticalBattle(TacticalBattleConfig config)
        {
            _sceneBuilder?.BuildScene(config);
        }

        private void OnBattleComplete(TacticalBattleResult result)
        {
            if (_flowManager != null)
            {
                _flowManager.OnTacticalBattleComplete(result);
            }
        }

        /// <summary>
        /// Creates a test battle config for testing purposes.
        /// </summary>
        public static TacticalBattleConfig CreateTestBattleConfig()
        {
            var attackerShips = new System.Collections.Generic.List<TacticalShipConfig>
            {
                TacticalShipConfig.CreateCapitalShip("Star Destroyer", 2000, 1000, 30, 15,
                    new System.Collections.Generic.List<TacticalWeaponConfig>
                    {
                        TacticalWeaponConfig.CreateLaserCannon(100, 50),
                        TacticalWeaponConfig.CreateIonCannon(50, 40),
                    }),
                TacticalShipConfig.CreateCapitalShip("Cruiser", 1500, 800, 25, 20,
                    new System.Collections.Generic.List<TacticalWeaponConfig>
                    {
                        TacticalWeaponConfig.CreateLaserCannon(80, 45),
                    }),
            };

            var defenderShips = new System.Collections.Generic.List<TacticalShipConfig>
            {
                TacticalShipConfig.CreateCapitalShip("Imperial Star Destroyer", 2000, 1000, 30, 15,
                    new System.Collections.Generic.List<TacticalWeaponConfig>
                    {
                        TacticalWeaponConfig.CreateLaserCannon(100, 50),
                        TacticalWeaponConfig.CreateIonCannon(50, 40),
                    }),
                TacticalShipConfig.CreateCapitalShip("Mandalorian Cruiser", 1200, 600, 35, 25,
                    new System.Collections.Generic.List<TacticalWeaponConfig>
                    {
                        TacticalWeaponConfig.CreateLaserCannon(70, 40),
                        TacticalWeaponConfig.CreateTorpedo(150, 60),
                    }),
            };

            return new TacticalBattleConfig(
                attackerShips,
                defenderShips,
                "Tatooine",
                System.DateTime.Now.Second * 1000);
        }
    }
}

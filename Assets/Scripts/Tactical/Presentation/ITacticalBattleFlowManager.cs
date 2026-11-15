using Rebellion.Tactical.Data;

namespace Rebellion.Tactical.Presentation
{
    /// <summary>
    /// Interface for the tactical battle flow manager.
    /// Implemented by TacticalBattleFlowManager in GameAssembly to avoid
    /// circular assembly dependencies.
    /// </summary>
    public interface ITacticalBattleFlowManager
    {
        /// <summary>
        /// Called when the tactical battle scene is loaded.
        /// Initializes the battle with the stored config.
        /// </summary>
        void OnTacticalSceneLoaded();

        /// <summary>
        /// Called when the tactical battle ends.
        /// Returns to the strategic scene and applies results.
        /// </summary>
        void OnTacticalBattleComplete(TacticalBattleResult result);
    }
}

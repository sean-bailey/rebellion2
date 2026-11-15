using UnityEditor;
using UnityEngine;

namespace Rebellion.Tactical.Presentation
{
    /// <summary>
    /// Editor tool to create the TacticalBattle scene programmatically.
    /// Run via Tools > Tactical > Create Tactical Battle Scene.
    /// </summary>
    public static class TacticalBattleSceneCreator
    {
        private const string ScenePath = "Assets/Scenes/TacticalBattle.unity";

        [MenuItem("Tools/Tactical/Create Tactical Battle Scene")]
        public static void CreateScene()
        {
            // Create scene directory if it doesn't exist
            if (!System.IO.Directory.Exists("Assets/Scenes"))
            {
                System.IO.Directory.CreateDirectory("Assets/Scenes");
            }

            // Create new scene
            var scene = UnityEngine.SceneManagement.SceneManager.CreateScene("TacticalBattle");

            // Create game manager object
            var gameManager = new GameObject("TacticalBattleManager");

            // Add TacticalBattleEntry component
            gameManager.AddComponent<TacticalBattleEntry>();

            // Add TacticalBattleSceneBuilder component
            gameManager.AddComponent<TacticalBattleSceneBuilder>();

            // Add TacticalBattleCoordinator component
            gameManager.AddComponent<TacticalBattleCoordinator>();

            // Create main camera
            var cameraGO = new GameObject("Main Camera");
            cameraGO.AddComponent<Camera>();
            cameraGO.AddComponent<TacticalCameraController>();

            // Create directional light
            var lightGO = new GameObject("Directional Light");
            var light = lightGO.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.5f;
            light.color = new Color(1f, 0.95f, 0.85f);
            lightGO.transform.rotation = Quaternion.Euler(-30f, 45f, 0);

            // Create ambient light
            var ambientGO = new GameObject("Ambient Light");
            var ambient = ambientGO.AddComponent<Light>();
            ambient.type = LightType.Ambient;
            ambient.intensity = 0.3f;
            ambient.color = new Color(0.5f, 0.5f, 0.7f);

            // Save scene
            UnityEngine.SceneManagement.SceneManager.SaveScene(ScenePath);
            UnityEngine.SceneManagement.SceneManager.OpenScene(ScenePath);

            Debug.Log($"Tactical battle scene created at {ScenePath}");
        }
    }
}
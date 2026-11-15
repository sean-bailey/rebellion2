using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Rebellion.Tactical.Data;
using Rebellion.Tactical.Simulation;

namespace Rebellion.Tactical.Presentation
{
    /// <summary>
    /// Programmatically builds the tactical battle scene content.
    /// Creates starfield, lighting, camera, UI, and ship renderers.
    /// </summary>
    public sealed class TacticalBattleSceneBuilder : MonoBehaviour
    {
        [Header("Scene Settings")]
        [SerializeField] private float _battlefieldRadius = 5000f;
        [SerializeField] private int _starCount = 2000;
        [SerializeField] private Color _spaceColor = new Color(0.02f, 0.02f, 0.05f);

        private TacticalBattleCoordinator _coordinator;
        private TacticalShipRenderer[] _shipRenderers;
        private TacticalBattleUI _battleUI;
        private GameObject _shipContainer;
        private GameObject _projectileContainer;

        /// <summary>
        /// Builds the tactical battle scene and starts the battle.
        /// </summary>
        public void BuildScene(TacticalBattleConfig config)
        {
            // Clear existing scene content
            ClearScene();

            // Set background color
            var camera = Camera.main;
            if (camera != null)
            {
                camera.backgroundColor = _spaceColor;
                camera.clearFlags = CameraClearFlags.SolidColor;
            }

            // Create coordinator if not present
            _coordinator = GetComponent<TacticalBattleCoordinator>();
            if (_coordinator == null)
                _coordinator = gameObject.AddComponent<TacticalBattleCoordinator>();

            // Create ship containers
            _shipContainer = new GameObject("Ships");
            _projectileContainer = new GameObject("Projectiles");

            // Create starfield
            CreateStarfield();

            // Create lighting
            CreateLighting();

            // Create UI
            _battleUI = TacticalBattleUI.CreateUI();

            // Create ship renderers
            CreateShipRenderers(config);

            // Start the battle
            _coordinator.StartBattle(config);
        }

        /// <summary>
        /// Updates all ship renderers to match simulation state.
        /// </summary>
        public void UpdateShipRenderers()
        {
            if (_coordinator == null || _coordinator.State != TacticalBattleStateEnum.Running)
                return;

            var simulation = _coordinator.Simulation;
            if (simulation == null)
                return;

            // Update all ships
            for (int i = 0; i < simulation.AllShips.Count; i++)
            {
                var ship = simulation.AllShips[i];
                if (i < _shipRenderers.Length)
                {
                    _shipRenderers[i].UpdateShip(ship);
                }
            }
        }

        /// <summary>
        /// Gets the coordinator for external access.
        /// </summary>
        public TacticalBattleCoordinator Coordinator => _coordinator;

        private void ClearScene()
        {
            // Remove existing ship renderers
            if (_shipContainer != null)
                Destroy(_shipContainer);
            if (_projectileContainer != null)
                Destroy(_projectileContainer);
        }

        private void CreateStarfield()
        {
            var starfield = new GameObject("Starfield");
            var positions = new Vector3[_starCount];
            var colors = new Color[_starCount];

            for (int i = 0; i < _starCount; i++)
            {
                // Random position on a sphere
                var direction = Random.insideUnitSphere * 10000f;
                positions[i] = direction;
                colors[i] = new Color(1f, 1f, 1f, Random.Range(0.3f, 1f));
            }

            var mesh = new Mesh();
            mesh.vertices = positions;
            mesh.colors = colors;

            var meshFilter = starfield.AddComponent<MeshFilter>();
            meshFilter.mesh = mesh;

            var material = new Material(Shader.Find("Sprites/Default"));
            material.SetColor("_Color", Color.white);
            var meshRenderer = starfield.AddComponent<MeshRenderer>();
            meshRenderer.material = material;
            meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            meshRenderer.receiveShadows = false;
        }

        private void CreateLighting()
        {
            // Ambient light
            var ambient = new GameObject("AmbientLight");
            ambient.AddComponent<Light>().type = LightType.Ambient;
            ambient.GetComponent<Light>().intensity = 0.3f;
            ambient.GetComponent<Light>().color = new Color(0.5f, 0.5f, 0.7f);

            // Directional light (sun)
            var sun = new GameObject("Sun");
            sun.AddComponent<Light>().type = LightType.Directional;
            sun.GetComponent<Light>().intensity = 1.5f;
            sun.GetComponent<Light>().color = new Color(1f, 0.95f, 0.85f);
            sun.transform.rotation = Quaternion.Euler(-30f, 45f, 0);
        }

        private void CreateShipRenderers(TacticalBattleConfig config)
        {
            var allConfigs = new List<TacticalShipConfig>();
            allConfigs.AddRange(config.AttackerShips);
            allConfigs.AddRange(config.DefenderShips);

            _shipRenderers = new TacticalShipRenderer[allConfigs.Count];

            for (int i = 0; i < allConfigs.Count; i++)
            {
                var rendererGO = new GameObject($"Ship_{i}");
                rendererGO.transform.SetParent(_shipContainer.transform);
                _shipRenderers[i] = rendererGO.AddComponent<TacticalShipRenderer>();
                _shipRenderers[i].Initialize(allConfigs[i], i < config.AttackerShips.Count);
            }
        }
    }
}
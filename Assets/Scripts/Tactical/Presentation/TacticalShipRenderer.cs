using UnityEngine;
using Rebellion.Tactical.Data;
using Rebellion.Tactical.Simulation;

namespace Rebellion.Tactical.Presentation
{
    /// <summary>
    /// Renders a tactical ship entity as a 3D object.
    /// Uses simple geometry for ships (box for capital ships, small box for fighters).
    /// Includes health bar and selection highlight.
    /// </summary>
    public sealed class TacticalShipRenderer : MonoBehaviour
    {
        private TacticalShipConfig _config;
        private bool _isAttacker;
        private TacticalShipEntity _shipEntity;

        private MeshRenderer _shipMesh;
        private MeshRenderer _healthBarMesh;
        private MeshRenderer _selectionRing;
        private Material _shipMaterial;
        private Material _healthBarMaterial;
        private Transform _healthBarFillTransform;
        private Vector3 _healthBarFillBaseScale;

        private static readonly Color AttackerColor = new Color(0.2f, 0.5f, 1f);
        private static readonly Color DefenderColor = new Color(1f, 0.3f, 0.2f);
        private static readonly Color HealthGreen = new Color(0.2f, 1f, 0.2f);
        private static readonly Color HealthRed = new Color(1f, 0.2f, 0.2f);
        private static readonly Color SelectionColor = new Color(1f, 1f, 0.2f, 0.5f);

        /// <summary>
        /// Initializes the renderer with ship config.
        /// </summary>
        public void Initialize(TacticalShipConfig config, bool isAttacker)
        {
            _config = config;
            _isAttacker = isAttacker;

            // Create ship mesh
            CreateShipMesh();

            // Create health bar
            CreateHealthBar();

            // Create selection ring
            CreateSelectionRing();
        }

        /// <summary>
        /// Updates the renderer to match the ship entity state.
        /// </summary>
        public void UpdateShip(TacticalShipEntity ship)
        {
            _shipEntity = ship;

            // Update position and orientation
            transform.position = new Vector3(ship.Position.x, ship.Position.y, ship.Position.z);
            transform.rotation = new Quaternion(ship.Orientation.x, ship.Orientation.y, ship.Orientation.z, ship.Orientation.w);

            // Update health bar
            UpdateHealthBar(ship);

            // Hide destroyed ships
            transform.gameObject.SetActive(!ship.IsDestroyed);
        }

        private void CreateShipMesh()
        {
            var shipGO = new GameObject("ShipBody");
            shipGO.transform.SetParent(transform);

            // Create simple ship geometry based on type
            if (_config.IsStarfighter)
            {
                CreateFighterMesh(shipGO);
            }
            else
            {
                CreateCapitalShipMesh(shipGO);
            }

            // Set color based on side
            _shipMaterial = new Material(Shader.Find("Standard"));
            _shipMaterial.SetColor("_Color", _isAttacker ? AttackerColor : DefenderColor);
            _shipMaterial.SetFloat("_Metallic", 0.5f);
            _shipMaterial.SetFloat("_Glossiness", 0.3f);
            _shipMesh = shipGO.GetComponent<MeshRenderer>();
            _shipMesh.material = _shipMaterial;
        }

        private void CreateFighterMesh(GameObject parent)
        {
            var mesh = CreateBoxMesh(10f, 5f, 15f);
            var meshFilter = parent.AddComponent<MeshFilter>();
            meshFilter.mesh = mesh;
        }

        private void CreateCapitalShipMesh(GameObject parent)
        {
            var mesh = CreateBoxMesh(50f, 30f, 100f);
            var meshFilter = parent.AddComponent<MeshFilter>();
            meshFilter.mesh = mesh;
        }

        private static Mesh CreateBoxMesh(float x, float y, float z)
        {
            var mesh = new Mesh();
            mesh.vertices = new Vector3[]
            {
                new Vector3(-x/2, -y/2, -z/2), new Vector3(x/2, -y/2, -z/2),
                new Vector3(x/2, -y/2, z/2), new Vector3(-x/2, -y/2, z/2),
                new Vector3(-x/2, y/2, -z/2), new Vector3(x/2, y/2, -z/2),
                new Vector3(x/2, y/2, z/2), new Vector3(-x/2, y/2, z/2)
            };
            mesh.triangles = new int[]
            {
                0, 1, 2, 0, 2, 3, // Bottom
                4, 5, 6, 4, 6, 7, // Top
                0, 1, 5, 0, 5, 4, // Front
                2, 3, 7, 2, 7, 6, // Back
                0, 3, 7, 0, 7, 4, // Left
                1, 2, 6, 1, 6, 5  // Right
            };
            mesh.RecalculateNormals();
            return mesh;
        }

        private void CreateHealthBar()
        {
            var healthBarGO = new GameObject("HealthBar");
            healthBarGO.transform.SetParent(transform);
            healthBarGO.transform.position = new Vector3(0, _config.Size * 0.6f, 0);

            // Health bar background
            var backgroundGO = new GameObject("HealthBarBG");
            backgroundGO.transform.SetParent(healthBarGO.transform);
            backgroundGO.transform.localScale = new Vector3(_config.Size * 0.8f, 2f, 2f);
            var bgMesh = CreateCubeMesh();
            backgroundGO.AddComponent<MeshFilter>().mesh = bgMesh;
            var bgMaterial = new Material(Shader.Find("Standard"));
            bgMaterial.SetColor("_Color", new Color(0.3f, 0.3f, 0.3f));
            backgroundGO.AddComponent<MeshRenderer>().material = bgMaterial;

            // Health bar fill
            var fillGO = new GameObject("HealthBarFill");
            fillGO.transform.SetParent(healthBarGO.transform);
            _healthBarFillBaseScale = new Vector3(_config.Size * 0.8f, 1f, 1f);
            fillGO.transform.localScale = _healthBarFillBaseScale;
            var fillMesh = CreateCubeMesh();
            fillGO.AddComponent<MeshFilter>().mesh = fillMesh;
            _healthBarMaterial = new Material(Shader.Find("Standard"));
            _healthBarMaterial.SetColor("_Color", HealthGreen);
            _healthBarMesh = fillGO.AddComponent<MeshRenderer>();
            _healthBarMesh.material = _healthBarMaterial;
            _healthBarFillTransform = fillGO.transform;
        }

        private static Mesh CreateCubeMesh()
        {
            var mesh = new Mesh();
            mesh.vertices = new Vector3[]
            {
                new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(0.5f, -0.5f, -0.5f),
                new Vector3(0.5f, -0.5f, 0.5f), new Vector3(-0.5f, -0.5f, 0.5f),
                new Vector3(-0.5f, 0.5f, -0.5f), new Vector3(0.5f, 0.5f, -0.5f),
                new Vector3(0.5f, 0.5f, 0.5f), new Vector3(-0.5f, 0.5f, 0.5f)
            };
            mesh.triangles = new int[]
            {
                0, 1, 2, 0, 2, 3, // Bottom
                4, 5, 6, 4, 6, 7, // Top
                0, 1, 5, 0, 5, 4, // Front
                2, 3, 7, 2, 7, 6, // Back
                0, 3, 7, 0, 7, 4, // Left
                1, 2, 6, 1, 6, 5  // Right
            };
            mesh.RecalculateNormals();
            return mesh;
        }

        private void CreateSelectionRing()
        {
            var ringGO = new GameObject("SelectionRing");
            ringGO.transform.SetParent(transform);
            ringGO.transform.position = new Vector3(0, -_config.Size * 0.2f, 0);

            var ringMesh = CreateRingMesh(_config.Size * 0.7f);
            ringGO.AddComponent<MeshFilter>().mesh = ringMesh;
            var ringMaterial = new Material(Shader.Find("Standard"));
            ringMaterial.SetColor("_Color", SelectionColor);
            ringMaterial.SetFloat("_Metallic", 0.8f);
            ringMaterial.SetFloat("_Glossiness", 0.2f);
            _selectionRing = ringGO.AddComponent<MeshRenderer>();
            _selectionRing.material = ringMaterial;
            ringGO.SetActive(false);
        }

        private static Mesh CreateRingMesh(float radius)
        {
            const int segments = 32;
            const float thickness = 2f;

            var vertices = new Vector3[segments * 2];
            var triangles = new int[segments * 6];

            for (int i = 0; i < segments; i++)
            {
                float angle = (i / (float)segments) * Mathf.PI * 2f;
                float nextAngle = ((i + 1) / (float)segments) * Mathf.PI * 2f;

                float cosA = Mathf.Cos(angle);
                float sinA = Mathf.Sin(angle);
                float cosB = Mathf.Cos(nextAngle);
                float sinB = Mathf.Sin(nextAngle);

                vertices[i * 2] = new Vector3(cosA * radius, 0, sinA * radius);
                vertices[i * 2 + 1] = new Vector3(cosB * radius, 0, sinB * radius);

                int vertexIndex = i * 2;
                int nextVertexIndex = (i + 1) * 2;

                triangles[vertexIndex * 3] = vertexIndex;
                triangles[vertexIndex * 3 + 1] = vertexIndex + 1;
                triangles[vertexIndex * 3 + 2] = nextVertexIndex;
            }

            var mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            return mesh;
        }

        private void UpdateHealthBar(TacticalShipEntity ship)
        {
            if (_shipEntity == null || _healthBarMesh == null)
                return;

            // Calculate health percentage
            float hullPct = (float)ship.Hull / ship.Config.MaxHull;
            float shieldPct = ship.Config.MaxShields > 0 ? (float)ship.Shields / ship.Config.MaxShields : 0f;
            float healthPct = hullPct * 0.7f + shieldPct * 0.3f;

            // Scale health bar based on health (using base scale to avoid compounding)
            if (_healthBarFillTransform != null)
            {
                _healthBarFillTransform.localScale = new Vector3(
                    _healthBarFillBaseScale.x * healthPct,
                    _healthBarFillBaseScale.y,
                    _healthBarFillBaseScale.z);
            }

            // Change color based on health
            var color = Color.Lerp(HealthRed, HealthGreen, healthPct);
            _healthBarMaterial.SetColor("_Color", color);
        }
    }
}

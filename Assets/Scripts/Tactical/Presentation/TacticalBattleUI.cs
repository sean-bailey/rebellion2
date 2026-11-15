using UnityEngine;
using UnityEngine.UI;
using Rebellion.Tactical.Data;
using Rebellion.Tactical.Simulation;
using Rebellion.Tactical.UI;

namespace Rebellion.Tactical.Presentation
{
    /// <summary>
    /// UI panel for the tactical battle scene.
    /// Shows ship status, speed control, and battle information.
    /// </summary>
    public sealed class TacticalBattleUI : MonoBehaviour
    {
        private TacticalBattleCoordinator _coordinator;
        private TacticalSpeedController _speedController;

        private Text _battleInfoText;
        private Text _shipStatusText;
        private Text _speedText;
        private Button _pauseButton;
        private Button _speedUpButton;
        private Button _speedDownButton;
        private Button _withdrawButton;

        private void Start()
        {
            _coordinator = FindObjectOfType<TacticalBattleCoordinator>();
            _speedController = _coordinator?.SpeedController;
        }

        /// <summary>
        /// Creates the tactical battle UI panel.
        /// </summary>
        public static TacticalBattleUI CreateUI()
        {
            // Create UI canvas
            var canvasGO = new GameObject("BattleUI_Canvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1;

            var canvasScaler = canvasGO.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.UIScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceWidth = 1920;
            canvasScaler.referenceHeight = 1080;

            var graphicRaycaster = canvasGO.AddComponent<GraphicRaycaster>();

            // Create UI controller
            var uiController = canvasGO.AddComponent<TacticalBattleUI>();

            // Create battle info panel (top-left)
            uiController.CreateBattleInfoPanel(canvasGO);

            // Create ship status panel (right side)
            uiController.CreateShipStatusPanel(canvasGO);

            // Create speed control panel (bottom)
            uiController.CreateSpeedControlPanel(canvasGO);

            return uiController;
        }

        private void CreateBattleInfoPanel(GameObject canvasGO)
        {
            var infoPanel = CreatePanel(canvasGO, "BattleInfo", new Rect(10, 10, 300, 150));

            var infoTextGO = new GameObject("BattleInfoText");
            infoTextGO.transform.SetParent(infoPanel.transform);
            infoTextGO.GetComponent<RectTransform>().anchorMin = Vector2.zero;
            infoTextGO.GetComponent<RectTransform>().anchorMax = Vector2.one;
            infoTextGO.GetComponent<RectTransform>().offsetMin = new Vector2(5, 5);
            infoTextGO.GetComponent<RectTransform>().offsetMax = new Vector2(-5, -5);

            _battleInfoText = infoTextGO.AddComponent<Text>();
            _battleInfoText.text = "Tactical Battle\n---";
            _battleInfoText.color = Color.white;
            _battleInfoText.fontSize = 14;
            _battleInfoText.alignment = TextAlignment.MiddleLeft;
        }

        private void CreateShipStatusPanel(GameObject canvasGO)
        {
            var statusPanel = CreatePanel(canvasGO, "ShipStatus", new Rect(1920 - 310, 10, 300, 400));

            var statusTextGO = new GameObject("ShipStatusText");
            statusTextGO.transform.SetParent(statusPanel.transform);
            statusTextGO.GetComponent<RectTransform>().anchorMin = Vector2.zero;
            statusTextGO.GetComponent<RectTransform>().anchorMax = Vector2.one;
            statusTextGO.GetComponent<RectTransform>().offsetMin = new Vector2(5, 5);
            statusTextGO.GetComponent<RectTransform>().offsetMax = new Vector2(-5, -5);

            _shipStatusText = statusTextGO.AddComponent<Text>();
            _shipStatusText.text = "Ship Status\n---";
            _shipStatusText.color = Color.white;
            _shipStatusText.fontSize = 14;
            _shipStatusText.alignment = TextAlignment.MiddleLeft;
        }

        private void CreateSpeedControlPanel(GameObject canvasGO)
        {
            var speedPanel = CreatePanel(canvasGO, "SpeedControl", new Rect(1920 - 400, 1080 - 80, 390, 70));

            // Speed text
            var speedTextGO = new GameObject("SpeedText");
            speedTextGO.transform.SetParent(speedPanel.transform);
            speedTextGO.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0.5f);
            speedTextGO.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
            speedTextGO.GetComponent<RectTransform>().offsetMin = new Vector2(5, 0);
            speedTextGO.GetComponent<RectTransform>().offsetMax = new Vector2(-5, 0);

            _speedText = speedTextGO.AddComponent<Text>();
            _speedText.text = "Speed: Real Time";
            _speedText.color = Color.white;
            _speedText.fontSize = 16;
            _speedText.alignment = TextAlignment.MidCenter;

            // Pause button
            _pauseButton = CreateButton(speedPanel, "Pause", new Rect(10, 10, 80, 40));
            _pauseButton.onClick.AddListener(() =>
            {
                _coordinator?.Pause();
            });

            // Speed up button
            _speedUpButton = CreateButton(speedPanel, "Fast >>", new Rect(100, 10, 100, 40));
            _speedUpButton.onClick.AddListener(() =>
            {
                _speedController?.CycleSpeed();
            });

            // Speed down button
            _speedDownButton = CreateButton(speedPanel, "Slow <", new Rect(210, 10, 100, 40));
            _speedDownButton.onClick.AddListener(() =>
            {
                // Cycle backwards
                if (_speedController != null)
                {
                    var current = _speedController.CurrentSpeed;
                    switch (current)
                    {
                        case TacticalBattleSpeed.Paused:
                            _speedController.SetSpeed(TacticalBattleSpeed.VeryFast);
                            break;
                        case TacticalBattleSpeed.RealTime:
                            _speedController.SetSpeed(TacticalBattleSpeed.Paused);
                            break;
                        case TacticalBattleSpeed.Fast:
                            _speedController.SetSpeed(TacticalBattleSpeed.RealTime);
                            break;
                        case TacticalBattleSpeed.VeryFast:
                            _speedController.SetSpeed(TacticalBattleSpeed.Fast);
                            break;
                    }
                }
            });

            // Withdraw button
            _withdrawButton = CreateButton(speedPanel, "Withdraw", new Rect(310, 10, 80, 40));
            _withdrawButton.onClick.AddListener(() =>
            {
                // Handle withdrawal
                Debug.Log("Withdraw requested");
            });
        }

        private static GameObject CreatePanel(GameObject canvasGO, string name, Rect rect)
        {
            var panelGO = new GameObject(name);
            panelGO.transform.SetParent(canvasGO.transform);

            var rectTransform = panelGO.GetComponent<RectTransform>() ?? panelGO.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.zero;
            rectTransform.position = new Vector3(rect.x, rect.y, 0);
            rectTransform.sizeDelta = new Vector2(rect.width, rect.height);

            var image = panelGO.AddComponent<Image>();
            image.color = new Color(0.1f, 0.1f, 0.15f, 0.8f);

            return panelGO;
        }

        private static Button CreateButton(GameObject parent, string label, Rect rect)
        {
            var buttonGO = new GameObject(label);
            buttonGO.transform.SetParent(parent.transform);

            var rectTransform = buttonGO.GetComponent<RectTransform>() ?? buttonGO.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.zero;
            rectTransform.position = new Vector3(rect.x, rect.y, 0);
            rectTransform.sizeDelta = new Vector2(rect.width, rect.height);

            var image = buttonGO.AddComponent<Image>();
            image.color = new Color(0.2f, 0.2f, 0.3f, 0.9f);

            var button = buttonGO.AddComponent<Button>();

            var textGO = new GameObject("Label");
            textGO.transform.SetParent(buttonGO.transform);
            textGO.GetComponent<RectTransform>().anchorMin = Vector2.zero;
            textGO.GetComponent<RectTransform>().anchorMax = Vector2.one;
            textGO.GetComponent<RectTransform>().offsetMin = Vector2.zero;
            textGO.GetComponent<RectTransform>().offsetMax = Vector2.zero;

            var text = textGO.AddComponent<Text>();
            text.text = label;
            text.color = Color.white;
            text.fontSize = 14;
            text.alignment = TextAlignment.Center;

            return button;
        }

        private void Update()
        {
            if (_coordinator == null || _coordinator.State != TacticalBattleStateEnum.Running)
                return;

            var simulation = _coordinator.Simulation;
            if (simulation == null)
                return;

            // Update battle info
            UpdateBattleInfo(simulation);

            // Update ship status
            UpdateShipStatus(simulation);

            // Update speed display
            UpdateSpeedDisplay();
        }

        private void UpdateBattleInfo(TacticalBattleSimulation simulation)
        {
            if (_battleInfoText == null)
                return;

            int attackersAlive = 0;
            int defendersAlive = 0;

            foreach (var ship in simulation.AttackerShips)
            {
                if (!ship.IsDestroyed) attackersAlive++;
            }

            foreach (var ship in simulation.DefenderShips)
            {
                if (!ship.IsDestroyed) defendersAlive++;
            }

            _battleInfoText.text =
                $"Tactical Battle\n" +
                $"Time: {simulation.ElapsedTime:F1}s\n" +
                $"Attackers: {attackersAlive}/{simulation.AttackerShips.Count}\n" +
                $"Defenders: {defendersAlive}/{simulation.DefenderShips.Count}";
        }

        private void UpdateShipStatus(TacticalBattleSimulation simulation)
        {
            if (_shipStatusText == null)
                return;

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Ship Status");
            sb.AppendLine("---");

            foreach (var ship in simulation.AllShips)
            {
                if (ship.IsDestroyed)
                    continue;

                float hullPct = (float)ship.Hull / ship.Config.MaxHull * 100f;
                float shieldPct = ship.Config.MaxShields > 0
                    ? (float)ship.Shields / ship.Config.MaxShields * 100f
                    : 0f;

                sb.AppendLine($"[{ship.Side}] {ship.Id}:");
                sb.AppendLine($"  Hull: {hullPct:F0}%");
                sb.AppendLine($"  Shields: {shieldPct:F0}%");
                sb.AppendLine($"  Speed: {ship.EffectiveSpeed:F1}");
                sb.AppendLine();
            }

            _shipStatusText.text = sb.ToString();
        }

        private void UpdateSpeedDisplay()
        {
            if (_speedText == null || _speedController == null)
                return;

            _speedText.text = $"Speed: {_speedController.CurrentSpeed}";
        }
    }
}
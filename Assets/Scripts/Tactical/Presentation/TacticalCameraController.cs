using UnityEngine;

namespace Rebellion.Tactical.Presentation
{
    /// <summary>
    /// Orbit camera controller for the tactical battle scene.
    /// Supports mouse drag rotation, scroll zoom, and right-click pan.
    /// </summary>
    public sealed class TacticalCameraController : MonoBehaviour
    {
        [Header("Camera Settings")]
        [SerializeField] private float _rotationSpeed = 2f;
        [SerializeField] private float _zoomSpeed = 100f;
        [SerializeField] private float _minDistance = 200f;
        [SerializeField] private float _maxDistance = 5000f;
        [SerializeField] private float _panSpeed = 50f;

        private Camera _camera;
        private float _distance = 2000f;
        private float _yaw = 0f;
        private float _pitch = -30f;
        private Vector3 _target;
        private bool _isDragging;
        private bool _isPanning;
        private Vector3 _lastMousePosition;

        private void Start()
        {
            _camera = Camera.main;
            if (_camera == null)
            {
                var camGO = new GameObject("MainCamera");
                camGO.AddComponent<Camera>();
                _camera = camGO.GetComponent<Camera>();
            }

            _camera.transform.position = new Vector3(0, 1000f, 2000f);
            _camera.transform.LookAt(Vector3.zero);
        }

        private void Update()
        {
            HandleInput();
            UpdateCameraPosition();
        }

        private void HandleInput()
        {
            // Left mouse drag - rotate
            if (Input.GetMouseButtonDown(0))
            {
                _isDragging = true;
                _lastMousePosition = Input.mousePosition;
            }

            if (Input.GetMouseButtonUp(0))
            {
                _isDragging = false;
            }

            if (_isDragging && Input.GetMouseButtonDown(0))
            {
                var mouseDelta = Input.mousePosition - _lastMousePosition;
                _yaw += mouseDelta.x * _rotationSpeed * 0.1f;
                _pitch = Mathf.Clamp(_pitch + mouseDelta.y * _rotationSpeed * 0.1f, -80f, 80f);
                _lastMousePosition = Input.mousePosition;
            }

            // Right mouse drag - pan
            if (Input.GetMouseButtonDown(1))
            {
                _isPanning = true;
                _lastMousePosition = Input.mousePosition;
            }

            if (Input.GetMouseButtonUp(1))
            {
                _isPanning = false;
            }

            if (_isPanning && Input.GetMouseButtonDown(1))
            {
                var mouseDelta = Input.mousePosition - _lastMousePosition;
                var panX = mouseDelta.x * _panSpeed * 0.01f;
                var panZ = mouseDelta.y * _panSpeed * 0.01f;

                var forward = _camera.transform.forward;
                var right = _camera.transform.right;

                _target += right * panX;
                _target -= forward * panZ;

                _lastMousePosition = Input.mousePosition;
            }

            // Scroll wheel - zoom
            if (Input.mousePresent)
            {
                float scroll = Input.GetAxis("Mouse ScrollWheel");
                if (scroll != 0)
                {
                    _distance = Mathf.Clamp(_distance - scroll * _zoomSpeed, _minDistance, _maxDistance);
                }
            }
        }

        private void UpdateCameraPosition()
        {
            // Convert yaw/pitch to radians
            float yawRad = _yaw * Mathf.Deg2Rad;
            float pitchRad = _pitch * Mathf.Deg2Rad;

            // Calculate camera position using spherical coordinates
            float x = _target.x + _distance * Mathf.Cos(pitchRad) * Mathf.Sin(yawRad);
            float y = _target.y + _distance * Mathf.Sin(pitchRad);
            float z = _target.z + _distance * Mathf.Cos(pitchRad) * Mathf.Cos(yawRad);

            _camera.transform.position = new Vector3(x, y, z);
            _camera.transform.LookAt(_target);
        }
    }
}
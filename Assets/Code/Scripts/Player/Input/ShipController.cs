using UnityEngine;
using UnityEngine.InputSystem;

namespace MoonsAndStars.Assets.Code.Scripts.Player.Input
{
    public class ShipController : MonoBehaviour, PlayerControls.IShipActions
    {
        [Header("Movement Settings")]
        [SerializeField] private float _forwardSpeed = 50f;
        [SerializeField] private float _strafeSpeed = 40f;
        [SerializeField] private float _verticalSpeed = 30f;
        [SerializeField] private float _boostMultiplier = 2f;

        [Header("Rotation Settings")]
        [SerializeField] private float _mouseSensitivity = 15f; // Adjusted for standard FPS feel
        [SerializeField] private float _rollSpeed = 120f;
        [SerializeField] private float _rollReturnSpeed = 5f; // Speed of auto-leveling to 0

        [Header("Physics")]
        [SerializeField] private Rigidbody _rb;
        [SerializeField] private float _drag = 0.95f;

        [Header("Weapons")]
        [SerializeField] private GameObject _projectilePrefab;
        [SerializeField] private Transform _firePoint;
        [SerializeField] private float _fireRate = 0.2f;
        [SerializeField] private float _projectileSpeed = 100f;

        private PlayerControls _controls;

        private float _currentMove;
        private float _currentStrafe;
        private float _currentVertical;
        private float _currentRoll;
        private bool _isBoosting;

        private float _nextFireTime;

        private float _yaw;
        private float _pitch;
        private float _roll;

        private Vector2 _mouseDelta;

        private void Awake()
        {
            if (_rb == null)
                _rb = GetComponent<Rigidbody>();

            _rb.useGravity = false;
            _rb.linearDamping = _drag;

            _controls = new PlayerControls();
            _controls.Ship.SetCallbacks(this);
        }

        private void Start()
        {
            // Lock cursor to center for proper FPS delta tracking
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            Vector3 angles = transform.eulerAngles;
            _yaw = angles.y;
            _pitch = angles.x;
            _roll = angles.z;
        }

        private void OnEnable() => _controls.Ship.Enable();
        private void OnDisable() => _controls.Ship.Disable();

        private void Update()
        {
            // Read value continuously without filtering pipelines or conditional gates
            _mouseDelta = _controls.Ship.Look.ReadValue<Vector2>();

            HandleMouseLook();
            HandleRoll();
            HandleCursorLock();
        }

        private void FixedUpdate()
        {
            ApplyMovement();
            ApplyRotation();
        }

        private void HandleMouseLook()
        {
            // Multiplied by Time.deltaTime for frame-rate independence
            float mouseX = _mouseDelta.x * _mouseSensitivity * Time.deltaTime;
            float mouseY = _mouseDelta.y * _mouseSensitivity * Time.deltaTime;

            _yaw += mouseX;
            _pitch -= mouseY;

            // Clamped pitch prevents upside-down camera flips
            _pitch = Mathf.Clamp(_pitch, -30f, 30f);
        }

        private void HandleRoll()
        {
            if (Mathf.Abs(_currentRoll) > 0.01f)
            {
                // Active rolling (Q/E pressed)
                _roll += -_currentRoll * _rollSpeed * Time.deltaTime;
            }
            else
            {
                // Auto-leveling: Smoothly lerps back to upright orientation (0 degrees) when released
                _roll = Mathf.LerpAngle(_roll, 0f, Time.deltaTime * _rollReturnSpeed);
            }
        }

        private void HandleCursorLock()
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                bool locked = Cursor.lockState == CursorLockMode.Locked;
                Cursor.lockState = locked ? CursorLockMode.None : CursorLockMode.Locked;
                Cursor.visible = locked;
            }
        }

        private void ApplyMovement()
        {
            float speed = _forwardSpeed;
            if (_isBoosting) speed *= _boostMultiplier;

            Vector3 moveDirection =
                transform.forward * _currentMove +
                transform.right * _currentStrafe +
                transform.up * _currentVertical;

            if (moveDirection.sqrMagnitude > 1f)
                moveDirection.Normalize();

            Vector3 targetVelocity = moveDirection * speed;

            _rb.linearVelocity = Vector3.Lerp(
                _rb.linearVelocity,
                targetVelocity,
                Time.fixedDeltaTime * 5f
            );
        }

        private void ApplyRotation()
        {
            Quaternion targetRotation = Quaternion.Euler(_pitch, _yaw, _roll);

            _rb.MoveRotation(Quaternion.Slerp(
                _rb.rotation,
                targetRotation,
                Time.fixedDeltaTime * 10f
            ));
        }

        private void Fire()
        {
            if (_projectilePrefab == null || _firePoint == null) return;
            if (Time.time < _nextFireTime) return;

            _nextFireTime = Time.time + _fireRate;

            GameObject projectile = Instantiate(_projectilePrefab, _firePoint.position, _firePoint.rotation);
            Rigidbody prb = projectile.GetComponent<Rigidbody>();

            if (prb != null)
            {
                prb.linearVelocity = transform.forward * _projectileSpeed + _rb.linearVelocity;
            }

            Destroy(projectile, 5f);
        }

        #region Input Callbacks
        public void OnMove(InputAction.CallbackContext context) => _currentMove = context.ReadValue<float>();
        public void OnStrafe(InputAction.CallbackContext context) => _currentStrafe = context.ReadValue<float>();
        public void OnUpDown(InputAction.CallbackContext context) => _currentVertical = context.ReadValue<float>();
        public void OnRoll(InputAction.CallbackContext context) => _currentRoll = context.ReadValue<float>();
        public void OnBoost(InputAction.CallbackContext context) => _isBoosting = context.performed;
        public void OnFire(InputAction.CallbackContext context) { if (context.performed) Fire(); }
        
        // Blank interface callback. We sample directly using ReadValue inside Update()
        public void OnLook(InputAction.CallbackContext context) { }
        #endregion

        private void OnDestroy()
        {
            _controls.Ship.RemoveCallbacks(this);
            _controls.Dispose();
        }
    }
}
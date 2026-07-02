using MoonsAndStars.Assets.Code.Scripts.Players.Input;
using MoonsAndStars.Assets.Code.Scripts.Planets; 
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using Unity.Netcode;

namespace MoonsAndStars.Assets.Code.Scripts.Players.Input
{
    public class ShipController : NetworkBehaviour, PlayerControls.IShipActions
    {
        [Header("Movement Settings")]
        [SerializeField] private float _forwardSpeed = 50f;
        [SerializeField] private float _strafeSpeed = 40f;
        [SerializeField] private float _verticalSpeed = 30f;
        [SerializeField] private float _boostMultiplier = 2f;

        [Header("Rotation Settings")]
        [SerializeField] private float _mouseSensitivity = 15f;
        [SerializeField] private float _rollSpeed = 120f;
        [SerializeField] private float _rollReturnSpeed = 5f;

        [Header("Gravity Settings")]
        [SerializeField] private bool _affectedByGravity = true;
        [SerializeField] private float _gravityScale = 1f;
        [SerializeField] private float _gravityCheckInterval = 0.5f;

        [Header("Physics")]
        [SerializeField] private Rigidbody _rb;
        [SerializeField] private float _drag = 0.95f;

        [Header("Weapons")]
        [SerializeField] private GameObject _projectilePrefab;
        [SerializeField] private Transform _firePoint;
        [SerializeField] private float _fireRate = 0.2f;
        [SerializeField] private float _projectileSpeed = 100f;

        [Header("Camera Setup")]
        [SerializeField] private GameObject _playerCamera;

        private PlayerControls _controls;
        private float _currentMove;
        private float _currentStrafe;
        private float _currentVertical;
        private float _currentRoll;
        private bool _isBoosting;
        private float _nextFireTime;
        private Vector2 _lookInputAccumulator;

        private bool _isInGameplayScene = false;

        // Planetary tracking variables
        private GravityWell[] _planetsInScene;
        private float _gravityTimer;

        public override void OnNetworkSpawn()
        {
            // Set up our active scene identity safely
            _isInGameplayScene = SceneManager.GetActiveScene().name == "Space";

            if (!IsOwner)
            {
                // Deactivate the camera view of remote players so we don't look through their eyes
                if (_playerCamera != null) _playerCamera.SetActive(false);
                
                // CRITICAL FOR NETWORKING: 
                // Remote clients must turn kinematic ON if they are using client-authoritative NetworkTransforms,
                // OR turn kinematic OFF if the server is synchronizing positions natively via a NetworkRigidbody.
                // Assuming standard NetworkTransform replication, let's keep kinematic false so positions update fluidly.
                if (_rb != null) 
                {
                    _rb.isKinematic = false; 
                }
                return;
            }

            // --- LOCAL OWNER INITIALIZATION ---
            if (_rb == null) _rb = GetComponent<Rigidbody>();
            if (_rb != null)
            {
                _rb.isKinematic = false;
                _rb.linearDamping = _drag;
            }

            if (_playerCamera != null) _playerCamera.SetActive(true);

            EnableControls();
            FindPlanetsInScene();
        }

        private void FindPlanetsInScene()
        {
            _planetsInScene = Object.FindObjectsByType<GravityWell>(FindObjectsSortMode.None);
        }

        private void EnableControls()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            _controls = new PlayerControls();
            _controls.Ship.SetCallbacks(this);
            _controls.Ship.Enable();
        }

        private void Update()
        {
            if (!IsOwner || !_isInGameplayScene) return;

            ProcessLookInput();

            _gravityTimer += Time.deltaTime;
            if (_gravityTimer >= _gravityCheckInterval)
            {
                _gravityTimer = 0f;
                FindPlanetsInScene();
            }
        }

        private void FixedUpdate()
        {
            if (!IsOwner || !_isInGameplayScene) return;

            ProcessMovement();
            ProcessRotation();
            if (_affectedByGravity) ProcessGravity();
        }

        private void ProcessLookInput()
        {
            if (Mouse.current != null)
            {
                _lookInputAccumulator += Mouse.current.delta.ReadValue() * _mouseSensitivity;
            }
        }

        private void ProcessMovement()
        {
            float currentForwardSpeed = _forwardSpeed * (_isBoosting ? _boostMultiplier : 1f);
            Vector3 movement = transform.forward * _currentMove * currentForwardSpeed +
                               transform.right * _currentStrafe * _strafeSpeed +
                               transform.up * _currentVertical * _verticalSpeed;

            _rb.AddForce(movement, ForceMode.Acceleration);
        }

        private void ProcessRotation()
        {
            float yaw = _lookInputAccumulator.x * Time.fixedDeltaTime;
            float pitch = -_lookInputAccumulator.y * Time.fixedDeltaTime;
            _lookInputAccumulator = Vector2.zero;

            transform.Rotate(Vector3.up, yaw, Space.World);
            transform.Rotate(Vector3.right, pitch, Space.Self);

            if (_currentRoll != 0f)
            {
                transform.Rotate(Vector3.forward, -_currentRoll * _rollSpeed * Time.fixedDeltaTime, Space.Self);
            }
            else
            {
                Vector3 currentRotation = transform.localEulerAngles;
                float zRotation = currentRotation.z > 180f ? currentRotation.z - 360f : currentRotation.z;
                float rollCorrection = Mathf.Lerp(zRotation, 0f, _rollReturnSpeed * Time.fixedDeltaTime) - zRotation;
                transform.Rotate(Vector3.forward, rollCorrection, Space.Self);
            }
        }

        private void ProcessGravity()
        {
            if (_planetsInScene == null || _planetsInScene.Length == 0) return;

            Vector3 combinedGravityForce = Vector3.zero;
            Vector3 shipVelocity = (_rb != null) ? _rb.linearVelocity : Vector3.zero;

            foreach (GravityWell planet in _planetsInScene)
            {
                if (planet == null) continue;
                Vector3 gravityAcceleration = planet.GetGravityForce(transform.position, shipVelocity);
                combinedGravityForce += gravityAcceleration;
            }

            if (combinedGravityForce != Vector3.zero)
            {
                _rb.AddForce(combinedGravityForce * _gravityScale, ForceMode.Acceleration);
            }
        }

        private void Fire()
        {
            if (Time.time < _nextFireTime) return;
            _nextFireTime = Time.time + _fireRate;
            RequestFireServerRpc(_firePoint.position, _firePoint.rotation, _rb.linearVelocity);
        }

        [ServerRpc]
        private void RequestFireServerRpc(Vector3 spawnPosition, Quaternion spawnRotation, Vector3 shipVelocity)
        {
            GameObject projectile = Instantiate(_projectilePrefab, spawnPosition, spawnRotation);
            NetworkObject netObj = projectile.GetComponent<NetworkObject>();
            if (netObj != null) netObj.Spawn(true);

            Rigidbody prb = projectile.GetComponent<Rigidbody>();
            if (prb != null)
            {
                prb.linearVelocity = (spawnRotation * Vector3.forward) * _projectileSpeed + shipVelocity;
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
        public void OnLook(InputAction.CallbackContext context) { }
        #endregion

        public override void OnNetworkDespawn()
        {
            if (IsOwner)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                if (_controls != null)
                {
                    _controls.Ship.RemoveCallbacks(this);
                    _controls.Dispose();
                }
            }
        }
    }
}
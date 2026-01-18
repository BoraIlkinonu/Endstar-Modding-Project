using UnityEngine;

namespace SwimmingSystem
{
    /// <summary>
    /// Handles swimming movement input and applies movement to the player.
    /// Overrides normal player movement when in water.
    /// </summary>
    public class SwimmingController : MonoBehaviour
    {
        private SwimmingManager _manager;
        private Transform _playerTransform;
        private CharacterController _characterController;

        private bool _isSwimming = false;
        private Vector3 _swimVelocity = Vector3.zero;

        // Movement input
        private float _horizontalInput = 0f;
        private float _verticalInput = 0f;
        private float _depthInput = 0f;  // Q/E for diving

        // Camera reference for movement direction
        private Transform _cameraTransform;

        public void Initialize(SwimmingManager manager, Transform playerTransform, CharacterController characterController)
        {
            _manager = manager;
            _playerTransform = playerTransform;
            _characterController = characterController;

            Plugin.Log.LogWarning("[SWIMCTRL] SwimmingController initialized");
        }

        void Update()
        {
            if (!_isSwimming) return;

            // Capture input
            CaptureInput();
        }

        void FixedUpdate()
        {
            if (!_isSwimming) return;

            // Apply movement
            ApplySwimmingMovement();
        }

        private void CaptureInput()
        {
            // Standard movement input (WASD / Arrows)
            _horizontalInput = Input.GetAxis("Horizontal");  // A/D or Left/Right
            _verticalInput = Input.GetAxis("Vertical");      // W/S or Up/Down

            // Depth input (Q/E)
            _depthInput = 0f;
            if (Input.GetKey(SwimConfig.DiveKey))
            {
                _depthInput = -1f;  // Dive down
            }
            else if (Input.GetKey(SwimConfig.SurfaceKey))
            {
                _depthInput = 1f;   // Surface up
            }
        }

        private void ApplySwimmingMovement()
        {
            if (_playerTransform == null) return;

            // Get camera if needed
            if (_cameraTransform == null)
            {
                var mainCam = Camera.main;
                if (mainCam != null)
                {
                    _cameraTransform = mainCam.transform;
                }
            }

            // Calculate movement direction relative to camera
            Vector3 moveDirection = Vector3.zero;

            if (_cameraTransform != null)
            {
                // Forward/backward relative to camera (but on XZ plane)
                Vector3 forward = _cameraTransform.forward;
                forward.y = 0;
                forward.Normalize();

                Vector3 right = _cameraTransform.right;
                right.y = 0;
                right.Normalize();

                moveDirection = (forward * _verticalInput + right * _horizontalInput);
            }
            else
            {
                // Fallback: use player's own forward
                moveDirection = (_playerTransform.forward * _verticalInput +
                                _playerTransform.right * _horizontalInput);
            }

            // Add vertical movement (diving/surfacing)
            moveDirection.y = _depthInput;

            // Normalize if moving diagonally
            if (moveDirection.magnitude > 1f)
            {
                moveDirection.Normalize();
            }

            // Apply swim speed
            float currentSpeed = SwimConfig.SwimSpeed;
            if (_depthInput < 0) currentSpeed = SwimConfig.DiveSpeed;
            else if (_depthInput > 0) currentSpeed = SwimConfig.SurfaceSpeed;

            _swimVelocity = moveDirection * currentSpeed;

            // Apply movement
            if (_characterController != null && _characterController.enabled)
            {
                _characterController.Move(_swimVelocity * Time.fixedDeltaTime);
            }
            else
            {
                // Direct transform movement
                _playerTransform.position += _swimVelocity * Time.fixedDeltaTime;
            }

            // Rotate player to face movement direction (only on XZ plane)
            if (_horizontalInput != 0 || _verticalInput != 0)
            {
                Vector3 lookDir = new Vector3(moveDirection.x, 0, moveDirection.z);
                if (lookDir.magnitude > 0.1f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(lookDir);
                    _playerTransform.rotation = Quaternion.RotateTowards(
                        _playerTransform.rotation,
                        targetRotation,
                        SwimConfig.RotationSpeed * Time.fixedDeltaTime
                    );
                }
            }
        }

        /// <summary>
        /// Called when entering water
        /// </summary>
        public void OnEnterWater()
        {
            _isSwimming = true;
            _swimVelocity = Vector3.zero;

            Plugin.Log.LogWarning("[SWIMCTRL] Swimming mode ENABLED");
            Plugin.Log.LogWarning("[SWIMCTRL] Controls: WASD=Swim, Q=Dive, E=Surface");

            // Optionally disable gravity on CharacterController
            // (would need to hook into the game's physics system)
        }

        /// <summary>
        /// Called when exiting water
        /// </summary>
        public void OnExitWater()
        {
            _isSwimming = false;
            _swimVelocity = Vector3.zero;

            Plugin.Log.LogWarning("[SWIMCTRL] Swimming mode DISABLED");
        }

        /// <summary>
        /// Check if player is currently moving
        /// </summary>
        public bool IsMoving => Mathf.Abs(_horizontalInput) > 0.1f ||
                                Mathf.Abs(_verticalInput) > 0.1f ||
                                Mathf.Abs(_depthInput) > 0.1f;

        /// <summary>
        /// Get current swim velocity for animation blending
        /// </summary>
        public Vector3 SwimVelocity => _swimVelocity;

        /// <summary>
        /// Check if diving down
        /// </summary>
        public bool IsDiving => _depthInput < -0.1f;

        /// <summary>
        /// Check if surfacing up
        /// </summary>
        public bool IsSurfacing => _depthInput > 0.1f;
    }
}

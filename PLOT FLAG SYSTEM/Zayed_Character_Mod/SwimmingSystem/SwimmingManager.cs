using UnityEngine;
using System.Collections.Generic;
using System.Reflection;

namespace SwimmingSystem
{
    /// <summary>
    /// Main coordinator for the swimming system.
    /// Manages water detection, swimming controller, and animations.
    /// </summary>
    public class SwimmingManager : MonoBehaviour
    {
        // Components
        private SwimmingController _swimController;
        private SwimAnimator _swimAnimator;

        // Player references
        private Transform _playerTransform;
        private Animator _gameAnimator;
        private Transform _animationRoot;
        private CharacterController _characterController;

        // State
        private bool _isInitialized = false;
        private bool _isInWater = false;
        private float _initTimer = 0f;

        // Bone cache for animation
        internal Dictionary<string, Transform> BoneCache = new Dictionary<string, Transform>();

        void Start()
        {
            Plugin.Log.LogWarning("[MANAGER] SwimmingManager starting...");
        }

        void Update()
        {
            _initTimer += Time.deltaTime;

            // Try to initialize if not yet done
            if (!_isInitialized && _initTimer > 2f)
            {
                TryInitialize();
            }

            // Debug key to test swimming (will be replaced by water detection)
            if (_isInitialized && Input.GetKeyDown(SwimConfig.DebugToggleKey))
            {
                if (_isInWater)
                {
                    ExitWater();
                }
                else
                {
                    EnterWater();
                }
            }
        }

        private void TryInitialize()
        {
            // Find player through AppearanceAnimator (same method as AnimationInjector)
            var allMonoBehaviours = FindObjectsOfType<MonoBehaviour>();
            foreach (var mb in allMonoBehaviours)
            {
                if (mb.GetType().Name == "AppearanceAnimator")
                {
                    var animatorField = mb.GetType().GetField("animator",
                        BindingFlags.NonPublic | BindingFlags.Instance);

                    if (animatorField != null)
                    {
                        _gameAnimator = animatorField.GetValue(mb) as Animator;
                        if (_gameAnimator != null)
                        {
                            Plugin.Log.LogWarning("[MANAGER] Found AppearanceAnimator!");
                            Plugin.Log.LogWarning($"[MANAGER] Animator on: {_gameAnimator.gameObject.name}");

                            // Find animation root (parent of rig)
                            _animationRoot = FindAnimationRoot(_gameAnimator.transform);
                            if (_animationRoot != null)
                            {
                                Plugin.Log.LogWarning($"[MANAGER] Animation root: {_animationRoot.name}");

                                // Build bone cache
                                BuildBoneCache(_animationRoot);
                                Plugin.Log.LogWarning($"[MANAGER] Built bone cache with {BoneCache.Count} bones");

                                // Find player transform (for movement)
                                _playerTransform = FindPlayerTransform(_gameAnimator.transform);
                                if (_playerTransform != null)
                                {
                                    Plugin.Log.LogWarning($"[MANAGER] Player transform: {_playerTransform.name}");

                                    // Try to find CharacterController
                                    _characterController = _playerTransform.GetComponentInParent<CharacterController>();
                                    if (_characterController != null)
                                    {
                                        Plugin.Log.LogWarning($"[MANAGER] CharacterController found on: {_characterController.gameObject.name}");
                                    }

                                    // Create swimming controller
                                    _swimController = gameObject.AddComponent<SwimmingController>();
                                    _swimController.Initialize(this, _playerTransform, _characterController);

                                    // Create swim animator
                                    _swimAnimator = gameObject.AddComponent<SwimAnimator>();
                                    _swimAnimator.Initialize(this, _animationRoot, _gameAnimator);

                                    // Setup water zone detector on all water objects
                                    SetupWaterDetection();

                                    _isInitialized = true;
                                    Plugin.Log.LogWarning("[MANAGER] ═══════════════════════════════════════════════════════");
                                    Plugin.Log.LogWarning("[MANAGER] Swimming system initialized!");
                                    Plugin.Log.LogWarning("[MANAGER] Press 9 to toggle swimming mode (DEBUG/TESTING ONLY)");
                                    Plugin.Log.LogWarning("[MANAGER] In water: 1=Dive, 2=Surface, WASD=Swim");
                                    Plugin.Log.LogWarning("[MANAGER] ═══════════════════════════════════════════════════════");
                                }
                            }
                            return;
                        }
                    }
                }
            }

            // Not found yet, will try again
            if (!_isInitialized && _initTimer < 10f)
            {
                Plugin.Log.LogInfo("[MANAGER] Player not found yet, will retry...");
            }
        }

        private Transform FindAnimationRoot(Transform animatorTransform)
        {
            // Look for the rig parent
            string[] possibleRigNames = { "ThePack_Felix_VC_01", "Armature", "Root", "Skeleton" };

            foreach (var rigName in possibleRigNames)
            {
                var result = FindTransformWithDirectChild(animatorTransform, rigName);
                if (result != null)
                {
                    return result;
                }
            }

            return animatorTransform;
        }

        private Transform FindTransformWithDirectChild(Transform parent, string childName)
        {
            foreach (Transform child in parent)
            {
                if (child.name == childName)
                {
                    return parent;
                }
            }
            foreach (Transform child in parent)
            {
                var result = FindTransformWithDirectChild(child, childName);
                if (result != null) return result;
            }
            return null;
        }

        private Transform FindPlayerTransform(Transform start)
        {
            // Walk up to find the main player object
            Transform current = start;
            while (current.parent != null)
            {
                if (current.name.Contains("Player") || current.name.Contains("Character"))
                {
                    // Check if this has a CharacterController or is the root player object
                    if (current.GetComponent<CharacterController>() != null ||
                        current.parent.GetComponent<CharacterController>() != null)
                    {
                        return current.parent.GetComponent<CharacterController>() != null
                            ? current.parent : current;
                    }
                }
                current = current.parent;
            }
            // Return the topmost ancestor
            current = start;
            while (current.parent != null) current = current.parent;
            return current;
        }

        private void BuildBoneCache(Transform root)
        {
            BoneCache.Clear();
            CacheBonesRecursive(root, "");
        }

        private void CacheBonesRecursive(Transform current, string path)
        {
            foreach (Transform child in current)
            {
                string childPath = string.IsNullOrEmpty(path) ? child.name : path + "/" + child.name;
                BoneCache[childPath] = child;
                CacheBonesRecursive(child, childPath);
            }
        }

        private void SetupWaterDetection()
        {
            // Find all objects that might be water
            var allObjects = FindObjectsOfType<GameObject>();
            int waterCount = 0;

            foreach (var obj in allObjects)
            {
                // Look for water-related names
                string nameLower = obj.name.ToLower();
                if (nameLower.Contains("water") || nameLower.Contains("ocean") ||
                    nameLower.Contains("sea") || nameLower.Contains("pool"))
                {
                    // Check if it has a collider that could be a trigger
                    var collider = obj.GetComponent<Collider>();
                    if (collider != null)
                    {
                        // Add our water zone detector
                        var detector = obj.GetComponent<WaterZoneDetector>();
                        if (detector == null)
                        {
                            detector = obj.AddComponent<WaterZoneDetector>();
                            detector.Initialize(this);
                            waterCount++;
                            Plugin.Log.LogWarning($"[MANAGER] Added water detector to: {obj.name}");
                        }
                    }
                }
            }

            Plugin.Log.LogWarning($"[MANAGER] Setup water detection on {waterCount} objects");
        }

        /// <summary>
        /// Called when player enters water
        /// </summary>
        public void EnterWater()
        {
            if (_isInWater) return;

            _isInWater = true;
            Plugin.Log.LogWarning("[MANAGER] ═══ ENTERING WATER ═══");

            // Notify components
            _swimController?.OnEnterWater();
            _swimAnimator?.OnEnterWater();
        }

        /// <summary>
        /// Called when player exits water
        /// </summary>
        public void ExitWater()
        {
            if (!_isInWater) return;

            _isInWater = false;
            Plugin.Log.LogWarning("[MANAGER] ═══ EXITING WATER ═══");

            // Notify components
            _swimController?.OnExitWater();
            _swimAnimator?.OnExitWater();
        }

        /// <summary>
        /// Check if currently in water
        /// </summary>
        public bool IsInWater => _isInWater;

        /// <summary>
        /// Get the game's animator (for disabling/enabling)
        /// </summary>
        public Animator GameAnimator => _gameAnimator;
    }
}

using UnityEngine;
using System.Collections.Generic;

namespace SwimmingSystem
{
    /// <summary>
    /// Manages swimming animation playback using the SampleAnimation approach.
    /// Handles state transitions between dive entry, swimming, and treading.
    /// </summary>
    public class SwimAnimator : MonoBehaviour
    {
        private SwimmingManager _manager;
        private Transform _animationRoot;
        private Animator _gameAnimator;
        private SwimmingController _swimController;

        // Animation clips
        private AnimationClip _swimmingClip;
        private AnimationClip _treadingClip;
        private AnimationClip _diveEntryClip;

        // State
        private enum SwimAnimState
        {
            None,
            DiveEntry,
            Swimming,
            Treading
        }
        private SwimAnimState _currentState = SwimAnimState.None;
        private SwimAnimState _previousState = SwimAnimState.None;

        // Playback
        private float _playbackTime = 0f;
        private AnimationClip _currentClip = null;
        private bool _isAnimating = false;

        // Original poses for restoration
        private Dictionary<string, BoneSnapshot> _originalPoses = new Dictionary<string, BoneSnapshot>();

        private struct BoneSnapshot
        {
            public Vector3 localPosition;
            public Quaternion localRotation;
            public Vector3 localScale;
        }

        public void Initialize(SwimmingManager manager, Transform animationRoot, Animator gameAnimator)
        {
            _manager = manager;
            _animationRoot = animationRoot;
            _gameAnimator = gameAnimator;

            // Get swim controller reference
            _swimController = GetComponent<SwimmingController>();

            // Load animation clips
            _swimmingClip = Plugin.GetClip(SwimConfig.SwimmingClipName);
            _treadingClip = Plugin.GetClip(SwimConfig.TreadingClipName);
            _diveEntryClip = Plugin.GetClip(SwimConfig.DiveEntryClipName);

            if (_swimmingClip != null && _treadingClip != null && _diveEntryClip != null)
            {
                Plugin.Log.LogWarning("[SWIMANIM] SwimAnimator initialized with all clips");
            }
            else
            {
                Plugin.Log.LogError("[SWIMANIM] Missing animation clips!");
            }
        }

        void LateUpdate()
        {
            if (!_isAnimating || _currentClip == null || _animationRoot == null)
                return;

            // Update playback time
            _playbackTime += Time.deltaTime;

            // Handle state-specific logic
            switch (_currentState)
            {
                case SwimAnimState.DiveEntry:
                    // Dive entry plays once, then transitions to treading
                    if (_playbackTime >= _diveEntryClip.length)
                    {
                        TransitionToState(SwimAnimState.Treading);
                        return;
                    }
                    break;

                case SwimAnimState.Swimming:
                case SwimAnimState.Treading:
                    // Loop these animations
                    if (_playbackTime >= _currentClip.length)
                    {
                        _playbackTime = _playbackTime % _currentClip.length;
                    }
                    break;
            }

            // Sample the animation
            _currentClip.SampleAnimation(_animationRoot.gameObject, _playbackTime);
        }

        void Update()
        {
            if (!_isAnimating) return;

            // Skip state updates during dive entry
            if (_currentState == SwimAnimState.DiveEntry) return;

            // Check if should switch between swimming and treading
            if (_swimController != null)
            {
                if (_swimController.IsMoving && _currentState != SwimAnimState.Swimming)
                {
                    TransitionToState(SwimAnimState.Swimming);
                }
                else if (!_swimController.IsMoving && _currentState != SwimAnimState.Treading)
                {
                    TransitionToState(SwimAnimState.Treading);
                }
            }
        }

        /// <summary>
        /// Called when entering water
        /// </summary>
        public void OnEnterWater()
        {
            Plugin.Log.LogWarning("[SWIMANIM] Starting swimming animations");

            // Store original poses
            StoreOriginalPoses();

            // Disable game animator
            if (_gameAnimator != null)
            {
                _gameAnimator.enabled = false;
                Plugin.Log.LogWarning("[SWIMANIM] Disabled game animator");
            }

            // Disable all child animators
            if (_animationRoot != null)
            {
                var animators = _animationRoot.GetComponentsInChildren<Animator>();
                foreach (var anim in animators)
                {
                    if (anim.enabled)
                    {
                        anim.enabled = false;
                    }
                }
            }

            _isAnimating = true;

            // Start with dive entry animation
            TransitionToState(SwimAnimState.DiveEntry);
        }

        /// <summary>
        /// Called when exiting water
        /// </summary>
        public void OnExitWater()
        {
            Plugin.Log.LogWarning("[SWIMANIM] Stopping swimming animations");

            _isAnimating = false;
            _currentState = SwimAnimState.None;
            _currentClip = null;

            // Restore original poses
            RestoreOriginalPoses();

            // Re-enable game animator
            if (_gameAnimator != null)
            {
                _gameAnimator.enabled = true;
                Plugin.Log.LogWarning("[SWIMANIM] Re-enabled game animator");
            }
        }

        private void TransitionToState(SwimAnimState newState)
        {
            _previousState = _currentState;
            _currentState = newState;
            _playbackTime = 0f;

            switch (newState)
            {
                case SwimAnimState.DiveEntry:
                    _currentClip = _diveEntryClip;
                    Plugin.Log.LogWarning("[SWIMANIM] Playing: Dive Entry");
                    break;

                case SwimAnimState.Swimming:
                    _currentClip = _swimmingClip;
                    Plugin.Log.LogWarning("[SWIMANIM] Playing: Swimming");
                    break;

                case SwimAnimState.Treading:
                    _currentClip = _treadingClip;
                    Plugin.Log.LogWarning("[SWIMANIM] Playing: Treading Water");
                    break;

                default:
                    _currentClip = null;
                    break;
            }
        }

        private void StoreOriginalPoses()
        {
            _originalPoses.Clear();

            if (_manager?.BoneCache == null) return;

            foreach (var kvp in _manager.BoneCache)
            {
                if (kvp.Value != null)
                {
                    _originalPoses[kvp.Key] = new BoneSnapshot
                    {
                        localPosition = kvp.Value.localPosition,
                        localRotation = kvp.Value.localRotation,
                        localScale = kvp.Value.localScale
                    };
                }
            }

            Plugin.Log.LogWarning($"[SWIMANIM] Stored {_originalPoses.Count} original bone poses");
        }

        private void RestoreOriginalPoses()
        {
            if (_manager?.BoneCache == null) return;

            int restored = 0;
            foreach (var kvp in _originalPoses)
            {
                if (_manager.BoneCache.TryGetValue(kvp.Key, out Transform bone) && bone != null)
                {
                    bone.localPosition = kvp.Value.localPosition;
                    bone.localRotation = kvp.Value.localRotation;
                    bone.localScale = kvp.Value.localScale;
                    restored++;
                }
            }

            Plugin.Log.LogWarning($"[SWIMANIM] Restored {restored} bone poses");
        }

        /// <summary>
        /// Get current animation state name
        /// </summary>
        public string CurrentStateName => _currentState.ToString();
    }
}

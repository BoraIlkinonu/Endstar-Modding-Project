using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace EndstarPropInjector
{
    [BepInPlugin("com.endstar.propinjector", "Endstar Prop Injector", "10.43.0")]
    public class Plugin : BaseUnityPlugin
    {
        // ========== v9.0.0: STATE MACHINE SWIMMING ==========
        private enum SwimState
        {
            SURFACE,    // Above water, normal gameplay
            ENTERING,   // Transitioning into water
            SWIMMING,   // Fully in water
            EXITING     // Grace period after leaving water
        }

        private static SwimState _currentSwimState = SwimState.SURFACE;
        private static int _exitGraceFrames = 0;
        private const int EXIT_GRACE_DURATION = 30;  // 1.5 seconds (30 frames * 50ms)
        private const float SURFACE_MARGIN = 0.1f;   // v10.1.0: Reduced - just needs to be slightly above water
        private const float SUBMERSION_DEPTH = 0.5f; // v10.1.0: How deep below surface waist should be when swimming
        private const float JUMP_FORCE = 2.5f;       // v10.3.0: Reduced to 2.5 per user request
        private const float TELEPORT_THRESHOLD = 5f; // v10.1.0: Position change > this = teleport/death
        private const float AUTO_SINK_FORCE = -2f;   // v10.2.0: Force to sink when above swim ceiling

        // v10.1.0: Teleport detection
        private static Vector3 _lastKnownPosition = Vector3.zero;
        private static bool _positionInitialized = false;

        // v10.4.1: Animation debugging
        private static int _landingResetFrames = 0;
        private const int LANDING_RESET_DURATION = 5;
        private static int _animLogCounter = 0;
        private static bool _lastGroundedState = true;
        private static SwimState _lastLoggedSwimState = SwimState.SURFACE;

        // v10.5.0: Falling animation capture
        private static bool _fallAnimLogged = false;
        private static float _lastFallTime = 0f;
        private static Animator _cachedAnimator = null;
        private static bool _animatorCached = false;

        // v10.7.1: Dive animation state tracking
        private static bool _lastDiveState = false;
        private static int _diveLogThrottle = 0;

        // v10.8.0: Direct animator control
        private static bool _animatorStatesLogged = false;

        // v10.17.0: COMPREHENSIVE SWIMMING SYSTEM (from AnimatorController extraction)
        // Extracted from: Character_Base_Animator-sharedassets0.assets-1620.txt
        // Note: Values > int.MaxValue are converted using unchecked cast (same bit pattern)
        // ========== STATE HASHES ==========
        private static readonly int HASH_ENTER_WATER = unchecked((int)3061384307);  // "Enter Water" - entry from surface
        private static readonly int HASH_ENTER_WATER_HIGH = unchecked((int)2940148679);  // "Enter Water High" - entry from height
        private static readonly int HASH_ABOVE_WATER_IDLE = 1260217858;    // "Above Water Idle" - floating at surface
        private static readonly int HASH_ABOVE_WATER_SWIM_START = 1966917648;  // "Above Water Swim Start"
        private static readonly int HASH_ABOVE_WATER_SWIM_STOP = 697086002;    // "Above Water Swim Stop"
        private static readonly int HASH_ABOVE_WATER_SWIM_BLEND = unchecked((int)3747168042);  // "Above Water Swim Blendtree"
        private static readonly int HASH_DIVE = 914991998;                 // "Dive" - diving animation
        private static readonly int HASH_BELOW_WATER_IDLE = 1443112800;    // "Below Water Idle" - underwater idle
        private static readonly int HASH_BELOW_WATER_SWIM_START = 1820082062;  // "Below Water Swim Start"
        private static readonly int HASH_BELOW_WATER_SWIM_STOP = 88956836;     // "Below Water Swim Stop"
        private static readonly int HASH_BELOW_WATER_SWIM_BLEND = unchecked((int)2791254829);  // "Below Water Swim Blendtree"
        private static readonly int HASH_BREACH = 940887619;               // "Breach" - rising to surface

        // ========== PARAMETER HASHES (for SetTrigger/SetBool by hash) ==========
        private static readonly int PARAM_WATER_IN = 1691914667;           // "WaterIn" (Bool)
        private static readonly int PARAM_WATER_IN_BELOW = 86030555;       // "WaterInBelow" (Bool)
        private static readonly int PARAM_WATER_BELOW = 119971305;         // "WaterBelow" (Bool)
        private static readonly int PARAM_WATER_ENTER = unchecked((int)4206307381);  // "WaterEnter" (Trigger)
        private static readonly int PARAM_WATER_EXIT = 973649254;          // "WaterExit" (Trigger)
        private static readonly int PARAM_WATER_DIVE = 951727400;          // "WaterDive" (Trigger)
        private static readonly int PARAM_WATER_BREACH = unchecked((int)3275916513);  // "WaterBreach" (Trigger)

        // ========== SWIMMING STATE TRACKING ==========
        private static bool _wasInWater = false;           // Track water entry/exit
        private static bool _wasUnderwater = false;        // Track dive/breach transitions
        private static bool _isMovingInWater = false;      // Track movement for swim blendtrees

        // v10.19.0: Smooth animation transitions
        private static float _smoothedVelY = 0f;           // Lerped velY for smooth BlendTree transitions
        private const float VELY_LERP_SPEED = 3f;          // How fast velY transitions (lower = smoother)

        // Legacy capture hashes (kept for comparison/debugging)
        private static int _fallStateHash = 0;
        private static int _idleStateHash = 0;

        // v10.15.0: Runtime state hash capture
        private static bool _idleHashCaptured = false;
        private static bool _fallHashCaptured = false;
        private static int _lastCapturedGroundedHash = 0;
        private static int _framesSinceGrounded = 0;

        // v10.12.0: Comprehensive animator diagnostics
        private static float _lastDiagnosticTime = 0f;
        private static int _lastStateHash = 0;
        private static bool _wasDivingForDiag = false;

        // v10.3.0: Jump immunity - prevent immediate re-entry after jumping
        private static int _jumpImmunityFrames = 0;
        private const int JUMP_IMMUNITY_DURATION = 15; // Frames to ignore re-entry check after jump

        public static ManualLogSource Log;
        public static Plugin Instance;
        private Harmony _harmony;

        // Custom prop registry
        public static Dictionary<string, object> CustomBaseTypes = new Dictionary<string, object>();
        public static Dictionary<string, object> CustomProps = new Dictionary<string, object>();

        // Our custom GUID for the test prop
        public static readonly string CustomPropGuid = "11111111-1111-1111-1111-111111111111";
        public static readonly string CustomBaseTypeGuid = "22222222-2222-2222-2222-222222222222";

        // AssetBundle loading
        private static AssetBundle _customPropsBundle;
        private static GameObject _loadedPrefab;
        private static Sprite _loadedIcon;

        // Cached types
        private static Type _baseTypeDefinitionType;
        private static Type _componentDefinitionType;
        private static Type _propType;
        private static Type _serializableGuidType;
        private static Type _stageManagerType;
        private static Type _abstractComponentListBaseType;
        private static Type _endlessVisualsType;
        private static Type _endlessPropType;
        private static Type _propBasedToolType;
        private static Type _propLibraryType;
        private static Type _playerControllerType;
        private static Type _depthPlaneType;
        private static Type _appearanceAnimatorType;
        private static Type _filterType;
        private static Type _playerReferenceManagerType;
        private static Type _netStateType;

        // v10.23.0: Item/Inventory diagnostic types
        private static Type _itemType;
        private static Type _inventoryType;
        private static Type _itemInteractableType;
        private static Type _inventorySlotType;
        private static Type _inventoryUsableDefinitionType;

        // v10.26.0: Collectible item types
        private static Type _treasureItemType;
        private static Type _genericUsableDefinitionType;
        private static Type _interactableBaseType;
        private static object _cachedTreasureDefinition;  // Cached "Treasure Usable Definition - Anachronist"
        private const int PLAYER_INTERACTABLE_LAYER = 11;  // Layer for pickupable items

        // v10.32.0: Rotation and icon types
        private static Type _simpleYawSpinType;
        private static Type _usableDefinitionType;
        private static object _customUsableDefinition;  // Our custom GenericUsableDefinition with Pearl Basket icon

        // Custom prefab and definition
        private static GameObject _customPrefab;
        private static GameObject _visualPrefab;
        private static object _customBaseTypeDefinition;
        private static GameObject _emptyVisualPlaceholder;  // v10.31.4: Empty placeholder for VisualsInfo

        // ========== UNDERWATER SWIMMING STATE (v6.0.0 DIAGNOSTIC) ==========
        public static bool IsPlayerUnderwater = false;
        public static bool IsSwimmingActive = false;
        public static float UnderwaterDepth = 0f;
        public static float SwimmingVerticalSpeed = 5f;
        public static float DepthPlaneY = float.MinValue;
        public static float WaterSurfaceY = float.MinValue;
        public static float WaterEntryY = float.MaxValue;
        public static float SwimActivationDepth = 1.0f;

        // Swimming bounds
        public static float MaxSwimDepth = 10f;
        public static float SurfaceStopOffset = 0.5f;
        public static float WaterEntryBuffer = 0.5f;
        public static float WaterExitBuffer = 1.0f;

        // v6.0.0: Motor-based swimming (like ghost mode)
        private static int _swimVerticalMotor = 0;
        private static int _swimMotorFrames = 8;
        private static int _swimMotorDegradeRate = 2;

        // v6.0.0: Cached PlayerController instance
        private static object _cachedPlayerController = null;
        private static bool _swimLoggedOnce = false;

        // v6.0.0: Cached field references for diagnostics
        private static FieldInfo _currentStateField = null;
        private static FieldInfo _totalForceField = null;
        private static FieldInfo _calculatedMotionField = null;
        private static FieldInfo _blockGravityField = null;
        private static FieldInfo _framesSinceStableGroundField = null;
        private static bool _fieldsInitialized = false;

        // v6.0.0: Blur/Filter state
        private static bool _blurReductionApplied = false;
        private static Dictionary<object, float> _originalVolumeWeights = new Dictionary<object, float>();

        // v10.20.0: Volume diagnostics
        private static bool _volumeDiagnosticsLogged = false;

        // v10.23.0: Item pickup diagnostics
        private static bool _itemPickupDiagnosticsLogged = false;

        // v6.0.0: Shader modification flag
        private static bool _shaderAlreadyModified = false;

        // v6.2.1: Swimming state tracking
        private static bool _swimPhysicsLogged = false;
        private static bool _wasSwimmingLastFrame = false;  // Track transition for exit handling
        private static float _waterExitCooldown = 0f;       // Cooldown after exiting to prevent state bugs
        private static float _lastExitTime = 0f;            // Time when last exited water

        // v8.0.0: CharacterController caching for waist-based surface detection
        private static CharacterController _cachedCharController = null;
        private static float _characterHeight = 2.0f;   // Default fallback
        private static float _characterRadius = 0.5f;   // Default fallback
        private static bool _charControllerCached = false;

        void Awake()
        {
            Instance = this;
            Log = Logger;
            Log.LogInfo("===========================================");
            Log.LogInfo("[INJECTOR] Plugin v10.24.0 - FULL DEFINITION DUMP");
            Log.LogInfo("[INJECTOR] - Dumps ALL InventoryUsableDefinition properties");
            Log.LogInfo("[INJECTOR] - Public fields, private fields, base type");
            Log.LogInfo("[INJECTOR] - Runtime extraction of MonoBehaviour data");
            Log.LogInfo("===========================================");

            try
            {
                _harmony = new Harmony("com.endstar.propinjector");

                // Cache types
                CacheTypes();

                // Initialize field references
                InitializeFieldReferences();

                // Apply patches - Prop Injection
                PatchStageManager();
                PatchEndlessPropBuildPrefab();
                PatchSpawnPreview();
                PatchFindAndManageChildRenderers();

                // v6.0.0 DIAGNOSTIC: Swimming patches
                PatchProcessFallOffStage();           // Disable death underwater
                PatchProcessPhysicsNetFrame();        // v6.0.0: DIAGNOSTIC Postfix (logs only!)
                PatchProcessJumpPrefix();             // Prefix for jump (uses reflection)
                PatchAppearanceAnimator();            // Swimming animation via parameters
                PatchDepthPlaneStart();               // Reduce shader blur
                PatchFilterStartTransition();         // Disable Blurred filter

                // v10.23.0: Item pickup diagnostic patches
                PatchItemPickup();                    // Log Item.Pickup calls
                PatchInventoryAttemptPickup();        // Log Inventory.AttemptPickupItem calls
                PatchItemInteractable();              // Log ItemInteractable interactions

                // v10.33.0: Hook ComponentInitialize to inject our custom visuals
                PatchItemComponentInitialize();

                // v10.34.0: Diagnostic for animator state after equipping
                PatchToggleLocalVisibility();

                Log.LogInfo("[INJECTOR] All patches applied successfully");
            }
            catch (Exception ex)
            {
                Log.LogError($"[INJECTOR] Failed to initialize: {ex}");
            }
        }

        void Update()
        {
            if (IsSwimmingActive)
            {
                UpdateSwimMotor();

                if (!_blurReductionApplied)
                {
                    TryReduceBlur();
                }
            }
            else
            {
                _swimVerticalMotor = 0;

                if (_blurReductionApplied)
                {
                    TryRestoreBlur();
                }
            }

            if (!IsPlayerUnderwater && _swimLoggedOnce)
            {
                ResetSwimmingCaches();
            }
        }

        private void UpdateSwimMotor()
        {
            float input = 0f;
            // Q = up (positive), E = down (negative) - intuitive controls
            if (Input.GetKey(KeyCode.Q)) input = 1f;
            else if (Input.GetKey(KeyCode.E)) input = -1f;

            _swimVerticalMotor = MotorFromInput(input, _swimVerticalMotor, _swimMotorFrames, _swimMotorDegradeRate);
        }

        private static int MotorFromInput(float input, int currentMotor, int maxFrames, int degradeRate)
        {
            if (input > 0f)
            {
                return Mathf.Min(currentMotor + 1, maxFrames);
            }
            else if (input < 0f)
            {
                return Mathf.Max(currentMotor - 1, -maxFrames);
            }
            else
            {
                if (currentMotor > 0)
                    return Mathf.Max(0, currentMotor - degradeRate);
                else if (currentMotor < 0)
                    return Mathf.Min(0, currentMotor + degradeRate);
                return 0;
            }
        }

        private void ResetSwimmingCaches()
        {
            // v10.0.0: Reset state machine
            _currentSwimState = SwimState.SURFACE;
            _exitGraceFrames = 0;

            _cachedPlayerController = null;
            _swimLoggedOnce = false;
            _swimVerticalMotor = 0;
            _swimPhysicsLogged = false;
            _wasSwimmingLastFrame = false;
            _waterExitCooldown = 0f;

            // v10.0.0: Reset water debug logging so it logs again on next water entry
            _waterDebugLogged = false;
            _cachedDepthPlane = null;

            if (_blurReductionApplied)
            {
                TryRestoreBlur();
            }
            _blurReductionApplied = false;

            Log.LogInfo("[SWIM] Caches reset (exited water), _currentSwimState=SURFACE");
        }

        /// <summary>
        /// v10.1.0: Full state reset - resets state machine and all swimming variables
        /// </summary>
        private static void FullSwimmingStateReset()
        {
            // v10.1.0: Reset state machine
            _currentSwimState = SwimState.SURFACE;
            _exitGraceFrames = 0;

            // Reset all swimming variables
            IsPlayerUnderwater = false;
            IsSwimmingActive = false;
            UnderwaterDepth = 0f;
            WaterEntryY = float.MaxValue;
            WaterSurfaceY = float.MinValue;
            _swimVerticalMotor = 0;
            _swimLoggedOnce = false;
            _wasSwimmingLastFrame = false;
            _lastExitTime = Time.time;
            _waterExitCooldown = 0f;

            // v10.0.0: Reset water debug logging
            _waterDebugLogged = false;
            _cachedDepthPlane = null;

            // v10.1.0: Reset position tracking (will re-init on next frame)
            _positionInitialized = false;

            // v10.4.0: Landing reset removed - game handles animation naturally

            // v10.3.0: Reset jump immunity
            _jumpImmunityFrames = 0;

            Log.LogInfo("[SWIM-RESET] Full state reset: _currentSwimState=SURFACE");
        }

        private void CacheTypes()
        {
            _baseTypeDefinitionType = AccessTools.TypeByName("Endless.Gameplay.BaseTypeDefinition");
            _componentDefinitionType = AccessTools.TypeByName("Endless.Gameplay.ComponentDefinition");
            _propType = AccessTools.TypeByName("Endless.Props.Assets.Prop");
            _serializableGuidType = AccessTools.TypeByName("Endless.Shared.DataTypes.SerializableGuid");
            _stageManagerType = AccessTools.TypeByName("Endless.Gameplay.LevelEditing.Level.StageManager");
            _endlessVisualsType = AccessTools.TypeByName("Endless.Gameplay.Scripting.EndlessVisuals");
            _endlessPropType = AccessTools.TypeByName("Endless.Gameplay.Scripting.EndlessProp");
            _propBasedToolType = AccessTools.TypeByName("Endless.Creator.LevelEditing.Runtime.PropBasedTool");
            _propLibraryType = AccessTools.TypeByName("Endless.Gameplay.LevelEditing.PropLibrary");
            _playerControllerType = AccessTools.TypeByName("Endless.Gameplay.PlayerController");
            _depthPlaneType = AccessTools.TypeByName("Endless.Gameplay.DepthPlane");
            _appearanceAnimatorType = AccessTools.TypeByName("Endless.Gameplay.AppearanceAnimator");
            _filterType = AccessTools.TypeByName("Endless.Gameplay.Filter");
            _playerReferenceManagerType = AccessTools.TypeByName("Endless.Gameplay.PlayerReferenceManager");
            _netStateType = AccessTools.TypeByName("Endless.Gameplay.NetState");

            // v10.23.0: Item/Inventory types for pickup diagnostics
            _itemType = AccessTools.TypeByName("Endless.Gameplay.Item");
            _inventoryType = AccessTools.TypeByName("Endless.Gameplay.Inventory");
            _itemInteractableType = AccessTools.TypeByName("Endless.Gameplay.ItemInteractable");
            _inventorySlotType = AccessTools.TypeByName("Endless.Gameplay.PlayerInventory.InventorySlot");
            _inventoryUsableDefinitionType = AccessTools.TypeByName("Endless.Gameplay.PlayerInventory.InventoryUsableDefinition");

            // v10.26.0: Collectible item types
            _treasureItemType = AccessTools.TypeByName("Endless.Gameplay.TreasureItem");
            _genericUsableDefinitionType = AccessTools.TypeByName("Endless.Gameplay.GenericUsableDefinition");
            _interactableBaseType = AccessTools.TypeByName("Endless.Gameplay.InteractableBase");

            // v10.32.0: Rotation and icon types
            _simpleYawSpinType = AccessTools.TypeByName("Endless.Gameplay.SimpleYawSpin");
            _usableDefinitionType = AccessTools.TypeByName("Endless.Gameplay.UsableDefinition");

            var baseTypeListType = AccessTools.TypeByName("Endless.Gameplay.BaseTypeList");
            if (baseTypeListType != null)
            {
                _abstractComponentListBaseType = baseTypeListType.BaseType;
            }

            Log.LogInfo($"[INJECTOR] Types cached:");
            Log.LogInfo($"[INJECTOR]   PlayerController: {_playerControllerType != null}");
            Log.LogInfo($"[INJECTOR]   NetState: {_netStateType != null}");
            Log.LogInfo($"[INJECTOR]   DepthPlane: {_depthPlaneType != null}");
            Log.LogInfo($"[INJECTOR]   Item: {_itemType != null}");
            Log.LogInfo($"[INJECTOR]   Inventory: {_inventoryType != null}");
            Log.LogInfo($"[INJECTOR]   ItemInteractable: {_itemInteractableType != null}");
            Log.LogInfo($"[INJECTOR]   TreasureItem: {_treasureItemType != null}");
            Log.LogInfo($"[INJECTOR]   GenericUsableDefinition: {_genericUsableDefinitionType != null}");
        }

        /// <summary>
        /// v6.0.0: Initialize field references for diagnostic logging
        /// </summary>
        private void InitializeFieldReferences()
        {
            if (_playerControllerType == null || _netStateType == null) return;

            _currentStateField = AccessTools.Field(_playerControllerType, "currentState");
            if (_currentStateField != null)
            {
                var stateType = _currentStateField.FieldType;
                _totalForceField = AccessTools.Field(stateType, "TotalForce");
                _calculatedMotionField = AccessTools.Field(stateType, "CalculatedMotion");
                _blockGravityField = AccessTools.Field(stateType, "BlockGravity");
                _framesSinceStableGroundField = AccessTools.Field(stateType, "FramesSinceStableGround");
            }

            _fieldsInitialized = _currentStateField != null && _totalForceField != null;

            Log.LogInfo($"[SWIM] Field references initialized:");
            Log.LogInfo($"[SWIM]   currentState: {_currentStateField != null} (type: {_currentStateField?.FieldType?.Name ?? "null"})");
            Log.LogInfo($"[SWIM]   TotalForce: {_totalForceField != null}");
            Log.LogInfo($"[SWIM]   CalculatedMotion: {_calculatedMotionField != null}");
            Log.LogInfo($"[SWIM]   BlockGravity: {_blockGravityField != null}");
            Log.LogInfo($"[SWIM]   FramesSinceStableGround: {_framesSinceStableGroundField != null}");
        }

        /// <summary>
        /// v8.0.0: Cache CharacterController via playerReferences reflection chain
        /// This gives us accurate height/radius for waist position calculation
        /// </summary>
        private static void CacheCharacterController(object playerController)
        {
            if (_charControllerCached) return;
            if (playerController == null) return;

            try
            {
                // Reflection chain: playerController.playerReferences.CharacterController
                var playerRefsField = playerController.GetType().GetField("playerReferences",
                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                if (playerRefsField == null)
                {
                    Log.LogWarning("[SWIM] playerReferences field not found");
                    return;
                }

                var playerRefs = playerRefsField.GetValue(playerController);
                if (playerRefs == null)
                {
                    Log.LogWarning("[SWIM] playerReferences value is null");
                    return;
                }

                // Try property first, then field
                var charControllerProp = playerRefs.GetType().GetProperty("CharacterController",
                    BindingFlags.Public | BindingFlags.Instance);
                if (charControllerProp != null)
                {
                    _cachedCharController = charControllerProp.GetValue(playerRefs) as CharacterController;
                }
                else
                {
                    var charControllerField = playerRefs.GetType().GetField("CharacterController",
                        BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
                    if (charControllerField != null)
                    {
                        _cachedCharController = charControllerField.GetValue(playerRefs) as CharacterController;
                    }
                }

                if (_cachedCharController != null)
                {
                    _characterHeight = _cachedCharController.height;
                    _characterRadius = _cachedCharController.radius;
                    _charControllerCached = true;
                    Log.LogInfo($"[SWIM] CharacterController cached: height={_characterHeight:F2}, radius={_characterRadius:F2}");
                }
                else
                {
                    Log.LogWarning("[SWIM] CharacterController not found, using defaults");
                }
            }
            catch (Exception ex)
            {
                Log.LogError($"[SWIM] Failed to cache CharacterController: {ex.Message}");
            }
        }

        // ========== SWIMMING PATCHES (v6.0.0 DIAGNOSTIC) ==========

        private void PatchProcessFallOffStage()
        {
            if (_playerControllerType != null)
            {
                var method = AccessTools.Method(_playerControllerType, "ProcessFallOffStage_NetFrame");
                if (method != null)
                {
                    var prefix = new HarmonyMethod(typeof(Plugin).GetMethod(nameof(ProcessFallOffStage_Prefix),
                        BindingFlags.Public | BindingFlags.Static));
                    _harmony.Patch(method, prefix: prefix);
                    Log.LogInfo("[INJECTOR] Patched PlayerController.ProcessFallOffStage_NetFrame");
                }
            }
        }

        /// <summary>
        /// v6.1.0: Patch ProcessPhysics_NetFrame with a POSTFIX
        /// This is the KEY patch - runs AFTER physics are processed but BEFORE Move() is called
        /// </summary>
        private void PatchProcessPhysicsNetFrame()
        {
            if (_playerControllerType != null)
            {
                var method = AccessTools.Method(_playerControllerType, "ProcessPhysics_NetFrame");
                if (method != null)
                {
                    var postfix = new HarmonyMethod(typeof(Plugin).GetMethod(nameof(ProcessPhysics_Postfix_Swimming),
                        BindingFlags.Public | BindingFlags.Static));
                    _harmony.Patch(method, postfix: postfix);
                    Log.LogInfo("[INJECTOR] Patched PlayerController.ProcessPhysics_NetFrame (v6.1.0 Swimming Postfix)");
                }
                else
                {
                    Log.LogError("[INJECTOR] ProcessPhysics_NetFrame method not found!");
                }
            }
        }

        private void PatchProcessJumpPrefix()
        {
            if (_playerControllerType != null)
            {
                var method = AccessTools.Method(_playerControllerType, "ProcessJump_NetFrame",
                    new Type[] { AccessTools.TypeByName("Endless.Gameplay.NetInput") });
                if (method != null)
                {
                    var prefix = new HarmonyMethod(typeof(Plugin).GetMethod(nameof(ProcessJump_Prefix),
                        BindingFlags.Public | BindingFlags.Static));
                    _harmony.Patch(method, prefix: prefix);
                    Log.LogInfo("[INJECTOR] Patched PlayerController.ProcessJump_NetFrame (prefix)");
                }
            }
        }

        private void PatchAppearanceAnimator()
        {
            if (_appearanceAnimatorType != null)
            {
                var method = AccessTools.Method(_appearanceAnimatorType, "SetAnimationState");
                if (method != null)
                {
                    var prefix = new HarmonyMethod(typeof(Plugin).GetMethod(nameof(SetAnimationState_Prefix),
                        BindingFlags.Public | BindingFlags.Static));
                    _harmony.Patch(method, prefix: prefix);
                    Log.LogInfo("[INJECTOR] Patched AppearanceAnimator.SetAnimationState");
                }
            }
        }

        private void PatchDepthPlaneStart()
        {
            if (_depthPlaneType != null)
            {
                var method = AccessTools.Method(_depthPlaneType, "Start");
                if (method != null)
                {
                    var postfix = new HarmonyMethod(typeof(Plugin).GetMethod(nameof(DepthPlane_Start_Postfix),
                        BindingFlags.Public | BindingFlags.Static));
                    _harmony.Patch(method, postfix: postfix);
                    Log.LogInfo("[INJECTOR] Patched DepthPlane.Start");
                }
            }
        }

        private void PatchFilterStartTransition()
        {
            if (_filterType != null)
            {
                var method = AccessTools.Method(_filterType, "StartTransition");
                if (method != null)
                {
                    var prefix = new HarmonyMethod(typeof(Plugin).GetMethod(nameof(StartTransition_Prefix),
                        BindingFlags.Public | BindingFlags.Static));
                    _harmony.Patch(method, prefix: prefix);
                    Log.LogInfo("[INJECTOR] Patched Filter.StartTransition");
                }
            }
        }

        // ========== v6.0.0 DIAGNOSTIC PATCH METHODS ==========

        private static object _cachedStageManager = null;
        private static PropertyInfo _cachedActiveStageProperty = null;
        private static PropertyInfo _cachedFallOffHeightProperty = null;
        private static bool _cacheInitialized = false;

        /// <summary>
        /// v10.0.0: ProcessPhysics_NetFrame POSTFIX - GHOST MODE PATTERN with State Machine
        /// Uses VERIFIED water surface from planeParent.position.y
        ///
        /// KEY INSIGHT: Ground detection (line 715) is SKIPPED when TotalForce.y + CalculatedMotion.y >= 1
        ///
        /// THE FIX (copying ghost mode from PlayerController.cs:414-418):
        /// 1. TotalForce.y = 0 (keeps ground detection threshold low)
        /// 2. FallFrames = 0 (like ghost mode line 417)
        /// 3. CalculatedMotion.y = swimSpeed (controlled movement like ghost mode)
        /// </summary>
        public static void ProcessPhysics_Postfix_Swimming(object __instance)
        {
            try
            {
                // v9.0.0: Only apply swimming physics in SWIMMING or ENTERING states
                if (_currentSwimState != SwimState.SWIMMING && _currentSwimState != SwimState.ENTERING)
                {
                    return;
                }

                // ===== STEP 1: GET PLAYER POSITION & CACHE CHARACTER CONTROLLER =====
                var transform = (__instance as MonoBehaviour)?.transform;
                if (transform == null) return;

                CacheCharacterController(__instance);

                float feetY = transform.position.y;
                float waistY = feetY + (_characterHeight * 0.5f - _characterRadius);

                // ===== STEP 2: POSITION SAFETY CHECK =====
                if (WaterSurfaceY == float.MinValue)
                {
                    return;  // Water surface not known yet
                }

                // ===== STEP 3: CACHE FIELD REFERENCES =====
                if (_currentStateField == null)
                {
                    _currentStateField = __instance.GetType().GetField("currentState",
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                }
                if (_currentStateField == null) return;

                object boxedState = _currentStateField.GetValue(__instance);
                if (boxedState == null) return;

                // Cache all needed fields
                if (_totalForceField == null)
                {
                    _totalForceField = boxedState.GetType().GetField("TotalForce",
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                }
                if (_calculatedMotionField == null)
                {
                    _calculatedMotionField = boxedState.GetType().GetField("CalculatedMotion",
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                }

                // v9.0.0: Cache FallFrames field (ghost mode pattern)
                FieldInfo fallFramesField = boxedState.GetType().GetField("FallFrames",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                if (_totalForceField == null || _calculatedMotionField == null)
                {
                    Log.LogError("[SWIM] Missing required fields!");
                    return;
                }

                // ===== STEP 4: CALCULATE SWIMMING =====
                // v10.1.0: Swim ceiling puts waist BELOW water surface by SUBMERSION_DEPTH
                // This ensures character is properly submerged, not floating at surface
                float waistAtSurface = WaterSurfaceY - SUBMERSION_DEPTH;  // Where waist should be
                float swimCeilingY = waistAtSurface - (_characterHeight * 0.5f - _characterRadius);  // Convert to feet position
                float minHeight = WaterSurfaceY - MaxSwimDepth;       // Max depth (10 units)

                float swimSpeed = SwimmingVerticalSpeed * ((float)_swimVerticalMotor / (float)_swimMotorFrames);

                // v10.2.0: Bounds enforcement - ALWAYS sink when above ceiling
                if (feetY >= swimCeilingY)
                {
                    // Above swim ceiling - apply auto-sink force to pull character down
                    // This ensures proper submersion when entering water
                    if (swimSpeed >= 0f)
                    {
                        swimSpeed = AUTO_SINK_FORCE;  // Force sink even with no input
                    }
                }
                else if (feetY <= minHeight)
                {
                    if (swimSpeed < 0f)
                    {
                        // At max depth - can't go deeper
                        swimSpeed = 0f;
                    }
                }

                // ===== STEP 5: APPLY GHOST MODE PATTERN (PlayerController.cs:414-418) =====

                // 1. TotalForce.y = 0 (like ghost mode zeroing TotalForce)
                Vector3 totalForce = (Vector3)_totalForceField.GetValue(boxedState);
                totalForce.y = 0f;
                _totalForceField.SetValue(boxedState, totalForce);

                // 2. FallFrames = 0 (ghost mode line 417)
                if (fallFramesField != null)
                {
                    fallFramesField.SetValue(boxedState, 0);
                }

                // 3. CalculatedMotion.y = swimSpeed (controlled movement like ghost mode)
                Vector3 calcMotion = (Vector3)_calculatedMotionField.GetValue(boxedState);
                calcMotion.y = swimSpeed;
                _calculatedMotionField.SetValue(boxedState, calcMotion);

                // Write the modified state back
                _currentStateField.SetValue(__instance, boxedState);

                // Log once
                if (!_swimLoggedOnce)
                {
                    Log.LogInfo($"[SWIM] v10.3.0 Swimming active (state={_currentSwimState})");
                    Log.LogInfo($"[SWIM] Water surface from planeParent.position.y = {WaterSurfaceY:F2}");
                    Log.LogInfo($"[SWIM] CharHeight={_characterHeight:F2}, CharRadius={_characterRadius:F2}");
                    Log.LogInfo($"[SWIM] SwimCeiling(feet)={swimCeilingY:F2}, MaxDepth={minHeight:F2}");
                    Log.LogInfo($"[SWIM] Controls: Q=up, E=down, Space=jump at surface");
                    _swimLoggedOnce = true;
                }
            }
            catch (Exception ex)
            {
                Log.LogError($"[SWIM] Physics error: {ex.Message}");
            }
        }

        /// <summary>
        /// v10.0.0: State machine-based death protection using VERIFIED water surface
        /// States: SURFACE -> ENTERING -> SWIMMING -> EXITING -> SURFACE
        /// Water surface = planeParent.position.y (verified from DepthPlane.cs line 366)
        /// NEVER returns true while in ENTERING, SWIMMING, or EXITING states
        /// </summary>
        public static bool ProcessFallOffStage_Prefix(object __instance)
        {
            try
            {
                var transform = (__instance as MonoBehaviour)?.transform;
                if (transform == null) return true;

                Vector3 currentPos = transform.position;
                float feetY = currentPos.y;

                // v10.1.0: TELEPORT/DEATH DETECTION - Reset state machine if player teleported
                if (_positionInitialized)
                {
                    float posDelta = Vector3.Distance(currentPos, _lastKnownPosition);
                    if (posDelta > TELEPORT_THRESHOLD)
                    {
                        // Player teleported (death respawn, etc.) - full reset
                        Log.LogInfo($"[SWIM] Teleport detected (delta={posDelta:F2}), resetting state machine");
                        FullSwimmingStateReset();
                        _lastKnownPosition = currentPos;
                        return true;  // Allow normal death check after reset
                    }
                }
                _lastKnownPosition = currentPos;
                _positionInitialized = true;

                // Cache CharacterController and calculate waist position
                CacheCharacterController(__instance);
                float waistY = feetY + (_characterHeight * 0.5f - _characterRadius);

                // Cache PlayerController instance
                _cachedPlayerController = __instance;

                // Initialize StageManager cache
                if (!_cacheInitialized)
                {
                    var stageManagerType = AccessTools.TypeByName("Endless.Gameplay.LevelEditing.Level.StageManager");
                    if (stageManagerType == null) return true;

                    var instanceProp = stageManagerType.GetProperty("Instance",
                        BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
                    if (instanceProp == null)
                    {
                        var baseType = stageManagerType.BaseType;
                        while (baseType != null && instanceProp == null)
                        {
                            instanceProp = baseType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
                            baseType = baseType.BaseType;
                        }
                    }
                    if (instanceProp == null) return true;

                    _cachedStageManager = instanceProp.GetValue(null);
                    _cachedActiveStageProperty = stageManagerType.GetProperty("ActiveStage");
                    _cacheInitialized = true;
                    Log.LogInfo("[SWIM] StageManager cache initialized (v10.0.0)");
                }

                if (_cachedStageManager == null || _cachedActiveStageProperty == null) return true;

                var activeStage = _cachedActiveStageProperty.GetValue(_cachedStageManager);
                if (activeStage == null) return true;

                if (_cachedFallOffHeightProperty == null)
                {
                    _cachedFallOffHeightProperty = activeStage.GetType().GetProperty("StageFallOffHeight");
                }
                if (_cachedFallOffHeightProperty == null) return true;

                // v10.0.0: fallOffHeight is the DEATH ZONE (planeParent.y + KillOffset)
                float fallOffHeight = (float)_cachedFallOffHeightProperty.GetValue(activeStage);
                DepthPlaneY = fallOffHeight;

                // v10.0.0: Get ACTUAL water surface from planeParent.position.y
                float waterSurfaceY = GetWaterSurfaceY(activeStage, fallOffHeight);

                // v10.0.0: If no valid water surface (Empty plane type), allow normal death check
                if (waterSurfaceY == float.MinValue)
                {
                    // No water on this stage - normal death behavior
                    return true;
                }

                // Store water surface for other methods (swimming physics)
                if (WaterSurfaceY == float.MinValue || _currentSwimState == SwimState.SURFACE)
                {
                    WaterSurfaceY = waterSurfaceY;
                }

                // ===== v10.0.0 STATE MACHINE (using verified water surface) =====
                switch (_currentSwimState)
                {
                    case SwimState.SURFACE:
                        // Check if entering water (waist below ACTUAL water surface)
                        if (waistY < waterSurfaceY)
                        {
                            _currentSwimState = SwimState.ENTERING;
                            WaterEntryY = feetY;
                            WaterSurfaceY = waterSurfaceY;
                            IsPlayerUnderwater = true;
                            Log.LogInfo($"[SWIM] SURFACE -> ENTERING: waist {waistY:F2} < surface {waterSurfaceY:F2}");
                            Log.LogInfo($"[SWIM] (Death zone is at Y={fallOffHeight:F2}, KillOffset={fallOffHeight - waterSurfaceY:F2})");
                            return false;  // Block death
                        }
                        return true;  // Normal death check when on surface

                    case SwimState.ENTERING:
                        // Check if fully submerged (feet below surface)
                        if (feetY < waterSurfaceY)
                        {
                            _currentSwimState = SwimState.SWIMMING;
                            IsSwimmingActive = true;
                            _swimLoggedOnce = false;
                            Log.LogInfo($"[SWIM] ENTERING -> SWIMMING: feet {feetY:F2} < surface {waterSurfaceY:F2}");
                        }
                        return false;  // Block death while entering

                    case SwimState.SWIMMING:
                        IsPlayerUnderwater = true;
                        IsSwimmingActive = true;
                        UnderwaterDepth = waterSurfaceY - feetY;

                        // Check if exiting (waist above surface)
                        if (waistY > waterSurfaceY)
                        {
                            _currentSwimState = SwimState.EXITING;
                            _exitGraceFrames = EXIT_GRACE_DURATION;
                            // v10.4.2: CRITICAL FIX - stop animation override immediately!
                            // This allows grounded animation to play as soon as player lands
                            IsSwimmingActive = false;
                            Log.LogInfo($"[SWIM] SWIMMING -> EXITING: waist {waistY:F2} > surface {waterSurfaceY:F2}, grace={EXIT_GRACE_DURATION}");
                        }
                        return false;  // Block death while swimming

                    case SwimState.EXITING:
                        _exitGraceFrames--;

                        // v10.3.0: Count down jump immunity
                        if (_jumpImmunityFrames > 0)
                        {
                            _jumpImmunityFrames--;
                        }

                        // v10.3.0: Re-entry check - only if NOT in jump immunity period
                        // This prevents immediate re-entry right after jumping
                        if (_jumpImmunityFrames <= 0 && waistY < waterSurfaceY)
                        {
                            _currentSwimState = SwimState.SWIMMING;
                            IsSwimmingActive = true;
                            IsPlayerUnderwater = true;
                            Log.LogInfo($"[SWIM] EXITING -> SWIMMING (re-entry): waist {waistY:F2} < surface {waterSurfaceY:F2}");
                            return false;
                        }

                        // Grace period still active - keep protecting
                        if (_exitGraceFrames > 0)
                        {
                            return false;
                        }

                        // v10.1.0: Grace expired - check if safely on ground OR above water
                        bool grounded = GetGroundedState(__instance);
                        bool aboveWater = feetY > waterSurfaceY + SURFACE_MARGIN;

                        // v10.1.0: If grounded (anywhere) OR clearly above water, transition to SURFACE
                        if (grounded || aboveWater)
                        {
                            _currentSwimState = SwimState.SURFACE;
                            FullSwimmingStateReset();
                            Log.LogInfo($"[SWIM] EXITING -> SURFACE: grounded={grounded}, aboveWater={aboveWater}, feet={feetY:F2}");
                            return true;  // Safe, normal death check
                        }

                        // v10.1.0: Still in air/falling - extend protection but limit total time
                        if (_exitGraceFrames > -60)  // Max 3 seconds extra protection
                        {
                            _exitGraceFrames = 10;  // Short extension
                            return false;  // Keep protecting
                        }
                        else
                        {
                            // Failsafe: too long in exit state, force reset
                            Log.LogWarning("[SWIM] EXITING timeout, forcing reset");
                            _currentSwimState = SwimState.SURFACE;
                            FullSwimmingStateReset();
                            return true;
                        }
                }

                return true;  // Fallback
            }
            catch (Exception ex)
            {
                Log.LogError($"[SWIM] ProcessFallOffStage error: {ex.Message}");
                return true;
            }
        }

        // v10.0.0: Cache for debug logging (only log once per water entry)
        private static bool _waterDebugLogged = false;
        private static object _cachedDepthPlane = null;

        /// <summary>
        /// v10.0.0: Get ACTUAL water surface Y from DepthPlane.planeParent.position.y
        /// VERIFIED from DepthPlane.cs lines 366 and 81-85:
        /// - planeParent.transform.position.y = water surface
        /// - GetFallOffHeight() = planeParent.position.y + KillOffset = death zone
        /// </summary>
        private static float GetWaterSurfaceY(object activeStage, float fallbackHeight)
        {
            try
            {
                // v10.0.0: Use Stage.DepthPlane PUBLIC PROPERTY (Stage.cs lines 44-50)
                // NOT the private field!
                var depthPlaneProp = activeStage.GetType().GetProperty("DepthPlane",
                    BindingFlags.Public | BindingFlags.Instance);
                if (depthPlaneProp == null)
                {
                    Log.LogWarning("[WATER] Stage.DepthPlane property not found, trying field fallback");
                    // Fallback to field access
                    var depthPlaneField = activeStage.GetType().GetField("depthPlane",
                        BindingFlags.NonPublic | BindingFlags.Instance);
                    if (depthPlaneField == null) return float.MinValue;
                    _cachedDepthPlane = depthPlaneField.GetValue(activeStage);
                }
                else
                {
                    _cachedDepthPlane = depthPlaneProp.GetValue(activeStage);
                }

                if (_cachedDepthPlane == null) return float.MinValue;

                // v10.0.0: Check OverrideFallOffHeight property (DepthPlane.cs lines 38-44)
                // Returns true when PlaneType > Empty (i.e., Ocean)
                var overrideProp = _cachedDepthPlane.GetType().GetProperty("OverrideFallOffHeight",
                    BindingFlags.Public | BindingFlags.Instance);
                if (overrideProp != null)
                {
                    bool overrides = (bool)overrideProp.GetValue(_cachedDepthPlane);
                    if (!overrides)
                    {
                        // Empty plane type - no water on this stage
                        return float.MinValue;
                    }
                }

                // v10.0.0: Get planeParent PRIVATE field (DepthPlane.cs line 366)
                // [SerializeField] private Transform planeParent;
                var planeParentField = _cachedDepthPlane.GetType().GetField("planeParent",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                if (planeParentField == null)
                {
                    Log.LogError("[WATER] planeParent field not found!");
                    return float.MinValue;
                }

                var planeParent = planeParentField.GetValue(_cachedDepthPlane) as Transform;
                if (planeParent == null)
                {
                    Log.LogError("[WATER] planeParent is null!");
                    return float.MinValue;
                }

                // v10.0.0: THE ACTUAL WATER SURFACE
                float waterSurfaceY = planeParent.position.y;

                // Log debug info on first water detection
                if (!_waterDebugLogged)
                {
                    LogWaterDebugInfo(_cachedDepthPlane, waterSurfaceY, fallbackHeight);
                    _waterDebugLogged = true;
                }

                return waterSurfaceY;
            }
            catch (Exception ex)
            {
                Log.LogError($"[WATER] GetWaterSurfaceY error: {ex.Message}");
                return float.MinValue;  // Return MinValue to indicate no valid water surface
            }
        }

        /// <summary>
        /// v10.0.0: Log KillOffset and other debug values (user requested)
        /// Verified from DepthPlane.cs lines 402-416 (DepthPlaneInfo structure)
        /// </summary>
        private static void LogWaterDebugInfo(object depthPlane, float waterSurfaceY, float deathZoneY)
        {
            try
            {
                Log.LogInfo("========== WATER DEBUG INFO (v10.0.0) ==========");
                Log.LogInfo($"[WATER-DEBUG] Water Surface Y (planeParent.position.y): {waterSurfaceY:F2}");
                Log.LogInfo($"[WATER-DEBUG] Death Zone Y (StageFallOffHeight): {deathZoneY:F2}");
                Log.LogInfo($"[WATER-DEBUG] Calculated KillOffset: {deathZoneY - waterSurfaceY:F2}");
                Log.LogInfo($"[WATER-DEBUG]   (If negative: death below surface = swimming safe)");
                Log.LogInfo($"[WATER-DEBUG]   (If positive: death above surface = dangerous!)");
                Log.LogInfo($"[WATER-DEBUG]   (If zero: death at surface = no swim zone)");

                // Get visuals list to log actual KillOffset from DepthPlaneInfo
                var visualsField = depthPlane.GetType().GetField("visuals",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                var visuals = visualsField?.GetValue(depthPlane) as IList;

                if (visuals != null && visuals.Count > 0)
                {
                    Log.LogInfo("[WATER-DEBUG] DepthPlaneInfo entries:");
                    foreach (var info in visuals)
                    {
                        if (info == null) continue;

                        var planeTypeField = info.GetType().GetField("PlaneType");
                        var killOffsetField = info.GetType().GetField("KillOffset");

                        if (planeTypeField != null && killOffsetField != null)
                        {
                            int planeType = (int)planeTypeField.GetValue(info);
                            float killOffset = (float)killOffsetField.GetValue(info);
                            string planeTypeName = planeType == 0 ? "Empty" : planeType == 1 ? "Ocean" : $"Unknown({planeType})";
                            Log.LogInfo($"[WATER-DEBUG]   PlaneType={planeTypeName}, KillOffset={killOffset:F2}");
                        }
                    }
                }
                else
                {
                    Log.LogWarning("[WATER-DEBUG] Could not read visuals list");
                }

                Log.LogInfo("=================================================");
            }
            catch (Exception ex)
            {
                Log.LogError($"[WATER-DEBUG] Error logging debug info: {ex.Message}");
            }
        }

        /// <summary>
        /// v9.0.0: Get Grounded state from NetState
        /// </summary>
        private static bool GetGroundedState(object playerController)
        {
            try
            {
                if (_currentStateField == null) return false;

                object boxedState = _currentStateField.GetValue(playerController);
                if (boxedState == null) return false;

                var groundedField = boxedState.GetType().GetField("Grounded",
                    BindingFlags.Public | BindingFlags.Instance);
                if (groundedField == null) return false;

                return (bool)groundedField.GetValue(boxedState);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// v9.0.0: Surface jump using state machine
        /// Only allows jump when in SWIMMING state and near surface
        /// Transitions to EXITING state on successful jump
        /// </summary>
        public static void ProcessJump_Prefix(object __instance)
        {
            try
            {
                // v9.0.0: Only enable surface jump in SWIMMING state
                if (_currentSwimState != SwimState.SWIMMING)
                    return;

                if (_currentStateField == null) return;

                var transform = (__instance as MonoBehaviour)?.transform;
                if (transform == null) return;

                // Calculate waist position for surface check
                CacheCharacterController(__instance);
                float feetY = transform.position.y;
                float waistY = feetY + (_characterHeight * 0.5f - _characterRadius);
                float surfaceY = WaterSurfaceY;

                // v10.3.0: Allow jump when at swim ceiling (not just surface)
                // Swim ceiling puts waist at (surfaceY - SUBMERSION_DEPTH - small margin)
                float swimCeilingWaist = surfaceY - SUBMERSION_DEPTH;
                if (waistY < swimCeilingWaist - 0.3f) return;  // Must be near swim ceiling or above

                // Get boxed state
                object boxedState = _currentStateField.GetValue(__instance);
                if (boxedState == null) return;

                // Reset FramesSinceStableGround to 0 (enables coyote time jump)
                if (_framesSinceStableGroundField != null)
                {
                    _framesSinceStableGroundField.SetValue(boxedState, 0);
                }

                // Check for Space key to apply jump force
                if (Input.GetKey(KeyCode.Space))
                {
                    // v10.3.0: Apply jump force
                    if (_totalForceField != null)
                    {
                        Vector3 totalForce = (Vector3)_totalForceField.GetValue(boxedState);
                        totalForce.y = JUMP_FORCE;
                        _totalForceField.SetValue(boxedState, totalForce);
                    }

                    // Transition to EXITING state with extended grace period
                    _currentSwimState = SwimState.EXITING;
                    _exitGraceFrames = EXIT_GRACE_DURATION * 2;

                    // v10.4.3: CRITICAL FIX - stop animation override on jump exit!
                    IsSwimmingActive = false;

                    // v10.3.0: Set jump immunity to prevent immediate re-entry
                    _jumpImmunityFrames = JUMP_IMMUNITY_DURATION;

                    Log.LogInfo($"[SWIM-JUMP] Surface jump! waist={waistY:F2}, ceiling={swimCeilingWaist:F2}, force={JUMP_FORCE}");
                }

                // Write back
                _currentStateField.SetValue(__instance, boxedState);
            }
            catch (Exception)
            {
                // Silent
            }
        }

        /// <summary>
        /// v10.2.0: Animation parameter manipulation for horizontal swimming pose
        /// v10.5.0: Added falling animation diagnostic capture
        /// </summary>
        public static void SetAnimationState_Prefix(
            object __instance,
            ref float rotation,
            ref bool moving,
            ref bool walking,
            ref bool grounded,
            ref float slopeAngle,
            ref float airTime,
            ref float fallTime,
            ref Vector2 worldVelocity,
            ref float velX,
            ref float velY,
            ref float velZ,
            ref float angularVelocity,
            ref float horizontalVelMagnitude,
            ref string interactorToggleString,
            ref int comboBookmark,
            ref bool ghostmode,
            ref bool ads,
            ref float playerAngleDot,
            ref Vector3 aimPoint,
            ref bool useIK)
        {
            try
            {
                // v10.5.0: FALLING ANIMATION DIAGNOSTIC
                // Capture animation info when falling from height (horizontal dive pose)
                // Trigger: not grounded, falling (velY < -5), high fallTime (> 0.5)
                if (!grounded && velY < -5f && fallTime > 0.5f)
                {
                    // Log when fallTime increases significantly (new fall or deeper fall)
                    if (fallTime > _lastFallTime + 0.3f || !_fallAnimLogged)
                    {
                        Log.LogInfo("========== FALLING ANIMATION CAPTURE (v10.5.0) ==========");
                        Log.LogInfo($"[FALL-ANIM] grounded={grounded}, airTime={airTime:F2}, fallTime={fallTime:F2}");
                        Log.LogInfo($"[FALL-ANIM] velX={velX:F2}, velY={velY:F2}, velZ={velZ:F2}");
                        Log.LogInfo($"[FALL-ANIM] horizontalVelMagnitude={horizontalVelMagnitude:F2}");
                        Log.LogInfo($"[FALL-ANIM] moving={moving}, walking={walking}, ghostmode={ghostmode}");
                        Log.LogInfo($"[FALL-ANIM] slopeAngle={slopeAngle:F2}, rotation={rotation:F2}");
                        Log.LogInfo($"[FALL-ANIM] playerAngleDot={playerAngleDot:F2}, useIK={useIK}");

                        // Try to get Animator info
                        CacheAndLogAnimator(__instance);

                        Log.LogInfo("==========================================================");
                        _fallAnimLogged = true;
                        _lastFallTime = fallTime;
                    }
                }

                // Reset fall logging when grounded
                if (grounded && _fallAnimLogged)
                {
                    Log.LogInfo("[FALL-ANIM] Landed - resetting fall animation capture");
                    _fallAnimLogged = false;
                    _lastFallTime = 0f;
                }

                // v10.15.0: RUNTIME STATE HASH CAPTURE
                // Capture locomotion hash when player is MOVING on ground (not at initialization)
                if (!IsSwimmingActive && _cachedAnimator != null)
                {
                    if (grounded)
                    {
                        _framesSinceGrounded++;
                        // Capture locomotion hash when player is moving AND grounded (skip initialization)
                        if (_framesSinceGrounded > 30 && moving && horizontalVelMagnitude > 0.5f && !_idleHashCaptured)
                        {
                            var stateInfo = _cachedAnimator.GetCurrentAnimatorStateInfo(0);
                            _idleStateHash = stateInfo.shortNameHash;
                            _idleHashCaptured = true;
                            Log.LogInfo($"[HASH-CAPTURE] LOCOMOTION state captured (moving): hash={_idleStateHash}");
                        }
                    }
                    else
                    {
                        _framesSinceGrounded = 0;
                    }

                    // Capture fall hash when airborne with any downward velocity
                    if (!grounded && velY < -2f && fallTime > 0.1f && !_fallHashCaptured)
                    {
                        var stateInfo = _cachedAnimator.GetCurrentAnimatorStateInfo(0);
                        _fallStateHash = stateInfo.shortNameHash;
                        _fallHashCaptured = true;
                        Log.LogInfo($"[HASH-CAPTURE] FALL state captured: hash={_fallStateHash}");
                    }
                }

                // v10.4.1: DIAGNOSTIC LOGGING (swim state changes)
                if (_currentSwimState != _lastLoggedSwimState)
                {
                    Log.LogInfo($"[ANIM-DIAG] SwimState changed: {_lastLoggedSwimState} -> {_currentSwimState}");
                    Log.LogInfo($"[ANIM-DIAG]   IsSwimmingActive={IsSwimmingActive}, grounded(input)={grounded}, airTime={airTime:F2}");
                    _lastLoggedSwimState = _currentSwimState;
                    _animLogCounter = 0;
                }

                if (grounded != _lastGroundedState)
                {
                    _lastGroundedState = grounded;
                }

                // v10.15.0: Cache animator (re-cache if destroyed after respawn)
                if (!_animatorCached || _cachedAnimator == null)
                {
                    var mono = __instance as MonoBehaviour;
                    if (mono != null)
                    {
                        _cachedAnimator = mono.GetComponent<Animator>();
                        if (_cachedAnimator == null)
                            _cachedAnimator = mono.GetComponentInChildren<Animator>();
                    }
                    _animatorCached = true;
                    if (_cachedAnimator != null)
                        Log.LogInfo($"[ANIM-DIAG] Animator cached: {_cachedAnimator.name}");
                    else
                        Log.LogWarning("[ANIM-DIAG] Failed to cache animator - null!");
                }

                // v10.17.0: COMPREHENSIVE NATIVE SWIMMING SYSTEM
                // Uses the game's built-in swimming animations via triggers and state hashes

                bool isInWater = IsSwimmingActive;

                if (!isInWater)
                {
                    // === EXITING WATER ===
                    if (_wasInWater && _cachedAnimator != null)
                    {
                        Log.LogInfo("[SWIM-ANIM] Exiting water - firing WaterExit trigger");
                        _cachedAnimator.SetBool("WaterIn", false);
                        _cachedAnimator.SetBool("WaterInBelow", false);
                        _cachedAnimator.SetBool("WaterBelow", false);
                        _cachedAnimator.SetTrigger("WaterExit");
                    }
                    _wasInWater = false;
                    _wasUnderwater = false;
                    _isMovingInWater = false;
                    _smoothedVelY = 0f;  // v10.19.0: Reset smooth velY

                    // v10.34.0: Periodic logging of animator state when NOT swimming
                    // Logs every 200 frames when moving, to help debug animation stuck issues
                    if (moving && _cachedAnimator != null && _animLogCounter++ % 200 == 0)
                    {
                        int equippedItem = _cachedAnimator.GetInteger("EquippedItem");
                        var stateInfo = _cachedAnimator.GetCurrentAnimatorStateInfo(0);
                        Log.LogInfo($"[ANIM-STATE] NotSwimming: moving={moving}, grounded={grounded}, velX={velX:F2}, velZ={velZ:F2}, HorizVel={horizontalVelMagnitude:F2}");
                        Log.LogInfo($"[ANIM-STATE]   EquippedItem={equippedItem}, StateHash={stateInfo.shortNameHash}, NormTime={stateInfo.normalizedTime:F2}");
                    }

                    return;
                }

                // === IN WATER ===
                grounded = false;
                walking = false;
                // Keep 'moving' based on actual input for swim blendtrees

                float diveDepth = UnderwaterDepth;
                const float DIVE_THRESHOLD = 1.5f;
                bool isUnderwater = diveDepth > DIVE_THRESHOLD;
                bool isMoving = horizontalVelMagnitude > 0.1f;

                if (_cachedAnimator != null)
                {
                    // === WATER ENTRY (first frame in water) ===
                    if (!_wasInWater)
                    {
                        Log.LogInfo($"[SWIM-ANIM] Entering water - depth={diveDepth:F2}, fallTime={fallTime:F2}");
                        _cachedAnimator.SetBool("WaterIn", true);
                        _cachedAnimator.SetBool("WaterBelow", true);

                        // Fire WaterEnter trigger - the animator will choose Enter Water or Enter Water High
                        // based on FallTime (high entry if FallTime > 1)
                        _cachedAnimator.SetTrigger("WaterEnter");
                        Log.LogInfo("[SWIM-ANIM] Fired WaterEnter trigger");

                        // If entering while already deep, also set underwater state
                        if (isUnderwater)
                        {
                            _cachedAnimator.SetBool("WaterInBelow", true);
                            _cachedAnimator.SetTrigger("WaterDive");
                            Log.LogInfo("[SWIM-ANIM] Deep entry - also fired WaterDive trigger");
                        }

                        _wasInWater = true;
                        _wasUnderwater = isUnderwater;
                    }
                    else
                    {
                        // === CONTINUOUS WATER STATE ===
                        _cachedAnimator.SetBool("WaterIn", true);
                        _cachedAnimator.SetBool("WaterBelow", true);
                        _cachedAnimator.SetBool("WaterInBelow", isUnderwater);

                        // === DIVE/BREACH TRANSITIONS ===
                        if (isUnderwater != _wasUnderwater)
                        {
                            if (isUnderwater)
                            {
                                // Surface -> Underwater: DIVE
                                Log.LogInfo($"[SWIM-ANIM] DIVING - depth={diveDepth:F2}");
                                _cachedAnimator.SetTrigger("WaterDive");

                                // Also force state via CrossFade for reliability
                                _cachedAnimator.CrossFadeInFixedTime(HASH_DIVE, 0.3f, 0);
                            }
                            else
                            {
                                // Underwater -> Surface: BREACH
                                Log.LogInfo($"[SWIM-ANIM] BREACHING - depth={diveDepth:F2}");
                                _cachedAnimator.SetTrigger("WaterBreach");

                                // Also force state via CrossFade for reliability
                                _cachedAnimator.CrossFadeInFixedTime(HASH_BREACH, 0.3f, 0);
                            }
                            _wasUnderwater = isUnderwater;
                        }

                        // === MOVEMENT HANDLING ===
                        // The blendtrees should handle movement automatically via velocity params
                        // but we can also help by transitioning to swim start states
                        if (isMoving != _isMovingInWater)
                        {
                            if (isMoving)
                            {
                                // Started moving in water
                                int swimStartHash = isUnderwater ? HASH_BELOW_WATER_SWIM_START : HASH_ABOVE_WATER_SWIM_START;
                                Log.LogInfo($"[SWIM-ANIM] Started swimming - underwater={isUnderwater}");
                                _cachedAnimator.CrossFadeInFixedTime(swimStartHash, 0.2f, 0);
                            }
                            // Stopping handled automatically by blendtree or idle transition
                            _isMovingInWater = isMoving;
                        }
                    }

                    // Log state periodically for debugging
                    if (_diveLogThrottle++ % 100 == 0)
                    {
                        var stateInfo = _cachedAnimator.GetCurrentAnimatorStateInfo(0);
                        Log.LogInfo($"[SWIM-STATE] hash={stateInfo.shortNameHash}, depth={diveDepth:F2}, underwater={isUnderwater}, moving={isMoving}");
                    }
                }

                // v10.19.0: Set animation parameters with SMOOTH LERPING
                // BlendTrees use VelY to choose between swim up/down/forward animations
                float targetVelY = 0f;

                if (isUnderwater)
                {
                    // Underwater - VelY controls animation direction in BlendTree:
                    // - Positive VelY = swimming up animations
                    // - Negative VelY = swimming down animations
                    // - Zero VelY = forward swimming animations
                    float verticalDirection = (float)_swimVerticalMotor / (float)_swimMotorFrames;
                    targetVelY = verticalDirection * SwimmingVerticalSpeed;

                    // Keep airTime/fallTime for state machine support
                    airTime = 0.5f;
                    fallTime = isMoving ? 0f : 1.5f;  // Only set fallTime high when idle (for dive pose)
                }
                else
                {
                    // At surface - neutral parameters
                    airTime = 0.2f;
                    fallTime = 0f;
                    targetVelY = 0f;
                }

                // v10.19.0: Smooth lerp velY for seamless animation blending
                _smoothedVelY = Mathf.Lerp(_smoothedVelY, targetVelY, Time.deltaTime * VELY_LERP_SPEED);
                velY = _smoothedVelY;
            }
            catch (Exception ex)
            {
                Log.LogError($"[ANIM] Error: {ex.Message}");
            }
        }

        /// <summary>
        /// v10.8.0: Cache Animator and discover available states
        /// </summary>
        private static void CacheAnimatorAndLogStates(object appearanceAnimator)
        {
            try
            {
                if (!_animatorCached)
                {
                    var mono = appearanceAnimator as MonoBehaviour;
                    if (mono != null)
                    {
                        _cachedAnimator = mono.GetComponent<Animator>();
                        if (_cachedAnimator == null)
                        {
                            _cachedAnimator = mono.GetComponentInChildren<Animator>();
                        }
                    }
                    _animatorCached = true;

                    if (_cachedAnimator != null)
                    {
                        Log.LogInfo($"[ANIM-CTRL] Animator cached: {_cachedAnimator.name}");
                    }
                }

                // Log all states once
                if (_cachedAnimator != null && !_animatorStatesLogged)
                {
                    _animatorStatesLogged = true;

                    Log.LogInfo("========== ANIMATOR STATES DISCOVERY (v10.8.0) ==========");

                    // Get current state info
                    var currentState = _cachedAnimator.GetCurrentAnimatorStateInfo(0);
                    Log.LogInfo($"[ANIM-CTRL] Current state hash: {currentState.shortNameHash}");
                    Log.LogInfo($"[ANIM-CTRL] Current fullPath hash: {currentState.fullPathHash}");

                    // Get clip info
                    var clips = _cachedAnimator.GetCurrentAnimatorClipInfo(0);
                    foreach (var clip in clips)
                    {
                        if (clip.clip != null)
                        {
                            Log.LogInfo($"[ANIM-CTRL] Current clip: {clip.clip.name}");
                        }
                    }

                    // Log all parameters
                    Log.LogInfo("[ANIM-CTRL] Parameters:");
                    foreach (var param in _cachedAnimator.parameters)
                    {
                        Log.LogInfo($"[ANIM-CTRL]   {param.name} ({param.type})");
                    }

                    // Try common state name hashes
                    string[] fallNames = { "Fall", "Falling", "FallLoop", "AirFall", "FreeFall", "Dive", "FallIdle" };
                    string[] idleNames = { "Idle", "Standing", "Grounded", "Locomotion", "Blend Tree", "Movement" };

                    foreach (var name in fallNames)
                    {
                        int hash = Animator.StringToHash(name);
                        if (_cachedAnimator.HasState(0, hash))
                        {
                            Log.LogInfo($"[ANIM-CTRL] FOUND FALL STATE: {name} (hash={hash})");
                            if (_fallStateHash == 0) _fallStateHash = hash;
                        }
                    }

                    foreach (var name in idleNames)
                    {
                        int hash = Animator.StringToHash(name);
                        if (_cachedAnimator.HasState(0, hash))
                        {
                            Log.LogInfo($"[ANIM-CTRL] FOUND IDLE STATE: {name} (hash={hash})");
                            if (_idleStateHash == 0) _idleStateHash = hash;
                        }
                    }

                    Log.LogInfo($"[ANIM-CTRL] Using fallStateHash={_fallStateHash}, idleStateHash={_idleStateHash}");
                    Log.LogInfo("==========================================================");
                }
            }
            catch (Exception ex)
            {
                Log.LogError($"[ANIM-CTRL] Error: {ex.Message}");
            }
        }

        /// <summary>
        /// v10.5.0: Cache Animator and log current animation state info
        /// </summary>
        private static void CacheAndLogAnimator(object appearanceAnimator)
        {
            try
            {
                if (!_animatorCached)
                {
                    // Try to find Animator component on the AppearanceAnimator or its children
                    var mono = appearanceAnimator as MonoBehaviour;
                    if (mono != null)
                    {
                        _cachedAnimator = mono.GetComponent<Animator>();
                        if (_cachedAnimator == null)
                        {
                            _cachedAnimator = mono.GetComponentInChildren<Animator>();
                        }
                    }
                    _animatorCached = true;

                    if (_cachedAnimator != null)
                    {
                        Log.LogInfo($"[FALL-ANIM] Animator found: {_cachedAnimator.name}");
                    }
                    else
                    {
                        Log.LogWarning("[FALL-ANIM] No Animator component found");
                    }
                }

                if (_cachedAnimator != null && _cachedAnimator.isActiveAndEnabled)
                {
                    // Log current animator state info
                    var stateInfo = _cachedAnimator.GetCurrentAnimatorStateInfo(0);
                    var clipInfo = _cachedAnimator.GetCurrentAnimatorClipInfo(0);

                    Log.LogInfo($"[FALL-ANIM] Animator Layer 0 State:");
                    Log.LogInfo($"[FALL-ANIM]   stateInfo.fullPathHash={stateInfo.fullPathHash}");
                    Log.LogInfo($"[FALL-ANIM]   stateInfo.shortNameHash={stateInfo.shortNameHash}");
                    Log.LogInfo($"[FALL-ANIM]   stateInfo.normalizedTime={stateInfo.normalizedTime:F2}");
                    Log.LogInfo($"[FALL-ANIM]   stateInfo.length={stateInfo.length:F2}");
                    Log.LogInfo($"[FALL-ANIM]   stateInfo.speed={stateInfo.speed:F2}");
                    Log.LogInfo($"[FALL-ANIM]   stateInfo.loop={stateInfo.loop}");

                    if (clipInfo.Length > 0)
                    {
                        foreach (var clip in clipInfo)
                        {
                            if (clip.clip != null)
                            {
                                Log.LogInfo($"[FALL-ANIM]   CLIP: {clip.clip.name} (weight={clip.weight:F2})");
                            }
                        }
                    }
                    else
                    {
                        Log.LogInfo("[FALL-ANIM]   No clip info available (may be using state name)");
                    }

                    // Log animator parameters
                    Log.LogInfo("[FALL-ANIM] Animator Parameters:");
                    foreach (var param in _cachedAnimator.parameters)
                    {
                        switch (param.type)
                        {
                            case AnimatorControllerParameterType.Float:
                                Log.LogInfo($"[FALL-ANIM]   {param.name} (float) = {_cachedAnimator.GetFloat(param.name):F2}");
                                break;
                            case AnimatorControllerParameterType.Int:
                                Log.LogInfo($"[FALL-ANIM]   {param.name} (int) = {_cachedAnimator.GetInteger(param.name)}");
                                break;
                            case AnimatorControllerParameterType.Bool:
                                Log.LogInfo($"[FALL-ANIM]   {param.name} (bool) = {_cachedAnimator.GetBool(param.name)}");
                                break;
                            case AnimatorControllerParameterType.Trigger:
                                Log.LogInfo($"[FALL-ANIM]   {param.name} (trigger)");
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.LogError($"[FALL-ANIM] Animator access error: {ex.Message}");
            }
        }

        /// <summary>
        /// v6.0.0: Reduce shader blur on Ocean_Plane
        /// </summary>
        public static void DepthPlane_Start_Postfix(object __instance)
        {
            try
            {
                if (_shaderAlreadyModified)
                {
                    Log.LogInfo("[SHADER] Already modified this session, skipping");
                    return;
                }

                Log.LogInfo("[SHADER] DepthPlane.Start - Modifying Ocean_Plane shader");

                var depthPlane = __instance as MonoBehaviour;
                if (depthPlane == null) return;

                var oceanPlane = FindChildRecursive(depthPlane.transform, "Ocean_Plane");
                if (oceanPlane == null)
                {
                    Log.LogWarning("[SHADER] Could not find Ocean_Plane child");
                    return;
                }

                var renderer = oceanPlane.GetComponent<MeshRenderer>();
                if (renderer == null)
                {
                    Log.LogWarning("[SHADER] Ocean_Plane has no MeshRenderer");
                    return;
                }

                var sharedMaterials = renderer.sharedMaterials;
                bool anyModified = false;

                for (int i = 0; i < sharedMaterials.Length; i++)
                {
                    var material = sharedMaterials[i];
                    if (material == null) continue;

                    Log.LogInfo($"[SHADER] Material[{i}]: {material.name}, shader: {material.shader?.name}");

                    if (material.HasProperty("_Refraction"))
                    {
                        float oldVal = material.GetFloat("_Refraction");
                        material.SetFloat("_Refraction", 0f);
                        Log.LogInfo($"[SHADER]   _Refraction: {oldVal:F3} -> 0.000");
                        anyModified = true;
                    }

                    if (material.HasProperty("_Depth"))
                    {
                        float oldVal = material.GetFloat("_Depth");
                        material.SetFloat("_Depth", oldVal * 0.2f);
                        Log.LogInfo($"[SHADER]   _Depth: {oldVal:F3} -> {oldVal * 0.2f:F3}");
                        anyModified = true;
                    }

                    if (material.HasProperty("_Displacement"))
                    {
                        float oldVal = material.GetFloat("_Displacement");
                        material.SetFloat("_Displacement", oldVal * 0.1f);
                        Log.LogInfo($"[SHADER]   _Displacement: {oldVal:F3} -> {oldVal * 0.1f:F3}");
                        anyModified = true;
                    }
                }

                if (anyModified)
                {
                    _shaderAlreadyModified = true;
                }

                DisableDeeperPlane(depthPlane);
            }
            catch (Exception ex)
            {
                Log.LogError($"[SHADER] Error: {ex}");
            }
        }

        private static void DisableDeeperPlane(MonoBehaviour depthPlane)
        {
            try
            {
                var visualsField = AccessTools.Field(_depthPlaneType, "visuals");
                if (visualsField == null) return;

                var visuals = visualsField.GetValue(depthPlane) as IList;
                if (visuals == null || visuals.Count == 0) return;

                foreach (var planeInfo in visuals)
                {
                    if (planeInfo == null) continue;

                    var deeperPlaneField = planeInfo.GetType().GetField("DeeperPlane");
                    if (deeperPlaneField == null) continue;

                    var deeperPlane = deeperPlaneField.GetValue(planeInfo) as GameObject;
                    if (deeperPlane == null) continue;

                    var deeperRenderer = deeperPlane.GetComponent<MeshRenderer>();
                    if (deeperRenderer != null)
                    {
                        deeperRenderer.enabled = false;
                        Log.LogInfo($"[DEEPER] Disabled DeeperPlane renderer: {deeperPlane.name}");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.LogError($"[DEEPER] Error: {ex.Message}");
            }
        }

        private static bool _filterLoggedOnce = false;

        public static bool StartTransition_Prefix(object filterType)
        {
            try
            {
                int filterValue = (int)filterType;

                if (filterValue == 3) // Blurred
                {
                    if (!_filterLoggedOnce)
                    {
                        Log.LogInfo("[FILTER] Blocked Blurred filter transition");
                        _filterLoggedOnce = true;
                    }
                    return false;
                }

                return true;
            }
            catch (Exception)
            {
                return true;
            }
        }

        /// <summary>
        /// v10.20.0: Comprehensive Volume diagnostics - logs ALL post-processing data
        /// </summary>
        private static void LogAllVolumeData()
        {
            if (_volumeDiagnosticsLogged) return;
            _volumeDiagnosticsLogged = true;

            try
            {
                Log.LogInfo("==========================================================");
                Log.LogInfo("[VOLUME-DIAG] ===== POST-PROCESSING VOLUME DIAGNOSTICS =====");
                Log.LogInfo("==========================================================");

                // Find Volume type
                Type volumeType = AccessTools.TypeByName("UnityEngine.Rendering.Volume");
                if (volumeType == null)
                {
                    foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        if (assembly.FullName.Contains("RenderPipelines") || assembly.FullName.Contains("Rendering"))
                        {
                            volumeType = assembly.GetType("UnityEngine.Rendering.Volume");
                            if (volumeType != null) break;
                        }
                    }
                }

                if (volumeType == null)
                {
                    Log.LogWarning("[VOLUME-DIAG] Volume type NOT FOUND in any assembly");
                    return;
                }

                Log.LogInfo($"[VOLUME-DIAG] Volume type found: {volumeType.FullName}");

                // Get Volume properties/fields
                var weightProp = volumeType.GetProperty("weight");
                var priorityProp = volumeType.GetProperty("priority");
                var isGlobalProp = volumeType.GetProperty("isGlobal");
                var profileProp = volumeType.GetProperty("profile");
                var sharedProfileProp = volumeType.GetProperty("sharedProfile");

                Log.LogInfo($"[VOLUME-DIAG] Properties found: weight={weightProp != null}, priority={priorityProp != null}, isGlobal={isGlobalProp != null}, profile={profileProp != null}");

                // Find all volumes
                var allVolumes = UnityEngine.Object.FindObjectsOfType(volumeType);
                Log.LogInfo($"[VOLUME-DIAG] Total Volumes in scene: {allVolumes.Length}");
                Log.LogInfo("----------------------------------------------------------");

                int volIndex = 0;
                foreach (var volume in allVolumes)
                {
                    var volMono = volume as MonoBehaviour;
                    if (volMono == null) continue;

                    volIndex++;
                    string volName = volMono.name;
                    string goPath = GetGameObjectPath(volMono.gameObject);
                    bool isEnabled = volMono.enabled;

                    Log.LogInfo($"[VOLUME-DIAG] [{volIndex}] === {volName} ===");
                    Log.LogInfo($"[VOLUME-DIAG]     Path: {goPath}");
                    Log.LogInfo($"[VOLUME-DIAG]     Enabled: {isEnabled}");

                    // Get weight
                    if (weightProp != null)
                    {
                        float weight = (float)weightProp.GetValue(volume);
                        Log.LogInfo($"[VOLUME-DIAG]     Weight: {weight:F3}");
                    }

                    // Get priority
                    if (priorityProp != null)
                    {
                        float priority = (float)priorityProp.GetValue(volume);
                        Log.LogInfo($"[VOLUME-DIAG]     Priority: {priority}");
                    }

                    // Get isGlobal
                    if (isGlobalProp != null)
                    {
                        bool isGlobal = (bool)isGlobalProp.GetValue(volume);
                        Log.LogInfo($"[VOLUME-DIAG]     IsGlobal: {isGlobal}");
                    }

                    // Get profile
                    if (profileProp != null)
                    {
                        var profile = profileProp.GetValue(volume);
                        if (profile != null)
                        {
                            string profileName = profile.ToString();
                            Log.LogInfo($"[VOLUME-DIAG]     Profile: {profileName}");

                            // Try to get profile components (overrides)
                            LogVolumeProfileContents(profile);
                        }
                        else
                        {
                            Log.LogInfo($"[VOLUME-DIAG]     Profile: NULL");
                        }
                    }

                    // Get sharedProfile (may be different)
                    if (sharedProfileProp != null)
                    {
                        var sharedProfile = sharedProfileProp.GetValue(volume);
                        if (sharedProfile != null)
                        {
                            string sharedName = sharedProfile.ToString();
                            Log.LogInfo($"[VOLUME-DIAG]     SharedProfile: {sharedName}");
                        }
                    }

                    Log.LogInfo("----------------------------------------------------------");
                }

                // Also log any VolumeProfile assets we can find
                LogAllVolumeProfiles();

                Log.LogInfo("==========================================================");
                Log.LogInfo("[VOLUME-DIAG] ===== END VOLUME DIAGNOSTICS =====");
                Log.LogInfo("==========================================================");
            }
            catch (Exception ex)
            {
                Log.LogError($"[VOLUME-DIAG] Error: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private static string GetGameObjectPath(GameObject go)
        {
            string path = go.name;
            Transform parent = go.transform.parent;
            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }
            return path;
        }

        private static void LogVolumeProfileContents(object profile)
        {
            try
            {
                // VolumeProfile has a 'components' list containing VolumeComponents
                Type profileType = profile.GetType();
                var componentsProp = profileType.GetProperty("components");

                if (componentsProp == null)
                {
                    // Try field
                    var componentsField = profileType.GetField("components", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (componentsField != null)
                    {
                        var components = componentsField.GetValue(profile) as System.Collections.IList;
                        if (components != null)
                        {
                            Log.LogInfo($"[VOLUME-DIAG]     Effects ({components.Count}):");
                            foreach (var comp in components)
                            {
                                LogVolumeComponent(comp);
                            }
                        }
                    }
                    return;
                }

                var compList = componentsProp.GetValue(profile) as System.Collections.IList;
                if (compList != null)
                {
                    Log.LogInfo($"[VOLUME-DIAG]     Effects ({compList.Count}):");
                    foreach (var comp in compList)
                    {
                        LogVolumeComponent(comp);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.LogWarning($"[VOLUME-DIAG]     Could not read profile contents: {ex.Message}");
            }
        }

        private static void LogVolumeComponent(object comp)
        {
            try
            {
                if (comp == null) return;

                Type compType = comp.GetType();
                string typeName = compType.Name;

                // Check if active
                var activeProp = compType.GetProperty("active");
                bool isActive = activeProp != null ? (bool)activeProp.GetValue(comp) : true;

                Log.LogInfo($"[VOLUME-DIAG]       - {typeName} (active={isActive})");

                // Log specific settings for common effects
                if (typeName.Contains("Bloom"))
                {
                    LogEffectParameter(comp, "intensity");
                    LogEffectParameter(comp, "threshold");
                    LogEffectParameter(comp, "scatter");
                }
                else if (typeName.Contains("DepthOfField") || typeName.Contains("DOF"))
                {
                    LogEffectParameter(comp, "mode");
                    LogEffectParameter(comp, "focusDistance");
                    LogEffectParameter(comp, "aperture");
                    LogEffectParameter(comp, "focalLength");
                }
                else if (typeName.Contains("ColorAdjustments") || typeName.Contains("ColorGrading"))
                {
                    LogEffectParameter(comp, "postExposure");
                    LogEffectParameter(comp, "contrast");
                    LogEffectParameter(comp, "saturation");
                }
                else if (typeName.Contains("Vignette"))
                {
                    LogEffectParameter(comp, "intensity");
                    LogEffectParameter(comp, "smoothness");
                }
                else if (typeName.Contains("ChromaticAberration"))
                {
                    LogEffectParameter(comp, "intensity");
                }
                else if (typeName.Contains("Fog") || typeName.Contains("Underwater"))
                {
                    // Log all fields for underwater/fog effects
                    LogAllEffectParameters(comp);
                }
            }
            catch (Exception ex)
            {
                Log.LogWarning($"[VOLUME-DIAG]       Error reading component: {ex.Message}");
            }
        }

        private static void LogEffectParameter(object comp, string paramName)
        {
            try
            {
                Type compType = comp.GetType();
                var field = compType.GetField(paramName, BindingFlags.Public | BindingFlags.Instance);
                if (field != null)
                {
                    var paramObj = field.GetValue(comp);
                    if (paramObj != null)
                    {
                        // VolumeParameters have a 'value' property
                        var valueProp = paramObj.GetType().GetProperty("value");
                        if (valueProp != null)
                        {
                            var value = valueProp.GetValue(paramObj);
                            Log.LogInfo($"[VOLUME-DIAG]           {paramName}: {value}");
                        }
                        else
                        {
                            Log.LogInfo($"[VOLUME-DIAG]           {paramName}: {paramObj}");
                        }
                    }
                }
            }
            catch { }
        }

        private static void LogAllEffectParameters(object comp)
        {
            try
            {
                Type compType = comp.GetType();
                var fields = compType.GetFields(BindingFlags.Public | BindingFlags.Instance);
                foreach (var field in fields)
                {
                    if (field.Name == "active") continue;
                    try
                    {
                        var paramObj = field.GetValue(comp);
                        if (paramObj != null)
                        {
                            var valueProp = paramObj.GetType().GetProperty("value");
                            if (valueProp != null)
                            {
                                var value = valueProp.GetValue(paramObj);
                                Log.LogInfo($"[VOLUME-DIAG]           {field.Name}: {value}");
                            }
                        }
                    }
                    catch { }
                }
            }
            catch { }
        }

        private static void LogAllVolumeProfiles()
        {
            try
            {
                Type profileType = AccessTools.TypeByName("UnityEngine.Rendering.VolumeProfile");
                if (profileType == null) return;

                var allProfiles = Resources.FindObjectsOfTypeAll(profileType);
                Log.LogInfo($"[VOLUME-DIAG] Found {allProfiles.Length} VolumeProfile assets in memory:");

                foreach (var profile in allProfiles)
                {
                    var assetName = (profile as UnityEngine.Object)?.name ?? "Unknown";
                    Log.LogInfo($"[VOLUME-DIAG]   - {assetName}");
                }
            }
            catch (Exception ex)
            {
                Log.LogWarning($"[VOLUME-DIAG] Could not enumerate VolumeProfiles: {ex.Message}");
            }
        }

        // v10.22.0: Underwater effect settings
        private const float UNDERWATER_BLUR_INTENSITY = 0.35f;  // Blur amount (0 = none, 1 = full)

        // Ocean blue tint color (RGB values 0-1)
        private static readonly Color UNDERWATER_TINT = new Color(0.4f, 0.7f, 0.9f, 1f);  // Light ocean blue
        private static object _originalColorFilter = null;
        private static bool _colorFilterModified = false;

        private static void TryReduceBlur()
        {
            // v10.20.0: Log diagnostics before modifying anything
            LogAllVolumeData();

            try
            {
                Type volumeType = null;

                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (assembly.FullName.Contains("RenderPipelines") || assembly.FullName.Contains("Rendering"))
                    {
                        volumeType = assembly.GetType("UnityEngine.Rendering.Volume");
                        if (volumeType != null) break;
                    }
                }

                if (volumeType == null)
                {
                    volumeType = AccessTools.TypeByName("UnityEngine.Rendering.Volume");
                }

                if (volumeType == null)
                {
                    Log.LogWarning("[BLUR] Volume type not found");
                    _blurReductionApplied = true;
                    return;
                }

                var weightField = volumeType.GetField("weight", BindingFlags.Public | BindingFlags.Instance);
                if (weightField == null)
                {
                    var weightProp = volumeType.GetProperty("weight", BindingFlags.Public | BindingFlags.Instance);
                    if (weightProp != null)
                    {
                        Log.LogInfo("[BLUR] weight is a property, not a field");
                        TryReduceBlurViaProperty(volumeType, weightProp);
                        return;
                    }

                    Log.LogWarning("[BLUR] Volume.weight field/property not found");
                    _blurReductionApplied = true;
                    return;
                }

                Log.LogInfo("[BLUR] Found Volume.weight as FIELD");

                var allVolumes = UnityEngine.Object.FindObjectsOfType(volumeType);
                Log.LogInfo($"[BLUR] Found {allVolumes.Length} Volume components");

                // v10.21.0: Only reduce underwater volume blur, keep other volumes intact
                foreach (var volume in allVolumes)
                {
                    var volMono = volume as MonoBehaviour;
                    if (volMono == null || !volMono.enabled) continue;

                    string volName = volMono.name.ToLower();

                    // Only modify underwater-related volumes
                    if (volName.Contains("under") && volName.Contains("water"))
                    {
                        float currentWeight = (float)weightField.GetValue(volume);

                        if (!_originalVolumeWeights.ContainsKey(volume))
                        {
                            _originalVolumeWeights[volume] = currentWeight;
                        }

                        // Set blur intensity
                        weightField.SetValue(volume, UNDERWATER_BLUR_INTENSITY);
                        Log.LogInfo($"[BLUR] Underwater volume '{volMono.name}' weight: {currentWeight:F2} -> {UNDERWATER_BLUR_INTENSITY:F2}");

                        // v10.22.0: Apply ocean blue tint via ColorAdjustments
                        ApplyUnderwaterColorTint(volume);
                    }
                    else
                    {
                        // v10.21.0: Keep other volumes (Bloom, Volumetric, Color Correction) at full weight
                        Log.LogInfo($"[BLUR] Keeping '{volMono.name}' at original weight (not underwater)");
                    }
                }

                _blurReductionApplied = true;
                Log.LogInfo($"[BLUR] Modified {_originalVolumeWeights.Count} underwater volume(s), kept other volumes intact");
            }
            catch (Exception ex)
            {
                Log.LogError($"[BLUR] Error: {ex.Message}");
                _blurReductionApplied = true;
            }
        }

        private static void TryReduceBlurViaProperty(Type volumeType, PropertyInfo weightProp)
        {
            try
            {
                var allVolumes = UnityEngine.Object.FindObjectsOfType(volumeType);

                // v10.21.0: Only reduce underwater volume blur, keep other volumes intact
                foreach (var volume in allVolumes)
                {
                    var volMono = volume as MonoBehaviour;
                    if (volMono == null || !volMono.enabled) continue;

                    string volName = volMono.name.ToLower();

                    // Only modify underwater-related volumes
                    if (volName.Contains("under") && volName.Contains("water"))
                    {
                        float currentWeight = (float)weightProp.GetValue(volume);

                        if (!_originalVolumeWeights.ContainsKey(volume))
                        {
                            _originalVolumeWeights[volume] = currentWeight;
                        }

                        weightProp.SetValue(volume, UNDERWATER_BLUR_INTENSITY);
                        Log.LogInfo($"[BLUR] Underwater volume '{volMono.name}' weight (via prop): {currentWeight:F2} -> {UNDERWATER_BLUR_INTENSITY:F2}");
                    }
                    else
                    {
                        Log.LogInfo($"[BLUR] Keeping '{volMono.name}' at original weight (not underwater)");
                    }
                }

                _blurReductionApplied = true;
            }
            catch (Exception ex)
            {
                Log.LogError($"[BLUR] Error via property: {ex.Message}");
                _blurReductionApplied = true;
            }
        }

        private static void TryRestoreBlur()
        {
            try
            {
                Type volumeType = AccessTools.TypeByName("UnityEngine.Rendering.Volume");
                if (volumeType == null)
                {
                    _originalVolumeWeights.Clear();
                    _blurReductionApplied = false;
                    return;
                }

                var weightField = volumeType.GetField("weight", BindingFlags.Public | BindingFlags.Instance);
                var weightProp = weightField == null ? volumeType.GetProperty("weight") : null;

                foreach (var kvp in _originalVolumeWeights)
                {
                    if (kvp.Key == null) continue;

                    if (weightField != null)
                    {
                        weightField.SetValue(kvp.Key, kvp.Value);
                    }
                    else if (weightProp != null)
                    {
                        weightProp.SetValue(kvp.Key, kvp.Value);
                    }
                }

                Log.LogInfo($"[BLUR] Restored {_originalVolumeWeights.Count} volumes");
                _originalVolumeWeights.Clear();
                _blurReductionApplied = false;

                // v10.22.0: Restore color filter
                RestoreUnderwaterColorTint();
            }
            catch (Exception ex)
            {
                Log.LogError($"[BLUR] Restore error: {ex.Message}");
                _originalVolumeWeights.Clear();
                _blurReductionApplied = false;
            }
        }

        /// <summary>
        /// v10.22.0: Apply ocean blue tint to underwater ColorAdjustments
        /// </summary>
        private static object _cachedColorAdjustments = null;
        private static FieldInfo _colorFilterField = null;

        private static void ApplyUnderwaterColorTint(object volume)
        {
            try
            {
                // Get the Volume's profile
                var volumeType = volume.GetType();
                var profileProp = volumeType.GetProperty("profile");
                if (profileProp == null) return;

                var profile = profileProp.GetValue(volume);
                if (profile == null) return;

                // Get profile's components list
                var profileType = profile.GetType();
                var componentsField = profileType.GetField("components", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (componentsField == null) return;

                var components = componentsField.GetValue(profile) as System.Collections.IList;
                if (components == null) return;

                // Find ColorAdjustments component
                foreach (var comp in components)
                {
                    if (comp == null) continue;
                    string typeName = comp.GetType().Name;

                    if (typeName.Contains("ColorAdjustments"))
                    {
                        _cachedColorAdjustments = comp;

                        // Find colorFilter field
                        _colorFilterField = comp.GetType().GetField("colorFilter", BindingFlags.Public | BindingFlags.Instance);
                        if (_colorFilterField == null)
                        {
                            Log.LogWarning("[UNDERWATER] ColorAdjustments found but no colorFilter field");
                            return;
                        }

                        // Get the ColorParameter
                        var colorParam = _colorFilterField.GetValue(comp);
                        if (colorParam == null)
                        {
                            Log.LogWarning("[UNDERWATER] colorFilter parameter is null");
                            return;
                        }

                        // Get current value and save it
                        var valueProp = colorParam.GetType().GetProperty("value");
                        if (valueProp != null)
                        {
                            if (!_colorFilterModified)
                            {
                                _originalColorFilter = valueProp.GetValue(colorParam);
                            }

                            // Set ocean blue tint
                            valueProp.SetValue(colorParam, UNDERWATER_TINT);
                            _colorFilterModified = true;

                            Log.LogInfo($"[UNDERWATER] Applied ocean blue tint: {UNDERWATER_TINT}");
                        }

                        // Also enable the override if needed
                        var overrideStateProp = colorParam.GetType().GetProperty("overrideState");
                        if (overrideStateProp != null)
                        {
                            overrideStateProp.SetValue(colorParam, true);
                        }

                        return;
                    }
                }

                Log.LogInfo("[UNDERWATER] No ColorAdjustments component found in underwater volume");
            }
            catch (Exception ex)
            {
                Log.LogError($"[UNDERWATER] Error applying color tint: {ex.Message}");
            }
        }

        private static void RestoreUnderwaterColorTint()
        {
            try
            {
                if (!_colorFilterModified || _cachedColorAdjustments == null || _colorFilterField == null)
                    return;

                var colorParam = _colorFilterField.GetValue(_cachedColorAdjustments);
                if (colorParam == null) return;

                var valueProp = colorParam.GetType().GetProperty("value");
                if (valueProp != null && _originalColorFilter != null)
                {
                    valueProp.SetValue(colorParam, _originalColorFilter);
                    Log.LogInfo("[UNDERWATER] Restored original color filter");
                }

                _colorFilterModified = false;
            }
            catch (Exception ex)
            {
                Log.LogError($"[UNDERWATER] Error restoring color: {ex.Message}");
            }
        }

        private static Transform FindChildRecursive(Transform parent, string name)
        {
            foreach (Transform child in parent)
            {
                if (child.name == name) return child;
                var found = FindChildRecursive(child, name);
                if (found != null) return found;
            }
            return null;
        }

        // ========== EXISTING PATCHES (PROP INJECTION) ==========

        private void PatchStageManager()
        {
            if (_stageManagerType != null)
            {
                var awakeMethod = AccessTools.Method(_stageManagerType, "Awake");
                if (awakeMethod != null)
                {
                    var postfix = new HarmonyMethod(typeof(Plugin).GetMethod(nameof(StageManager_Awake_Postfix),
                        BindingFlags.Public | BindingFlags.Static));
                    _harmony.Patch(awakeMethod, postfix: postfix);
                    Log.LogInfo("[INJECTOR] Patched StageManager.Awake");
                }
            }
        }

        private void PatchEndlessPropBuildPrefab()
        {
            if (_endlessPropType != null)
            {
                var buildPrefabMethod = AccessTools.Method(_endlessPropType, "BuildPrefab");
                if (buildPrefabMethod != null)
                {
                    var prefix = new HarmonyMethod(typeof(Plugin).GetMethod(nameof(BuildPrefab_Prefix),
                        BindingFlags.Public | BindingFlags.Static));
                    var postfix = new HarmonyMethod(typeof(Plugin).GetMethod(nameof(BuildPrefab_Postfix),
                        BindingFlags.Public | BindingFlags.Static));
                    _harmony.Patch(buildPrefabMethod, prefix: prefix, postfix: postfix);
                    Log.LogInfo("[INJECTOR] Patched EndlessProp.BuildPrefab");
                }
            }
        }

        private void PatchSpawnPreview()
        {
            if (_propBasedToolType != null)
            {
                var spawnPreviewMethod = AccessTools.Method(_propBasedToolType, "SpawnPreview");
                if (spawnPreviewMethod != null)
                {
                    var prefix = new HarmonyMethod(typeof(Plugin).GetMethod(nameof(SpawnPreview_Prefix),
                        BindingFlags.Public | BindingFlags.Static));
                    var postfix = new HarmonyMethod(typeof(Plugin).GetMethod(nameof(SpawnPreview_Postfix),
                        BindingFlags.Public | BindingFlags.Static));
                    _harmony.Patch(spawnPreviewMethod, prefix: prefix, postfix: postfix);
                    Log.LogInfo("[INJECTOR] Patched PropBasedTool.SpawnPreview");
                }
            }
        }

        private void PatchFindAndManageChildRenderers()
        {
            if (_endlessPropType != null)
            {
                var findRenderersMethod = AccessTools.Method(_endlessPropType, "FindAndManageChildRenderers");
                if (findRenderersMethod != null)
                {
                    var prefix = new HarmonyMethod(typeof(Plugin).GetMethod(nameof(FindAndManageChildRenderers_Prefix),
                        BindingFlags.Public | BindingFlags.Static));
                    var postfix = new HarmonyMethod(typeof(Plugin).GetMethod(nameof(FindAndManageChildRenderers_Postfix),
                        BindingFlags.Public | BindingFlags.Static));
                    _harmony.Patch(findRenderersMethod, prefix: prefix, postfix: postfix);
                    Log.LogInfo("[INJECTOR] Patched EndlessProp.FindAndManageChildRenderers");
                }
            }
        }

        public static void BuildPrefab_Prefix(object __instance, object prop, GameObject testPrefab, object testScript)
        {
            try
            {
                var assetCoreType = AccessTools.TypeByName("Endless.Assets.AssetCore");
                var nameField = assetCoreType?.GetField("Name");
                var propName = nameField?.GetValue(prop) as string ?? "UNKNOWN";

                var baseTypeIdField = AccessTools.Field(_propType, "baseTypeId");
                var baseTypeId = baseTypeIdField?.GetValue(prop) as string ?? "UNKNOWN";

                if (baseTypeId == CustomBaseTypeGuid)
                {
                    Log.LogInfo($"[TRACE] Building custom prop: {propName}");
                }
            }
            catch (Exception ex)
            {
                Log.LogError($"[TRACE] BuildPrefab_Prefix error: {ex.Message}");
            }
        }

        public static void BuildPrefab_Postfix(object __instance) { }
        public static void SpawnPreview_Prefix(object __instance, object runtimePropInfo) { }
        public static void SpawnPreview_Postfix(object __instance) { }
        public static void FindAndManageChildRenderers_Prefix(object __instance, GameObject targetObject) { }
        public static void FindAndManageChildRenderers_Postfix(object __instance) { }

        public static void StageManager_Awake_Postfix(object __instance)
        {
            try
            {
                Log.LogInfo("[INJECTOR] StageManager.Awake - Creating custom prop");

                LogAvailableShaders();
                CreateCustomPrefab();
                CreateCustomBaseTypeDefinition();
                InjectDefinitionIntoBaseTypeList(__instance);
                InjectCustomProp(__instance);

                // v10.25.0: Delay collectible research until scene objects are loaded
                if (__instance is MonoBehaviour mono)
                {
                    mono.StartCoroutine(DelayedCollectibleResearch());
                }
            }
            catch (Exception ex)
            {
                Log.LogError($"[INJECTOR] StageManager.Awake failed: {ex}");
            }
        }

        /// <summary>
        /// v10.25.0: Coroutine to delay collectible research until scene is fully loaded
        /// </summary>
        private static IEnumerator DelayedCollectibleResearch()
        {
            // Wait a few frames for scene to fully load
            yield return null;
            yield return null;
            yield return new WaitForSeconds(2f); // Wait 2 seconds for all props to spawn

            ResearchCollectibleSystem();
        }

        private static void InjectDefinitionIntoBaseTypeList(object stageManager)
        {
            try
            {
                var baseTypeListField = AccessTools.Field(_stageManagerType, "baseTypeList");
                var baseTypeList = baseTypeListField?.GetValue(stageManager);
                if (baseTypeList == null) return;

                var componentsField = AccessTools.Field(_abstractComponentListBaseType, "components");
                var componentsList = componentsField?.GetValue(baseTypeList);
                if (componentsList == null) return;

                var addMethod = componentsList.GetType().GetMethod("Add");
                addMethod?.Invoke(componentsList, new object[] { _customBaseTypeDefinition });

                var definitionMapField = AccessTools.Field(_abstractComponentListBaseType, "definitionMap");
                if (definitionMapField?.GetValue(baseTypeList) != null)
                {
                    definitionMapField.SetValue(baseTypeList, null);
                }
            }
            catch (Exception ex)
            {
                Log.LogError($"[INJECTOR] InjectDefinitionIntoBaseTypeList failed: {ex}");
            }
        }

        /// <summary>
        /// v10.26.0: Find the Treasure Usable Definition from Resources
        /// </summary>
        private static void FindTreasureDefinition()
        {
            if (_cachedTreasureDefinition != null) return;

            try
            {
                // Find InventoryUsableDefinition base type (correct namespace)
                Type invDefType = AccessTools.TypeByName("Endless.Gameplay.InventoryUsableDefinition");
                if (invDefType == null)
                {
                    // Try alternate namespace
                    invDefType = _genericUsableDefinitionType?.BaseType;
                }

                if (invDefType == null)
                {
                    Log.LogWarning("[COLLECT] InventoryUsableDefinition type not found");
                    return;
                }

                // Find all InventoryUsableDefinition instances
                var allDefs = Resources.FindObjectsOfTypeAll(invDefType);
                Log.LogInfo($"[COLLECT] Found {allDefs.Length} InventoryUsableDefinition instances");

                foreach (var def in allDefs)
                {
                    var defObj = def as ScriptableObject;
                    if (defObj != null && defObj.name.Contains("Treasure") && defObj.name.Contains("Anachronist"))
                    {
                        _cachedTreasureDefinition = def;
                        Log.LogInfo($"[COLLECT] Cached Treasure Definition: {defObj.name}");
                        break;
                    }
                }

                if (_cachedTreasureDefinition == null)
                {
                    // Fallback: try to find any Treasure definition
                    foreach (var def in allDefs)
                    {
                        var defObj = def as ScriptableObject;
                        if (defObj != null && defObj.name.Contains("Treasure"))
                        {
                            _cachedTreasureDefinition = def;
                            Log.LogInfo($"[COLLECT] Cached Treasure Definition (fallback): {defObj.name}");
                            break;
                        }
                    }
                }

                if (_cachedTreasureDefinition == null)
                {
                    Log.LogWarning("[COLLECT] No Treasure Usable Definition found!");
                }
            }
            catch (Exception ex)
            {
                Log.LogError($"[COLLECT] Error finding Treasure Definition: {ex.Message}");
            }
        }

        private static void CreateCustomPrefab()
        {
            if (_customPrefab != null) return;

            // v10.28.0: Clone existing Treasure prefab to inherit NetworkBehaviour registration
            // This avoids the network initialization issues from adding NetworkBehaviours at runtime

            if (!LoadAssetBundle())
            {
                Log.LogWarning("[COLLECT] Failed to load asset bundle, using fallback");
                CreateFallbackPrefab();
                return;
            }

            // Find an existing TreasureItem to clone
            GameObject treasureSource = FindTreasurePrefabSource();
            if (treasureSource == null)
            {
                Log.LogWarning("[COLLECT] No Treasure source found, using StaticProp fallback");
                CreateStaticPropFallback();
                return;
            }

            // Clone the treasure prefab
            _customPrefab = UnityEngine.Object.Instantiate(treasureSource);
            _customPrefab.name = "CustomCollectibleProp";
            _customPrefab.SetActive(false);
            Log.LogInfo($"[COLLECT] Cloned Treasure prefab: {treasureSource.name}");

            // Log the hierarchy structure for debugging
            LogHierarchy(_customPrefab, 0);

            // v10.31.1: DESTROY EndlessVisuals component - disabling isn't enough because
            // when SetupBaseType instantiates this prefab, the component's Awake/Start runs
            // and creates the treasure mesh before we can stop it.
            if (_endlessVisualsType != null)
            {
                var endlessVisuals = _customPrefab.GetComponent(_endlessVisualsType) as MonoBehaviour;
                if (endlessVisuals != null)
                {
                    UnityEngine.Object.DestroyImmediate(endlessVisuals);
                    Log.LogInfo("[COLLECT] DESTROYED EndlessVisuals component (prevents default mesh spawn)");
                }
                else
                {
                    Log.LogWarning("[COLLECT] EndlessVisuals component not found on prefab");
                }
            }
            else
            {
                Log.LogWarning("[COLLECT] _endlessVisualsType is null - cannot destroy EndlessVisuals");
            }

            // v10.31.2: Also destroy any existing renderers/meshes on the cloned prefab
            // The treasure prefab may have mesh children that aren't created by EndlessVisuals
            var existingRenderers = _customPrefab.GetComponentsInChildren<Renderer>(true);
            foreach (var renderer in existingRenderers)
            {
                Log.LogInfo($"[COLLECT] Destroying existing renderer: {renderer.gameObject.name}");
                UnityEngine.Object.DestroyImmediate(renderer.gameObject);
            }
            Log.LogInfo($"[COLLECT] Destroyed {existingRenderers.Length} existing renderer objects");

            // v10.31.0: DO NOT add custom visuals to _customPrefab here!
            // The visuals come from _visualPrefab via InjectProp->BuildPrefab flow:
            //   1. SetupBaseType instantiates _customPrefab (provides TreasureItem component)
            //   2. BuildPrefab instantiates _visualPrefab as child visuals
            // Adding visuals here would cause double visuals.
            Log.LogInfo("[COLLECT] BaseType prefab ready (visuals will come from InjectProp testPrefab)");

            // Update the assetID to our custom GUID
            if (_treasureItemType != null && _itemType != null)
            {
                var treasureItem = _customPrefab.GetComponent(_treasureItemType);
                if (treasureItem != null)
                {
                    // v10.32.1: Use our visual prefab (with SimpleYawSpin) as GroundVisualsInfo.
                    // This makes it work like native treasures:
                    // - runtimeGroundVisuals = our Pearl Basket with rotation
                    // - SetActive(false) is called on pickup, stopping rotation
                    // - Game handles everything natively

                    // Create empty placeholder for equipped visuals (not visible when held)
                    if (_emptyVisualPlaceholder == null)
                    {
                        _emptyVisualPlaceholder = new GameObject("EmptyVisualPlaceholder");
                        _emptyVisualPlaceholder.SetActive(false);
                        UnityEngine.Object.DontDestroyOnLoad(_emptyVisualPlaceholder);
                        Log.LogInfo("[COLLECT] Created empty visual placeholder GameObject");
                    }

                    // Create ground visual prefab with SimpleYawSpin
                    var groundVisualPrefab = CreateGroundVisualWithRotation();

                    try
                    {
                        // Set tempVisualsInfoGround.GameObject to our visual WITH SimpleYawSpin
                        var groundVisualsField = AccessTools.Field(_treasureItemType, "tempVisualsInfoGround");
                        if (groundVisualsField != null)
                        {
                            var groundVisuals = groundVisualsField.GetValue(treasureItem);
                            if (groundVisuals != null)
                            {
                                var gameObjectField = groundVisuals.GetType().GetField("GameObject");
                                if (gameObjectField != null)
                                {
                                    gameObjectField.SetValue(groundVisuals, groundVisualPrefab);
                                    groundVisualsField.SetValue(treasureItem, groundVisuals);
                                    Log.LogInfo("[COLLECT] Set tempVisualsInfoGround.GameObject to Pearl Basket with SimpleYawSpin");
                                }
                            }
                        }

                        // Set tempVisualsInfoEqupped.GameObject to empty placeholder (note: typo in original game code)
                        var equippedVisualsField = AccessTools.Field(_treasureItemType, "tempVisualsInfoEqupped");
                        if (equippedVisualsField != null)
                        {
                            var equippedVisuals = equippedVisualsField.GetValue(treasureItem);
                            if (equippedVisuals != null)
                            {
                                var gameObjectField = equippedVisuals.GetType().GetField("GameObject");
                                if (gameObjectField != null)
                                {
                                    gameObjectField.SetValue(equippedVisuals, _emptyVisualPlaceholder);
                                    equippedVisualsField.SetValue(treasureItem, equippedVisuals);
                                    Log.LogInfo("[COLLECT] Set tempVisualsInfoEqupped.GameObject to empty placeholder");
                                }
                            }
                        }

                        Log.LogInfo("[COLLECT] Configured VisualsInfo for proper rotation behavior");
                    }
                    catch (Exception ex)
                    {
                        Log.LogError($"[COLLECT] Failed to clear VisualsInfo: {ex.Message}");
                    }

                    var assetIdField = AccessTools.Field(_itemType, "assetID");
                    if (assetIdField != null)
                    {
                        var serializableGuidType = assetIdField.FieldType;
                        var guidConstructor = serializableGuidType.GetConstructor(new Type[] { typeof(string) });
                        if (guidConstructor != null)
                        {
                            var guid = guidConstructor.Invoke(new object[] { CustomPropGuid.ToString() });
                            assetIdField.SetValue(treasureItem, guid);
                            Log.LogInfo($"[COLLECT] Updated assetID to {CustomPropGuid}");
                        }
                    }

                    // v10.33.0: Definition is now handled in Item_ComponentInitialize_Prefix hook
                    // which modifies the ORIGINAL definition (preserves animationTrigger)
                }
            }

            _customPrefab.SetActive(true);
            UnityEngine.Object.DontDestroyOnLoad(_customPrefab);

            // v10.31.0: Register prefab with NetworkManager for proper network spawning
            RegisterPrefabWithNetworkManager(_customPrefab);

            if (_loadedPrefab != null) _visualPrefab = _loadedPrefab;

            Log.LogInfo($"[COLLECT] Created collectible prefab: {_customPrefab.name}");
        }

        private static GameObject FindTreasurePrefabSource()
        {
            // First try to find a TreasureItem instance in the scene
            if (_treasureItemType != null)
            {
                var treasures = UnityEngine.Object.FindObjectsOfType(_treasureItemType);
                foreach (var treasure in treasures)
                {
                    var mb = treasure as MonoBehaviour;
                    if (mb != null && mb.gameObject != null)
                    {
                        Log.LogInfo($"[COLLECT] Found TreasureItem instance: {mb.gameObject.name}");
                        return mb.gameObject;
                    }
                }
            }

            // Try to find via Resources
            var allTreasures = Resources.FindObjectsOfTypeAll(_treasureItemType);
            foreach (var treasure in allTreasures)
            {
                var mb = treasure as MonoBehaviour;
                if (mb != null && mb.gameObject != null)
                {
                    Log.LogInfo($"[COLLECT] Found TreasureItem via Resources: {mb.gameObject.name}");
                    return mb.gameObject;
                }
            }

            Log.LogWarning("[COLLECT] No TreasureItem found in scene or resources");
            return null;
        }

        private static void LogHierarchy(GameObject obj, int depth)
        {
            string indent = new string(' ', depth * 2);
            var components = obj.GetComponents<Component>();
            string componentList = string.Join(", ", components.Select(c => c?.GetType().Name ?? "null"));
            Log.LogInfo($"[HIERARCHY] {indent}{obj.name} (layer={obj.layer}) [{componentList}]");

            foreach (Transform child in obj.transform)
            {
                LogHierarchy(child.gameObject, depth + 1);
            }
        }

        // v10.31.0: Network registration helpers
        private static Type _networkManagerType;
        private static Type _networkObjectType;
        private static bool _networkTypesInitialized = false;

        private static void InitializeNetworkTypes()
        {
            if (_networkTypesInitialized) return;

            _networkManagerType = AccessTools.TypeByName("Unity.Netcode.NetworkManager");
            _networkObjectType = AccessTools.TypeByName("Unity.Netcode.NetworkObject");

            _networkTypesInitialized = true;

            Log.LogInfo($"[NETWORK] NetworkManager type: {(_networkManagerType != null ? "Found" : "NOT FOUND")}");
            Log.LogInfo($"[NETWORK] NetworkObject type: {(_networkObjectType != null ? "Found" : "NOT FOUND")}");
        }

        private static void RegisterPrefabWithNetworkManager(GameObject prefab)
        {
            try
            {
                InitializeNetworkTypes();

                if (_networkManagerType == null)
                {
                    Log.LogWarning("[NETWORK] NetworkManager type not found - cannot register prefab");
                    return;
                }

                // Get NetworkManager.Singleton
                var singletonProperty = _networkManagerType.GetProperty("Singleton", BindingFlags.Public | BindingFlags.Static);
                if (singletonProperty == null)
                {
                    Log.LogWarning("[NETWORK] NetworkManager.Singleton property not found");
                    return;
                }

                var networkManager = singletonProperty.GetValue(null);
                if (networkManager == null)
                {
                    Log.LogWarning("[NETWORK] NetworkManager.Singleton is null - network not initialized yet");
                    return;
                }

                // Get AddNetworkPrefab method
                var addPrefabMethod = _networkManagerType.GetMethod("AddNetworkPrefab", new Type[] { typeof(GameObject) });
                if (addPrefabMethod == null)
                {
                    Log.LogWarning("[NETWORK] AddNetworkPrefab method not found");
                    return;
                }

                // Check if prefab has NetworkObject component
                if (_networkObjectType != null)
                {
                    var networkObject = prefab.GetComponent(_networkObjectType);
                    if (networkObject == null)
                    {
                        Log.LogWarning("[NETWORK] Prefab has no NetworkObject component - cannot register");
                        return;
                    }
                }

                // Register the prefab
                addPrefabMethod.Invoke(networkManager, new object[] { prefab });
                Log.LogInfo($"[NETWORK] Registered prefab '{prefab.name}' with NetworkManager");
            }
            catch (Exception ex)
            {
                Log.LogError($"[NETWORK] Failed to register prefab: {ex.Message}");
            }
        }

        /// <summary>
        /// v10.31.0: Spawn NetworkObject on an instance for proper network initialization
        /// Call this after instantiating the prefab in the scene
        /// </summary>
        public static void SpawnNetworkObject(GameObject instance)
        {
            try
            {
                InitializeNetworkTypes();

                if (_networkObjectType == null)
                {
                    Log.LogWarning("[NETWORK] NetworkObject type not found - cannot spawn");
                    return;
                }

                var networkObject = instance.GetComponent(_networkObjectType);
                if (networkObject == null)
                {
                    Log.LogWarning($"[NETWORK] No NetworkObject on '{instance.name}' - cannot spawn");
                    return;
                }

                // Get Spawn method: void Spawn(bool destroyWithScene = false)
                var spawnMethod = _networkObjectType.GetMethod("Spawn", new Type[] { typeof(bool) });
                if (spawnMethod == null)
                {
                    // Try parameterless overload
                    spawnMethod = _networkObjectType.GetMethod("Spawn", Type.EmptyTypes);
                }

                if (spawnMethod == null)
                {
                    Log.LogWarning("[NETWORK] Spawn method not found on NetworkObject");
                    return;
                }

                // Spawn the object (false = don't destroy with scene)
                if (spawnMethod.GetParameters().Length > 0)
                {
                    spawnMethod.Invoke(networkObject, new object[] { false });
                }
                else
                {
                    spawnMethod.Invoke(networkObject, null);
                }

                Log.LogInfo($"[NETWORK] Spawned NetworkObject on '{instance.name}'");
            }
            catch (Exception ex)
            {
                Log.LogError($"[NETWORK] Failed to spawn NetworkObject: {ex.Message}");
            }
        }

        /// <summary>
        /// v10.32.1: Create ground visual prefab with SimpleYawSpin for proper rotation behavior.
        /// This becomes runtimeGroundVisuals, which the game disables on pickup.
        /// </summary>
        private static GameObject CreateGroundVisualWithRotation()
        {
            try
            {
                // Use loaded prefab or create fallback
                GameObject sourceVisual = _loadedPrefab ?? _visualPrefab;

                if (sourceVisual == null)
                {
                    Log.LogWarning("[ROTATION] No visual source - creating fallback cube");
                    sourceVisual = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    sourceVisual.name = "FallbackVisual";
                    var collider = sourceVisual.GetComponent<Collider>();
                    if (collider != null) UnityEngine.Object.DestroyImmediate(collider);
                }

                // Clone the visual so we don't modify the original
                var groundVisual = UnityEngine.Object.Instantiate(sourceVisual);
                groundVisual.name = "PearlBasket_GroundVisual";
                groundVisual.SetActive(false);  // Keep inactive as prefab
                UnityEngine.Object.DontDestroyOnLoad(groundVisual);

                // Add SimpleYawSpin if type is available
                if (_simpleYawSpinType != null)
                {
                    var existingSpinner = groundVisual.GetComponent(_simpleYawSpinType);
                    if (existingSpinner == null)
                    {
                        var spinner = groundVisual.AddComponent(_simpleYawSpinType);
                        if (spinner != null)
                        {
                            // Set spin speed (180 degrees/sec = full rotation in 2 seconds)
                            var spinSpeedField = AccessTools.Field(_simpleYawSpinType, "spinDegreesPerSecond");
                            if (spinSpeedField != null)
                            {
                                spinSpeedField.SetValue(spinner, 180f);
                            }

                            // Enable random initial rotation
                            var randomRotField = AccessTools.Field(_simpleYawSpinType, "randomizeInitialRotation");
                            if (randomRotField != null)
                            {
                                randomRotField.SetValue(spinner, true);
                            }

                            Log.LogInfo("[ROTATION] Added SimpleYawSpin to ground visual (180 deg/sec)");
                        }
                    }
                    else
                    {
                        Log.LogInfo("[ROTATION] Ground visual already has SimpleYawSpin");
                    }
                }
                else
                {
                    Log.LogWarning("[ROTATION] SimpleYawSpin type not found - rotation won't work");
                }

                return groundVisual;
            }
            catch (Exception ex)
            {
                Log.LogError($"[ROTATION] Failed to create ground visual: {ex.Message}");

                // Return empty placeholder as fallback
                var fallback = new GameObject("FallbackGroundVisual");
                fallback.SetActive(false);
                UnityEngine.Object.DontDestroyOnLoad(fallback);
                return fallback;
            }
        }

        /// <summary>
        /// v10.32.0: Create custom InventoryUsableDefinition with our icon for the Pearl Basket
        /// </summary>
        private static void CreateCustomInventoryDefinition(object treasureItem)
        {
            if (treasureItem == null) return;

            try
            {
                // v10.32.2: Clone from original treasure definition to preserve all fields
                // (especially animationTrigger which is required for character movement animations)

                if (_genericUsableDefinitionType == null)
                {
                    Log.LogWarning("[ICON] GenericUsableDefinition type not found");
                    return;
                }

                // Check if we already created one
                if (_customUsableDefinition == null)
                {
                    // Get the original definition from the cloned TreasureItem
                    var definitionField = AccessTools.Field(_itemType, "inventoryUsableDefinition");
                    object originalDefinition = null;
                    if (definitionField != null)
                    {
                        originalDefinition = definitionField.GetValue(treasureItem);
                    }

                    if (originalDefinition != null)
                    {
                        // Clone by creating new instance and copying all serialized fields
                        _customUsableDefinition = ScriptableObject.CreateInstance(_genericUsableDefinitionType);
                        if (_customUsableDefinition == null)
                        {
                            Log.LogError("[ICON] Failed to create GenericUsableDefinition instance");
                            return;
                        }

                        // Copy all fields from original using EditorUtility alternative (reflection)
                        CopyDefinitionFields(originalDefinition, _customUsableDefinition);
                        Log.LogInfo("[ICON] Copied all fields from original treasure definition");
                    }
                    else
                    {
                        // Fallback: create from scratch
                        _customUsableDefinition = ScriptableObject.CreateInstance(_genericUsableDefinitionType);
                        if (_customUsableDefinition == null)
                        {
                            Log.LogError("[ICON] Failed to create GenericUsableDefinition instance");
                            return;
                        }
                        Log.LogWarning("[ICON] No original definition found, creating from scratch");
                    }

                    // Set the name
                    ((ScriptableObject)_customUsableDefinition).name = "Pearl Basket Usable Definition";

                    // Override specific fields for our custom item
                    if (_usableDefinitionType != null)
                    {
                        // Set the GUID (use our custom prop GUID)
                        var guidField = AccessTools.Field(_usableDefinitionType, "guid");
                        if (guidField != null)
                        {
                            var serializableGuidType = guidField.FieldType;
                            var guidConstructor = serializableGuidType.GetConstructor(new Type[] { typeof(string) });
                            if (guidConstructor != null)
                            {
                                var guid = guidConstructor.Invoke(new object[] { CustomPropGuid.ToString() });
                                guidField.SetValue(_customUsableDefinition, guid);
                                Log.LogInfo($"[ICON] Set definition GUID to {CustomPropGuid}");
                            }
                        }

                        // Set the display name
                        var displayNameField = AccessTools.Field(_usableDefinitionType, "displayName");
                        if (displayNameField != null)
                        {
                            displayNameField.SetValue(_customUsableDefinition, "Pearl Basket");
                            Log.LogInfo("[ICON] Set definition display name to 'Pearl Basket'");
                        }

                        // Set the icon sprite
                        var spriteField = AccessTools.Field(_usableDefinitionType, "sprite");
                        if (spriteField != null && _loadedIcon != null)
                        {
                            spriteField.SetValue(_customUsableDefinition, _loadedIcon);
                            Log.LogInfo("[ICON] Set definition sprite to loaded icon");
                        }
                        else if (_loadedIcon == null)
                        {
                            // Create a placeholder icon if no icon was loaded
                            var iconTexture = new Texture2D(64, 64);
                            var colors = new Color[64 * 64];
                            for (int i = 0; i < colors.Length; i++) colors[i] = new Color(0.8f, 0.6f, 0.9f);  // Light purple
                            iconTexture.SetPixels(colors);
                            iconTexture.Apply();
                            var placeholderSprite = Sprite.Create(iconTexture, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f));
                            spriteField.SetValue(_customUsableDefinition, placeholderSprite);
                            Log.LogInfo("[ICON] Set definition sprite to placeholder (no icon loaded)");
                        }
                    }

                    UnityEngine.Object.DontDestroyOnLoad((UnityEngine.Object)_customUsableDefinition);
                    Log.LogInfo("[ICON] Created custom GenericUsableDefinition for Pearl Basket");
                }

                // Assign to the TreasureItem
                var defField = AccessTools.Field(_itemType, "inventoryUsableDefinition");
                if (defField != null)
                {
                    defField.SetValue(treasureItem, _customUsableDefinition);
                    Log.LogInfo("[ICON] Assigned custom definition to TreasureItem");
                }
            }
            catch (Exception ex)
            {
                Log.LogError($"[ICON] Failed to create custom inventory definition: {ex.Message}");
            }
        }

        /// <summary>
        /// v10.32.2: Copy all serialized fields from original definition to new one.
        /// This preserves animationTrigger, inventoryType, and other critical fields.
        /// </summary>
        private static void CopyDefinitionFields(object source, object target)
        {
            if (source == null || target == null) return;

            try
            {
                // Copy fields from all base types up the hierarchy
                var currentType = source.GetType();
                while (currentType != null && currentType != typeof(UnityEngine.Object))
                {
                    var fields = currentType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                    foreach (var field in fields)
                    {
                        try
                        {
                            // Skip Unity internal fields
                            if (field.Name.StartsWith("m_") && field.DeclaringType == typeof(UnityEngine.Object))
                                continue;

                            var value = field.GetValue(source);
                            field.SetValue(target, value);
                        }
                        catch
                        {
                            // Skip fields that can't be copied
                        }
                    }
                    currentType = currentType.BaseType;
                }

                Log.LogInfo("[ICON] Copied definition fields via reflection");
            }
            catch (Exception ex)
            {
                Log.LogWarning($"[ICON] Field copy partially failed: {ex.Message}");
            }
        }

        private static void StripOldVisuals(GameObject obj)
        {
            // Remove MeshRenderer and MeshFilter from root and immediate children
            var meshRenderers = obj.GetComponentsInChildren<MeshRenderer>(true);
            var meshFilters = obj.GetComponentsInChildren<MeshFilter>(true);
            var skinnedRenderers = obj.GetComponentsInChildren<SkinnedMeshRenderer>(true);

            int strippedCount = 0;

            foreach (var renderer in meshRenderers)
            {
                if (renderer.gameObject != obj)
                {
                    UnityEngine.Object.DestroyImmediate(renderer.gameObject);
                    strippedCount++;
                }
                else
                {
                    UnityEngine.Object.DestroyImmediate(renderer);
                }
            }

            foreach (var filter in meshFilters)
            {
                if (filter != null)
                    UnityEngine.Object.DestroyImmediate(filter);
            }

            foreach (var skinned in skinnedRenderers)
            {
                if (skinned.gameObject != obj)
                {
                    UnityEngine.Object.DestroyImmediate(skinned.gameObject);
                    strippedCount++;
                }
                else
                {
                    UnityEngine.Object.DestroyImmediate(skinned);
                }
            }

            Log.LogInfo($"[COLLECT] Stripped {strippedCount} visual objects from clone");
        }

        private static void CreateStaticPropFallback()
        {
            if (!LoadAssetBundle()) return;

            _customPrefab = new GameObject("CustomProp");
            _customPrefab.SetActive(false);

            var staticPropType = AccessTools.TypeByName("Endless.Gameplay.StaticProp");
            if (staticPropType != null)
            {
                _customPrefab.AddComponent(staticPropType);
                Log.LogInfo("[INJECTOR] Added StaticProp component (fallback)");
            }

            _customPrefab.SetActive(true);
            UnityEngine.Object.DontDestroyOnLoad(_customPrefab);

            if (_loadedPrefab != null) _visualPrefab = _loadedPrefab;

            Log.LogInfo($"[INJECTOR] Created fallback prop prefab: {_customPrefab.name}");
        }

        private static bool LoadAssetBundle()
        {
            // v10.33.2: Check if already loaded - Unity's LoadFromFile returns null for already-loaded bundles
            // which would overwrite our valid reference and cause fallback to sphere
            if (_customPropsBundle != null && _loadedPrefab != null)
            {
                return true;
            }

            try
            {
                string pluginPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                string bundlePath = System.IO.Path.Combine(pluginPath, "custom_props.bundle");

                if (!System.IO.File.Exists(bundlePath)) return false;

                _customPropsBundle = AssetBundle.LoadFromFile(bundlePath);
                if (_customPropsBundle == null) return false;

                string[] assetNames = _customPropsBundle.GetAllAssetNames();

                foreach (var name in assetNames)
                {
                    if (name.EndsWith(".prefab"))
                    {
                        _loadedPrefab = _customPropsBundle.LoadAsset<GameObject>(name);
                        if (_loadedPrefab != null) break;
                    }
                }

                foreach (var name in assetNames)
                {
                    if (name.Contains("icon") && name.EndsWith(".png"))
                    {
                        _loadedIcon = _customPropsBundle.LoadAsset<Sprite>(name);
                        if (_loadedIcon != null) break;
                    }
                }

                if (_loadedIcon == null)
                {
                    var sprites = _customPropsBundle.LoadAllAssets<Sprite>();
                    if (sprites.Length > 0) _loadedIcon = sprites[0];
                }

                return _loadedPrefab != null;
            }
            catch (Exception ex)
            {
                Log.LogError($"[INJECTOR] Error loading AssetBundle: {ex}");
                return false;
            }
        }

        private static void CreateFallbackPrefab()
        {
            _customPrefab = new GameObject("CustomInjectedProp_BaseType");
            _customPrefab.SetActive(false);

            var staticPropType = AccessTools.TypeByName("Endless.Gameplay.StaticProp");
            if (staticPropType != null) _customPrefab.AddComponent(staticPropType);

            _customPrefab.SetActive(true);
            UnityEngine.Object.DontDestroyOnLoad(_customPrefab);

            _visualPrefab = new GameObject("CustomInjectedProp_Visual");
            _visualPrefab.transform.position = new Vector3(0, -10000, 0);

            var meshFilter = _visualPrefab.AddComponent<MeshFilter>();
            var meshRenderer = _visualPrefab.AddComponent<MeshRenderer>();
            meshFilter.mesh = CreateCubeMesh();

            var tempCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            meshRenderer.material = new Material(tempCube.GetComponent<MeshRenderer>().sharedMaterial);
            meshRenderer.material.color = Color.magenta;
            UnityEngine.Object.DestroyImmediate(tempCube);

            UnityEngine.Object.DontDestroyOnLoad(_visualPrefab);
        }

        private static Mesh CreateCubeMesh()
        {
            var mesh = new Mesh();

            Vector3[] vertices = {
                new Vector3(-0.5f, -0.5f, -0.5f), new Vector3( 0.5f, -0.5f, -0.5f),
                new Vector3( 0.5f,  0.5f, -0.5f), new Vector3(-0.5f,  0.5f, -0.5f),
                new Vector3(-0.5f, -0.5f,  0.5f), new Vector3( 0.5f, -0.5f,  0.5f),
                new Vector3( 0.5f,  0.5f,  0.5f), new Vector3(-0.5f,  0.5f,  0.5f)
            };

            int[] triangles = {
                0, 2, 1, 0, 3, 2, 1, 6, 5, 1, 2, 6,
                5, 7, 4, 5, 6, 7, 4, 3, 0, 4, 7, 3,
                3, 6, 2, 3, 7, 6, 4, 1, 5, 4, 0, 1
            };

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();

            return mesh;
        }

        private static void CreateCustomBaseTypeDefinition()
        {
            if (_customBaseTypeDefinition != null) return;

            _customBaseTypeDefinition = ScriptableObject.CreateInstance(_baseTypeDefinitionType);

            var prefabField = AccessTools.Field(_componentDefinitionType, "prefab");
            prefabField?.SetValue(_customBaseTypeDefinition, _customPrefab);

            var componentIdField = AccessTools.Field(_componentDefinitionType, "componentId");
            if (componentIdField != null)
            {
                var guid = CreateSerializableGuid(CustomBaseTypeGuid);
                if (guid != null) componentIdField.SetValue(_customBaseTypeDefinition, guid);
            }

            var isUserExposedField = AccessTools.Field(_baseTypeDefinitionType, "isUserExposed");
            isUserExposedField?.SetValue(_customBaseTypeDefinition, true);

            var isNetworkedField = AccessTools.Field(_componentDefinitionType, "isNetworked");
            // v10.31.0: MUST be true for proper network spawn and pickup functionality
            isNetworkedField?.SetValue(_customBaseTypeDefinition, true);

            CustomBaseTypes[CustomBaseTypeGuid] = _customBaseTypeDefinition;
            UnityEngine.Object.DontDestroyOnLoad(_customBaseTypeDefinition as UnityEngine.Object);
        }

        private static object CreateSerializableGuid(string guidString)
        {
            try
            {
                var ctor = _serializableGuidType.GetConstructor(new Type[] { typeof(string) });
                if (ctor != null) return ctor.Invoke(new object[] { guidString });

                var implicitOp = _serializableGuidType.GetMethod("op_Implicit", new Type[] { typeof(string) });
                if (implicitOp != null) return implicitOp.Invoke(null, new object[] { guidString });

                var guidCtor = _serializableGuidType.GetConstructor(new Type[] { typeof(Guid) });
                if (guidCtor != null) return guidCtor.Invoke(new object[] { Guid.Parse(guidString) });
            }
            catch (Exception ex)
            {
                Log.LogError($"[INJECTOR] CreateSerializableGuid failed: {ex}");
            }
            return null;
        }

        private static void InjectCustomProp(object stageManager)
        {
            try
            {
                var prop = Activator.CreateInstance(_propType);
                var assetCoreType = AccessTools.TypeByName("Endless.Assets.AssetCore");

                var nameField = assetCoreType?.GetField("Name");
                nameField?.SetValue(prop, _loadedPrefab != null ? _loadedPrefab.name : "Custom Injected Prop");

                var assetIdField = assetCoreType?.GetField("AssetID");
                assetIdField?.SetValue(prop, CustomPropGuid);

                var assetTypeField = assetCoreType?.GetField("AssetType");
                assetTypeField?.SetValue(prop, "Prop");

                var baseTypeIdField = AccessTools.Field(_propType, "baseTypeId");
                baseTypeIdField?.SetValue(prop, CustomBaseTypeGuid);

                var componentIdsField = AccessTools.Field(_propType, "componentIds");
                componentIdsField?.SetValue(prop, new List<string>());

                var propLocationOffsetsField = AccessTools.Field(_propType, "propLocationOffsets");
                if (propLocationOffsetsField != null)
                {
                    var propLocationOffsetType = AccessTools.TypeByName("Endless.Props.Assets.PropLocationOffset");
                    if (propLocationOffsetType != null)
                    {
                        var offset = Activator.CreateInstance(propLocationOffsetType);
                        propLocationOffsetType.GetField("Offset")?.SetValue(offset, Vector3Int.zero);

                        var offsetArray = Array.CreateInstance(propLocationOffsetType, 1);
                        offsetArray.SetValue(offset, 0);
                        propLocationOffsetsField.SetValue(prop, offsetArray);
                    }
                }

                CustomProps[CustomPropGuid] = prop;

                var injectPropMethod = AccessTools.Method(_stageManagerType, "InjectProp");
                if (injectPropMethod != null)
                {
                    Sprite iconSprite = _loadedIcon;
                    if (iconSprite == null)
                    {
                        var iconTexture = new Texture2D(64, 64);
                        var colors = new Color[64 * 64];
                        for (int i = 0; i < colors.Length; i++) colors[i] = Color.magenta;
                        iconTexture.SetPixels(colors);
                        iconTexture.Apply();
                        iconSprite = Sprite.Create(iconTexture, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f));
                    }

                    // v10.43.0: Don't pass testPrefab - matches how normal game TreasureItems work
                    // Research showed that normal game Items have testPrefab=null and use only VisualsInfo.
                    // The visual comes from tempVisualsInfoGround -> runtimeGroundVisuals via ComponentInitialize.
                    // Passing testPrefab caused double visual because BuildPrefab instantiates it in ADDITION
                    // to the runtimeGroundVisuals created by ComponentInitialize.
                    injectPropMethod.Invoke(stageManager, new object[] { prop, null, null, iconSprite });
                    Log.LogInfo("[INJECTOR] Called StageManager.InjectProp (testPrefab=null, using VisualsInfo only)");
                }
            }
            catch (Exception ex)
            {
                Log.LogError($"[INJECTOR] InjectCustomProp failed: {ex}");
            }
        }

        // ========== v10.33.0: COMPONENT INITIALIZE VISUAL INJECTION ==========

        /// <summary>
        /// v10.33.0: Hook Item.ComponentInitialize to inject our custom visuals at the RIGHT moment.
        /// This is the correct approach because:
        /// - BuildPrefab creates a FRESH TreasureItem from the base type definition (not our clone)
        /// - ComponentInitialize reads VisualsInfo from that fresh instance
        /// - We hook ComponentInitialize to modify VisualsInfo BEFORE visuals are instantiated
        /// </summary>
        private void PatchItemComponentInitialize()
        {
            if (_itemType != null)
            {
                var method = AccessTools.Method(_itemType, "ComponentInitialize");
                if (method != null)
                {
                    var prefix = new HarmonyMethod(typeof(Plugin).GetMethod(nameof(Item_ComponentInitialize_Prefix),
                        BindingFlags.Public | BindingFlags.Static));
                    // v10.38.0: Removed postfix - it was running on template, affecting preview
                    // Renderer hiding now done in TrackNonNetworkedObject postfix (for placed props only)
                    _harmony.Patch(method, prefix: prefix);
                    Log.LogInfo("[INJECTOR] Patched Item.ComponentInitialize (v10.33.0 prefix only)");
                }
                else
                {
                    Log.LogWarning("[INJECTOR] Item.ComponentInitialize method not found");
                }
            }

            // v10.43.0: TrackNonNetworkedObject patch no longer needed since we don't pass testPrefab
            // Keeping the patch for diagnostic logging only
            PatchTrackNonNetworkedObject();
        }

        private static Type _stageType;
        private void PatchTrackNonNetworkedObject()
        {
            _stageType = AccessTools.TypeByName("Endless.Gameplay.LevelEditing.Level.Stage");
            if (_stageType == null)
            {
                Log.LogWarning("[INJECTOR] Stage type not found for TrackNonNetworkedObject patch");
                return;
            }

            var method = AccessTools.Method(_stageType, "TrackNonNetworkedObject");
            if (method != null)
            {
                var postfix = new HarmonyMethod(typeof(Plugin).GetMethod(nameof(TrackNonNetworkedObject_Postfix),
                    BindingFlags.Public | BindingFlags.Static));
                _harmony.Patch(method, postfix: postfix);
                Log.LogInfo("[INJECTOR] Patched Stage.TrackNonNetworkedObject (v10.43.0 - diagnostic logging only)");
            }
            else
            {
                Log.LogWarning("[INJECTOR] Stage.TrackNonNetworkedObject method not found");
            }
        }

        // v10.43.0: Track which props we've logged to avoid spam
        private static HashSet<string> _loggedPropTypes = new HashSet<string>();

        /// <summary>
        /// v10.43.0: POSTFIX for Stage.TrackNonNetworkedObject.
        /// Now only logs diagnostic info since we no longer pass testPrefab to InjectProp.
        /// </summary>
        public static void TrackNonNetworkedObject_Postfix(object assetId, object instanceId, GameObject newObject)
        {
            try
            {
                if (newObject == null) return;

                bool isOurProp = newObject.GetComponent<CustomPropMarker>() != null;
                if (!isOurProp) return; // Only log our custom prop

                Log.LogInfo($"[TRACK-POSTFIX] v10.43.0: Our custom prop placed");

                // Get the Item component and log runtime visuals for diagnostics
                var item = newObject.GetComponentInChildren(_itemType) as MonoBehaviour;
                if (item == null)
                {
                    Log.LogWarning("[TRACK-POSTFIX] No Item component found");
                    return;
                }

                // Get runtimeGroundVisuals and runtimeEquippedVisuals via reflection
                var runtimeGroundVisualsField = _itemType.GetField("runtimeGroundVisuals", BindingFlags.NonPublic | BindingFlags.Instance);
                var runtimeEquippedVisualsField = _itemType.GetField("runtimeEquippedVisuals", BindingFlags.NonPublic | BindingFlags.Instance);

                GameObject runtimeGroundVisuals = runtimeGroundVisualsField?.GetValue(item) as GameObject;
                GameObject runtimeEquippedVisuals = runtimeEquippedVisualsField?.GetValue(item) as GameObject;

                Log.LogInfo($"[TRACK-POSTFIX] runtimeGroundVisuals: {runtimeGroundVisuals?.name ?? "null"}");
                Log.LogInfo($"[TRACK-POSTFIX] runtimeEquippedVisuals: {runtimeEquippedVisuals?.name ?? "null"}");

                // Log all renderers for diagnostics
                var allRenderers = newObject.GetComponentsInChildren<Renderer>(true);
                Log.LogInfo($"[TRACK-POSTFIX] Total renderers: {allRenderers.Length}");
                foreach (var r in allRenderers)
                {
                    Log.LogInfo($"[TRACK-POSTFIX]   {r.gameObject.name} | enabled={r.enabled}");
                }

                // v10.43.0: No longer destroying testPrefab because we don't pass it to InjectProp
                // The hierarchy should now only have runtimeGroundVisuals and runtimeEquippedVisuals
            }
            catch (Exception ex)
            {
                Log.LogError($"[TRACK-POSTFIX] Error: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private static void LogHierarchy(Transform t, int depth)
        {
            string indent = new string(' ', depth * 2);
            var components = t.GetComponents<Component>();
            string compList = string.Join(", ", components.Select(c => c.GetType().Name));
            Log.LogInfo($"[HIERARCHY] {indent}{t.name} [{compList}]");
            foreach (Transform child in t)
            {
                LogHierarchy(child, depth + 1);
            }
        }

        private static string GetTransformPath(Transform t)
        {
            string path = t.name;
            while (t.parent != null)
            {
                t = t.parent;
                path = t.name + "/" + path;
            }
            return path;
        }

        /// <summary>
        /// v10.33.0: PREFIX hook for Item.ComponentInitialize.
        /// Checks if this is our custom prop and injects our Pearl Basket visual.
        /// This runs BEFORE ComponentInitialize instantiates the visuals.
        /// </summary>
        public static void Item_ComponentInitialize_Prefix(object __instance, object referenceBase, object endlessProp)
        {
            try
            {
                if (endlessProp == null || __instance == null) return;

                // v10.33.1: Get the Prop from EndlessProp - NOTE: Prop is a PROPERTY, not a field!
                var propProperty = AccessTools.Property(_endlessPropType, "Prop");
                if (propProperty == null)
                {
                    Log.LogError("[VISUAL-INJECT] Could not find Prop property on EndlessProp");
                    return;
                }
                var prop = propProperty.GetValue(endlessProp);
                if (prop == null)
                {
                    Log.LogWarning("[VISUAL-INJECT] Prop is null on EndlessProp");
                    return;
                }

                // Get the AssetID - this is also likely a property
                var assetIdProperty = AccessTools.Property(_propType, "AssetID");
                object assetId = null;
                if (assetIdProperty != null)
                {
                    assetId = assetIdProperty.GetValue(prop);
                }
                else
                {
                    // Try as field
                    var assetIdField = AccessTools.Field(_propType, "AssetID");
                    if (assetIdField != null)
                    {
                        assetId = assetIdField.GetValue(prop);
                    }
                }

                if (assetId == null)
                {
                    Log.LogWarning("[VISUAL-INJECT] Could not get AssetID from Prop");
                    return;
                }

                // Convert to string for comparison
                string assetIdStr = assetId.ToString();
                Log.LogInfo($"[VISUAL-INJECT] ComponentInitialize called for AssetID: {assetIdStr}");

                // Check if this is our custom prop
                if (assetIdStr != CustomPropGuid.ToString())
                {
                    return;  // Not our prop, let original handle it
                }

                Log.LogInfo($"[VISUAL-INJECT] ComponentInitialize for OUR CUSTOM PROP detected!");

                // v10.39.0: Add marker component to identify this as our custom prop
                // This marker will be present on template AND placed instances (since placement clones the template)
                // The TrackNonNetworkedObject_Postfix checks for this marker to hide testPrefab
                var endlessPropMono = endlessProp as MonoBehaviour;
                if (endlessPropMono != null && endlessPropMono.GetComponent<CustomPropMarker>() == null)
                {
                    endlessPropMono.gameObject.AddComponent<CustomPropMarker>();
                    Log.LogInfo("[VISUAL-INJECT] Added CustomPropMarker to EndlessProp");
                }

                // v10.42.0: Removed TestPrefabMarker logic - it was marking the wrong object (Interactable)
                // testPrefab is now identified and destroyed in TrackNonNetworkedObject_Postfix by checking
                // for direct children of Item with renderers that are NOT runtimeGroundVisuals or runtimeEquippedVisuals

                // Ensure we have our visual prefab ready
                if (_pearlBasketVisualPrefab == null)
                {
                    CreatePearlBasketVisualPrefab();
                }

                if (_pearlBasketVisualPrefab == null)
                {
                    Log.LogError("[VISUAL-INJECT] Pearl Basket visual prefab is null!");
                    return;
                }

                // Now modify the VisualsInfo on THIS INSTANCE (the actual TreasureItem being initialized)
                // The __instance is the Item (TreasureItem) that ComponentInitialize will use

                // Get the VisualsInfo struct type
                var visualsInfoType = _itemType.GetNestedType("VisualsInfo", BindingFlags.NonPublic | BindingFlags.Public);
                if (visualsInfoType == null)
                {
                    Log.LogError("[VISUAL-INJECT] Could not find VisualsInfo type");
                    return;
                }

                // Modify tempVisualsInfoGround (ground visual with rotation)
                var groundField = AccessTools.Field(_treasureItemType, "tempVisualsInfoGround");
                if (groundField != null)
                {
                    // Get current struct value (copy)
                    var groundInfo = groundField.GetValue(__instance);
                    if (groundInfo != null)
                    {
                        // Modify the GameObject field
                        var gameObjectField = visualsInfoType.GetField("GameObject");
                        if (gameObjectField != null)
                        {
                            gameObjectField.SetValue(groundInfo, _pearlBasketVisualPrefab);
                            // Write back the modified struct
                            groundField.SetValue(__instance, groundInfo);
                            Log.LogInfo("[VISUAL-INJECT] Injected Pearl Basket into GroundVisualsInfo");
                        }
                    }
                }

                // Modify tempVisualsInfoEqupped (equipped visual - use same prefab or empty)
                var equippedField = AccessTools.Field(_treasureItemType, "tempVisualsInfoEqupped");
                if (equippedField != null)
                {
                    var equippedInfo = equippedField.GetValue(__instance);
                    if (equippedInfo != null)
                    {
                        var gameObjectField = visualsInfoType.GetField("GameObject");
                        if (gameObjectField != null)
                        {
                            // Use same visual for equipped (or could use empty placeholder)
                            gameObjectField.SetValue(equippedInfo, _pearlBasketVisualPrefab);
                            equippedField.SetValue(__instance, equippedInfo);
                            Log.LogInfo("[VISUAL-INJECT] Injected Pearl Basket into EquippedVisualsInfo");
                        }
                    }
                }

                // v10.37.0: Renderer hiding moved to POSTFIX hook to avoid breaking preview
                // The postfix runs AFTER ComponentInitialize creates GroundVisualsInfo visual,
                // so we can safely identify and hide just the testPrefab visual.

                // v10.33.1: Clone the InventoryUsableDefinition to avoid modifying shared definition
                // This preserves animationTrigger while allowing custom icon
                var definitionField = AccessTools.Field(_itemType, "inventoryUsableDefinition");
                if (definitionField != null)
                {
                    var originalDefinition = definitionField.GetValue(__instance);
                    if (originalDefinition != null && _genericUsableDefinitionType != null)
                    {
                        Log.LogInfo($"[VISUAL-INJECT] Found original definition: {((ScriptableObject)originalDefinition).name}");

                        // Create a new definition instance
                        var clonedDefinition = ScriptableObject.CreateInstance(_genericUsableDefinitionType);
                        if (clonedDefinition != null)
                        {
                            // Copy ALL fields from original to clone (preserves animationTrigger)
                            CopyDefinitionFields(originalDefinition, clonedDefinition);

                            // Now override specific fields for our custom item
                            if (_usableDefinitionType != null)
                            {
                                // Set our custom icon
                                var spriteField = AccessTools.Field(_usableDefinitionType, "sprite");
                                if (spriteField != null && _loadedIcon != null)
                                {
                                    spriteField.SetValue(clonedDefinition, _loadedIcon);
                                    Log.LogInfo("[VISUAL-INJECT] Set custom icon on cloned definition");
                                }
                                else if (_loadedIcon == null)
                                {
                                    Log.LogWarning("[VISUAL-INJECT] _loadedIcon is null - icon won't be changed");
                                }

                                // Set display name
                                var displayNameField = AccessTools.Field(_usableDefinitionType, "displayName");
                                if (displayNameField != null)
                                {
                                    displayNameField.SetValue(clonedDefinition, "Pearl Basket");
                                    Log.LogInfo("[VISUAL-INJECT] Set display name to 'Pearl Basket'");
                                }

                                // Set a unique GUID for our definition
                                var guidField = AccessTools.Field(_usableDefinitionType, "guid");
                                if (guidField != null)
                                {
                                    var serializableGuidType = guidField.FieldType;
                                    var guidConstructor = serializableGuidType.GetConstructor(new Type[] { typeof(string) });
                                    if (guidConstructor != null)
                                    {
                                        var newGuid = guidConstructor.Invoke(new object[] { CustomPropGuid.ToString() });
                                        guidField.SetValue(clonedDefinition, newGuid);
                                        Log.LogInfo($"[VISUAL-INJECT] Set definition GUID to {CustomPropGuid}");
                                    }
                                }
                            }

                            // Assign the cloned definition to this Item instance
                            definitionField.SetValue(__instance, clonedDefinition);
                            ((ScriptableObject)clonedDefinition).name = "Pearl Basket Usable Definition";
                            UnityEngine.Object.DontDestroyOnLoad((UnityEngine.Object)clonedDefinition);
                            Log.LogInfo("[VISUAL-INJECT] Assigned cloned definition to Item");

                            // v10.34.0: CRITICAL - Register the cloned definition with RuntimeDatabase
                            // Without this, the game's animation system can't look up our definition by GUID!
                            RegisterDefinitionWithRuntimeDatabase(clonedDefinition);
                        }
                        else
                        {
                            Log.LogError("[VISUAL-INJECT] Failed to create cloned definition");
                        }
                    }
                    else
                    {
                        Log.LogWarning($"[VISUAL-INJECT] originalDefinition={originalDefinition != null}, _genericUsableDefinitionType={_genericUsableDefinitionType != null}");
                    }
                }

                Log.LogInfo("[VISUAL-INJECT] Successfully injected custom visuals into ComponentInitialize");
            }
            catch (Exception ex)
            {
                Log.LogError($"[VISUAL-INJECT] Error in ComponentInitialize prefix: {ex.Message}\n{ex.StackTrace}");
            }
        }

        /// <summary>
        /// v10.37.0: POSTFIX hook for Item.ComponentInitialize.
        /// Runs AFTER ComponentInitialize has created runtimeGroundVisuals and runtimeEquippedVisuals.
        /// Hides the testPrefab visual to prevent double visual issue while preserving preview functionality.
        /// </summary>
        public static void Item_ComponentInitialize_Postfix(object __instance, object referenceBase, object endlessProp)
        {
            try
            {
                if (endlessProp == null || __instance == null) return;

                // Get the Prop from EndlessProp to check if this is our custom prop
                var propProperty = AccessTools.Property(_endlessPropType, "Prop");
                if (propProperty == null) return;

                var prop = propProperty.GetValue(endlessProp);
                if (prop == null) return;

                var assetIdProperty = AccessTools.Property(_propType, "AssetID");
                object assetId = null;
                if (assetIdProperty != null)
                {
                    assetId = assetIdProperty.GetValue(prop);
                }
                else
                {
                    var assetIdField = AccessTools.Field(_propType, "AssetID");
                    if (assetIdField != null)
                        assetId = assetIdField.GetValue(prop);
                }

                if (assetId == null) return;

                string assetIdStr = assetId.ToString();
                if (assetIdStr != CustomPropGuid.ToString()) return;

                Log.LogInfo($"[VISUAL-POSTFIX] v10.37.0: Processing postfix for our custom prop");

                // Get runtimeGroundVisuals and runtimeEquippedVisuals from Item instance
                // These are the visuals created by ComponentInitialize that we want to KEEP
                var groundVisualsField = AccessTools.Field(_itemType, "runtimeGroundVisuals");
                var equippedVisualsField = AccessTools.Field(_itemType, "runtimeEquippedVisuals");

                GameObject runtimeGroundVisuals = null;
                GameObject runtimeEquippedVisuals = null;

                if (groundVisualsField != null)
                {
                    runtimeGroundVisuals = groundVisualsField.GetValue(__instance) as GameObject;
                    Log.LogInfo($"[VISUAL-POSTFIX] runtimeGroundVisuals: {runtimeGroundVisuals?.name ?? "null"}");
                }

                if (equippedVisualsField != null)
                {
                    runtimeEquippedVisuals = equippedVisualsField.GetValue(__instance) as GameObject;
                    Log.LogInfo($"[VISUAL-POSTFIX] runtimeEquippedVisuals: {runtimeEquippedVisuals?.name ?? "null"}");
                }

                // Build a set of renderers that belong to the game's visual system (keep these)
                var keepRenderers = new HashSet<Renderer>();

                if (runtimeGroundVisuals != null)
                {
                    foreach (var r in runtimeGroundVisuals.GetComponentsInChildren<Renderer>(true))
                    {
                        keepRenderers.Add(r);
                    }
                }

                if (runtimeEquippedVisuals != null)
                {
                    foreach (var r in runtimeEquippedVisuals.GetComponentsInChildren<Renderer>(true))
                    {
                        keepRenderers.Add(r);
                    }
                }

                Log.LogInfo($"[VISUAL-POSTFIX] Found {keepRenderers.Count} renderers to keep (from runtimeGroundVisuals/runtimeEquippedVisuals)");

                // Now find all renderers in the EndlessProp and hide those NOT in our keep set
                var endlessPropMono = endlessProp as MonoBehaviour;
                if (endlessPropMono == null) return;

                var allRenderers = endlessPropMono.GetComponentsInChildren<Renderer>(true);
                Log.LogInfo($"[VISUAL-POSTFIX] Found {allRenderers.Length} total renderers in EndlessProp");

                int hiddenCount = 0;
                foreach (var renderer in allRenderers)
                {
                    if (keepRenderers.Contains(renderer))
                    {
                        Log.LogInfo($"[VISUAL-POSTFIX] Keeping renderer '{renderer.gameObject.name}' (part of runtime visuals)");
                        continue;
                    }

                    // This renderer is from testPrefab, hide it
                    renderer.enabled = false;
                    hiddenCount++;
                    Log.LogInfo($"[VISUAL-POSTFIX] Hidden renderer '{renderer.gameObject.name}' (testPrefab visual)");
                }

                Log.LogInfo($"[VISUAL-POSTFIX] v10.37.0: Hidden {hiddenCount} testPrefab renderers, preview should still work");
            }
            catch (Exception ex)
            {
                Log.LogError($"[VISUAL-POSTFIX] Error in ComponentInitialize postfix: {ex.Message}\n{ex.StackTrace}");
            }
        }

        /// <summary>
        /// v10.34.0: Register a custom UsableDefinition with RuntimeDatabase's static dictionary.
        /// This is CRITICAL - without this, the game can't look up our definition by GUID,
        /// causing the animation system to fail when the item is equipped.
        /// </summary>
        private static bool _definitionRegistered = false;
        private static void RegisterDefinitionWithRuntimeDatabase(object clonedDefinition)
        {
            try
            {
                if (_definitionRegistered)
                {
                    Log.LogInfo("[RUNTIME-DB] Definition already registered, skipping");
                    return;
                }

                // Find RuntimeDatabase type
                Type runtimeDbType = null;
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    runtimeDbType = assembly.GetType("Endless.Gameplay.RuntimeDatabase");
                    if (runtimeDbType != null) break;
                }

                if (runtimeDbType == null)
                {
                    Log.LogError("[RUNTIME-DB] Could not find RuntimeDatabase type!");
                    return;
                }

                // Get the static usableDefinitionMap field
                var mapField = runtimeDbType.GetField("usableDefinitionMap", BindingFlags.NonPublic | BindingFlags.Static);
                if (mapField == null)
                {
                    Log.LogError("[RUNTIME-DB] Could not find usableDefinitionMap field!");
                    return;
                }

                // Get the dictionary
                var mapValue = mapField.GetValue(null);
                if (mapValue == null)
                {
                    Log.LogError("[RUNTIME-DB] usableDefinitionMap is null!");
                    return;
                }

                // Get the Guid from our cloned definition
                var guidProp = clonedDefinition.GetType().GetProperty("Guid");
                if (guidProp == null)
                {
                    Log.LogError("[RUNTIME-DB] Could not find Guid property on definition!");
                    return;
                }

                var guid = guidProp.GetValue(clonedDefinition);
                if (guid == null)
                {
                    Log.LogError("[RUNTIME-DB] Definition Guid is null!");
                    return;
                }

                // Use reflection to call the dictionary's Add or indexer
                // Dictionary<SerializableGuid, UsableDefinition>
                var dictType = mapValue.GetType();
                var indexerProp = dictType.GetProperty("Item");
                if (indexerProp != null)
                {
                    // Check if already registered
                    var containsKeyMethod = dictType.GetMethod("ContainsKey");
                    bool alreadyExists = containsKeyMethod != null && (bool)containsKeyMethod.Invoke(mapValue, new object[] { guid });

                    if (alreadyExists)
                    {
                        Log.LogInfo($"[RUNTIME-DB] Definition GUID {guid} already in RuntimeDatabase, updating");
                    }

                    // Set the value (works for both add and update)
                    indexerProp.SetValue(mapValue, clonedDefinition, new object[] { guid });
                    _definitionRegistered = true;
                    Log.LogInfo($"[RUNTIME-DB] Successfully registered definition with GUID {guid} in RuntimeDatabase!");

                    // Verify registration
                    var getMethod = dictType.GetMethod("TryGetValue");
                    if (getMethod != null)
                    {
                        var args = new object[] { guid, null };
                        bool found = (bool)getMethod.Invoke(mapValue, args);
                        Log.LogInfo($"[RUNTIME-DB] Verification: TryGetValue returned {found}");
                    }
                }
                else
                {
                    Log.LogError("[RUNTIME-DB] Could not find dictionary indexer!");
                }
            }
            catch (Exception ex)
            {
                Log.LogError($"[RUNTIME-DB] Error registering definition: {ex.Message}\n{ex.StackTrace}");
            }
        }

        /// <summary>
        /// v10.33.0: Create the Pearl Basket visual prefab with SimpleYawSpin.
        /// This is created once and reused for all instances.
        /// </summary>
        private static void CreatePearlBasketVisualPrefab()
        {
            try
            {
                // Load the asset bundle if not already loaded
                if (!LoadAssetBundle())
                {
                    Log.LogWarning("[VISUAL-INJECT] Could not load asset bundle, using fallback");
                    CreateFallbackVisualPrefab();
                    return;
                }

                // Use the loaded prefab as source
                GameObject sourceVisual = _loadedPrefab ?? _visualPrefab;
                if (sourceVisual == null)
                {
                    Log.LogWarning("[VISUAL-INJECT] No source visual found, using fallback");
                    CreateFallbackVisualPrefab();
                    return;
                }

                // Clone the visual
                _pearlBasketVisualPrefab = UnityEngine.Object.Instantiate(sourceVisual);
                _pearlBasketVisualPrefab.name = "PearlBasket_Visual";
                _pearlBasketVisualPrefab.SetActive(false);  // Keep inactive until instantiated by game
                UnityEngine.Object.DontDestroyOnLoad(_pearlBasketVisualPrefab);

                // v10.33.3: Do NOT add SimpleYawSpin to the visual prefab
                // The GroundedVisualsParent already has SimpleYawSpin which rotates all children
                // Adding it to the visual itself causes double rotation and wrong axis when equipped
                // Ground visual: inherits rotation from GroundedVisualsParent (correct)
                // Equipped visual: stays static under Item transform (correct)

                // Remove any existing SimpleYawSpin from the loaded prefab (in case it was baked in)
                if (_simpleYawSpinType != null)
                {
                    var existingSpinner = _pearlBasketVisualPrefab.GetComponent(_simpleYawSpinType);
                    if (existingSpinner != null)
                    {
                        UnityEngine.Object.DestroyImmediate(existingSpinner);
                        Log.LogInfo("[VISUAL-INJECT] Removed existing SimpleYawSpin from visual (rotation handled by parent)");
                    }
                }

                Log.LogInfo($"[VISUAL-INJECT] Created Pearl Basket visual prefab: {_pearlBasketVisualPrefab.name}");
            }
            catch (Exception ex)
            {
                Log.LogError($"[VISUAL-INJECT] Failed to create visual prefab: {ex.Message}");
                CreateFallbackVisualPrefab();
            }
        }

        private static void CreateFallbackVisualPrefab()
        {
            _pearlBasketVisualPrefab = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _pearlBasketVisualPrefab.name = "PearlBasket_Fallback";
            _pearlBasketVisualPrefab.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            var collider = _pearlBasketVisualPrefab.GetComponent<Collider>();
            if (collider != null) UnityEngine.Object.DestroyImmediate(collider);
            _pearlBasketVisualPrefab.SetActive(false);
            UnityEngine.Object.DontDestroyOnLoad(_pearlBasketVisualPrefab);
            Log.LogInfo("[VISUAL-INJECT] Created fallback sphere visual");
        }

        // Static reference to our visual prefab
        private static GameObject _pearlBasketVisualPrefab;

        // ========== v10.23.0: ITEM PICKUP DIAGNOSTIC PATCHES ==========

        private void PatchItemPickup()
        {
            if (_itemType != null)
            {
                var method = AccessTools.Method(_itemType, "Pickup");
                if (method != null)
                {
                    var prefix = new HarmonyMethod(typeof(Plugin).GetMethod(nameof(Item_Pickup_Prefix),
                        BindingFlags.Public | BindingFlags.Static));
                    var postfix = new HarmonyMethod(typeof(Plugin).GetMethod(nameof(Item_Pickup_Postfix),
                        BindingFlags.Public | BindingFlags.Static));
                    _harmony.Patch(method, prefix: prefix, postfix: postfix);
                    Log.LogInfo("[INJECTOR] Patched Item.Pickup (prefix+postfix)");
                }
                else
                {
                    Log.LogWarning("[INJECTOR] Item.Pickup method not found");
                }
            }
        }

        private void PatchInventoryAttemptPickup()
        {
            if (_inventoryType != null)
            {
                // Patch the Item overload: AttemptPickupItem(Item item, bool lockItem)
                var method = AccessTools.Method(_inventoryType, "AttemptPickupItem", new Type[] { _itemType, typeof(bool) });
                if (method != null)
                {
                    var prefix = new HarmonyMethod(typeof(Plugin).GetMethod(nameof(Inventory_AttemptPickupItem_Prefix),
                        BindingFlags.Public | BindingFlags.Static));
                    var postfix = new HarmonyMethod(typeof(Plugin).GetMethod(nameof(Inventory_AttemptPickupItem_Postfix),
                        BindingFlags.Public | BindingFlags.Static));
                    _harmony.Patch(method, prefix: prefix, postfix: postfix);
                    Log.LogInfo("[INJECTOR] Patched Inventory.AttemptPickupItem(Item) (prefix+postfix)");
                }
                else
                {
                    Log.LogWarning("[INJECTOR] Inventory.AttemptPickupItem(Item) method not found");
                }
            }
        }

        private void PatchItemInteractable()
        {
            if (_itemInteractableType != null)
            {
                var method = AccessTools.Method(_itemInteractableType, "AttemptInteract_ServerLogic");
                if (method != null)
                {
                    var prefix = new HarmonyMethod(typeof(Plugin).GetMethod(nameof(ItemInteractable_AttemptInteract_Prefix),
                        BindingFlags.Public | BindingFlags.Static));
                    var postfix = new HarmonyMethod(typeof(Plugin).GetMethod(nameof(ItemInteractable_AttemptInteract_Postfix),
                        BindingFlags.Public | BindingFlags.Static));
                    _harmony.Patch(method, prefix: prefix, postfix: postfix);
                    Log.LogInfo("[INJECTOR] Patched ItemInteractable.AttemptInteract_ServerLogic (prefix+postfix)");
                }
                else
                {
                    Log.LogWarning("[INJECTOR] ItemInteractable.AttemptInteract_ServerLogic method not found");
                }
            }
        }

        // v10.34.0: Patch ToggleLocalVisibility to track animator state after equipping
        private void PatchToggleLocalVisibility()
        {
            if (_itemType != null)
            {
                var method = AccessTools.Method(_itemType, "ToggleLocalVisibility");
                if (method != null)
                {
                    var postfix = new HarmonyMethod(typeof(Plugin).GetMethod(nameof(ToggleLocalVisibility_Postfix),
                        BindingFlags.Public | BindingFlags.Static));
                    _harmony.Patch(method, postfix: postfix);
                    Log.LogInfo("[INJECTOR] Patched Item.ToggleLocalVisibility (v10.34.0 animator diagnostic)");
                }
                else
                {
                    Log.LogWarning("[INJECTOR] Item.ToggleLocalVisibility method not found");
                }
            }
        }

        /// <summary>
        /// v10.34.0: Logs animator state after ToggleLocalVisibility is called
        /// This helps diagnose why movement animations might be stuck
        /// </summary>
        public static void ToggleLocalVisibility_Postfix(object __instance, object playerReferences, bool visible, bool useEquipmentAnimation)
        {
            try
            {
                string itemName = (__instance as MonoBehaviour)?.name ?? "Unknown";

                Log.LogInfo("==========================================================");
                Log.LogInfo("[EQUIP-ANIM] ===== Item.ToggleLocalVisibility called =====");
                Log.LogInfo($"[EQUIP-ANIM] Item: {itemName}");
                Log.LogInfo($"[EQUIP-ANIM] Visible: {visible}, UseEquipmentAnimation: {useEquipmentAnimation}");

                // Get animator from player
                if (playerReferences != null)
                {
                    var appearanceControllerProp = playerReferences.GetType().GetProperty("ApperanceController");
                    var appearanceController = appearanceControllerProp?.GetValue(playerReferences);
                    if (appearanceController != null)
                    {
                        var animatorProp = appearanceController.GetType().GetProperty("AppearanceAnimator");
                        var appearanceAnimator = animatorProp?.GetValue(appearanceController);
                        if (appearanceAnimator != null)
                        {
                            var animatorProp2 = appearanceAnimator.GetType().GetProperty("Animator");
                            var animator = animatorProp2?.GetValue(appearanceAnimator) as Animator;
                            if (animator != null)
                            {
                                // Log critical animator state
                                var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                                Log.LogInfo($"[EQUIP-ANIM] Animator State Hash: {stateInfo.shortNameHash}");
                                Log.LogInfo($"[EQUIP-ANIM] Animator FullPath Hash: {stateInfo.fullPathHash}");
                                Log.LogInfo($"[EQUIP-ANIM] Animator normalizedTime: {stateInfo.normalizedTime:F3}");

                                // Log key parameters
                                int equippedItem = animator.GetInteger("EquippedItem");
                                bool moving = animator.GetBool("Moving");
                                bool walking = animator.GetBool("Walking");
                                bool grounded = animator.GetBool("Grounded");
                                float velX = animator.GetFloat("VelX");
                                float velZ = animator.GetFloat("VelZ");
                                float horizVel = animator.GetFloat("HorizVelMagnitude");

                                Log.LogInfo($"[EQUIP-ANIM] EquippedItem: {equippedItem}");
                                Log.LogInfo($"[EQUIP-ANIM] Moving: {moving}, Walking: {walking}, Grounded: {grounded}");
                                Log.LogInfo($"[EQUIP-ANIM] VelX: {velX:F3}, VelZ: {velZ:F3}, HorizVel: {horizVel:F3}");

                                // Log animator layer weights
                                int layerCount = animator.layerCount;
                                Log.LogInfo($"[EQUIP-ANIM] Animator Layers ({layerCount}):");
                                for (int i = 0; i < layerCount; i++)
                                {
                                    float weight = animator.GetLayerWeight(i);
                                    string layerName = animator.GetLayerName(i);
                                    var layerState = animator.GetCurrentAnimatorStateInfo(i);
                                    Log.LogInfo($"[EQUIP-ANIM]   Layer {i} '{layerName}': weight={weight:F2}, stateHash={layerState.shortNameHash}");
                                }

                                // Check if animator is in transition
                                bool isInTransition = animator.IsInTransition(0);
                                Log.LogInfo($"[EQUIP-ANIM] IsInTransition: {isInTransition}");
                                if (isInTransition)
                                {
                                    var nextState = animator.GetNextAnimatorStateInfo(0);
                                    Log.LogInfo($"[EQUIP-ANIM] NextState Hash: {nextState.shortNameHash}");
                                }
                            }
                            else
                            {
                                Log.LogWarning("[EQUIP-ANIM] Could not get Animator component");
                            }
                        }
                    }
                }

                // Get item's equippedItemID for reference
                var itemType = __instance.GetType();
                var equippedItemIdField = AccessTools.Field(itemType.BaseType ?? itemType, "equippedItemID");
                int itemEquippedId = equippedItemIdField != null ? (int)equippedItemIdField.GetValue(__instance) : -1;
                var equippedParamNameField = AccessTools.Field(itemType.BaseType ?? itemType, "equippedItemParamName");
                string paramName = equippedParamNameField?.GetValue(__instance)?.ToString() ?? "Unknown";

                Log.LogInfo($"[EQUIP-ANIM] Item's equippedItemID: {itemEquippedId}");
                Log.LogInfo($"[EQUIP-ANIM] Item's equippedItemParamName: {paramName}");
                Log.LogInfo("==========================================================");
            }
            catch (Exception ex)
            {
                Log.LogError($"[EQUIP-ANIM] Error in postfix: {ex.Message}");
            }
        }

        // ========== v10.23.0: ITEM PICKUP DIAGNOSTIC METHODS ==========

        /// <summary>
        /// Logs when Item.Pickup is called BEFORE execution
        /// </summary>
        public static void Item_Pickup_Prefix(object __instance, object player)
        {
            try
            {
                string itemName = (__instance as MonoBehaviour)?.name ?? "Unknown";
                string playerName = "Unknown";

                // Try to get player name
                if (player != null)
                {
                    var nameProp = player.GetType().GetProperty("name");
                    if (nameProp != null)
                    {
                        playerName = nameProp.GetValue(player)?.ToString() ?? "Unknown";
                    }
                }

                // Get item details
                var itemType = __instance.GetType();
                string itemTypeName = itemType.Name;

                // AssetID
                var assetIdField = AccessTools.Field(itemType, "assetID");
                string assetId = assetIdField?.GetValue(__instance)?.ToString() ?? "Unknown";

                // IsPickupable
                var isPickupableProp = AccessTools.Property(itemType, "IsPickupable");
                bool isPickupable = isPickupableProp != null && (bool)isPickupableProp.GetValue(__instance);

                // ItemState
                var itemStateProp = AccessTools.Property(itemType, "ItemState");
                string itemState = itemStateProp?.GetValue(__instance)?.ToString() ?? "Unknown";

                // InventorySlot type
                var invSlotProp = AccessTools.Property(itemType, "InventorySlot");
                string invSlot = invSlotProp?.GetValue(__instance)?.ToString() ?? "Unknown";

                // IsStackable
                var isStackableProp = AccessTools.Property(itemType, "IsStackable");
                bool isStackable = isStackableProp != null && (bool)isStackableProp.GetValue(__instance);

                // StackCount
                var stackCountProp = AccessTools.Property(itemType, "StackCount");
                int stackCount = stackCountProp != null ? (int)stackCountProp.GetValue(__instance) : 1;

                // InventoryUsableDefinition
                var invUsableDefProp = AccessTools.Property(itemType, "InventoryUsableDefinition");
                object invUsableDef = invUsableDefProp?.GetValue(__instance);
                string defName = "None";
                string defGuid = "None";
                if (invUsableDef != null)
                {
                    defName = (invUsableDef as UnityEngine.Object)?.name ?? "Unknown";
                    var guidProp = invUsableDef.GetType().GetProperty("Guid");
                    defGuid = guidProp?.GetValue(invUsableDef)?.ToString() ?? "Unknown";
                }

                Log.LogInfo("==========================================================");
                Log.LogInfo("[ITEM-PICKUP] ===== Item.Pickup CALLED =====");
                Log.LogInfo($"[ITEM-PICKUP] Item: {itemName} (Type: {itemTypeName})");
                Log.LogInfo($"[ITEM-PICKUP] Player: {playerName}");
                Log.LogInfo($"[ITEM-PICKUP] AssetID: {assetId}");
                Log.LogInfo($"[ITEM-PICKUP] State: {itemState}");
                Log.LogInfo($"[ITEM-PICKUP] IsPickupable: {isPickupable}");
                Log.LogInfo($"[ITEM-PICKUP] InventorySlotType: {invSlot}");
                Log.LogInfo($"[ITEM-PICKUP] IsStackable: {isStackable}, StackCount: {stackCount}");
                Log.LogInfo($"[ITEM-PICKUP] InventoryUsableDefinition: {defName}");
                Log.LogInfo($"[ITEM-PICKUP] Definition GUID: {defGuid}");

                // v10.24.0: Dump full InventoryUsableDefinition data
                if (invUsableDef != null)
                {
                    LogInventoryUsableDefinition(invUsableDef);
                }

                Log.LogInfo("==========================================================");
            }
            catch (Exception ex)
            {
                Log.LogError($"[ITEM-PICKUP] Error in prefix: {ex.Message}");
            }
        }

        /// <summary>
        /// Logs Item.Pickup result AFTER execution
        /// </summary>
        public static void Item_Pickup_Postfix(object __instance, object __result, object player)
        {
            try
            {
                string resultItemName = "null";
                if (__result != null)
                {
                    resultItemName = (__result as MonoBehaviour)?.name ?? "Unknown";
                }

                Log.LogInfo($"[ITEM-PICKUP] Pickup returned: {resultItemName}");
                Log.LogInfo("----------------------------------------------------------");
            }
            catch (Exception ex)
            {
                Log.LogError($"[ITEM-PICKUP] Error in postfix: {ex.Message}");
            }
        }

        /// <summary>
        /// Logs when Inventory.AttemptPickupItem is called
        /// </summary>
        public static void Inventory_AttemptPickupItem_Prefix(object __instance, object item, bool lockItem)
        {
            try
            {
                string itemName = (item as MonoBehaviour)?.name ?? "Unknown";

                // Get inventory details
                var invType = __instance.GetType();

                // Total slots
                var totalSlotsProp = AccessTools.Property(invType, "TotalInventorySlotCount");
                int totalSlots = totalSlotsProp != null ? (int)totalSlotsProp.GetValue(__instance) : -1;

                // Get slots list to count empty
                var slotsField = AccessTools.Field(invType, "slots");
                int emptySlots = -1;
                if (slotsField != null)
                {
                    var slots = slotsField.GetValue(__instance) as IList;
                    if (slots != null)
                    {
                        emptySlots = 0;
                        foreach (var slot in slots)
                        {
                            if (slot == null) continue;
                            var defGuidProp = slot.GetType().GetProperty("DefinitionGuid");
                            if (defGuidProp != null)
                            {
                                var guid = defGuidProp.GetValue(slot);
                                // Check if empty (SerializableGuid.Empty)
                                var emptyField = guid?.GetType().GetField("Empty", BindingFlags.Public | BindingFlags.Static);
                                var emptyGuid = emptyField?.GetValue(null);
                                if (guid != null && guid.Equals(emptyGuid))
                                {
                                    emptySlots++;
                                }
                            }
                        }
                    }
                }

                Log.LogInfo("==========================================================");
                Log.LogInfo("[INVENTORY] ===== Inventory.AttemptPickupItem CALLED =====");
                Log.LogInfo($"[INVENTORY] Item: {itemName}");
                Log.LogInfo($"[INVENTORY] LockItem: {lockItem}");
                Log.LogInfo($"[INVENTORY] TotalSlots: {totalSlots}");
                Log.LogInfo($"[INVENTORY] EmptySlots: {emptySlots}");
                Log.LogInfo("==========================================================");
            }
            catch (Exception ex)
            {
                Log.LogError($"[INVENTORY] Error in prefix: {ex.Message}");
            }
        }

        /// <summary>
        /// Logs Inventory.AttemptPickupItem result
        /// </summary>
        public static void Inventory_AttemptPickupItem_Postfix(object __instance, bool __result, object item)
        {
            try
            {
                string itemName = (item as MonoBehaviour)?.name ?? "Unknown";
                Log.LogInfo($"[INVENTORY] AttemptPickupItem result: {__result} (item: {itemName})");

                // Log current inventory state after pickup
                if (__result)
                {
                    LogInventoryState(__instance);
                }
                Log.LogInfo("----------------------------------------------------------");
            }
            catch (Exception ex)
            {
                Log.LogError($"[INVENTORY] Error in postfix: {ex.Message}");
            }
        }

        /// <summary>
        /// Logs when ItemInteractable interaction occurs
        /// </summary>
        public static void ItemInteractable_AttemptInteract_Prefix(object __instance, object interactor)
        {
            try
            {
                string interactableName = (__instance as MonoBehaviour)?.name ?? "Unknown";

                // Get the item field
                var itemField = __instance.GetType().GetField("item", BindingFlags.NonPublic | BindingFlags.Instance);
                object item = itemField?.GetValue(__instance);
                string itemName = (item as MonoBehaviour)?.name ?? "Unknown";

                Log.LogInfo("==========================================================");
                Log.LogInfo("[INTERACT] ===== ItemInteractable.AttemptInteract =====");
                Log.LogInfo($"[INTERACT] Interactable: {interactableName}");
                Log.LogInfo($"[INTERACT] Item: {itemName}");

                // Log item details if available
                if (item != null)
                {
                    var itemType = item.GetType();
                    var assetIdField = AccessTools.Field(itemType, "assetID");
                    string assetId = assetIdField?.GetValue(item)?.ToString() ?? "Unknown";
                    Log.LogInfo($"[INTERACT] Item AssetID: {assetId}");
                    Log.LogInfo($"[INTERACT] Item Type: {itemType.Name}");
                }
                Log.LogInfo("==========================================================");
            }
            catch (Exception ex)
            {
                Log.LogError($"[INTERACT] Error in prefix: {ex.Message}");
            }
        }

        /// <summary>
        /// Logs ItemInteractable result
        /// </summary>
        public static void ItemInteractable_AttemptInteract_Postfix(object __instance, bool __result)
        {
            try
            {
                string interactableName = (__instance as MonoBehaviour)?.name ?? "Unknown";
                Log.LogInfo($"[INTERACT] AttemptInteract result: {__result} (interactable: {interactableName})");
                Log.LogInfo("----------------------------------------------------------");
            }
            catch (Exception ex)
            {
                Log.LogError($"[INTERACT] Error in postfix: {ex.Message}");
            }
        }

        /// <summary>
        /// Helper to log current inventory state
        /// </summary>
        private static void LogInventoryState(object inventory)
        {
            try
            {
                var invType = inventory.GetType();
                var slotsField = AccessTools.Field(invType, "slots");
                var slots = slotsField?.GetValue(inventory) as IList;

                if (slots == null)
                {
                    Log.LogInfo("[INVENTORY] Could not read slots");
                    return;
                }

                Log.LogInfo($"[INVENTORY] Current inventory state ({slots.Count} slots):");
                int slotIndex = 0;
                foreach (var slot in slots)
                {
                    if (slot == null)
                    {
                        Log.LogInfo($"[INVENTORY]   Slot {slotIndex}: null");
                        slotIndex++;
                        continue;
                    }

                    var slotType = slot.GetType();

                    // AssetID
                    var assetIdProp = slotType.GetProperty("AssetID");
                    string assetId = assetIdProp?.GetValue(slot)?.ToString() ?? "Empty";

                    // DefinitionGuid
                    var defGuidProp = slotType.GetProperty("DefinitionGuid");
                    string defGuid = defGuidProp?.GetValue(slot)?.ToString() ?? "Empty";

                    // Count
                    var countProp = slotType.GetProperty("Count");
                    int count = countProp != null ? (int)countProp.GetValue(slot) : 0;

                    // Item
                    var itemProp = slotType.GetProperty("Item");
                    object itemObj = itemProp?.GetValue(slot);
                    string itemName = (itemObj as MonoBehaviour)?.name ?? "null";

                    // Check if empty
                    bool isEmpty = defGuid.Contains("00000000-0000-0000-0000-000000000000") || itemObj == null;

                    if (!isEmpty)
                    {
                        Log.LogInfo($"[INVENTORY]   Slot {slotIndex}: {itemName} (Count: {count}, AssetID: {assetId})");
                    }
                    else
                    {
                        Log.LogInfo($"[INVENTORY]   Slot {slotIndex}: [EMPTY]");
                    }
                    slotIndex++;
                }
            }
            catch (Exception ex)
            {
                Log.LogError($"[INVENTORY] Error logging state: {ex.Message}");
            }
        }

        /// <summary>
        /// v10.24.0: Dump all properties of an InventoryUsableDefinition
        /// </summary>
        private static void LogInventoryUsableDefinition(object definition)
        {
            try
            {
                if (definition == null) return;

                Type defType = definition.GetType();
                Log.LogInfo($"[ITEM-DEF] ===== InventoryUsableDefinition Dump =====");
                Log.LogInfo($"[ITEM-DEF] Type: {defType.FullName}");

                // Get all public fields
                var fields = defType.GetFields(BindingFlags.Public | BindingFlags.Instance);
                Log.LogInfo($"[ITEM-DEF] --- Public Fields ({fields.Length}) ---");
                foreach (var field in fields)
                {
                    try
                    {
                        object value = field.GetValue(definition);
                        string valueStr = value?.ToString() ?? "null";

                        // Special handling for Unity Objects
                        if (value is UnityEngine.Object unityObj)
                        {
                            valueStr = $"{unityObj.name} ({unityObj.GetType().Name})";
                        }

                        Log.LogInfo($"[ITEM-DEF]   {field.Name} ({field.FieldType.Name}): {valueStr}");
                    }
                    catch (Exception ex)
                    {
                        Log.LogInfo($"[ITEM-DEF]   {field.Name}: <error: {ex.Message}>");
                    }
                }

                // Get all public properties
                var props = defType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                Log.LogInfo($"[ITEM-DEF] --- Public Properties ({props.Length}) ---");
                foreach (var prop in props)
                {
                    if (!prop.CanRead) continue;
                    try
                    {
                        object value = prop.GetValue(definition);
                        string valueStr = value?.ToString() ?? "null";

                        // Special handling for Unity Objects
                        if (value is UnityEngine.Object unityObj)
                        {
                            valueStr = $"{unityObj.name} ({unityObj.GetType().Name})";
                        }

                        Log.LogInfo($"[ITEM-DEF]   {prop.Name} ({prop.PropertyType.Name}): {valueStr}");
                    }
                    catch (Exception ex)
                    {
                        Log.LogInfo($"[ITEM-DEF]   {prop.Name}: <error: {ex.Message}>");
                    }
                }

                // Get private/protected fields too (often contain important data)
                var privateFields = defType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
                Log.LogInfo($"[ITEM-DEF] --- Private Fields ({privateFields.Length}) ---");
                foreach (var field in privateFields)
                {
                    try
                    {
                        object value = field.GetValue(definition);
                        string valueStr = value?.ToString() ?? "null";

                        // Special handling for Unity Objects
                        if (value is UnityEngine.Object unityObj)
                        {
                            valueStr = $"{unityObj.name} ({unityObj.GetType().Name})";
                        }
                        // Special handling for SerializableGuid
                        else if (value != null && field.FieldType.Name.Contains("Guid"))
                        {
                            valueStr = value.ToString();
                        }

                        Log.LogInfo($"[ITEM-DEF]   {field.Name} ({field.FieldType.Name}): {valueStr}");
                    }
                    catch (Exception ex)
                    {
                        Log.LogInfo($"[ITEM-DEF]   {field.Name}: <error: {ex.Message}>");
                    }
                }

                // Check base type for inherited fields
                Type baseType = defType.BaseType;
                if (baseType != null && baseType != typeof(UnityEngine.Object) && baseType != typeof(object))
                {
                    Log.LogInfo($"[ITEM-DEF] --- Base Type: {baseType.Name} ---");
                    var baseFields = baseType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    foreach (var field in baseFields)
                    {
                        try
                        {
                            object value = field.GetValue(definition);
                            string valueStr = value?.ToString() ?? "null";

                            if (value is UnityEngine.Object unityObj)
                            {
                                valueStr = $"{unityObj.name} ({unityObj.GetType().Name})";
                            }

                            Log.LogInfo($"[ITEM-DEF]   (base) {field.Name} ({field.FieldType.Name}): {valueStr}");
                        }
                        catch { }
                    }
                }

                Log.LogInfo($"[ITEM-DEF] ===== End Definition Dump =====");
            }
            catch (Exception ex)
            {
                Log.LogError($"[ITEM-DEF] Error dumping definition: {ex.Message}");
            }
        }

        // ========== v10.25.0: COLLECTIBLE RESEARCH DIAGNOSTICS ==========
        private static bool _collectibleResearchDone = false;

        /// <summary>
        /// v10.25.0: Research collectible system - find definition registries, dump TreasureItem, check layers
        /// Called once when first Treasure is found in scene
        /// </summary>
        private static void ResearchCollectibleSystem()
        {
            if (_collectibleResearchDone) return;
            _collectibleResearchDone = true;

            Log.LogInfo("==========================================================");
            Log.LogInfo("[COLLECT-RESEARCH] ===== COLLECTIBLE SYSTEM RESEARCH =====");
            Log.LogInfo("==========================================================");

            // 1. Find TreasureItem type and dump its structure
            ResearchTreasureItemStructure();

            // 2. Find where InventoryUsableDefinitions are stored
            ResearchDefinitionRegistry();

            // 3. Find ItemInteractable structure
            ResearchItemInteractableStructure();

            // 4. Check interaction layers
            ResearchInteractionLayers();

            Log.LogInfo("==========================================================");
            Log.LogInfo("[COLLECT-RESEARCH] ===== END COLLECTIBLE RESEARCH =====");
            Log.LogInfo("==========================================================");
        }

        /// <summary>
        /// v10.25.0: Find TreasureItem type and dump all its fields/properties
        /// </summary>
        private static void ResearchTreasureItemStructure()
        {
            Log.LogInfo("[COLLECT-RESEARCH] --- TreasureItem Structure ---");

            try
            {
                // Find TreasureItem type
                Type treasureItemType = null;
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    treasureItemType = assembly.GetType("Endless.Gameplay.TreasureItem");
                    if (treasureItemType != null) break;
                }

                if (treasureItemType == null)
                {
                    Log.LogWarning("[COLLECT-RESEARCH] TreasureItem type NOT FOUND");
                    return;
                }

                Log.LogInfo($"[COLLECT-RESEARCH] TreasureItem found: {treasureItemType.FullName}");
                Log.LogInfo($"[COLLECT-RESEARCH] Base type: {treasureItemType.BaseType?.FullName}");

                // Dump all fields
                var allFields = treasureItemType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                Log.LogInfo($"[COLLECT-RESEARCH] TreasureItem Fields ({allFields.Length}):");
                foreach (var field in allFields)
                {
                    var attrs = field.GetCustomAttributes(true);
                    string attrStr = attrs.Length > 0 ? $" [{string.Join(",", attrs.Select(a => a.GetType().Name))}]" : "";
                    Log.LogInfo($"[COLLECT-RESEARCH]   {field.Name} ({field.FieldType.Name}){attrStr}");
                }

                // Dump all properties
                var allProps = treasureItemType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                Log.LogInfo($"[COLLECT-RESEARCH] TreasureItem Properties ({allProps.Length}):");
                foreach (var prop in allProps)
                {
                    Log.LogInfo($"[COLLECT-RESEARCH]   {prop.Name} ({prop.PropertyType.Name}) get={prop.CanRead} set={prop.CanWrite}");
                }

                // Check base class (Item) fields too
                Type itemBase = treasureItemType.BaseType;
                if (itemBase != null && itemBase.Name == "Item")
                {
                    Log.LogInfo($"[COLLECT-RESEARCH] --- Base Item class fields ---");
                    var baseFields = itemBase.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    foreach (var field in baseFields)
                    {
                        var attrs = field.GetCustomAttributes(true);
                        string attrStr = attrs.Length > 0 ? $" [{string.Join(",", attrs.Select(a => a.GetType().Name))}]" : "";
                        Log.LogInfo($"[COLLECT-RESEARCH]   (base) {field.Name} ({field.FieldType.Name}){attrStr}");
                    }
                }

                // Find an actual TreasureItem instance in the scene
                var treasureInstances = UnityEngine.Object.FindObjectsOfType(treasureItemType);
                Log.LogInfo($"[COLLECT-RESEARCH] Found {treasureInstances.Length} TreasureItem instances in scene");

                if (treasureInstances.Length > 0)
                {
                    var treasure = treasureInstances[0];
                    var treasureMono = treasure as MonoBehaviour;
                    Log.LogInfo($"[COLLECT-RESEARCH] --- Dumping first TreasureItem instance: {treasureMono?.name} ---");

                    // Dump actual values
                    foreach (var field in allFields)
                    {
                        try
                        {
                            object val = field.GetValue(treasure);
                            string valStr = val?.ToString() ?? "null";
                            if (val is UnityEngine.Object unityObj)
                                valStr = $"{unityObj.name} ({unityObj.GetType().Name})";
                            Log.LogInfo($"[COLLECT-RESEARCH]   {field.Name} = {valStr}");
                        }
                        catch { }
                    }

                    // Dump base class values too
                    if (itemBase != null)
                    {
                        var baseFields = itemBase.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                        foreach (var field in baseFields)
                        {
                            try
                            {
                                object val = field.GetValue(treasure);
                                string valStr = val?.ToString() ?? "null";
                                if (val is UnityEngine.Object unityObj)
                                    valStr = $"{unityObj.name} ({unityObj.GetType().Name})";
                                Log.LogInfo($"[COLLECT-RESEARCH]   (base) {field.Name} = {valStr}");
                            }
                            catch { }
                        }
                    }

                    // Check GameObject layer and colliders
                    if (treasureMono != null)
                    {
                        Log.LogInfo($"[COLLECT-RESEARCH] GameObject layer: {treasureMono.gameObject.layer} ({LayerMask.LayerToName(treasureMono.gameObject.layer)})");
                        Log.LogInfo($"[COLLECT-RESEARCH] GameObject tag: {treasureMono.gameObject.tag}");

                        var colliders = treasureMono.GetComponents<Collider>();
                        Log.LogInfo($"[COLLECT-RESEARCH] Colliders on object: {colliders.Length}");
                        foreach (var col in colliders)
                        {
                            Log.LogInfo($"[COLLECT-RESEARCH]   Collider: {col.GetType().Name}, isTrigger={col.isTrigger}, enabled={col.enabled}");
                        }

                        // Check children for colliders too
                        var childColliders = treasureMono.GetComponentsInChildren<Collider>();
                        if (childColliders.Length > colliders.Length)
                        {
                            Log.LogInfo($"[COLLECT-RESEARCH] Child colliders: {childColliders.Length - colliders.Length}");
                            foreach (var col in childColliders)
                            {
                                if (!colliders.Contains(col))
                                {
                                    Log.LogInfo($"[COLLECT-RESEARCH]   Child {col.gameObject.name}: {col.GetType().Name}, layer={col.gameObject.layer}, isTrigger={col.isTrigger}");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.LogError($"[COLLECT-RESEARCH] TreasureItem research error: {ex.Message}");
            }
        }

        /// <summary>
        /// v10.25.0: Find where InventoryUsableDefinitions are stored/registered
        /// </summary>
        private static void ResearchDefinitionRegistry()
        {
            Log.LogInfo("[COLLECT-RESEARCH] --- Definition Registry Search ---");

            try
            {
                // Look for common registry patterns
                string[] potentialRegistryTypes = new string[]
                {
                    "Endless.Gameplay.Inventory.InventoryUsableDefinitionList",
                    "Endless.Gameplay.Inventory.InventoryDefinitionRegistry",
                    "Endless.Gameplay.InventoryUsableDefinitionList",
                    "Endless.Gameplay.ItemDefinitionList",
                    "Endless.Gameplay.RuntimeDatabase"
                };

                foreach (var typeName in potentialRegistryTypes)
                {
                    Type registryType = null;
                    foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        registryType = assembly.GetType(typeName);
                        if (registryType != null) break;
                    }

                    if (registryType != null)
                    {
                        Log.LogInfo($"[COLLECT-RESEARCH] Found registry type: {registryType.FullName}");

                        // Check for Instance property (singleton pattern)
                        var instanceProp = registryType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
                        if (instanceProp != null)
                        {
                            var instance = instanceProp.GetValue(null);
                            Log.LogInfo($"[COLLECT-RESEARCH]   Has Instance singleton: {instance != null}");

                            if (instance != null)
                            {
                                // Dump registry contents
                                DumpRegistryContents(instance, registryType);
                            }
                        }

                        // Check for static instance field
                        var instanceField = registryType.GetField("Instance", BindingFlags.Public | BindingFlags.Static);
                        if (instanceField != null)
                        {
                            var instance = instanceField.GetValue(null);
                            Log.LogInfo($"[COLLECT-RESEARCH]   Has Instance field: {instance != null}");

                            if (instance != null)
                            {
                                DumpRegistryContents(instance, registryType);
                            }
                        }

                        // Look for list/dictionary fields that might contain definitions
                        var fields = registryType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                        foreach (var field in fields)
                        {
                            if (field.FieldType.Name.Contains("List") || field.FieldType.Name.Contains("Dictionary") || field.FieldType.Name.Contains("Array"))
                            {
                                Log.LogInfo($"[COLLECT-RESEARCH]   Collection field: {field.Name} ({field.FieldType.Name})");
                            }
                        }
                    }
                }

                // Also search for any ScriptableObject with "Definition" in name in scene
                var allScriptableObjects = Resources.FindObjectsOfTypeAll<ScriptableObject>();
                var definitionObjects = allScriptableObjects.Where(so => so.name.Contains("Definition") || so.GetType().Name.Contains("Definition")).ToList();

                Log.LogInfo($"[COLLECT-RESEARCH] Found {definitionObjects.Count} Definition ScriptableObjects:");
                foreach (var def in definitionObjects.Take(20)) // Limit to first 20
                {
                    Log.LogInfo($"[COLLECT-RESEARCH]   {def.name} ({def.GetType().Name})");
                }
                if (definitionObjects.Count > 20)
                {
                    Log.LogInfo($"[COLLECT-RESEARCH]   ... and {definitionObjects.Count - 20} more");
                }

                // Find InventoryUsableDefinition base type
                Type invDefType = null;
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    invDefType = assembly.GetType("Endless.Gameplay.Inventory.InventoryUsableDefinition");
                    if (invDefType != null) break;
                }

                if (invDefType != null)
                {
                    Log.LogInfo($"[COLLECT-RESEARCH] InventoryUsableDefinition type: {invDefType.FullName}");

                    // Find all instances
                    var allDefs = Resources.FindObjectsOfTypeAll(invDefType);
                    Log.LogInfo($"[COLLECT-RESEARCH] Found {allDefs.Length} InventoryUsableDefinition instances:");
                    foreach (var def in allDefs)
                    {
                        var defObj = def as ScriptableObject;
                        if (defObj != null)
                        {
                            // Get GUID
                            var guidProp = invDefType.GetProperty("Guid");
                            string guid = guidProp?.GetValue(def)?.ToString() ?? "unknown";
                            Log.LogInfo($"[COLLECT-RESEARCH]   {defObj.name} (GUID: {guid})");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.LogError($"[COLLECT-RESEARCH] Registry research error: {ex.Message}");
            }
        }

        /// <summary>
        /// Helper to dump registry contents
        /// </summary>
        private static void DumpRegistryContents(object instance, Type registryType)
        {
            try
            {
                var fields = registryType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (var field in fields)
                {
                    var val = field.GetValue(instance);
                    if (val is IList list)
                    {
                        Log.LogInfo($"[COLLECT-RESEARCH]     {field.Name}: List with {list.Count} items");
                        int i = 0;
                        foreach (var item in list)
                        {
                            if (i >= 10) { Log.LogInfo($"[COLLECT-RESEARCH]       ... and {list.Count - 10} more"); break; }
                            string itemName = (item as UnityEngine.Object)?.name ?? item?.ToString() ?? "null";
                            Log.LogInfo($"[COLLECT-RESEARCH]       [{i}] {itemName}");
                            i++;
                        }
                    }
                    else if (val is IDictionary dict)
                    {
                        Log.LogInfo($"[COLLECT-RESEARCH]     {field.Name}: Dictionary with {dict.Count} entries");
                    }
                }
            }
            catch { }
        }

        /// <summary>
        /// v10.25.0: Research ItemInteractable structure
        /// </summary>
        private static void ResearchItemInteractableStructure()
        {
            Log.LogInfo("[COLLECT-RESEARCH] --- ItemInteractable Structure ---");

            try
            {
                Type interactableType = null;
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    interactableType = assembly.GetType("Endless.Gameplay.ItemInteractable");
                    if (interactableType != null) break;
                }

                if (interactableType == null)
                {
                    Log.LogWarning("[COLLECT-RESEARCH] ItemInteractable type NOT FOUND");
                    return;
                }

                Log.LogInfo($"[COLLECT-RESEARCH] ItemInteractable: {interactableType.FullName}");
                Log.LogInfo($"[COLLECT-RESEARCH] Base type: {interactableType.BaseType?.FullName}");

                // Dump fields
                var fields = interactableType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                Log.LogInfo($"[COLLECT-RESEARCH] Fields ({fields.Length}):");
                foreach (var field in fields)
                {
                    var attrs = field.GetCustomAttributes(true);
                    string attrStr = attrs.Length > 0 ? $" [{string.Join(",", attrs.Select(a => a.GetType().Name))}]" : "";
                    Log.LogInfo($"[COLLECT-RESEARCH]   {field.Name} ({field.FieldType.Name}){attrStr}");
                }

                // Find instances in scene
                var instances = UnityEngine.Object.FindObjectsOfType(interactableType);
                Log.LogInfo($"[COLLECT-RESEARCH] Found {instances.Length} ItemInteractable instances");

                if (instances.Length > 0)
                {
                    var interactable = instances[0];
                    var mono = interactable as MonoBehaviour;
                    Log.LogInfo($"[COLLECT-RESEARCH] First instance: {mono?.name}");

                    // Dump values
                    foreach (var field in fields)
                    {
                        try
                        {
                            object val = field.GetValue(interactable);
                            string valStr = val?.ToString() ?? "null";
                            if (val is UnityEngine.Object unityObj)
                                valStr = $"{unityObj.name} ({unityObj.GetType().Name})";
                            Log.LogInfo($"[COLLECT-RESEARCH]   {field.Name} = {valStr}");
                        }
                        catch { }
                    }

                    // Check base class InteractableBase fields
                    if (interactableType.BaseType != null)
                    {
                        var baseFields = interactableType.BaseType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                        Log.LogInfo($"[COLLECT-RESEARCH] Base class fields:");
                        foreach (var field in baseFields)
                        {
                            try
                            {
                                object val = field.GetValue(interactable);
                                string valStr = val?.ToString() ?? "null";
                                if (val is UnityEngine.Object unityObj)
                                    valStr = $"{unityObj.name} ({unityObj.GetType().Name})";
                                Log.LogInfo($"[COLLECT-RESEARCH]   (base) {field.Name} = {valStr}");
                            }
                            catch { }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.LogError($"[COLLECT-RESEARCH] ItemInteractable research error: {ex.Message}");
            }
        }

        /// <summary>
        /// v10.25.0: Research interaction layer system
        /// </summary>
        private static void ResearchInteractionLayers()
        {
            Log.LogInfo("[COLLECT-RESEARCH] --- Interaction Layers ---");

            try
            {
                // Log all layers
                Log.LogInfo("[COLLECT-RESEARCH] All Unity Layers:");
                for (int i = 0; i < 32; i++)
                {
                    string layerName = LayerMask.LayerToName(i);
                    if (!string.IsNullOrEmpty(layerName))
                    {
                        Log.LogInfo($"[COLLECT-RESEARCH]   Layer {i}: {layerName}");
                    }
                }

                // Look for PlayerInteractor to find what layer it looks for
                Type playerInteractorType = null;
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    playerInteractorType = assembly.GetType("Endless.Gameplay.PlayerInteractor");
                    if (playerInteractorType != null) break;
                }

                if (playerInteractorType != null)
                {
                    Log.LogInfo($"[COLLECT-RESEARCH] PlayerInteractor found: {playerInteractorType.FullName}");

                    // Look for layer mask fields
                    var fields = playerInteractorType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                    foreach (var field in fields)
                    {
                        if (field.FieldType == typeof(LayerMask) || field.Name.ToLower().Contains("layer") || field.Name.ToLower().Contains("mask"))
                        {
                            Log.LogInfo($"[COLLECT-RESEARCH]   Layer field: {field.Name} ({field.FieldType.Name})");
                        }
                    }

                    // Find instances
                    var instances = UnityEngine.Object.FindObjectsOfType(playerInteractorType);
                    if (instances.Length > 0)
                    {
                        var interactor = instances[0];
                        foreach (var field in fields)
                        {
                            if (field.FieldType == typeof(LayerMask))
                            {
                                try
                                {
                                    LayerMask mask = (LayerMask)field.GetValue(interactor);
                                    Log.LogInfo($"[COLLECT-RESEARCH]   {field.Name} = {mask.value} (binary: {Convert.ToString(mask.value, 2)})");
                                }
                                catch { }
                            }
                        }
                    }
                }

                // Also check InteractableBase for layer info
                Type interactableBaseType = null;
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    interactableBaseType = assembly.GetType("Endless.Gameplay.InteractableBase");
                    if (interactableBaseType != null) break;
                }

                if (interactableBaseType != null)
                {
                    var fields = interactableBaseType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                    foreach (var field in fields)
                    {
                        if (field.FieldType == typeof(LayerMask) || field.Name.ToLower().Contains("layer"))
                        {
                            Log.LogInfo($"[COLLECT-RESEARCH]   InteractableBase.{field.Name} ({field.FieldType.Name})");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.LogError($"[COLLECT-RESEARCH] Layer research error: {ex.Message}");
            }
        }

        private static void LogAvailableShaders()
        {
            try
            {
                var shaderClusterManagerType = AccessTools.TypeByName("Endless.Gameplay.VisualManagement.ShaderClusterManager");
                if (shaderClusterManagerType == null) return;

                var instanceProp = shaderClusterManagerType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
                var instance = instanceProp?.GetValue(null);
                if (instance == null) return;

                var listField = AccessTools.Field(shaderClusterManagerType, "shaderClusterList");
                var clusterList = listField?.GetValue(instance) as IList;
                if (clusterList == null) return;

                foreach (var cluster in clusterList)
                {
                    if (cluster == null) continue;
                    var displayId = cluster.GetType().GetField("DisplayId")?.GetValue(cluster) as string ?? "Unknown";
                    var primary = cluster.GetType().GetField("primaryShader")?.GetValue(cluster) as Shader;
                    Log.LogInfo($"[INJECTOR] Cluster {displayId}: {primary?.name ?? "null"}");
                }
            }
            catch (Exception ex)
            {
                Log.LogError($"[INJECTOR] LogAvailableShaders failed: {ex}");
            }
        }

        void OnDestroy()
        {
            _harmony?.UnpatchSelf();
        }
    }

    /// <summary>
    /// v10.39.0: Marker component to identify our custom props.
    /// Added to the EndlessProp during ComponentInitialize.
    /// Used by TrackNonNetworkedObject_Postfix to identify placed props.
    /// </summary>
    public class CustomPropMarker : MonoBehaviour { }

    /// <summary>
    /// v10.40.0: DEPRECATED - No longer used as of v10.42.0.
    /// Was marking the wrong object (Interactable instead of testPrefab).
    /// testPrefab is now identified by checking for renderers that aren't runtime visuals.
    /// </summary>
    public class TestPrefabMarker : MonoBehaviour { }
}

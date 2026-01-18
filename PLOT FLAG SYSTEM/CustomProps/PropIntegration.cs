using HarmonyLib;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace CustomProps
{
    /// <summary>
    /// Full integration with Endstar's native prop system.
    /// Hooks into InjectedProps and PropLibrary to add unlimited custom props.
    ///
    /// FIXED VERSION: Removed patches that were breaking the prop tool UI.
    /// Changes from original:
    /// - Removed PatchPropUIComponents entirely (was patching UI methods)
    /// - Removed heavy logging in GetAllRuntimePropsPostfix
    /// - Removed immediate injection in ScheduleInjection (now delayed)
    /// - Removed RefreshPropUI call (let game handle UI refresh naturally)
    /// - Added proper delayed injection via coroutine
    /// </summary>
    public static class PropIntegration
    {
        // Cached types from game assemblies
        private static Type _injectedPropsType;
        private static Type _propLibraryType;
        private static Type _runtimePropInfoType;
        private static Type _endlessPropType;
        private static Type _propType;
        private static Assembly _gameplayAsm;

        // Cached method info for InjectProp
        private static MethodInfo _injectPropMethod;

        // Our custom props registered with the game
        private static List<object> _registeredProps = new List<object>();
        private static bool _initialized = false;
        private static bool _propsInjected = false;

        /// <summary>
        /// Initialize by finding game types via reflection
        /// </summary>
        public static void Initialize()
        {
            if (_initialized) return;

            CustomPropsPlugin.Log?.LogInfo("=== PropIntegration Initializing ===");

            try
            {
                // Find game assemblies
                Assembly propsAsm = null;

                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    string name = asm.GetName().Name;
                    if (name == "Gameplay") _gameplayAsm = asm;
                    if (name == "Props") propsAsm = asm;
                }

                if (_gameplayAsm == null)
                {
                    CustomPropsPlugin.Log?.LogError("Gameplay assembly not found!");
                    return;
                }

                CustomPropsPlugin.Log?.LogInfo($"Found Gameplay assembly: {_gameplayAsm.FullName}");

                // Find key types
                _injectedPropsType = _gameplayAsm.GetType("Endless.Gameplay.LevelEditing.Level.InjectedProps");
                _propLibraryType = _gameplayAsm.GetType("Endless.Gameplay.LevelEditing.PropLibrary");

                // RuntimePropInfo is nested in PropLibrary
                _runtimePropInfoType = _gameplayAsm.GetType("Endless.Gameplay.LevelEditing.PropLibrary+RuntimePropInfo");

                // EndlessProp is in Scripting namespace
                _endlessPropType = _gameplayAsm.GetType("Endless.Gameplay.Scripting.EndlessProp");

                // Prop type is likely in Props.dll - search all assemblies
                _propType = FindPropType();

                // Log what we found (minimal logging)
                CustomPropsPlugin.Log?.LogInfo($"InjectedProps: {(_injectedPropsType != null ? "Found" : "Not found")}");
                CustomPropsPlugin.Log?.LogInfo($"PropLibrary: {(_propLibraryType != null ? "Found" : "Not found")}");
                CustomPropsPlugin.Log?.LogInfo($"RuntimePropInfo: {(_runtimePropInfoType != null ? "Found" : "Not found")}");
                CustomPropsPlugin.Log?.LogInfo($"EndlessProp: {(_endlessPropType != null ? "Found" : "Not found")}");
                CustomPropsPlugin.Log?.LogInfo($"Prop: {(_propType != null ? "Found" : "Not found")}");

                // Cache the InjectProp method
                if (_propLibraryType != null)
                {
                    _injectPropMethod = _propLibraryType.GetMethod("InjectProp",
                        BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
                    if (_injectPropMethod != null)
                    {
                        CustomPropsPlugin.Log?.LogInfo($"Found InjectProp method");
                    }
                }

                _initialized = true;
                CustomPropsPlugin.Log?.LogInfo("=== PropIntegration Initialized ===");
            }
            catch (Exception ex)
            {
                CustomPropsPlugin.Log?.LogError($"PropIntegration init failed: {ex}");
            }
        }

        private static Type FindPropType()
        {
            // Search all assemblies for a type named "Prop" that looks like prop data
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                string asmName = asm.GetName().Name;
                // Skip system assemblies
                if (asmName.StartsWith("System") || asmName.StartsWith("Unity") ||
                    asmName.StartsWith("mscorlib") || asmName.StartsWith("Mono"))
                    continue;

                try
                {
                    foreach (var type in asm.GetTypes())
                    {
                        // Look for "Prop" class that is a ScriptableObject or has prop-related fields
                        if (type.Name == "Prop" && !type.IsInterface && !type.IsAbstract)
                        {
                            return type;
                        }
                    }
                }
                catch { }
            }

            return null;
        }

        /// <summary>
        /// Register a custom prop with the game's prop system
        /// </summary>
        public static bool RegisterProp(PropData propData)
        {
            if (!_initialized)
            {
                Initialize();
            }

            if (_propLibraryType == null)
            {
                CustomPropsPlugin.Log?.LogError("Cannot register prop - PropLibrary type not found");
                return false;
            }

            try
            {
                CustomPropsPlugin.Log?.LogInfo($"Registering prop with game: {propData.PropId}");
                _registeredProps.Add(propData);
                return true;
            }
            catch (Exception ex)
            {
                CustomPropsPlugin.Log?.LogError($"Failed to register prop {propData.PropId}: {ex}");
                return false;
            }
        }

        /// <summary>
        /// Get all registered custom props
        /// </summary>
        public static IEnumerable<PropData> GetRegisteredProps()
        {
            foreach (var p in _registeredProps)
            {
                if (p is PropData pd)
                    yield return pd;
            }
        }

        public static Type InjectedPropsType => _injectedPropsType;
        public static Type PropLibraryType => _propLibraryType;
        public static Type RuntimePropInfoType => _runtimePropInfoType;
        public static Type EndlessPropType => _endlessPropType;
        public static Type PropType => _propType;
        public static Assembly GameplayAssembly => _gameplayAsm;
    }

    /// <summary>
    /// Harmony patches for prop system integration.
    /// FIXED: Removed UI patches that were breaking prop tool.
    /// </summary>
    [HarmonyPatch]
    public static class PropSystemPatches
    {
        private static bool _propsInjected = false;
        private static int _searchAttempts = 0;
        private static object _capturedPropLibrary = null;
        private static object _capturedStageManager = null;
        private static bool _patchesApplied = false;
        private static Type _stageManagerType = null;
        private static bool _injectionScheduled = false;

        // Delayed injection timing
        private static float _injectionDelaySeconds = 2.0f;
        private static float _lastCaptureTime = 0f;

        public static object CapturedPropLibrary => _capturedPropLibrary;
        public static object CapturedStageManager => _capturedStageManager;

        /// <summary>
        /// Apply Harmony patches to PropLibrary methods.
        /// FIXED: No longer patches UI components.
        /// </summary>
        public static void ApplyPatches(Harmony harmony)
        {
            if (_patchesApplied) return;

            try
            {
                // Find StageManager type
                _stageManagerType = PropIntegration.GameplayAssembly?.GetType("Endless.Gameplay.LevelEditing.Level.StageManager");
                if (_stageManagerType != null)
                {
                    CustomPropsPlugin.Log?.LogInfo($"Found StageManager type");
                }

                var propLibType = PropIntegration.PropLibraryType;
                if (propLibType == null)
                {
                    CustomPropsPlugin.Log?.LogWarning("Cannot apply patches - PropLibrary type not found");
                    return;
                }

                // Patch GetAllRuntimeProps - called when UI displays props
                var getAllMethod = propLibType.GetMethod("GetAllRuntimeProps",
                    BindingFlags.Public | BindingFlags.Instance);
                if (getAllMethod != null)
                {
                    var prefix = typeof(PropSystemPatches).GetMethod(nameof(GetAllRuntimePropsPrefix),
                        BindingFlags.Static | BindingFlags.NonPublic);
                    // FIXED: Removed heavy postfix - only use lightweight prefix for capture
                    harmony.Patch(getAllMethod, new HarmonyMethod(prefix));
                    CustomPropsPlugin.Log?.LogInfo("Patched PropLibrary.GetAllRuntimeProps (prefix only)");
                }

                // Patch InjectProp to log when game uses it
                var injectMethod = propLibType.GetMethod("InjectProp",
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
                if (injectMethod != null)
                {
                    var postfix = typeof(PropSystemPatches).GetMethod(nameof(InjectPropPostfix),
                        BindingFlags.Static | BindingFlags.NonPublic);
                    harmony.Patch(injectMethod, null, new HarmonyMethod(postfix));
                    CustomPropsPlugin.Log?.LogInfo("Patched PropLibrary.InjectProp (postfix for logging)");
                }

                // FIXED: Removed PatchPropLibraryReference - was causing issues
                // FIXED: Removed PatchPropUIComponents - was breaking prop tool UI

                _patchesApplied = true;
                CustomPropsPlugin.Log?.LogInfo("=== PropLibrary Harmony patches applied (FIXED version) ===");
            }
            catch (Exception ex)
            {
                CustomPropsPlugin.Log?.LogError($"Failed to apply PropLibrary patches: {ex}");
            }
        }

        /// <summary>
        /// Lightweight prefix for GetAllRuntimeProps - only captures instance
        /// FIXED: Removed heavy logging and immediate injection
        /// </summary>
        private static void GetAllRuntimePropsPrefix(object __instance)
        {
            if (__instance == null) return;

            if (_capturedPropLibrary == null)
            {
                _capturedPropLibrary = __instance;
                _lastCaptureTime = Time.time;
                CustomPropsPlugin.Log?.LogInfo($"Captured PropLibrary instance");

                // FIXED: Don't inject immediately - schedule for later
                ScheduleDelayedInjection();
            }
        }

        /// <summary>
        /// Postfix for InjectProp - only logs result
        /// </summary>
        private static void InjectPropPostfix(object __instance)
        {
            if (_capturedPropLibrary == null && __instance != null)
            {
                _capturedPropLibrary = __instance;
                CustomPropsPlugin.Log?.LogInfo($"Captured PropLibrary via InjectProp");
            }
        }

        /// <summary>
        /// FIXED: Schedule delayed injection instead of immediate
        /// This prevents interfering with UI initialization
        /// </summary>
        private static void ScheduleDelayedInjection()
        {
            if (_injectionScheduled || _propsInjected) return;
            _injectionScheduled = true;

            CustomPropsPlugin.Log?.LogInfo($"Injection scheduled - will inject after {_injectionDelaySeconds}s delay");
        }

        /// <summary>
        /// Try to inject props - called from Update loop
        /// FIXED: Added delay to ensure UI is fully initialized
        /// </summary>
        public static void TryInjectProps()
        {
            if (_propsInjected) return;

            // FIXED: Wait for delay before injecting
            if (_injectionScheduled && _lastCaptureTime > 0)
            {
                float elapsed = Time.time - _lastCaptureTime;
                if (elapsed < _injectionDelaySeconds)
                {
                    // Still waiting for delay
                    return;
                }
            }

            _searchAttempts++;

            // Limit search attempts
            if (_searchAttempts > 10 && _capturedPropLibrary == null)
            {
                // Only search occasionally after initial attempts
                if (_searchAttempts % 30 != 0) return;
            }

            // Check if we have StageManager and PropLibrary
            if (_capturedStageManager != null && _capturedPropLibrary != null)
            {
                CustomPropsPlugin.Log?.LogInfo($"Performing delayed injection (search #{_searchAttempts})");
                InjectCustomPropsViaStageManager(_capturedStageManager);
                return;
            }

            // Only search for StageManager if we don't have PropLibrary yet
            if (_capturedPropLibrary == null)
            {
                SearchForPropLibrary();
            }
            else if (_capturedStageManager == null)
            {
                SearchForStageManager();
            }

            // If we have PropLibrary but no StageManager, inject directly
            if (_capturedPropLibrary != null && _capturedStageManager == null)
            {
                CustomPropsPlugin.Log?.LogInfo("Injecting directly into PropLibrary (no StageManager)");
                InjectCustomProps(_capturedPropLibrary);
            }
        }

        /// <summary>
        /// Search for StageManager singleton
        /// </summary>
        private static void SearchForStageManager()
        {
            if (_stageManagerType == null) return;

            try
            {
                // Try to get Instance property
                var instanceProp = _stageManagerType.GetProperty("Instance",
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
                if (instanceProp != null)
                {
                    var stageManager = instanceProp.GetValue(null);
                    if (stageManager != null)
                    {
                        _capturedStageManager = stageManager;
                        CustomPropsPlugin.Log?.LogInfo($"Found StageManager via Instance");

                        // Get activePropLibrary
                        var activePropLibField = _stageManagerType.GetField("activePropLibrary",
                            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                        if (activePropLibField != null)
                        {
                            var activePropLib = activePropLibField.GetValue(stageManager);
                            if (activePropLib != null)
                            {
                                _capturedPropLibrary = activePropLib;
                                CustomPropsPlugin.Log?.LogInfo($"Found activePropLibrary");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CustomPropsPlugin.Log?.LogWarning($"StageManager search failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Search for PropLibrary in scene
        /// </summary>
        private static void SearchForPropLibrary()
        {
            if (PropIntegration.PropLibraryType == null) return;

            // First check if a Stage exists (only inject when in an actual level)
            try
            {
                var stageType = PropIntegration.GameplayAssembly?.GetType("Endless.Gameplay.LevelEditing.Level.Stage");
                if (stageType != null)
                {
                    var stages = UnityEngine.Object.FindObjectsOfType(stageType);
                    if (stages == null || stages.Length == 0)
                    {
                        // Not in a level yet
                        return;
                    }
                }
            }
            catch { }

            // Search for StageManager first (preferred)
            SearchForStageManager();
        }

        /// <summary>
        /// Inject custom props using StageManager.InjectProp
        /// </summary>
        public static void InjectCustomPropsViaStageManager(object stageManager)
        {
            if (_propsInjected) return;
            if (stageManager == null) return;

            CustomPropsPlugin.Log?.LogInfo("=== Injecting custom props via StageManager ===");

            var injectMethod = stageManager.GetType().GetMethod("InjectProp",
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);

            if (injectMethod == null)
            {
                CustomPropsPlugin.Log?.LogError("StageManager.InjectProp method not found!");
                return;
            }

            _propsInjected = true;

            // Inject each registered prop
            foreach (var prop in PropRegistry.GetAll())
            {
                try
                {
                    CustomPropsPlugin.Log?.LogInfo($"  Injecting: {prop.PropId}");
                    InjectSinglePropViaStageManager(stageManager, injectMethod, prop);
                }
                catch (Exception ex)
                {
                    CustomPropsPlugin.Log?.LogError($"  Failed to inject {prop.PropId}: {ex.Message}");
                }
            }

            CustomPropsPlugin.Log?.LogInfo("=== StageManager prop injection complete ===");

            // FIXED: Do NOT call RefreshPropUI - let the game handle UI refresh naturally
        }

        private static void InjectSinglePropViaStageManager(object stageManager, MethodInfo injectMethod, PropData prop)
        {
            var parameters = injectMethod.GetParameters();

            object propInstance = CreatePropInstance(prop);
            if (propInstance == null)
            {
                CustomPropsPlugin.Log?.LogError($"    Failed to create Prop instance");
                return;
            }

            // StageManager.InjectProp signature: (Prop prop, GameObject testPrefab, Script testScript, Sprite icon)
            object[] args = new object[4];
            args[0] = propInstance;
            args[1] = prop.Prefab;
            args[2] = null;
            args[3] = prop.Icon;

            try
            {
                var result = injectMethod.Invoke(stageManager, args);
                CustomPropsPlugin.Log?.LogInfo($"    StageManager.InjectProp SUCCESS!");
            }
            catch (Exception ex)
            {
                CustomPropsPlugin.Log?.LogError($"    StageManager.InjectProp failed: {ex.Message}");
                if (ex.InnerException != null)
                {
                    CustomPropsPlugin.Log?.LogError($"    Inner: {ex.InnerException.Message}");
                }
            }
        }

        /// <summary>
        /// Inject custom props directly into PropLibrary
        /// </summary>
        public static void InjectCustomProps(object propLibrary)
        {
            if (_propsInjected) return;

            CustomPropsPlugin.Log?.LogInfo("=== Injecting custom props into PropLibrary ===");

            var injectMethod = propLibrary.GetType().GetMethod("InjectProp",
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);

            if (injectMethod == null)
            {
                CustomPropsPlugin.Log?.LogError("InjectProp method not found!");
                return;
            }

            _propsInjected = true;

            foreach (var prop in PropRegistry.GetAll())
            {
                try
                {
                    CustomPropsPlugin.Log?.LogInfo($"  Injecting: {prop.PropId}");
                    InjectSingleProp(propLibrary, injectMethod, prop);
                }
                catch (Exception ex)
                {
                    CustomPropsPlugin.Log?.LogError($"  Failed to inject {prop.PropId}: {ex.Message}");
                }
            }

            CustomPropsPlugin.Log?.LogInfo("=== Prop injection complete ===");
        }

        private static void InjectSingleProp(object propLibrary, MethodInfo injectMethod, PropData prop)
        {
            object propInstance = CreatePropInstance(prop);
            if (propInstance == null)
            {
                CustomPropsPlugin.Log?.LogError($"    Failed to create Prop instance");
                return;
            }

            // PropLibrary.InjectProp signature: (Prop, GameObject, Script, Sprite, Transform, EndlessProp)
            object[] args = new object[6];
            args[0] = propInstance;
            args[1] = prop.Prefab;
            args[2] = null;
            args[3] = prop.Icon;
            args[4] = null;
            args[5] = null;

            try
            {
                var result = injectMethod.Invoke(propLibrary, args);
                CustomPropsPlugin.Log?.LogInfo($"    InjectProp SUCCESS!");
            }
            catch (Exception ex)
            {
                CustomPropsPlugin.Log?.LogError($"    InjectProp failed: {ex.Message}");
            }
        }

        private static object CreatePropInstance(PropData prop)
        {
            if (PropIntegration.PropType == null)
            {
                CustomPropsPlugin.Log?.LogError("Prop type not found");
                return null;
            }

            try
            {
                bool isScriptableObject = typeof(ScriptableObject).IsAssignableFrom(PropIntegration.PropType);
                object propInstance = null;

                if (isScriptableObject)
                {
                    propInstance = ScriptableObject.CreateInstance(PropIntegration.PropType);
                }
                else
                {
                    var ctor = PropIntegration.PropType.GetConstructor(Type.EmptyTypes);
                    if (ctor != null)
                    {
                        propInstance = ctor.Invoke(null);
                    }
                }

                if (propInstance == null)
                {
                    CustomPropsPlugin.Log?.LogError("propInstance is null after creation");
                    return null;
                }

                // Set basic fields
                SetFieldDeep(propInstance, "Name", prop.DisplayName);
                SetFieldDeep(propInstance, "Description", prop.DisplayName);
                SetFieldDeep(propInstance, "baseTypeId", "treasure");
                SetFieldDeep(propInstance, "openSource", true);

                // Handle AssetID
                var assetIdField = FindField(propInstance.GetType(), "AssetID");
                if (assetIdField != null)
                {
                    if (assetIdField.FieldType.Name == "SerializableGuid")
                    {
                        var guidType = assetIdField.FieldType;
                        var guid = Guid.NewGuid();

                        var ctor = guidType.GetConstructor(new[] { typeof(Guid) });
                        if (ctor != null)
                        {
                            var serGuid = ctor.Invoke(new object[] { guid });
                            assetIdField.SetValue(propInstance, serGuid);
                        }
                    }
                    else
                    {
                        assetIdField.SetValue(propInstance, prop.PropId);
                    }
                }

                return propInstance;
            }
            catch (Exception ex)
            {
                CustomPropsPlugin.Log?.LogError($"Failed to create Prop instance: {ex.Message}");
                return null;
            }
        }

        private static void SetFieldDeep(object obj, string fieldName, object value)
        {
            var field = FindField(obj.GetType(), fieldName);
            if (field != null)
            {
                try
                {
                    field.SetValue(obj, value);
                }
                catch { }
            }
        }

        private static FieldInfo FindField(Type type, string fieldName)
        {
            var current = type;
            while (current != null)
            {
                var field = current.GetField(fieldName,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                if (field != null) return field;
                current = current.BaseType;
            }
            return null;
        }

        /// <summary>
        /// Reset injection state (for scene changes)
        /// </summary>
        public static void ResetInjectionState()
        {
            _propsInjected = false;
            _injectionScheduled = false;
            _lastCaptureTime = 0f;
            _searchAttempts = 0;
            // Keep captured instances - they may still be valid
        }

        /// <summary>
        /// Full reset - clears everything
        /// </summary>
        public static void FullReset()
        {
            _propsInjected = false;
            _injectionScheduled = false;
            _lastCaptureTime = 0f;
            _searchAttempts = 0;
            _capturedPropLibrary = null;
            _capturedStageManager = null;
        }

        public static bool PropLibraryFound => _capturedPropLibrary != null;
        public static bool PropsInjected => _propsInjected;
    }

    /// <summary>
    /// Runtime prop spawner that integrates with the game
    /// </summary>
    public static class PropSpawner
    {
        /// <summary>
        /// Spawn a custom prop that integrates with the game's pickup system
        /// </summary>
        public static GameObject SpawnIntegrated(string propId, Vector3 position, Quaternion rotation)
        {
            var propData = PropRegistry.Get(propId);
            if (propData == null || propData.Prefab == null)
            {
                CustomPropsPlugin.Log?.LogWarning($"Cannot spawn {propId} - not found or no prefab");
                return null;
            }

            var instance = UnityEngine.Object.Instantiate(propData.Prefab, position, rotation);
            instance.name = $"CustomProp_{propId}";

            // Try to add EndlessProp component
            if (PropIntegration.EndlessPropType != null)
            {
                try
                {
                    instance.AddComponent(PropIntegration.EndlessPropType);
                }
                catch { }
            }

            // Add pickup handler as fallback
            var pickup = instance.GetComponent<CustomPropPickup>();
            if (pickup == null)
            {
                pickup = instance.AddComponent<CustomPropPickup>();
            }
            pickup.Initialize(propData);

            // Ensure collider
            if (instance.GetComponent<Collider>() == null)
            {
                var box = instance.AddComponent<BoxCollider>();
                box.isTrigger = true;
            }

            CustomPropsPlugin.Log?.LogInfo($"Spawned prop: {propId} at {position}");
            return instance;
        }
    }
}

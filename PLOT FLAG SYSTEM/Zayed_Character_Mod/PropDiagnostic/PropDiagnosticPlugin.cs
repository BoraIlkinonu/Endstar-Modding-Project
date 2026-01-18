using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PropDiagnostic
{
    [BepInPlugin("com.endstar.propdiagnostic", "Prop Diagnostic", "1.0.0")]
    public class PropDiagnosticPlugin : BaseUnityPlugin
    {
        public static ManualLogSource Log;
        private static Harmony _harmony;

        // Cached types
        private static Type _stageManagerType;
        private static Type _propLibraryType;
        private static Type _runtimePropInfoType;
        private static Type _propToolType;
        private static Type _creatorManagerType;

        private void Awake()
        {
            Log = Logger;
            Log.LogInfo("=== Prop Diagnostic v1.0.0 ===");
            Log.LogInfo("This plugin logs game's prop system behavior");
            Log.LogInfo("Press F11 to dump existing prop info");
            Log.LogInfo("Press F12 to dump PropLibrary state");

            CacheTypes();
            ApplyPatches();

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void CacheTypes()
        {
            var gameplayAsm = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == "Gameplay");
            var creatorAsm = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == "Creator");

            if (gameplayAsm != null)
            {
                _stageManagerType = gameplayAsm.GetType("Endless.Gameplay.LevelEditing.Level.StageManager");
                _propLibraryType = gameplayAsm.GetType("Endless.Gameplay.LevelEditing.PropLibrary");
                _runtimePropInfoType = gameplayAsm.GetType("Endless.Gameplay.LevelEditing.PropLibrary+RuntimePropInfo");
                Log.LogInfo($"StageManager: {_stageManagerType != null}");
                Log.LogInfo($"PropLibrary: {_propLibraryType != null}");
                Log.LogInfo($"RuntimePropInfo: {_runtimePropInfoType != null}");
            }

            if (creatorAsm != null)
            {
                _propToolType = creatorAsm.GetType("Endless.Creator.LevelEditing.Runtime.PropTool");
                _creatorManagerType = creatorAsm.GetType("Endless.Creator.CreatorManager");
                Log.LogInfo($"PropTool: {_propToolType != null}");
                Log.LogInfo($"CreatorManager: {_creatorManagerType != null}");
            }
        }

        private void ApplyPatches()
        {
            try
            {
                _harmony = new Harmony("com.endstar.propdiagnostic");

                // Patch PropLibrary.InjectProp
                if (_propLibraryType != null)
                {
                    var injectMethod = _propLibraryType.GetMethod("InjectProp",
                        BindingFlags.Public | BindingFlags.Instance);
                    if (injectMethod != null)
                    {
                        var prefix = typeof(PropLibraryPatches).GetMethod("InjectProp_Prefix",
                            BindingFlags.Public | BindingFlags.Static);
                        _harmony.Patch(injectMethod, new HarmonyMethod(prefix));
                        Log.LogInfo("Patched PropLibrary.InjectProp");
                    }
                }

                // Patch PropTool.UpdateSelectedAssetId (when user selects a prop)
                if (_propToolType != null)
                {
                    var updateMethod = _propToolType.GetMethod("UpdateSelectedAssetId",
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (updateMethod != null)
                    {
                        var prefix = typeof(PropToolPatches).GetMethod("UpdateSelectedAssetId_Prefix",
                            BindingFlags.Public | BindingFlags.Static);
                        _harmony.Patch(updateMethod, new HarmonyMethod(prefix));
                        Log.LogInfo("Patched PropTool.UpdateSelectedAssetId");
                    }

                    // Patch PlaceProp
                    var placeMethod = _propToolType.GetMethod("PlaceProp",
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (placeMethod != null)
                    {
                        var prefix = typeof(PropToolPatches).GetMethod("PlaceProp_Prefix",
                            BindingFlags.Public | BindingFlags.Static);
                        _harmony.Patch(placeMethod, new HarmonyMethod(prefix));
                        Log.LogInfo("Patched PropTool.PlaceProp");
                    }
                }

                Log.LogInfo("Harmony patches applied");
            }
            catch (Exception ex)
            {
                Log.LogError($"Patch error: {ex.Message}");
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Log.LogInfo($"=== Scene Loaded: {scene.name} ===");

            // Create a proper GameObject with MonoBehaviour for coroutines
            var go = new GameObject("PropDiagnosticRunner");
            UnityEngine.Object.DontDestroyOnLoad(go);
            var runner = go.AddComponent<DiagnosticRunner>();
            runner.Init(this, scene.name);
        }

        private void HookTerrainAndPropsLoaded()
        {
            try
            {
                if (_stageManagerType == null) return;

                var instanceProp = _stageManagerType.GetProperty("Instance",
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
                var stageManager = instanceProp?.GetValue(null);

                if (stageManager != null)
                {
                    var eventField = _stageManagerType.GetField("TerrainAndPropsLoaded",
                        BindingFlags.Public | BindingFlags.Instance);
                    if (eventField != null)
                    {
                        var unityEvent = eventField.GetValue(stageManager);
                        var addListenerMethod = unityEvent.GetType().GetMethod("AddListener");

                        // Create delegate - simplified for logging
                        Log.LogInfo("Found TerrainAndPropsLoaded event");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.LogError($"HookTerrainAndPropsLoaded error: {ex.Message}");
            }
        }

        private void HookOnPropsRepopulated()
        {
            try
            {
                if (_creatorManagerType == null) return;

                var instanceProp = _creatorManagerType.GetProperty("Instance",
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
                var creatorManager = instanceProp?.GetValue(null);

                if (creatorManager != null)
                {
                    var eventField = _creatorManagerType.GetField("OnPropsRepopulated",
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (eventField != null)
                    {
                        Log.LogInfo("Found CreatorManager.OnPropsRepopulated event");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.LogError($"HookOnPropsRepopulated error: {ex.Message}");
            }
        }

        // Public wrappers for DiagnosticRunner
        public void DumpPropLibraryStatePublic() => DumpPropLibraryState();
        public void DumpExistingPropStructurePublic() => DumpExistingPropStructure();

        public int GetLoadedPropCount()
        {
            try
            {
                var stageManager = GetStageManagerInstance();
                if (stageManager == null) return -1;

                var propLibraryProp = _stageManagerType.GetProperty("ActivePropLibrary",
                    BindingFlags.Public | BindingFlags.Instance);
                var propLibrary = propLibraryProp?.GetValue(stageManager);
                if (propLibrary == null) return -1;

                var loadedPropMapField = _propLibraryType.GetField("loadedPropMap",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                var loadedPropMap = loadedPropMapField?.GetValue(propLibrary);
                if (loadedPropMap == null) return -1;

                var countProp = loadedPropMap.GetType().GetProperty("Count");
                return (int)countProp.GetValue(loadedPropMap);
            }
            catch
            {
                return -1;
            }
        }

        private void DumpExistingPropStructure()
        {
            try
            {
                var stageManager = GetStageManagerInstance();
                if (stageManager == null)
                {
                    Log.LogWarning("StageManager not available");
                    return;
                }

                var propLibraryField = _stageManagerType.GetField("activePropLibrary",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                var propLibrary = propLibraryField?.GetValue(stageManager);

                if (propLibrary == null)
                {
                    Log.LogWarning("PropLibrary not available");
                    return;
                }

                var loadedPropMapField = _propLibraryType.GetField("loadedPropMap",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                var loadedPropMap = loadedPropMapField?.GetValue(propLibrary);

                if (loadedPropMap == null)
                {
                    Log.LogWarning("loadedPropMap not available");
                    return;
                }

                // Get first entry
                var enumerator = ((IEnumerable)loadedPropMap).GetEnumerator();
                if (enumerator.MoveNext())
                {
                    var kvp = enumerator.Current;
                    var kvpType = kvp.GetType();
                    var keyProp = kvpType.GetProperty("Key");
                    var valueProp = kvpType.GetProperty("Value");

                    var assetRef = keyProp.GetValue(kvp);
                    var runtimePropInfo = valueProp.GetValue(kvp);

                    Log.LogInfo("=== EXISTING PROP STRUCTURE ===");

                    // Dump AssetReference (key)
                    Log.LogInfo("--- AssetReference (Key) ---");
                    DumpObjectFields(assetRef, "  ");

                    // Dump RuntimePropInfo (value)
                    Log.LogInfo("--- RuntimePropInfo (Value) ---");
                    DumpObjectFields(runtimePropInfo, "  ");

                    // Get PropData from RuntimePropInfo
                    var propDataField = runtimePropInfo.GetType().GetField("PropData");
                    var propData = propDataField?.GetValue(runtimePropInfo);
                    if (propData != null)
                    {
                        Log.LogInfo("--- PropData (Prop ScriptableObject) ---");
                        DumpObjectFields(propData, "  ");
                    }

                    // Get EndlessProp
                    var endlessPropField = runtimePropInfo.GetType().GetField("EndlessProp");
                    var endlessProp = endlessPropField?.GetValue(runtimePropInfo);
                    if (endlessProp != null)
                    {
                        Log.LogInfo("--- EndlessProp ---");
                        DumpObjectFields(endlessProp, "  ", maxDepth: 1);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.LogError($"DumpExistingPropStructure error: {ex.Message}");
                Log.LogError(ex.StackTrace);
            }
        }

        private void DumpPropLibraryState()
        {
            try
            {
                Log.LogInfo("=== PROP LIBRARY STATE ===");

                // Try multiple ways to get StageManager
                var stageManager = GetStageManagerInstance();
                Log.LogInfo($"StageManager via reflection: {(stageManager != null ? stageManager.ToString() : "NULL")}");

                // Also try FindObjectOfType
                if (_stageManagerType != null)
                {
                    var findMethod = typeof(UnityEngine.Object).GetMethod("FindObjectOfType", new Type[] { typeof(Type) });
                    var foundSM = findMethod?.Invoke(null, new object[] { _stageManagerType });
                    Log.LogInfo($"StageManager via FindObjectOfType: {(foundSM != null ? foundSM.ToString() : "NULL")}");
                    if (stageManager == null && foundSM != null)
                        stageManager = foundSM;
                }

                if (stageManager == null)
                {
                    Log.LogWarning("StageManager not available via any method");
                    return;
                }

                // Try PROPERTY first (ActivePropLibrary), then field
                var propLibraryProp = _stageManagerType.GetProperty("ActivePropLibrary",
                    BindingFlags.Public | BindingFlags.Instance);
                var propLibrary = propLibraryProp?.GetValue(stageManager);
                Log.LogInfo($"PropLibrary via ActivePropLibrary property: {(propLibrary != null ? propLibrary.ToString() : "NULL")}");

                if (propLibrary == null)
                {
                    var propLibraryField = _stageManagerType.GetField("activePropLibrary",
                        BindingFlags.NonPublic | BindingFlags.Instance);
                    propLibrary = propLibraryField?.GetValue(stageManager);
                    Log.LogInfo($"PropLibrary via activePropLibrary field: {(propLibrary != null ? propLibrary.ToString() : "NULL")}");
                }

                if (propLibrary == null)
                {
                    Log.LogWarning("PropLibrary not available via property or field");
                    return;
                }

                // Dump ALL fields of PropLibrary to see what's there
                Log.LogInfo("--- PropLibrary ALL FIELDS ---");
                var plFields = propLibrary.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (var f in plFields)
                {
                    try
                    {
                        var val = f.GetValue(propLibrary);
                        string valStr = "null";
                        if (val != null)
                        {
                            if (val is ICollection col)
                                valStr = $"[Collection: {col.Count} items]";
                            else if (val is UnityEngine.Object uobj)
                                valStr = $"{uobj.name} ({uobj.GetType().Name})";
                            else
                                valStr = val.ToString();
                        }
                        Log.LogInfo($"  {f.Name}: {valStr}");
                    }
                    catch (Exception ex)
                    {
                        Log.LogInfo($"  {f.Name}: <error: {ex.Message}>");
                    }
                }

                // loadedPropMap
                Log.LogInfo("--- loadedPropMap details ---");
                var loadedPropMapField = _propLibraryType.GetField("loadedPropMap",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                Log.LogInfo($"loadedPropMapField found: {loadedPropMapField != null}");
                var loadedPropMap = loadedPropMapField?.GetValue(propLibrary);
                Log.LogInfo($"loadedPropMap value: {(loadedPropMap != null ? loadedPropMap.GetType().FullName : "NULL")}");
                if (loadedPropMap != null)
                {
                    var countProp = loadedPropMap.GetType().GetProperty("Count");
                    var count = (int)countProp.GetValue(loadedPropMap);
                    Log.LogInfo($"loadedPropMap count: {count}");

                    // List first 5 props with FULL details
                    int i = 0;
                    foreach (var kvp in (IEnumerable)loadedPropMap)
                    {
                        if (i >= 5) break;
                        var kvpType = kvp.GetType();
                        var keyProp = kvpType.GetProperty("Key");
                        var valueProp = kvpType.GetProperty("Value");

                        var assetRef = keyProp.GetValue(kvp);
                        var rpi = valueProp.GetValue(kvp);

                        // Get prop name
                        var propDataField = rpi.GetType().GetField("PropData");
                        var propData = propDataField?.GetValue(rpi);
                        string propName = "?";
                        if (propData != null)
                        {
                            var nameField = propData.GetType().GetField("Name",
                                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                            propName = nameField?.GetValue(propData)?.ToString() ?? "?";
                        }

                        Log.LogInfo($"  [{i}] {propName}");

                        // Dump AssetReference key (first prop only)
                        if (i == 0 && assetRef != null)
                        {
                            Log.LogInfo($"      --- AssetReference (Key) ---");
                            var assetRefFields = assetRef.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                            foreach (var f in assetRefFields)
                            {
                                var val = f.GetValue(assetRef);
                                Log.LogInfo($"      {f.Name}: {val}");
                            }

                            // Dump Prop fields
                            if (propData != null)
                            {
                                Log.LogInfo($"      --- Prop (PropData) ---");
                                var propFields = propData.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                                foreach (var f in propFields.Take(10))
                                {
                                    try
                                    {
                                        var val = f.GetValue(propData);
                                        string valStr = val?.ToString() ?? "null";
                                        if (val is UnityEngine.Object uobj)
                                            valStr = $"{uobj.name} ({uobj.GetType().Name})";
                                        Log.LogInfo($"      {f.Name}: {valStr}");
                                    }
                                    catch { }
                                }
                            }
                        }
                        i++;
                    }
                }

                // Check injectedProps on StageManager
                var injectedPropsField = _stageManagerType.GetField("injectedProps",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                var injectedProps = injectedPropsField?.GetValue(stageManager);
                if (injectedProps != null)
                {
                    var countProp = injectedProps.GetType().GetProperty("Count");
                    var count = (int)countProp.GetValue(injectedProps);
                    Log.LogInfo($"StageManager.injectedProps count: {count}");
                }

                // basePropPrefab
                var basePropField = _stageManagerType.GetField("basePropPrefab",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                var baseProp = basePropField?.GetValue(stageManager);
                Log.LogInfo($"basePropPrefab: {(baseProp != null ? ((UnityEngine.Object)baseProp).name : "null")}");

            }
            catch (Exception ex)
            {
                Log.LogError($"DumpPropLibraryState error: {ex.Message}");
            }
        }

        private object GetStageManagerInstance()
        {
            if (_stageManagerType == null) return null;
            var instanceProp = _stageManagerType.GetProperty("Instance",
                BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            return instanceProp?.GetValue(null);
        }

        private void DumpObjectFields(object obj, string indent, int maxDepth = 2, int currentDepth = 0)
        {
            if (obj == null || currentDepth > maxDepth) return;

            var type = obj.GetType();
            Log.LogInfo($"{indent}Type: {type.FullName}");

            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var field in fields.Take(15))
            {
                try
                {
                    var value = field.GetValue(obj);
                    string valueStr;

                    if (value == null)
                        valueStr = "null";
                    else if (value is string s)
                        valueStr = $"\"{s}\"";
                    else if (value is UnityEngine.Object uObj)
                        valueStr = $"{uObj.name} ({uObj.GetType().Name})";
                    else if (value.GetType().IsValueType || value.GetType() == typeof(string))
                        valueStr = value.ToString();
                    else
                        valueStr = $"[{value.GetType().Name}]";

                    Log.LogInfo($"{indent}  {field.Name}: {valueStr}");
                }
                catch
                {
                    Log.LogInfo($"{indent}  {field.Name}: <error reading>");
                }
            }
        }
    }

    // Harmony patches for logging
    public static class PropLibraryPatches
    {
        public static void InjectProp_Prefix(object __instance, object prop, GameObject testPrefab,
            object testScript, Sprite icon, Transform prefabSpawnTransform, object propPrefab)
        {
            PropDiagnosticPlugin.Log.LogInfo("=== PropLibrary.InjectProp CALLED ===");
            PropDiagnosticPlugin.Log.LogInfo($"  prop: {prop}");
            PropDiagnosticPlugin.Log.LogInfo($"  testPrefab: {(testPrefab != null ? testPrefab.name : "null")}");
            PropDiagnosticPlugin.Log.LogInfo($"  testScript: {testScript}");
            PropDiagnosticPlugin.Log.LogInfo($"  icon: {(icon != null ? icon.name : "null")}");
            PropDiagnosticPlugin.Log.LogInfo($"  prefabSpawnTransform: {(prefabSpawnTransform != null ? prefabSpawnTransform.name : "null")}");
            PropDiagnosticPlugin.Log.LogInfo($"  propPrefab: {propPrefab}");
        }
    }

    public static class PropToolPatches
    {
        public static void UpdateSelectedAssetId_Prefix(object __instance, object selectedAssetId)
        {
            PropDiagnosticPlugin.Log.LogInfo("=== PropTool.UpdateSelectedAssetId CALLED ===");
            PropDiagnosticPlugin.Log.LogInfo($"  selectedAssetId: {selectedAssetId}");
        }

        public static void PlaceProp_Prefix(object assetId, ulong networkObjectId,
            Vector3 position, Vector3 eulerRotation, object instanceId)
        {
            PropDiagnosticPlugin.Log.LogInfo("=== PropTool.PlaceProp CALLED ===");
            PropDiagnosticPlugin.Log.LogInfo($"  assetId: {assetId}");
            PropDiagnosticPlugin.Log.LogInfo($"  networkObjectId: {networkObjectId}");
            PropDiagnosticPlugin.Log.LogInfo($"  position: {position}");
            PropDiagnosticPlugin.Log.LogInfo($"  eulerRotation: {eulerRotation}");
            PropDiagnosticPlugin.Log.LogInfo($"  instanceId: {instanceId}");
        }
    }

    // Actual MonoBehaviour that runs on main thread
    public class DiagnosticRunner : MonoBehaviour
    {
        private PropDiagnosticPlugin _plugin;
        private string _sceneName;
        private float _timer = 0f;
        private float _lastDumpTime = 0f;
        private int _dumpCount = 0;
        private int _lastPropCount = -1;

        public void Init(PropDiagnosticPlugin plugin, string sceneName)
        {
            _plugin = plugin;
            _sceneName = sceneName;
            PropDiagnosticPlugin.Log.LogInfo($"[DiagnosticRunner] Created for scene: {sceneName}");
            PropDiagnosticPlugin.Log.LogInfo($"[DiagnosticRunner] Will dump every 10s until props are found, then on F12");
        }

        private void Update()
        {
            _timer += Time.deltaTime;

            // Check F12 for manual dump
            if (Input.GetKeyDown(KeyCode.F12))
            {
                PropDiagnosticPlugin.Log.LogInfo($"[DiagnosticRunner] F12 pressed - manual dump");
                DoDump();
            }

            // Auto-dump every 10 seconds until we find props
            if (_timer - _lastDumpTime >= 10f)
            {
                _lastDumpTime = _timer;
                _dumpCount++;

                // Get current prop count
                int currentCount = _plugin.GetLoadedPropCount();

                // Only dump if count changed or first few dumps
                if (currentCount != _lastPropCount || _dumpCount <= 3)
                {
                    PropDiagnosticPlugin.Log.LogInfo($"[DiagnosticRunner] Auto-dump #{_dumpCount} at {_timer:F0}s (propCount: {_lastPropCount} -> {currentCount})");
                    DoDump();
                    _lastPropCount = currentCount;
                }
                else
                {
                    PropDiagnosticPlugin.Log.LogInfo($"[DiagnosticRunner] Skipping dump #{_dumpCount} - count unchanged ({currentCount})");
                }
            }
        }

        private void DoDump()
        {
            _plugin.DumpPropLibraryStatePublic();
        }
    }
}

using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CustomProps.Research
{
    /// <summary>
    /// PROP SYSTEM RESEARCH PLUGIN
    ///
    /// Purpose: Discover the exact data flow from PropLibrary to UI display
    ///
    /// Hotkeys:
    ///   F5 - Dump loadedPropMap contents
    ///   F6 - Dump _referenceFilterMap contents
    ///   F7 - Dump UIRuntimePropInfoListModel.List contents
    ///   F8 - Force trigger Synchronize and log results
    ///   F4 - Toggle verbose logging
    ///
    /// All findings are logged to BepInEx console and log file.
    /// </summary>
    [BepInPlugin(GUID, NAME, VERSION)]
    public class PropSystemResearch : BaseUnityPlugin
    {
        public const string GUID = "com.endstar.research.propsystem";
        public const string NAME = "Prop System Research";
        public const string VERSION = "1.0.0";

        public static ManualLogSource Log { get; private set; }
        public static bool VerboseLogging = true;

        private Harmony _harmony;
        private static ResearchHelper _helper;

        // Cached reflection info
        private static Assembly _gameplayAsm;
        private static Assembly _creatorAsm;
        private static Type _propLibraryType;
        private static Type _runtimePropInfoType;
        private static Type _stageManagerType;
        private static Type _creatorManagerType;
        private static Type _listModelType;
        private static Type _panelViewType;

        private void Awake()
        {
            Log = Logger;
            Log.LogInfo($"=== {NAME} v{VERSION} ===");
            Log.LogInfo("Purpose: Discover prop system data flow");
            Log.LogInfo("Hotkeys: F4=Toggle Verbose, F5=PropMap, F6=FilterMap, F7=UIList, F8=ForceSyncˇ");

            CacheReflectionInfo();
            ApplyPatches();
            CreateHelper();
        }

        private void CacheReflectionInfo()
        {
            Log.LogInfo("Caching reflection info...");

            _gameplayAsm = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == "Gameplay");
            _creatorAsm = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == "Creator");

            if (_gameplayAsm != null)
            {
                _propLibraryType = _gameplayAsm.GetType("Endless.Gameplay.LevelEditing.PropLibrary");
                _stageManagerType = _gameplayAsm.GetType("Endless.Gameplay.LevelEditing.Level.StageManager");

                if (_propLibraryType != null)
                {
                    _runtimePropInfoType = _propLibraryType.GetNestedType("RuntimePropInfo",
                        BindingFlags.Public | BindingFlags.NonPublic);
                }

                Log.LogInfo($"  PropLibrary: {_propLibraryType != null}");
                Log.LogInfo($"  RuntimePropInfo: {_runtimePropInfoType != null}");
                Log.LogInfo($"  StageManager: {_stageManagerType != null}");
            }

            if (_creatorAsm != null)
            {
                _creatorManagerType = _creatorAsm.GetType("Endless.Creator.CreatorManager");
                _listModelType = _creatorAsm.GetTypes()
                    .FirstOrDefault(t => t.Name == "UIRuntimePropInfoListModel");
                _panelViewType = _creatorAsm.GetTypes()
                    .FirstOrDefault(t => t.Name == "UIPropToolPanelView");

                Log.LogInfo($"  CreatorManager: {_creatorManagerType != null}");
                Log.LogInfo($"  UIRuntimePropInfoListModel: {_listModelType != null}");
                Log.LogInfo($"  UIPropToolPanelView: {_panelViewType != null}");
            }
        }

        private void ApplyPatches()
        {
            _harmony = new Harmony(GUID);

            try
            {
                // Patch 1: Log when OnPropsRepopulated is invoked
                if (_creatorManagerType != null)
                {
                    var onPropsField = _creatorManagerType.GetField("OnPropsRepopulated",
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                    if (onPropsField != null)
                    {
                        Log.LogInfo($"Found OnPropsRepopulated field: {onPropsField.FieldType.Name}");
                    }
                }

                // Patch 2: PropLibrary.GetReferenceFilteredDefinitionList
                if (_propLibraryType != null)
                {
                    var getFilteredMethod = _propLibraryType.GetMethod("GetReferenceFilteredDefinitionList",
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (getFilteredMethod != null)
                    {
                        _harmony.Patch(getFilteredMethod,
                            prefix: new HarmonyMethod(typeof(PropSystemResearch), nameof(GetFilteredPrefix)),
                            postfix: new HarmonyMethod(typeof(PropSystemResearch), nameof(GetFilteredPostfix)));
                        Log.LogInfo("Patched: GetReferenceFilteredDefinitionList");
                    }
                }

                // Patch 3: PropLibrary.PopulateReferenceFilterMap
                if (_propLibraryType != null)
                {
                    var populateMethod = _propLibraryType.GetMethod("PopulateReferenceFilterMap",
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (populateMethod != null)
                    {
                        _harmony.Patch(populateMethod,
                            prefix: new HarmonyMethod(typeof(PropSystemResearch), nameof(PopulateFilterMapPrefix)),
                            postfix: new HarmonyMethod(typeof(PropSystemResearch), nameof(PopulateFilterMapPostfix)));
                        Log.LogInfo("Patched: PopulateReferenceFilterMap");
                    }
                }

                // Patch 4: UIRuntimePropInfoListModel.Synchronize
                if (_listModelType != null)
                {
                    var syncMethods = _listModelType.GetMethods(
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                        .Where(m => m.Name == "Synchronize").ToList();

                    foreach (var syncMethod in syncMethods)
                    {
                        _harmony.Patch(syncMethod,
                            prefix: new HarmonyMethod(typeof(PropSystemResearch), nameof(SynchronizePrefix)),
                            postfix: new HarmonyMethod(typeof(PropSystemResearch), nameof(SynchronizePostfix)));
                        Log.LogInfo($"Patched: Synchronize ({syncMethod.GetParameters().Length} params)");
                    }
                }

                // Patch 5: PropLibrary.GetAllRuntimeProps
                if (_propLibraryType != null)
                {
                    var getAllMethod = _propLibraryType.GetMethod("GetAllRuntimeProps",
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (getAllMethod != null)
                    {
                        _harmony.Patch(getAllMethod,
                            postfix: new HarmonyMethod(typeof(PropSystemResearch), nameof(GetAllRuntimePropsPostfix)));
                        Log.LogInfo("Patched: GetAllRuntimeProps");
                    }
                }

                Log.LogInfo("All patches applied successfully");
            }
            catch (Exception ex)
            {
                Log.LogError($"Patching failed: {ex.Message}");
                Log.LogError(ex.StackTrace);
            }
        }

        private void CreateHelper()
        {
            var helperGO = new GameObject("PropSystemResearchHelper");
            UnityEngine.Object.DontDestroyOnLoad(helperGO);
            _helper = helperGO.AddComponent<ResearchHelper>();
        }

        // =====================================================================
        // HARMONY PATCHES - Timing & Data Flow Logging
        // =====================================================================

        static void GetFilteredPrefix(object __instance, object referenceFilter)
        {
            if (VerboseLogging)
                Log.LogInfo($"[TIMING] GetReferenceFilteredDefinitionList CALLED with filter={referenceFilter}");
        }

        static void GetFilteredPostfix(object __instance, object __result, object referenceFilter)
        {
            try
            {
                var resultType = __result?.GetType();
                if (resultType != null)
                {
                    var countProp = resultType.GetProperty("Count");
                    var count = countProp?.GetValue(__result) ?? "?";
                    Log.LogInfo($"[TIMING] GetReferenceFilteredDefinitionList RETURNED {count} props for filter={referenceFilter}");
                }
            }
            catch (Exception ex)
            {
                Log.LogError($"GetFilteredPostfix error: {ex.Message}");
            }
        }

        static void PopulateFilterMapPrefix(object __instance)
        {
            Log.LogInfo("[TIMING] PopulateReferenceFilterMap CALLED");
        }

        static void PopulateFilterMapPostfix(object __instance)
        {
            Log.LogInfo("[TIMING] PopulateReferenceFilterMap COMPLETED");

            // Dump the filter map contents
            try
            {
                var filterMapField = _propLibraryType.GetField("_referenceFilterMap",
                    BindingFlags.NonPublic | BindingFlags.Instance);

                if (filterMapField != null)
                {
                    var filterMap = filterMapField.GetValue(__instance);
                    if (filterMap != null)
                    {
                        DumpFilterMap(filterMap);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.LogError($"PopulateFilterMapPostfix dump error: {ex.Message}");
            }
        }

        static void SynchronizePrefix(object __instance, object[] __args)
        {
            var argsStr = string.Join(", ", __args?.Select(a => a?.ToString() ?? "null") ?? new[] { "no args" });
            Log.LogInfo($"[TIMING] UIRuntimePropInfoListModel.Synchronize CALLED with: {argsStr}");
        }

        static void SynchronizePostfix(object __instance)
        {
            Log.LogInfo("[TIMING] UIRuntimePropInfoListModel.Synchronize COMPLETED");

            // Try to dump the resulting list
            try
            {
                var listField = _listModelType.GetField("_list",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                if (listField == null)
                {
                    listField = _listModelType.GetField("list",
                        BindingFlags.NonPublic | BindingFlags.Instance);
                }
                if (listField == null)
                {
                    // Try property
                    var listProp = _listModelType.GetProperty("List",
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (listProp != null)
                    {
                        var list = listProp.GetValue(__instance);
                        DumpUIList(list);
                    }
                }
                else
                {
                    var list = listField.GetValue(__instance);
                    DumpUIList(list);
                }
            }
            catch (Exception ex)
            {
                Log.LogError($"SynchronizePostfix dump error: {ex.Message}");
            }
        }

        static void GetAllRuntimePropsPostfix(object __result)
        {
            if (VerboseLogging && __result != null)
            {
                var arr = __result as Array;
                Log.LogInfo($"[TIMING] GetAllRuntimeProps returned {arr?.Length ?? 0} props");
            }
        }

        // =====================================================================
        // DUMP METHODS - State Inspection
        // =====================================================================

        public static void DumpLoadedPropMap()
        {
            Log.LogInfo("=== DUMPING loadedPropMap ===");

            try
            {
                var propLibrary = GetActivePropLibrary();
                if (propLibrary == null)
                {
                    Log.LogWarning("PropLibrary not found");
                    return;
                }

                var mapField = _propLibraryType.GetField("loadedPropMap",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                if (mapField == null)
                {
                    Log.LogWarning("loadedPropMap field not found");
                    return;
                }

                var map = mapField.GetValue(propLibrary);
                if (map == null)
                {
                    Log.LogWarning("loadedPropMap is null");
                    return;
                }

                var mapType = map.GetType();
                var countProp = mapType.GetProperty("Count");
                var count = (int)(countProp?.GetValue(map) ?? 0);
                Log.LogInfo($"loadedPropMap contains {count} entries");

                // Get Values
                var valuesProp = mapType.GetProperty("Values");
                var values = valuesProp?.GetValue(map) as IEnumerable;

                if (values != null)
                {
                    int i = 0;
                    foreach (var rpi in values)
                    {
                        if (i >= 10)
                        {
                            Log.LogInfo($"  ... and {count - 10} more");
                            break;
                        }

                        var propDataField = rpi.GetType().GetField("PropData");
                        var propData = propDataField?.GetValue(rpi);

                        string name = "?";
                        string id = "?";

                        if (propData != null)
                        {
                            var nameField = propData.GetType().GetProperty("Name") ??
                                           propData.GetType().GetField("Name") as MemberInfo;
                            var idField = propData.GetType().GetProperty("AssetID") ??
                                         propData.GetType().GetField("AssetID") as MemberInfo;

                            if (nameField is PropertyInfo pi) name = pi.GetValue(propData)?.ToString() ?? "?";
                            else if (nameField is FieldInfo fi) name = fi.GetValue(propData)?.ToString() ?? "?";

                            if (idField is PropertyInfo pi2) id = pi2.GetValue(propData)?.ToString() ?? "?";
                            else if (idField is FieldInfo fi2) id = fi2.GetValue(propData)?.ToString() ?? "?";
                        }

                        Log.LogInfo($"  [{i}] {name} (ID: {id})");
                        i++;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.LogError($"DumpLoadedPropMap error: {ex.Message}");
                Log.LogError(ex.StackTrace);
            }
        }

        public static void DumpReferenceFilterMap()
        {
            Log.LogInfo("=== DUMPING _referenceFilterMap ===");

            try
            {
                var propLibrary = GetActivePropLibrary();
                if (propLibrary == null)
                {
                    Log.LogWarning("PropLibrary not found");
                    return;
                }

                var filterMapField = _propLibraryType.GetField("_referenceFilterMap",
                    BindingFlags.NonPublic | BindingFlags.Instance);

                if (filterMapField == null)
                {
                    Log.LogWarning("_referenceFilterMap field not found");
                    return;
                }

                var filterMap = filterMapField.GetValue(propLibrary);
                DumpFilterMap(filterMap);
            }
            catch (Exception ex)
            {
                Log.LogError($"DumpReferenceFilterMap error: {ex.Message}");
            }
        }

        private static void DumpFilterMap(object filterMap)
        {
            if (filterMap == null)
            {
                Log.LogWarning("_referenceFilterMap is null");
                return;
            }

            var mapType = filterMap.GetType();
            var countProp = mapType.GetProperty("Count");
            var count = (int)(countProp?.GetValue(filterMap) ?? 0);
            Log.LogInfo($"_referenceFilterMap has {count} filter categories");

            // Iterate through keys
            var keysProp = mapType.GetProperty("Keys");
            var keys = keysProp?.GetValue(filterMap) as IEnumerable;

            if (keys != null)
            {
                foreach (var key in keys)
                {
                    // Get the list for this key
                    var indexer = mapType.GetProperty("Item");
                    var list = indexer?.GetValue(filterMap, new[] { key });

                    int listCount = 0;
                    if (list != null)
                    {
                        var listCountProp = list.GetType().GetProperty("Count");
                        listCount = (int)(listCountProp?.GetValue(list) ?? 0);
                    }

                    Log.LogInfo($"  Filter {key} ({(int)key}): {listCount} props");
                }
            }
        }

        public static void DumpUIListModel()
        {
            Log.LogInfo("=== DUMPING UIRuntimePropInfoListModel ===");

            try
            {
                // Find all instances of UIRuntimePropInfoListModel
                var instances = UnityEngine.Object.FindObjectsOfType(_listModelType as Type ?? typeof(MonoBehaviour));
                Log.LogInfo($"Found {instances.Length} UIRuntimePropInfoListModel instances");

                // Also try to find via UIPropToolPanelView
                if (_panelViewType != null)
                {
                    var panelViews = UnityEngine.Object.FindObjectsOfType(_panelViewType as Type ?? typeof(MonoBehaviour));
                    Log.LogInfo($"Found {panelViews.Length} UIPropToolPanelView instances");

                    foreach (var panel in panelViews)
                    {
                        var listModelField = _panelViewType.GetField("runtimePropInfoListModel",
                            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                        if (listModelField != null)
                        {
                            var listModel = listModelField.GetValue(panel);
                            if (listModel != null)
                            {
                                Log.LogInfo("Found listModel via UIPropToolPanelView");
                                DumpUIListFromModel(listModel);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.LogError($"DumpUIListModel error: {ex.Message}");
            }
        }

        private static void DumpUIListFromModel(object listModel)
        {
            try
            {
                // Try various field/property names
                var listField = listModel.GetType().GetField("_list",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                if (listField == null)
                    listField = listModel.GetType().GetField("list",
                        BindingFlags.NonPublic | BindingFlags.Instance);

                object list = null;
                if (listField != null)
                {
                    list = listField.GetValue(listModel);
                }
                else
                {
                    var listProp = listModel.GetType().GetProperty("List",
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (listProp != null)
                        list = listProp.GetValue(listModel);
                }

                DumpUIList(list);
            }
            catch (Exception ex)
            {
                Log.LogError($"DumpUIListFromModel error: {ex.Message}");
            }
        }

        private static void DumpUIList(object list)
        {
            if (list == null)
            {
                Log.LogWarning("UI List is null");
                return;
            }

            var listType = list.GetType();
            var countProp = listType.GetProperty("Count");
            var count = (int)(countProp?.GetValue(list) ?? 0);
            Log.LogInfo($"UI List contains {count} props");

            if (count > 0)
            {
                var enumerable = list as IEnumerable;
                int i = 0;
                foreach (var item in enumerable)
                {
                    if (i >= 10)
                    {
                        Log.LogInfo($"  ... and {count - 10} more");
                        break;
                    }

                    var propDataField = item.GetType().GetField("PropData");
                    var propData = propDataField?.GetValue(item);

                    string name = "?";
                    if (propData != null)
                    {
                        var nameMember = propData.GetType().GetProperty("Name") ??
                                        propData.GetType().GetField("Name") as MemberInfo;
                        if (nameMember is PropertyInfo pi) name = pi.GetValue(propData)?.ToString() ?? "?";
                        else if (nameMember is FieldInfo fi) name = fi.GetValue(propData)?.ToString() ?? "?";
                    }

                    Log.LogInfo($"  [{i}] {name}");
                    i++;
                }
            }
        }

        public static void ForceSynchronize()
        {
            Log.LogInfo("=== FORCING SYNCHRONIZE ===");

            try
            {
                // Find UIPropToolPanelView and call its refresh method
                if (_panelViewType != null)
                {
                    var panelViews = UnityEngine.Object.FindObjectsOfType(_panelViewType as Type ?? typeof(MonoBehaviour));

                    foreach (var panel in panelViews)
                    {
                        // Try to find and call OnLibraryRepopulated
                        var refreshMethod = _panelViewType.GetMethod("OnLibraryRepopulated",
                            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                        if (refreshMethod != null)
                        {
                            Log.LogInfo("Calling OnLibraryRepopulated...");
                            refreshMethod.Invoke(panel, null);
                            Log.LogInfo("OnLibraryRepopulated called successfully");
                        }
                        else
                        {
                            Log.LogWarning("OnLibraryRepopulated method not found");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.LogError($"ForceSynchronize error: {ex.Message}");
            }
        }

        private static object GetActivePropLibrary()
        {
            try
            {
                if (_stageManagerType == null) return null;

                var instanceProp = _stageManagerType.GetProperty("Instance",
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
                var stageManager = instanceProp?.GetValue(null);

                if (stageManager == null) return null;

                var propLibField = _stageManagerType.GetField("activePropLibrary",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                return propLibField?.GetValue(stageManager);
            }
            catch
            {
                return null;
            }
        }

        private void OnDestroy()
        {
            _harmony?.UnpatchSelf();
        }
    }

    /// <summary>
    /// Helper MonoBehaviour for hotkey handling
    /// </summary>
    public class ResearchHelper : MonoBehaviour
    {
        void Update()
        {
            // F4 - Toggle verbose logging
            if (Input.GetKeyDown(KeyCode.F4))
            {
                PropSystemResearch.VerboseLogging = !PropSystemResearch.VerboseLogging;
                PropSystemResearch.Log.LogInfo($"Verbose logging: {PropSystemResearch.VerboseLogging}");
            }

            // F5 - Dump loadedPropMap
            if (Input.GetKeyDown(KeyCode.F5))
            {
                PropSystemResearch.Log.LogInfo("F5 pressed");
                PropSystemResearch.DumpLoadedPropMap();
            }

            // F6 - Dump _referenceFilterMap
            if (Input.GetKeyDown(KeyCode.F6))
            {
                PropSystemResearch.Log.LogInfo("F6 pressed");
                PropSystemResearch.DumpReferenceFilterMap();
            }

            // F7 - Dump UI list model
            if (Input.GetKeyDown(KeyCode.F7))
            {
                PropSystemResearch.Log.LogInfo("F7 pressed");
                PropSystemResearch.DumpUIListModel();
            }

            // F8 - Force synchronize
            if (Input.GetKeyDown(KeyCode.F8))
            {
                PropSystemResearch.Log.LogInfo("F8 pressed");
                PropSystemResearch.ForceSynchronize();
            }
        }
    }
}

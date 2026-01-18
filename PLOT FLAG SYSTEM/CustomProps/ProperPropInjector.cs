using HarmonyLib;
using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;

namespace CustomProps
{
    /// <summary>
    /// V4 - OPTION B: Direct UI List manipulation via Harmony postfix.
    ///
    /// Key insight from research:
    /// - UIPropToolPanelView.OnLibraryRepopulated() calls Synchronize()
    /// - Synchronize populates UIRuntimePropInfoListModel.List from _referenceFilterMap
    /// - We postfix OnLibraryRepopulated to add our prop AFTER Synchronize completes
    /// - This bypasses all filter requirements - guaranteed UI visibility!
    /// </summary>
    public static class ProperPropInjector
    {
        // Option B: Cached UI types for direct list manipulation
        private static Type _panelViewType;
        private static Type _listModelType;
        private static FieldInfo _listModelField;
        private static MethodInfo _listAddMethod;
        private static bool _optionBInitialized = false;

        // Cached types
        private static Type _stageManagerType;
        private static Type _propLibraryType;
        private static Type _propType;
        private static Type _runtimePropInfoType;
        private static Type _creatorManagerType;
        private static Type _assetReferenceType;
        private static Type _referenceFilterType;

        // State
        private static bool _initialized = false;
        private static bool _subscribedToEvents = false;
        private static bool _injectionComplete = false;
        private static string _validBaseTypeId = null;
        private static object _pendingRuntimePropInfo = null;
        private static bool _uiRefreshPending = false;

        // Cached property/field accessors
        private static PropertyInfo _stageManagerInstanceProp;
        private static FieldInfo _activePropLibraryField;
        private static PropertyInfo _activePropLibraryProp;  // DLL: get_ActivePropLibrary()
        private static PropertyInfo _creatorManagerInstanceProp;
        private static FieldInfo _onPropsRepopulatedField;
        private static FieldInfo _loadedPropMapField;
        private static FieldInfo _referenceFilterMapField;
        private static MethodInfo _toAssetReferenceMethod;

        public static void Initialize()
        {
            if (_initialized) return;

            CustomPropsPlugin.Log?.LogInfo("=== ProperPropInjector V3 Initializing ===");

            try
            {
                var gameplayAsm = AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(a => a.GetName().Name == "Gameplay");
                var propsAsm = AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(a => a.GetName().Name == "Props");
                var creatorAsm = AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(a => a.GetName().Name == "Creator");
                var assetsAsm = AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(a => a.GetName().Name == "Assets");

                if (gameplayAsm == null || propsAsm == null)
                {
                    CustomPropsPlugin.Log?.LogError("Required assemblies not found!");
                    return;
                }

                // Get types
                _stageManagerType = gameplayAsm.GetType("Endless.Gameplay.LevelEditing.Level.StageManager");
                _propLibraryType = gameplayAsm.GetType("Endless.Gameplay.LevelEditing.PropLibrary");
                _runtimePropInfoType = gameplayAsm.GetType("Endless.Gameplay.LevelEditing.PropLibrary+RuntimePropInfo");
                _propType = propsAsm.GetType("Endless.Props.Assets.Prop");

                if (creatorAsm != null)
                {
                    _creatorManagerType = creatorAsm.GetType("Endless.Creator.CreatorManager");
                }

                if (assetsAsm != null)
                {
                    _assetReferenceType = assetsAsm.GetType("Endless.Assets.AssetReference");
                }

                // Get ReferenceFilter enum type
                _referenceFilterType = gameplayAsm.GetType("Endless.Gameplay.ReferenceFilter");

                // Get accessors
                _stageManagerInstanceProp = _stageManagerType?.GetProperty("Instance",
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

                _activePropLibraryField = _stageManagerType?.GetField("activePropLibrary",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                // DLL: get_ActivePropLibrary() - public property getter
                _activePropLibraryProp = _stageManagerType?.GetProperty("ActivePropLibrary",
                    BindingFlags.Public | BindingFlags.Instance);

                _creatorManagerInstanceProp = _creatorManagerType?.GetProperty("Instance",
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

                _onPropsRepopulatedField = _creatorManagerType?.GetField("OnPropsRepopulated",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                _loadedPropMapField = _propLibraryType?.GetField("loadedPropMap",
                    BindingFlags.NonPublic | BindingFlags.Instance);

                _referenceFilterMapField = _propLibraryType?.GetField("_referenceFilterMap",
                    BindingFlags.NonPublic | BindingFlags.Instance);

                _toAssetReferenceMethod = _propType?.GetMethod("ToAssetReference",
                    BindingFlags.Public | BindingFlags.Instance);

                CustomPropsPlugin.Log?.LogInfo($"Types found:");
                CustomPropsPlugin.Log?.LogInfo($"  StageManager: {_stageManagerType != null}");
                CustomPropsPlugin.Log?.LogInfo($"  PropLibrary: {_propLibraryType != null}");
                CustomPropsPlugin.Log?.LogInfo($"  CreatorManager: {_creatorManagerType != null}");
                CustomPropsPlugin.Log?.LogInfo($"  RuntimePropInfo: {_runtimePropInfoType != null}");
                CustomPropsPlugin.Log?.LogInfo($"  AssetReference: {_assetReferenceType != null}");
                CustomPropsPlugin.Log?.LogInfo($"  ReferenceFilter: {_referenceFilterType != null}");

                CustomPropsPlugin.Log?.LogInfo($"Accessors found:");
                CustomPropsPlugin.Log?.LogInfo($"  StageManager.Instance: {_stageManagerInstanceProp != null}");
                CustomPropsPlugin.Log?.LogInfo($"  activePropLibrary (FIELD): {_activePropLibraryField != null}");
                CustomPropsPlugin.Log?.LogInfo($"  ActivePropLibrary (PROPERTY): {_activePropLibraryProp != null}");
                CustomPropsPlugin.Log?.LogInfo($"  CreatorManager.Instance: {_creatorManagerInstanceProp != null}");
                CustomPropsPlugin.Log?.LogInfo($"  OnPropsRepopulated: {_onPropsRepopulatedField != null}");
                CustomPropsPlugin.Log?.LogInfo($"  loadedPropMap: {_loadedPropMapField != null}");
                CustomPropsPlugin.Log?.LogInfo($"  _referenceFilterMap: {_referenceFilterMapField != null}");
                CustomPropsPlugin.Log?.LogInfo($"  ToAssetReference: {_toAssetReferenceMethod != null}");

                _initialized = true;
                CustomPropsPlugin.Log?.LogInfo("=== ProperPropInjector V4 Initialized ===");

                // Initialize Option B (UI direct add)
                InitializeOptionB();
            }
            catch (Exception ex)
            {
                CustomPropsPlugin.Log?.LogError($"Init failed: {ex.Message}");
                CustomPropsPlugin.Log?.LogError(ex.StackTrace);
            }
        }

        /// <summary>
        /// OPTION B: Initialize UI types for direct list manipulation.
        /// This caches all reflection info needed to add props directly to UI.
        /// </summary>
        private static void InitializeOptionB()
        {
            if (_optionBInitialized) return;

            try
            {
                CustomPropsPlugin.Log?.LogInfo("[OPTION B] Initializing UI types...");

                var creatorAsm = AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(a => a.GetName().Name == "Creator");

                if (creatorAsm == null)
                {
                    CustomPropsPlugin.Log?.LogWarning("[OPTION B] Creator assembly not found");
                    return;
                }

                // Get UIPropToolPanelView type
                _panelViewType = creatorAsm.GetType("Endless.Creator.UI.UIPropToolPanelView");
                if (_panelViewType == null)
                {
                    CustomPropsPlugin.Log?.LogWarning("[OPTION B] UIPropToolPanelView type not found");
                    return;
                }

                // Get runtimePropInfoListModel field
                _listModelField = _panelViewType.GetField("runtimePropInfoListModel",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (_listModelField == null)
                {
                    CustomPropsPlugin.Log?.LogWarning("[OPTION B] runtimePropInfoListModel field not found");
                    return;
                }

                _listModelType = _listModelField.FieldType;

                // Get Add(RuntimePropInfo, bool) method
                _listAddMethod = _listModelType.GetMethod("Add",
                    BindingFlags.Public | BindingFlags.Instance,
                    null,
                    new Type[] { _runtimePropInfoType, typeof(bool) },
                    null);

                if (_listAddMethod == null)
                {
                    CustomPropsPlugin.Log?.LogWarning("[OPTION B] Add method not found, will use fallback");
                }

                _optionBInitialized = true;
                CustomPropsPlugin.Log?.LogInfo("[OPTION B] UI types initialized successfully!");
                CustomPropsPlugin.Log?.LogInfo($"  PanelView: {_panelViewType.Name}");
                CustomPropsPlugin.Log?.LogInfo($"  ListModel: {_listModelType.Name}");
                CustomPropsPlugin.Log?.LogInfo($"  Add method: {(_listAddMethod != null ? "found" : "fallback mode")}");
            }
            catch (Exception ex)
            {
                CustomPropsPlugin.Log?.LogError($"[OPTION B] Init failed: {ex.Message}");
            }
        }

        /// <summary>
        /// OPTION B: Apply Harmony postfixes to multiple entry points.
        /// - OnLibraryRepopulated: When props are repopulated
        /// - OnEnable/Display: When panel becomes visible
        /// </summary>
        public static void ApplyOptionBPatch(Harmony harmony)
        {
            if (!_optionBInitialized)
            {
                CustomPropsPlugin.Log?.LogWarning("[OPTION B] Not initialized, skipping patch");
                return;
            }

            try
            {
                // Patch 1: OnLibraryRepopulated (when props are repopulated)
                var onLibraryRepopulatedMethod = _panelViewType.GetMethod("OnLibraryRepopulated",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                if (onLibraryRepopulatedMethod != null)
                {
                    var postfix = new HarmonyMethod(typeof(ProperPropInjector), nameof(OnLibraryRepopulated_Postfix));
                    harmony.Patch(onLibraryRepopulatedMethod, postfix: postfix);
                    CustomPropsPlugin.Log?.LogInfo("[OPTION B] Patched OnLibraryRepopulated with postfix!");
                }
                else
                {
                    CustomPropsPlugin.Log?.LogWarning("[OPTION B] OnLibraryRepopulated method not found");
                }

                // Patch 2: OnEnable (when panel becomes active)
                var onEnableMethod = _panelViewType.GetMethod("OnEnable",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                if (onEnableMethod != null)
                {
                    var postfix = new HarmonyMethod(typeof(ProperPropInjector), nameof(OnPanelEnable_Postfix));
                    harmony.Patch(onEnableMethod, postfix: postfix);
                    CustomPropsPlugin.Log?.LogInfo("[OPTION B] Patched OnEnable with postfix!");
                }

                // Patch 3: Display method (if exists)
                var displayMethod = _panelViewType.GetMethod("Display",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                if (displayMethod != null)
                {
                    var postfix = new HarmonyMethod(typeof(ProperPropInjector), nameof(OnPanelDisplay_Postfix));
                    harmony.Patch(displayMethod, postfix: postfix);
                    CustomPropsPlugin.Log?.LogInfo("[OPTION B] Patched Display with postfix!");
                }

                // Patch 4: Synchronize on the list model (catches ALL synchronizations)
                var syncMethod = _listModelType.GetMethod("Synchronize",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                if (syncMethod != null)
                {
                    var postfix = new HarmonyMethod(typeof(ProperPropInjector), nameof(OnSynchronize_Postfix));
                    harmony.Patch(syncMethod, postfix: postfix);
                    CustomPropsPlugin.Log?.LogInfo("[OPTION B] Patched Synchronize with postfix!");
                }
                else
                {
                    CustomPropsPlugin.Log?.LogWarning("[OPTION B] Synchronize method not found");
                }

                CustomPropsPlugin.Log?.LogInfo("[OPTION B] All patches applied!");
            }
            catch (Exception ex)
            {
                CustomPropsPlugin.Log?.LogError($"[OPTION B] Patch failed: {ex.Message}");
                CustomPropsPlugin.Log?.LogError(ex.StackTrace);
            }
        }

        /// <summary>
        /// Postfix for Synchronize - runs AFTER the list is populated.
        /// This is the most reliable hook point.
        /// </summary>
        private static void OnSynchronize_Postfix(object __instance)
        {
            try
            {
                CustomPropsPlugin.Log?.LogInfo("[OPTION B] Synchronize_Postfix triggered!");
                AddCustomPropsToListModel(__instance);
            }
            catch (Exception ex)
            {
                CustomPropsPlugin.Log?.LogError($"[OPTION B] Synchronize_Postfix error: {ex.Message}");
            }
        }

        /// <summary>
        /// Postfix for OnEnable - runs when panel becomes active.
        /// </summary>
        private static void OnPanelEnable_Postfix(object __instance)
        {
            try
            {
                CustomPropsPlugin.Log?.LogInfo("[OPTION B] OnPanelEnable_Postfix triggered!");

                // Get list model from panel view
                var listModel = _listModelField?.GetValue(__instance);
                if (listModel != null)
                {
                    AddCustomPropsToListModel(listModel);
                }
            }
            catch (Exception ex)
            {
                CustomPropsPlugin.Log?.LogError($"[OPTION B] OnPanelEnable_Postfix error: {ex.Message}");
            }
        }

        /// <summary>
        /// Postfix for Display - runs when panel is displayed.
        /// </summary>
        private static void OnPanelDisplay_Postfix(object __instance)
        {
            try
            {
                CustomPropsPlugin.Log?.LogInfo("[OPTION B] OnPanelDisplay_Postfix triggered!");

                // Get list model from panel view
                var listModel = _listModelField?.GetValue(__instance);
                if (listModel != null)
                {
                    AddCustomPropsToListModel(listModel);
                }
            }
            catch (Exception ex)
            {
                CustomPropsPlugin.Log?.LogError($"[OPTION B] OnPanelDisplay_Postfix error: {ex.Message}");
            }
        }

        /// <summary>
        /// Shared logic to add custom props to a list model.
        /// </summary>
        private static void AddCustomPropsToListModel(object listModel)
        {
            if (listModel == null)
            {
                CustomPropsPlugin.Log?.LogWarning("[OPTION B] listModel is null");
                return;
            }

            // Get all custom props that need to be added
            var customProps = PropRegistry.GetAll().ToList();
            if (customProps.Count == 0)
            {
                CustomPropsPlugin.Log?.LogInfo("[OPTION B] No custom props to add");
                return;
            }

            CustomPropsPlugin.Log?.LogInfo($"[OPTION B] Adding {customProps.Count} custom props to UI list...");

            foreach (var propData in customProps)
            {
                try
                {
                    // Get or create RuntimePropInfo for this prop
                    var runtimePropInfo = GetOrCreateRuntimePropInfo(propData);
                    if (runtimePropInfo == null)
                    {
                        CustomPropsPlugin.Log?.LogWarning($"[OPTION B] Failed to get RuntimePropInfo for {propData.PropId}");
                        continue;
                    }

                    // Check if already in list (avoid duplicates)
                    if (IsAlreadyInUIList(listModel, propData.PropId))
                    {
                        CustomPropsPlugin.Log?.LogInfo($"[OPTION B] {propData.PropId} already in UI list, skipping");
                        continue;
                    }

                    // Add to UI list using Add(item, triggerEvents=true)
                    if (_listAddMethod != null)
                    {
                        _listAddMethod.Invoke(listModel, new object[] { runtimePropInfo, true });
                        CustomPropsPlugin.Log?.LogInfo($"[OPTION B] Added {propData.PropId} to UI list!");
                    }
                    else
                    {
                        // Fallback: add to List field directly
                        AddToListDirectly(listModel, runtimePropInfo);
                        CustomPropsPlugin.Log?.LogInfo($"[OPTION B] Added {propData.PropId} via fallback");
                    }
                }
                catch (Exception ex)
                {
                    CustomPropsPlugin.Log?.LogError($"[OPTION B] Failed to add {propData.PropId}: {ex.Message}");
                }
            }

            CustomPropsPlugin.Log?.LogInfo("[OPTION B] All custom props added to UI!");
        }

        /// <summary>
        /// OPTION B POSTFIX: Runs AFTER OnLibraryRepopulated (after Synchronize).
        /// Adds our custom prop directly to the UI list.
        /// </summary>
        private static void OnLibraryRepopulated_Postfix(object __instance)
        {
            try
            {
                CustomPropsPlugin.Log?.LogInfo("[OPTION B] OnLibraryRepopulated_Postfix triggered!");

                // Get the list model from the panel view instance
                var listModel = _listModelField?.GetValue(__instance);
                if (listModel != null)
                {
                    AddCustomPropsToListModel(listModel);
                }
            }
            catch (Exception ex)
            {
                CustomPropsPlugin.Log?.LogError($"[OPTION B] OnLibraryRepopulated_Postfix error: {ex.Message}");
            }
        }

        /// <summary>
        /// Check if a prop is already in the UI list (by PropId/Name).
        /// </summary>
        private static bool IsAlreadyInUIList(object listModel, string propId)
        {
            try
            {
                // Get the List field
                var listField = _listModelType.GetField("List",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                var list = listField?.GetValue(listModel);

                if (list == null) return false;

                // Iterate and check names
                var enumerable = list as IEnumerable;
                if (enumerable == null) return false;

                foreach (var item in enumerable)
                {
                    var propDataField = item.GetType().GetField("PropData",
                        BindingFlags.Public | BindingFlags.Instance);
                    var propData = propDataField?.GetValue(item);

                    if (propData != null)
                    {
                        var nameField = propData.GetType().GetField("Name",
                            BindingFlags.Public | BindingFlags.Instance);
                        var name = nameField?.GetValue(propData) as string;

                        // Check if name matches our propId or display name
                        var ourPropData = PropRegistry.GetAll().FirstOrDefault(p => p.PropId == propId);
                        if (ourPropData != null &&
                            (name == ourPropData.DisplayName || name == propId))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Fallback: Add to List field directly if Add method not found.
        /// </summary>
        private static void AddToListDirectly(object listModel, object runtimePropInfo)
        {
            var listField = _listModelType.GetField("List",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var list = listField?.GetValue(listModel);

            if (list != null)
            {
                var addMethod = list.GetType().GetMethod("Add");
                addMethod?.Invoke(list, new object[] { runtimePropInfo });
            }
        }

        /// <summary>
        /// Get RuntimePropInfo from loadedPropMap or create a new one.
        /// </summary>
        private static object GetOrCreateRuntimePropInfo(PropData propData)
        {
            try
            {
                // First try to get from loadedPropMap (if already injected there)
                var stageManager = _stageManagerInstanceProp?.GetValue(null);
                if (stageManager != null)
                {
                    var propLibrary = _activePropLibraryField?.GetValue(stageManager);
                    if (propLibrary != null)
                    {
                        var loadedPropMap = _loadedPropMapField?.GetValue(propLibrary);
                        if (loadedPropMap != null)
                        {
                            // Search for our prop by name
                            var values = loadedPropMap.GetType().GetProperty("Values")?.GetValue(loadedPropMap) as IEnumerable;
                            if (values != null)
                            {
                                foreach (var rpi in values)
                                {
                                    var propDataField = _runtimePropInfoType.GetField("PropData",
                                        BindingFlags.Public | BindingFlags.Instance);
                                    var pd = propDataField?.GetValue(rpi);

                                    if (pd != null)
                                    {
                                        var nameField = pd.GetType().GetField("Name",
                                            BindingFlags.Public | BindingFlags.Instance);
                                        var name = nameField?.GetValue(pd) as string;

                                        if (name == propData.DisplayName)
                                        {
                                            CustomPropsPlugin.Log?.LogInfo($"[OPTION B] Found existing RuntimePropInfo for {propData.PropId}");
                                            return rpi;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                // Not found in loadedPropMap - create a new RuntimePropInfo
                CustomPropsPlugin.Log?.LogInfo($"[OPTION B] Creating new RuntimePropInfo for {propData.PropId}");
                return CreateRuntimePropInfoForUI(propData);
            }
            catch (Exception ex)
            {
                CustomPropsPlugin.Log?.LogError($"[OPTION B] GetOrCreateRuntimePropInfo failed: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Create a minimal RuntimePropInfo for UI display only.
        /// </summary>
        private static object CreateRuntimePropInfoForUI(PropData propData)
        {
            try
            {
                // Create Prop instance
                var propInstance = CreatePropInstance(propData);
                if (propInstance == null) return null;

                // Create RuntimePropInfo
                var runtimePropInfo = Activator.CreateInstance(_runtimePropInfoType);

                // Set PropData
                var propDataField = _runtimePropInfoType.GetField("PropData",
                    BindingFlags.Public | BindingFlags.Instance);
                propDataField?.SetValue(runtimePropInfo, propInstance);

                // Set Icon
                var iconField = _runtimePropInfoType.GetField("Icon",
                    BindingFlags.Public | BindingFlags.Instance);
                iconField?.SetValue(runtimePropInfo, propData.Icon);

                // Set IsLoading = false
                var isLoadingField = _runtimePropInfoType.GetField("<IsLoading>k__BackingField",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                isLoadingField?.SetValue(runtimePropInfo, false);

                // Set IsMissingObject = false (we have a prefab)
                var isMissingField = _runtimePropInfoType.GetField("<IsMissingObject>k__BackingField",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                isMissingField?.SetValue(runtimePropInfo, false);

                // Get or create EndlessProp with our prefab attached
                var endlessProp = CreateEndlessPropWithMesh(propData, propInstance);
                if (endlessProp != null)
                {
                    var endlessPropField = _runtimePropInfoType.GetField("EndlessProp",
                        BindingFlags.Public | BindingFlags.Instance);
                    endlessPropField?.SetValue(runtimePropInfo, endlessProp);
                }

                CustomPropsPlugin.Log?.LogInfo($"[OPTION B] Created RuntimePropInfo for UI: {propData.DisplayName}");
                return runtimePropInfo;
            }
            catch (Exception ex)
            {
                CustomPropsPlugin.Log?.LogError($"[OPTION B] CreateRuntimePropInfoForUI failed: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Create an EndlessProp instance with our custom mesh attached.
        /// </summary>
        private static object CreateEndlessPropWithMesh(PropData propData, object propInstance)
        {
            try
            {
                // Get basePropPrefab from PropLibrary
                var stageManager = _stageManagerInstanceProp?.GetValue(null);
                if (stageManager == null) return null;

                var propLibrary = _activePropLibraryField?.GetValue(stageManager);
                if (propLibrary == null) return null;

                var basePropPrefabField = _propLibraryType.GetField("basePropPrefab",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                var basePropPrefab = basePropPrefabField?.GetValue(propLibrary) as Component;

                if (basePropPrefab == null)
                {
                    CustomPropsPlugin.Log?.LogWarning("[OPTION B] basePropPrefab is null");
                    return null;
                }

                // Instantiate new EndlessProp from basePropPrefab
                var newEndlessPropGO = UnityEngine.Object.Instantiate(basePropPrefab.gameObject);
                newEndlessPropGO.name = $"CustomProp_{propData.PropId}";
                UnityEngine.Object.DontDestroyOnLoad(newEndlessPropGO);

                // Get EndlessProp component
                var endlessPropType = _propLibraryType.Assembly.GetType("Endless.Gameplay.Scripting.EndlessProp");
                var newEndlessProp = newEndlessPropGO.GetComponent(endlessPropType);

                if (newEndlessProp == null)
                {
                    UnityEngine.Object.Destroy(newEndlessPropGO);
                    return null;
                }

                // Attach custom mesh
                if (propData.Prefab != null)
                {
                    var customMeshInstance = UnityEngine.Object.Instantiate(propData.Prefab, newEndlessPropGO.transform);
                    customMeshInstance.name = $"{propData.PropId}_Mesh";
                    customMeshInstance.transform.localPosition = Vector3.zero;
                    customMeshInstance.transform.localRotation = Quaternion.identity;
                    customMeshInstance.transform.localScale = Vector3.one;
                    customMeshInstance.SetActive(true);
                }

                // Set Prop property
                var propProperty = endlessPropType.GetProperty("Prop",
                    BindingFlags.Public | BindingFlags.Instance);
                if (propProperty?.CanWrite == true)
                {
                    propProperty.SetValue(newEndlessProp, propInstance);
                }

                // Set ReferenceFilter to InventoryItem (8)
                var filterProp = endlessPropType.GetProperty("ReferenceFilter",
                    BindingFlags.Public | BindingFlags.Instance);
                if (filterProp?.CanWrite == true)
                {
                    var filterEnumType = filterProp.PropertyType;
                    var inventoryItemValue = Enum.ToObject(filterEnumType, 8);
                    filterProp.SetValue(newEndlessProp, inventoryItemValue);
                }

                return newEndlessProp;
            }
            catch (Exception ex)
            {
                CustomPropsPlugin.Log?.LogError($"[OPTION B] CreateEndlessPropWithMesh failed: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Start monitoring for StageManager and CreatorManager instances.
        /// Called from a coroutine.
        /// </summary>
        public static IEnumerator MonitorAndInject()
        {
            CustomPropsPlugin.Log?.LogInfo("Starting MonitorAndInject coroutine...");

            int attempts = 0;
            const int maxAttempts = 300; // 5 minutes at 1 check per second

            while (attempts < maxAttempts)
            {
                attempts++;

                try
                {
                    // Try to get StageManager.Instance
                    var stageManager = _stageManagerInstanceProp?.GetValue(null);

                    if (stageManager != null)
                    {
                        CustomPropsPlugin.Log?.LogInfo($"[Attempt {attempts}] StageManager.Instance found!");

                        // Get activePropLibrary
                        var propLibrary = _activePropLibraryField?.GetValue(stageManager);

                        if (propLibrary != null)
                        {
                            CustomPropsPlugin.Log?.LogInfo("activePropLibrary found!");

                            // Try to subscribe to CreatorManager events
                            if (!_subscribedToEvents)
                            {
                                TrySubscribeToEvents();
                            }

                            // DON'T inject here - wait for OnPropsRepopulated event
                            // Injecting during load causes "unknown error loading creator"
                            var loadedPropMap = _loadedPropMapField?.GetValue(propLibrary);
                            if (loadedPropMap != null)
                            {
                                var countProp = loadedPropMap.GetType().GetProperty("Count");
                                var count = (int)(countProp?.GetValue(loadedPropMap) ?? 0);

                                // DIAGNOSTIC: Log instance identity to verify we have correct PropLibrary
                                if (attempts == 1 || attempts % 20 == 0)
                                {
                                    CustomPropsPlugin.Log?.LogInfo($"[DIAG] PropLibrary (via FIELD) hashcode: {propLibrary.GetHashCode()}");

                                    // Compare field vs property (DLL: get_ActivePropLibrary)
                                    var propLibraryViaProp = _activePropLibraryProp?.GetValue(stageManager);
                                    if (propLibraryViaProp != null)
                                    {
                                        CustomPropsPlugin.Log?.LogInfo($"[DIAG] PropLibrary (via PROPERTY) hashcode: {propLibraryViaProp.GetHashCode()}");
                                        CustomPropsPlugin.Log?.LogInfo($"[DIAG] SAME INSTANCE? {object.ReferenceEquals(propLibrary, propLibraryViaProp)}");

                                        // Check loadedPropMap count via property
                                        var loadedPropMapViaProp = _loadedPropMapField?.GetValue(propLibraryViaProp);
                                        if (loadedPropMapViaProp != null)
                                        {
                                            var countPropViaProp = loadedPropMapViaProp.GetType().GetProperty("Count");
                                            var countViaProp = (int)(countPropViaProp?.GetValue(loadedPropMapViaProp) ?? 0);
                                            CustomPropsPlugin.Log?.LogInfo($"[DIAG] loadedPropMap Count via PROPERTY: {countViaProp}");
                                        }
                                    }
                                    else
                                    {
                                        CustomPropsPlugin.Log?.LogInfo($"[DIAG] PropLibrary via PROPERTY is NULL");
                                    }

                                    CustomPropsPlugin.Log?.LogInfo($"[DIAG] loadedPropMap type: {loadedPropMap.GetType().FullName}");
                                    CustomPropsPlugin.Log?.LogInfo($"[DIAG] loadedPropMap hashcode: {loadedPropMap.GetHashCode()}");
                                    CustomPropsPlugin.Log?.LogInfo($"[DIAG] Count property found: {countProp != null}");
                                    CustomPropsPlugin.Log?.LogInfo($"[DIAG] Count value (via FIELD): {count}");

                                    // Try to enumerate keys directly
                                    try
                                    {
                                        var keysProperty = loadedPropMap.GetType().GetProperty("Keys");
                                        var keys = keysProperty?.GetValue(loadedPropMap);
                                        if (keys != null)
                                        {
                                            var keysCountProp = keys.GetType().GetProperty("Count");
                                            var keysCount = keysCountProp?.GetValue(keys);
                                            CustomPropsPlugin.Log?.LogInfo($"[DIAG] Keys.Count: {keysCount}");
                                        }
                                    }
                                    catch (Exception diagEx)
                                    {
                                        CustomPropsPlugin.Log?.LogWarning($"[DIAG] Keys enum error: {diagEx.Message}");
                                    }
                                }

                                if (count > 0)
                                {
                                    // Also check if _referenceFilterMap is populated (needed for UI visibility)
                                    var filterMap = _referenceFilterMapField?.GetValue(propLibrary);
                                    bool filterMapReady = filterMap != null;

                                    if (!_injectionComplete && filterMapReady)
                                    {
                                        CustomPropsPlugin.Log?.LogInfo($"loadedPropMap has {count} props AND _referenceFilterMap is ready - INJECTING NOW!");
                                        InjectIntoLibrary(propLibrary);
                                    }
                                    else if (!_injectionComplete && !filterMapReady)
                                    {
                                        if (attempts % 5 == 0)
                                        {
                                            CustomPropsPlugin.Log?.LogInfo($"loadedPropMap has {count} props but _referenceFilterMap is null - waiting...");
                                        }
                                    }
                                    // Stop polling once injection is complete
                                    if (_injectionComplete)
                                    {
                                        CustomPropsPlugin.Log?.LogInfo("Injection complete - stopping poll loop");
                                        yield break; // Exit coroutine
                                    }
                                }
                                else
                                {
                                    CustomPropsPlugin.Log?.LogInfo("loadedPropMap is empty - waiting for props to load...");
                                }
                            }
                        }
                        else
                        {
                            if (attempts % 10 == 0)
                            {
                                CustomPropsPlugin.Log?.LogInfo($"[Attempt {attempts}] activePropLibrary is null, waiting...");
                            }
                        }
                    }
                    else
                    {
                        if (attempts % 30 == 0)
                        {
                            CustomPropsPlugin.Log?.LogInfo($"[Attempt {attempts}] StageManager.Instance is null, waiting...");
                        }
                    }
                }
                catch (Exception ex)
                {
                    CustomPropsPlugin.Log?.LogWarning($"Monitor error: {ex.Message}");
                }

                yield return new WaitForSeconds(1f);
            }

            CustomPropsPlugin.Log?.LogWarning("MonitorAndInject timed out after 5 minutes");
        }

        private static void TrySubscribeToEvents()
        {
            try
            {
                var creatorManager = _creatorManagerInstanceProp?.GetValue(null);

                if (creatorManager != null)
                {
                    CustomPropsPlugin.Log?.LogInfo("CreatorManager.Instance found!");

                    // Subscribe to OnPropsRepopulated
                    // CRITICAL: Prepend our handler so it runs BEFORE OnLibraryRepopulated
                    // OnLibraryRepopulated calls Synchronize() which populates the UI list
                    // If we run AFTER, our prop won't be in the list when Synchronize runs
                    var currentAction = _onPropsRepopulatedField?.GetValue(creatorManager) as Action;
                    Action myHandler = OnPropsRepopulatedHandler;
                    // Delegate.Combine(A, B) runs A first, then B
                    // So Combine(myHandler, currentAction) runs OUR handler FIRST
                    var newAction = (Action)Delegate.Combine(myHandler, currentAction);
                    _onPropsRepopulatedField?.SetValue(creatorManager, newAction);
                    CustomPropsPlugin.Log?.LogInfo("Subscribed to CreatorManager.OnPropsRepopulated (PREPENDED - runs before UI refresh)!");

                    // Also subscribe to OnCreatorStarted for session changes
                    var onCreatorStartedField = _creatorManagerType?.GetField("OnCreatorStarted",
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (onCreatorStartedField != null)
                    {
                        var eventObj = onCreatorStartedField.GetValue(creatorManager);
                        if (eventObj != null)
                        {
                            // OnCreatorStarted is a UnityEvent, need to use AddListener
                            var addListenerMethod = eventObj.GetType().GetMethod("AddListener",
                                BindingFlags.Public | BindingFlags.Instance);
                            if (addListenerMethod != null)
                            {
                                // Create UnityAction delegate
                                var unityActionType = typeof(UnityEngine.Events.UnityAction);
                                var handler = Delegate.CreateDelegate(unityActionType,
                                    typeof(ProperPropInjector).GetMethod(nameof(OnCreatorStartedHandler),
                                    BindingFlags.NonPublic | BindingFlags.Static));
                                addListenerMethod.Invoke(eventObj, new object[] { handler });
                                CustomPropsPlugin.Log?.LogInfo("Subscribed to CreatorManager.OnCreatorStarted!");
                            }
                        }
                    }

                    _subscribedToEvents = true;

                    // IMPORTANT: If props are already loaded, the event already fired before we subscribed
                    // Check and inject now if needed
                    var stageManager = _stageManagerInstanceProp?.GetValue(null);
                    if (stageManager != null)
                    {
                        var propLibrary = _activePropLibraryField?.GetValue(stageManager);
                        if (propLibrary != null)
                        {
                            var loadedPropMap = _loadedPropMapField?.GetValue(propLibrary);
                            if (loadedPropMap != null)
                            {
                                var countProp = loadedPropMap.GetType().GetProperty("Count");
                                var count = (int)(countProp?.GetValue(loadedPropMap) ?? 0);

                                if (count > 0 && !_injectionComplete)
                                {
                                    // Also check if _referenceFilterMap is populated
                                    var filterMap = _referenceFilterMapField?.GetValue(propLibrary);
                                    if (filterMap != null)
                                    {
                                        CustomPropsPlugin.Log?.LogInfo($"[FALLBACK] Props already loaded ({count}) - subscribed AFTER OnPropsRepopulated fired!");
                                        CustomPropsPlugin.Log?.LogInfo("[FALLBACK] Injecting now - will need to call Synchronize manually...");
                                        InjectIntoLibrary(propLibrary);
                                    }
                                    else
                                    {
                                        CustomPropsPlugin.Log?.LogInfo($"Props loaded ({count}) but _referenceFilterMap null - will inject when ready");
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CustomPropsPlugin.Log?.LogWarning($"Failed to subscribe to events: {ex.Message}");
            }
        }

        /// <summary>
        /// Called when a new creator session starts
        /// </summary>
        private static void OnCreatorStartedHandler()
        {
            CustomPropsPlugin.Log?.LogInfo("=== OnCreatorStarted event fired! (New session detected) ===");
            _injectionComplete = false; // Reset for new session

            // BUGFIX: Must re-inject when entering creator mode!
            // OnPropsRepopulated may NOT fire again if props are already loaded.
            // Check if props are loaded and inject immediately.
            try
            {
                var stageManager = _stageManagerInstanceProp?.GetValue(null);
                if (stageManager != null)
                {
                    var propLibrary = _activePropLibraryField?.GetValue(stageManager);
                    if (propLibrary != null)
                    {
                        var loadedPropMap = _loadedPropMapField?.GetValue(propLibrary);
                        var filterMap = _referenceFilterMapField?.GetValue(propLibrary);

                        if (loadedPropMap != null && filterMap != null)
                        {
                            var countProp = loadedPropMap.GetType().GetProperty("Count");
                            var count = (int)(countProp?.GetValue(loadedPropMap) ?? 0);

                            if (count > 0)
                            {
                                CustomPropsPlugin.Log?.LogInfo($"[OnCreatorStarted] Props already loaded ({count}) - RE-INJECTING NOW!");
                                InjectIntoLibrary(propLibrary);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CustomPropsPlugin.Log?.LogWarning($"[OnCreatorStarted] Re-injection check failed: {ex.Message}");
            }
        }

        /// <summary>
        /// OPTION A: Instantiate NEW EndlessProp per prop (evidence-based solution)
        /// - basePropPrefab is SHARED, causes PopulateReferenceFilterMap to fail
        /// - Must create unique EndlessProp instance with correct ReferenceFilter
        /// </summary>
        private static void TestMinimalInjection(object loadedPropMap, PropData propData)
        {
            try
            {
                var propInstance = CreatePropInstance(propData);
                if (propInstance == null) return;

                // Create AssetReference key - MUST use same AssetID as Prop.AssetID for lookup to work!
                object assetRefKey = null;
                if (_toAssetReferenceMethod != null)
                {
                    assetRefKey = _toAssetReferenceMethod.Invoke(propInstance, null);
                    CustomPropsPlugin.Log?.LogInfo($"AssetReference from ToAssetReference: {assetRefKey}");
                }

                // Fallback: create AssetReference with the GUID from Prop.AssetID (not propData.PropId!)
                if (assetRefKey == null)
                {
                    var propAssetIdField = propInstance.GetType().GetField("AssetID",
                        BindingFlags.Public | BindingFlags.Instance);
                    var propAssetId = propAssetIdField?.GetValue(propInstance) as string;

                    if (!string.IsNullOrEmpty(propAssetId))
                    {
                        assetRefKey = CreateAssetReference(propAssetId);
                        CustomPropsPlugin.Log?.LogInfo($"AssetReference created with GUID: {propAssetId}");
                    }
                    else
                    {
                        // Last resort - use propId (but this will break placement lookup!)
                        assetRefKey = CreateAssetReference(propData.PropId);
                        CustomPropsPlugin.Log?.LogWarning($"AssetReference created with propId (placement may fail): {propData.PropId}");
                    }
                }

                if (assetRefKey == null)
                {
                    CustomPropsPlugin.Log?.LogError("Failed to create AssetReference key");
                    return;
                }

                // Get basePropPrefab from PropLibrary
                var stageManager = _stageManagerInstanceProp?.GetValue(null);
                var propLibrary = _activePropLibraryField?.GetValue(stageManager);
                var basePropPrefabField = _propLibraryType.GetField("basePropPrefab",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                var basePropPrefab = basePropPrefabField?.GetValue(propLibrary) as Component;

                if (basePropPrefab == null)
                {
                    CustomPropsPlugin.Log?.LogError("basePropPrefab is null!");
                    return;
                }
                CustomPropsPlugin.Log?.LogInfo($"basePropPrefab: found");

                // OPTION A: Instantiate NEW EndlessProp from basePropPrefab
                CustomPropsPlugin.Log?.LogInfo($"Instantiating new EndlessProp from basePropPrefab...");
                var newEndlessPropGO = UnityEngine.Object.Instantiate(basePropPrefab.gameObject);
                newEndlessPropGO.name = $"CustomProp_{propData.PropId}";

                // Get EndlessProp component from instantiated object
                var endlessPropType = _propLibraryType.Assembly.GetType("Endless.Gameplay.Scripting.EndlessProp");
                var newEndlessProp = newEndlessPropGO.GetComponent(endlessPropType);

                // OPTION 1 FIX: Attach our custom prefab mesh to the EndlessProp
                // This makes the custom mesh visible WITHOUT needing BuildPrefab to load it
                if (propData.Prefab != null)
                {
                    CustomPropsPlugin.Log?.LogInfo($"Attaching custom prefab mesh: {propData.Prefab.name}");
                    var customMeshInstance = UnityEngine.Object.Instantiate(propData.Prefab, newEndlessPropGO.transform);
                    customMeshInstance.name = $"{propData.PropId}_Mesh";
                    customMeshInstance.transform.localPosition = Vector3.zero;
                    customMeshInstance.transform.localRotation = Quaternion.identity;
                    customMeshInstance.transform.localScale = Vector3.one;
                    CustomPropsPlugin.Log?.LogInfo($"Custom mesh attached to EndlessProp!");
                }
                else
                {
                    CustomPropsPlugin.Log?.LogWarning($"propData.Prefab is null - no custom mesh to attach!");
                }

                if (newEndlessProp == null)
                {
                    CustomPropsPlugin.Log?.LogError("Failed to get EndlessProp component from instantiated object!");
                    UnityEngine.Object.Destroy(newEndlessPropGO);
                    return;
                }
                CustomPropsPlugin.Log?.LogInfo($"New EndlessProp instantiated: {newEndlessPropGO.name}");

                // Set the Prop property on EndlessProp (DLL verified: has setter)
                var propProperty = endlessPropType.GetProperty("Prop",
                    BindingFlags.Public | BindingFlags.Instance);
                if (propProperty?.CanWrite == true)
                {
                    propProperty.SetValue(newEndlessProp, propInstance);
                    CustomPropsPlugin.Log?.LogInfo($"Set EndlessProp.Prop to our prop instance");
                }

                // Set ReferenceFilter directly (CalculateReferenceFilter fails for custom props
                // because it tries to lookup BaseTypeDefinition which doesn't exist)
                //
                // CRITICAL (DLL-Verified 2026-01-07):
                // dynamicFilters = { Npc (2), InventoryItem (8) } ONLY
                // Props with ReferenceFilter=None (0) DON'T APPEAR in UI!
                // Must use a value IN dynamicFilters: Npc=2 or InventoryItem=8
                //
                // UI shows InventoryItem(8) category - verified via FILTER_MAP diagnostic
                var filterProp = endlessPropType.GetProperty("ReferenceFilter",
                    BindingFlags.Public | BindingFlags.Instance);
                if (filterProp?.CanWrite == true)
                {
                    // Set to InventoryItem (8) - this is what UI displays
                    var filterEnumType = filterProp.PropertyType;
                    var inventoryItemValue = Enum.ToObject(filterEnumType, 8); // InventoryItem = 8
                    filterProp.SetValue(newEndlessProp, inventoryItemValue);
                    CustomPropsPlugin.Log?.LogInfo($"Set ReferenceFilter to InventoryItem (8) for UI visibility");
                }
                else
                {
                    // Try setting backing field directly
                    var backingField = endlessPropType.GetField("<ReferenceFilter>k__BackingField",
                        BindingFlags.NonPublic | BindingFlags.Instance);
                    if (backingField != null)
                    {
                        var filterEnumType = backingField.FieldType;
                        var inventoryItemValue = Enum.ToObject(filterEnumType, 8); // InventoryItem = 8
                        backingField.SetValue(newEndlessProp, inventoryItemValue);
                        CustomPropsPlugin.Log?.LogInfo($"Set ReferenceFilter backing field to InventoryItem (8)");
                    }
                }

                // Create RuntimePropInfo with our NEW EndlessProp instance
                var runtimePropInfo = Activator.CreateInstance(_runtimePropInfoType);

                var propDataField = _runtimePropInfoType.GetField("PropData",
                    BindingFlags.Public | BindingFlags.Instance);
                propDataField?.SetValue(runtimePropInfo, propInstance);

                var iconField = _runtimePropInfoType.GetField("Icon",
                    BindingFlags.Public | BindingFlags.Instance);
                iconField?.SetValue(runtimePropInfo, propData.Icon);

                // SET EndlessProp to our NEW instance (NOT shared basePropPrefab!)
                var endlessPropField = _runtimePropInfoType.GetField("EndlessProp",
                    BindingFlags.Public | BindingFlags.Instance);
                endlessPropField?.SetValue(runtimePropInfo, newEndlessProp);
                CustomPropsPlugin.Log?.LogInfo($"Set RuntimePropInfo.EndlessProp to NEW instance");

                // OPTION 1 FIX: Set IsMissingObject = FALSE since we attached our custom mesh
                // When false, game uses our EndlessProp directly instead of showing placeholder
                var isMissingField = _runtimePropInfoType.GetField("<IsMissingObject>k__BackingField",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                if (isMissingField != null)
                {
                    isMissingField.SetValue(runtimePropInfo, false);
                    CustomPropsPlugin.Log?.LogInfo($"Set IsMissingObject = false (custom mesh attached)");
                }

                // Check if already exists (StageManager.InjectProp may have added it)
                var containsMethod = loadedPropMap.GetType().GetMethod("ContainsKey");
                bool exists = (bool)(containsMethod?.Invoke(loadedPropMap, new[] { assetRefKey }) ?? false);

                if (exists)
                {
                    // Entry already exists from StageManager.InjectProp - UPDATE it instead of adding duplicate
                    CustomPropsPlugin.Log?.LogInfo($"Entry already exists in loadedPropMap - UPDATING instead of adding duplicate");

                    // Get the existing RuntimePropInfo
                    var indexer = loadedPropMap.GetType().GetProperty("Item");
                    var existingRuntimePropInfo = indexer?.GetValue(loadedPropMap, new[] { assetRefKey });

                    if (existingRuntimePropInfo != null)
                    {
                        // Update the existing entry's EndlessProp with our mesh-attached version
                        // (reuse endlessPropField from outer scope)
                        endlessPropField?.SetValue(existingRuntimePropInfo, newEndlessProp);
                        CustomPropsPlugin.Log?.LogInfo($"Updated existing RuntimePropInfo.EndlessProp with custom mesh");

                        // Also update IsMissingObject to false
                        isMissingField?.SetValue(existingRuntimePropInfo, false);
                        CustomPropsPlugin.Log?.LogInfo($"Updated existing RuntimePropInfo.IsMissingObject = false");

                        // Store for UI refresh
                        _pendingRuntimePropInfo = existingRuntimePropInfo;
                        _uiRefreshPending = true;
                    }
                }
                else
                {
                    // Entry doesn't exist - add new one
                    var addMethod = loadedPropMap.GetType().GetMethod("Add");
                    addMethod?.Invoke(loadedPropMap, new[] { assetRefKey, runtimePropInfo });
                    CustomPropsPlugin.Log?.LogInfo($"Added NEW RuntimePropInfo with EndlessProp for {propData.PropId}");

                    // CRITICAL: Also add to PropLibrary.injectedPropIds for GUID lookup!
                    // GetRuntimePropInfo(SerializableGuid) may use this list for lookup
                    try
                    {
                        var propAssetIdField = propInstance.GetType().GetField("AssetID",
                            BindingFlags.Public | BindingFlags.Instance);
                        var propAssetIdStr = propAssetIdField?.GetValue(propInstance) as string;

                        if (!string.IsNullOrEmpty(propAssetIdStr))
                        {
                            var smForInjectedIds = _stageManagerInstanceProp?.GetValue(null);
                            var currentPropLibrary = _activePropLibraryField?.GetValue(smForInjectedIds);
                            if (currentPropLibrary != null)
                            {
                                var injectedPropIdsField = _propLibraryType.GetField("injectedPropIds",
                                    BindingFlags.NonPublic | BindingFlags.Instance);
                                var injectedPropIds = injectedPropIdsField?.GetValue(currentPropLibrary);

                                if (injectedPropIds != null)
                                {
                                    // Create SerializableGuid from string
                                    var serializableGuidType = _propLibraryType.Assembly.GetType("Endless.Shared.DataTypes.SerializableGuid");
                                    if (serializableGuidType == null)
                                    {
                                        // Try Shared assembly
                                        var sharedAsm = Assembly.Load("Shared.DataTypes");
                                        serializableGuidType = sharedAsm?.GetType("Endless.Shared.DataTypes.SerializableGuid");
                                    }

                                    if (serializableGuidType != null)
                                    {
                                        var sgCtor = serializableGuidType.GetConstructor(new[] { typeof(string) });
                                        if (sgCtor != null)
                                        {
                                            var serializableGuid = sgCtor.Invoke(new object[] { propAssetIdStr });
                                            var addToListMethod = injectedPropIds.GetType().GetMethod("Add");
                                            addToListMethod?.Invoke(injectedPropIds, new[] { serializableGuid });
                                            CustomPropsPlugin.Log?.LogInfo($"Added SerializableGuid to PropLibrary.injectedPropIds: {propAssetIdStr}");
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception guidEx)
                    {
                        CustomPropsPlugin.Log?.LogWarning($"Failed to add to injectedPropIds: {guidEx.Message}");
                    }

                    // Store for delayed UI refresh (UIPropToolPanelView may not exist yet)
                    _pendingRuntimePropInfo = runtimePropInfo;
                    _uiRefreshPending = true;

                    // DISABLED: AddDirectlyToUIList causes DUPLICATE entries!
                    // The UI should pick up the prop from loadedPropMap automatically.
                    // AddDirectlyToUIList(runtimePropInfo);
                }
            }
            catch (Exception ex)
            {
                CustomPropsPlugin.Log?.LogError($"TestMinimalInjection failed: {ex.Message}");
                CustomPropsPlugin.Log?.LogError(ex.StackTrace);
            }
        }

        /// <summary>
        /// TEST: StageManager.InjectProp + InjectSingleProp to isolate if manual injection breaks prop tool
        /// </summary>
        private static void TestStageManagerInjectOnly(object propLibrary)
        {
            try
            {
                var stageManager = _stageManagerInstanceProp?.GetValue(null);
                if (stageManager == null)
                {
                    CustomPropsPlugin.Log?.LogError("StageManager.Instance is null!");
                    return;
                }

                var smInjectMethod = _stageManagerType.GetMethod("InjectProp",
                    BindingFlags.Public | BindingFlags.Instance);
                if (smInjectMethod == null)
                {
                    CustomPropsPlugin.Log?.LogError("StageManager.InjectProp method not found!");
                    return;
                }

                var loadedPropMap = _loadedPropMapField?.GetValue(propLibrary);

                var customProps = PropRegistry.GetAll().ToList();
                foreach (var propData in customProps)
                {
                    var propInstance = CreatePropInstance(propData);
                    if (propInstance == null) continue;

                    // Step 1: SKIP StageManager.InjectProp - causes "unknown error loading gameplay"
                    // The game's InjectProp may be validating the prop in ways we don't satisfy
                    // Only add to loadedPropMap directly instead
                    CustomPropsPlugin.Log?.LogInfo($"[TEST] Step 1: SKIPPING StageManager.InjectProp (causes gameplay load error)");

                    // Step 2: MINIMAL loadedPropMap injection (testing what breaks)
                    CustomPropsPlugin.Log?.LogInfo($"[TEST] Step 2: Minimal RuntimePropInfo (NO IsMissingObject, NO EndlessProp)...");
                    TestMinimalInjection(loadedPropMap, propData);
                    CustomPropsPlugin.Log?.LogInfo($"[TEST] Minimal injection done");

                    // Step 3: FixEndlessPropReferenceFilter - SKIPPED

                    // Step 4: PopulateReferenceFilterMap - TESTING if this makes prop visible
                    CustomPropsPlugin.Log?.LogInfo($"[TEST] Step 4: Calling PopulateReferenceFilterMap...");
                    RefreshReferenceFilterMap(propLibrary);
                    CustomPropsPlugin.Log?.LogInfo($"[TEST] PopulateReferenceFilterMap done");

                    // Step 5: SOLUTION 1 - Call Synchronize to refresh UI list
                    CustomPropsPlugin.Log?.LogInfo($"[TEST] Step 5: Calling RefreshUIAfterInjection (SOLUTION 1)...");
                    RefreshUIAfterInjection();
                    CustomPropsPlugin.Log?.LogInfo($"[TEST] RefreshUIAfterInjection done");
                }
            }
            catch (Exception ex)
            {
                CustomPropsPlugin.Log?.LogError($"TestStageManagerInjectOnly failed: {ex.Message}");
                CustomPropsPlugin.Log?.LogError(ex.StackTrace);
            }
        }

        private static void OnPropsRepopulatedHandler()
        {
            CustomPropsPlugin.Log?.LogInfo("=== OnPropsRepopulated: OUR HANDLER (prepended, runs BEFORE UI refresh) ===");

            _injectionComplete = false; // Reset - need to re-inject after repopulate

            try
            {
                var stageManager = _stageManagerInstanceProp?.GetValue(null);
                if (stageManager != null)
                {
                    var propLibrary = _activePropLibraryField?.GetValue(stageManager);
                    if (propLibrary != null)
                    {
                        CustomPropsPlugin.Log?.LogInfo("Injecting NOW - before OnLibraryRepopulated calls Synchronize...");
                        InjectIntoLibrary(propLibrary);
                        CustomPropsPlugin.Log?.LogInfo("Injection complete - UI's Synchronize should now include our prop!");
                    }
                }
            }
            catch (Exception ex)
            {
                CustomPropsPlugin.Log?.LogError($"OnPropsRepopulated handler error: {ex.Message}");
            }
        }

        private static void InjectIntoLibrary(object propLibrary)
        {
            // TEST: Enable only StageManager.InjectProp to isolate which step breaks prop tool
            CustomPropsPlugin.Log?.LogInfo("=== TESTING: StageManager.InjectProp ONLY ===");
            TestStageManagerInjectOnly(propLibrary);
            _injectionComplete = true;

            // Start UI refresh monitor coroutine if pending
            if (_uiRefreshPending)
            {
                CustomPropsPlugin.Log?.LogInfo("Starting UI refresh monitor coroutine...");
                StartUIMonitorCoroutine();
            }
            return;

            if (_injectionComplete)
            {
                CustomPropsPlugin.Log?.LogInfo("Already injected, skipping");
                return;
            }

            CustomPropsPlugin.Log?.LogInfo("=== Injecting custom props using DUAL API (StageManager + PropLibrary) ===");

            try
            {
                // Get StageManager instance
                var stageManager = _stageManagerInstanceProp?.GetValue(null);
                if (stageManager == null)
                {
                    CustomPropsPlugin.Log?.LogError("StageManager.Instance is null!");
                    return;
                }

                // Get StageManager.InjectProp method (4 params)
                var smInjectMethod = _stageManagerType.GetMethod("InjectProp",
                    BindingFlags.Public | BindingFlags.Instance);
                if (smInjectMethod == null)
                {
                    CustomPropsPlugin.Log?.LogError("StageManager.InjectProp method not found!");
                    return;
                }
                CustomPropsPlugin.Log?.LogInfo("Found StageManager.InjectProp method (4 params)");

                // Get PropLibrary.InjectProp method (6 params)
                var plInjectMethod = _propLibraryType.GetMethod("InjectProp",
                    BindingFlags.Public | BindingFlags.Instance);
                if (plInjectMethod == null)
                {
                    CustomPropsPlugin.Log?.LogError("PropLibrary.InjectProp method not found!");
                    return;
                }
                CustomPropsPlugin.Log?.LogInfo("Found PropLibrary.InjectProp method (6 params)");

                // Get prefabSpawnRoot from PropLibrary
                var prefabSpawnRootField = _propLibraryType.GetField("prefabSpawnRoot",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                var prefabSpawnRoot = prefabSpawnRootField?.GetValue(propLibrary);
                CustomPropsPlugin.Log?.LogInfo($"prefabSpawnRoot: {(prefabSpawnRoot != null ? "found" : "null")}");

                // Get basePropPrefab from PropLibrary
                var basePropPrefabField = _propLibraryType.GetField("basePropPrefab",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                var basePropPrefab = basePropPrefabField?.GetValue(propLibrary);
                CustomPropsPlugin.Log?.LogInfo($"basePropPrefab: {(basePropPrefab != null ? "found" : "null")}");

                // Get a valid baseTypeId from existing props
                var loadedPropMap = _loadedPropMapField?.GetValue(propLibrary);
                if (loadedPropMap != null)
                {
                    _validBaseTypeId = GetValidBaseTypeId(loadedPropMap);
                    CustomPropsPlugin.Log?.LogInfo($"Using baseTypeId: {_validBaseTypeId}");
                }

                // Get custom props to inject
                var customProps = PropRegistry.GetAll().ToList();
                CustomPropsPlugin.Log?.LogInfo($"Injecting {customProps.Count} custom props...");

                foreach (var propData in customProps)
                {
                    try
                    {
                        // Create the Prop instance once
                        var propInstance = CreatePropInstance(propData);
                        if (propInstance == null)
                        {
                            CustomPropsPlugin.Log?.LogError($"Failed to create Prop instance for {propData.PropId}");
                            continue;
                        }

                        var prefab = propData.Prefab;
                        var icon = propData.Icon;

                        // STEP 1: Call StageManager.InjectProp (stores testPrefab in injectedProps)
                        CustomPropsPlugin.Log?.LogInfo($"[{propData.PropId}] Step 1: StageManager.InjectProp...");
                        object[] smArgs = new object[] { propInstance, prefab, null, icon };
                        smInjectMethod.Invoke(stageManager, smArgs);
                        CustomPropsPlugin.Log?.LogInfo($"[{propData.PropId}] StageManager.InjectProp SUCCESS");

                        // STEP 2: Manually add to loadedPropMap (PropLibrary.InjectProp doesn't work)
                        // PropLibrary.InjectProp returns without error but doesn't add to loadedPropMap
                        CustomPropsPlugin.Log?.LogInfo($"[{propData.PropId}] Step 2: Manual injection to loadedPropMap...");
                        InjectSingleProp(loadedPropMap, propData);

                        // Verify the prop was added
                        var newCount = (int)(_loadedPropMapField?.GetValue(propLibrary)?.GetType().GetProperty("Count")?.GetValue(_loadedPropMapField.GetValue(propLibrary)) ?? 0);
                        CustomPropsPlugin.Log?.LogInfo($"[{propData.PropId}] loadedPropMap count after manual injection: {newCount}");

                        // STEP 3: Fix ReferenceFilter on the EndlessProp so it appears in filter map
                        CustomPropsPlugin.Log?.LogInfo($"[{propData.PropId}] Step 3: Fixing ReferenceFilter on EndlessProp...");
                        FixEndlessPropReferenceFilter(propLibrary, propInstance, propData.PropId);

                        // STEP 4: Refresh _referenceFilterMap for UI visibility
                        CustomPropsPlugin.Log?.LogInfo($"[{propData.PropId}] Step 4: Refreshing _referenceFilterMap...");
                        RefreshReferenceFilterMap(propLibrary);

                        // STEP 5: Record injection for persistence
                        try
                        {
                            var assetIdField = propInstance.GetType().GetField("AssetID",
                                BindingFlags.Public | BindingFlags.Instance);
                            if (assetIdField != null)
                            {
                                var assetId = assetIdField.GetValue(propInstance) as string;
                                if (!string.IsNullOrEmpty(assetId))
                                {
                                    PropPersistence.RecordInjection(propData.PropId, assetId, propData.DisplayName);
                                    CustomPropsPlugin.Log?.LogInfo($"[{propData.PropId}] Recorded AssetId={assetId} for persistence");
                                }
                            }
                        }
                        catch (Exception pex)
                        {
                            CustomPropsPlugin.Log?.LogWarning($"[{propData.PropId}] Failed to record persistence: {pex.Message}");
                        }

                        CustomPropsPlugin.Log?.LogInfo($"=== {propData.PropId} fully injected! ===");
                    }
                    catch (Exception ex)
                    {
                        CustomPropsPlugin.Log?.LogError($"Failed to inject {propData.PropId}: {ex.Message}");
                        CustomPropsPlugin.Log?.LogError(ex.StackTrace);
                    }
                }

                // Verify injection
                VerifyInjection(loadedPropMap);

                _injectionComplete = true;
                CustomPropsPlugin.Log?.LogInfo("=== DUAL Injection complete! ===");
                CustomPropsPlugin.Log?.LogInfo("Prop should now appear in BOTH injectedProps AND loadedPropMap (UI)");
            }
            catch (Exception ex)
            {
                CustomPropsPlugin.Log?.LogError($"Injection failed: {ex.Message}");
                CustomPropsPlugin.Log?.LogError(ex.StackTrace);
            }
        }

        private static void VerifyInjection(object loadedPropMap)
        {
            CustomPropsPlugin.Log?.LogInfo("=== Verifying injection ===");

            if (loadedPropMap == null)
            {
                CustomPropsPlugin.Log?.LogWarning("Cannot verify: loadedPropMap is null");
                return;
            }

            try
            {
                // Check loadedPropMap count
                var countProp = loadedPropMap.GetType().GetProperty("Count");
                if (countProp != null)
                {
                    var count = (int)countProp.GetValue(loadedPropMap);
                    CustomPropsPlugin.Log?.LogInfo($"loadedPropMap now has {count} entries");
                }

                // Check injectedPropIds
                var stageManager = _stageManagerInstanceProp?.GetValue(null);
                if (stageManager != null)
                {
                    var injectedPropsField = _stageManagerType.GetField("injectedProps",
                        BindingFlags.NonPublic | BindingFlags.Instance);
                    if (injectedPropsField != null)
                    {
                        var injectedProps = injectedPropsField.GetValue(stageManager);
                        if (injectedProps != null)
                        {
                            var injectedCountProp = injectedProps.GetType().GetProperty("Count");
                            if (injectedCountProp != null)
                            {
                                var injectedCount = (int)injectedCountProp.GetValue(injectedProps);
                                CustomPropsPlugin.Log?.LogInfo($"StageManager.injectedProps has {injectedCount} entries");
                            }
                        }
                    }
                }

                CustomPropsPlugin.Log?.LogInfo("=== Verification complete ===");
            }
            catch (Exception ex)
            {
                CustomPropsPlugin.Log?.LogWarning($"Verification error: {ex.Message}");
            }
        }

        /// <summary>
        /// Fix the ReferenceFilter on the EndlessProp so it gets categorized in filter map.
        /// PopulateReferenceFilterMap reads EndlessProp.ReferenceFilter to categorize props.
        /// </summary>
        private static void FixEndlessPropReferenceFilter(object propLibrary, object propInstance, string propId)
        {
            try
            {
                // Get the RuntimePropInfo for our prop from loadedPropMap
                var loadedPropMap = _loadedPropMapField?.GetValue(propLibrary);
                if (loadedPropMap == null)
                {
                    CustomPropsPlugin.Log?.LogWarning("loadedPropMap is null, cannot fix ReferenceFilter");
                    return;
                }

                // Create AssetReference key to find our prop
                var assetIdField = propInstance.GetType().GetField("AssetID",
                    BindingFlags.Public | BindingFlags.Instance);
                var assetId = assetIdField?.GetValue(propInstance) as string;

                if (string.IsNullOrEmpty(assetId))
                {
                    CustomPropsPlugin.Log?.LogWarning("AssetID is null, cannot find RuntimePropInfo");
                    return;
                }

                // Iterate through loadedPropMap to find our RuntimePropInfo
                var enumerator = loadedPropMap.GetType().GetMethod("GetEnumerator")?.Invoke(loadedPropMap, null);
                if (enumerator == null) return;

                var moveNextMethod = enumerator.GetType().GetMethod("MoveNext");
                var currentProp = enumerator.GetType().GetProperty("Current");

                object foundRuntimePropInfo = null;

                while ((bool)moveNextMethod.Invoke(enumerator, null))
                {
                    var kvp = currentProp.GetValue(enumerator);
                    var valueField = kvp.GetType().GetProperty("Value");
                    var rpi = valueField?.GetValue(kvp);

                    if (rpi != null)
                    {
                        // Get PropData from RuntimePropInfo
                        var propDataField = _runtimePropInfoType?.GetField("PropData",
                            BindingFlags.Public | BindingFlags.Instance);
                        var propData = propDataField?.GetValue(rpi);

                        if (propData != null)
                        {
                            var rpiAssetIdField = propData.GetType().GetField("AssetID",
                                BindingFlags.Public | BindingFlags.Instance);
                            var rpiAssetId = rpiAssetIdField?.GetValue(propData) as string;

                            if (rpiAssetId == assetId)
                            {
                                foundRuntimePropInfo = rpi;
                                CustomPropsPlugin.Log?.LogInfo($"Found RuntimePropInfo for {propId}");
                                break;
                            }
                        }
                    }
                }

                // Dispose enumerator if IDisposable
                if (enumerator is IDisposable disposable)
                {
                    disposable.Dispose();
                }

                if (foundRuntimePropInfo == null)
                {
                    CustomPropsPlugin.Log?.LogWarning($"Could not find RuntimePropInfo for {propId}");
                    return;
                }

                // Get EndlessProp from RuntimePropInfo
                var endlessPropField = _runtimePropInfoType?.GetField("EndlessProp",
                    BindingFlags.Public | BindingFlags.Instance);
                var endlessProp = endlessPropField?.GetValue(foundRuntimePropInfo);

                if (endlessProp == null)
                {
                    CustomPropsPlugin.Log?.LogWarning($"EndlessProp is null for {propId}");
                    return;
                }

                // Get current ReferenceFilter value
                var refFilterProp = endlessProp.GetType().GetProperty("ReferenceFilter",
                    BindingFlags.Public | BindingFlags.Instance);
                var currentFilter = refFilterProp?.GetValue(endlessProp);
                CustomPropsPlugin.Log?.LogInfo($"Current ReferenceFilter for {propId}: {currentFilter}");

                // Set ReferenceFilter to None (0) - this should make it appear in the default category
                // ReferenceFilter enum: None=0, NonStatic=1, Npc=2, PhysicsObject=4, InventoryItem=8, Key=16, Resource=32
                if (_referenceFilterType != null)
                {
                    // Try to set it to None first
                    var noneValue = Enum.ToObject(_referenceFilterType, 0);
                    refFilterProp?.SetValue(endlessProp, noneValue);
                    CustomPropsPlugin.Log?.LogInfo($"Set ReferenceFilter to None (0) for {propId}");

                    // Verify
                    var newFilter = refFilterProp?.GetValue(endlessProp);
                    CustomPropsPlugin.Log?.LogInfo($"Verified ReferenceFilter is now: {newFilter}");
                }
            }
            catch (Exception ex)
            {
                CustomPropsPlugin.Log?.LogError($"FixEndlessPropReferenceFilter failed: {ex.Message}");
                CustomPropsPlugin.Log?.LogError(ex.StackTrace);
            }
        }

        /// <summary>
        /// Call PopulateReferenceFilterMap to rebuild the filter map with our injected prop included.
        /// </summary>
        private static void RefreshReferenceFilterMap(object propLibrary)
        {
            try
            {
                // Get PopulateReferenceFilterMap method
                var populateMethod = _propLibraryType?.GetMethod("PopulateReferenceFilterMap",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                if (populateMethod == null)
                {
                    CustomPropsPlugin.Log?.LogWarning("PopulateReferenceFilterMap method not found");
                    return;
                }

                CustomPropsPlugin.Log?.LogInfo("Calling PopulateReferenceFilterMap to rebuild filter map...");
                populateMethod.Invoke(propLibrary, null);

                // Verify the filter map was populated
                var filterMap = _referenceFilterMapField?.GetValue(propLibrary);
                if (filterMap != null)
                {
                    var countProp = filterMap.GetType().GetProperty("Count");
                    var count = countProp?.GetValue(filterMap);
                    CustomPropsPlugin.Log?.LogInfo($"PopulateReferenceFilterMap complete. Filter map has {count} categories");

                    // DIAGNOSTIC: Enumerate filter map to find our prop
                    try
                    {
                        var enumerator = filterMap.GetType().GetMethod("GetEnumerator")?.Invoke(filterMap, null);
                        var moveNext = enumerator.GetType().GetMethod("MoveNext");
                        var currentProp = enumerator.GetType().GetProperty("Current");

                        while ((bool)moveNext.Invoke(enumerator, null))
                        {
                            var kvp = currentProp.GetValue(enumerator);
                            var keyProp = kvp.GetType().GetProperty("Key");
                            var valueProp = kvp.GetType().GetProperty("Value");
                            var filterKey = keyProp?.GetValue(kvp);
                            var propList = valueProp?.GetValue(kvp);

                            if (propList != null)
                            {
                                var listCount = propList.GetType().GetProperty("Count")?.GetValue(propList);
                                CustomPropsPlugin.Log?.LogInfo($"[FILTER_MAP] Category {filterKey}: {listCount} props");

                                // Check if our prop is in this category (Npc=2)
                                if (filterKey != null && filterKey.ToString() == "Npc")
                                {
                                    var listEnumerator = ((IEnumerable)propList).GetEnumerator();
                                    int idx = 0;
                                    while (listEnumerator.MoveNext() && idx < 5)
                                    {
                                        var rpi = listEnumerator.Current;
                                        var propDataField = rpi.GetType().GetField("PropData", BindingFlags.Public | BindingFlags.Instance);
                                        var propData = propDataField?.GetValue(rpi);
                                        var nameField = propData?.GetType().GetField("Name", BindingFlags.Public | BindingFlags.Instance);
                                        var name = nameField?.GetValue(propData);
                                        CustomPropsPlugin.Log?.LogInfo($"  [NPC {idx}] {name}");
                                        idx++;
                                    }
                                }
                            }
                        }

                        if (enumerator is IDisposable disp) disp.Dispose();
                    }
                    catch (Exception diagEx)
                    {
                        CustomPropsPlugin.Log?.LogWarning($"[DIAG] Filter map enum error: {diagEx.Message}");
                    }
                }
                else
                {
                    CustomPropsPlugin.Log?.LogWarning("Filter map still null after PopulateReferenceFilterMap");
                }
            }
            catch (Exception ex)
            {
                CustomPropsPlugin.Log?.LogError($"Failed to refresh filter map: {ex.Message}");
                CustomPropsPlugin.Log?.LogError(ex.StackTrace);
            }
        }

        private static void InjectSingleProp(object loadedPropMap, PropData propData)
        {
            CustomPropsPlugin.Log?.LogInfo($"Injecting: {propData.PropId}");

            // Create Prop instance
            var propInstance = CreatePropInstance(propData);
            if (propInstance == null)
            {
                CustomPropsPlugin.Log?.LogError("Failed to create Prop instance");
                return;
            }

            // Create AssetReference key - MUST use same AssetID as Prop.AssetID for lookup to work!
            object assetRefKey = null;
            if (_toAssetReferenceMethod != null)
            {
                assetRefKey = _toAssetReferenceMethod.Invoke(propInstance, null);
                CustomPropsPlugin.Log?.LogInfo($"Created AssetReference key via ToAssetReference");
            }

            // Fallback: create AssetReference with the GUID from Prop.AssetID (not propData.PropId!)
            if (assetRefKey == null)
            {
                var propAssetIdField = propInstance.GetType().GetField("AssetID",
                    BindingFlags.Public | BindingFlags.Instance);
                var propAssetId = propAssetIdField?.GetValue(propInstance) as string;

                if (!string.IsNullOrEmpty(propAssetId))
                {
                    assetRefKey = CreateAssetReference(propAssetId);
                    CustomPropsPlugin.Log?.LogInfo($"Created AssetReference with GUID: {propAssetId}");
                }
                else
                {
                    assetRefKey = CreateAssetReference(propData.PropId);
                    CustomPropsPlugin.Log?.LogWarning($"Created AssetReference with propId (placement may fail): {propData.PropId}");
                }
            }

            if (assetRefKey == null)
            {
                CustomPropsPlugin.Log?.LogError("Failed to create AssetReference key");
                return;
            }

            // Create RuntimePropInfo
            var runtimePropInfo = Activator.CreateInstance(_runtimePropInfoType);

            var propDataField = _runtimePropInfoType.GetField("PropData",
                BindingFlags.Public | BindingFlags.Instance);
            propDataField?.SetValue(runtimePropInfo, propInstance);

            var iconField = _runtimePropInfoType.GetField("Icon",
                BindingFlags.Public | BindingFlags.Instance);
            iconField?.SetValue(runtimePropInfo, propData.Icon);

            // CRITICAL: Mark as missing object so game uses fallback prefab
            // Without this, the game crashes trying to load non-existent prefab
            var isMissingField = _runtimePropInfoType.GetField("<IsMissingObject>k__BackingField",
                BindingFlags.NonPublic | BindingFlags.Instance);
            if (isMissingField != null)
            {
                isMissingField.SetValue(runtimePropInfo, true);
                CustomPropsPlugin.Log?.LogInfo("Set IsMissingObject = true");
            }

            // Try to get the missing object prefab from StageManager
            try
            {
                var stageManager = _stageManagerInstanceProp?.GetValue(null);
                if (stageManager != null)
                {
                    var missingPrefabField = _stageManagerType.GetField("missingObjectPrefab",
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    var missingPrefab = missingPrefabField?.GetValue(stageManager);

                    if (missingPrefab != null)
                    {
                        var endlessPropField = _runtimePropInfoType.GetField("EndlessProp",
                            BindingFlags.Public | BindingFlags.Instance);
                        endlessPropField?.SetValue(runtimePropInfo, missingPrefab);
                        CustomPropsPlugin.Log?.LogInfo("Set EndlessProp to missingObjectPrefab");
                    }
                }
            }
            catch (Exception ex)
            {
                CustomPropsPlugin.Log?.LogWarning($"Could not set missing prefab: {ex.Message}");
            }

            // Check if already exists
            var containsMethod = loadedPropMap.GetType().GetMethod("ContainsKey");
            bool exists = (bool)(containsMethod?.Invoke(loadedPropMap, new[] { assetRefKey }) ?? false);

            if (!exists)
            {
                // Add to dictionary
                var addMethod = loadedPropMap.GetType().GetMethod("Add");
                addMethod?.Invoke(loadedPropMap, new[] { assetRefKey, runtimePropInfo });
                CustomPropsPlugin.Log?.LogInfo($"SUCCESS: Added {propData.PropId} to loadedPropMap");
            }
            else
            {
                CustomPropsPlugin.Log?.LogInfo($"Prop {propData.PropId} already exists in map");
            }
        }

        private static object CreatePropInstance(PropData propData)
        {
            try
            {
                var ctor = _propType.GetConstructor(Type.EmptyTypes);
                if (ctor == null)
                {
                    CustomPropsPlugin.Log?.LogError("Prop has no default constructor");
                    return null;
                }

                var prop = ctor.Invoke(null);

                // Set basic fields via reflection
                SetFieldDeep(prop, "Name", propData.DisplayName);
                // AssetID must be valid GUID format - game parses it as System.Guid
                var assetGuid = GenerateGuidFromString(propData.PropId);
                SetFieldDeep(prop, "AssetID", assetGuid.ToString());
                CustomPropsPlugin.Log?.LogInfo($"Generated AssetID GUID: {assetGuid} for {propData.PropId}");
                SetFieldDeep(prop, "AssetVersion", "1.0.0");
                SetFieldDeep(prop, "AssetType", "Prop");
                SetFieldDeep(prop, "Description", $"Custom prop: {propData.DisplayName}");
                SetFieldDeep(prop, "baseTypeId", _validBaseTypeId ?? "treasure");
                SetFieldDeep(prop, "openSource", true);

                // Set bounds
                var boundsField = FindFieldDeep(_propType, "bounds");
                if (boundsField != null && boundsField.FieldType == typeof(Vector3Int))
                {
                    boundsField.SetValue(prop, new Vector3Int(1, 1, 1));
                }

                // Initialize List fields to empty lists to avoid null reference exceptions
                InitializeListField(prop, "componentIds");
                InitializeListField(prop, "visualAssets");
                InitializeListField(prop, "transformRemaps");

                // Initialize empty array for propLocationOffsets
                var offsetsField = FindFieldDeep(_propType, "propLocationOffsets");
                if (offsetsField != null && offsetsField.FieldType.IsArray)
                {
                    var elementType = offsetsField.FieldType.GetElementType();
                    var emptyArray = Array.CreateInstance(elementType, 0);
                    offsetsField.SetValue(prop, emptyArray);
                }

                CustomPropsPlugin.Log?.LogInfo($"Created Prop instance: {propData.DisplayName}");
                return prop;
            }
            catch (Exception ex)
            {
                CustomPropsPlugin.Log?.LogError($"CreatePropInstance failed: {ex.Message}");
                return null;
            }
        }

        private static void InitializeListField(object obj, string fieldName)
        {
            try
            {
                var field = FindFieldDeep(obj.GetType(), fieldName);
                if (field != null && field.FieldType.IsGenericType &&
                    field.FieldType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    var listType = field.FieldType;
                    var emptyList = Activator.CreateInstance(listType);
                    field.SetValue(obj, emptyList);
                }
            }
            catch { }
        }

        private static object CreateAssetReference(string assetId)
        {
            try
            {
                if (_assetReferenceType == null) return null;

                var ctor = _assetReferenceType.GetConstructor(Type.EmptyTypes);
                if (ctor == null) return null;

                var assetRef = ctor.Invoke(null);

                // Set ALL fields - AssetReference.Equals may compare all of them!
                var assetIdField = _assetReferenceType.GetField("AssetID",
                    BindingFlags.Public | BindingFlags.Instance);
                assetIdField?.SetValue(assetRef, assetId);

                var assetVersionField = _assetReferenceType.GetField("AssetVersion",
                    BindingFlags.Public | BindingFlags.Instance);
                assetVersionField?.SetValue(assetRef, "1.0.0");

                var assetTypeField = _assetReferenceType.GetField("AssetType",
                    BindingFlags.Public | BindingFlags.Instance);
                assetTypeField?.SetValue(assetRef, "Prop");

                return assetRef;
            }
            catch
            {
                return null;
            }
        }

        private static string GetValidBaseTypeId(object loadedPropMap)
        {
            try
            {
                // Get first entry from dictionary
                var valuesMethod = loadedPropMap.GetType().GetProperty("Values");
                var values = valuesMethod?.GetValue(loadedPropMap) as IEnumerable<object>;

                if (values != null)
                {
                    foreach (var rpi in values)
                    {
                        var propDataField = _runtimePropInfoType.GetField("PropData",
                            BindingFlags.Public | BindingFlags.Instance);
                        var propData = propDataField?.GetValue(rpi);

                        if (propData != null)
                        {
                            var baseTypeField = FindFieldDeep(propData.GetType(), "baseTypeId");
                            var baseTypeId = baseTypeField?.GetValue(propData)?.ToString();

                            if (!string.IsNullOrEmpty(baseTypeId))
                            {
                                return baseTypeId;
                            }
                        }
                    }
                }

                return "treasure";
            }
            catch
            {
                return "treasure";
            }
        }

        private static FieldInfo FindFieldDeep(Type type, string fieldName)
        {
            if (type == null) return null;

            var current = type;
            while (current != null && current != typeof(object))
            {
                var field = current.GetField(fieldName,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                if (field != null) return field;
                current = current.BaseType;
            }
            return null;
        }

        private static void SetFieldDeep(object obj, string fieldName, object value)
        {
            var field = FindFieldDeep(obj.GetType(), fieldName);
            if (field != null)
            {
                try { field.SetValue(obj, value); }
                catch { }
            }
        }

        /// <summary>
        /// Generates a deterministic GUID from a string using MD5 hash.
        /// Same input always produces same GUID.
        /// </summary>
        private static Guid GenerateGuidFromString(string input)
        {
            using (var md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
                return new Guid(hash);
            }
        }

        /// <summary>
        /// SOLUTION 1: Call Synchronize again after adding to loadedPropMap.
        /// DLL-verified: UIRuntimePropInfoListModel.Synchronize [Token: 0x06000559]
        /// UIPropToolPanelView.runtimePropInfoListModel [Token: 0x040009BA]
        /// </summary>
        private static void RefreshUIAfterInjection()
        {
            try
            {
                CustomPropsPlugin.Log?.LogInfo("[SOLUTION 1] Refreshing UI via Synchronize...");

                var creatorAsm = AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(a => a.GetName().Name == "Creator");
                if (creatorAsm == null)
                {
                    CustomPropsPlugin.Log?.LogWarning("[SOLUTION 1] Creator assembly not found");
                    return;
                }

                var panelType = creatorAsm.GetType("Endless.Creator.UI.UIPropToolPanelView");
                if (panelType == null)
                {
                    CustomPropsPlugin.Log?.LogWarning("[SOLUTION 1] UIPropToolPanelView type not found");
                    return;
                }

                var findMethod = typeof(UnityEngine.Object).GetMethod("FindObjectOfType",
                    new Type[] { typeof(Type) });
                var panelInstance = findMethod?.Invoke(null, new object[] { panelType });

                if (panelInstance == null)
                {
                    CustomPropsPlugin.Log?.LogWarning("[SOLUTION 1] UIPropToolPanelView instance not found");
                    return;
                }
                CustomPropsPlugin.Log?.LogInfo("[SOLUTION 1] Found UIPropToolPanelView instance");

                var listModelField = panelType.GetField("runtimePropInfoListModel",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                if (listModelField == null)
                {
                    CustomPropsPlugin.Log?.LogWarning("[SOLUTION 1] runtimePropInfoListModel field not found");
                    return;
                }

                var listModel = listModelField.GetValue(panelInstance);
                if (listModel == null)
                {
                    CustomPropsPlugin.Log?.LogWarning("[SOLUTION 1] runtimePropInfoListModel is null");
                    return;
                }
                CustomPropsPlugin.Log?.LogInfo("[SOLUTION 1] Got runtimePropInfoListModel");

                var listModelType = listModel.GetType();
                var syncMethod = listModelType.GetMethod("Synchronize",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (syncMethod == null)
                {
                    CustomPropsPlugin.Log?.LogWarning("[SOLUTION 1] Synchronize method not found");
                    return;
                }

                var gameplayAsm = AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(a => a.GetName().Name == "Gameplay");
                var refFilterType = gameplayAsm?.GetType("Endless.Gameplay.ReferenceFilter");
                object filterValue = Enum.ToObject(refFilterType, 8);

                // propsToIgnore is IReadOnlyList<RuntimePropInfo>, not AssetReference
                var runtimePropInfoType = gameplayAsm?.GetType("Endless.Gameplay.LevelEditing.PropLibrary+RuntimePropInfo");
                var listType = typeof(List<>).MakeGenericType(runtimePropInfoType);
                var emptyList = Activator.CreateInstance(listType);

                CustomPropsPlugin.Log?.LogInfo("[SOLUTION 1] Calling Synchronize(InventoryItem, emptyList)...");
                syncMethod.Invoke(listModel, new object[] { filterValue, emptyList });
                CustomPropsPlugin.Log?.LogInfo("[SOLUTION 1] Synchronize completed!");

                var listField = listModelType.GetField("List",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (listField != null)
                {
                    var list = listField.GetValue(listModel);
                    if (list != null)
                    {
                        var countProp = list.GetType().GetProperty("Count");
                        var count = countProp?.GetValue(list);
                        CustomPropsPlugin.Log?.LogInfo($"[SOLUTION 1] List now has {count} items");
                    }
                }
            }
            catch (Exception ex)
            {
                CustomPropsPlugin.Log?.LogError($"[SOLUTION 1] Failed: {ex.Message}");
                CustomPropsPlugin.Log?.LogError(ex.StackTrace);
            }
        }

        /// <summary>
        /// Start the UI monitor coroutine on any available MonoBehaviour
        /// </summary>
        private static void StartUIMonitorCoroutine()
        {
            try
            {
                // Find any MonoBehaviour to run coroutine on
                var monoBehaviour = UnityEngine.Object.FindObjectOfType<MonoBehaviour>();
                if (monoBehaviour != null)
                {
                    monoBehaviour.StartCoroutine(MonitorForUIRefresh());
                    CustomPropsPlugin.Log?.LogInfo("[UI_MONITOR] Coroutine started on " + monoBehaviour.GetType().Name);
                }
                else
                {
                    CustomPropsPlugin.Log?.LogWarning("[UI_MONITOR] No MonoBehaviour found to run coroutine");
                }
            }
            catch (Exception ex)
            {
                CustomPropsPlugin.Log?.LogError($"[UI_MONITOR] Failed to start coroutine: {ex.Message}");
            }
        }

        /// <summary>
        /// Coroutine to monitor for UIPropToolPanelView and refresh UI when found.
        /// DLL-verified: UIPropToolPanelView, runtimePropInfoListModel [Token: 0x040009BA]
        /// </summary>
        public static IEnumerator MonitorForUIRefresh()
        {
            CustomPropsPlugin.Log?.LogInfo("[UI_MONITOR] Starting UI refresh monitor...");

            int attempts = 0;
            const int maxAttempts = 60; // 60 seconds max

            while (_uiRefreshPending && attempts < maxAttempts)
            {
                attempts++;

                try
                {
                    var creatorAsm = AppDomain.CurrentDomain.GetAssemblies()
                        .FirstOrDefault(a => a.GetName().Name == "Creator");
                    if (creatorAsm != null)
                    {
                        var panelType = creatorAsm.GetType("Endless.Creator.UI.UIPropToolPanelView");
                        if (panelType != null)
                        {
                            var findMethod = typeof(UnityEngine.Object).GetMethod("FindObjectOfType",
                                new Type[] { typeof(Type) });
                            var panelInstance = findMethod?.Invoke(null, new object[] { panelType });

                            if (panelInstance != null)
                            {
                                CustomPropsPlugin.Log?.LogInfo($"[UI_MONITOR] Found UIPropToolPanelView on attempt {attempts}!");

                                // DISABLED: AddDirectlyToUIList causes DUPLICATE entries!
                                // UI will pick up prop from loadedPropMap when it refreshes.
                                // if (_pendingRuntimePropInfo != null)
                                // {
                                //     AddDirectlyToUIList(_pendingRuntimePropInfo);
                                // }

                                _uiRefreshPending = false;
                                _pendingRuntimePropInfo = null;
                                CustomPropsPlugin.Log?.LogInfo("[UI_MONITOR] UI refresh complete (no direct add - relying on loadedPropMap)!");
                                yield break;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    CustomPropsPlugin.Log?.LogWarning($"[UI_MONITOR] Error: {ex.Message}");
                }

                if (attempts % 10 == 0)
                {
                    CustomPropsPlugin.Log?.LogInfo($"[UI_MONITOR] Waiting for UIPropToolPanelView... (attempt {attempts})");
                }

                yield return new WaitForSeconds(1f);
            }

            if (_uiRefreshPending)
            {
                CustomPropsPlugin.Log?.LogWarning("[UI_MONITOR] Timed out waiting for UIPropToolPanelView");
            }
        }

        /// <summary>
        /// SOLUTION 3: Add RuntimePropInfo directly to UIRuntimePropInfoListModel.List
        /// DLL-verified: UIPropToolPanelView.runtimePropInfoListModel [Token: 0x040009BA]
        /// DLL-verified: UIRuntimePropInfoListModel.List field
        /// </summary>
        private static void AddDirectlyToUIList(object runtimePropInfo)
        {
            try
            {
                CustomPropsPlugin.Log?.LogInfo("[SOLUTION 3] Adding RuntimePropInfo to List...");

                var creatorAsm = AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(a => a.GetName().Name == "Creator");
                if (creatorAsm == null)
                {
                    CustomPropsPlugin.Log?.LogWarning("[SOLUTION 3] Creator assembly not found");
                    return;
                }

                var panelType = creatorAsm.GetType("Endless.Creator.UI.UIPropToolPanelView");
                if (panelType == null)
                {
                    CustomPropsPlugin.Log?.LogWarning("[SOLUTION 3] UIPropToolPanelView type not found");
                    return;
                }

                var findMethod = typeof(UnityEngine.Object).GetMethod("FindObjectOfType",
                    new Type[] { typeof(Type) });
                var panelInstance = findMethod?.Invoke(null, new object[] { panelType });

                if (panelInstance == null)
                {
                    CustomPropsPlugin.Log?.LogWarning("[SOLUTION 3] UIPropToolPanelView not found");
                    return;
                }

                var listModelField = panelType.GetField("runtimePropInfoListModel",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                var listModel = listModelField?.GetValue(panelInstance);
                if (listModel == null)
                {
                    CustomPropsPlugin.Log?.LogWarning("[SOLUTION 3] runtimePropInfoListModel is null");
                    return;
                }

                var listModelType = listModel.GetType();

                // Use the proper Add(item, triggerEvents) method from UIBaseLocalFilterableListModel
                // This will trigger UI refresh automatically when triggerEvents=true
                var addMethod = listModelType.GetMethod("Add",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                    null,
                    new Type[] { runtimePropInfo.GetType(), typeof(bool) },
                    null);

                if (addMethod != null)
                {
                    // Add with triggerEvents=true to refresh UI
                    addMethod.Invoke(listModel, new object[] { runtimePropInfo, true });
                    CustomPropsPlugin.Log?.LogInfo("[SOLUTION 3] Added via Add(item, triggerEvents=true) - should refresh UI!");
                }
                else
                {
                    // Fallback: add to List directly and call ReFilter
                    CustomPropsPlugin.Log?.LogWarning("[SOLUTION 3] Add(item, bool) not found, trying fallback...");

                    var listField = listModelType.GetField("List",
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    var list = listField?.GetValue(listModel);

                    if (list != null)
                    {
                        var listAddMethod = list.GetType().GetMethod("Add");
                        listAddMethod?.Invoke(list, new object[] { runtimePropInfo });
                        CustomPropsPlugin.Log?.LogInfo("[SOLUTION 3] Added to List directly");

                        // Call ReFilter(true) to trigger refresh
                        var reFilterMethod = listModelType.GetMethod("ReFilter",
                            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                        if (reFilterMethod != null)
                        {
                            reFilterMethod.Invoke(listModel, new object[] { true });
                            CustomPropsPlugin.Log?.LogInfo("[SOLUTION 3] Called ReFilter(true) to refresh UI");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CustomPropsPlugin.Log?.LogError($"[SOLUTION 3] Failed: {ex.Message}");
                CustomPropsPlugin.Log?.LogError(ex.StackTrace);
            }
        }

        /// <summary>
        /// Apply Harmony patches. V4 uses Option B postfix on OnLibraryRepopulated.
        /// </summary>
        public static void ApplyHarmonyPatches(Harmony harmony)
        {
            CustomPropsPlugin.Log?.LogInfo("V4: Applying Option B Harmony patch (OnLibraryRepopulated postfix)");

            // Apply Option B patch - this is the key to making custom props visible in UI
            ApplyOptionBPatch(harmony);
        }

        public static void TryInjectNow()
        {
            if (!_initialized) Initialize();

            try
            {
                var stageManager = _stageManagerInstanceProp?.GetValue(null);
                if (stageManager != null)
                {
                    var propLibrary = _activePropLibraryField?.GetValue(stageManager);
                    if (propLibrary != null && !_injectionComplete)
                    {
                        InjectIntoLibrary(propLibrary);
                    }
                }
            }
            catch (Exception ex)
            {
                CustomPropsPlugin.Log?.LogWarning($"TryInjectNow failed: {ex.Message}");
            }
        }

        public static void Reset()
        {
            _injectionComplete = false;
        }

        public static bool IsInjectionComplete => _injectionComplete;
    }
}

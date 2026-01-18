using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx.Logging;
using UnityEngine;

namespace CustomProps
{
    /// <summary>
    /// Direct prop injection using StageManager.InjectProp method.
    /// Based on DLL analysis: StageManager.InjectProp adds to injectedProps list,
    /// which stores TestPrefab separately so BuildPrefab uses it instead of prefabBundle.
    /// Following HOOK E: Use real assets only, never stubs.
    /// </summary>
    public class DirectPropInjector
    {
        private static ManualLogSource Log => CustomPropsPlugin.Log;

        // Cached types
        private Type _stageManagerType;
        private Type _propLibraryType;
        private Type _runtimePropInfoType;
        private Type _assetReferenceType;
        private Type _propType;
        private Type _serializableGuidType;
        private Type _endlessPropType;

        // Cached accessors
        private PropertyInfo _stageManagerInstance;
        private PropertyInfo _activePropLibrary;
        private FieldInfo _loadedPropMapField;
        private FieldInfo _basePropPrefabField;
        private FieldInfo _prefabSpawnRootField;
        private MethodInfo _stageManagerInjectPropMethod;  // 4 params - stores testPrefab
        private MethodInfo _propLibraryInjectPropMethod;   // 6 params - adds to loadedPropMap

        // Loaded assets from bundle
        private AssetBundle _assetBundle;
        private GameObject _pearlBasketPrefab;
        private Sprite _pearlBasketIcon;

        private bool _initialized = false;

        public bool Initialize()
        {
            try
            {
                Log.LogInfo("=== DirectPropInjector Initializing ===");

                // Get assemblies
                var gameplayAsm = GetAssembly("Gameplay");
                var assetsAsm = GetAssembly("Assets");
                var propsAsm = GetAssembly("Props");
                var sharedAsm = GetAssembly("Shared.DataTypes");

                if (gameplayAsm == null || assetsAsm == null || propsAsm == null)
                {
                    Log.LogError("Required assemblies not found");
                    return false;
                }

                // Cache types
                _stageManagerType = gameplayAsm.GetType("Endless.Gameplay.LevelEditing.Level.StageManager");
                _propLibraryType = gameplayAsm.GetType("Endless.Gameplay.LevelEditing.PropLibrary");
                _runtimePropInfoType = gameplayAsm.GetType("Endless.Gameplay.LevelEditing.PropLibrary+RuntimePropInfo");
                _assetReferenceType = assetsAsm.GetType("Endless.Assets.AssetReference");
                _propType = propsAsm.GetType("Endless.Props.Assets.Prop");
                _serializableGuidType = sharedAsm?.GetType("Endless.Shared.DataTypes.SerializableGuid");
                _endlessPropType = gameplayAsm.GetType("Endless.Gameplay.Scripting.EndlessProp");

                Log.LogInfo($"StageManager: {_stageManagerType != null}");
                Log.LogInfo($"PropLibrary: {_propLibraryType != null}");
                Log.LogInfo($"RuntimePropInfo: {_runtimePropInfoType != null}");
                Log.LogInfo($"AssetReference: {_assetReferenceType != null}");
                Log.LogInfo($"Prop: {_propType != null}");
                Log.LogInfo($"SerializableGuid: {_serializableGuidType != null}");
                Log.LogInfo($"EndlessProp: {_endlessPropType != null}");

                if (_stageManagerType == null || _propLibraryType == null ||
                    _runtimePropInfoType == null || _assetReferenceType == null || _propType == null)
                {
                    Log.LogError("Required types not found");
                    return false;
                }

                // Cache accessors
                _stageManagerInstance = _stageManagerType.GetProperty("Instance",
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
                _activePropLibrary = _stageManagerType.GetProperty("ActivePropLibrary",
                    BindingFlags.Public | BindingFlags.Instance);
                _loadedPropMapField = _propLibraryType.GetField("loadedPropMap",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                _basePropPrefabField = _propLibraryType.GetField("basePropPrefab",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                _prefabSpawnRootField = _propLibraryType.GetField("prefabSpawnRoot",
                    BindingFlags.NonPublic | BindingFlags.Instance);

                // Get BOTH InjectProp methods
                // StageManager.InjectProp: 4 params - stores testPrefab in injectedProps
                _stageManagerInjectPropMethod = _stageManagerType.GetMethod("InjectProp",
                    BindingFlags.Public | BindingFlags.Instance);

                // PropLibrary.InjectProp: 6 params - adds to loadedPropMap for UI
                _propLibraryInjectPropMethod = _propLibraryType.GetMethod("InjectProp",
                    BindingFlags.Public | BindingFlags.Instance);

                Log.LogInfo($"StageManager.Instance: {_stageManagerInstance != null}");
                Log.LogInfo($"ActivePropLibrary: {_activePropLibrary != null}");
                Log.LogInfo($"loadedPropMap: {_loadedPropMapField != null}");
                Log.LogInfo($"basePropPrefab: {_basePropPrefabField != null}");
                Log.LogInfo($"prefabSpawnRoot: {_prefabSpawnRootField != null}");
                Log.LogInfo($"StageManager.InjectProp: {_stageManagerInjectPropMethod != null}");
                Log.LogInfo($"PropLibrary.InjectProp: {_propLibraryInjectPropMethod != null}");

                if (_stageManagerInjectPropMethod != null)
                {
                    Log.LogInfo("StageManager.InjectProp parameters:");
                    foreach (var p in _stageManagerInjectPropMethod.GetParameters())
                    {
                        Log.LogInfo($"  {p.Position}: {p.ParameterType.Name} {p.Name}");
                    }
                }
                if (_propLibraryInjectPropMethod != null)
                {
                    Log.LogInfo("PropLibrary.InjectProp parameters:");
                    foreach (var p in _propLibraryInjectPropMethod.GetParameters())
                    {
                        Log.LogInfo($"  {p.Position}: {p.ParameterType.Name} {p.Name}");
                    }
                }

                // Load asset bundle
                LoadAssetBundle();

                _initialized = true;
                Log.LogInfo("=== DirectPropInjector Initialized ===");
                return true;
            }
            catch (Exception ex)
            {
                Log.LogError($"Initialize error: {ex.Message}");
                return false;
            }
        }

        private void LoadAssetBundle()
        {
            try
            {
                string bundlePath = Path.Combine(
                    Path.GetDirectoryName(typeof(DirectPropInjector).Assembly.Location),
                    "customprops.bundle");

                Log.LogInfo($"Loading asset bundle from: {bundlePath}");

                if (!File.Exists(bundlePath))
                {
                    Log.LogError($"Asset bundle not found at: {bundlePath}");
                    return;
                }

                _assetBundle = AssetBundle.LoadFromFile(bundlePath);
                if (_assetBundle == null)
                {
                    Log.LogError("Failed to load asset bundle");
                    return;
                }

                Log.LogInfo("Asset bundle loaded. Contents:");
                foreach (var name in _assetBundle.GetAllAssetNames())
                {
                    Log.LogInfo($"  - {name}");
                }

                // Load prefab
                _pearlBasketPrefab = _assetBundle.LoadAsset<GameObject>("Assets/Pearl Basket/PearlBasket.prefab");
                Log.LogInfo($"Loaded PearlBasket prefab: {_pearlBasketPrefab != null}");

                // Load icon
                var iconTexture = _assetBundle.LoadAsset<Texture2D>("Assets/Pearl Basket/pearl basket icon.png");
                if (iconTexture != null)
                {
                    _pearlBasketIcon = Sprite.Create(iconTexture,
                        new Rect(0, 0, iconTexture.width, iconTexture.height),
                        new Vector2(0.5f, 0.5f));
                    Log.LogInfo($"Loaded PearlBasket icon: {_pearlBasketIcon != null}");
                }
            }
            catch (Exception ex)
            {
                Log.LogError($"LoadAssetBundle error: {ex.Message}");
            }
        }

        public bool InjectProp(string propId, string propName)
        {
            if (!_initialized)
            {
                Log.LogError("DirectPropInjector not initialized");
                return false;
            }

            if (_pearlBasketPrefab == null)
            {
                Log.LogError("Pearl Basket prefab not loaded");
                return false;
            }

            if (_stageManagerInjectPropMethod == null || _propLibraryInjectPropMethod == null)
            {
                Log.LogError($"InjectProp methods not found - SM: {_stageManagerInjectPropMethod != null}, PL: {_propLibraryInjectPropMethod != null}");
                return false;
            }

            try
            {
                Log.LogInfo($"=== Injecting prop using OFFICIAL API: {propName} (ID: {propId}) ===");

                // Get PropLibrary
                var stageManager = _stageManagerInstance?.GetValue(null);
                if (stageManager == null)
                {
                    Log.LogError("StageManager.Instance is null");
                    return false;
                }

                var propLibrary = _activePropLibrary?.GetValue(stageManager);
                if (propLibrary == null)
                {
                    Log.LogError("ActivePropLibrary is null");
                    return false;
                }

                // Get loadedPropMap to verify props are loaded and to clone from
                var loadedPropMap = _loadedPropMapField?.GetValue(propLibrary);
                if (loadedPropMap == null)
                {
                    Log.LogError("loadedPropMap is null");
                    return false;
                }

                var countProp = loadedPropMap.GetType().GetProperty("Count");
                int beforeCount = (int)countProp.GetValue(loadedPropMap);
                Log.LogInfo($"loadedPropMap count before: {beforeCount}");

                if (beforeCount == 0)
                {
                    Log.LogWarning("Props not loaded yet - cannot inject");
                    return false;
                }

                // Get required fields from PropLibrary
                var basePropPrefab = _basePropPrefabField?.GetValue(propLibrary);
                var prefabSpawnRoot = _prefabSpawnRootField?.GetValue(propLibrary);

                Log.LogInfo($"basePropPrefab: {basePropPrefab != null}");
                Log.LogInfo($"prefabSpawnRoot: {prefabSpawnRoot != null}");

                // 1. Get an existing prop to clone its data structure (HOOK E compliance)
                var existingProp = GetFirstExistingProp(loadedPropMap);
                if (existingProp == null)
                {
                    Log.LogError("Could not get existing prop to clone");
                    return false;
                }
                Log.LogInfo($"Got existing prop to clone structure from");

                // Log existing prop details for debugging
                LogPropDetails("Existing prop", existingProp);

                // 2. Create our Prop by copying ALL fields from existing prop
                var newProp = CloneProp(existingProp, propId, propName);
                if (newProp == null)
                {
                    Log.LogError("Failed to clone prop");
                    return false;
                }
                Log.LogInfo($"Created new Prop: {propName}");

                // Log new prop details
                LogPropDetails("New prop", newProp);

                // 3. Call BOTH InjectProp methods
                // Step 3a: StageManager.InjectProp - stores testPrefab in injectedProps
                Log.LogInfo("Calling StageManager.InjectProp (stores testPrefab)...");
                _stageManagerInjectPropMethod.Invoke(stageManager, new object[] {
                    newProp,
                    _pearlBasketPrefab,
                    null,  // testScript
                    _pearlBasketIcon
                });
                Log.LogInfo("StageManager.InjectProp called!");

                // Step 3b: PropLibrary.InjectProp - adds to loadedPropMap for UI
                Log.LogInfo("Calling PropLibrary.InjectProp (adds to UI)...");
                _propLibraryInjectPropMethod.Invoke(propLibrary, new object[] {
                    newProp,
                    _pearlBasketPrefab,
                    null,  // testScript
                    _pearlBasketIcon,
                    prefabSpawnRoot,
                    basePropPrefab
                });
                Log.LogInfo("PropLibrary.InjectProp called!");

                // Verify
                int afterCount = (int)countProp.GetValue(loadedPropMap);
                Log.LogInfo($"loadedPropMap count after: {afterCount}");

                if (afterCount > beforeCount)
                {
                    Log.LogInfo($"=== Successfully injected: {propName} ===");
                    return true;
                }
                else
                {
                    Log.LogWarning($"Prop count did not increase - injection may have failed silently");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log.LogError($"InjectProp error: {ex.Message}");
                Log.LogError(ex.StackTrace);
                if (ex.InnerException != null)
                {
                    Log.LogError($"Inner exception: {ex.InnerException.Message}");
                    Log.LogError(ex.InnerException.StackTrace);
                }
                return false;
            }
        }

        private void LogPropDetails(string label, object prop)
        {
            try
            {
                var assetIdField = GetFieldRecursive(_propType, "AssetID");
                var nameField = GetFieldRecursive(_propType, "Name");
                var baseTypeIdField = GetFieldRecursive(_propType, "baseTypeId");

                Log.LogInfo($"{label}:");
                Log.LogInfo($"  AssetID: {assetIdField?.GetValue(prop)}");
                Log.LogInfo($"  Name: {nameField?.GetValue(prop)}");
                Log.LogInfo($"  baseTypeId: {baseTypeIdField?.GetValue(prop)}");
            }
            catch (Exception ex)
            {
                Log.LogWarning($"Could not log prop details: {ex.Message}");
            }
        }

        private FieldInfo GetFieldRecursive(Type type, string fieldName)
        {
            while (type != null)
            {
                var field = type.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                if (field != null) return field;
                type = type.BaseType;
            }
            return null;
        }

        private object GetFirstExistingProp(object loadedPropMap)
        {
            try
            {
                var enumerator = ((IEnumerable)loadedPropMap).GetEnumerator();
                if (!enumerator.MoveNext())
                    return null;

                var firstEntry = enumerator.Current;
                var valueProperty = firstEntry.GetType().GetProperty("Value");
                var runtimePropInfo = valueProperty.GetValue(firstEntry);

                var propDataField = _runtimePropInfoType.GetField("PropData", BindingFlags.Public | BindingFlags.Instance);
                return propDataField.GetValue(runtimePropInfo);
            }
            catch (Exception ex)
            {
                Log.LogError($"GetFirstExistingProp error: {ex.Message}");
                return null;
            }
        }

        private object CloneProp(object existingProp, string newAssetId, string newName)
        {
            try
            {
                // Create new Prop instance
                var newProp = Activator.CreateInstance(_propType);

                // Copy ALL fields from existing prop (HOOK E: use real data)
                var allFields = GetAllFields(_propType);
                int copiedCount = 0;
                foreach (var field in allFields)
                {
                    try
                    {
                        var value = field.GetValue(existingProp);
                        field.SetValue(newProp, value);
                        copiedCount++;
                    }
                    catch { }
                }
                Log.LogInfo($"Copied {copiedCount} fields from existing prop");

                // Override the fields we need to change
                SetFieldValue(newProp, "AssetID", newAssetId);
                SetFieldValue(newProp, "Name", newName);
                SetFieldValue(newProp, "Description", newName);

                // NOTE: We keep the original baseTypeId from the cloned prop (Treasure's baseTypeId).
                // Changing baseTypeId breaks the prop's visibility because baseTypeRequirementLookup
                // validates that baseTypeId exists in the game's BaseType system.
                // The mesh merge issue needs to be solved differently.
                var currentBaseTypeId = GetFieldValue(newProp, "baseTypeId");
                Log.LogInfo($"Keeping baseTypeId: {currentBaseTypeId} (required for prop validation)");

                // CRITICAL: Create VALID AssetReference with unique ID instead of null!
                // loadedPropMap uses prefabBundle as dictionary KEY - null breaks lookup on reload.
                // Solution: Use our unique prop ID - game won't find it in asset system,
                // but dictionary lookup will work, and testPrefab provides the actual mesh.
                var prefabBundleField = GetFieldRecursive(_propType, "prefabBundle");
                if (prefabBundleField != null)
                {
                    // Log what we're replacing
                    var oldPrefabBundle = prefabBundleField.GetValue(newProp);
                    if (oldPrefabBundle != null)
                    {
                        var oldAssetIdField = oldPrefabBundle.GetType().GetField("AssetID", BindingFlags.Public | BindingFlags.Instance);
                        Log.LogInfo($"Replacing prefabBundle (was: {oldAssetIdField?.GetValue(oldPrefabBundle)})");
                    }

                    // Create new AssetReference with our unique prop ID
                    var newPrefabBundle = Activator.CreateInstance(_assetReferenceType);

                    // Set fields using reflection
                    var assetIdField = _assetReferenceType.GetField("AssetID", BindingFlags.Public | BindingFlags.Instance);
                    var assetVersionField = _assetReferenceType.GetField("AssetVersion", BindingFlags.Public | BindingFlags.Instance);
                    var assetTypeField = _assetReferenceType.GetField("AssetType", BindingFlags.Public | BindingFlags.Instance);

                    assetIdField?.SetValue(newPrefabBundle, newAssetId);
                    assetVersionField?.SetValue(newPrefabBundle, "1.0.0");
                    assetTypeField?.SetValue(newPrefabBundle, "Prop");

                    prefabBundleField.SetValue(newProp, newPrefabBundle);
                    Log.LogInfo($"prefabBundle set to valid AssetReference with ID: {newAssetId}");
                }

                // CRITICAL: Also clear visualAssets list - this contains additional asset references
                // that may load Treasure's mesh alongside testPrefab
                var visualAssetsField = GetFieldRecursive(_propType, "visualAssets");
                if (visualAssetsField != null)
                {
                    var oldVisualAssets = visualAssetsField.GetValue(newProp);
                    if (oldVisualAssets != null)
                    {
                        var countProp = oldVisualAssets.GetType().GetProperty("Count");
                        var oldCount = countProp?.GetValue(oldVisualAssets);
                        Log.LogInfo($"Clearing visualAssets (had {oldCount} entries)");

                        // Clear the list
                        var clearMethod = oldVisualAssets.GetType().GetMethod("Clear");
                        clearMethod?.Invoke(oldVisualAssets, null);
                        Log.LogInfo("visualAssets cleared");
                    }
                }

                // CRITICAL: Clear scriptAsset - might load Treasure's script/visuals
                var scriptAssetField = GetFieldRecursive(_propType, "scriptAsset");
                if (scriptAssetField != null)
                {
                    var oldScriptAsset = scriptAssetField.GetValue(newProp);
                    if (oldScriptAsset != null)
                    {
                        var assetIdField = oldScriptAsset.GetType().GetField("AssetID", BindingFlags.Public | BindingFlags.Instance);
                        Log.LogInfo($"Clearing scriptAsset (was: {assetIdField?.GetValue(oldScriptAsset)})");
                    }
                    scriptAssetField.SetValue(newProp, null);
                    Log.LogInfo("scriptAsset set to null");
                }

                // CRITICAL: Clear componentIds - might reference Treasure's components
                var componentIdsField = GetFieldRecursive(_propType, "componentIds");
                if (componentIdsField != null)
                {
                    var oldComponentIds = componentIdsField.GetValue(newProp);
                    if (oldComponentIds != null)
                    {
                        var countProp = oldComponentIds.GetType().GetProperty("Count");
                        var oldCount = countProp?.GetValue(oldComponentIds);
                        Log.LogInfo($"Clearing componentIds (had {oldCount} entries)");

                        var clearMethod = oldComponentIds.GetType().GetMethod("Clear");
                        clearMethod?.Invoke(oldComponentIds, null);
                        Log.LogInfo("componentIds cleared");
                    }
                }

                // Clear iconFileInstanceId so our provided icon is used
                SetFieldValue(newProp, "iconFileInstanceId", 0);
                Log.LogInfo("iconFileInstanceId set to 0");

                // Set bounds to 1x1x1 for proper placement
                var boundsField = GetFieldRecursive(_propType, "bounds");
                if (boundsField != null)
                {
                    var bounds = new Vector3Int(1, 1, 1);
                    boundsField.SetValue(newProp, bounds);
                }

                Log.LogInfo($"Cloned prop with all asset references cleared");
                return newProp;
            }
            catch (Exception ex)
            {
                Log.LogError($"CloneProp error: {ex.Message}");
                return null;
            }
        }

        private List<FieldInfo> GetAllFields(Type type)
        {
            var fields = new List<FieldInfo>();
            while (type != null && type != typeof(object))
            {
                fields.AddRange(type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
                type = type.BaseType;
            }
            return fields;
        }

        private void SetFieldValue(object obj, string fieldName, object value)
        {
            var type = obj.GetType();
            while (type != null)
            {
                var field = type.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                if (field != null)
                {
                    field.SetValue(obj, value);
                    return;
                }
                type = type.BaseType;
            }
            Log.LogWarning($"Field not found: {fieldName}");
        }

        private object GetFieldValue(object obj, string fieldName)
        {
            var field = GetFieldRecursive(obj.GetType(), fieldName);
            return field?.GetValue(obj);
        }

        public int GetLoadedPropCount()
        {
            try
            {
                var stageManager = _stageManagerInstance?.GetValue(null);
                if (stageManager == null) return -1;

                var propLibrary = _activePropLibrary?.GetValue(stageManager);
                if (propLibrary == null) return -1;

                var loadedPropMap = _loadedPropMapField?.GetValue(propLibrary);
                if (loadedPropMap == null) return -1;

                var countProp = loadedPropMap.GetType().GetProperty("Count");
                return (int)countProp.GetValue(loadedPropMap);
            }
            catch
            {
                return -1;
            }
        }

        private Assembly GetAssembly(string name)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (asm.GetName().Name == name)
                    return asm;
            }
            return null;
        }
    }
}

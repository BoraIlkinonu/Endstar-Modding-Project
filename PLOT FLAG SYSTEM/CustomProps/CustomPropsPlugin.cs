using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CustomProps
{
    /// <summary>
    /// V3 - Uses singleton access and event subscription instead of Harmony patches
    ///
    /// Key changes from V2:
    /// 1. Uses StageManager.Instance singleton to access activePropLibrary
    /// 2. Subscribes to CreatorManager.OnPropsRepopulated event
    /// 3. Polls for instances via MonitorAndInject coroutine
    /// 4. Injects directly into loadedPropMap dictionary
    /// 5. No Harmony patches on PropLibrary methods (they weren't firing)
    /// </summary>

    /// <summary>
    /// Helper MonoBehaviour - V3: Runs MonitorAndInject coroutine
    /// </summary>
    public class PropSearchHelper : MonoBehaviour
    {
        public CustomPropsPlugin Plugin;
        private bool _monitorStarted = false;

        void Awake()
        {
            CustomPropsPlugin.Log?.LogInfo("[Helper] Awake()");
        }

        void Start()
        {
            // DISABLED FOR TESTING - MonitorAndInject may be causing gameplay load error
            CustomPropsPlugin.Log?.LogInfo("[Helper] V3 MonitorAndInject DISABLED for testing");
            // if (!_monitorStarted)
            // {
            //     _monitorStarted = true;
            //     CustomPropsPlugin.Log?.LogInfo("[Helper] Starting V3 MonitorAndInject coroutine");
            //     StartCoroutine(ProperPropInjector.MonitorAndInject());
            // }
        }

        void Update()
        {
            // Only handle key presses - injection is handled by coroutine
            HandleKeyInput();
        }

        void HandleKeyInput()
        {
            // F9 - Spawn test prop
            if (Input.GetKeyDown(KeyCode.F9))
            {
                CustomPropsPlugin.Log?.LogInfo("F9 pressed - spawning test prop");
                Plugin?.SpawnTestPropPublic();
            }

            // F10 - List registered props
            if (Input.GetKeyDown(KeyCode.F10))
            {
                CustomPropsPlugin.Log?.LogInfo("F10 pressed - listing props");
                Plugin?.ListRegisteredPropsPublic();
            }

            // F11 - Force injection
            if (Input.GetKeyDown(KeyCode.F11))
            {
                CustomPropsPlugin.Log?.LogInfo("F11 pressed - forcing injection");
                ProperPropInjector.Reset();
                ProperPropInjector.TryInjectNow();
            }

            // F12 - Check status
            if (Input.GetKeyDown(KeyCode.F12))
            {
                CustomPropsPlugin.Log?.LogInfo("F12 pressed - checking status");
                Plugin?.CheckStatusPublic();
            }
        }

        void OnDestroy()
        {
            CustomPropsPlugin.Log?.LogInfo("[Helper] Destroyed");
        }
    }

    [BepInPlugin(GUID, NAME, VERSION)]
    public class CustomPropsPlugin : BaseUnityPlugin
    {
        public const string GUID = "com.endstar.customprops";
        public const string NAME = "Custom Props";
        public const string VERSION = "1.3.0"; // V4: Option B - Direct UI list manipulation

        public static ManualLogSource Log { get; private set; }
        public static CustomPropsPlugin Instance { get; private set; }
        public static AssetBundle PropsBundle { get; private set; }

        private Harmony _harmony;
        private static PropSearchHelper _helper;

        private void Awake()
        {
            Instance = this;
            Log = Logger;
            Log.LogInfo($"=== {NAME} v{VERSION} (V3: Singleton+Events) ===");

            // Initialize persistence system
            var pluginFolder = Path.GetDirectoryName(Info.Location);
            PropPersistence.Initialize(pluginFolder);
            Log.LogInfo("PropPersistence initialized");

            // REMOVED: Heavy startup analysis that was slowing things down
            // - AnalyzeGameLibraries()
            // - DeepDLLAnalyzer.AnalyzePropSystem()
            // - ComprehensiveDLLReader.ReadAllPropRelatedCode()

            // Load asset bundle (optional)
            if (!LoadAssetBundle())
            {
                Log.LogWarning("Asset bundle not found - using test prop only");
            }
            else
            {
                LoadPropDefinitions();
            }

            // Initialize integration
            PropIntegration.Initialize();

            // Register test prop
            RegisterTestProp();

            // Register props with game
            RegisterPropsWithGame();

            // V4: OPTION B - Enable Harmony patches for OnLibraryRepopulated postfix
            try
            {
                _harmony = new Harmony(GUID);

                // Initialize ProperPropInjector (caches types for Option B)
                Log.LogInfo("Initializing ProperPropInjector V4...");
                ProperPropInjector.Initialize();

                // Apply Option B Harmony patch (postfix on OnLibraryRepopulated)
                Log.LogInfo("Applying Option B Harmony patch...");
                ProperPropInjector.ApplyHarmonyPatches(_harmony);

                Log.LogInfo("Option B Harmony patch applied - custom props will be added to UI after Synchronize!");
            }
            catch (Exception ex)
            {
                Log.LogError($"Harmony patch failed: {ex.Message}");
                Log.LogError(ex.StackTrace);
            }

            Log.LogInfo($"{NAME} initialized with {PropRegistry.Count} props!");

            // Subscribe to scene events
            SceneManager.sceneLoaded += OnSceneLoaded;
            Log.LogInfo("Subscribed to SceneManager.sceneLoaded");
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Log.LogInfo($"=== Scene loaded: {scene.name} ===");

            // Create helper if not exists
            if (_helper == null)
            {
                Log.LogInfo("Creating PropSearchHelper...");
                var helperGO = new GameObject("CustomPropsHelper");
                UnityEngine.Object.DontDestroyOnLoad(helperGO);
                _helper = helperGO.AddComponent<PropSearchHelper>();
                _helper.Plugin = this;
            }

            // Reset injection state for new scene
            ProperPropInjector.Reset();

            // REMOVED: Immediate injection calls here
            // Let the Harmony patches handle injection timing naturally
        }

        // REMOVED: DelayedPropLibrarySearch and PropLibrarySearchCoroutine
        // These were causing race conditions

        private bool LoadAssetBundle()
        {
            string pluginDir = Path.GetDirectoryName(Info.Location);
            string bundlePath = Path.Combine(pluginDir, "custom_props.bundle");

            if (!File.Exists(bundlePath))
            {
                Log.LogWarning($"Bundle not found: {bundlePath}");
                return false;
            }

            try
            {
                PropsBundle = AssetBundle.LoadFromFile(bundlePath);
                if (PropsBundle == null)
                {
                    Log.LogError("AssetBundle.LoadFromFile returned null");
                    return false;
                }

                // Log all asset names in bundle
                var assetNames = PropsBundle.GetAllAssetNames();
                Log.LogInfo($"Bundle loaded with {assetNames.Length} assets:");
                foreach (var name in assetNames)
                {
                    Log.LogInfo($"  - {name}");
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.LogError($"Failed to load bundle: {ex.Message}");
                return false;
            }
        }

        private void RegisterTestProp()
        {
            // If bundle is loaded, use assets from bundle
            if (PropsBundle != null)
            {
                LoadPropsFromBundle();
                return;
            }

            // Fallback: create test cube if no bundle
            Log.LogWarning("No bundle loaded - creating fallback test prop");
            var testPrefab = GameObject.CreatePrimitive(PrimitiveType.Cube);
            testPrefab.name = "TestProp_PearlBasket";
            testPrefab.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

            var renderer = testPrefab.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = Color.cyan;
            }

            testPrefab.SetActive(false);
            UnityEngine.Object.DontDestroyOnLoad(testPrefab);

            var icon = CreateRuntimeIcon(Color.cyan, "PearlBasket");

            PropRegistry.Register(new PropData
            {
                PropId = "custom_pearl_basket",
                DisplayName = "Pearl Basket",
                Prefab = testPrefab,
                Icon = icon,
                Value = 100,
                Behavior = PropBehaviorType.Treasure
            });

            Log.LogInfo($"Registered fallback test prop: custom_pearl_basket");
        }

        private void LoadPropsFromBundle()
        {
            Log.LogInfo("=== Loading props from bundle ===");

            try
            {
                // Asset paths from Addressables catalog
                string prefabPath = "assets/pearl basket/pearlbasket.prefab";
                string iconPath = "assets/pearl basket/pearl basket icon.png";
                string definitionPath = "assets/pearl basket/newcustomprop.asset";

                // Load prefab
                var prefab = PropsBundle.LoadAsset<GameObject>(prefabPath);
                if (prefab == null)
                {
                    Log.LogError($"Failed to load prefab: {prefabPath}");
                    // Try alternate paths
                    var allNames = PropsBundle.GetAllAssetNames();
                    foreach (var name in allNames)
                    {
                        if (name.EndsWith(".prefab"))
                        {
                            prefab = PropsBundle.LoadAsset<GameObject>(name);
                            Log.LogInfo($"Loaded prefab from: {name}");
                            break;
                        }
                    }
                }
                else
                {
                    Log.LogInfo($"Loaded prefab: {prefab.name}");
                }

                // Load icon sprite
                Sprite icon = null;
                var iconTexture = PropsBundle.LoadAsset<Texture2D>(iconPath);
                if (iconTexture != null)
                {
                    icon = Sprite.Create(iconTexture, new Rect(0, 0, iconTexture.width, iconTexture.height), new Vector2(0.5f, 0.5f));
                    Log.LogInfo($"Loaded icon: {iconTexture.name}");
                }
                else
                {
                    // Try to load as sprite directly
                    icon = PropsBundle.LoadAsset<Sprite>(iconPath);
                    if (icon != null)
                    {
                        Log.LogInfo($"Loaded icon sprite: {icon.name}");
                    }
                    else
                    {
                        Log.LogWarning($"Failed to load icon: {iconPath}");
                    }
                }

                // Load CustomPropDefinition (optional - for metadata)
                var definition = PropsBundle.LoadAsset<ScriptableObject>(definitionPath);
                if (definition != null)
                {
                    Log.LogInfo($"Loaded definition: {definition.name}");
                    // Extract data from definition
                    var propIdField = definition.GetType().GetField("PropId");
                    var displayNameField = definition.GetType().GetField("DisplayName");
                    var valueField = definition.GetType().GetField("Value");

                    string propId = propIdField?.GetValue(definition)?.ToString() ?? "pearl_basket";
                    string displayName = displayNameField?.GetValue(definition)?.ToString() ?? "Pearl Basket";
                    int value = (int)(valueField?.GetValue(definition) ?? 100);

                    Log.LogInfo($"Definition data: PropId={propId}, DisplayName={displayName}, Value={value}");
                }

                if (prefab != null)
                {
                    // Make prefab persistent
                    UnityEngine.Object.DontDestroyOnLoad(prefab);

                    PropRegistry.Register(new PropData
                    {
                        PropId = "pearl_basket",
                        DisplayName = "Pearl Basket",
                        Prefab = prefab,
                        Icon = icon,
                        Value = 100,
                        Behavior = PropBehaviorType.Treasure
                    });

                    Log.LogInfo("=== Registered Pearl Basket from bundle! ===");
                }
                else
                {
                    Log.LogError("No prefab loaded - cannot register prop");
                }
            }
            catch (Exception ex)
            {
                Log.LogError($"LoadPropsFromBundle failed: {ex.Message}");
                Log.LogError(ex.StackTrace);
            }
        }

        private Sprite CreateRuntimeIcon(Color color, string name)
        {
            try
            {
                int size = 128;
                var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
                texture.name = $"Icon_{name}";

                Color borderColor = new Color(color.r * 0.5f, color.g * 0.5f, color.b * 0.5f, 1f);
                int borderWidth = 4;

                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        bool isBorder = x < borderWidth || x >= size - borderWidth ||
                                       y < borderWidth || y >= size - borderWidth;
                        float gradient = 1f - (float)y / size * 0.3f;
                        Color pixelColor = isBorder ? borderColor :
                            new Color(color.r * gradient, color.g * gradient, color.b * gradient, 1f);
                        texture.SetPixel(x, y, pixelColor);
                    }
                }

                texture.Apply();

                var sprite = Sprite.Create(
                    texture,
                    new Rect(0, 0, size, size),
                    new Vector2(0.5f, 0.5f),
                    100f
                );
                sprite.name = $"Sprite_{name}";

                UnityEngine.Object.DontDestroyOnLoad(texture);
                return sprite;
            }
            catch (Exception ex)
            {
                Log.LogError($"Failed to create icon: {ex.Message}");
                return null;
            }
        }

        private void LoadPropDefinitions()
        {
            if (PropsBundle == null) return;

            try
            {
                var definitions = PropsBundle.LoadAllAssets<ScriptableObject>();
                Log.LogInfo($"Found {definitions.Length} ScriptableObjects in bundle");

                foreach (var def in definitions)
                {
                    var propId = GetFieldValue<string>(def, "PropId");
                    var displayName = GetFieldValue<string>(def, "DisplayName");
                    var prefab = GetFieldValue<GameObject>(def, "Prefab");
                    var icon = GetFieldValue<Sprite>(def, "Icon");
                    var value = GetFieldValue<int>(def, "Value");

                    if (!string.IsNullOrEmpty(propId) && prefab != null)
                    {
                        PropRegistry.Register(new PropData
                        {
                            PropId = propId,
                            DisplayName = displayName ?? propId,
                            Prefab = prefab,
                            Icon = icon,
                            Value = value
                        });
                        Log.LogInfo($"Registered: {propId}");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.LogError($"Failed to load definitions: {ex.Message}");
            }
        }

        private void RegisterPropsWithGame()
        {
            foreach (var prop in PropRegistry.GetAll())
            {
                if (prop.Prefab == null) continue;
                PropIntegration.RegisterProp(prop);
            }
        }

        private T GetFieldValue<T>(object obj, string fieldName)
        {
            try
            {
                var field = obj.GetType().GetField(fieldName);
                if (field != null) return (T)field.GetValue(obj);

                var prop = obj.GetType().GetProperty(fieldName);
                if (prop != null) return (T)prop.GetValue(obj);
            }
            catch { }
            return default;
        }

        // SIMPLIFIED Update - much longer interval, only when not injected
        private float _injectionCheckTimer = 0f;
        private const float INJECTION_CHECK_INTERVAL = 10f; // Changed from 3s to 10s
        private bool _injectionLogged = false;

        private void Update()
        {
            // Skip entirely if injection is complete
            if (ProperPropInjector.IsInjectionComplete)
            {
                if (!_injectionLogged)
                {
                    Log.LogInfo("Injection complete - Update checks stopped");
                    _injectionLogged = true;
                }
                return;
            }

            // Much longer interval to avoid interference
            _injectionCheckTimer += Time.deltaTime;
            if (_injectionCheckTimer >= INJECTION_CHECK_INTERVAL)
            {
                _injectionCheckTimer = 0f;
                // Let Harmony patches handle injection - don't force it here
                // Only try if we haven't injected yet after a long time
            }
        }

        // Public methods for helper
        public void SpawnTestPropPublic() => SpawnTestProp();
        public void ListRegisteredPropsPublic() => ListRegisteredProps();
        public void CheckStatusPublic() => CheckStatus();

        private void CheckStatus()
        {
            Log.LogInfo("=== Plugin Status ===");
            Log.LogInfo($"Props registered: {PropRegistry.Count}");
            Log.LogInfo($"Injection complete: {ProperPropInjector.IsInjectionComplete}");

            // Check if PropLibrary exists
            if (PropIntegration.PropLibraryType != null)
            {
                var propLib = UnityEngine.Object.FindObjectOfType(PropIntegration.PropLibraryType);
                Log.LogInfo($"PropLibrary in scene: {propLib != null}");
            }

            // Check StageManager.injectedProps
            Log.LogInfo("Checking injectedProps...");
            try
            {
                var gameplayAsm = AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(a => a.GetName().Name == "Gameplay");
                Log.LogInfo($"Gameplay assembly: {gameplayAsm != null}");

                var stageManagerType = gameplayAsm?.GetType("Endless.Gameplay.LevelEditing.Level.StageManager");
                Log.LogInfo($"StageManager type: {stageManagerType != null}");

                var instanceProp = stageManagerType?.GetProperty("Instance",
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
                Log.LogInfo($"Instance prop: {instanceProp != null}");

                var stageManager = instanceProp?.GetValue(null);
                Log.LogInfo($"StageManager instance: {stageManager != null}");

                if (stageManager != null)
                {
                    Log.LogInfo("=== StageManager.injectedProps ===");
                    var injectedPropsField = stageManagerType.GetField("injectedProps",
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                    if (injectedPropsField != null)
                    {
                        var injectedProps = injectedPropsField.GetValue(stageManager);
                        if (injectedProps != null)
                        {
                            var listType = injectedProps.GetType();
                            var countProp = listType.GetProperty("Count");
                            var count = (int)(countProp?.GetValue(injectedProps) ?? 0);
                            Log.LogInfo($"injectedProps count: {count}");

                            // Iterate and log each
                            if (count > 0)
                            {
                                var indexer = listType.GetProperty("Item");
                                for (int i = 0; i < count; i++)
                                {
                                    var item = indexer?.GetValue(injectedProps, new object[] { i });
                                    if (item != null)
                                    {
                                        var propField = item.GetType().GetField("Prop");
                                        var prefabField = item.GetType().GetField("TestPrefab");
                                        var prop = propField?.GetValue(item);
                                        var prefab = prefabField?.GetValue(item);

                                        string propName = "?";
                                        if (prop != null)
                                        {
                                            var nameField = prop.GetType().GetField("Name",
                                                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                                            propName = nameField?.GetValue(prop)?.ToString() ?? "?";
                                        }

                                        Log.LogInfo($"  [{i}] Prop: {propName}, Prefab: {(prefab != null ? ((UnityEngine.Object)prefab).name : "null")}");
                                    }
                                }
                            }
                        }
                        else
                        {
                            Log.LogInfo("injectedProps is null");
                        }
                    }
                    else
                    {
                        Log.LogWarning("injectedProps field not found");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.LogError($"CheckStatus error: {ex.Message}");
            }
        }

        private void SpawnTestProp()
        {
            var props = PropRegistry.GetAll();
            if (props.Count == 0)
            {
                Log.LogWarning("No props registered");
                return;
            }

            PropData propToSpawn = null;
            foreach (var p in props) { propToSpawn = p; break; }

            var cam = Camera.main;
            if (cam == null)
            {
                Log.LogWarning("No main camera");
                return;
            }

            Vector3 spawnPos = cam.transform.position + cam.transform.forward * 3f;
            var spawned = PropRegistry.Spawn(propToSpawn.PropId, spawnPos, Quaternion.identity);

            if (spawned != null)
            {
                Log.LogInfo($"Spawned {propToSpawn.PropId} at {spawnPos}");
            }
        }

        private void ListRegisteredProps()
        {
            Log.LogInfo($"=== Registered Props ({PropRegistry.Count}) ===");
            foreach (var prop in PropRegistry.GetAll())
            {
                Log.LogInfo($"  {prop.PropId}: {prop.DisplayName}");
            }
        }

        private void OnDestroy()
        {
            _harmony?.UnpatchSelf();
            PropsBundle?.Unload(false);
        }

        // REMOVED: AnalyzeGameLibraries() - was causing slow startup
        // REMOVED: AnalyzeTypeComprehensive() - part of removed analysis
        // REMOVED: ForceAnalyzePropSystem() - was directly manipulating PropLibrary
    }
}

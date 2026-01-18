using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

namespace CustomProps
{
    /// <summary>
    /// V4 - Clean implementation based on runtime analysis findings.
    /// Uses DirectPropInjector which was built from actual runtime data capture.
    /// </summary>
    [BepInPlugin("com.endstar.customprops", "Custom Props", "2.0.0")]
    public class CustomPropsPlugin : BaseUnityPlugin
    {
        public static ManualLogSource Log { get; private set; }
        public static CustomPropsPlugin Instance { get; private set; }

        private DirectPropInjector _injector;
        private InjectorRunner _runner;
        private Harmony _harmony;
        private bool _injectionAttempted = false;

        private void Awake()
        {
            Instance = this;
            Log = Logger;
            Log.LogInfo("=== Custom Props v2.0.0 (V4: Direct Injection) ===");

            // Initialize injector
            _injector = new DirectPropInjector();
            if (!_injector.Initialize())
            {
                Log.LogError("Failed to initialize DirectPropInjector");
                return;
            }

            // Apply Harmony patches to prevent mesh merging
            try
            {
                _harmony = new Harmony("com.endstar.customprops");
                Log.LogInfo("Applying BuildPrefabPatch...");
                BuildPrefabPatch.ApplyPatches(_harmony);
                Log.LogInfo("Harmony patches applied");
            }
            catch (Exception ex)
            {
                Log.LogError($"Failed to apply Harmony patches: {ex.Message}");
            }

            // Subscribe to scene events
            SceneManager.sceneLoaded += OnSceneLoaded;
            Log.LogInfo("Ready - will inject when props are loaded");
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Log.LogInfo($"=== Scene loaded: {scene.name} ===");

            // Create runner if not exists
            if (_runner == null)
            {
                var go = new GameObject("CustomPropsRunner");
                UnityEngine.Object.DontDestroyOnLoad(go);
                _runner = go.AddComponent<InjectorRunner>();
                _runner.Init(this, _injector);
            }

            // Reset injection flag for new scene
            _injectionAttempted = false;
        }

        public void TryInjectTestProp()
        {
            if (_injectionAttempted)
            {
                Log.LogInfo("Injection already attempted");
                return;
            }

            int propCount = _injector.GetLoadedPropCount();
            Log.LogInfo($"Current prop count: {propCount}");

            if (propCount <= 0)
            {
                Log.LogWarning("Props not loaded yet");
                return;
            }

            _injectionAttempted = true;

            // Generate unique GUID for our prop
            string propId = Guid.NewGuid().ToString();
            string propName = "Pearl Basket";

            Log.LogInfo($"Attempting to inject: {propName}");

            bool success = _injector.InjectProp(propId, propName);

            if (success)
            {
                Log.LogInfo($"SUCCESS! Injected {propName}");
                Log.LogInfo($"New prop count: {_injector.GetLoadedPropCount()}");
            }
            else
            {
                Log.LogError($"FAILED to inject {propName}");
            }
        }
    }

    /// <summary>
    /// MonoBehaviour to run injection logic on main thread
    /// </summary>
    public class InjectorRunner : MonoBehaviour
    {
        private CustomPropsPlugin _plugin;
        private DirectPropInjector _injector;
        private float _timer = 0f;
        private float _lastCheckTime = 0f;
        private bool _injected = false;
        private int _lastPropCount = -1;

        public void Init(CustomPropsPlugin plugin, DirectPropInjector injector)
        {
            _plugin = plugin;
            _injector = injector;
            CustomPropsPlugin.Log.LogInfo("[InjectorRunner] Created - polling for props every 5s, F11 to force inject");
        }

        private void Update()
        {
            _timer += Time.deltaTime;

            // F11 - Manual inject
            if (Input.GetKeyDown(KeyCode.F11))
            {
                CustomPropsPlugin.Log.LogInfo("[InjectorRunner] F11 pressed - forcing injection");
                _plugin.TryInjectTestProp();
            }

            // F12 - Check status
            if (Input.GetKeyDown(KeyCode.F12))
            {
                int count = _injector.GetLoadedPropCount();
                CustomPropsPlugin.Log.LogInfo($"[InjectorRunner] F12 - Prop count: {count}, Injected: {_injected}");
            }

            // Auto-check every 5 seconds
            if (!_injected && _timer - _lastCheckTime >= 5f)
            {
                _lastCheckTime = _timer;
                int currentCount = _injector.GetLoadedPropCount();

                if (currentCount != _lastPropCount)
                {
                    CustomPropsPlugin.Log.LogInfo($"[InjectorRunner] Prop count changed: {_lastPropCount} -> {currentCount}");
                    _lastPropCount = currentCount;

                    // If props are now loaded, try to inject
                    if (currentCount > 0 && !_injected)
                    {
                        CustomPropsPlugin.Log.LogInfo("[InjectorRunner] Props loaded! Attempting injection...");
                        _plugin.TryInjectTestProp();
                        _injected = true;
                    }
                }
            }
        }
    }
}

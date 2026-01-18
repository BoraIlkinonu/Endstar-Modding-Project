using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace SwimmingSystem
{
    [BepInPlugin("com.endstar.swimmingsystem", "SwimmingSystem", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log;
        internal static string BundlePath;
        internal static AssetBundle AnimationBundle;
        internal static Dictionary<string, AnimationClip> SwimClips = new Dictionary<string, AnimationClip>();

        internal static bool _managerCreated = false;
        private float _checkTimer = 0f;
        private int _checkCount = 0;

        private void Awake()
        {
            Log = Logger;
            BundlePath = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                SwimConfig.BundleFileName
            );

            Log.LogWarning("╔════════════════════════════════════════════════════════════╗");
            Log.LogWarning("║         SwimmingSystem v1.0.0 - Zayed Character Mod        ║");
            Log.LogWarning("╚════════════════════════════════════════════════════════════╝");
            Log.LogWarning($"[INIT] Bundle path: {BundlePath}");
            Log.LogWarning($"[INIT] Bundle exists: {File.Exists(BundlePath)}");

            // Load animation bundle
            LoadAnimationBundle();

            // Subscribe to scene events
            SceneManager.sceneLoaded += OnSceneLoaded;
            Log.LogWarning("[INIT] Subscribed to sceneLoaded event");

            // Check if we're already in a scene (late load)
            var currentScene = SceneManager.GetActiveScene();
            Log.LogWarning($"[INIT] Current scene: '{currentScene.name}'");
            if (currentScene.name == SwimConfig.GameplaySceneName)
            {
                Log.LogWarning("[INIT] Already in gameplay scene! Initializing now...");
                CreateSwimmingManager();
            }

            // Create a polling object since BepInEx plugins don't get Update() calls
            Log.LogWarning("[INIT] Creating scene poller GameObject...");
            var pollerGO = new GameObject("SwimmingSystemPoller");
            Object.DontDestroyOnLoad(pollerGO);
            pollerGO.AddComponent<ScenePoller>();
            Log.LogWarning("[INIT] Scene poller created!");
        }

        private void CreateSwimmingManager()
        {
            if (_managerCreated)
            {
                Log.LogWarning("[CREATE] Manager already created, skipping.");
                return;
            }

            try
            {
                Log.LogWarning("[CREATE] Creating SwimmingSystemManager...");
                var managerGO = new GameObject("SwimmingSystemManager");
                Object.DontDestroyOnLoad(managerGO);
                managerGO.AddComponent<SwimmingManager>();
                _managerCreated = true;
                Log.LogWarning("[CREATE] SwimmingManager created successfully!");
            }
            catch (System.Exception ex)
            {
                Log.LogError($"[CREATE] EXCEPTION: {ex.Message}");
                Log.LogError($"[CREATE] Stack: {ex.StackTrace}");
            }
        }

        private void LoadAnimationBundle()
        {
            if (!File.Exists(BundlePath))
            {
                Log.LogError($"[BUNDLE] Animation bundle not found at: {BundlePath}");
                return;
            }

            try
            {
                AnimationBundle = AssetBundle.LoadFromFile(BundlePath);
                if (AnimationBundle == null)
                {
                    Log.LogError("[BUNDLE] Failed to load animation bundle!");
                    return;
                }

                var clips = AnimationBundle.LoadAllAssets<AnimationClip>();
                Log.LogWarning($"[BUNDLE] Loaded {clips.Length} animation clips:");

                foreach (var clip in clips)
                {
                    SwimClips[clip.name] = clip;
                    Log.LogWarning($"[BUNDLE]   - {clip.name} (legacy={clip.legacy}, length={clip.length:F2}s)");

                    if (!clip.legacy)
                    {
                        Log.LogError($"[BUNDLE]   WARNING: {clip.name} is NOT legacy! Animation may not work.");
                    }
                }

                // Verify required clips
                VerifyRequiredClip(SwimConfig.SwimmingClipName);
                VerifyRequiredClip(SwimConfig.TreadingClipName);
                VerifyRequiredClip(SwimConfig.DiveEntryClipName);
            }
            catch (System.Exception ex)
            {
                Log.LogError($"[BUNDLE] Exception loading bundle: {ex.Message}");
            }
        }

        private void VerifyRequiredClip(string clipName)
        {
            if (SwimClips.ContainsKey(clipName))
            {
                Log.LogWarning($"[BUNDLE] Required clip '{clipName}' found.");
            }
            else
            {
                Log.LogError($"[BUNDLE] MISSING required clip: '{clipName}'!");
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            try
            {
                Log.LogWarning($"[SCENE] ═══════════════════════════════════════════════════════");
                Log.LogWarning($"[SCENE] Scene loaded: '{scene.name}' (mode: {mode})");

                // Only activate in gameplay mode (MainScene), not in Creator
                if (scene.name != SwimConfig.GameplaySceneName)
                {
                    Log.LogWarning($"[SCENE] Not gameplay scene ('{scene.name}' != '{SwimConfig.GameplaySceneName}'), swimming system inactive.");
                    return;
                }

                Log.LogWarning($"[SCENE] Gameplay scene detected via callback!");
                CreateSwimmingManager();
            }
            catch (System.Exception ex)
            {
                Log.LogError($"[SCENE] EXCEPTION in OnSceneLoaded: {ex.Message}");
                Log.LogError($"[SCENE] Stack: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// Get a swimming animation clip by name
        /// </summary>
        public static AnimationClip GetClip(string name)
        {
            if (SwimClips.TryGetValue(name, out var clip))
            {
                return clip;
            }
            Log.LogError($"[CLIP] Clip not found: {name}");
            return null;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (AnimationBundle != null)
            {
                AnimationBundle.Unload(false);
            }
        }
    }

    /// <summary>
    /// Separate MonoBehaviour to poll for scene changes since BepInEx plugins don't get Update()
    /// </summary>
    public class ScenePoller : MonoBehaviour
    {
        private float _checkTimer = 0f;
        private int _checkCount = 0;

        void Update()
        {
            if (Plugin._managerCreated) return;

            _checkTimer += Time.deltaTime;
            if (_checkTimer >= 2f)
            {
                _checkTimer = 0f;
                _checkCount++;

                var scene = SceneManager.GetActiveScene();
                if (_checkCount <= 10)
                {
                    Plugin.Log.LogWarning($"[POLLER] Check #{_checkCount}: scene='{scene.name}' (looking for '{SwimConfig.GameplaySceneName}')");
                }

                if (scene.name == SwimConfig.GameplaySceneName && !Plugin._managerCreated)
                {
                    Plugin.Log.LogWarning("[POLLER] Found gameplay scene! Creating manager...");
                    CreateManager();
                }
            }
        }

        private void CreateManager()
        {
            if (Plugin._managerCreated) return;

            try
            {
                Plugin.Log.LogWarning("[POLLER] Creating SwimmingSystemManager...");
                var managerGO = new GameObject("SwimmingSystemManager");
                Object.DontDestroyOnLoad(managerGO);
                managerGO.AddComponent<SwimmingManager>();
                Plugin._managerCreated = true;
                Plugin.Log.LogWarning("[POLLER] SwimmingManager created successfully!");
            }
            catch (System.Exception ex)
            {
                Plugin.Log.LogError($"[POLLER] EXCEPTION: {ex.Message}");
            }
        }
    }
}

using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Text;

namespace AnimationInjector
{
    [BepInPlugin("com.endstar.animationinjector", "AnimationInjector", "7.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log;
        internal static string BundlePath;

        private void Awake()
        {
            Log = Logger;
            BundlePath = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "generic_animations.bundle"
            );

            Logger.LogWarning("╔════════════════════════════════════════════════════════════╗");
            Logger.LogWarning("║  AnimationInjector v7.0.0 - DETAILED DIAGNOSTIC LOGGING    ║");
            Logger.LogWarning("╚════════════════════════════════════════════════════════════╝");
            Logger.LogWarning($"[INIT] Bundle path: {BundlePath}");
            Logger.LogWarning($"[INIT] Bundle exists: {File.Exists(BundlePath)}");
            Logger.LogWarning($"[INIT] Time: {System.DateTime.Now:HH:mm:ss.fff}");

            SceneManager.sceneLoaded += OnSceneLoaded;
            Logger.LogWarning("[INIT] Subscribed to sceneLoaded event");
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Log.LogWarning($"[SCENE] ═══════════════════════════════════════════════════════");
            Log.LogWarning($"[SCENE] Scene Loaded: {scene.name} (mode: {mode})");
            Log.LogWarning($"[SCENE] Time: {System.DateTime.Now:HH:mm:ss.fff}");

            var go = new GameObject("AnimationInjectorRunner");
            Object.DontDestroyOnLoad(go);
            var runner = go.AddComponent<AnimationRunner>();
            runner.Init();

            Log.LogWarning("[SCENE] AnimationRunner GameObject created and initialized");
        }
    }

    public class AnimationRunner : MonoBehaviour
    {
        // Bundle/Clip state
        private static bool _bundleLoaded = false;
        private static AssetBundle _bundle = null;
        private static List<AnimationClip> _clips = new List<AnimationClip>();
        private static int _currentIndex = -1;

        // Timing
        private float _timer = 0f;
        private float _lastTickLog = -10f;
        private bool _initialized = false;
        private float _initTime = 0f;

        // Animation playback
        private Transform _animationRoot = null;
        private Animator _gameAnimator = null;
        private bool _isPlayingCustom = false;
        private AnimationClip _currentClip = null;
        private float _playbackTime = 0f;
        private int _frameCount = 0;

        // Bone tracking
        private Dictionary<string, Transform> _boneCache = new Dictionary<string, Transform>();
        private Dictionary<string, BoneSnapshot> _originalPoses = new Dictionary<string, BoneSnapshot>();
        private Dictionary<string, BoneSnapshot> _lastFramePoses = new Dictionary<string, BoneSnapshot>();

        // Diagnostic tracking
        private bool _autoTestDone = false;
        private List<string> _keyPressLog = new List<string>();
        private int _sampleCallCount = 0;
        private bool _clipBindingsLogged = false;

        // SkinnedMeshRenderer tracking
        private List<SkinnedMeshRenderer> _trackedMeshes = new List<SkinnedMeshRenderer>();

        private struct BoneSnapshot
        {
            public Vector3 localPosition;
            public Quaternion localRotation;
            public Vector3 localScale;
            public Vector3 worldPosition;
        }

        public void Init()
        {
            if (_initialized) return;
            _initialized = true;
            _initTime = Time.time;

            Plugin.Log.LogWarning("╔════════════════════════════════════════════════════════════╗");
            Plugin.Log.LogWarning("║              ANIMATION RUNNER INITIALIZED                  ║");
            Plugin.Log.LogWarning("╚════════════════════════════════════════════════════════════╝");
            Plugin.Log.LogWarning("[RUNNER] Version: 7.0.0 - Detailed Diagnostic Logging");
            Plugin.Log.LogWarning("[RUNNER] HOTKEYS:");
            Plugin.Log.LogWarning("[RUNNER]   Numpad1 = Play Next Animation");
            Plugin.Log.LogWarning("[RUNNER]   Numpad2 = Play Previous Animation");
            Plugin.Log.LogWarning("[RUNNER]   Numpad3 = Stop Animation");
            Plugin.Log.LogWarning("[RUNNER]   Numpad4 = Test All Roots");
            Plugin.Log.LogWarning("[RUNNER]   Numpad5 = Log Clip Bindings (curve paths)");
            Plugin.Log.LogWarning("[RUNNER]   Numpad6 = Log All Bone Positions");
            Plugin.Log.LogWarning("[RUNNER]   Numpad0 = Log Full Hierarchy");
            Plugin.Log.LogWarning("[RUNNER] AUTO-TEST: Will auto-play first clip 5s after character found");

            LoadBundle();
        }

        private void LoadBundle()
        {
            if (_bundleLoaded) return;
            _bundleLoaded = true;

            Plugin.Log.LogWarning("[BUNDLE] ═══════════════════════════════════════════════════════");
            Plugin.Log.LogWarning($"[BUNDLE] Loading from: {Plugin.BundlePath}");

            if (!File.Exists(Plugin.BundlePath))
            {
                Plugin.Log.LogError("[BUNDLE] ERROR: Bundle file NOT FOUND!");
                return;
            }

            var fileInfo = new FileInfo(Plugin.BundlePath);
            Plugin.Log.LogWarning($"[BUNDLE] File size: {fileInfo.Length} bytes");
            Plugin.Log.LogWarning($"[BUNDLE] Last modified: {fileInfo.LastWriteTime:yyyy-MM-dd HH:mm:ss}");

            try
            {
                _bundle = AssetBundle.LoadFromFile(Plugin.BundlePath);
                if (_bundle == null)
                {
                    Plugin.Log.LogError("[BUNDLE] ERROR: AssetBundle.LoadFromFile returned NULL!");
                    return;
                }

                Plugin.Log.LogWarning($"[BUNDLE] Bundle loaded successfully");

                // List all assets in bundle
                var allAssets = _bundle.GetAllAssetNames();
                Plugin.Log.LogWarning($"[BUNDLE] All asset names in bundle ({allAssets.Length}):");
                foreach (var name in allAssets)
                {
                    Plugin.Log.LogWarning($"[BUNDLE]   - {name}");
                }

                var bundleClips = _bundle.LoadAllAssets<AnimationClip>();
                Plugin.Log.LogWarning($"[BUNDLE] ───────────────────────────────────────────────────────");
                Plugin.Log.LogWarning($"[BUNDLE] Found {bundleClips.Length} AnimationClips:");

                foreach (var clip in bundleClips)
                {
                    _clips.Add(clip);
                    int idx = _clips.Count - 1;

                    Plugin.Log.LogWarning($"[CLIP {idx}] ─────────────────────────────────────────────────");
                    Plugin.Log.LogWarning($"[CLIP {idx}] Name: {clip.name}");
                    Plugin.Log.LogWarning($"[CLIP {idx}] Legacy: {clip.legacy}");
                    Plugin.Log.LogWarning($"[CLIP {idx}] Length: {clip.length:F3}s");
                    Plugin.Log.LogWarning($"[CLIP {idx}] FrameRate: {clip.frameRate}");
                    Plugin.Log.LogWarning($"[CLIP {idx}] WrapMode: {clip.wrapMode}");
                    Plugin.Log.LogWarning($"[CLIP {idx}] IsHumanMotion: {clip.isHumanMotion}");
                    Plugin.Log.LogWarning($"[CLIP {idx}] Empty: {clip.empty}");
                    Plugin.Log.LogWarning($"[CLIP {idx}] HasGenericRootTransform: {clip.hasGenericRootTransform}");
                    Plugin.Log.LogWarning($"[CLIP {idx}] HasMotionCurves: {clip.hasMotionCurves}");
                    Plugin.Log.LogWarning($"[CLIP {idx}] HasMotionFloatCurves: {clip.hasMotionFloatCurves}");
                    Plugin.Log.LogWarning($"[CLIP {idx}] HasRootCurves: {clip.hasRootCurves}");
                    Plugin.Log.LogWarning($"[CLIP {idx}] HumanMotion: {clip.humanMotion}");
                    Plugin.Log.LogWarning($"[CLIP {idx}] LocalBounds: {clip.localBounds}");

                    if (!clip.legacy)
                    {
                        Plugin.Log.LogError($"[CLIP {idx}] *** WARNING: NOT LEGACY! SampleAnimation will NOT work! ***");
                    }
                }

                Plugin.Log.LogWarning($"[BUNDLE] ═══════════════════════════════════════════════════════");
                if (_clips.Count > 0 && _clips.All(c => c.legacy))
                {
                    Plugin.Log.LogWarning("[BUNDLE] SUCCESS: All clips are legacy=true!");
                }
                else if (_clips.Count > 0)
                {
                    Plugin.Log.LogError("[BUNDLE] FAILURE: Some clips are NOT legacy!");
                }
            }
            catch (System.Exception ex)
            {
                Plugin.Log.LogError($"[BUNDLE] EXCEPTION: {ex.Message}");
                Plugin.Log.LogError($"[BUNDLE] Stack: {ex.StackTrace}");
            }
        }

        private void Update()
        {
            _timer += Time.deltaTime;
            _frameCount++;

            // Log tick every 10 seconds with detailed state
            if (_timer - _lastTickLog >= 10f)
            {
                _lastTickLog = _timer;
                LogDetailedTick();
            }

            // Try to find animation root if not found
            if (_animationRoot == null)
            {
                if (_frameCount % 60 == 0) // Every ~1 second
                {
                    FindAnimationRoot();
                }
            }

            // Auto-test: Play first clip 5 seconds after finding character
            if (!_autoTestDone && _animationRoot != null && _clips.Count > 0)
            {
                float timeSinceRoot = _timer - _initTime;
                if (timeSinceRoot > 5f)
                {
                    _autoTestDone = true;
                    Plugin.Log.LogWarning("[AUTO-TEST] ═══════════════════════════════════════════════════════");
                    Plugin.Log.LogWarning("[AUTO-TEST] Starting automatic playback test!");
                    Plugin.Log.LogWarning("[AUTO-TEST] This tests if animations work without manual input.");
                    _currentIndex = -1;
                    PlayNext();
                }
            }

            // Track ALL key presses
            CheckAllKeyPresses();

            // HOTKEYS
            if (Input.GetKeyDown(KeyCode.Keypad1))
            {
                LogKeyPress("Numpad1", "Play Next");
                PlayNext();
            }
            if (Input.GetKeyDown(KeyCode.Keypad2))
            {
                LogKeyPress("Numpad2", "Play Previous");
                PlayPrev();
            }
            if (Input.GetKeyDown(KeyCode.Keypad3))
            {
                LogKeyPress("Numpad3", "Stop");
                StopCustomAnimation();
            }
            if (Input.GetKeyDown(KeyCode.Keypad4))
            {
                LogKeyPress("Numpad4", "Test All Roots");
                TestDifferentRoots();
            }
            if (Input.GetKeyDown(KeyCode.Keypad5))
            {
                LogKeyPress("Numpad5", "Log Clip Bindings");
                LogAllClipBindings();
            }
            if (Input.GetKeyDown(KeyCode.Keypad6))
            {
                LogKeyPress("Numpad6", "Log Bone Positions");
                LogAllBonePositions();
            }
            if (Input.GetKeyDown(KeyCode.Keypad0))
            {
                LogKeyPress("Numpad0", "Log Hierarchy");
                LogFullHierarchy();
            }
        }

        private void CheckAllKeyPresses()
        {
            // Log any numpad key press for diagnostics
            for (int i = 0; i <= 9; i++)
            {
                KeyCode kc = (KeyCode)((int)KeyCode.Keypad0 + i);
                if (Input.GetKeyDown(kc))
                {
                    string entry = $"[{_timer:F2}s] Keypad{i} pressed";
                    _keyPressLog.Add(entry);
                }
            }

            // Also check regular number keys as backup
            for (int i = 0; i <= 9; i++)
            {
                KeyCode kc = (KeyCode)((int)KeyCode.Alpha0 + i);
                if (Input.GetKeyDown(kc))
                {
                    string entry = $"[{_timer:F2}s] Alpha{i} pressed";
                    _keyPressLog.Add(entry);
                }
            }
        }

        private void LogKeyPress(string key, string action)
        {
            Plugin.Log.LogWarning($"[KEYPRESS] ═══════════════════════════════════════════════════════");
            Plugin.Log.LogWarning($"[KEYPRESS] Key: {key} | Action: {action}");
            Plugin.Log.LogWarning($"[KEYPRESS] Time: {_timer:F2}s | Frame: {_frameCount}");
            Plugin.Log.LogWarning($"[KEYPRESS] DateTime: {System.DateTime.Now:HH:mm:ss.fff}");
        }

        private void LogDetailedTick()
        {
            Plugin.Log.LogWarning($"[TICK] ═══════════════════════════════════════════════════════");
            Plugin.Log.LogWarning($"[TICK] Time: {_timer:F1}s | Frame: {_frameCount} | DateTime: {System.DateTime.Now:HH:mm:ss}");
            Plugin.Log.LogWarning($"[TICK] Clips loaded: {_clips.Count}");
            Plugin.Log.LogWarning($"[TICK] AnimRoot: {(_animationRoot != null ? _animationRoot.name : "NULL")}");
            Plugin.Log.LogWarning($"[TICK] GameAnimator: {(_gameAnimator != null ? _gameAnimator.name : "NULL")}");
            Plugin.Log.LogWarning($"[TICK] IsPlayingCustom: {_isPlayingCustom}");
            Plugin.Log.LogWarning($"[TICK] CurrentClip: {(_currentClip != null ? _currentClip.name : "NULL")}");
            Plugin.Log.LogWarning($"[TICK] PlaybackTime: {_playbackTime:F3}s");
            Plugin.Log.LogWarning($"[TICK] SampleAnimation calls: {_sampleCallCount}");
            Plugin.Log.LogWarning($"[TICK] AutoTestDone: {_autoTestDone}");
            Plugin.Log.LogWarning($"[TICK] Key presses this session: {_keyPressLog.Count}");

            if (_gameAnimator != null)
            {
                Plugin.Log.LogWarning($"[TICK] Animator.enabled: {_gameAnimator.enabled}");
                Plugin.Log.LogWarning($"[TICK] Animator.isActiveAndEnabled: {_gameAnimator.isActiveAndEnabled}");
                Plugin.Log.LogWarning($"[TICK] Animator.runtimeAnimatorController: {(_gameAnimator.runtimeAnimatorController != null ? _gameAnimator.runtimeAnimatorController.name : "NULL")}");
            }

            // Log tracked bone changes
            if (_isPlayingCustom && _boneCache.Count > 0)
            {
                LogBoneChanges();
            }
        }

        private void LateUpdate()
        {
            if (_isPlayingCustom && _currentClip != null && _animationRoot != null)
            {
                float prevTime = _playbackTime;
                _playbackTime += Time.deltaTime;

                if (_playbackTime > _currentClip.length)
                {
                    _playbackTime = _playbackTime % _currentClip.length;
                    Plugin.Log.LogWarning($"[PLAYBACK] Animation looped. New time: {_playbackTime:F3}s");
                }

                // Store pre-sample positions
                StoreBoneSnapshot(_lastFramePoses);

                // Sample the animation
                _currentClip.SampleAnimation(_animationRoot.gameObject, _playbackTime);
                _sampleCallCount++;

                // Log every 30 frames (~0.5s at 60fps)
                if (_frameCount % 30 == 0)
                {
                    Plugin.Log.LogInfo($"[SAMPLE] t={_playbackTime:F3}s | calls={_sampleCallCount} | frame={_frameCount}");
                }
            }
        }

        private void StoreBoneSnapshot(Dictionary<string, BoneSnapshot> target)
        {
            target.Clear();
            foreach (var kvp in _boneCache)
            {
                if (kvp.Value != null)
                {
                    target[kvp.Key] = new BoneSnapshot
                    {
                        localPosition = kvp.Value.localPosition,
                        localRotation = kvp.Value.localRotation,
                        localScale = kvp.Value.localScale,
                        worldPosition = kvp.Value.position
                    };
                }
            }
        }

        private void LogBoneChanges()
        {
            int movedCount = 0;
            var movedBones = new List<string>();

            foreach (var kvp in _boneCache)
            {
                if (kvp.Value != null && _lastFramePoses.TryGetValue(kvp.Key, out BoneSnapshot lastPose))
                {
                    float posDiff = Vector3.Distance(kvp.Value.localPosition, lastPose.localPosition);
                    float rotDiff = Quaternion.Angle(kvp.Value.localRotation, lastPose.localRotation);

                    if (posDiff > 0.0001f || rotDiff > 0.01f)
                    {
                        movedCount++;
                        if (movedBones.Count < 5)
                        {
                            movedBones.Add($"{kvp.Key} (pos:{posDiff:F4}, rot:{rotDiff:F2}°)");
                        }
                    }
                }
            }

            Plugin.Log.LogWarning($"[BONES] Moved this tick: {movedCount}/{_boneCache.Count}");
            foreach (var bone in movedBones)
            {
                Plugin.Log.LogWarning($"[BONES]   {bone}");
            }
        }

        private void FindAnimationRoot()
        {
            var allMonoBehaviours = FindObjectsOfType<MonoBehaviour>();
            foreach (var mb in allMonoBehaviours)
            {
                if (mb.GetType().Name == "AppearanceAnimator")
                {
                    var animatorField = mb.GetType().GetField("animator",
                        BindingFlags.NonPublic | BindingFlags.Instance);

                    if (animatorField != null)
                    {
                        _gameAnimator = animatorField.GetValue(mb) as Animator;
                        if (_gameAnimator != null)
                        {
                            Plugin.Log.LogWarning("[FOUND] ═══════════════════════════════════════════════════════");
                            Plugin.Log.LogWarning($"[FOUND] AppearanceAnimator detected!");
                            Plugin.Log.LogWarning($"[FOUND] Time: {_timer:F2}s | Frame: {_frameCount}");

                            LogAnimatorDetails(_gameAnimator);

                            _animationRoot = FindAnimationRootTransform(_gameAnimator.transform);

                            if (_animationRoot != null)
                            {
                                Plugin.Log.LogWarning($"[FOUND] Animation root: {_animationRoot.name}");
                                Plugin.Log.LogWarning($"[FOUND] Full path: {GetFullPath(_animationRoot)}");

                                BuildBoneCache(_animationRoot);
                                Plugin.Log.LogWarning($"[FOUND] Built bone cache with {_boneCache.Count} bones");

                                FindAndLogSkinnedMeshRenderers();
                                LogClipBindingsVsGameBones();
                            }
                            return;
                        }
                    }
                }
            }
        }

        private void LogAnimatorDetails(Animator anim)
        {
            Plugin.Log.LogWarning($"[ANIMATOR] ─────────────────────────────────────────────────────");
            Plugin.Log.LogWarning($"[ANIMATOR] GameObject: {anim.gameObject.name}");
            Plugin.Log.LogWarning($"[ANIMATOR] Enabled: {anim.enabled}");
            Plugin.Log.LogWarning($"[ANIMATOR] IsActiveAndEnabled: {anim.isActiveAndEnabled}");
            Plugin.Log.LogWarning($"[ANIMATOR] UpdateMode: {anim.updateMode}");
            Plugin.Log.LogWarning($"[ANIMATOR] CullingMode: {anim.cullingMode}");
            Plugin.Log.LogWarning($"[ANIMATOR] HasRootMotion: {anim.hasRootMotion}");
            Plugin.Log.LogWarning($"[ANIMATOR] IsHuman: {anim.isHuman}");
            Plugin.Log.LogWarning($"[ANIMATOR] IsOptimizable: {anim.isOptimizable}");
            Plugin.Log.LogWarning($"[ANIMATOR] LayerCount: {anim.layerCount}");

            if (anim.runtimeAnimatorController != null)
            {
                Plugin.Log.LogWarning($"[ANIMATOR] Controller: {anim.runtimeAnimatorController.name}");
                Plugin.Log.LogWarning($"[ANIMATOR] Controller clips: {anim.runtimeAnimatorController.animationClips?.Length ?? 0}");
            }
            else
            {
                Plugin.Log.LogWarning($"[ANIMATOR] Controller: NULL");
            }

            if (anim.avatar != null)
            {
                Plugin.Log.LogWarning($"[ANIMATOR] Avatar: {anim.avatar.name}");
                Plugin.Log.LogWarning($"[ANIMATOR] Avatar.isHuman: {anim.avatar.isHuman}");
                Plugin.Log.LogWarning($"[ANIMATOR] Avatar.isValid: {anim.avatar.isValid}");
            }
            else
            {
                Plugin.Log.LogWarning($"[ANIMATOR] Avatar: NULL");
            }
        }

        private void FindAndLogSkinnedMeshRenderers()
        {
            _trackedMeshes.Clear();

            if (_animationRoot == null) return;

            var meshes = _animationRoot.GetComponentsInChildren<SkinnedMeshRenderer>();
            Plugin.Log.LogWarning($"[MESH] ─────────────────────────────────────────────────────");
            Plugin.Log.LogWarning($"[MESH] Found {meshes.Length} SkinnedMeshRenderers under animation root:");

            foreach (var mesh in meshes)
            {
                _trackedMeshes.Add(mesh);
                Plugin.Log.LogWarning($"[MESH]   {mesh.name}:");
                Plugin.Log.LogWarning($"[MESH]     - Bones: {mesh.bones?.Length ?? 0}");
                Plugin.Log.LogWarning($"[MESH]     - RootBone: {(mesh.rootBone != null ? mesh.rootBone.name : "NULL")}");
                Plugin.Log.LogWarning($"[MESH]     - SharedMesh: {(mesh.sharedMesh != null ? mesh.sharedMesh.name : "NULL")}");
                Plugin.Log.LogWarning($"[MESH]     - BlendShapes: {mesh.sharedMesh?.blendShapeCount ?? 0}");
                Plugin.Log.LogWarning($"[MESH]     - Enabled: {mesh.enabled}");
                Plugin.Log.LogWarning($"[MESH]     - UpdateWhenOffscreen: {mesh.updateWhenOffscreen}");

                if (mesh.bones != null && mesh.bones.Length > 0)
                {
                    Plugin.Log.LogWarning($"[MESH]     - First 5 bone names:");
                    for (int i = 0; i < Mathf.Min(5, mesh.bones.Length); i++)
                    {
                        var bone = mesh.bones[i];
                        Plugin.Log.LogWarning($"[MESH]       [{i}] {(bone != null ? bone.name : "NULL")}");
                    }
                }
            }
        }

        private void LogAllClipBindings()
        {
            Plugin.Log.LogWarning("[BINDINGS] ═══════════════════════════════════════════════════════");
            Plugin.Log.LogWarning("[BINDINGS] Animation Clip Curve Bindings (what paths clips target):");

            foreach (var clip in _clips)
            {
                Plugin.Log.LogWarning($"[BINDINGS] ─────────────────────────────────────────────────────");
                Plugin.Log.LogWarning($"[BINDINGS] Clip: {clip.name}");

                // Use AnimationUtility equivalent via reflection
                var curveBindings = GetCurveBindings(clip);

                if (curveBindings.Count == 0)
                {
                    Plugin.Log.LogWarning($"[BINDINGS]   (No bindings found - clip may be empty or inaccessible)");
                }
                else
                {
                    // Group by path
                    var pathGroups = curveBindings.GroupBy(b => b.path).OrderBy(g => g.Key);
                    Plugin.Log.LogWarning($"[BINDINGS]   Total bindings: {curveBindings.Count}");
                    Plugin.Log.LogWarning($"[BINDINGS]   Unique paths: {pathGroups.Count()}");

                    int pathCount = 0;
                    foreach (var group in pathGroups)
                    {
                        pathCount++;
                        if (pathCount <= 20) // Limit output
                        {
                            var properties = string.Join(", ", group.Select(b => b.propertyName).Take(5));
                            Plugin.Log.LogWarning($"[BINDINGS]   Path: \"{group.Key}\"");
                            Plugin.Log.LogWarning($"[BINDINGS]     Properties: {properties}{(group.Count() > 5 ? "..." : "")}");
                        }
                    }

                    if (pathCount > 20)
                    {
                        Plugin.Log.LogWarning($"[BINDINGS]   ... and {pathCount - 20} more paths");
                    }
                }
            }
        }

        private List<CurveBinding> GetCurveBindings(AnimationClip clip)
        {
            var bindings = new List<CurveBinding>();

            try
            {
                // Try to get bindings via reflection on AnimationUtility
                var animUtilType = System.Type.GetType("UnityEditor.AnimationUtility, UnityEditor");
                if (animUtilType != null)
                {
                    var method = animUtilType.GetMethod("GetCurveBindings", BindingFlags.Static | BindingFlags.Public);
                    if (method != null)
                    {
                        var result = method.Invoke(null, new object[] { clip });
                        // Process result...
                    }
                }
            }
            catch { }

            // Fallback: manually sample the clip and detect which transforms move
            if (bindings.Count == 0 && _animationRoot != null)
            {
                Plugin.Log.LogWarning($"[BINDINGS]   Using manual detection (sampling at t=0 vs t=0.5)...");

                // Store original
                var origPoses = new Dictionary<string, BoneSnapshot>();
                StoreBoneSnapshot(origPoses);

                // Sample at 0
                clip.SampleAnimation(_animationRoot.gameObject, 0f);
                var pose0 = new Dictionary<string, BoneSnapshot>();
                StoreBoneSnapshot(pose0);

                // Sample at 0.5
                clip.SampleAnimation(_animationRoot.gameObject, 0.5f);
                var pose05 = new Dictionary<string, BoneSnapshot>();
                StoreBoneSnapshot(pose05);

                // Compare
                int affectedCount = 0;
                foreach (var kvp in pose0)
                {
                    if (pose05.TryGetValue(kvp.Key, out var p05))
                    {
                        float posDiff = Vector3.Distance(kvp.Value.localPosition, p05.localPosition);
                        float rotDiff = Quaternion.Angle(kvp.Value.localRotation, p05.localRotation);

                        if (posDiff > 0.0001f || rotDiff > 0.01f)
                        {
                            affectedCount++;
                            bindings.Add(new CurveBinding { path = kvp.Key, propertyName = "transform" });

                            if (affectedCount <= 10)
                            {
                                Plugin.Log.LogWarning($"[BINDINGS]   AFFECTED: {kvp.Key} (pos:{posDiff:F4}, rot:{rotDiff:F2}°)");
                            }
                        }
                    }
                }

                Plugin.Log.LogWarning($"[BINDINGS]   Total affected transforms: {affectedCount}");

                // Restore original poses
                foreach (var kvp in origPoses)
                {
                    if (_boneCache.TryGetValue(kvp.Key, out var bone) && bone != null)
                    {
                        bone.localPosition = kvp.Value.localPosition;
                        bone.localRotation = kvp.Value.localRotation;
                        bone.localScale = kvp.Value.localScale;
                    }
                }
            }

            return bindings;
        }

        private struct CurveBinding
        {
            public string path;
            public string propertyName;
        }

        private void LogClipBindingsVsGameBones()
        {
            if (_clips.Count == 0 || _boneCache.Count == 0) return;

            Plugin.Log.LogWarning("[PATHCHECK] ═══════════════════════════════════════════════════════");
            Plugin.Log.LogWarning("[PATHCHECK] Checking if clip paths match game bone paths...");

            // Log first 20 bone paths from cache
            Plugin.Log.LogWarning("[PATHCHECK] First 20 bone paths in game:");
            int count = 0;
            foreach (var path in _boneCache.Keys.OrderBy(k => k))
            {
                if (count++ >= 20) break;
                Plugin.Log.LogWarning($"[PATHCHECK]   {path}");
            }

            // Do a quick sample test
            if (_clips.Count > 0)
            {
                var clip = _clips[0];
                Plugin.Log.LogWarning($"[PATHCHECK] Quick sample test with clip: {clip.name}");

                // Find pelvis
                Transform pelvis = null;
                string pelvisPath = "";
                foreach (var kvp in _boneCache)
                {
                    if (kvp.Key.Contains("Pelvis"))
                    {
                        pelvis = kvp.Value;
                        pelvisPath = kvp.Key;
                        break;
                    }
                }

                if (pelvis != null)
                {
                    Vector3 before = pelvis.localPosition;
                    clip.SampleAnimation(_animationRoot.gameObject, 0f);
                    Vector3 at0 = pelvis.localPosition;
                    clip.SampleAnimation(_animationRoot.gameObject, 0.5f);
                    Vector3 at05 = pelvis.localPosition;

                    Plugin.Log.LogWarning($"[PATHCHECK] Pelvis path: {pelvisPath}");
                    Plugin.Log.LogWarning($"[PATHCHECK] Pelvis before: {before}");
                    Plugin.Log.LogWarning($"[PATHCHECK] Pelvis at t=0: {at0}");
                    Plugin.Log.LogWarning($"[PATHCHECK] Pelvis at t=0.5: {at05}");

                    bool moved = Vector3.Distance(at0, at05) > 0.0001f;
                    Plugin.Log.LogWarning($"[PATHCHECK] Pelvis MOVED: {moved}");

                    if (!moved)
                    {
                        Plugin.Log.LogError("[PATHCHECK] *** ANIMATION NOT AFFECTING BONES! ***");
                        Plugin.Log.LogError("[PATHCHECK] Likely cause: Path mismatch between clip curves and game hierarchy");
                    }

                    // Restore
                    pelvis.localPosition = before;
                }
            }
        }

        private void LogAllBonePositions()
        {
            Plugin.Log.LogWarning("[BONEPOS] ═══════════════════════════════════════════════════════");
            Plugin.Log.LogWarning($"[BONEPOS] All bone positions ({_boneCache.Count} bones):");

            int count = 0;
            foreach (var kvp in _boneCache.OrderBy(k => k.Key))
            {
                if (kvp.Value != null)
                {
                    count++;
                    if (count <= 50)
                    {
                        Plugin.Log.LogWarning($"[BONEPOS] {kvp.Key}");
                        Plugin.Log.LogWarning($"[BONEPOS]   localPos: {kvp.Value.localPosition}");
                        Plugin.Log.LogWarning($"[BONEPOS]   localRot: {kvp.Value.localRotation.eulerAngles}");
                    }
                }
            }

            if (count > 50)
            {
                Plugin.Log.LogWarning($"[BONEPOS] ... and {count - 50} more bones");
            }
        }

        private void BuildBoneCache(Transform root)
        {
            _boneCache.Clear();
            _originalPoses.Clear();
            CacheBonesRecursive(root, "");
        }

        private void CacheBonesRecursive(Transform current, string path)
        {
            foreach (Transform child in current)
            {
                string childPath = string.IsNullOrEmpty(path) ? child.name : path + "/" + child.name;
                _boneCache[childPath] = child;
                _originalPoses[childPath] = new BoneSnapshot
                {
                    localPosition = child.localPosition,
                    localRotation = child.localRotation,
                    localScale = child.localScale,
                    worldPosition = child.position
                };
                CacheBonesRecursive(child, childPath);
            }
        }

        private Transform FindAnimationRootTransform(Transform animatorTransform)
        {
            // Look for common character rig patterns
            string[] possibleRigNames = { "ThePack_Felix_VC_01", "Armature", "Root", "Skeleton" };

            foreach (var rigName in possibleRigNames)
            {
                var result = FindTransformWithDirectChild(animatorTransform, rigName);
                if (result != null)
                {
                    Plugin.Log.LogWarning($"[ROOT] Found root with child '{rigName}': {result.name}");
                    Plugin.Log.LogWarning($"[ROOT] Direct children of {result.name}:");
                    foreach (Transform child in result)
                    {
                        Plugin.Log.LogWarning($"[ROOT]   - {child.name}");
                    }
                    return result;
                }
            }

            // Fallback: just use animator transform
            Plugin.Log.LogWarning($"[ROOT] No specific rig found, using animator transform: {animatorTransform.name}");
            return animatorTransform;
        }

        private Transform FindTransformWithDirectChild(Transform parent, string childName)
        {
            foreach (Transform child in parent)
            {
                if (child.name == childName)
                {
                    return parent;
                }
            }
            foreach (Transform child in parent)
            {
                var result = FindTransformWithDirectChild(child, childName);
                if (result != null) return result;
            }
            return null;
        }

        private void PlayNext()
        {
            if (_clips.Count == 0)
            {
                Plugin.Log.LogWarning("[PLAY] No clips loaded!");
                return;
            }
            if (_animationRoot == null)
            {
                FindAnimationRoot();
                if (_animationRoot == null)
                {
                    Plugin.Log.LogWarning("[PLAY] No animation root found!");
                    return;
                }
            }

            _currentIndex = (_currentIndex + 1) % _clips.Count;
            PlayClip(_currentIndex);
        }

        private void PlayPrev()
        {
            if (_clips.Count == 0 || _animationRoot == null) return;
            _currentIndex--;
            if (_currentIndex < 0) _currentIndex = _clips.Count - 1;
            PlayClip(_currentIndex);
        }

        private void PlayClip(int idx)
        {
            var clip = _clips[idx];

            Plugin.Log.LogWarning($"[PLAY] ═══════════════════════════════════════════════════════");
            Plugin.Log.LogWarning($"[PLAY] PLAYING CLIP [{idx}]: {clip.name}");
            Plugin.Log.LogWarning($"[PLAY] Time: {_timer:F2}s | DateTime: {System.DateTime.Now:HH:mm:ss.fff}");
            Plugin.Log.LogWarning($"[PLAY] ─────────────────────────────────────────────────────");
            Plugin.Log.LogWarning($"[PLAY] Clip length: {clip.length:F3}s");
            Plugin.Log.LogWarning($"[PLAY] Clip legacy: {clip.legacy}");
            Plugin.Log.LogWarning($"[PLAY] Clip humanMotion: {clip.isHumanMotion}");
            Plugin.Log.LogWarning($"[PLAY] Animation root: {_animationRoot.name}");
            Plugin.Log.LogWarning($"[PLAY] Bone count: {_boneCache.Count}");

            // Log animator state BEFORE disabling
            if (_gameAnimator != null)
            {
                Plugin.Log.LogWarning($"[PLAY] ─────────────────────────────────────────────────────");
                Plugin.Log.LogWarning($"[PLAY] BEFORE - Animator state:");
                Plugin.Log.LogWarning($"[PLAY]   enabled: {_gameAnimator.enabled}");
                Plugin.Log.LogWarning($"[PLAY]   isActiveAndEnabled: {_gameAnimator.isActiveAndEnabled}");

                // Disable animator
                _gameAnimator.enabled = false;
                Plugin.Log.LogWarning($"[PLAY] AFTER - Disabled main animator");
                Plugin.Log.LogWarning($"[PLAY]   enabled: {_gameAnimator.enabled}");
            }

            // Disable all child animators
            var allAnimators = _animationRoot.GetComponentsInChildren<Animator>();
            int disabledCount = 0;
            foreach (var anim in allAnimators)
            {
                if (anim.enabled)
                {
                    anim.enabled = false;
                    disabledCount++;
                    Plugin.Log.LogWarning($"[PLAY] Disabled child animator: {anim.gameObject.name}");
                }
            }
            Plugin.Log.LogWarning($"[PLAY] Total animators disabled: {disabledCount + 1}");

            // Store original poses
            StoreBoneSnapshot(_originalPoses);
            Plugin.Log.LogWarning($"[PLAY] Stored {_originalPoses.Count} original bone poses");

            // Set playback state
            _currentClip = clip;
            _playbackTime = 0f;
            _isPlayingCustom = true;
            _sampleCallCount = 0;

            Plugin.Log.LogWarning($"[PLAY] ─────────────────────────────────────────────────────");
            Plugin.Log.LogWarning($"[PLAY] IMMEDIATE SAMPLE TEST:");

            // Test sample at different times
            TestSampleOnRoot(_animationRoot, clip);

            Plugin.Log.LogWarning($"[PLAY] ─────────────────────────────────────────────────────");
            Plugin.Log.LogWarning($"[PLAY] Playback started. LateUpdate will sample continuously.");

            // Log first few clip bindings
            if (!_clipBindingsLogged)
            {
                _clipBindingsLogged = true;
                LogAllClipBindings();
            }
        }

        private void TestSampleOnRoot(Transform root, AnimationClip clip)
        {
            Plugin.Log.LogWarning($"[TEST] Testing SampleAnimation on: {root.name}");

            // Find key bones to track
            var trackedBones = new List<(string path, Transform bone)>();
            foreach (var kvp in _boneCache)
            {
                if (kvp.Key.Contains("Pelvis") || kvp.Key.Contains("Spine") ||
                    kvp.Key.Contains("Head") || kvp.Key.Contains("Hand"))
                {
                    trackedBones.Add((kvp.Key, kvp.Value));
                    if (trackedBones.Count >= 5) break;
                }
            }

            if (trackedBones.Count == 0)
            {
                Plugin.Log.LogWarning($"[TEST] No key bones found to track!");
                return;
            }

            Plugin.Log.LogWarning($"[TEST] Tracking {trackedBones.Count} bones:");
            foreach (var (path, bone) in trackedBones)
            {
                Plugin.Log.LogWarning($"[TEST]   {path}");
            }

            // Initial positions
            Plugin.Log.LogWarning($"[TEST] ─── Initial positions ───");
            var initialPos = new Dictionary<string, Vector3>();
            foreach (var (path, bone) in trackedBones)
            {
                initialPos[path] = bone.localPosition;
                Plugin.Log.LogWarning($"[TEST]   {path}: {bone.localPosition}");
            }

            // Sample at t=0
            Plugin.Log.LogWarning($"[TEST] ─── Sampling at t=0 ───");
            clip.SampleAnimation(root.gameObject, 0f);
            _sampleCallCount++;

            foreach (var (path, bone) in trackedBones)
            {
                var moved = Vector3.Distance(bone.localPosition, initialPos[path]) > 0.0001f;
                Plugin.Log.LogWarning($"[TEST]   {path}: {bone.localPosition} (moved: {moved})");
            }

            // Sample at t=0.5
            Plugin.Log.LogWarning($"[TEST] ─── Sampling at t=0.5 ───");
            var pos0 = new Dictionary<string, Vector3>();
            foreach (var (path, bone) in trackedBones)
            {
                pos0[path] = bone.localPosition;
            }

            clip.SampleAnimation(root.gameObject, 0.5f);
            _sampleCallCount++;

            int movedCount = 0;
            foreach (var (path, bone) in trackedBones)
            {
                var moved = Vector3.Distance(bone.localPosition, pos0[path]) > 0.0001f;
                if (moved) movedCount++;
                Plugin.Log.LogWarning($"[TEST]   {path}: {bone.localPosition} (moved: {moved})");
            }

            // Sample at t=1.0
            Plugin.Log.LogWarning($"[TEST] ─── Sampling at t=1.0 ───");
            clip.SampleAnimation(root.gameObject, 1.0f);
            _sampleCallCount++;

            foreach (var (path, bone) in trackedBones)
            {
                Plugin.Log.LogWarning($"[TEST]   {path}: {bone.localPosition}");
            }

            Plugin.Log.LogWarning($"[TEST] ═══════════════════════════════════════════════════════");
            Plugin.Log.LogWarning($"[TEST] RESULT: {movedCount}/{trackedBones.Count} bones moved between t=0 and t=0.5");

            if (movedCount == 0)
            {
                Plugin.Log.LogError($"[TEST] *** NO BONES MOVED! ANIMATION IS NOT WORKING! ***");
                Plugin.Log.LogError($"[TEST] Possible causes:");
                Plugin.Log.LogError($"[TEST]   1. Clip curve paths don't match game bone paths");
                Plugin.Log.LogError($"[TEST]   2. Clip is empty or corrupted");
                Plugin.Log.LogError($"[TEST]   3. Wrong animation root");
            }
            else
            {
                Plugin.Log.LogWarning($"[TEST] *** ANIMATION IS WORKING! Bones are moving! ***");
            }
        }

        private void TestDifferentRoots()
        {
            if (_clips.Count == 0)
            {
                Plugin.Log.LogWarning("[ROOTTEST] No clips to test!");
                return;
            }
            if (_gameAnimator == null)
            {
                FindAnimationRoot();
            }
            if (_gameAnimator == null)
            {
                Plugin.Log.LogWarning("[ROOTTEST] No animator found!");
                return;
            }

            var clip = _clips[0];
            Plugin.Log.LogWarning($"[ROOTTEST] ═══════════════════════════════════════════════════════");
            Plugin.Log.LogWarning($"[ROOTTEST] Testing clip on all potential roots");
            Plugin.Log.LogWarning($"[ROOTTEST] Clip: {clip.name}");

            // Disable animators first
            _gameAnimator.enabled = false;
            var allAnimators = _gameAnimator.GetComponentsInChildren<Animator>();
            foreach (var anim in allAnimators)
                anim.enabled = false;

            // Test on various potential roots
            var testRoots = new List<Transform>();
            testRoots.Add(_gameAnimator.transform);
            CollectAllDescendants(_gameAnimator.transform, testRoots, 4);

            Plugin.Log.LogWarning($"[ROOTTEST] Testing {testRoots.Count} potential roots...");

            foreach (var root in testRoots)
            {
                // Find pelvis under this root
                Transform pelvis = FindFirstByName(root, "Rig.Pelvis");
                if (pelvis == null) pelvis = FindFirstByName(root, "Pelvis");
                if (pelvis == null) pelvis = FindFirstByName(root, "Hips");

                if (pelvis != null)
                {
                    Vector3 before = pelvis.localPosition;

                    clip.SampleAnimation(root.gameObject, 0f);
                    Vector3 at0 = pelvis.localPosition;

                    clip.SampleAnimation(root.gameObject, 0.5f);
                    Vector3 at05 = pelvis.localPosition;

                    bool changed = Vector3.Distance(at0, at05) > 0.001f;

                    if (changed)
                    {
                        Plugin.Log.LogWarning($"[ROOTTEST] *** WORKING ROOT FOUND! ***");
                        Plugin.Log.LogWarning($"[ROOTTEST]   Root: {root.name}");
                        Plugin.Log.LogWarning($"[ROOTTEST]   Full path: {GetFullPath(root)}");
                        Plugin.Log.LogWarning($"[ROOTTEST]   Pelvis moved: at0={at0}, at0.5={at05}");
                    }

                    // Restore
                    pelvis.localPosition = before;
                }
            }

            // Re-enable animator
            _gameAnimator.enabled = true;
            Plugin.Log.LogWarning($"[ROOTTEST] Test complete.");
        }

        private void CollectAllDescendants(Transform parent, List<Transform> results, int maxDepth)
        {
            if (maxDepth <= 0) return;
            foreach (Transform child in parent)
            {
                results.Add(child);
                CollectAllDescendants(child, results, maxDepth - 1);
            }
        }

        private Transform FindFirstByName(Transform parent, string name)
        {
            if (parent.name == name) return parent;
            foreach (Transform child in parent)
            {
                var result = FindFirstByName(child, name);
                if (result != null) return result;
            }
            return null;
        }

        private void StopCustomAnimation()
        {
            Plugin.Log.LogWarning($"[STOP] ═══════════════════════════════════════════════════════");
            Plugin.Log.LogWarning($"[STOP] Stopping animation playback");
            Plugin.Log.LogWarning($"[STOP] Was playing: {_isPlayingCustom}");
            Plugin.Log.LogWarning($"[STOP] Playback time: {_playbackTime:F3}s");
            Plugin.Log.LogWarning($"[STOP] Sample calls made: {_sampleCallCount}");

            _isPlayingCustom = false;
            _currentClip = null;
            _playbackTime = 0f;

            // Restore original poses
            int restoredCount = 0;
            foreach (var kvp in _originalPoses)
            {
                if (_boneCache.TryGetValue(kvp.Key, out Transform bone) && bone != null)
                {
                    bone.localPosition = kvp.Value.localPosition;
                    bone.localRotation = kvp.Value.localRotation;
                    bone.localScale = kvp.Value.localScale;
                    restoredCount++;
                }
            }
            Plugin.Log.LogWarning($"[STOP] Restored {restoredCount} bone poses");

            if (_gameAnimator != null)
            {
                _gameAnimator.enabled = true;
                Plugin.Log.LogWarning($"[STOP] Re-enabled game animator");
            }

            Plugin.Log.LogWarning($"[STOP] Animation stopped and poses restored");
        }

        private void LogFullHierarchy()
        {
            if (_gameAnimator == null) FindAnimationRoot();

            if (_gameAnimator != null)
            {
                Plugin.Log.LogWarning("[HIERARCHY] ═══════════════════════════════════════════════════════");
                Plugin.Log.LogWarning("[HIERARCHY] Full hierarchy from animator (depth 15):");
                LogHierarchy(_gameAnimator.transform, 0, 15);
            }
        }

        private void LogHierarchy(Transform t, int depth, int maxDepth)
        {
            if (depth > maxDepth) return;
            string indent = new string(' ', depth * 2);

            var components = t.GetComponents<Component>();
            var componentNames = string.Join(",", components.Select(c => c.GetType().Name).Where(n => n != "Transform"));

            string info = componentNames.Length > 0 ? $" [{componentNames}]" : "";
            Plugin.Log.LogWarning($"[HIERARCHY] {indent}- {t.name}{info}");

            foreach (Transform child in t)
            {
                LogHierarchy(child, depth + 1, maxDepth);
            }
        }

        private string GetFullPath(Transform t)
        {
            string path = t.name;
            while (t.parent != null)
            {
                t = t.parent;
                path = t.name + "/" + path;
            }
            return path;
        }

        private void OnDestroy()
        {
            Plugin.Log.LogWarning("[DESTROY] ═══════════════════════════════════════════════════════");
            Plugin.Log.LogWarning($"[DESTROY] AnimationRunner being destroyed");
            Plugin.Log.LogWarning($"[DESTROY] Total runtime: {_timer:F2}s");
            Plugin.Log.LogWarning($"[DESTROY] Total key presses logged: {_keyPressLog.Count}");
            Plugin.Log.LogWarning($"[DESTROY] Total sample calls: {_sampleCallCount}");

            if (_keyPressLog.Count > 0)
            {
                Plugin.Log.LogWarning($"[DESTROY] Key press history:");
                foreach (var entry in _keyPressLog)
                {
                    Plugin.Log.LogWarning($"[DESTROY]   {entry}");
                }
            }
        }
    }
}

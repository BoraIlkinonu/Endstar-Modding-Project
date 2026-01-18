using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System;
using System.Text;
using System.Collections.Generic;

namespace CameraDump
{
    [BepInPlugin("com.debug.cameradump", "Camera Dump", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log;
        private static float _lastDumpTime = 0f;
        private static int _frameCount = 0;
        private static string _lastSceneName = "";

        private void Awake()
        {
            Log = Logger;
            Log.LogInfo("CameraDump plugin loaded!");

            var harmony = new Harmony("com.debug.cameradump");
            harmony.PatchAll();
        }

        public static void DumpAllCameras()
        {
            // Throttle dumps to once per 5 seconds
            if (Time.time - _lastDumpTime < 5f) return;
            _lastDumpTime = Time.time;

            var sb = new StringBuilder();
            sb.AppendLine("\n========== CAMERA DUMP ==========");
            sb.AppendLine($"Time: {Time.time:F2}, Frame: {Time.frameCount}");
            sb.AppendLine($"Active Scene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");

            // Find all cameras
            var cameras = UnityEngine.Object.FindObjectsOfType<Camera>(true);
            sb.AppendLine($"\nFound {cameras.Length} cameras:");

            foreach (var cam in cameras)
            {
                sb.AppendLine($"\n--- Camera: {cam.name} ---");
                sb.AppendLine($"  GameObject: {GetFullPath(cam.gameObject)}");
                sb.AppendLine($"  Enabled: {cam.enabled}, GameObject Active: {cam.gameObject.activeInHierarchy}");
                sb.AppendLine($"  Depth: {cam.depth}");
                sb.AppendLine($"  CullingMask: {cam.cullingMask} ({GetLayerNames(cam.cullingMask)})");
                sb.AppendLine($"  ClearFlags: {cam.clearFlags}");
                sb.AppendLine($"  BackgroundColor: {cam.backgroundColor}");
                sb.AppendLine($"  RenderingPath: {cam.actualRenderingPath}");
                sb.AppendLine($"  Orthographic: {cam.orthographic}");
                sb.AppendLine($"  FOV: {cam.fieldOfView}");
                sb.AppendLine($"  NearClip: {cam.nearClipPlane}, FarClip: {cam.farClipPlane}");
                sb.AppendLine($"  TargetTexture: {(cam.targetTexture != null ? cam.targetTexture.name : "null")}");
                sb.AppendLine($"  AllowHDR: {cam.allowHDR}");
                sb.AppendLine($"  AllowMSAA: {cam.allowMSAA}");

                // URP Additional Camera Data
                var urpData = cam.GetComponent<UniversalAdditionalCameraData>();
                if (urpData != null)
                {
                    sb.AppendLine($"  [URP Data]");
                    sb.AppendLine($"    RenderType: {urpData.renderType}");
                    sb.AppendLine($"    Renderer: {urpData.scriptableRenderer?.GetType().Name ?? "null"}");
                    sb.AppendLine($"    RenderShadows: {urpData.renderShadows}");
                    sb.AppendLine($"    RenderPostProcessing: {urpData.renderPostProcessing}");
                    sb.AppendLine($"    Antialiasing: {urpData.antialiasing}");
                    sb.AppendLine($"    AntialiasingQuality: {urpData.antialiasingQuality}");
                    sb.AppendLine($"    StopNaN: {urpData.stopNaN}");
                    sb.AppendLine($"    Dithering: {urpData.dithering}");
                    sb.AppendLine($"    RequiresColorOption: {urpData.requiresColorOption}");
                    sb.AppendLine($"    RequiresDepthOption: {urpData.requiresDepthOption}");
                    sb.AppendLine($"    RequiresColorTexture: {urpData.requiresColorTexture}");
                    sb.AppendLine($"    RequiresDepthTexture: {urpData.requiresDepthTexture}");

                    // Check for overlay cameras
                    try
                    {
                        var stack = urpData.cameraStack;
                        if (stack != null && stack.Count > 0)
                        {
                            sb.AppendLine($"    CameraStack ({stack.Count}):");
                            foreach (var stackCam in stack)
                            {
                                sb.AppendLine($"      - {stackCam?.name ?? "null"}");
                            }
                        }
                    }
                    catch { }
                }
                else
                {
                    sb.AppendLine($"  [No URP Additional Camera Data]");
                }
            }

            // Dump URP Asset settings
            sb.AppendLine("\n--- URP Pipeline Asset ---");
            var pipeline = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if (pipeline != null)
            {
                sb.AppendLine($"  Name: {pipeline.name}");
                sb.AppendLine($"  RenderScale: {pipeline.renderScale}");
                sb.AppendLine($"  SupportsHDR: {pipeline.supportsHDR}");
                sb.AppendLine($"  MSAA: {pipeline.msaaSampleCount}");
                sb.AppendLine($"  MainLightRenderingMode: {pipeline.mainLightRenderingMode}");
                sb.AppendLine($"  AdditionalLightsRenderingMode: {pipeline.additionalLightsRenderingMode}");
                sb.AppendLine($"  SupportsSoftShadows: {pipeline.supportsSoftShadows}");
                sb.AppendLine($"  ShadowDistance: {pipeline.shadowDistance}");
                sb.AppendLine($"  MaxAdditionalLightsCount: {pipeline.maxAdditionalLightsCount}");
                sb.AppendLine($"  SupportsLightCookies: {pipeline.supportsLightCookies}");
            }

            // Find "UI Background" layer index
            int uiBackgroundLayer = LayerMask.NameToLayer("UI Background");
            sb.AppendLine($"\n--- Layer Info ---");
            sb.AppendLine($"  'UI Background' layer index: {uiBackgroundLayer}");

            // List all layers
            sb.AppendLine($"  All Layers:");
            for (int i = 0; i < 32; i++)
            {
                string layerName = LayerMask.LayerToName(i);
                if (!string.IsNullOrEmpty(layerName))
                {
                    sb.AppendLine($"    [{i}] {layerName}");
                }
            }

            // Find renderers on UI Background layer
            sb.AppendLine($"\n--- Renderers on 'UI Background' layer ---");
            var allRenderers = UnityEngine.Object.FindObjectsOfType<Renderer>(true);
            int count = 0;
            foreach (var rend in allRenderers)
            {
                if (rend.gameObject.layer == uiBackgroundLayer)
                {
                    count++;
                    if (count <= 20) // Limit output
                    {
                        sb.AppendLine($"  {GetFullPath(rend.gameObject)}");
                        sb.AppendLine($"    Type: {rend.GetType().Name}");
                        sb.AppendLine($"    Enabled: {rend.enabled}");
                        if (rend.sharedMaterials != null)
                        {
                            foreach (var mat in rend.sharedMaterials)
                            {
                                if (mat != null)
                                {
                                    sb.AppendLine($"    Material: {mat.name}");
                                    sb.AppendLine($"      Shader: {mat.shader?.name ?? "null"}");
                                    sb.AppendLine($"      RenderQueue: {mat.renderQueue}");

                                    // Get shader keywords
                                    var keywords = mat.shaderKeywords;
                                    if (keywords != null && keywords.Length > 0)
                                    {
                                        sb.AppendLine($"      Keywords: {string.Join(", ", keywords)}");
                                    }

                                    // Check specific properties
                                    if (mat.HasProperty("_AlphaClip"))
                                        sb.AppendLine($"      _AlphaClip: {mat.GetFloat("_AlphaClip")}");
                                    if (mat.HasProperty("_AlphaToMask"))
                                        sb.AppendLine($"      _AlphaToMask: {mat.GetFloat("_AlphaToMask")}");
                                    if (mat.HasProperty("_Surface"))
                                        sb.AppendLine($"      _Surface: {mat.GetFloat("_Surface")}");
                                    if (mat.HasProperty("_Blend"))
                                        sb.AppendLine($"      _Blend: {mat.GetFloat("_Blend")}");
                                }
                            }
                        }
                    }
                }
            }
            sb.AppendLine($"  Total renderers on UI Background: {count}");

            // Look for ALL Renderers on UI Background layer
            sb.AppendLine($"\n--- ALL CHARACTER RENDERERS ---");
            var allRends = UnityEngine.Object.FindObjectsOfType<Renderer>(true);
            int charCount = 0;
            foreach (var rend in allRends)
            {
                string path = GetFullPath(rend.gameObject);
                // Only dump character-related renderers
                if (path.Contains("Character") || path.Contains("Cosmetic") || path.Contains("LOD"))
                {
                    charCount++;
                    sb.AppendLine($"  [{rend.GetType().Name}] {path}");
                    sb.AppendLine($"    Layer: {rend.gameObject.layer} ({LayerMask.LayerToName(rend.gameObject.layer)})");
                    sb.AppendLine($"    Enabled: {rend.enabled}, Active: {rend.gameObject.activeInHierarchy}");

                    // Dump material info for any renderer
                    if (rend.sharedMaterials != null)
                    {
                        foreach (var mat in rend.sharedMaterials)
                        {
                            if (mat != null)
                            {
                                sb.AppendLine($"    Material: {mat.name}");
                                sb.AppendLine($"      Shader: {mat.shader?.name}");
                                sb.AppendLine($"      RenderQueue: {mat.renderQueue}");
                                sb.AppendLine($"      PassCount: {mat.passCount}");
                                sb.AppendLine($"      Keywords: [{string.Join(", ", mat.shaderKeywords)}]");

                                if (mat.HasProperty("_AlphaClip"))
                                    sb.AppendLine($"      _AlphaClip: {mat.GetFloat("_AlphaClip")}");
                            }
                        }
                    }
                }
            }
            sb.AppendLine($"  Total character renderers: {charCount}");

            sb.AppendLine("\n========== END CAMERA DUMP ==========");
            Log.LogInfo(sb.ToString());
        }

        private static void DumpSkinnedMeshRenderer(StringBuilder sb, SkinnedMeshRenderer smr)
        {
            sb.AppendLine($"    [SkinnedMeshRenderer Details]");
            sb.AppendLine($"      Bounds: {smr.bounds}");
            sb.AppendLine($"      ShadowCastingMode: {smr.shadowCastingMode}");
            sb.AppendLine($"      ReceiveShadows: {smr.receiveShadows}");
            sb.AppendLine($"      LightProbeUsage: {smr.lightProbeUsage}");
            sb.AppendLine($"      ReflectionProbeUsage: {smr.reflectionProbeUsage}");
            sb.AppendLine($"      MotionVectorGenerationMode: {smr.motionVectorGenerationMode}");
            sb.AppendLine($"      AllowOcclusionWhenDynamic: {smr.allowOcclusionWhenDynamic}");
            sb.AppendLine($"      RenderingLayerMask: {smr.renderingLayerMask}");
            sb.AppendLine($"      SortingLayerID: {smr.sortingLayerID}");
            sb.AppendLine($"      SortingOrder: {smr.sortingOrder}");

            if (smr.sharedMaterials != null)
            {
                sb.AppendLine($"      Materials ({smr.sharedMaterials.Length}):");
                foreach (var mat in smr.sharedMaterials)
                {
                    if (mat != null)
                    {
                        sb.AppendLine($"        [{mat.name}]");
                        sb.AppendLine($"          Shader: {mat.shader?.name}");
                        sb.AppendLine($"          RenderQueue: {mat.renderQueue}");
                        sb.AppendLine($"          PassCount: {mat.passCount}");

                        // Shader keywords
                        var keywords = mat.shaderKeywords;
                        sb.AppendLine($"          Keywords: [{string.Join(", ", keywords)}]");

                        // Enabled keywords
                        sb.AppendLine($"          EnabledKeywords:");
                        foreach (var kw in mat.enabledKeywords)
                        {
                            sb.AppendLine($"            - {kw.name}");
                        }

                        // Key properties
                        DumpMaterialProperties(sb, mat);
                    }
                }
            }
        }

        private static void DumpMaterialProperties(StringBuilder sb, Material mat)
        {
            string[] propsToCheck = {
                "_AlphaClip", "_AlphaToMask", "_Surface", "_Blend", "_SrcBlend", "_DstBlend",
                "_ZWrite", "_ZTest", "_Cull", "_QueueOffset", "Albedo_Tint",
                "_ALPHATEST_ON", "_ALPHAPREMULTIPLY_ON", "_SURFACE_TYPE_TRANSPARENT"
            };

            foreach (var prop in propsToCheck)
            {
                try
                {
                    if (mat.HasProperty(prop))
                    {
                        // Try float first
                        float val = mat.GetFloat(prop);
                        sb.AppendLine($"          {prop}: {val}");
                    }
                }
                catch { }
            }

            // Check for color properties
            try
            {
                if (mat.HasProperty("Albedo_Tint"))
                {
                    var color = mat.GetColor("Albedo_Tint");
                    sb.AppendLine($"          Albedo_Tint (color): R={color.r}, G={color.g}, B={color.b}, A={color.a}");
                }
            }
            catch { }

            // Render state
            sb.AppendLine($"          [Render State from Shader]");
            for (int i = 0; i < mat.passCount; i++)
            {
                string passName = mat.GetPassName(i);
                bool enabled = mat.GetShaderPassEnabled(passName);
                sb.AppendLine($"            Pass[{i}] '{passName}': enabled={enabled}");
            }
        }

        private static string GetFullPath(GameObject go)
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

        private static string GetLayerNames(int mask)
        {
            var layers = new List<string>();
            for (int i = 0; i < 32; i++)
            {
                if ((mask & (1 << i)) != 0)
                {
                    string name = LayerMask.LayerToName(i);
                    if (!string.IsNullOrEmpty(name))
                        layers.Add(name);
                    else
                        layers.Add($"[{i}]");
                }
            }
            return string.Join(", ", layers);
        }

        private static int _lastSMRCount = 0;
        private static bool _f8WasPressed = false;

        [HarmonyPatch(typeof(Time), "get_deltaTime")]
        public static class TimePatch
        {
            static void Postfix()
            {
                _frameCount++;

                // Check F8 key state
                try
                {
                    bool f8Pressed = Input.GetKey(KeyCode.F8);
                    if (f8Pressed && !_f8WasPressed)
                    {
                        Log.LogWarning("=== MANUAL DUMP (F8) ===");
                        DumpAllCameras();
                    }
                    _f8WasPressed = f8Pressed;
                }
                catch { }

                // Dump every second when there's ANY renderer on UI Background layer
                if (_frameCount % 60 == 0)
                {
                    var uiLayer = LayerMask.NameToLayer("UI Background");
                    var allRenderers = UnityEngine.Object.FindObjectsOfType<Renderer>(true);
                    string currentChar = "";

                    foreach (var rend in allRenderers)
                    {
                        // Look for character-like objects (has "Character" or "Cosmetic" in path)
                        if (rend.gameObject.layer == uiLayer && rend.gameObject.activeInHierarchy)
                        {
                            string path = GetFullPath(rend.gameObject);
                            if (path.Contains("Character") || path.Contains("Cosmetic"))
                            {
                                currentChar = path;
                                break;
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(currentChar) && currentChar != _lastSceneName)
                    {
                        _lastSceneName = currentChar;
                        Log.LogWarning($"=== CHARACTER DETECTED ===");
                        DumpAllCameras();
                    }
                }
            }
        }
    }
}

using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;

namespace SilhouetteFix
{
    [BepInPlugin("com.endstar.silhouettefix", "SilhouetteFix", "4.2.0")]
    public class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log;
        internal static int CallCount = 0;
        internal static Material GoodMaterial = null;
        internal static HashSet<int> SwappedRenderers = new HashSet<int>();
        internal static HashSet<string> LoggedNames = new HashSet<string>();

        private void Awake()
        {
            Log = Logger;
            Logger.LogWarning("=== SilhouetteFix v4.2.0 - DEBUG ALL MESHES ===");

            var harmony = new Harmony("com.endstar.silhouettefix");
            harmony.PatchAll(typeof(Plugin).Assembly);
        }

        public static void SwapMaterials()
        {
            CallCount++;
            if (CallCount % 60 != 1) return;

            var renderers = Object.FindObjectsOfType<SkinnedMeshRenderer>();

            // Log ALL skinned mesh renderers we find
            foreach (var renderer in renderers)
            {
                if (renderer == null) continue;
                string name = renderer.gameObject.name;

                if (!LoggedNames.Contains(name))
                {
                    LoggedNames.Add(name);
                    var mat = renderer.sharedMaterial;
                    string matName = mat != null ? mat.name : "NULL";
                    int passes = mat != null ? mat.passCount : 0;
                    Log.LogWarning($"MESH: {name} | Material: {matName} | Passes: {passes}");
                }
            }

            // Find good material
            if (GoodMaterial == null)
            {
                foreach (var renderer in renderers)
                {
                    if (renderer == null) continue;
                    string name = renderer.gameObject.name;
                    if (name.Contains("TigerGuy")) continue;

                    var mat = renderer.sharedMaterial;
                    if (mat == null) continue;

                    bool hasAlphaTest = System.Array.Exists(mat.shaderKeywords, k => k == "_ALPHATEST_ON");
                    bool has6Passes = mat.passCount == 6;

                    if (hasAlphaTest && has6Passes)
                    {
                        GoodMaterial = mat;
                        Log.LogWarning($"CAPTURED good material: {mat.name} from {name}");
                        break;
                    }
                }
            }

            if (GoodMaterial == null) return;

            // Find and fix TigerGuy (any mesh containing "Tiger")
            foreach (var renderer in renderers)
            {
                if (renderer == null) continue;

                string objName = renderer.gameObject.name;
                if (!objName.Contains("Tiger")) continue;

                int id = renderer.GetInstanceID();
                if (SwappedRenderers.Contains(id)) continue;

                var tigerMat = renderer.sharedMaterial;
                if (tigerMat == null) continue;

                Log.LogWarning($"FIXING: {objName}");
                Log.LogWarning($"  Original passes: {tigerMat.passCount}");

                Material newMat = new Material(GoodMaterial);
                newMat.name = tigerMat.name + "_Fixed";

                // Copy textures
                string[] texProps = { "_Albedo", "_MainTex", "_BaseMap", "_Pre_Integrated_Scattering" };
                foreach (var prop in texProps)
                {
                    if (tigerMat.HasProperty(prop) && newMat.HasProperty(prop))
                    {
                        var tex = tigerMat.GetTexture(prop);
                        if (tex != null) newMat.SetTexture(prop, tex);
                    }
                }

                // Copy colors
                if (tigerMat.HasProperty("_BaseColor") && newMat.HasProperty("_BaseColor"))
                    newMat.SetColor("_BaseColor", tigerMat.GetColor("_BaseColor"));

                renderer.sharedMaterial = newMat;
                SwappedRenderers.Add(id);

                Log.LogWarning($"  New passes: {newMat.passCount}");
                Log.LogWarning($"  SWAPPED!");
            }
        }
    }

    [HarmonyPatch(typeof(Time), "get_deltaTime")]
    public static class TimePatch
    {
        static void Postfix()
        {
            Plugin.SwapMaterials();
        }
    }
}

using HarmonyLib;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace CustomProps
{
    /// <summary>
    /// Harmony patch to prevent mesh merging for custom props.
    ///
    /// Problem: When BuildPrefab is called with testPrefab, SetupBaseType and SetupComponent
    /// are still called, loading base type visuals that merge with the custom prefab mesh.
    ///
    /// Solution (VERIFIED 2026-01-08):
    /// - SetupBaseType and SetupComponent are PRIVATE SYNCHRONOUS methods
    /// - They are called within BuildPrefab BEFORE any await
    /// - We patch them with Prefix returning false to skip execution when testPrefab is active
    /// - CRITICAL: We do NOT wrap the BuildPrefab Task return (that breaks prop tool UI!)
    /// </summary>
    public static class BuildPrefabPatch
    {
        // Track which EndlessProp instances have testPrefab active (by instance ID)
        private static HashSet<int> _activeTestPrefabInstances = new HashSet<int>();
        private static bool _patchApplied = false;

        public static void ApplyPatches(Harmony harmony)
        {
            if (_patchApplied) return;

            try
            {
                var gameplayAsm = AppDomain.CurrentDomain.GetAssemblies()
                    .Find(a => a.GetName().Name == "Gameplay");

                if (gameplayAsm == null)
                {
                    CustomPropsPlugin.Log?.LogError("BuildPrefabPatch: Gameplay assembly not found");
                    return;
                }

                var endlessPropType = gameplayAsm.GetType("Endless.Gameplay.Scripting.EndlessProp");
                if (endlessPropType == null)
                {
                    CustomPropsPlugin.Log?.LogError("BuildPrefabPatch: EndlessProp type not found");
                    return;
                }

                // 1. Patch BuildPrefab - track testPrefab state (Prefix + simple Postfix)
                var buildPrefabMethod = endlessPropType.GetMethod("BuildPrefab",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                if (buildPrefabMethod != null)
                {
                    var prefix = new HarmonyMethod(typeof(BuildPrefabPatch).GetMethod(nameof(BuildPrefab_Prefix),
                        BindingFlags.Public | BindingFlags.Static));
                    var postfix = new HarmonyMethod(typeof(BuildPrefabPatch).GetMethod(nameof(BuildPrefab_Postfix),
                        BindingFlags.Public | BindingFlags.Static));

                    harmony.Patch(buildPrefabMethod, prefix: prefix, postfix: postfix);
                    CustomPropsPlugin.Log?.LogInfo("BuildPrefabPatch: Patched EndlessProp.BuildPrefab");
                }
                else
                {
                    CustomPropsPlugin.Log?.LogWarning("BuildPrefabPatch: BuildPrefab method not found");
                }

                // 2. Patch SetupBaseType - skip if testPrefab active
                var setupBaseTypeMethod = endlessPropType.GetMethod("SetupBaseType",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                if (setupBaseTypeMethod != null)
                {
                    var prefix = new HarmonyMethod(typeof(BuildPrefabPatch).GetMethod(nameof(SetupBaseType_Prefix),
                        BindingFlags.Public | BindingFlags.Static));

                    harmony.Patch(setupBaseTypeMethod, prefix: prefix);
                    CustomPropsPlugin.Log?.LogInfo("BuildPrefabPatch: Patched EndlessProp.SetupBaseType");
                }
                else
                {
                    CustomPropsPlugin.Log?.LogWarning("BuildPrefabPatch: SetupBaseType method not found");
                }

                // 3. Patch SetupComponent - skip if testPrefab active
                var setupComponentMethod = endlessPropType.GetMethod("SetupComponent",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                if (setupComponentMethod != null)
                {
                    var prefix = new HarmonyMethod(typeof(BuildPrefabPatch).GetMethod(nameof(SetupComponent_Prefix),
                        BindingFlags.Public | BindingFlags.Static));

                    harmony.Patch(setupComponentMethod, prefix: prefix);
                    CustomPropsPlugin.Log?.LogInfo("BuildPrefabPatch: Patched EndlessProp.SetupComponent");
                }
                else
                {
                    CustomPropsPlugin.Log?.LogWarning("BuildPrefabPatch: SetupComponent method not found");
                }

                _patchApplied = true;
                CustomPropsPlugin.Log?.LogInfo("BuildPrefabPatch: All patches applied successfully!");
            }
            catch (Exception ex)
            {
                CustomPropsPlugin.Log?.LogError($"BuildPrefabPatch: Failed to apply patches: {ex.Message}");
                CustomPropsPlugin.Log?.LogError(ex.StackTrace);
            }
        }

        /// <summary>
        /// Prefix for BuildPrefab - track when testPrefab is provided
        /// Also log ALL calls to diagnose placement issues
        /// </summary>
        public static void BuildPrefab_Prefix(MonoBehaviour __instance, object prop, GameObject testPrefab, object testScript, CancellationToken cancelToken)
        {
            int instanceId = __instance.GetInstanceID();
            string propName = "null";
            string assetId = "null";

            // Try to get prop name and asset ID
            if (prop != null)
            {
                try
                {
                    var nameField = prop.GetType().GetField("Name", BindingFlags.Public | BindingFlags.Instance);
                    propName = nameField?.GetValue(prop)?.ToString() ?? "?";
                    var assetIdField = prop.GetType().GetField("AssetID", BindingFlags.Public | BindingFlags.Instance);
                    assetId = assetIdField?.GetValue(prop)?.ToString() ?? "?";
                }
                catch { }
            }

            CustomPropsPlugin.Log?.LogInfo($"[BUILD] BuildPrefab called: prop={propName}, assetId={assetId}, testPrefab={(testPrefab != null ? testPrefab.name : "NULL")}, instanceId={instanceId}");

            // Check if this is our custom prop
            if (assetId.Contains("409c8bd8"))
            {
                CustomPropsPlugin.Log?.LogInfo($"[BUILD] *** CUSTOM PROP DETECTED! testPrefab={(testPrefab != null ? "PROVIDED" : "MISSING")} ***");

                if (testPrefab == null)
                {
                    CustomPropsPlugin.Log?.LogWarning($"[BUILD] *** WARNING: BuildPrefab for custom prop WITHOUT testPrefab - this may cause placement to fail! ***");
                }
            }

            if (testPrefab == null) return;

            _activeTestPrefabInstances.Add(instanceId);

            CustomPropsPlugin.Log?.LogInfo($"[BUILD] testPrefab active for instanceId={instanceId} - SetupBaseType and SetupComponent will be SKIPPED");
        }

        /// <summary>
        /// Postfix for BuildPrefab - clear tracking (DO NOT modify __result!)
        /// CRITICAL: Modifying the Task return breaks prop tool UI!
        /// </summary>
        public static void BuildPrefab_Postfix(MonoBehaviour __instance)
        {
            int instanceId = __instance.GetInstanceID();

            if (_activeTestPrefabInstances.Contains(instanceId))
            {
                _activeTestPrefabInstances.Remove(instanceId);
                CustomPropsPlugin.Log?.LogInfo($"BuildPrefabPatch: BuildPrefab completed for instanceId={instanceId}, tracking cleared");
            }
        }

        /// <summary>
        /// Prefix for SetupBaseType - return false to skip if testPrefab is active
        /// </summary>
        public static bool SetupBaseType_Prefix(MonoBehaviour __instance)
        {
            int instanceId = __instance.GetInstanceID();

            if (_activeTestPrefabInstances.Contains(instanceId))
            {
                CustomPropsPlugin.Log?.LogInfo($"BuildPrefabPatch: SKIPPING SetupBaseType for instanceId={instanceId} (testPrefab active)");
                return false; // Skip original method
            }

            return true; // Run original method
        }

        /// <summary>
        /// Prefix for SetupComponent - return false to skip if testPrefab is active
        /// </summary>
        public static bool SetupComponent_Prefix(MonoBehaviour __instance)
        {
            int instanceId = __instance.GetInstanceID();

            if (_activeTestPrefabInstances.Contains(instanceId))
            {
                CustomPropsPlugin.Log?.LogInfo($"BuildPrefabPatch: SKIPPING SetupComponent for instanceId={instanceId} (testPrefab active)");
                return false; // Skip original method
            }

            return true; // Run original method
        }
    }

    // Extension method to find assembly
    public static class AssemblyExtensions
    {
        public static Assembly Find(this Assembly[] assemblies, Func<Assembly, bool> predicate)
        {
            foreach (var asm in assemblies)
            {
                if (predicate(asm)) return asm;
            }
            return null;
        }
    }
}

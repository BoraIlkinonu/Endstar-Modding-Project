using HarmonyLib;
using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;

namespace CustomProps
{
    /// <summary>
    /// V3 Diagnostic - Previous patches weren't firing at runtime.
    /// Using HarmonyPatchAll with attribute-based patches for reliability.
    /// Also patching ALL methods on PropTool/PropBasedTool to catch any activity.
    /// </summary>
    public static class PlacementDiagnosticPatch
    {
        private static Assembly _creatorAssembly;
        private static Type _propToolType;
        private static Type _propBasedToolType;

        public static void ApplyPatches(Harmony harmony)
        {
            try
            {
                // Find Creator assembly
                _creatorAssembly = AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(a => a.GetName().Name == "Creator");

                if (_creatorAssembly == null)
                {
                    CustomPropsPlugin.Log?.LogWarning("[DIAG-V3] Creator assembly not found!");
                    return;
                }

                // Find PropTool and PropBasedTool
                _propToolType = _creatorAssembly.GetType("Endless.Creator.LevelEditing.Runtime.PropTool");
                _propBasedToolType = _creatorAssembly.GetType("Endless.Creator.LevelEditing.Runtime.PropBasedTool");

                if (_propToolType == null)
                {
                    CustomPropsPlugin.Log?.LogWarning("[DIAG-V3] PropTool type not found!");

                    // Try to find it by searching all types
                    var allTypes = _creatorAssembly.GetTypes();
                    var propToolCandidates = allTypes.Where(t => t.Name.Contains("PropTool")).ToList();
                    CustomPropsPlugin.Log?.LogInfo($"[DIAG-V3] PropTool candidates: {string.Join(", ", propToolCandidates.Select(t => t.FullName))}");
                }
                else
                {
                    CustomPropsPlugin.Log?.LogInfo($"[DIAG-V3] Found PropTool: {_propToolType.FullName}");
                }

                if (_propBasedToolType == null)
                {
                    CustomPropsPlugin.Log?.LogWarning("[DIAG-V3] PropBasedTool type not found!");
                }
                else
                {
                    CustomPropsPlugin.Log?.LogInfo($"[DIAG-V3] Found PropBasedTool: {_propBasedToolType.FullName}");
                }

                // Patch ALL public methods on PropTool to see what gets called
                if (_propToolType != null)
                {
                    PatchAllMethodsOnType(harmony, _propToolType, "PropTool");
                }

                // Patch ALL public methods on PropBasedTool
                if (_propBasedToolType != null)
                {
                    PatchAllMethodsOnType(harmony, _propBasedToolType, "PropBasedTool");
                }

                // Also patch the base CreatorTool if it exists
                var creatorToolType = _creatorAssembly.GetType("Endless.Creator.LevelEditing.Runtime.CreatorTool");
                if (creatorToolType != null)
                {
                    CustomPropsPlugin.Log?.LogInfo($"[DIAG-V3] Found CreatorTool: {creatorToolType.FullName}");
                    PatchAllMethodsOnType(harmony, creatorToolType, "CreatorTool");
                }
            }
            catch (Exception ex)
            {
                CustomPropsPlugin.Log?.LogError($"[DIAG-V3] ApplyPatches failed: {ex}");
            }
        }

        private static void PatchAllMethodsOnType(Harmony harmony, Type type, string typeName)
        {
            try
            {
                var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                    .Where(m => !m.IsSpecialName && !m.IsGenericMethod)
                    .ToList();

                CustomPropsPlugin.Log?.LogInfo($"[DIAG-V3] {typeName} has {methods.Count} methods to patch");

                int patched = 0;
                foreach (var method in methods)
                {
                    try
                    {
                        // Skip property getters/setters
                        if (method.Name.StartsWith("get_") || method.Name.StartsWith("set_"))
                            continue;

                        // Skip compiler-generated methods
                        if (method.Name.Contains("<") || method.Name.Contains(">"))
                            continue;

                        var prefix = new HarmonyMethod(typeof(PlacementDiagnosticPatch).GetMethod(nameof(UniversalPrefix),
                            BindingFlags.Static | BindingFlags.NonPublic));

                        harmony.Patch(method, prefix: prefix);
                        patched++;
                    }
                    catch (Exception ex)
                    {
                        CustomPropsPlugin.Log?.LogWarning($"[DIAG-V3] Failed to patch {typeName}.{method.Name}: {ex.Message}");
                    }
                }

                CustomPropsPlugin.Log?.LogInfo($"[DIAG-V3] Successfully patched {patched} methods on {typeName}");

                // List all method names for debugging
                CustomPropsPlugin.Log?.LogInfo($"[DIAG-V3] {typeName} methods: {string.Join(", ", methods.Select(m => m.Name))}");
            }
            catch (Exception ex)
            {
                CustomPropsPlugin.Log?.LogError($"[DIAG-V3] PatchAllMethodsOnType({typeName}) failed: {ex}");
            }
        }

        // Universal prefix that logs ANY method call
        private static void UniversalPrefix(MethodBase __originalMethod)
        {
            try
            {
                var methodName = __originalMethod?.Name ?? "Unknown";
                var typeName = __originalMethod?.DeclaringType?.Name ?? "Unknown";

                // Only log interesting methods (not Update, etc.)
                if (methodName == "Update" || methodName == "LateUpdate" || methodName == "FixedUpdate")
                    return;

                CustomPropsPlugin.Log?.LogInfo($"[DIAG-V3-CALL] {typeName}.{methodName}()");
            }
            catch (Exception ex)
            {
                CustomPropsPlugin.Log?.LogError($"[DIAG-V3] UniversalPrefix error: {ex.Message}");
            }
        }
    }
}

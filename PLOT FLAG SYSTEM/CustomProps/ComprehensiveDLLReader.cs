using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CustomProps
{
    /// <summary>
    /// Comprehensive DLL reader - extracts EVERYTHING about the prop system
    /// No assumptions - just facts from the code
    /// </summary>
    public static class ComprehensiveDLLReader
    {
        private static StringBuilder _output;
        private static HashSet<string> _analyzedTypes = new HashSet<string>();

        public static void ReadAllPropRelatedCode(string outputPath)
        {
            _output = new StringBuilder();
            _analyzedTypes.Clear();

            _output.AppendLine("# COMPREHENSIVE ENDSTAR PROP SYSTEM - DLL ANALYSIS");
            _output.AppendLine($"Generated: {DateTime.Now}");
            _output.AppendLine();
            _output.AppendLine("This document contains EVERYTHING extracted from the game DLLs about props.");
            _output.AppendLine("NO ASSUMPTIONS - only facts from the actual code.");
            _output.AppendLine();

            var assemblies = new Dictionary<string, Assembly>();
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                var name = asm.GetName().Name;
                if (name == "Gameplay" || name == "Creator" || name == "Props" || name == "Assets")
                {
                    assemblies[name] = asm;
                }
            }

            // SECTION 1: All prop-related types with COMPLETE information
            _output.AppendLine("=" .PadRight(80, '='));
            _output.AppendLine("# SECTION 1: ALL PROP-RELATED TYPES (COMPLETE)");
            _output.AppendLine("=".PadRight(80, '='));
            _output.AppendLine();

            foreach (var kvp in assemblies)
            {
                AnalyzeAssemblyPropTypes(kvp.Key, kvp.Value);
            }

            // SECTION 2: PropLibrary - COMPLETE ANALYSIS
            _output.AppendLine("=".PadRight(80, '='));
            _output.AppendLine("# SECTION 2: PropLibrary COMPLETE ANALYSIS");
            _output.AppendLine("=".PadRight(80, '='));
            _output.AppendLine();

            if (assemblies.TryGetValue("Gameplay", out var gameplayAsm))
            {
                AnalyzePropLibraryComplete(gameplayAsm);
            }

            // SECTION 3: StageManager - COMPLETE ANALYSIS
            _output.AppendLine("=".PadRight(80, '='));
            _output.AppendLine("# SECTION 3: StageManager COMPLETE ANALYSIS");
            _output.AppendLine("=".PadRight(80, '='));
            _output.AppendLine();

            if (assemblies.TryGetValue("Gameplay", out gameplayAsm))
            {
                AnalyzeStageManagerComplete(gameplayAsm);
            }

            // SECTION 4: Stage - COMPLETE ANALYSIS
            _output.AppendLine("=".PadRight(80, '='));
            _output.AppendLine("# SECTION 4: Stage COMPLETE ANALYSIS");
            _output.AppendLine("=".PadRight(80, '='));
            _output.AppendLine();

            if (assemblies.TryGetValue("Gameplay", out gameplayAsm))
            {
                AnalyzeStageComplete(gameplayAsm);
            }

            // SECTION 5: UI Components - COMPLETE ANALYSIS
            _output.AppendLine("=".PadRight(80, '='));
            _output.AppendLine("# SECTION 5: UI COMPONENTS COMPLETE ANALYSIS");
            _output.AppendLine("=".PadRight(80, '='));
            _output.AppendLine();

            if (assemblies.TryGetValue("Creator", out var creatorAsm))
            {
                AnalyzeUIComponentsComplete(creatorAsm);
            }

            // SECTION 6: Prop Asset Type - COMPLETE ANALYSIS
            _output.AppendLine("=".PadRight(80, '='));
            _output.AppendLine("# SECTION 6: Prop ASSET TYPE COMPLETE ANALYSIS");
            _output.AppendLine("=".PadRight(80, '='));
            _output.AppendLine();

            if (assemblies.TryGetValue("Props", out var propsAsm))
            {
                AnalyzePropAssetComplete(propsAsm);
            }

            // SECTION 7: RuntimePropInfo - COMPLETE ANALYSIS
            _output.AppendLine("=".PadRight(80, '='));
            _output.AppendLine("# SECTION 7: RuntimePropInfo COMPLETE ANALYSIS");
            _output.AppendLine("=".PadRight(80, '='));
            _output.AppendLine();

            if (assemblies.TryGetValue("Gameplay", out gameplayAsm))
            {
                AnalyzeRuntimePropInfoComplete(gameplayAsm);
            }

            // SECTION 8: InjectedProps - COMPLETE ANALYSIS
            _output.AppendLine("=".PadRight(80, '='));
            _output.AppendLine("# SECTION 8: InjectedProps COMPLETE ANALYSIS");
            _output.AppendLine("=".PadRight(80, '='));
            _output.AppendLine();

            if (assemblies.TryGetValue("Gameplay", out gameplayAsm))
            {
                AnalyzeInjectedPropsComplete(gameplayAsm);
            }

            // SECTION 9: Method Call Graph for InjectProp
            _output.AppendLine("=".PadRight(80, '='));
            _output.AppendLine("# SECTION 9: METHOD IL ANALYSIS");
            _output.AppendLine("=".PadRight(80, '='));
            _output.AppendLine();

            if (assemblies.TryGetValue("Gameplay", out gameplayAsm))
            {
                AnalyzeMethodIL(gameplayAsm);
            }

            // SECTION 10: Cross-references
            _output.AppendLine("=".PadRight(80, '='));
            _output.AppendLine("# SECTION 10: CROSS-REFERENCES - WHO USES WHAT");
            _output.AppendLine("=".PadRight(80, '='));
            _output.AppendLine();

            AnalyzeCrossReferences(assemblies);

            File.WriteAllText(outputPath, _output.ToString());
        }

        private static void AnalyzeAssemblyPropTypes(string asmName, Assembly asm)
        {
            _output.AppendLine($"## Assembly: {asmName}");
            _output.AppendLine();

            try
            {
                var propTypes = asm.GetTypes()
                    .Where(t => t.FullName != null &&
                           (t.Name.Contains("Prop") || t.Name.Contains("RuntimePropInfo") ||
                            t.Name == "Stage" || t.Name == "StageManager" ||
                            t.Name.Contains("Library") && t.Namespace?.Contains("LevelEditing") == true))
                    .Where(t => !t.Name.Contains("<") && !t.Name.Contains(">"))
                    .OrderBy(t => t.FullName)
                    .ToList();

                _output.AppendLine($"Found {propTypes.Count} relevant types:");
                _output.AppendLine();

                foreach (var type in propTypes)
                {
                    _output.AppendLine($"- `{type.FullName}`");
                }
                _output.AppendLine();
            }
            catch (Exception ex)
            {
                _output.AppendLine($"ERROR: {ex.Message}");
            }
        }

        private static void AnalyzePropLibraryComplete(Assembly asm)
        {
            var type = asm.GetType("Endless.Gameplay.LevelEditing.PropLibrary");
            if (type == null)
            {
                _output.AppendLine("PropLibrary NOT FOUND");
                return;
            }

            AnalyzeTypeComplete(type, "PropLibrary");
        }

        private static void AnalyzeStageManagerComplete(Assembly asm)
        {
            var type = asm.GetType("Endless.Gameplay.LevelEditing.Level.StageManager");
            if (type == null)
            {
                _output.AppendLine("StageManager NOT FOUND");
                return;
            }

            AnalyzeTypeComplete(type, "StageManager");
        }

        private static void AnalyzeStageComplete(Assembly asm)
        {
            var type = asm.GetType("Endless.Gameplay.LevelEditing.Level.Stage");
            if (type == null)
            {
                _output.AppendLine("Stage NOT FOUND");
                return;
            }

            AnalyzeTypeComplete(type, "Stage");
        }

        private static void AnalyzeUIComponentsComplete(Assembly asm)
        {
            var types = new[]
            {
                "Endless.Creator.UI.UIRuntimePropInfoListModel",
                "Endless.Creator.UI.UIRuntimePropInfoListController",
                "Endless.Creator.UI.UIRuntimePropInfoListView",
                "Endless.Creator.UI.UIPropToolPanelController",
                "Endless.Creator.UI.UIRuntimePropInfoPresenter"
            };

            foreach (var typeName in types)
            {
                var type = asm.GetType(typeName);
                if (type != null)
                {
                    AnalyzeTypeComplete(type, type.Name);
                }
            }
        }

        private static void AnalyzePropAssetComplete(Assembly asm)
        {
            var type = asm.GetType("Endless.Props.Assets.Prop");
            if (type == null)
            {
                _output.AppendLine("Prop asset type NOT FOUND");
                return;
            }

            AnalyzeTypeComplete(type, "Prop (Asset)");

            // Also analyze base types
            var currentBase = type.BaseType;
            while (currentBase != null && currentBase != typeof(object))
            {
                _output.AppendLine($"### Base Type: {currentBase.FullName}");
                AnalyzeTypeComplete(currentBase, currentBase.Name);
                currentBase = currentBase.BaseType;
            }
        }

        private static void AnalyzeRuntimePropInfoComplete(Assembly asm)
        {
            var propLibType = asm.GetType("Endless.Gameplay.LevelEditing.PropLibrary");
            if (propLibType == null) return;

            var rpiType = propLibType.GetNestedType("RuntimePropInfo", BindingFlags.Public | BindingFlags.NonPublic);
            if (rpiType == null)
            {
                _output.AppendLine("RuntimePropInfo NOT FOUND");
                return;
            }

            AnalyzeTypeComplete(rpiType, "RuntimePropInfo");
        }

        private static void AnalyzeInjectedPropsComplete(Assembly asm)
        {
            var type = asm.GetType("Endless.Gameplay.LevelEditing.Level.InjectedProps");
            if (type == null)
            {
                _output.AppendLine("InjectedProps NOT FOUND - searching nested types...");

                // Search in Stage
                var stageType = asm.GetType("Endless.Gameplay.LevelEditing.Level.Stage");
                if (stageType != null)
                {
                    type = stageType.GetNestedType("InjectedProps", BindingFlags.Public | BindingFlags.NonPublic);
                }
            }

            if (type == null)
            {
                _output.AppendLine("InjectedProps still NOT FOUND");
                return;
            }

            AnalyzeTypeComplete(type, "InjectedProps");
        }

        private static void AnalyzeTypeComplete(Type type, string displayName)
        {
            if (_analyzedTypes.Contains(type.FullName)) return;
            _analyzedTypes.Add(type.FullName);

            _output.AppendLine($"### {displayName}");
            _output.AppendLine($"**Full Name:** `{type.FullName}`");
            _output.AppendLine();

            // Classification
            _output.AppendLine("**Classification:**");
            if (type.IsClass) _output.AppendLine("- Class");
            if (type.IsValueType && !type.IsEnum) _output.AppendLine("- Struct");
            if (type.IsEnum) _output.AppendLine("- Enum");
            if (type.IsInterface) _output.AppendLine("- Interface");
            if (type.IsAbstract) _output.AppendLine("- Abstract");
            if (type.IsSealed) _output.AppendLine("- Sealed");
            if (type.IsNested) _output.AppendLine($"- Nested in: {type.DeclaringType?.Name}");

            // Check for Unity base types
            var baseChain = GetBaseTypeChain(type);
            if (baseChain.Contains("MonoBehaviour")) _output.AppendLine("- **Unity MonoBehaviour**");
            if (baseChain.Contains("ScriptableObject")) _output.AppendLine("- **Unity ScriptableObject**");
            if (baseChain.Contains("MonoBehaviourSingleton")) _output.AppendLine("- **Singleton Pattern**");
            _output.AppendLine();

            // Inheritance
            if (type.BaseType != null && type.BaseType != typeof(object))
            {
                _output.AppendLine("**Inheritance Chain:**");
                _output.AppendLine($"```");
                _output.AppendLine(string.Join(" -> ", baseChain));
                _output.AppendLine($"```");
                _output.AppendLine();
            }

            // Interfaces
            var interfaces = type.GetInterfaces();
            if (interfaces.Length > 0)
            {
                _output.AppendLine("**Implements:**");
                foreach (var iface in interfaces.Take(10))
                {
                    _output.AppendLine($"- `{iface.Name}`");
                }
                if (interfaces.Length > 10) _output.AppendLine($"- ... and {interfaces.Length - 10} more");
                _output.AppendLine();
            }

            // Constructors
            var ctors = type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (ctors.Length > 0)
            {
                _output.AppendLine("**Constructors:**");
                foreach (var ctor in ctors)
                {
                    var access = GetAccessModifier(ctor);
                    var pars = FormatParameters(ctor.GetParameters());
                    _output.AppendLine($"- `{access} {type.Name}({pars})`");
                }
                _output.AppendLine();
            }

            // Fields (ALL)
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);
            if (fields.Length > 0)
            {
                _output.AppendLine("**Fields:**");
                _output.AppendLine("```");
                foreach (var f in fields.OrderBy(f => f.Name))
                {
                    var access = f.IsPublic ? "public" : f.IsPrivate ? "private" : "protected";
                    var stat = f.IsStatic ? "static " : "";
                    _output.AppendLine($"{access} {stat}{f.FieldType.Name} {f.Name}");
                }
                _output.AppendLine("```");
                _output.AppendLine();
            }

            // Properties (ALL)
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);
            if (props.Length > 0)
            {
                _output.AppendLine("**Properties:**");
                _output.AppendLine("```");
                foreach (var p in props.OrderBy(p => p.Name))
                {
                    var access = p.GetMethod?.IsPublic == true ? "public" : "private";
                    _output.AppendLine($"{access} {p.PropertyType.Name} {p.Name} {{ get; set; }}");
                }
                _output.AppendLine("```");
                _output.AppendLine();
            }

            // Methods (ALL with full signatures)
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Where(m => !m.IsSpecialName)
                .OrderBy(m => m.Name)
                .ToList();

            if (methods.Count > 0)
            {
                _output.AppendLine("**Methods:**");
                _output.AppendLine("```");
                foreach (var m in methods)
                {
                    if (m.Name.StartsWith("<")) continue; // Skip compiler-generated

                    var access = GetAccessModifier(m);
                    var stat = m.IsStatic ? "static " : "";
                    var virt = m.IsVirtual ? "virtual " : "";
                    var async = m.Name.Contains("Async") || m.ReturnType.Name.Contains("Task") ? "async " : "";
                    var pars = FormatParameters(m.GetParameters());
                    _output.AppendLine($"{access} {stat}{virt}{async}{m.ReturnType.Name} {m.Name}({pars})");
                }
                _output.AppendLine("```");
                _output.AppendLine();
            }

            // Events
            var events = type.GetEvents(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);
            if (events.Length > 0)
            {
                _output.AppendLine("**Events:**");
                foreach (var e in events)
                {
                    _output.AppendLine($"- `{e.Name}` ({e.EventHandlerType?.Name})");
                }
                _output.AppendLine();
            }

            // Nested Types
            var nested = type.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic);
            if (nested.Length > 0)
            {
                _output.AppendLine("**Nested Types:**");
                foreach (var nt in nested)
                {
                    if (nt.Name.StartsWith("<")) continue;
                    _output.AppendLine($"- `{nt.Name}`");
                }
                _output.AppendLine();
            }

            _output.AppendLine("---");
            _output.AppendLine();
        }

        private static void AnalyzeMethodIL(Assembly asm)
        {
            _output.AppendLine("## Key Method Analysis");
            _output.AppendLine();

            // PropLibrary.InjectProp
            var propLibType = asm.GetType("Endless.Gameplay.LevelEditing.PropLibrary");
            if (propLibType != null)
            {
                var injectMethod = propLibType.GetMethod("InjectProp",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (injectMethod != null)
                {
                    AnalyzeMethodDetails(injectMethod, "PropLibrary.InjectProp");
                }

                var getAllMethod = propLibType.GetMethod("GetAllRuntimeProps",
                    BindingFlags.Public | BindingFlags.Instance);
                if (getAllMethod != null)
                {
                    AnalyzeMethodDetails(getAllMethod, "PropLibrary.GetAllRuntimeProps");
                }
            }

            // StageManager.InjectProp
            var stageManagerType = asm.GetType("Endless.Gameplay.LevelEditing.Level.StageManager");
            if (stageManagerType != null)
            {
                var injectMethod = stageManagerType.GetMethod("InjectProp",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (injectMethod != null)
                {
                    AnalyzeMethodDetails(injectMethod, "StageManager.InjectProp");
                }
            }
        }

        private static void AnalyzeMethodDetails(MethodInfo method, string name)
        {
            _output.AppendLine($"### {name}");
            _output.AppendLine();

            _output.AppendLine("**Signature:**");
            _output.AppendLine($"```csharp");
            var pars = FormatParameters(method.GetParameters());
            _output.AppendLine($"{method.ReturnType.Name} {method.Name}({pars})");
            _output.AppendLine($"```");
            _output.AppendLine();

            _output.AppendLine("**Parameters:**");
            foreach (var p in method.GetParameters())
            {
                _output.AppendLine($"- `{p.Name}`: `{p.ParameterType.FullName}`");
                if (p.HasDefaultValue) _output.AppendLine($"  - Default: {p.DefaultValue}");
                if (p.IsOptional) _output.AppendLine($"  - Optional");
                if (p.IsOut) _output.AppendLine($"  - Out parameter");
                if (p.IsIn) _output.AppendLine($"  - In parameter");
            }
            _output.AppendLine();

            // Try to get method body info
            try
            {
                var body = method.GetMethodBody();
                if (body != null)
                {
                    _output.AppendLine("**Method Body Info:**");
                    _output.AppendLine($"- Local variables: {body.LocalVariables.Count}");
                    _output.AppendLine($"- Max stack size: {body.MaxStackSize}");
                    _output.AppendLine($"- IL size: {body.GetILAsByteArray()?.Length ?? 0} bytes");

                    if (body.LocalVariables.Count > 0)
                    {
                        _output.AppendLine("- Local variable types:");
                        foreach (var local in body.LocalVariables.Take(20))
                        {
                            _output.AppendLine($"  - [{local.LocalIndex}] {local.LocalType.Name}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _output.AppendLine($"Could not analyze method body: {ex.Message}");
            }
            _output.AppendLine();
        }

        private static void AnalyzeCrossReferences(Dictionary<string, Assembly> assemblies)
        {
            _output.AppendLine("## Who calls InjectProp?");
            _output.AppendLine();

            foreach (var kvp in assemblies)
            {
                try
                {
                    var types = kvp.Value.GetTypes();
                    foreach (var type in types)
                    {
                        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic |
                            BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);

                        foreach (var method in methods)
                        {
                            if (method.Name.Contains("InjectProp") && !method.Name.StartsWith("<"))
                            {
                                var pars = string.Join(", ", method.GetParameters().Select(p => p.ParameterType.Name));
                                _output.AppendLine($"- `{type.FullName}.{method.Name}({pars})`");
                            }
                        }
                    }
                }
                catch { }
            }

            _output.AppendLine();
            _output.AppendLine("## Who uses loadedPropMap?");
            _output.AppendLine();

            // This would require IL analysis to properly trace

            _output.AppendLine("## Who uses RuntimePropInfo?");
            _output.AppendLine();

            foreach (var kvp in assemblies)
            {
                try
                {
                    var types = kvp.Value.GetTypes();
                    foreach (var type in types)
                    {
                        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic |
                            BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);

                        foreach (var method in methods)
                        {
                            if (method.Name.StartsWith("<")) continue;

                            bool usesRPI = method.ReturnType.Name.Contains("RuntimePropInfo") ||
                                          method.GetParameters().Any(p => p.ParameterType.Name.Contains("RuntimePropInfo"));

                            if (usesRPI)
                            {
                                var pars = string.Join(", ", method.GetParameters().Select(p => p.ParameterType.Name));
                                _output.AppendLine($"- `{type.Name}.{method.Name}({pars})` -> {method.ReturnType.Name}");
                            }
                        }
                    }
                }
                catch { }
            }
        }

        private static List<string> GetBaseTypeChain(Type type)
        {
            var chain = new List<string> { type.Name };
            var current = type.BaseType;
            while (current != null && current != typeof(object))
            {
                chain.Add(current.Name);
                current = current.BaseType;
            }
            return chain;
        }

        private static string GetAccessModifier(MethodBase method)
        {
            if (method.IsPublic) return "public";
            if (method.IsPrivate) return "private";
            if (method.IsFamily) return "protected";
            if (method.IsAssembly) return "internal";
            return "private";
        }

        private static string FormatParameters(ParameterInfo[] parameters)
        {
            return string.Join(", ", parameters.Select(p =>
            {
                var prefix = p.IsOut ? "out " : p.IsIn ? "in " : p.ParameterType.IsByRef ? "ref " : "";
                var typeName = p.ParameterType.Name.Replace("&", "");
                return $"{prefix}{typeName} {p.Name}";
            }));
        }
    }
}

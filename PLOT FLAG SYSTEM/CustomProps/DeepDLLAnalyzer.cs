using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CustomProps
{
    /// <summary>
    /// Deep analysis of Endstar DLLs to understand prop system flow
    /// </summary>
    public static class DeepDLLAnalyzer
    {
        public static void AnalyzePropSystem(string outputPath)
        {
            var sb = new StringBuilder();
            sb.AppendLine("# DEEP ENDSTAR PROP SYSTEM ANALYSIS");
            sb.AppendLine($"Generated: {DateTime.Now}");
            sb.AppendLine();
            sb.AppendLine("## Purpose: Understand EXACTLY how props flow from injection to UI display");
            sb.AppendLine();

            // Get assemblies
            var gameplayAsm = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == "Gameplay");
            var creatorAsm = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == "Creator");
            var propsAsm = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == "Props");

            if (gameplayAsm == null || creatorAsm == null)
            {
                sb.AppendLine("ERROR: Could not find Gameplay or Creator assemblies");
                File.WriteAllText(outputPath, sb.ToString());
                return;
            }

            // 1. ANALYZE StageManager.InjectProp - what does it actually do?
            sb.AppendLine("---");
            sb.AppendLine("## 1. StageManager.InjectProp Analysis");
            sb.AppendLine();
            AnalyzeStageManagerInjectProp(gameplayAsm, sb);

            // 2. ANALYZE PropLibrary - how are props stored?
            sb.AppendLine("---");
            sb.AppendLine("## 2. PropLibrary Internal Structure");
            sb.AppendLine();
            AnalyzePropLibrary(gameplayAsm, sb);

            // 3. ANALYZE UIRuntimePropInfoListModel - how does UI get props?
            sb.AppendLine("---");
            sb.AppendLine("## 3. UI Prop List Model Analysis");
            sb.AppendLine();
            AnalyzeUIListModel(creatorAsm, sb);

            // 4. ANALYZE UIRuntimePropInfoListController
            sb.AppendLine("---");
            sb.AppendLine("## 4. UI List Controller Analysis");
            sb.AppendLine();
            AnalyzeUIListController(creatorAsm, sb);

            // 5. ANALYZE UIPropToolPanelController - the main prop panel
            sb.AppendLine("---");
            sb.AppendLine("## 5. Prop Tool Panel Controller");
            sb.AppendLine();
            AnalyzePropToolPanel(creatorAsm, sb);

            // 6. Find ALL methods that reference RuntimePropInfo
            sb.AppendLine("---");
            sb.AppendLine("## 6. All Methods Referencing RuntimePropInfo");
            sb.AppendLine();
            FindRuntimePropInfoReferences(gameplayAsm, creatorAsm, sb);

            // 7. Find ALL methods that modify loadedPropMap
            sb.AppendLine("---");
            sb.AppendLine("## 7. Methods That Could Modify Prop Storage");
            sb.AppendLine();
            FindPropStorageModifiers(gameplayAsm, sb);

            // 8. Analyze InjectedProps struct
            sb.AppendLine("---");
            sb.AppendLine("## 8. InjectedProps Struct Analysis");
            sb.AppendLine();
            AnalyzeInjectedProps(gameplayAsm, sb);

            // 9. Look for event subscriptions related to props
            sb.AppendLine("---");
            sb.AppendLine("## 9. Prop-Related Events and Callbacks");
            sb.AppendLine();
            FindPropEvents(gameplayAsm, creatorAsm, sb);

            // 10. Trace the full flow
            sb.AppendLine("---");
            sb.AppendLine("## 10. Hypothesized Prop Flow");
            sb.AppendLine();
            sb.AppendLine("Based on analysis above, document the prop flow here after reviewing.");

            File.WriteAllText(outputPath, sb.ToString());
        }

        private static void AnalyzeStageManagerInjectProp(Assembly asm, StringBuilder sb)
        {
            var stageManagerType = asm.GetType("Endless.Gameplay.LevelEditing.Level.StageManager");
            if (stageManagerType == null)
            {
                sb.AppendLine("StageManager type not found!");
                return;
            }

            sb.AppendLine($"### StageManager: `{stageManagerType.FullName}`");
            sb.AppendLine();

            // Get ALL InjectProp overloads
            var injectMethods = stageManagerType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(m => m.Name == "InjectProp")
                .ToList();

            sb.AppendLine($"Found {injectMethods.Count} InjectProp method(s):");
            sb.AppendLine();

            foreach (var method in injectMethods)
            {
                var pars = method.GetParameters();
                sb.AppendLine($"#### `{method.ReturnType.Name} InjectProp({string.Join(", ", pars.Select(p => $"{p.ParameterType.Name} {p.Name}"))})`");
                sb.AppendLine();
                sb.AppendLine("Parameters:");
                foreach (var p in pars)
                {
                    sb.AppendLine($"- `{p.Name}`: `{p.ParameterType.FullName}`");
                    if (p.HasDefaultValue)
                        sb.AppendLine($"  - Default: `{p.DefaultValue}`");
                    if (p.IsOptional)
                        sb.AppendLine($"  - Optional: true");
                }
                sb.AppendLine();

                // Check what fields StageManager has that might be used
                sb.AppendLine("StageManager fields that might be used:");
                var fields = stageManagerType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (var f in fields)
                {
                    if (f.FieldType.Name.Contains("Prop") || f.FieldType.Name.Contains("Library"))
                    {
                        sb.AppendLine($"- `{f.FieldType.Name} {f.Name}`");
                    }
                }
                sb.AppendLine();
            }

            // Check for activePropLibrary field
            var propLibField = stageManagerType.GetField("activePropLibrary",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (propLibField != null)
            {
                sb.AppendLine($"### activePropLibrary field: `{propLibField.FieldType.FullName}`");
                sb.AppendLine();
            }
        }

        private static void AnalyzePropLibrary(Assembly asm, StringBuilder sb)
        {
            var propLibType = asm.GetType("Endless.Gameplay.LevelEditing.PropLibrary");
            if (propLibType == null)
            {
                sb.AppendLine("PropLibrary type not found!");
                return;
            }

            sb.AppendLine($"### PropLibrary: `{propLibType.FullName}`");
            sb.AppendLine();

            // ALL fields
            sb.AppendLine("#### All Fields:");
            var fields = propLibType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var f in fields)
            {
                sb.AppendLine($"- `{f.FieldType.Name} {f.Name}` ({(f.IsPrivate ? "private" : "public")})");
            }
            sb.AppendLine();

            // ALL methods with full signatures
            sb.AppendLine("#### All Methods:");
            var methods = propLibType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(m => !m.IsSpecialName)
                .OrderBy(m => m.Name);
            foreach (var m in methods)
            {
                var pars = string.Join(", ", m.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
                sb.AppendLine($"- `{(m.IsPrivate ? "private" : "public")} {m.ReturnType.Name} {m.Name}({pars})`");
            }
            sb.AppendLine();

            // InjectProp specifically
            var injectMethod = propLibType.GetMethod("InjectProp",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (injectMethod != null)
            {
                sb.AppendLine("#### PropLibrary.InjectProp Details:");
                var pars = injectMethod.GetParameters();
                foreach (var p in pars)
                {
                    sb.AppendLine($"- `{p.Name}`: `{p.ParameterType.FullName}`");
                }
                sb.AppendLine();
            }

            // Nested types (RuntimePropInfo)
            sb.AppendLine("#### Nested Types:");
            var nestedTypes = propLibType.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var nt in nestedTypes)
            {
                sb.AppendLine($"- `{nt.Name}`");
                var ntFields = nt.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (var f in ntFields)
                {
                    sb.AppendLine($"  - `{f.FieldType.Name} {f.Name}`");
                }
            }
            sb.AppendLine();
        }

        private static void AnalyzeUIListModel(Assembly asm, StringBuilder sb)
        {
            var modelType = asm.GetType("Endless.Creator.UI.UIRuntimePropInfoListModel");
            if (modelType == null)
            {
                sb.AppendLine("UIRuntimePropInfoListModel not found!");
                return;
            }

            sb.AppendLine($"### UIRuntimePropInfoListModel: `{modelType.FullName}`");
            sb.AppendLine();

            // Base type chain
            sb.AppendLine("#### Inheritance:");
            var baseType = modelType.BaseType;
            while (baseType != null)
            {
                sb.AppendLine($"- `{baseType.FullName}`");
                baseType = baseType.BaseType;
            }
            sb.AppendLine();

            // ALL fields including inherited
            sb.AppendLine("#### All Fields (including inherited):");
            var currentType = modelType;
            while (currentType != null && currentType != typeof(object))
            {
                var fields = currentType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                if (fields.Length > 0)
                {
                    sb.AppendLine($"From `{currentType.Name}`:");
                    foreach (var f in fields)
                    {
                        sb.AppendLine($"  - `{f.FieldType.Name} {f.Name}`");
                    }
                }
                currentType = currentType.BaseType;
            }
            sb.AppendLine();

            // ALL methods including inherited that might set data
            sb.AppendLine("#### Methods that might set/update data:");
            currentType = modelType;
            while (currentType != null && !currentType.FullName.StartsWith("UnityEngine"))
            {
                var methods = currentType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                    .Where(m => !m.IsSpecialName &&
                           (m.Name.Contains("Set") || m.Name.Contains("Add") || m.Name.Contains("Update") ||
                            m.Name.Contains("Refresh") || m.Name.Contains("Load") || m.Name.Contains("Init") ||
                            m.Name.Contains("Populate") || m.Name.Contains("Bind")));
                foreach (var m in methods)
                {
                    var pars = string.Join(", ", m.GetParameters().Select(p => $"{p.ParameterType.Name}"));
                    sb.AppendLine($"- `{currentType.Name}.{m.Name}({pars})` -> `{m.ReturnType.Name}`");
                }
                currentType = currentType.BaseType;
            }
            sb.AppendLine();
        }

        private static void AnalyzeUIListController(Assembly asm, StringBuilder sb)
        {
            var controllerType = asm.GetType("Endless.Creator.UI.UIRuntimePropInfoListController");
            if (controllerType == null)
            {
                sb.AppendLine("UIRuntimePropInfoListController not found!");
                return;
            }

            sb.AppendLine($"### UIRuntimePropInfoListController: `{controllerType.FullName}`");
            sb.AppendLine();

            // Inheritance
            sb.AppendLine("#### Inheritance:");
            var baseType = controllerType.BaseType;
            while (baseType != null)
            {
                sb.AppendLine($"- `{baseType.FullName}`");
                baseType = baseType.BaseType;
            }
            sb.AppendLine();

            // Fields
            sb.AppendLine("#### Fields:");
            var currentType = controllerType;
            while (currentType != null && !currentType.FullName.StartsWith("UnityEngine"))
            {
                var fields = currentType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                foreach (var f in fields)
                {
                    sb.AppendLine($"- `{f.FieldType.Name} {f.Name}` (from {currentType.Name})");
                }
                currentType = currentType.BaseType;
            }
            sb.AppendLine();
        }

        private static void AnalyzePropToolPanel(Assembly asm, StringBuilder sb)
        {
            var panelType = asm.GetType("Endless.Creator.UI.UIPropToolPanelController");
            if (panelType == null)
            {
                sb.AppendLine("UIPropToolPanelController not found!");
                return;
            }

            sb.AppendLine($"### UIPropToolPanelController: `{panelType.FullName}`");
            sb.AppendLine();

            // ALL fields
            sb.AppendLine("#### Fields:");
            var fields = panelType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var f in fields)
            {
                sb.AppendLine($"- `{f.FieldType.Name} {f.Name}`");
            }
            sb.AppendLine();

            // ALL methods
            sb.AppendLine("#### Methods:");
            var methods = panelType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(m => !m.IsSpecialName);
            foreach (var m in methods)
            {
                var pars = string.Join(", ", m.GetParameters().Select(p => $"{p.ParameterType.Name}"));
                sb.AppendLine($"- `{m.Name}({pars})` -> `{m.ReturnType.Name}`");
            }
            sb.AppendLine();
        }

        private static void FindRuntimePropInfoReferences(Assembly gameplayAsm, Assembly creatorAsm, StringBuilder sb)
        {
            var runtimePropInfoType = gameplayAsm.GetType("Endless.Gameplay.LevelEditing.PropLibrary+RuntimePropInfo");
            if (runtimePropInfoType == null)
            {
                sb.AppendLine("RuntimePropInfo type not found!");
                return;
            }

            sb.AppendLine("### Methods with RuntimePropInfo in signature:");
            sb.AppendLine();

            foreach (var asm in new[] { gameplayAsm, creatorAsm })
            {
                sb.AppendLine($"#### In {asm.GetName().Name}:");
                try
                {
                    foreach (var type in asm.GetTypes())
                    {
                        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);
                        foreach (var m in methods)
                        {
                            bool hasRPI = m.ReturnType == runtimePropInfoType ||
                                         m.ReturnType.FullName?.Contains("RuntimePropInfo") == true ||
                                         m.GetParameters().Any(p => p.ParameterType == runtimePropInfoType ||
                                                                   p.ParameterType.FullName?.Contains("RuntimePropInfo") == true);
                            if (hasRPI && !m.Name.StartsWith("<"))
                            {
                                var pars = string.Join(", ", m.GetParameters().Select(p => p.ParameterType.Name));
                                sb.AppendLine($"- `{type.Name}.{m.Name}({pars})` -> `{m.ReturnType.Name}`");
                            }
                        }
                    }
                }
                catch { }
                sb.AppendLine();
            }
        }

        private static void FindPropStorageModifiers(Assembly asm, StringBuilder sb)
        {
            sb.AppendLine("### Methods that might modify prop storage:");
            sb.AppendLine();

            var propLibType = asm.GetType("Endless.Gameplay.LevelEditing.PropLibrary");
            if (propLibType == null) return;

            var methods = propLibType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(m => m.Name.Contains("Add") || m.Name.Contains("Inject") ||
                           m.Name.Contains("Register") || m.Name.Contains("Set") ||
                           m.Name.Contains("Load") || m.Name.Contains("Insert"));

            foreach (var m in methods)
            {
                var pars = string.Join(", ", m.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
                sb.AppendLine($"- `{m.Name}({pars})` -> `{m.ReturnType.Name}`");
            }
            sb.AppendLine();
        }

        private static void AnalyzeInjectedProps(Assembly asm, StringBuilder sb)
        {
            var injectedType = asm.GetType("Endless.Gameplay.LevelEditing.Level.InjectedProps");
            if (injectedType == null)
            {
                // Try nested
                var stageType = asm.GetType("Endless.Gameplay.LevelEditing.Level.Stage");
                if (stageType != null)
                {
                    injectedType = stageType.GetNestedType("InjectedProps", BindingFlags.Public | BindingFlags.NonPublic);
                }
            }

            if (injectedType == null)
            {
                sb.AppendLine("InjectedProps type not found!");
                return;
            }

            sb.AppendLine($"### InjectedProps: `{injectedType.FullName}`");
            sb.AppendLine();

            sb.AppendLine("#### Fields:");
            var fields = injectedType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var f in fields)
            {
                sb.AppendLine($"- `{f.FieldType.FullName} {f.Name}`");
            }
            sb.AppendLine();

            sb.AppendLine("#### Constructors:");
            var ctors = injectedType.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var c in ctors)
            {
                var pars = string.Join(", ", c.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
                sb.AppendLine($"- `InjectedProps({pars})`");
            }
            sb.AppendLine();
        }

        private static void FindPropEvents(Assembly gameplayAsm, Assembly creatorAsm, StringBuilder sb)
        {
            sb.AppendLine("### Events related to props:");
            sb.AppendLine();

            foreach (var asm in new[] { gameplayAsm, creatorAsm })
            {
                sb.AppendLine($"#### In {asm.GetName().Name}:");
                try
                {
                    foreach (var type in asm.GetTypes())
                    {
                        if (!type.FullName.Contains("Prop")) continue;

                        var events = type.GetEvents(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                        foreach (var e in events)
                        {
                            sb.AppendLine($"- `{type.Name}.{e.Name}` ({e.EventHandlerType?.Name})");
                        }

                        // Also check for UnityEvent fields
                        var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                            .Where(f => f.FieldType.Name.Contains("Event") || f.FieldType.Name.Contains("Action"));
                        foreach (var f in fields)
                        {
                            sb.AppendLine($"- `{type.Name}.{f.Name}` ({f.FieldType.Name})");
                        }
                    }
                }
                catch { }
            }
            sb.AppendLine();
        }
    }
}

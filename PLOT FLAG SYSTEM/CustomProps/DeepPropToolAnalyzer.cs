using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

/// <summary>
/// Deep analyzer for Endstar's Prop Tool UI system.
/// Run this to understand how the prop tool panel works.
/// </summary>
public class DeepPropToolAnalyzer
{
    private static StringBuilder report = new StringBuilder();
    private static string managedPath = @"C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed";

    public static void Main(string[] args)
    {
        Console.WriteLine("=== ENDSTAR PROP TOOL UI DEEP ANALYSIS ===\n");

        try
        {
            // Load key assemblies
            var creatorDll = Assembly.LoadFrom(Path.Combine(managedPath, "Creator.dll"));
            var gameplayDll = Assembly.LoadFrom(Path.Combine(managedPath, "Gameplay.dll"));
            var propsDll = Assembly.LoadFrom(Path.Combine(managedPath, "Props.dll"));
            var assetsDll = Assembly.LoadFrom(Path.Combine(managedPath, "Assets.dll"));
            var coreDll = Assembly.LoadFrom(Path.Combine(managedPath, "Core.dll"));

            Report("LOADED ASSEMBLIES:");
            Report($"  Creator.dll: {creatorDll.GetTypes().Length} types");
            Report($"  Gameplay.dll: {gameplayDll.GetTypes().Length} types");
            Report($"  Props.dll: {propsDll.GetTypes().Length} types");
            Report($"  Assets.dll: {assetsDll.GetTypes().Length} types");
            Report($"  Core.dll: {coreDll.GetTypes().Length} types");
            Report("");

            // SECTION 1: Find all UI classes related to props
            Report("=" .PadRight(80, '='));
            Report("SECTION 1: PROP TOOL UI CLASSES (Creator.dll)");
            Report("=" .PadRight(80, '='));
            AnalyzePropToolUIClasses(creatorDll);

            // SECTION 2: Analyze UIPropToolPanelView in detail
            Report("\n" + "=" .PadRight(80, '='));
            Report("SECTION 2: UIPropToolPanelView DETAILED ANALYSIS");
            Report("=" .PadRight(80, '='));
            AnalyzeUIPropToolPanelView(creatorDll);

            // SECTION 3: Analyze UIRuntimePropInfoListModel
            Report("\n" + "=" .PadRight(80, '='));
            Report("SECTION 3: UIRuntimePropInfoListModel DETAILED ANALYSIS");
            Report("=" .PadRight(80, '='));
            AnalyzeUIRuntimePropInfoListModel(creatorDll);

            // SECTION 4: Find what opens/shows the prop tool
            Report("\n" + "=" .PadRight(80, '='));
            Report("SECTION 4: PROP TOOL OPENING MECHANISM");
            Report("=" .PadRight(80, '='));
            FindPropToolOpeningMechanism(creatorDll);

            // SECTION 5: Analyze Creator Mode / Tool Controller
            Report("\n" + "=" .PadRight(80, '='));
            Report("SECTION 5: CREATOR MODE / TOOL CONTROLLER");
            Report("=" .PadRight(80, '='));
            AnalyzeCreatorModeController(creatorDll);

            // SECTION 6: Find UI initialization chain
            Report("\n" + "=" .PadRight(80, '='));
            Report("SECTION 6: UI INITIALIZATION CHAIN");
            Report("=" .PadRight(80, '='));
            AnalyzeUIInitializationChain(creatorDll);

            // SECTION 7: Analyze PropLibrary <-> UI connection
            Report("\n" + "=" .PadRight(80, '='));
            Report("SECTION 7: PropLibrary TO UI CONNECTION");
            Report("=" .PadRight(80, '='));
            AnalyzePropLibraryUIConnection(gameplayDll, creatorDll);

            // SECTION 8: Find all event handlers for prop tool
            Report("\n" + "=" .PadRight(80, '='));
            Report("SECTION 8: EVENT HANDLERS AND CALLBACKS");
            Report("=" .PadRight(80, '='));
            FindEventHandlers(creatorDll);

            // SECTION 9: Analyze ToolManager / ToolState classes
            Report("\n" + "=" .PadRight(80, '='));
            Report("SECTION 9: TOOL MANAGER / TOOL STATE ANALYSIS");
            Report("=" .PadRight(80, '='));
            AnalyzeToolManager(creatorDll, gameplayDll);

            // SECTION 10: Find MonoBehaviour lifecycle methods
            Report("\n" + "=" .PadRight(80, '='));
            Report("SECTION 10: MONOBEHAVIOUR LIFECYCLE METHODS");
            Report("=" .PadRight(80, '='));
            FindMonoBehaviourLifecycleMethods(creatorDll);

            // Save report
            string reportPath = Path.Combine(
                @"D:\Endstar Plot Flag\PLOT FLAG SYSTEM\Zayed_Character_Mod\CustomProps",
                "PROP_TOOL_UI_ANALYSIS.md");
            File.WriteAllText(reportPath, report.ToString());

            Console.WriteLine(report.ToString());
            Console.WriteLine($"\n\nReport saved to: {reportPath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }

    static void Report(string line)
    {
        report.AppendLine(line);
        Console.WriteLine(line);
    }

    static void AnalyzePropToolUIClasses(Assembly creatorDll)
    {
        var propUITypes = creatorDll.GetTypes()
            .Where(t => t.Name.Contains("Prop") || t.Name.Contains("Tool"))
            .OrderBy(t => t.Name)
            .ToList();

        Report($"\nFound {propUITypes.Count} types containing 'Prop' or 'Tool':\n");

        foreach (var type in propUITypes)
        {
            Report($"  {type.FullName}");
            if (type.BaseType != null && type.BaseType.Name != "Object")
                Report($"    └── Inherits: {type.BaseType.Name}");
        }

        // Also find UI types
        var uiTypes = creatorDll.GetTypes()
            .Where(t => t.Name.StartsWith("UI") && (t.Name.Contains("Panel") || t.Name.Contains("View") || t.Name.Contains("List")))
            .OrderBy(t => t.Name)
            .ToList();

        Report($"\n\nFound {uiTypes.Count} UI Panel/View/List types:\n");
        foreach (var type in uiTypes)
        {
            Report($"  {type.FullName}");
        }
    }

    static void AnalyzeUIPropToolPanelView(Assembly creatorDll)
    {
        var type = creatorDll.GetTypes().FirstOrDefault(t => t.Name == "UIPropToolPanelView");
        if (type == null)
        {
            Report("UIPropToolPanelView NOT FOUND - searching alternatives...");
            var alternatives = creatorDll.GetTypes()
                .Where(t => t.Name.Contains("PropTool") && t.Name.Contains("Panel"))
                .ToList();
            foreach (var alt in alternatives)
            {
                Report($"  Alternative found: {alt.FullName}");
                type = alt;
            }
        }

        if (type == null) return;

        AnalyzeTypeInDetail(type, "UIPropToolPanelView");
    }

    static void AnalyzeUIRuntimePropInfoListModel(Assembly creatorDll)
    {
        var type = creatorDll.GetTypes().FirstOrDefault(t => t.Name == "UIRuntimePropInfoListModel");
        if (type == null)
        {
            Report("UIRuntimePropInfoListModel NOT FOUND - searching alternatives...");
            var alternatives = creatorDll.GetTypes()
                .Where(t => t.Name.Contains("RuntimeProp") || t.Name.Contains("PropInfo"))
                .ToList();
            foreach (var alt in alternatives)
            {
                Report($"  Alternative found: {alt.FullName}");
            }
            type = alternatives.FirstOrDefault();
        }

        if (type == null) return;

        AnalyzeTypeInDetail(type, "UIRuntimePropInfoListModel");
    }

    static void FindPropToolOpeningMechanism(Assembly creatorDll)
    {
        // Search for methods that might open/show/activate the prop tool
        var searchTerms = new[] { "Open", "Show", "Activate", "Enable", "Display", "SetActive", "OnSelected" };

        foreach (var type in creatorDll.GetTypes())
        {
            if (!type.Name.Contains("Prop") && !type.Name.Contains("Tool")) continue;

            var relevantMethods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                .Where(m => searchTerms.Any(term => m.Name.Contains(term)))
                .ToList();

            if (relevantMethods.Any())
            {
                Report($"\n{type.Name}:");
                foreach (var method in relevantMethods)
                {
                    var parameters = string.Join(", ", method.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
                    Report($"  {method.ReturnType.Name} {method.Name}({parameters})");
                }
            }
        }
    }

    static void AnalyzeCreatorModeController(Assembly creatorDll)
    {
        var creatorTypes = creatorDll.GetTypes()
            .Where(t => t.Name.Contains("Creator") || t.Name.Contains("Editor") || t.Name.Contains("Mode"))
            .OrderBy(t => t.Name)
            .Take(30)
            .ToList();

        Report($"\nCreator/Editor/Mode types found: {creatorTypes.Count}\n");

        foreach (var type in creatorTypes)
        {
            Report($"  {type.FullName}");

            // Check for singleton pattern
            var instanceField = type.GetField("Instance", BindingFlags.Public | BindingFlags.Static);
            var instanceProp = type.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
            if (instanceField != null || instanceProp != null)
                Report($"    *** SINGLETON - has Instance ***");
        }
    }

    static void AnalyzeUIInitializationChain(Assembly creatorDll)
    {
        Report("\nLooking for Initialize/Setup/Init methods in UI types...\n");

        var uiTypes = creatorDll.GetTypes()
            .Where(t => t.Name.StartsWith("UI"))
            .ToList();

        foreach (var type in uiTypes)
        {
            var initMethods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(m => m.Name.Contains("Init") || m.Name.Contains("Setup") ||
                           m.Name == "Awake" || m.Name == "Start" || m.Name == "OnEnable")
                .ToList();

            if (initMethods.Any())
            {
                Report($"{type.Name}:");
                foreach (var method in initMethods)
                {
                    var parameters = string.Join(", ", method.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
                    Report($"  {method.Name}({parameters})");
                }
            }
        }
    }

    static void AnalyzePropLibraryUIConnection(Assembly gameplayDll, Assembly creatorDll)
    {
        // Find PropLibrary
        var propLibType = gameplayDll.GetTypes().FirstOrDefault(t => t.Name == "PropLibrary");
        if (propLibType != null)
        {
            Report("\nPropLibrary events and delegates:\n");

            var events = propLibType.GetEvents(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            foreach (var evt in events)
            {
                Report($"  Event: {evt.Name} ({evt.EventHandlerType?.Name})");
            }

            var delegates = propLibType.GetNestedTypes()
                .Where(t => typeof(Delegate).IsAssignableFrom(t))
                .ToList();
            foreach (var del in delegates)
            {
                Report($"  Delegate: {del.Name}");
            }
        }

        // Find what in Creator references PropLibrary
        Report("\nCreator types that reference PropLibrary:\n");

        foreach (var type in creatorDll.GetTypes())
        {
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            var propLibFields = fields.Where(f => f.FieldType.Name.Contains("PropLibrary")).ToList();

            if (propLibFields.Any())
            {
                Report($"  {type.Name}:");
                foreach (var field in propLibFields)
                {
                    Report($"    Field: {field.Name} ({field.FieldType.Name})");
                }
            }
        }
    }

    static void FindEventHandlers(Assembly creatorDll)
    {
        Report("\nEvent handlers in prop-related UI types:\n");

        var propUITypes = creatorDll.GetTypes()
            .Where(t => t.Name.Contains("Prop") ||
                       (t.Name.StartsWith("UI") && (t.Name.Contains("Tool") || t.Name.Contains("List"))))
            .ToList();

        foreach (var type in propUITypes)
        {
            var eventMethods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(m => m.Name.StartsWith("On") || m.Name.Contains("Handler") || m.Name.Contains("Callback"))
                .ToList();

            if (eventMethods.Any())
            {
                Report($"\n{type.Name}:");
                foreach (var method in eventMethods)
                {
                    var parameters = string.Join(", ", method.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
                    Report($"  {method.Name}({parameters})");
                }
            }
        }
    }

    static void AnalyzeToolManager(Assembly creatorDll, Assembly gameplayDll)
    {
        var toolTypes = creatorDll.GetTypes()
            .Concat(gameplayDll.GetTypes())
            .Where(t => t.Name.Contains("Tool") && (t.Name.Contains("Manager") || t.Name.Contains("State") || t.Name.Contains("Controller")))
            .OrderBy(t => t.Name)
            .ToList();

        Report($"\nTool Manager/State/Controller types: {toolTypes.Count}\n");

        foreach (var type in toolTypes)
        {
            Report($"\n{type.FullName}:");
            Report($"  Base: {type.BaseType?.Name ?? "None"}");

            // Fields
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                .Take(10)
                .ToList();
            if (fields.Any())
            {
                Report("  Fields:");
                foreach (var field in fields)
                {
                    Report($"    {field.FieldType.Name} {field.Name}");
                }
            }

            // Key methods
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(m => !m.IsSpecialName)
                .Take(10)
                .ToList();
            if (methods.Any())
            {
                Report("  Methods:");
                foreach (var method in methods)
                {
                    Report($"    {method.ReturnType.Name} {method.Name}()");
                }
            }
        }
    }

    static void FindMonoBehaviourLifecycleMethods(Assembly creatorDll)
    {
        Report("\nMonoBehaviour lifecycle methods in prop-related types:\n");

        var propTypes = creatorDll.GetTypes()
            .Where(t => t.Name.Contains("Prop") && t.BaseType?.Name == "MonoBehaviour")
            .ToList();

        // Also check inheritance chain for MonoBehaviour
        var monoBehaviourTypes = creatorDll.GetTypes()
            .Where(t => IsMonoBehaviour(t) && (t.Name.Contains("Prop") || t.Name.Contains("Tool")))
            .ToList();

        Report($"Found {monoBehaviourTypes.Count} MonoBehaviour types related to Prop/Tool\n");

        foreach (var type in monoBehaviourTypes)
        {
            var lifecycleMethods = new[] { "Awake", "Start", "OnEnable", "OnDisable", "OnDestroy", "Update", "LateUpdate" };
            var foundMethods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(m => lifecycleMethods.Contains(m.Name))
                .ToList();

            if (foundMethods.Any())
            {
                Report($"{type.Name}:");
                foreach (var method in foundMethods)
                {
                    Report($"  {method.Name}()");
                }
            }
        }
    }

    static bool IsMonoBehaviour(Type type)
    {
        var current = type;
        while (current != null)
        {
            if (current.Name == "MonoBehaviour") return true;
            current = current.BaseType;
        }
        return false;
    }

    static void AnalyzeTypeInDetail(Type type, string label)
    {
        Report($"\n=== {label} ===");
        Report($"Full Name: {type.FullName}");
        Report($"Base Type: {type.BaseType?.FullName ?? "None"}");
        Report($"Is Abstract: {type.IsAbstract}");
        Report($"Is Sealed: {type.IsSealed}");

        // Interfaces
        var interfaces = type.GetInterfaces();
        if (interfaces.Any())
        {
            Report($"\nInterfaces ({interfaces.Length}):");
            foreach (var iface in interfaces.Take(10))
            {
                Report($"  {iface.Name}");
            }
        }

        // Fields
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
        Report($"\nFields ({fields.Length}):");
        foreach (var field in fields)
        {
            var access = field.IsPublic ? "public" : field.IsPrivate ? "private" : "protected";
            var isStatic = field.IsStatic ? " static" : "";
            Report($"  {access}{isStatic} {field.FieldType.Name} {field.Name}");
        }

        // Properties
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
        Report($"\nProperties ({properties.Length}):");
        foreach (var prop in properties)
        {
            var getter = prop.GetGetMethod(true) != null ? "get;" : "";
            var setter = prop.GetSetMethod(true) != null ? "set;" : "";
            Report($"  {prop.PropertyType.Name} {prop.Name} {{ {getter} {setter} }}");
        }

        // Methods
        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(m => !m.IsSpecialName)
            .ToList();
        Report($"\nMethods ({methods.Count}):");
        foreach (var method in methods)
        {
            var access = method.IsPublic ? "public" : method.IsPrivate ? "private" : "protected";
            var isStatic = method.IsStatic ? " static" : "";
            var isVirtual = method.IsVirtual ? " virtual" : "";
            var isAsync = method.GetCustomAttributes(false).Any(a => a.GetType().Name.Contains("Async")) ? " async" : "";
            var parameters = string.Join(", ", method.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
            Report($"  {access}{isStatic}{isVirtual}{isAsync} {method.ReturnType.Name} {method.Name}({parameters})");
        }

        // Events
        var events = type.GetEvents(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
        if (events.Any())
        {
            Report($"\nEvents ({events.Length}):");
            foreach (var evt in events)
            {
                Report($"  {evt.EventHandlerType?.Name} {evt.Name}");
            }
        }

        // Nested types
        var nestedTypes = type.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic);
        if (nestedTypes.Any())
        {
            Report($"\nNested Types ({nestedTypes.Length}):");
            foreach (var nested in nestedTypes)
            {
                Report($"  {nested.Name}");
            }
        }
    }
}

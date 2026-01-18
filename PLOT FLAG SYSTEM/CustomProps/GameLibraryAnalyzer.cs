using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

/// <summary>
/// Standalone analyzer to dump Endstar game library structures
/// Run this to understand the actual architecture before coding
/// </summary>
public static class GameLibraryAnalyzer
{
    public static void AnalyzeGameLibraries(string outputPath)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Endstar Game Library Analysis");
        sb.AppendLine($"Generated: {DateTime.Now}");
        sb.AppendLine();

        string managedPath = @"C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed";

        // Analyze key DLLs
        string[] targetDlls = { "Props.dll", "Gameplay.dll", "Creator.dll", "Assets.dll", "Core.dll" };

        foreach (var dllName in targetDlls)
        {
            string dllPath = Path.Combine(managedPath, dllName);
            if (!File.Exists(dllPath))
            {
                sb.AppendLine($"## {dllName} - NOT FOUND");
                continue;
            }

            try
            {
                var asm = Assembly.LoadFrom(dllPath);
                sb.AppendLine($"## {dllName}");
                sb.AppendLine();

                // Find all types related to Props
                var propTypes = asm.GetTypes()
                    .Where(t => t.FullName != null &&
                           (t.FullName.Contains("Prop") || t.FullName.Contains("Library")))
                    .OrderBy(t => t.FullName)
                    .ToList();

                sb.AppendLine($"### Types containing 'Prop' or 'Library' ({propTypes.Count})");
                sb.AppendLine();

                foreach (var type in propTypes)
                {
                    AnalyzeType(type, sb);
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine($"## {dllName} - ERROR: {ex.Message}");
            }

            sb.AppendLine();
            sb.AppendLine("---");
            sb.AppendLine();
        }

        File.WriteAllText(outputPath, sb.ToString());
    }

    private static void AnalyzeType(Type type, StringBuilder sb)
    {
        sb.AppendLine($"#### `{type.FullName}`");

        // Base class
        if (type.BaseType != null && type.BaseType != typeof(object))
        {
            sb.AppendLine($"- **Base:** `{type.BaseType.FullName}`");
        }

        // Interfaces
        var interfaces = type.GetInterfaces();
        if (interfaces.Length > 0)
        {
            sb.AppendLine($"- **Implements:** {string.Join(", ", interfaces.Select(i => $"`{i.Name}`"))}");
        }

        // Is it a MonoBehaviour, ScriptableObject, etc?
        var baseChain = GetBaseTypeChain(type);
        if (baseChain.Contains("MonoBehaviour"))
            sb.AppendLine("- **Type:** MonoBehaviour (scene component)");
        else if (baseChain.Contains("ScriptableObject"))
            sb.AppendLine("- **Type:** ScriptableObject (asset)");
        else if (type.IsValueType)
            sb.AppendLine("- **Type:** Struct/ValueType");
        else if (type.IsInterface)
            sb.AppendLine("- **Type:** Interface");
        else if (type.IsAbstract)
            sb.AppendLine("- **Type:** Abstract Class");

        // Constructors
        var ctors = type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (ctors.Length > 0)
        {
            sb.AppendLine("- **Constructors:**");
            foreach (var ctor in ctors)
            {
                var pars = string.Join(", ", ctor.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
                sb.AppendLine($"  - `{type.Name}({pars})`");
            }
        }

        // Fields (non-inherited)
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        if (fields.Length > 0)
        {
            sb.AppendLine("- **Fields:**");
            foreach (var f in fields.Take(20)) // Limit to 20
            {
                string access = f.IsPublic ? "public" : f.IsPrivate ? "private" : "protected";
                sb.AppendLine($"  - `{access} {f.FieldType.Name} {f.Name}`");
            }
            if (fields.Length > 20)
                sb.AppendLine($"  - ... and {fields.Length - 20} more");
        }

        // Properties (non-inherited)
        var props = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        if (props.Length > 0)
        {
            sb.AppendLine("- **Properties:**");
            foreach (var p in props.Take(20))
            {
                string access = p.GetMethod?.IsPublic == true ? "public" : "private";
                sb.AppendLine($"  - `{access} {p.PropertyType.Name} {p.Name} {{ get; set; }}`");
            }
            if (props.Length > 20)
                sb.AppendLine($"  - ... and {props.Length - 20} more");
        }

        // Methods (non-inherited, non-property)
        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(m => !m.IsSpecialName) // Exclude property getters/setters
            .ToList();
        if (methods.Count > 0)
        {
            sb.AppendLine("- **Methods:**");
            foreach (var m in methods.Take(30))
            {
                string access = m.IsPublic ? "public" : m.IsPrivate ? "private" : "protected";
                var pars = string.Join(", ", m.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
                sb.AppendLine($"  - `{access} {m.ReturnType.Name} {m.Name}({pars})`");
            }
            if (methods.Count > 30)
                sb.AppendLine($"  - ... and {methods.Count - 30} more");
        }

        // Static members
        var staticFields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly);
        var staticProps = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly);
        if (staticFields.Length > 0 || staticProps.Length > 0)
        {
            sb.AppendLine("- **Static Members:**");
            foreach (var f in staticFields.Take(10))
                sb.AppendLine($"  - `static {f.FieldType.Name} {f.Name}`");
            foreach (var p in staticProps.Take(10))
                sb.AppendLine($"  - `static {p.PropertyType.Name} {p.Name}`");
        }

        sb.AppendLine();
    }

    private static string GetBaseTypeChain(Type type)
    {
        var chain = new StringBuilder();
        var current = type.BaseType;
        while (current != null)
        {
            chain.Append(current.Name);
            chain.Append(" -> ");
            current = current.BaseType;
        }
        return chain.ToString();
    }
}

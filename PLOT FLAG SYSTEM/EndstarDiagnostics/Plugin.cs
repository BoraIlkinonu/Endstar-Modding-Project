using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EndstarDiagnostics
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "com.endstar.diagnostics";
        public const string PluginName = "Endstar Diagnostics";
        public const string PluginVersion = "4.0.0";

        internal static ManualLogSource Log;
        internal static StreamWriter FileLog;
        internal static DateTime StartTime;
        internal static int CallCounter = 0;
        internal static bool Initialized = false;
        internal static HashSet<string> LoggedBaseTypes = new HashSet<string>();
        internal static HashSet<string> LoggedComponents = new HashSet<string>();
        internal static bool StageManagerDumped = false;

        private Harmony _harmony;

        void Awake()
        {
            if (Initialized) return;
            Initialized = true;

            Log = Logger;
            StartTime = DateTime.Now;

            var logPath = Path.Combine(
                Path.GetDirectoryName(Info.Location),
                $"diagnostics_{StartTime:yyyy-MM-dd_HH-mm-ss}.log"
            );

            try
            {
                FileLog = new StreamWriter(logPath, false, Encoding.UTF8) { AutoFlush = true };
            }
            catch (Exception ex)
            {
                Log.LogError($"Failed to create log file: {ex.Message}");
            }

            LogSection("INITIALIZATION");
            LogEvent("INIT", "Plugin", $"v{PluginVersion} - Full Prop Placement Pipeline");
            LogEvent("INIT", "LogFile", logPath);

            _harmony = new Harmony(PluginGuid);
            ApplyPatches();

            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;

            LogEvent("INIT", "Complete", "All patches applied");
        }

        void ApplyPatches()
        {
            try
            {
                _harmony.PatchAll(typeof(Plugin).Assembly);
                LogEvent("PATCH", "Attributes", "OK");
            }
            catch (Exception ex)
            {
                LogEvent("PATCH", "Attributes", $"Error: {ex.Message}");
            }

            PatchStageManager();
            PatchPropLibrary();
            PatchEndlessProp();
            PatchBaseTypeList();
            PatchComponentList();

            // NEW: Prop placement pipeline
            PatchToolManager();
            PatchPropTool();
            PatchStage();
            PatchCreatorManager();
        }

        void PatchStageManager()
        {
            var type = AccessTools.TypeByName("Endless.Gameplay.LevelEditing.Level.StageManager");
            if (type == null) { LogEvent("PATCH", "StageManager", "Type not found"); return; }

            LogEvent("PATCH", "StageManager", $"Found type: {type.FullName}");
            DumpTypeInfo(type, "StageManager");

            PatchMethod(type, "Awake", typeof(Hooks), nameof(Hooks.StageManager_Awake_Postfix), false);

            var injectProp = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(m => m.Name == "InjectProp" && m.GetParameters().Length == 4);
            if (injectProp != null)
            {
                _harmony.Patch(injectProp, prefix: new HarmonyMethod(typeof(Hooks), nameof(Hooks.StageManager_InjectProp_Prefix)));
                LogEvent("PATCH", "StageManager.InjectProp", "OK");
            }

            PatchMethod(type, "LoadLibraryPrefabs", typeof(Hooks), nameof(Hooks.StageManager_LoadLibraryPrefabs_Prefix), true);
        }

        void PatchPropLibrary()
        {
            var type = AccessTools.TypeByName("Endless.Gameplay.LevelEditing.PropLibrary");
            if (type == null) { LogEvent("PATCH", "PropLibrary", "Type not found"); return; }

            LogEvent("PATCH", "PropLibrary", $"Found type: {type.FullName}");
            DumpTypeInfo(type, "PropLibrary");

            var injectProp = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(m => m.Name == "InjectProp" && m.GetParameters().Length == 6);
            if (injectProp != null)
            {
                _harmony.Patch(injectProp, prefix: new HarmonyMethod(typeof(Hooks), nameof(Hooks.PropLibrary_InjectProp_Prefix)));
                LogEvent("PATCH", "PropLibrary.InjectProp", "OK");
            }
        }

        void PatchEndlessProp()
        {
            var type = AccessTools.TypeByName("Endless.Gameplay.Scripting.EndlessProp");
            if (type == null) { LogEvent("PATCH", "EndlessProp", "Type not found"); return; }

            LogEvent("PATCH", "EndlessProp", $"Found type: {type.FullName}");
            DumpTypeInfo(type, "EndlessProp");

            var buildPrefab = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(m => m.Name == "BuildPrefab" && m.GetParameters().Length == 4);
            if (buildPrefab != null)
            {
                _harmony.Patch(buildPrefab,
                    prefix: new HarmonyMethod(typeof(Hooks), nameof(Hooks.EndlessProp_BuildPrefab_Prefix)),
                    postfix: new HarmonyMethod(typeof(Hooks), nameof(Hooks.EndlessProp_BuildPrefab_Postfix)));
                LogEvent("PATCH", "EndlessProp.BuildPrefab", "OK");
            }
        }

        void PatchBaseTypeList()
        {
            var type = AccessTools.TypeByName("Endless.Gameplay.BaseTypeList");
            if (type == null) { LogEvent("PATCH", "BaseTypeList", "Type not found"); return; }

            LogEvent("PATCH", "BaseTypeList", $"Found type: {type.FullName}");
            DumpTypeInfo(type, "BaseTypeList");

            var tryGetDef = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(m => m.Name == "TryGetDefinition" &&
                                     m.GetParameters().Length == 2 &&
                                     m.GetParameters()[1].ParameterType.IsByRef);
            if (tryGetDef != null)
            {
                _harmony.Patch(tryGetDef,
                    postfix: new HarmonyMethod(typeof(Hooks), nameof(Hooks.BaseTypeList_TryGetDefinition_Postfix)));
                LogEvent("PATCH", "BaseTypeList.TryGetDefinition", "OK");
            }
        }

        void PatchComponentList()
        {
            var type = AccessTools.TypeByName("Endless.Gameplay.ComponentList");
            if (type == null) { LogEvent("PATCH", "ComponentList", "Type not found"); return; }

            LogEvent("PATCH", "ComponentList", $"Found type: {type.FullName}");

            var tryGetDef = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(m => m.Name == "TryGetDefinition" &&
                                     m.GetParameters().Length == 2 &&
                                     m.GetParameters()[1].ParameterType.IsByRef);
            if (tryGetDef != null)
            {
                _harmony.Patch(tryGetDef,
                    postfix: new HarmonyMethod(typeof(Hooks), nameof(Hooks.ComponentList_TryGetDefinition_Postfix)));
                LogEvent("PATCH", "ComponentList.TryGetDefinition", "OK");
            }
        }

        void PatchToolManager()
        {
            var type = AccessTools.TypeByName("Endless.Creator.LevelEditing.Runtime.ToolManager");
            if (type == null) { LogEvent("PATCH", "ToolManager", "Type not found (Creator.dll not loaded?)"); return; }

            LogEvent("PATCH", "ToolManager", $"Found type: {type.FullName}");

            // Patch SetActiveTool(EndlessTool)
            var setActiveTool = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(m => m.Name == "SetActiveTool" && m.GetParameters().Length == 1 &&
                                     m.GetParameters()[0].ParameterType.Name == "EndlessTool");
            if (setActiveTool != null)
            {
                _harmony.Patch(setActiveTool,
                    postfix: new HarmonyMethod(typeof(Hooks), nameof(Hooks.ToolManager_SetActiveTool_Postfix)));
                LogEvent("PATCH", "ToolManager.SetActiveTool", "OK");
            }

            // Patch Activate
            var activate = AccessTools.Method(type, "Activate");
            if (activate != null)
            {
                _harmony.Patch(activate,
                    postfix: new HarmonyMethod(typeof(Hooks), nameof(Hooks.ToolManager_Activate_Postfix)));
                LogEvent("PATCH", "ToolManager.Activate", "OK");
            }
        }

        void PatchPropTool()
        {
            var type = AccessTools.TypeByName("Endless.Creator.LevelEditing.Runtime.PropTool");
            if (type == null) { LogEvent("PATCH", "PropTool", "Type not found"); return; }

            LogEvent("PATCH", "PropTool", $"Found type: {type.FullName}");
            DumpTypeInfo(type, "PropTool");

            // Patch UpdateSelectedAssetId
            var updateSelected = AccessTools.Method(type, "UpdateSelectedAssetId");
            if (updateSelected != null)
            {
                _harmony.Patch(updateSelected,
                    prefix: new HarmonyMethod(typeof(Hooks), nameof(Hooks.PropTool_UpdateSelectedAssetId_Prefix)));
                LogEvent("PATCH", "PropTool.UpdateSelectedAssetId", "OK");
            }

            // Patch ToolPressed
            var toolPressed = AccessTools.Method(type, "ToolPressed");
            if (toolPressed != null)
            {
                _harmony.Patch(toolPressed,
                    prefix: new HarmonyMethod(typeof(Hooks), nameof(Hooks.PropTool_ToolPressed_Prefix)));
                LogEvent("PATCH", "PropTool.ToolPressed", "OK");
            }

            // Patch ToolReleased
            var toolReleased = AccessTools.Method(type, "ToolReleased");
            if (toolReleased != null)
            {
                _harmony.Patch(toolReleased,
                    prefix: new HarmonyMethod(typeof(Hooks), nameof(Hooks.PropTool_ToolReleased_Prefix)));
                LogEvent("PATCH", "PropTool.ToolReleased", "OK");
            }

            // Patch AttemptPlaceProp (private method)
            var attemptPlace = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .FirstOrDefault(m => m.Name == "AttemptPlaceProp");
            if (attemptPlace != null)
            {
                _harmony.Patch(attemptPlace,
                    prefix: new HarmonyMethod(typeof(Hooks), nameof(Hooks.PropTool_AttemptPlaceProp_Prefix)));
                LogEvent("PATCH", "PropTool.AttemptPlaceProp", "OK");
            }

            // Patch PlaceProp (private method)
            var placeProp = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .FirstOrDefault(m => m.Name == "PlaceProp" && m.GetParameters().Length == 5);
            if (placeProp != null)
            {
                _harmony.Patch(placeProp,
                    prefix: new HarmonyMethod(typeof(Hooks), nameof(Hooks.PropTool_PlaceProp_Prefix)));
                LogEvent("PATCH", "PropTool.PlaceProp", "OK");
            }

            // Patch HandleSelected
            var handleSelected = AccessTools.Method(type, "HandleSelected");
            if (handleSelected != null)
            {
                _harmony.Patch(handleSelected,
                    postfix: new HarmonyMethod(typeof(Hooks), nameof(Hooks.PropTool_HandleSelected_Postfix)));
                LogEvent("PATCH", "PropTool.HandleSelected", "OK");
            }
        }

        void PatchStage()
        {
            var type = AccessTools.TypeByName("Endless.Gameplay.LevelEditing.Level.Stage");
            if (type == null) { LogEvent("PATCH", "Stage", "Type not found"); return; }

            LogEvent("PATCH", "Stage", $"Found type: {type.FullName}");

            // Patch TrackNonNetworkedObject
            var trackNonNet = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(m => m.Name == "TrackNonNetworkedObject");
            if (trackNonNet != null)
            {
                _harmony.Patch(trackNonNet,
                    prefix: new HarmonyMethod(typeof(Hooks), nameof(Hooks.Stage_TrackNonNetworkedObject_Prefix)));
                LogEvent("PATCH", "Stage.TrackNonNetworkedObject", "OK");
            }

            // Patch PlacementIsValid
            var placementValid = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(m => m.Name == "PlacementIsValid");
            if (placementValid != null)
            {
                _harmony.Patch(placementValid,
                    postfix: new HarmonyMethod(typeof(Hooks), nameof(Hooks.Stage_PlacementIsValid_Postfix)));
                LogEvent("PATCH", "Stage.PlacementIsValid", "OK");
            }
        }

        void PatchCreatorManager()
        {
            var type = AccessTools.TypeByName("Endless.Creator.CreatorManager");
            if (type == null)
            {
                type = AccessTools.TypeByName("Endless.Creator.LevelEditing.Runtime.CreatorManager");
            }
            if (type == null) { LogEvent("PATCH", "CreatorManager", "Type not found"); return; }

            LogEvent("PATCH", "CreatorManager", $"Found type: {type.FullName}");

            // Try to find play mode methods
            var allMethods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var m in allMethods.Where(m => m.Name.Contains("Play") || m.Name.Contains("Creator") || m.Name.Contains("Edit")))
            {
                LogEvent("PATCH", "CreatorManager.Method", $"{m.Name}({string.Join(",", m.GetParameters().Select(p => p.Name))})");
            }
        }

        void PatchMethod(Type type, string methodName, Type hookType, string hookMethodName, bool isPrefix)
        {
            try
            {
                var method = AccessTools.Method(type, methodName);
                if (method == null) { LogEvent("PATCH", $"{type.Name}.{methodName}", "Method not found"); return; }

                var hook = hookType.GetMethod(hookMethodName, BindingFlags.Public | BindingFlags.Static);
                if (hook == null) { LogEvent("PATCH", $"{type.Name}.{methodName}", "Hook not found"); return; }

                if (isPrefix)
                    _harmony.Patch(method, prefix: new HarmonyMethod(hook));
                else
                    _harmony.Patch(method, postfix: new HarmonyMethod(hook));
                LogEvent("PATCH", $"{type.Name}.{methodName}", "OK");
            }
            catch (Exception ex)
            {
                LogEvent("PATCH", $"{type.Name}.{methodName}", $"Error: {ex.Message}");
            }
        }

        static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            LogSection($"SCENE LOADED: {scene.name}");
            LogEvent("SCENE", "Loaded", $"name={scene.name} buildIndex={scene.buildIndex} mode={mode}");
        }

        static void OnSceneUnloaded(Scene scene)
        {
            LogEvent("SCENE", "Unloaded", $"name={scene.name}");
        }

        void OnApplicationQuit()
        {
            LogSection("APPLICATION QUIT");
            FileLog?.Flush();
            FileLog?.Close();
        }

        // =====================================================================
        // LOGGING UTILITIES
        // =====================================================================

        internal static void LogSection(string title)
        {
            var separator = new string('=', 80);
            FileLog?.WriteLine($"\n{separator}\n{title}\n{separator}");
        }

        internal static void LogEvent(string category, string context, string message)
        {
            if (Log == null) return;
            try
            {
                CallCounter++;
                var elapsed = (DateTime.Now - StartTime).TotalMilliseconds;
                var line = $"[{CallCounter:D5}][{elapsed:F0}ms][{category}] {context}: {message}";
                Log.LogInfo(line);
                FileLog?.WriteLine(line);
            }
            catch { }
        }

        internal static void LogData(string label, string data)
        {
            FileLog?.WriteLine($"  {label}: {data}");
        }

        internal static void LogObject(string prefix, object obj, int depth = 0)
        {
            if (obj == null) { LogData(prefix, "null"); return; }
            if (depth > 3) { LogData(prefix, "[max depth]"); return; }

            var type = obj.GetType();
            var indent = new string(' ', depth * 2);

            FileLog?.WriteLine($"{indent}{prefix} ({type.Name}):");

            foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                try
                {
                    if (prop.GetIndexParameters().Length > 0) continue;
                    var value = prop.GetValue(obj);
                    var valueStr = FormatValue(value);
                    FileLog?.WriteLine($"{indent}  .{prop.Name} = {valueStr}");
                }
                catch (Exception ex)
                {
                    FileLog?.WriteLine($"{indent}  .{prop.Name} = [Error: {ex.Message}]");
                }
            }

            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                try
                {
                    var value = field.GetValue(obj);
                    var valueStr = FormatValue(value);
                    FileLog?.WriteLine($"{indent}  .{field.Name} = {valueStr}");
                }
                catch (Exception ex)
                {
                    FileLog?.WriteLine($"{indent}  .{field.Name} = [Error: {ex.Message}]");
                }
            }
        }

        internal static string FormatValue(object value)
        {
            if (value == null) return "null";

            var type = value.GetType();

            if (type.IsPrimitive || type == typeof(string) || type == typeof(decimal))
                return value.ToString();

            if (type.IsEnum)
                return $"{type.Name}.{value}";

            if (value is GameObject go)
                return $"GameObject('{go.name}')";

            if (value is UnityEngine.Object uobj)
                return $"{type.Name}('{uobj.name}')";

            if (value is IList list)
                return $"List<{(type.IsGenericType ? type.GetGenericArguments()[0].Name : "?")}>[{list.Count}]";

            if (value is IDictionary dict)
                return $"Dictionary[{dict.Count}]";

            if (type.IsGenericType && type.GetGenericTypeDefinition().Name.Contains("Guid"))
                return value.ToString();

            return $"{type.Name}";
        }

        internal static void DumpTypeInfo(Type type, string label)
        {
            FileLog?.WriteLine($"\n--- TYPE: {label} ---");
            FileLog?.WriteLine($"FullName: {type.FullName}");
            FileLog?.WriteLine($"BaseType: {type.BaseType?.FullName ?? "none"}");
            FileLog?.WriteLine($"Interfaces: {string.Join(", ", type.GetInterfaces().Select(i => i.Name))}");

            FileLog?.WriteLine("Fields:");
            foreach (var f in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                FileLog?.WriteLine($"  {(f.IsPublic ? "public" : "private")} {f.FieldType.Name} {f.Name}");
            }

            FileLog?.WriteLine("Properties:");
            foreach (var p in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                FileLog?.WriteLine($"  {p.PropertyType.Name} {p.Name} {{ {(p.CanRead ? "get;" : "")} {(p.CanWrite ? "set;" : "")} }}");
            }

            FileLog?.WriteLine("Methods:");
            foreach (var m in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                var pars = string.Join(", ", m.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
                FileLog?.WriteLine($"  {m.ReturnType.Name} {m.Name}({pars})");
            }
            FileLog?.WriteLine("--- END TYPE ---\n");
        }

        // =====================================================================
        // DATA EXTRACTION
        // =====================================================================

        internal static void DumpProp(object prop, string context)
        {
            if (prop == null) { LogEvent("PROP", context, "null"); return; }

            var t = prop.GetType();
            FileLog?.WriteLine($"\n--- PROP DATA ({context}) ---");

            var name = GetPropertyValue(prop, "Name");
            var assetId = GetPropertyValue(prop, "AssetID");
            var baseTypeId = GetPropertyValue(prop, "BaseTypeId");

            FileLog?.WriteLine($"Name: {name}");
            FileLog?.WriteLine($"AssetID: {assetId}");
            FileLog?.WriteLine($"BaseTypeId: {baseTypeId}");

            // Get ComponentIds
            var componentIds = GetPropertyValue(prop, "ComponentIds");
            if (componentIds is IList list)
            {
                FileLog?.WriteLine($"ComponentIds: [{list.Count}]");
                for (int i = 0; i < list.Count && i < 20; i++)
                {
                    FileLog?.WriteLine($"  [{i}] {list[i]}");
                }
            }

            // Get Script
            var script = GetPropertyValue(prop, "Script");
            if (script != null)
            {
                FileLog?.WriteLine($"Script: {script.GetType().Name}");
                var scriptName = GetPropertyValue(script, "Name");
                var scriptId = GetPropertyValue(script, "AssetID");
                FileLog?.WriteLine($"  Script.Name: {scriptName}");
                FileLog?.WriteLine($"  Script.AssetID: {scriptId}");
            }

            // Dump all other properties
            FileLog?.WriteLine("All Properties:");
            foreach (var p in t.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                try
                {
                    if (p.GetIndexParameters().Length > 0) continue;
                    var val = p.GetValue(prop);
                    FileLog?.WriteLine($"  {p.Name}: {FormatValue(val)}");
                }
                catch { }
            }
            FileLog?.WriteLine("--- END PROP ---\n");

            LogEvent("PROP", context, $"Name={name} AssetID={assetId} BaseTypeId={baseTypeId}");
        }

        internal static void DumpBaseTypeDefinition(object def, string id)
        {
            if (def == null) return;
            if (LoggedBaseTypes.Contains(id)) return;
            LoggedBaseTypes.Add(id);

            var t = def.GetType();
            FileLog?.WriteLine($"\n--- BASETYPE DEFINITION: {id} ---");
            FileLog?.WriteLine($"Type: {t.FullName}");

            var componentId = GetPropertyValue(def, "ComponentId");
            var prefab = GetPropertyValue(def, "Prefab");
            var isNetworked = GetPropertyValue(def, "IsNetworked");
            var isUserExposed = GetPropertyValue(def, "IsUserExposed");
            var isSpawnPoint = GetPropertyValue(def, "IsSpawnPoint");

            FileLog?.WriteLine($"ComponentId: {componentId}");
            FileLog?.WriteLine($"Prefab: {FormatValue(prefab)}");
            FileLog?.WriteLine($"IsNetworked: {isNetworked}");
            FileLog?.WriteLine($"IsUserExposed: {isUserExposed}");
            FileLog?.WriteLine($"IsSpawnPoint: {isSpawnPoint}");

            // Dump prefab hierarchy
            if (prefab is GameObject go)
            {
                DumpGameObjectHierarchy(go, "Prefab");
            }

            // Dump inspectable members
            var members = GetPropertyValue(def, "InspectableMembers");
            if (members is IList memList)
            {
                FileLog?.WriteLine($"InspectableMembers: [{memList.Count}]");
                foreach (var mem in memList)
                {
                    var memName = GetPropertyValue(mem, "Name");
                    var memType = GetPropertyValue(mem, "Type");
                    FileLog?.WriteLine($"  - {memName} ({memType})");
                }
            }

            // Dump events
            var events = GetPropertyValue(def, "AvailableEvents");
            if (events is IList evList)
            {
                FileLog?.WriteLine($"AvailableEvents: [{evList.Count}]");
                foreach (var ev in evList)
                {
                    var evName = GetPropertyValue(ev, "Name");
                    FileLog?.WriteLine($"  - {evName}");
                }
            }

            // Dump receivers
            var receivers = GetPropertyValue(def, "AvailableReceivers");
            if (receivers is IList recList)
            {
                FileLog?.WriteLine($"AvailableReceivers: [{recList.Count}]");
                foreach (var rec in recList)
                {
                    var recName = GetPropertyValue(rec, "Name");
                    FileLog?.WriteLine($"  - {recName}");
                }
            }

            FileLog?.WriteLine("--- END BASETYPE ---\n");
        }

        internal static void DumpComponentDefinition(object def, string id)
        {
            if (def == null) return;
            if (LoggedComponents.Contains(id)) return;
            LoggedComponents.Add(id);

            var t = def.GetType();
            FileLog?.WriteLine($"\n--- COMPONENT DEFINITION: {id} ---");
            FileLog?.WriteLine($"Type: {t.FullName}");

            var componentId = GetPropertyValue(def, "ComponentId");
            var prefab = GetPropertyValue(def, "Prefab");
            var isNetworked = GetPropertyValue(def, "IsNetworked");

            FileLog?.WriteLine($"ComponentId: {componentId}");
            FileLog?.WriteLine($"Prefab: {FormatValue(prefab)}");
            FileLog?.WriteLine($"IsNetworked: {isNetworked}");

            if (prefab is GameObject go)
            {
                DumpGameObjectHierarchy(go, "Prefab");
            }

            FileLog?.WriteLine("--- END COMPONENT ---\n");
        }

        internal static void DumpGameObjectHierarchy(GameObject go, string label, int depth = 0)
        {
            if (go == null || depth > 5) return;
            var indent = new string(' ', depth * 2);

            FileLog?.WriteLine($"{indent}{label}: GameObject('{go.name}') active={go.activeSelf}");

            // List components
            var components = go.GetComponents<Component>();
            foreach (var comp in components)
            {
                if (comp == null) continue;
                var compType = comp.GetType();
                FileLog?.WriteLine($"{indent}  [Component] {compType.Name}");

                // Check for IBaseType
                if (compType.GetInterfaces().Any(i => i.Name == "IBaseType"))
                {
                    FileLog?.WriteLine($"{indent}    ** Implements IBaseType **");
                    DumpIBaseType(comp, depth + 2);
                }
            }

            // Recurse children
            for (int i = 0; i < go.transform.childCount && i < 10; i++)
            {
                var child = go.transform.GetChild(i).gameObject;
                DumpGameObjectHierarchy(child, $"Child[{i}]", depth + 1);
            }
        }

        internal static void DumpIBaseType(Component comp, int depth)
        {
            var indent = new string(' ', depth * 2);
            var t = comp.GetType();

            var filter = GetPropertyValue(comp, "Filter");
            var navValue = GetPropertyValue(comp, "NavValue");
            var componentRefType = GetPropertyValue(comp, "ComponentReferenceType");

            FileLog?.WriteLine($"{indent}Filter: {filter}");
            FileLog?.WriteLine($"{indent}NavValue: {navValue}");
            FileLog?.WriteLine($"{indent}ComponentReferenceType: {componentRefType}");
        }

        internal static void DumpStageManager(object instance)
        {
            if (instance == null || StageManagerDumped) return;
            StageManagerDumped = true;

            FileLog?.WriteLine("\n" + new string('=', 80));
            FileLog?.WriteLine("STAGEMANAGER FULL STATE DUMP");
            FileLog?.WriteLine(new string('=', 80));

            var t = instance.GetType();

            // BaseTypeList
            var baseTypeList = GetFieldOrPropertyValue(instance, "baseTypeList");
            if (baseTypeList != null)
            {
                FileLog?.WriteLine("\n--- BaseTypeList ---");
                var allDefs = GetPropertyValue(baseTypeList, "AllDefinitions");
                if (allDefs is IList defList)
                {
                    FileLog?.WriteLine($"Count: {defList.Count}");
                    foreach (var def in defList)
                    {
                        var id = GetPropertyValue(def, "ComponentId")?.ToString() ?? "?";
                        var prefab = GetPropertyValue(def, "Prefab");
                        var isNet = GetPropertyValue(def, "IsNetworked");
                        var isUser = GetPropertyValue(def, "IsUserExposed");
                        FileLog?.WriteLine($"  {id}: Prefab={FormatValue(prefab)} IsNetworked={isNet} IsUserExposed={isUser}");
                    }
                }
            }

            // ComponentList
            var componentList = GetFieldOrPropertyValue(instance, "componentList");
            if (componentList != null)
            {
                FileLog?.WriteLine("\n--- ComponentList ---");
                var allDefs = GetPropertyValue(componentList, "AllDefinitions");
                if (allDefs is IList defList)
                {
                    FileLog?.WriteLine($"Count: {defList.Count}");
                    foreach (var def in defList)
                    {
                        var id = GetPropertyValue(def, "ComponentId")?.ToString() ?? "?";
                        var prefab = GetPropertyValue(def, "Prefab");
                        FileLog?.WriteLine($"  {id}: Prefab={FormatValue(prefab)}");
                    }
                }
            }

            // InjectedProps
            var injectedProps = GetFieldOrPropertyValue(instance, "injectedProps");
            if (injectedProps is IList injList)
            {
                FileLog?.WriteLine($"\n--- InjectedProps: [{injList.Count}] ---");
                foreach (var inj in injList)
                {
                    var prop = GetFieldOrPropertyValue(inj, "Prop");
                    var propName = GetPropertyValue(prop, "Name");
                    FileLog?.WriteLine($"  {propName}");
                }
            }

            // ActivePropLibrary
            var activePropLib = GetFieldOrPropertyValue(instance, "activePropLibrary");
            if (activePropLib != null)
            {
                FileLog?.WriteLine("\n--- ActivePropLibrary ---");
                DumpPropLibrary(activePropLib);
            }

            FileLog?.WriteLine("\n" + new string('=', 80));
            FileLog?.WriteLine("END STAGEMANAGER DUMP");
            FileLog?.WriteLine(new string('=', 80) + "\n");
        }

        internal static void DumpPropLibrary(object lib)
        {
            if (lib == null) return;

            var loadedPropMap = GetFieldOrPropertyValue(lib, "loadedPropMap");
            if (loadedPropMap is IDictionary dict)
            {
                FileLog?.WriteLine($"loadedPropMap: [{dict.Count}] entries");
                int i = 0;
                foreach (DictionaryEntry entry in dict)
                {
                    if (i++ >= 50) { FileLog?.WriteLine("  ... (truncated)"); break; }
                    var info = entry.Value;
                    var propData = GetFieldOrPropertyValue(info, "PropData");
                    var propName = GetPropertyValue(propData, "Name");
                    var endlessProp = GetFieldOrPropertyValue(info, "EndlessProp");
                    FileLog?.WriteLine($"  [{entry.Key}] -> {propName} (EndlessProp={FormatValue(endlessProp)})");
                }
            }

            var injectedIds = GetFieldOrPropertyValue(lib, "injectedPropIds");
            if (injectedIds is IList idList)
            {
                FileLog?.WriteLine($"injectedPropIds: [{idList.Count}]");
                foreach (var id in idList)
                {
                    FileLog?.WriteLine($"  {id}");
                }
            }
        }

        internal static void DumpEndlessProp(object endlessProp, string context)
        {
            if (endlessProp == null) return;

            FileLog?.WriteLine($"\n--- ENDLESSPROP ({context}) ---");

            var t = endlessProp.GetType();
            foreach (var p in t.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                try
                {
                    if (p.GetIndexParameters().Length > 0) continue;
                    var val = p.GetValue(endlessProp);
                    FileLog?.WriteLine($"  {p.Name}: {FormatValue(val)}");
                }
                catch { }
            }

            // Get the GameObject
            if (endlessProp is Component comp)
            {
                DumpGameObjectHierarchy(comp.gameObject, "GameObject", 1);
            }

            FileLog?.WriteLine("--- END ENDLESSPROP ---\n");
        }

        internal static object GetPropertyValue(object obj, string name)
        {
            if (obj == null) return null;
            try
            {
                var prop = obj.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                return prop?.GetValue(obj);
            }
            catch { return null; }
        }

        internal static object GetFieldOrPropertyValue(object obj, string name)
        {
            if (obj == null) return null;
            try
            {
                var t = obj.GetType();
                var prop = t.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (prop != null) return prop.GetValue(obj);

                var field = t.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                return field?.GetValue(obj);
            }
            catch { return null; }
        }
    }

    public static class Hooks
    {
        public static void StageManager_Awake_Postfix(object __instance)
        {
            Plugin.LogSection("STAGEMANAGER AWAKE");
            Plugin.LogEvent("STAGE", "Awake", "StageManager initialized - dumping full state");
            Plugin.DumpStageManager(__instance);
        }

        public static void StageManager_InjectProp_Prefix(object __instance, object __0, object __1, object __2, object __3)
        {
            Plugin.LogSection("STAGEMANAGER.INJECTPROP CALLED");
            Plugin.DumpProp(__0, "InjectProp.Prop");
            Plugin.LogData("TestPrefab", Plugin.FormatValue(__1));
            Plugin.LogData("TestScript", Plugin.FormatValue(__2));
            Plugin.LogData("Icon", Plugin.FormatValue(__3));
        }

        public static void StageManager_LoadLibraryPrefabs_Prefix(object __instance)
        {
            Plugin.LogSection("STAGEMANAGER.LOADLIBRARYPREFABS");
            Plugin.LogEvent("STAGE", "LoadLibraryPrefabs", "Starting - dumping current state");

            // Dump state before loading
            var injectedProps = Plugin.GetFieldOrPropertyValue(__instance, "injectedProps");
            if (injectedProps is IList list)
            {
                Plugin.LogEvent("STAGE", "InjectedProps", $"Count before load: {list.Count}");
            }
        }

        public static void PropLibrary_InjectProp_Prefix(object __0, object __1, object __2, object __3, object __4, object __5)
        {
            Plugin.LogSection("PROPLIBRARY.INJECTPROP CALLED");
            Plugin.DumpProp(__0, "PropLibrary.InjectProp.Prop");
            Plugin.LogData("TestPrefab", Plugin.FormatValue(__1));
            Plugin.LogData("TestScript", Plugin.FormatValue(__2));
            Plugin.LogData("Icon", Plugin.FormatValue(__3));
            Plugin.LogData("PrefabSpawnTransform", Plugin.FormatValue(__4));
            Plugin.LogData("BasePropPrefab", Plugin.FormatValue(__5));
        }

        public static void EndlessProp_BuildPrefab_Prefix(object __instance, object __0, object __1, object __2)
        {
            Plugin.DumpProp(__0, "BuildPrefab.Input");
        }

        public static void EndlessProp_BuildPrefab_Postfix(object __instance, object __0)
        {
            Plugin.DumpEndlessProp(__instance, "BuildPrefab.Output");
        }

        public static void BaseTypeList_TryGetDefinition_Postfix(object __0, bool __result, object __1)
        {
            var id = __0?.ToString() ?? "null";
            if (__result && __1 != null)
            {
                Plugin.DumpBaseTypeDefinition(__1, id);
            }
            else
            {
                Plugin.LogEvent("BASETYPE", "NotFound", $"id={id}");
            }
        }

        public static void ComponentList_TryGetDefinition_Postfix(object __0, bool __result, object __1)
        {
            var id = __0?.ToString() ?? "null";
            if (__result && __1 != null)
            {
                Plugin.DumpComponentDefinition(__1, id);
            }
            else
            {
                Plugin.LogEvent("COMPONENT", "NotFound", $"id={id}");
            }
        }

        // =====================================================================
        // NEW: TOOL SELECTION HOOKS
        // =====================================================================

        public static void ToolManager_SetActiveTool_Postfix(object __0)
        {
            if (__0 == null) return;
            var toolType = Plugin.GetPropertyValue(__0, "ToolType");
            var toolName = Plugin.GetPropertyValue(__0, "ToolTypeName") ?? __0.GetType().Name;
            Plugin.LogSection($"TOOL SELECTED: {toolName}");
            Plugin.LogEvent("TOOL", "SetActiveTool", $"Type={toolType} Name={toolName}");
        }

        public static void ToolManager_Activate_Postfix()
        {
            Plugin.LogEvent("TOOL", "ToolManager.Activate", "Creator mode activated - tools enabled");
        }

        // =====================================================================
        // NEW: PROP TOOL HOOKS (Selection & Placement)
        // =====================================================================

        public static void PropTool_HandleSelected_Postfix(object __instance)
        {
            Plugin.LogEvent("PROPTOOL", "HandleSelected", "PropTool is now active");
            var selectedId = Plugin.GetPropertyValue(__instance, "SelectedAssetId");
            Plugin.LogEvent("PROPTOOL", "CurrentSelection", $"AssetId={selectedId}");
        }

        public static void PropTool_UpdateSelectedAssetId_Prefix(object __0)
        {
            var selectedAssetId = __0?.ToString() ?? "null";
            Plugin.LogSection($"PROP SELECTED IN UI: {selectedAssetId}");
            Plugin.LogEvent("PROPTOOL", "UpdateSelectedAssetId", $"AssetId={selectedAssetId}");

            // Try to get prop info
            var stageManager = AccessTools.TypeByName("Endless.Gameplay.LevelEditing.Level.StageManager");
            if (stageManager != null)
            {
                var instanceProp = stageManager.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
                var instance = instanceProp?.GetValue(null);
                if (instance != null)
                {
                    var propLib = Plugin.GetPropertyValue(instance, "ActivePropLibrary");
                    if (propLib != null)
                    {
                        // Try to get RuntimePropInfo
                        var tryGet = propLib.GetType().GetMethod("TryGetRuntimePropInfo",
                            new[] { __0.GetType(), typeof(object).MakeByRefType() });
                        // Just log for now
                        Plugin.LogEvent("PROPTOOL", "PropLibraryLookup", "Attempting to get prop info...");
                    }
                }
            }
        }

        public static void PropTool_ToolPressed_Prefix(object __instance)
        {
            Plugin.LogEvent("PROPTOOL", "ToolPressed", "User started click/placement");
            var ghost = Plugin.GetFieldOrPropertyValue(__instance, "PropGhostTransform");
            if (ghost != null)
            {
                Plugin.LogEvent("PROPTOOL", "GhostPosition", $"Ghost exists: {Plugin.FormatValue(ghost)}");
            }
        }

        public static void PropTool_ToolReleased_Prefix(object __instance)
        {
            Plugin.LogEvent("PROPTOOL", "ToolReleased", "User released click - checking for placement");
            var ghost = Plugin.GetFieldOrPropertyValue(__instance, "PropGhostTransform");
            var activeId = Plugin.GetPropertyValue(__instance, "ActiveAssetId");
            Plugin.LogEvent("PROPTOOL", "PlacementAttempt", $"AssetId={activeId} HasGhost={ghost != null}");
        }

        public static void PropTool_AttemptPlaceProp_Prefix(object __0, object __1, object __2)
        {
            Plugin.LogSection("PROP PLACEMENT - SERVER SIDE");
            Plugin.LogEvent("PROPTOOL", "AttemptPlaceProp", $"Position={__0} Rotation={__1} AssetId={__2}");
        }

        public static void PropTool_PlaceProp_Prefix(object __0, object __1, object __2, object __3, object __4)
        {
            Plugin.LogSection("PROP PLACEMENT - CLIENT SIDE");
            Plugin.LogEvent("PROPTOOL", "PlaceProp", $"Position={__0} Rotation={__1} AssetId={__2} InstanceId={__3} NetworkObjectId={__4}");
        }

        // =====================================================================
        // NEW: STAGE TRACKING HOOKS
        // =====================================================================

        public static void Stage_TrackNonNetworkedObject_Prefix(object __0, object __1, object __2, bool __3)
        {
            Plugin.LogSection("STAGE TRACKING OBJECT");
            Plugin.LogEvent("STAGE", "TrackNonNetworkedObject", $"AssetId={__0} InstanceId={__1} IsFromEditor={__3}");
            if (__2 is GameObject go)
            {
                Plugin.LogEvent("STAGE", "TrackedGameObject", $"Name={go.name}");
                Plugin.DumpGameObjectHierarchy(go, "TrackedObject", 1);
            }
        }

        public static void Stage_PlacementIsValid_Postfix(object __0, object __1, object __2, bool __result)
        {
            Plugin.LogEvent("STAGE", "PlacementIsValid", $"Prop={Plugin.FormatValue(__0)} Position={__1} Rotation={__2} Result={__result}");
        }
    }

    [HarmonyPatch(typeof(UnityEngine.Object), nameof(UnityEngine.Object.DontDestroyOnLoad))]
    public static class DontDestroyOnLoad_Patch
    {
        public static void Prefix(UnityEngine.Object target)
        {
            if (target?.name == null) return;
            var name = target.name;
            if (name.Contains("Stage") || name.Contains("Manager") || name.Contains("Network") || name.Contains("Prop"))
            {
                Plugin.LogEvent("PERSIST", "DontDestroyOnLoad", $"{target.GetType().Name}('{name}')");
            }
        }
    }
}

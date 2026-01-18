using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using Endless.Gameplay;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.Scripting;
using Endless.Props.Assets;
using Endless.Shared.DataTypes;

namespace EndstarPropMod
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "com.endstar.propmod";
        public const string PluginName = "Endstar Prop Mod";
        public const string PluginVersion = "1.0.0";

        internal static ManualLogSource Log;
        internal static Plugin Instance;
        private Harmony _harmony;

        void Awake()
        {
            Instance = this;
            Log = Logger;
            Log.LogInfo($"=== {PluginName} v{PluginVersion} ===");

            // Initialize prop registry
            CustomPropRegistry.Initialize();

            // Apply Harmony patches
            _harmony = new Harmony(PluginGuid);

            // Manual patch for TryGetDefinition (generic method needs special handling)
            BaseTypeLookupPatches.Apply(_harmony);

            // Auto-patch other patches (StageManager.Awake)
            _harmony.PatchAll(typeof(Plugin).Assembly);
            Log.LogInfo("Harmony patches applied");
        }

        internal static void InjectCustomProps(StageManager stageManager)
        {
            foreach (var customProp in CustomPropRegistry.GetAllProps())
            {
                try
                {
                    // Create the Prop object
                    var prop = CustomPropFactory.CreateProp(customProp);

                    // Create test prefab (visual representation)
                    var testPrefab = CustomPropFactory.CreateVisualPrefab(customProp);

                    // Inject using game's built-in system
                    stageManager.InjectProp(prop, testPrefab, null, null);

                    Log.LogInfo($"Injected prop: {customProp.Name} ({customProp.Id})");
                }
                catch (Exception ex)
                {
                    Log.LogError($"Failed to inject prop {customProp.Name}: {ex}");
                }
            }
        }

        void OnDestroy()
        {
            _harmony?.UnpatchSelf();
        }
    }

    /// <summary>
    /// Registry of custom prop definitions
    /// </summary>
    public static class CustomPropRegistry
    {
        private static Dictionary<SerializableGuid, CustomPropDefinition> _props = new Dictionary<SerializableGuid, CustomPropDefinition>();
        private static Dictionary<SerializableGuid, BaseTypeDefinition> _customBaseTypes = new Dictionary<SerializableGuid, BaseTypeDefinition>();
        private static bool _initialized = false;

        public static void Initialize()
        {
            if (_initialized) return;
            _initialized = true;

            // Register a test custom prop
            var testProp = new CustomPropDefinition
            {
                Id = new SerializableGuid("custom-test-prop-00000001"),
                Name = "Custom Test Cube",
                BaseTypeId = new SerializableGuid("custom-basetype-static-001"),
                Color = new Color(0.8f, 0.2f, 0.8f) // Purple
            };
            RegisterProp(testProp);

            Plugin.Log.LogInfo($"Registered {_props.Count} custom prop(s)");
        }

        public static void RegisterProp(CustomPropDefinition prop)
        {
            _props[prop.Id] = prop;

            // Also register the base type if needed
            if (!_customBaseTypes.ContainsKey(prop.BaseTypeId))
            {
                var baseTypeDef = CreateBaseTypeDefinition(prop.BaseTypeId);
                _customBaseTypes[prop.BaseTypeId] = baseTypeDef;
            }
        }

        public static IEnumerable<CustomPropDefinition> GetAllProps() => _props.Values;

        public static bool TryGetCustomBaseType(SerializableGuid id, out BaseTypeDefinition definition)
        {
            return _customBaseTypes.TryGetValue(id, out definition);
        }

        private static BaseTypeDefinition CreateBaseTypeDefinition(SerializableGuid id)
        {
            // Create the definition ScriptableObject
            var definition = ScriptableObject.CreateInstance<BaseTypeDefinition>();

            // Create a prefab with IBaseType component
            var prefab = new GameObject("CustomBaseTypePrefab");
            prefab.SetActive(false); // Keep inactive as template
            var baseType = prefab.AddComponent<CustomBaseType>();

            // Set private fields via reflection
            var prefabField = typeof(ComponentDefinition).GetField("prefab", BindingFlags.NonPublic | BindingFlags.Instance);
            var componentIdField = typeof(ComponentDefinition).GetField("componentId", BindingFlags.NonPublic | BindingFlags.Instance);
            var isUserExposedField = typeof(BaseTypeDefinition).GetField("isUserExposed", BindingFlags.NonPublic | BindingFlags.Instance);

            prefabField?.SetValue(definition, prefab);
            componentIdField?.SetValue(definition, id);
            isUserExposedField?.SetValue(definition, true);

            // Don't destroy the prefab
            UnityEngine.Object.DontDestroyOnLoad(prefab);
            UnityEngine.Object.DontDestroyOnLoad(definition);

            Plugin.Log.LogInfo($"Created BaseTypeDefinition for {id}");
            return definition;
        }
    }

    /// <summary>
    /// Custom base type component that implements IBaseType
    /// </summary>
    public class CustomBaseType : MonoBehaviour, IBaseType
    {
        private Context _context;
        private WorldObject _worldObject;

        public Context Context
        {
            get
            {
                if (_context == null && _worldObject != null)
                {
                    _context = new Context(_worldObject);
                }
                return _context;
            }
        }

        public WorldObject WorldObject => _worldObject;
        public Type ComponentReferenceType => null;
        public ReferenceFilter Filter => ReferenceFilter.None;
        public NavType NavValue => NavType.Static;

        public void ComponentInitialize(Endless.Props.ReferenceComponents.ReferenceBase referenceBase, EndlessProp endlessProp)
        {
            // No special initialization needed
        }

        public void PrefabInitialize(WorldObject worldObject)
        {
            _worldObject = worldObject;
        }
    }

    /// <summary>
    /// Factory for creating Prop and prefab objects
    /// </summary>
    public static class CustomPropFactory
    {
        public static Prop CreateProp(CustomPropDefinition definition)
        {
            // Create prop via ScriptableObject.CreateInstance (proper Unity pattern)
            var prop = ScriptableObject.CreateInstance<Prop>();

            // Set public fields from AssetCore base class
            prop.AssetID = definition.Id.ToString();
            prop.Name = definition.Name;
            prop.AssetType = "Prop";
            prop.AssetVersion = "1.0.0";

            // Set BaseTypeId using the game's method
            prop.SetComponentIds(definition.BaseTypeId, new List<string>());

            UnityEngine.Object.DontDestroyOnLoad(prop);
            return prop;
        }

        public static GameObject CreateVisualPrefab(CustomPropDefinition definition)
        {
            // Create a simple cube as visual representation
            var prefab = GameObject.CreatePrimitive(PrimitiveType.Cube);
            prefab.name = definition.Name + "_Visual";

            // Apply color
            var renderer = prefab.GetComponent<Renderer>();
            if (renderer != null)
            {
                var material = new Material(Shader.Find("Standard"));
                material.color = definition.Color;
                renderer.material = material;
            }

            // Remove collider (the game handles collisions differently)
            var collider = prefab.GetComponent<Collider>();
            if (collider != null)
            {
                UnityEngine.Object.Destroy(collider);
            }

            prefab.SetActive(false);
            UnityEngine.Object.DontDestroyOnLoad(prefab);
            return prefab;
        }
    }

    /// <summary>
    /// Definition of a custom prop
    /// </summary>
    public class CustomPropDefinition
    {
        public SerializableGuid Id { get; set; }
        public string Name { get; set; }
        public SerializableGuid BaseTypeId { get; set; }
        public Color Color { get; set; } = Color.white;
    }

    /// <summary>
    /// Harmony patches for intercepting base type lookups
    /// </summary>
    public static class BaseTypeLookupPatches
    {
        public static void Apply(Harmony harmony)
        {
            // Get the TryGetDefinition method from AbstractComponentList<BaseTypeDefinition>
            var targetType = typeof(AbstractComponentList<BaseTypeDefinition>);
            var targetMethod = targetType.GetMethod("TryGetDefinition", new Type[] {
                typeof(SerializableGuid),
                typeof(BaseTypeDefinition).MakeByRefType()
            });

            if (targetMethod == null)
            {
                Plugin.Log.LogError("Could not find TryGetDefinition method!");
                return;
            }

            var postfix = typeof(BaseTypeLookupPatches).GetMethod("TryGetDefinition_Postfix",
                BindingFlags.Public | BindingFlags.Static);

            harmony.Patch(targetMethod, postfix: new HarmonyMethod(postfix));
            Plugin.Log.LogInfo("Patched AbstractComponentList<BaseTypeDefinition>.TryGetDefinition");
        }

        /// <summary>
        /// Postfix that returns our custom base types when the original method doesn't find them
        /// </summary>
        public static void TryGetDefinition_Postfix(SerializableGuid componentId, ref BaseTypeDefinition componentDefinition, ref bool __result)
        {
            // If the original method found it, don't override
            if (__result) return;

            // Check if it's one of our custom base types
            if (CustomPropRegistry.TryGetCustomBaseType(componentId, out var customDef))
            {
                componentDefinition = customDef;
                __result = true;
                Plugin.Log.LogInfo($"Returning custom BaseTypeDefinition for {componentId}");
            }
        }
    }

    /// <summary>
    /// Harmony patch for StageManager initialization
    /// </summary>
    [HarmonyPatch(typeof(StageManager), "Awake")]
    public static class StageManagerAwakePatch
    {
        [HarmonyPostfix]
        public static void Postfix(StageManager __instance)
        {
            Plugin.Log.LogInfo("StageManager.Awake - injecting custom props");
            try
            {
                Plugin.InjectCustomProps(__instance);
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"Failed to inject props: {ex}");
            }
        }
    }
}

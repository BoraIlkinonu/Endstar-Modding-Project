# Custom Props Plugin Design

Add unlimited custom props to Endstar WITHOUT replacing existing ones.

---

## Overview

| Aspect | Details |
|--------|---------|
| **Approach** | BepInEx plugin + custom asset bundle |
| **Props Count** | Unlimited |
| **Replaces Existing** | No |
| **Complexity** | Advanced |
| **Requirements** | Unity, C#, BepInEx, Harmony |

---

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    ENDSTAR GAME                              │
├─────────────────────────────────────────────────────────────┤
│  Props.dll                    Gameplay.dll                   │
│  ├── PropLoader              ├── PropLibrary                │
│  ├── PropAssets              ├── TreasureItem               │
│  └── PropReferences          └── UsableDefinition           │
├─────────────────────────────────────────────────────────────┤
│                    BepInEx Plugin Layer                      │
├─────────────────────────────────────────────────────────────┤
│  CustomPropsPlugin.dll                                       │
│  ├── Load custom_props.bundle                               │
│  ├── Register CustomPropDefinitions                         │
│  ├── Harmony patches to PropLoader/PropLibrary              │
│  └── Hook into spawning system                              │
├─────────────────────────────────────────────────────────────┤
│  custom_props.bundle (Unity AssetBundle)                    │
│  ├── Prefabs (mesh + material + collider)                   │
│  ├── Textures (albedo, normal, metallic, emission)          │
│  ├── Icons (inventory sprites)                              │
│  └── CustomPropDefinition ScriptableObjects                 │
└─────────────────────────────────────────────────────────────┘
```

---

## Phase 1: Unity Project Setup

### Step 1.1: Create Unity Project

1. Create new Unity project (match Endstar's Unity version if possible)
2. Project name: `EndstarCustomProps`
3. Template: 3D (URP if Endstar uses URP)

### Step 1.2: Create Folder Structure

```
Assets/
├── CustomProps/
│   ├── Prefabs/           # Prop prefabs
│   ├── Textures/          # Prop textures
│   ├── Materials/         # Prop materials
│   ├── Icons/             # Inventory icons
│   └── Definitions/       # ScriptableObject definitions
├── Scripts/
│   └── CustomPropDefinition.cs
└── Editor/
    └── BuildAssetBundle.cs
```

### Step 1.3: Create CustomPropDefinition ScriptableObject

`Assets/Scripts/CustomPropDefinition.cs`:

```csharp
using UnityEngine;

[CreateAssetMenu(fileName = "NewProp", menuName = "Endstar/Custom Prop Definition")]
public class CustomPropDefinition : ScriptableObject
{
    [Header("Identity")]
    public string PropId;           // Unique ID: "pearl_basket_01"
    public string DisplayName;      // "Pearl Basket"
    public string Description;      // "A basket filled with precious pearls"

    [Header("Visuals")]
    public GameObject Prefab;       // 3D model prefab
    public Sprite Icon;             // Inventory icon (128x128)

    [Header("Behavior")]
    public PropBehaviorType Behavior = PropBehaviorType.Treasure;
    public int Value = 100;         // Score/currency value

    [Header("Audio/VFX")]
    public AudioClip PickupSound;
    public GameObject PickupVFX;

    [Header("Spawn Settings")]
    public float SpawnWeight = 1f;  // Relative spawn probability
    public string[] AllowedLevels;  // Empty = all levels
}

public enum PropBehaviorType
{
    Treasure,       // Collect for points/currency
    Key,            // Unlock doors/chests
    Resource,       // Crafting material
    Consumable,     // Use immediately (health, etc.)
    Quest           // Quest item
}
```

### Step 1.4: Create Prop Prefab

For each custom prop:

1. Import your 3D model (FBX/OBJ)
2. Import textures
3. Create material with textures
4. Create prefab:
   - Root GameObject with prop name
   - Mesh Renderer with material
   - Collider (Box/Sphere/Mesh)
   - Scale to match Endstar props (~0.3-0.5 units)

**Prefab structure:**
```
PearlBasket_Prefab
├── MeshRenderer (with PearlBasket_Material)
├── BoxCollider (trigger)
└── (No scripts - plugin handles behavior)
```

### Step 1.5: Create Prop Definition Asset

1. Right-click in Definitions folder
2. Create → Endstar → Custom Prop Definition
3. Fill in all fields:
   - PropId: `pearl_basket_01`
   - DisplayName: `Pearl Basket`
   - Prefab: drag your prefab
   - Icon: drag your icon sprite
   - Behavior: Treasure
   - Value: 100

### Step 1.6: Build Asset Bundle

`Assets/Editor/BuildAssetBundle.cs`:

```csharp
using UnityEditor;
using UnityEngine;
using System.IO;

public class BuildAssetBundle
{
    [MenuItem("Endstar/Build Custom Props Bundle")]
    public static void Build()
    {
        string outputPath = "Assets/AssetBundles";
        if (!Directory.Exists(outputPath))
            Directory.CreateDirectory(outputPath);

        // Mark all prop assets for bundling
        var prefabs = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/CustomProps/Prefabs" });
        var definitions = AssetDatabase.FindAssets("t:CustomPropDefinition", new[] { "Assets/CustomProps/Definitions" });

        // Create bundle build
        AssetBundleBuild build = new AssetBundleBuild
        {
            assetBundleName = "custom_props.bundle",
            assetNames = new string[prefabs.Length + definitions.Length]
        };

        int i = 0;
        foreach (var guid in prefabs)
            build.assetNames[i++] = AssetDatabase.GUIDToAssetPath(guid);
        foreach (var guid in definitions)
            build.assetNames[i++] = AssetDatabase.GUIDToAssetPath(guid);

        BuildPipeline.BuildAssetBundles(
            outputPath,
            new[] { build },
            BuildAssetBundleOptions.None,
            BuildTarget.StandaloneWindows64
        );

        Debug.Log($"Built: {outputPath}/custom_props.bundle");
    }
}
```

Build the bundle: **Endstar → Build Custom Props Bundle**

---

## Phase 2: BepInEx Plugin Development

### Step 2.1: Create Plugin Project

```
Zayed_Character_Mod/
└── CustomProps/
    ├── CustomPropsPlugin.csproj
    ├── CustomPropsPlugin.cs
    ├── PropRegistry.cs
    ├── PropSpawnPatcher.cs
    └── Patches/
        ├── PropLoaderPatch.cs
        └── PropLibraryPatch.cs
```

### Step 2.2: Project File

`CustomPropsPlugin.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netstandard2.1</TargetFramework>
    <AssemblyName>CustomPropsPlugin</AssemblyName>
    <Version>1.0.0</Version>
  </PropertyGroup>

  <ItemGroup>
    <Reference Include="UnityEngine">
      <HintPath>$(EndstarPath)\Endstar_Data\Managed\UnityEngine.dll</HintPath>
    </Reference>
    <Reference Include="UnityEngine.CoreModule">
      <HintPath>$(EndstarPath)\Endstar_Data\Managed\UnityEngine.CoreModule.dll</HintPath>
    </Reference>
    <Reference Include="BepInEx">
      <HintPath>$(EndstarPath)\BepInEx\core\BepInEx.dll</HintPath>
    </Reference>
    <Reference Include="0Harmony">
      <HintPath>$(EndstarPath)\BepInEx\core\0Harmony.dll</HintPath>
    </Reference>
    <Reference Include="Props">
      <HintPath>$(EndstarPath)\Endstar_Data\Managed\Props.dll</HintPath>
    </Reference>
    <Reference Include="Gameplay">
      <HintPath>$(EndstarPath)\Endstar_Data\Managed\Gameplay.dll</HintPath>
    </Reference>
  </ItemGroup>
</Project>
```

### Step 2.3: Main Plugin Class

`CustomPropsPlugin.cs`:

```csharp
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

namespace CustomProps
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class CustomPropsPlugin : BaseUnityPlugin
    {
        public const string GUID = "com.endstar.customprops";
        public const string NAME = "Custom Props";
        public const string VERSION = "1.0.0";

        public static ManualLogSource Log { get; private set; }
        public static AssetBundle PropsBundle { get; private set; }
        public static List<CustomPropDefinition> LoadedProps { get; } = new();

        private Harmony _harmony;

        private void Awake()
        {
            Log = Logger;
            Log.LogInfo($"{NAME} v{VERSION} initializing...");

            // Load asset bundle
            LoadAssetBundle();

            // Load prop definitions
            LoadPropDefinitions();

            // Apply Harmony patches
            _harmony = new Harmony(GUID);
            _harmony.PatchAll();

            Log.LogInfo($"{NAME} loaded {LoadedProps.Count} custom props!");
        }

        private void LoadAssetBundle()
        {
            string pluginDir = Path.GetDirectoryName(Info.Location);
            string bundlePath = Path.Combine(pluginDir, "custom_props.bundle");

            if (File.Exists(bundlePath))
            {
                PropsBundle = AssetBundle.LoadFromFile(bundlePath);
                Log.LogInfo($"Loaded bundle: {bundlePath}");
            }
            else
            {
                Log.LogError($"Bundle not found: {bundlePath}");
            }
        }

        private void LoadPropDefinitions()
        {
            if (PropsBundle == null) return;

            var definitions = PropsBundle.LoadAllAssets<CustomPropDefinition>();
            foreach (var def in definitions)
            {
                LoadedProps.Add(def);
                PropRegistry.Register(def);
                Log.LogInfo($"Registered prop: {def.PropId} ({def.DisplayName})");
            }
        }

        private void OnDestroy()
        {
            _harmony?.UnpatchSelf();
            PropsBundle?.Unload(true);
        }
    }
}
```

### Step 2.4: Prop Registry

`PropRegistry.cs`:

```csharp
using System.Collections.Generic;
using UnityEngine;

namespace CustomProps
{
    public static class PropRegistry
    {
        private static Dictionary<string, CustomPropDefinition> _props = new();
        private static Dictionary<string, GameObject> _instances = new();

        public static void Register(CustomPropDefinition prop)
        {
            if (!_props.ContainsKey(prop.PropId))
            {
                _props[prop.PropId] = prop;
            }
        }

        public static CustomPropDefinition Get(string propId)
        {
            return _props.TryGetValue(propId, out var prop) ? prop : null;
        }

        public static IEnumerable<CustomPropDefinition> GetAll()
        {
            return _props.Values;
        }

        public static GameObject Spawn(string propId, Vector3 position, Quaternion rotation)
        {
            var prop = Get(propId);
            if (prop == null || prop.Prefab == null)
            {
                CustomPropsPlugin.Log.LogWarning($"Cannot spawn prop: {propId}");
                return null;
            }

            var instance = Object.Instantiate(prop.Prefab, position, rotation);
            instance.name = $"CustomProp_{prop.PropId}";

            // Add pickup handler
            var handler = instance.AddComponent<CustomPropPickup>();
            handler.Definition = prop;

            return instance;
        }
    }
}
```

### Step 2.5: Pickup Handler

`CustomPropPickup.cs`:

```csharp
using UnityEngine;

namespace CustomProps
{
    public class CustomPropPickup : MonoBehaviour
    {
        public CustomPropDefinition Definition { get; set; }

        private void OnTriggerEnter(Collider other)
        {
            // Check if player
            if (!IsPlayer(other)) return;

            // Handle pickup based on behavior
            switch (Definition.Behavior)
            {
                case PropBehaviorType.Treasure:
                    HandleTreasurePickup();
                    break;
                case PropBehaviorType.Key:
                    HandleKeyPickup();
                    break;
                case PropBehaviorType.Resource:
                    HandleResourcePickup();
                    break;
                case PropBehaviorType.Consumable:
                    HandleConsumablePickup();
                    break;
                case PropBehaviorType.Quest:
                    HandleQuestPickup();
                    break;
            }

            // Play effects
            PlayPickupEffects();

            // Destroy prop
            Destroy(gameObject);
        }

        private bool IsPlayer(Collider other)
        {
            // TODO: Check Endstar's player tag/layer
            return other.CompareTag("Player") || other.GetComponent<PlayerController>() != null;
        }

        private void HandleTreasurePickup()
        {
            // TODO: Hook into Endstar's score/currency system
            CustomPropsPlugin.Log.LogInfo($"Collected treasure: {Definition.DisplayName} (+{Definition.Value})");
        }

        private void HandleKeyPickup()
        {
            // TODO: Add to player's key inventory
            CustomPropsPlugin.Log.LogInfo($"Collected key: {Definition.DisplayName}");
        }

        private void HandleResourcePickup()
        {
            // TODO: Add to player's resource inventory
            CustomPropsPlugin.Log.LogInfo($"Collected resource: {Definition.DisplayName}");
        }

        private void HandleConsumablePickup()
        {
            // TODO: Apply immediate effect
            CustomPropsPlugin.Log.LogInfo($"Used consumable: {Definition.DisplayName}");
        }

        private void HandleQuestPickup()
        {
            // TODO: Update quest state
            CustomPropsPlugin.Log.LogInfo($"Collected quest item: {Definition.DisplayName}");
        }

        private void PlayPickupEffects()
        {
            if (Definition.PickupSound != null)
            {
                AudioSource.PlayClipAtPoint(Definition.PickupSound, transform.position);
            }

            if (Definition.PickupVFX != null)
            {
                Instantiate(Definition.PickupVFX, transform.position, Quaternion.identity);
            }
        }
    }
}
```

### Step 2.6: Harmony Patches (Investigate Required)

`Patches/PropLoaderPatch.cs`:

```csharp
using HarmonyLib;
using System.Collections.Generic;

namespace CustomProps.Patches
{
    /// <summary>
    /// Patches to integrate custom props into Endstar's prop system.
    /// These patches need to be refined based on actual game code analysis.
    /// </summary>

    // Example: Patch PropLibrary to include custom props
    // [HarmonyPatch(typeof(Endless.Gameplay.LevelEditing.PropLibrary), "GetAllProps")]
    // public class PropLibraryPatch
    // {
    //     [HarmonyPostfix]
    //     public static void Postfix(ref List<PropInfo> __result)
    //     {
    //         foreach (var customProp in PropRegistry.GetAll())
    //         {
    //             __result.Add(new PropInfo
    //             {
    //                 Id = customProp.PropId,
    //                 Name = customProp.DisplayName,
    //                 Prefab = customProp.Prefab
    //             });
    //         }
    //     }
    // }

    // Example: Patch spawning system
    // [HarmonyPatch(typeof(Props.Loaders.PropLoader), "SpawnProp")]
    // public class PropSpawnerPatch
    // {
    //     [HarmonyPrefix]
    //     public static bool Prefix(string propId, Vector3 position, ref GameObject __result)
    //     {
    //         // Check if it's a custom prop
    //         var customProp = PropRegistry.Get(propId);
    //         if (customProp != null)
    //         {
    //             __result = PropRegistry.Spawn(propId, position, Quaternion.identity);
    //             return false; // Skip original method
    //         }
    //         return true; // Continue with original
    //     }
    // }
}
```

---

## Phase 3: Deployment

### Step 3.1: Build Plugin

```bash
dotnet build -c Release
```

### Step 3.2: Deploy Files

Copy to BepInEx plugins folder:

```
C:\Endless Studios\Endless Launcher\Endstar\BepInEx\plugins\CustomProps\
├── CustomPropsPlugin.dll
└── custom_props.bundle
```

### Step 3.3: Test

1. Launch Endstar.exe directly
2. Check BepInEx console for plugin load messages
3. Verify props spawn in game

---

## Phase 4: Spawning Custom Props

### Option A: Console Commands (Debug)

Add to plugin:

```csharp
private void Update()
{
    if (Input.GetKeyDown(KeyCode.F9))
    {
        // Spawn test prop at player position
        var player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            var pos = player.transform.position + player.transform.forward * 2f;
            PropRegistry.Spawn("pearl_basket_01", pos, Quaternion.identity);
        }
    }
}
```

### Option B: Replace Spawner Logic

Patch the game's prop spawner to include custom props in spawn pools.

### Option C: Custom Spawner

Create placement system for level designers to place custom props.

---

## File Structure Summary

```
D:\Endstar Plot Flag\PLOT FLAG SYSTEM\
├── Zayed_Character_Mod\
│   └── CustomProps\
│       ├── CustomPropsPlugin.cs
│       ├── CustomPropsPlugin.csproj
│       ├── PropRegistry.cs
│       ├── CustomPropPickup.cs
│       └── Patches\
│           └── PropLoaderPatch.cs
│
├── Unity_CustomProps\              # Unity project
│   └── Assets\
│       ├── CustomProps\
│       │   ├── Prefabs\
│       │   ├── Textures\
│       │   └── Definitions\
│       └── AssetBundles\
│           └── custom_props.bundle
│
└── Documentation\
    └── CUSTOM_PROPS_PLUGIN_DESIGN.md
```

---

## Next Steps

1. **Reverse engineer** Endstar's prop system in detail:
   - Decompile Props.dll and Gameplay.dll with dnSpy
   - Find exact classes for PropLibrary, PropLoader
   - Identify spawn points and spawner logic

2. **Implement Harmony patches** based on findings

3. **Build Unity project** with 10-15 custom props

4. **Test integration** with Endstar's inventory/score system

---

## Comparison: Replacement vs Plugin

| Aspect | Replacement | Plugin |
|--------|-------------|--------|
| Max Props | 3 per type | Unlimited |
| Complexity | Easy | Advanced |
| Unity Required | No | Yes |
| C# Coding | No | Yes |
| Game Updates | May break | May break |
| Reversibility | Restore backup | Remove plugin |

---

## Changelog

- **2026-01-06:** Initial design document created
- **2026-01-06:** Added plugin architecture and code templates
- **2026-01-06:** Added Unity project setup guide

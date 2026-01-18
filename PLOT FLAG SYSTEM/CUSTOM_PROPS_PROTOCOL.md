# Custom Props Protocol - Step by Step Guide

## Overview

This document describes the complete workflow for adding custom props to Endstar, from raw mesh to in-game placeable object.

---

## Phase 1: Asset Preparation (User in Unity)

### Step 1.1: Prepare Your Mesh

**Requirements:**
- 3D mesh file (FBX, OBJ, or similar)
- Textures (albedo, normal, etc.)
- Recommended: LOD variants for optimization

**Actions:**
1. Import mesh into Unity project
2. Set import settings:
   - Scale Factor: Match Endstar's scale (typically 1.0)
   - Mesh Compression: Medium or High
   - Generate Lightmap UVs: Yes (if needed)

### Step 1.2: Create Prefab

**Requirements:**
- Unity 2021.3.x or compatible version
- Mesh imported and configured

**Actions:**
1. Drag mesh into scene
2. Add required components:
   - `MeshRenderer` (automatic with mesh)
   - `MeshFilter` (automatic with mesh)
   - `Collider` (BoxCollider or MeshCollider for interaction)
3. Set materials and textures
4. Position pivot point at base center (for proper placement)
5. Drag from Hierarchy to Project to create prefab

### Step 1.3: Add Optimization (LODGroup)

**Optional but Recommended:**
1. Create LOD variants of mesh (LOD0, LOD1, LOD2)
2. Add `LODGroup` component to prefab root
3. Configure LOD levels:
   ```
   LOD 0: 0% - 50%   (full detail)
   LOD 1: 50% - 80%  (medium detail)
   LOD 2: 80% - 100% (low detail)
   Culled: 100%+
   ```

### Step 1.4: Create Icon

**Requirements:**
- Square image (recommended: 256x256 or 512x512)
- PNG format with transparency
- Clear representation of the prop

**Actions:**
1. Take screenshot of prop or create icon art
2. Import into Unity as Texture2D
3. Set import settings:
   - Texture Type: Sprite (2D and UI)
   - Sprite Mode: Single
   - Pixels Per Unit: 100

---

## Phase 2: Build AssetBundle (User in Unity)

### Step 2.1: Mark Assets for Bundle

**Actions:**
1. Select prefab in Project window
2. At bottom of Inspector, find "AssetBundle" dropdown
3. Create new bundle name: `customprops`
4. Repeat for icon sprite

### Step 2.2: Create Build Script

**Create file:** `Assets/Editor/BuildAssetBundles.cs`

```csharp
using UnityEditor;
using System.IO;

public class BuildAssetBundles
{
    [MenuItem("Assets/Build AssetBundles")]
    static void BuildAllAssetBundles()
    {
        string outputPath = "Assets/AssetBundles";

        if (!Directory.Exists(outputPath))
            Directory.CreateDirectory(outputPath);

        BuildPipeline.BuildAssetBundles(
            outputPath,
            BuildAssetBundleOptions.ChunkBasedCompression,  // LZ4
            BuildTarget.StandaloneWindows64
        );

        UnityEngine.Debug.Log("AssetBundles built to: " + outputPath);
    }
}
```

### Step 2.3: Build the Bundle

**Actions:**
1. In Unity menu: `Assets → Build AssetBundles`
2. Wait for build to complete
3. Find output in `Assets/AssetBundles/customprops`
4. Rename to `customprops.bundle` (optional, for clarity)

**Output Files:**
```
Assets/AssetBundles/
├── customprops              ← This is your bundle (rename to .bundle)
├── customprops.manifest     ← Metadata (not needed at runtime)
├── AssetBundles             ← Main manifest
└── AssetBundles.manifest
```

---

## Phase 3: Deploy to Game (User)

### Step 3.1: Install BepInEx

**If not already installed:**
1. Download BepInEx 5.x for Unity IL2CPP or Mono (match Endstar)
2. Extract to Endstar game folder
3. Run game once to generate config

### Step 3.2: Deploy Plugin Files

**Copy to:** `<Endstar>/BepInEx/plugins/CustomProps/`

```
BepInEx/plugins/CustomProps/
├── CustomPropsPlugin.dll    ← Built mod DLL
└── customprops.bundle       ← Your AssetBundle from Unity
```

### Step 3.3: Configure Props (Future: manifest.json)

**Optional manifest for multiple props:**
```json
{
  "props": [
    {
      "id": "pearl_basket_001",
      "name": "Pearl Basket",
      "prefabPath": "Assets/Pearl Basket/PearlBasket.prefab",
      "iconPath": "Assets/Pearl Basket/pearl basket icon.png",
      "bounds": [1, 1, 1]
    }
  ]
}
```

---

## Phase 4: Runtime Injection (Automatic - Plugin Code)

### Step 4.1: Plugin Initialization

**When:** Game starts, BepInEx loads plugin

**What happens:**
1. `CustomPropsPlugin.Awake()` called
2. `DirectPropInjector.Initialize()` caches reflection data:
   - Types: StageManager, PropLibrary, Prop, AssetReference, etc.
   - Methods: StageManager.InjectProp, PropLibrary.InjectProp
   - Fields: loadedPropMap, prefabSpawnRoot, basePropPrefab
3. `LoadAssetBundle()` loads `customprops.bundle`
4. Extracts prefab and icon from bundle

### Step 4.2: Prop Injection

**When:** User enters Creator mode (props loaded)

**What happens:**
1. Wait for `loadedPropMap.Count > 0` (props loaded)
2. Get existing prop from loadedPropMap (e.g., Treasure)
3. Clone all fields from existing prop (HOOK E: real data only)
4. Modify cloned prop:
   - `AssetID` → unique ID (e.g., "pearl_basket_001")
   - `Name` → display name (e.g., "Pearl Basket")
   - `Description` → description text
   - `prefabBundle` → **null** (prevents loading cloned asset)
   - `iconFileInstanceId` → 0 (use provided sprite)
   - `bounds` → Vector3Int(1,1,1)
5. Call injection methods:
   ```csharp
   // Store testPrefab in injectedProps list
   StageManager.InjectProp(prop, testPrefab, null, icon);

   // Add to loadedPropMap for UI visibility
   PropLibrary.InjectProp(prop, testPrefab, null, icon, prefabSpawnRoot, basePropPrefab);
   ```
6. Prop appears in Creator mode prop library

### Step 4.3: Prop Usage

**When:** User places prop in stage

**What happens:**
1. User selects Pearl Basket from prop library
2. PropLibrary.BuildPrefab creates EndlessProp instance
3. Since `prefabBundle = null`, only testPrefab mesh is used
4. Prop placed in stage with correct visuals

---

## Phase 5: Claude Code Workflow (What I Do)

### Building the Mod

**Command sequence:**
```
1. taskkill /F /IM Endstar.exe          ← Kill running instances (hook does this)
2. dotnet build -c Release               ← Build plugin DLL
3. (Automatic copy to BepInEx/plugins)   ← Post-build script
4. start "" "...Endstar.exe"             ← Launch game for testing
```

### Testing & Debugging

**Log location:** `<Endstar>/BepInEx/LogOutput.log`

**Key log messages to verify:**
```
[CustomProps] === DirectPropInjector Initializing ===
[CustomProps] Asset bundle loaded. Contents:
[CustomProps]   - assets/pearl basket/pearlbasket.prefab
[CustomProps]   - assets/pearl basket/pearl basket icon.png
[CustomProps] Loaded PearlBasket prefab: True
[CustomProps] Loaded PearlBasket icon: True
[CustomProps] === Injecting prop using OFFICIAL API: Pearl Basket ===
[CustomProps] loadedPropMap count before: 75
[CustomProps] Clearing prefabBundle (was: treasure_prefab_id)
[CustomProps] prefabBundle set to null - testPrefab will be used exclusively
[CustomProps] StageManager.InjectProp called!
[CustomProps] PropLibrary.InjectProp called!
[CustomProps] loadedPropMap count after: 76
[CustomProps] === Successfully injected: Pearl Basket ===
```

### Code Modification Workflow

**Before ANY code change, I must (HOOK A):**
1. Identify class/method being used
2. Verify it exists in DLL (quote from ENDSTAR_ASSEMBLY_REFERENCE.md)
3. List exact parameter types and return types
4. Explain when it's called in game lifecycle

**After ANY failure (HOOK C):**
1. Stop trying different approaches
2. Analyze exact error message
3. Go back to DLLs for actual types/signatures
4. Document findings
5. Then try again with verified knowledge

---

## Quick Reference: File Locations

| File | Location |
|------|----------|
| Plugin source | `D:\Endstar Plot Flag\PLOT FLAG SYSTEM\Zayed_Character_Mod\CustomProps\` |
| Built DLL | `...\CustomProps\bin\Release\netstandard2.1\CustomPropsPlugin.dll` |
| Deployed DLL | `C:\Endless Studios\Endless Launcher\Endstar\BepInEx\plugins\CustomPropsPlugin.dll` |
| Asset bundle | `...\BepInEx\plugins\customprops.bundle` |
| Game logs | `C:\Endless Studios\Endless Launcher\Endstar\BepInEx\LogOutput.log` |
| Game DLLs | `C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed\` |

---

## Quick Reference: Key Methods

| Method | Purpose | Parameters |
|--------|---------|------------|
| `StageManager.InjectProp` | Store testPrefab in injectedProps | `(Prop, GameObject testPrefab, MonoScript, Sprite icon)` |
| `PropLibrary.InjectProp` | Add to loadedPropMap for UI | `(Prop, GameObject testPrefab, MonoScript, Sprite icon, Transform prefabSpawnRoot, EndlessProp basePropPrefab)` |
| `AssetBundle.LoadFromFile` | Load bundle from disk | `(string path)` |
| `AssetBundle.LoadAsset<T>` | Extract asset from bundle | `(string assetPath)` |

---

## Checklist: Adding a New Prop

### User Tasks:
- [ ] Create/obtain 3D mesh
- [ ] Import into Unity
- [ ] Create prefab with proper components
- [ ] Add LODGroup (optional optimization)
- [ ] Create icon sprite
- [ ] Mark assets for AssetBundle
- [ ] Build AssetBundle with LZ4 compression
- [ ] Copy .bundle file to BepInEx/plugins/

### Claude Tasks:
- [ ] Update plugin code if needed (new prop ID, paths)
- [ ] Kill existing game instances
- [ ] Build plugin DLL
- [ ] Start game
- [ ] Verify in logs: bundle loaded, prop injected
- [ ] Test in Creator mode: prop visible, correct mesh, correct icon

# Custom Props Approach Analysis

## Architecture Overview

### What We Use vs Don't Use

```
┌─────────────────────────────┐
│      Addressables           │  ← We DON'T use this (game's native system)
│  (catalog, streaming, etc.) │
├─────────────────────────────┤
│      AssetBundles           │  ← We DO use this
│  (basic asset packaging)    │
└─────────────────────────────┘
```

**AssetBundle** - Unity's basic asset packaging format
- We build `customprops.bundle` containing prefabs + icons
- Load with `AssetBundle.LoadFromFile()`
- Simple, direct, no catalog needed

**Addressables** - Unity's higher-level system (built ON TOP of AssetBundles)
- Catalog system (asset registry)
- Remote/cloud loading
- Automatic dependency management
- Smart memory management
- Asset references by address
- **We bypass this entirely**

---

## Current Approach: AssetBundle + testPrefab Injection

### How It Works
1. Build Unity AssetBundle containing prefab + icon
2. Load bundle at runtime with `AssetBundle.LoadFromFile()`
3. Clone existing prop (e.g., Treasure) to get all required fields (HOOK E compliance)
4. Set `prefabBundle = null` to prevent loading original cloned asset
5. Call BOTH injection methods:
   - `StageManager.InjectProp(prop, testPrefab, script, icon)` - stores testPrefab in injectedProps
   - `PropLibrary.InjectProp(prop, testPrefab, script, icon, prefabSpawnRoot, basePropPrefab)` - adds to loadedPropMap for UI
6. Pass our prefab as `testPrefab` parameter
7. Pass our icon directly as `Sprite`

### Why Clone + Clear prefabBundle?

We clone from an existing prop (e.g., Treasure) to get all required fields. But the cloned prop has Treasure's prefabBundle pointing to Treasure's asset.

**If we DON'T clear prefabBundle:**
```
BuildPrefab runs:
  1. Sees prefabBundle → loads Treasure mesh
  2. Sees testPrefab → loads Pearl Basket mesh
  3. Result: BOTH meshes combined = bug (merged meshes)
```

**If we set prefabBundle = null:**
```
BuildPrefab runs:
  1. Sees prefabBundle = null → loads nothing
  2. Sees testPrefab → loads Pearl Basket mesh
  3. Result: Only Pearl Basket mesh = correct!
```

**Important:** We are NOT replacing Treasure. We create a NEW prop entry:
- Treasure remains unchanged (ID: treasure, prefabBundle: treasure_asset)
- Pearl Basket is added as new entry (ID: pearl_basket_001, prefabBundle: null, testPrefab: our mesh)

---

## Downsides & Mitigations

| Issue | Impact | Mitigation |
|-------|--------|------------|
| **Save/Load** | Stages with custom props need mod to load | Document requirement for users |
| **Multiplayer** | All players need mod installed | Document requirement; share mod with friends |
| **Memory** | Bundles stay in RAM | Implement load/unload on Creator mode enter/exit |
| **LOD/Optimization** | No automatic LOD | Add LODGroup to prefabs in Unity |
| **Icons** | iconFileInstanceId=0 workaround | Pass sprite directly, works for most UI |
| **Game Updates** | May break with updates | Monitor updates, fix as needed |

### Manageable Issues

1. **Save/Load** - Document: "Mod required to load stages containing custom props"
2. **Multiplayer** - Document: "All players must have mod installed to see custom props"
3. **Memory** - Implement smart loading (see Optimizations below)
4. **LOD/Culling** - Add to prefabs in Unity (see Optimizations below)

---

## Optimizations To Implement

### 1. LOD (Level of Detail)
Add `LODGroup` component to prefab in Unity before building bundle:
```
LOD 0: Full detail mesh (0-50% screen height)
LOD 1: Medium detail mesh (50-80% screen height)
LOD 2: Low detail mesh (80-100% screen height)
Culled: Beyond 100%
```

### 2. Frustum Culling
**Already automatic** in Unity for all renderers. No action needed.

### 3. AssetBundle Compression
When building AssetBundle in Unity, use compression:
```csharp
// In Unity Editor build script
BuildPipeline.BuildAssetBundles(
    outputPath,
    BuildAssetBundleOptions.ChunkBasedCompression,  // LZ4 - fast decompression
    BuildTarget.StandaloneWindows64
);
```

Options:
- `ChunkBasedCompression` (LZ4) - Fast load, good compression (RECOMMENDED)
- `None` - Fastest load, largest file
- Default (LZMA) - Smallest file, slower load

### 4. Smart Bundle Loading/Unloading
```csharp
// Load when entering Creator mode
void OnCreatorModeEnter()
{
    _assetBundle = AssetBundle.LoadFromFile(bundlePath);
    _prefab = _assetBundle.LoadAsset<GameObject>("PearlBasket");
}

// Unload when leaving Creator mode
void OnCreatorModeExit()
{
    _assetBundle.Unload(true);  // true = unload all loaded assets
    _assetBundle = null;
    _prefab = null;
}
```

### 5. Lazy Prop Loading (Future)
Only load specific props when user selects them:
```csharp
// Don't load all props at once
// Load individual prop when user clicks it in UI
void OnPropSelected(string propId)
{
    if (!loadedPrefabs.ContainsKey(propId))
    {
        var prefab = _assetBundle.LoadAsset<GameObject>(propId);
        loadedPrefabs[propId] = prefab;
    }
}
```

### 6. Mesh Optimization in Unity
Before building bundle:
- Optimize mesh in import settings (Mesh Compression: Medium/High)
- Remove unused vertices/UVs
- Combine meshes where possible
- Use appropriate texture sizes (512x512 for small props)

---

## File Structure

```
BepInEx/
└── plugins/
    └── CustomProps/
        ├── CustomPropsPlugin.dll      # Main plugin
        ├── customprops.bundle         # AssetBundle (compressed LZ4)
        └── manifest.json              # Optional: prop metadata
```

---

## User Requirements (Document for End Users)

### Requirements
- BepInEx 5.x installed
- Endstar game

### Limitations
1. **Save/Load**: Stages containing custom props require this mod to load correctly
2. **Multiplayer**: All players in a session must have the mod installed to see custom props
3. **Updates**: Mod may need updates after major game patches

---

## Summary

**Current approach is VIABLE** with proper optimizations:
- Use AssetBundles (not Addressables)
- Clone real props, clear prefabBundle, pass testPrefab
- Add LODGroup to prefabs for optimization
- Use LZ4 compression for bundles
- Implement load/unload for memory management
- Document requirements for users

No need to integrate with Endstar's Addressables system - too complex and invasive for the benefits.

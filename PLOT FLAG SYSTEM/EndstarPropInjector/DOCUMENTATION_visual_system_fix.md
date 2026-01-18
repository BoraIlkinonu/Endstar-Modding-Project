# EndstarPropInjector: Visual System Fix Documentation

## Version: v10.50.0 (Final Working Version)
## Date: January 16, 2026

---

# PART 1: PROBLEM HISTORY

## 1.1 The Original Problem: Double Visual Bug

When the custom Pearl Basket prop was placed in the world, TWO visual meshes rendered simultaneously:
1. **prefabInstance** - A static visual from the testPrefab parameter
2. **runtimeGroundVisuals** - The rotating visual from GroundVisualsInfo

This caused a "merged" or "ghosting" appearance where both models overlapped.

## 1.2 Version History and Attempted Fixes

| Version | Approach | Result | Why It Failed |
|---------|----------|--------|---------------|
| v10.36.0 | Hide renderers in PREFIX hook | Preview broken | Modified template before clone - preview cloned hidden renderers |
| v10.37.0 | Hide renderers in POSTFIX hook | No change | POSTFIX ran on template creation, not on placed instance |
| v10.38.0 | TrackNonNetworkedObject hook | Untested | Correct hook point but implementation incomplete |
| v10.45.0 | Asset bundle + testPrefab + GroundVisualsInfo | Double visual | Both visual paths were active simultaneously |
| v10.46.0 | Set GroundVisualsInfo=null | Error visual | Visibility system requires runtimeGroundVisuals to exist |
| v10.47.0 | testPrefab=null + inject GroundVisualsInfo | No double visual, no preview | Prefab SetActive(false) broke preview |
| v10.48.0 | Same as v10.47.0 + log fixes | No double visual, no preview | Same root cause - inactive prefab |
| v10.49.0 | Keep prefab ACTIVE + position offset | Working but hacky | Position offset workaround, not matching game pipeline |
| **v10.50.0** | **Use prefab ASSET directly (no Instantiate)** | **WORKING** | **Exactly matches game's pipeline** |

---

# PART 2: KEY DISCOVERIES

## 2.1 Discovery #1: Normal Props Pass testPrefab=null

**Source**: `PropLibrary.cs` line 479

```csharp
// Line 479 - Normal props pass testPrefab=null!
await newProp.BuildPrefab(prop, null, null, cancelToken);
```

Normal game props do NOT use the testPrefab parameter. They rely entirely on VisualsInfo.GameObject for their visuals.

**Our fix**: Pass testPrefab=null in InjectProp call (Plugin.cs line 3573):
```csharp
injectPropMethod.Invoke(stageManager, new object[] { prop, null, null, iconSprite });
```

## 2.2 Discovery #2: Two Visual Creation Paths in BuildPrefab

**Source**: `EndlessProp.cs` lines 109-245

There are TWO independent visual creation paths:

### Path A: prefabInstance (testPrefab parameter)
```csharp
// Line 168 - Only created if testPrefab is provided
if (testPrefab != null)
{
    prefabInstance = Object.Instantiate(testPrefab, baseTypeComponent.transform);
}
```

### Path B: runtimeGroundVisuals (VisualsInfo.GameObject)
```csharp
// Line 726 in Item.ComponentInitialize - ALWAYS created
this.runtimeGroundVisuals = Object.Instantiate(
    this.GroundVisualsInfo.GameObject,
    this.runtimeGroundedVisualsParentGameObject.transform
);
```

**The double visual bug occurred because BOTH paths were active.**

## 2.3 Discovery #3: Unity Instantiate Preserves Active State

**Unity Documentation**: `Object.Instantiate()` creates a clone that inherits the **active state** of the source GameObject.

- If source `activeSelf == true` → Clone `activeSelf == true`
- If source `activeSelf == false` → Clone `activeSelf == false`

**This was the root cause of the preview bug in v10.47.0-v10.48.0**:
- We called `_pearlBasketVisualPrefab.SetActive(false)`
- ComponentInitialize cloned it → `runtimeGroundVisuals` was INACTIVE
- Template stored with inactive visual
- SpawnPreview cloned template → Preview had inactive visual
- Preview showed nothing

## 2.4 Discovery #4: PrefabInitialize Only Deactivates Equipped Visuals

**Source**: `Item.cs` lines 740-748

```csharp
public void PrefabInitialize(WorldObject worldObject)
{
    // Register renderers
    this.WorldObject.EndlessVisuals.ManageRenderers(list);

    // Line 746: ONLY equipped visuals are deactivated
    this.runtimeEquippedVisuals.SetActive(false);

    // runtimeGroundVisuals is NEVER touched here!
}
```

Ground visuals stay in whatever state they were cloned with.

## 2.5 Discovery #5: HandleNetStateChanged Only Fires After networkSetup

**Source**: `Item.cs` lines 346-359

```csharp
private void HandleNetStateChanged(Item.NetState oldState, Item.NetState newState)
{
    if (!this.networkSetup)
        return;  // GATED - only runs when networkSetup=true

    // Line 355: Controls ground visuals visibility
    runtimeGroundVisuals.SetActive(state == Ground || Tossed || Teleporting);
}
```

**Critical finding**: Templates and preview clones NEVER have `networkSetup=true`.
Therefore, `HandleNetStateChanged` never runs on them, and visuals stay in their cloned state.

## 2.6 Discovery #6: Normal Game Props Use ACTIVE Prefabs

**Source**: `TreasureItem.cs` lines 47-53

```csharp
[SerializeField]
private Item.VisualsInfo tempVisualsInfoGround;

[SerializeField]
private Item.VisualsInfo tempVisualsInfoEqupped;
```

These fields are set in **Unity Editor** where designers drag prefab assets.
Prefab assets in Unity are **ACTIVE by default**.

There is NO code in the game that deactivates VisualsInfo.GameObject before use.
The game assumes these prefabs are active.

## 2.7 Discovery #7: VisualsInfo.GameObject is a PREFAB ASSET Reference (Not Instantiated)

**Critical insight for v10.50.0:**

The game's `VisualsInfo.GameObject` stores a reference to a **prefab asset** - NOT an instantiated GameObject in the scene.

**Game's flow:**
1. `tempVisualsInfoGround` is `[SerializeField]` - baked into TreasureItem prefab at design time
2. It references a prefab asset (data), not a scene object
3. `ComponentInitialize` calls `Instantiate()` on this prefab asset reference
4. The instantiated clone becomes `runtimeGroundVisuals`

**Our previous mistake (v10.47.0-v10.49.0):**
```csharp
// We were creating an INSTANTIATED object in the scene
_pearlBasketVisualPrefab = UnityEngine.Object.Instantiate(sourceVisual);
```

**The correct approach (v10.50.0):**
```csharp
// Use the prefab asset DIRECTLY - no Instantiate
_pearlBasketVisualPrefab = sourceVisual;
```

This matches exactly how the game works - `VisualsInfo.GameObject` is a prefab asset reference, and `ComponentInitialize` is the one that instantiates it.

---

# PART 3: THE COMPLETE PIPELINE

## 3.1 Prop Creation Pipeline

```
StageManager.InjectProp(prop, testPrefab=null, testScript, icon)
    │
    └─ Stores in injectedProps list
           │
           ↓
LoadPropLibraryReferences() (during level load)
    │
    └─ PropLibrary.InjectProp()
           │
           ↓
EndlessProp.BuildPrefab(prop, testPrefab=null, testScript)
    │
    ├─[Line 111-127] SetupBaseType → Creates TreasureItem
    │
    ├─[Line 128-169] prefabInstance path (SKIPPED - testPrefab=null)
    │
    ├─[Line 183] baseType.ComponentInitialize(component, this)
    │   │
    │   └─ Item.ComponentInitialize() [Lines 723-737]
    │       │
    │       ├─ runtimeGroundVisuals = Instantiate(GroundVisualsInfo.GameObject)
    │       └─ runtimeEquippedVisuals = Instantiate(EquippedVisualsInfo.GameObject)
    │
    ├─[Line 228] baseType.PrefabInitialize(worldObject)
    │   │
    │   └─ Item.PrefabInitialize() [Lines 740-748]
    │       │
    │       └─ runtimeEquippedVisuals.SetActive(false)  // Only equipped deactivated
    │
    └─ Template stored in PropLibrary.loadedPropMap
```

## 3.2 Preview Pipeline

```
User selects prop in creator UI
    │
    ↓
PropTool.UpdateSelectedAssetId(assetId)
    │
    ↓
PropLibrary.TryGetRuntimePropInfo(assetId) → RuntimePropInfo
    │
    ↓
PropBasedTool.SpawnPreview(runtimePropInfo)
    │
    ├─[Line 460] propGhostTransform = Instantiate<EndlessProp>(template)
    │   │
    │   └─ Clone inherits ALL active states from template
    │
    ├─[Line 462] propGhostTransform.gameObject.SetActive(!IsMobile)
    │
    └─[Lines 488-501] PurgeNonRenderMeshesFromGhost()
        │
        └─ Only removes Colliders, does NOT touch active states
```

## 3.3 Placement Pipeline

```
User clicks to place
    │
    ↓
PropTool.ToolReleased()
    │
    ↓
AttemptPlaceProp_ServerRPC(position, rotation, assetId)
    │
    ↓
Instantiate<EndlessProp>(template, position, rotation)
    │
    ↓
Stage.TrackNonNetworkedObject(assetId, instanceId, gameObject)
    │
    ↓
networkSetup becomes true
    │
    ↓
HandleNetStateChanged() fires with state=Ground
    │
    ↓
runtimeGroundVisuals.SetActive(true)  // Now managed by state machine
```

---

# PART 4: THE SOLUTION

## 4.1 The Fix: Use Prefab Asset Reference Directly (Match Game's Pipeline)

**The game's pattern**:
- `VisualsInfo.GameObject` is a `[SerializeField]` referencing a **prefab asset**
- It is NOT an instantiated object in the scene
- `ComponentInitialize` calls `Instantiate()` on this prefab asset reference

**Our previous violations**:
- v10.47.0-v10.48.0: Called `Instantiate()` then `SetActive(false)` → broke preview
- v10.49.0: Called `Instantiate()` then moved to Y=-10000 → worked but hacky

**The correct fix (v10.50.0)**: Use the prefab asset directly, no `Instantiate()`

## 4.2 Code Changes in v10.50.0

### Change 1: CreatePearlBasketVisualPrefab (Plugin.cs ~line 4345-4373)

**Before (v10.48.0):**
```csharp
_pearlBasketVisualPrefab = UnityEngine.Object.Instantiate(sourceVisual);
_pearlBasketVisualPrefab.name = "PearlBasket_Visual";
_pearlBasketVisualPrefab.SetActive(false);  // BUG: Causes inactive clones
UnityEngine.Object.DontDestroyOnLoad(_pearlBasketVisualPrefab);
```

**v10.49.0 (workaround):**
```csharp
_pearlBasketVisualPrefab = UnityEngine.Object.Instantiate(sourceVisual);
_pearlBasketVisualPrefab.name = "PearlBasket_Visual";
_pearlBasketVisualPrefab.transform.position = new Vector3(0, -10000, 0);  // Hacky workaround
UnityEngine.Object.DontDestroyOnLoad(_pearlBasketVisualPrefab);
```

**After (v10.50.0) - CORRECT:**
```csharp
// v10.50.0: Use the loaded prefab asset DIRECTLY - no Instantiate!
// This matches EXACTLY how the game works:
// - Game's VisualsInfo.GameObject points to prefab ASSET (not instantiated)
// - ComponentInitialize calls Instantiate() on it when needed
// - We do the same: just reference the prefab asset from the bundle
GameObject sourceVisual = _loadedPrefab ?? _visualPrefab;

// Use prefab asset directly - NO Instantiate, NO SetActive, NO position offset
_pearlBasketVisualPrefab = sourceVisual;

// Remove SimpleYawSpin from the prefab asset if it exists
// (AssetBundle-loaded prefabs CAN be modified in memory)
if (_simpleYawSpinType != null)
{
    var existingSpinner = _pearlBasketVisualPrefab.GetComponent(_simpleYawSpinType);
    if (existingSpinner != null)
    {
        UnityEngine.Object.DestroyImmediate(existingSpinner);
    }
}
```

### Change 2: CreateFallbackVisualPrefab (Plugin.cs ~line 4382-4395)

The fallback still uses `Instantiate()` because we're creating a primitive at runtime (no prefab asset exists). It uses the position offset workaround since there's no alternative.

## 4.3 Why v10.50.0 Works

```
1. _pearlBasketVisualPrefab = sourceVisual (prefab asset reference, NOT instantiated)
   │
2. PREFIX hook injects into tempVisualsInfoGround.GameObject
   │ • Now contains a PREFAB ASSET reference (same as game's serialized fields)
   │
3. ComponentInitialize: runtimeGroundVisuals = Instantiate(GroundVisualsInfo.GameObject)
   │ • Game calls Instantiate() on our prefab asset
   │ • Clone is created fresh, inherits active state from prefab
   │
4. Template has ACTIVE runtimeGroundVisuals
   │
5. SpawnPreview clones template
   │ • Clone has ACTIVE runtimeGroundVisuals
   │
6. Preview is VISIBLE ✓
   │
7. Placed prop: HandleNetStateChanged manages visibility correctly
   │
8. No double visual: testPrefab=null means no prefabInstance created
```

## 4.4 Why This Exactly Matches the Game's Pipeline

| Aspect | Game | v10.50.0 |
|--------|------|----------|
| VisualsInfo.GameObject type | Prefab asset reference | Prefab asset reference |
| When Instantiate() is called | ComponentInitialize | ComponentInitialize |
| Who calls Instantiate() | Game code | Game code |
| SetActive manipulation | None | None |
| Position manipulation | None | None |

---

# PART 5: VISUAL HIERARCHY

## 5.1 Template Hierarchy After BuildPrefab (v10.50.0)

```
EndlessProp (template, stored in loadedPropMap)
├── WorldObject
│   └── EndlessVisuals (manages renderers)
│
└── TreasureItem (baseTypeComponent, implements IBaseType)
    │
    ├── runtimeGroundedVisualsParentGameObject (has SimpleYawSpin)
    │   │
    │   └── runtimeGroundVisuals (ACTIVE - our Pearl Basket visual)
    │       ├── Mesh renderers
    │       └── Materials
    │
    └── runtimeEquippedVisuals (INACTIVE - set by PrefabInitialize)
        └── Same visual, shown when equipped

[NO prefabInstance - testPrefab was null]
```

## 5.2 Why Previous Versions Had Double Visual

```
v10.45.0 - v10.46.0 Hierarchy (BROKEN):

EndlessProp
└── TreasureItem
    │
    ├── prefabInstance (from testPrefab) ← ALWAYS VISIBLE, NOT MANAGED
    │   └── Pearl Basket mesh
    │
    ├── runtimeGroundedVisualsParentGameObject
    │   └── runtimeGroundVisuals ← MANAGED BY HandleNetStateChanged
    │       └── Pearl Basket mesh
    │
    └── runtimeEquippedVisuals
        └── Pearl Basket mesh

TWO Pearl Basket meshes rendered simultaneously!
```

---

# PART 6: KEY CODE REFERENCES

## 6.1 Game Code (Decompiled)

| File | Lines | Purpose |
|------|-------|---------|
| TreasureItem.cs | 47-53 | Serialized VisualsInfo fields |
| Item.cs | 1031-1043 | VisualsInfo struct definition |
| Item.cs | 723-737 | ComponentInitialize - creates visuals |
| Item.cs | 740-748 | PrefabInitialize - deactivates equipped only |
| Item.cs | 346-359 | HandleNetStateChanged - runtime visibility |
| EndlessProp.cs | 109-245 | BuildPrefab - complete flow |
| EndlessProp.cs | 168 | prefabInstance creation (skipped when null) |
| EndlessProp.cs | 183 | Calls baseType.ComponentInitialize |
| PropBasedTool.cs | 449-463 | SpawnPreview - clones template |
| PropBasedTool.cs | 488-501 | PurgeNonRenderMeshesFromGhost |
| PropLibrary.cs | 479 | Normal props: testPrefab=null |
| PropLibrary.cs | 64 | InjectProp can pass testPrefab |

## 6.2 Plugin Code

| File | Lines | Purpose |
|------|-------|---------|
| Plugin.cs | 250-255 | Version banner |
| Plugin.cs | 3573 | InjectProp call with testPrefab=null |
| Plugin.cs | 3909-4118 | Item_ComponentInitialize_Prefix |
| Plugin.cs | 3998-4030 | Inject visual into VisualsInfo fields |
| Plugin.cs | 4334-4386 | CreatePearlBasketVisualPrefab |
| Plugin.cs | 4358-4362 | Keep prefab active, position offset |
| Plugin.cs | 4388-4404 | CreateFallbackVisualPrefab |

---

# PART 7: LESSONS LEARNED

## 7.1 Understanding the Game's Design

The Endstar visual system is designed around these principles:
1. **VisualsInfo.GameObject** is the canonical source for item visuals
2. **Prefab assets** in Unity are active by default
3. **ComponentInitialize** creates runtime visuals by cloning VisualsInfo
4. **Visibility is state-driven** via HandleNetStateChanged
5. **Templates are cloned** for preview and placement

## 7.2 Why Fighting the System Failed

Earlier versions tried to:
- Hide visuals manually (broke preview cloning)
- Use both testPrefab AND VisualsInfo (created duplicates)
- Set VisualsInfo to null (broke visibility system)
- Deactivate prefab (broke Unity's clone behavior)

**The solution was to work WITH the system**, not against it.

## 7.3 The Importance of Data-Driven Research

The breakthrough came from:
1. Reading decompiled game code thoroughly
2. Understanding the COMPLETE pipeline (not just one part)
3. Identifying HOW normal game props work
4. Matching our implementation to the game's pattern

---

# PART 8: VERIFICATION CHECKLIST

## 8.1 Test Cases

- [x] Preview ghost visible in creator mode
- [x] Preview follows cursor correctly
- [x] Placed prop shows single visual (no double)
- [x] Ground visual rotates (SimpleYawSpin on parent)
- [x] Equipped visual is static
- [x] Toss behavior works correctly
- [x] Other game props unaffected
- [x] Props loaded from saved levels work

## 8.2 Log Messages to Verify

```
[INJECTOR] Plugin v10.50.0 - EXACT GAME PIPELINE
[INJECTOR] - Use prefab ASSET directly (no Instantiate)
[INJECTOR] - Matches exactly how game's VisualsInfo.GameObject works
```

---

# APPENDIX A: RESEARCH DOCUMENTS

Additional research is documented in:
- `RESEARCH_visual_system.md` - Deep dive into visual creation
- `PLAN_double_visual_fix.md` - Original fix planning document

---

# APPENDIX B: UNITY INSTANTIATE BEHAVIOR

From Unity Documentation:
> "The active status of a GameObject at the time of cloning is maintained, so if the original is inactive the clone is created in an inactive state too."

This single fact was the key to understanding why SetActive(false) broke the preview system.

---

**Document End**
**Version**: v10.50.0
**Status**: WORKING - Exactly matches game's pipeline
**Date**: January 16, 2026

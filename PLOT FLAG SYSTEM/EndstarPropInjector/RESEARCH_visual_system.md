# Deep Research: Endstar Visual System for Items

## Summary

This document contains data-driven research from decompiled game code explaining how Item visuals work in Endstar, why the "double visual" bug occurred, and why the v10.47.0 fix is correct.

---

## 1. Key Code Locations

| File | Lines | Purpose |
|------|-------|---------|
| `EndlessProp.cs` | 109-245 | `BuildPrefab()` - creates prefabInstance from testPrefab |
| `EndlessProp.cs` | 168 | `prefabInstance = Object.Instantiate(testPrefab, baseTypeComponent.transform)` |
| `EndlessProp.cs` | 183 | `baseType.ComponentInitialize(component, this)` |
| `Item.cs` | 723-737 | `ComponentInitialize()` - creates runtimeGroundVisuals & runtimeEquippedVisuals |
| `Item.cs` | 726 | `runtimeGroundVisuals = Instantiate(GroundVisualsInfo.GameObject, ...)` |
| `Item.cs` | 727 | `runtimeEquippedVisuals = Instantiate(EquippedVisualsInfo.GameObject, ...)` |
| `TreasureItem.cs` | 47-53 | Serialized fields: `tempVisualsInfoGround`, `tempVisualsInfoEqupped` |
| `PropLibrary.cs` | 479 | **CRITICAL**: `await newProp.BuildPrefab(prop, null, null, cancelToken)` |
| `PropLibrary.cs` | 64 | `await newProp.BuildPrefab(prop, testPrefab, testScript, ...)` |
| `PropBasedTool.cs` | 460 | `Object.Instantiate(runtimePropInfo.EndlessProp)` - preview clones template |

---

## 2. Visual Creation Flow

### 2.1 EndlessProp.BuildPrefab (lines 109-245)

```csharp
public async Task BuildPrefab(Prop prop, GameObject testPrefab = null, ...)
{
    // Setup base type (TreasureItem for our prop)
    baseType = this.SetupBaseType(base.transform, baseTypeDefinition);
    baseTypeComponent = baseType as Component;  // This is the Item/TreasureItem

    // === VISUAL PATH 1: testPrefab ===
    if (testPrefab == null)
    {
        if (prop.PrefabAsset != null && !string.IsNullOrEmpty(prop.PrefabAsset.AssetID))
        {
            // Load from asset bundle
            prefabInstance = Object.Instantiate(bundleAsset, baseTypeComponent.transform);
        }
        // ELSE: prefabInstance stays NULL
    }
    else
    {
        // Line 168: Create prefabInstance from testPrefab
        prefabInstance = Object.Instantiate(testPrefab, baseTypeComponent.transform);
    }

    // === VISUAL PATH 2: ComponentInitialize ===
    // Line 183: ALWAYS called
    baseType.ComponentInitialize(component, this);

    // Line 235: Register prefabInstance renderers (if exists)
    if (prefabInstance != null)
    {
        this.endlessVisuals.FindAndManageChildRenderers(prefabInstance);
    }
}
```

### 2.2 Item.ComponentInitialize (lines 723-737)

```csharp
public void ComponentInitialize(ReferenceBase referenceBase, EndlessProp endlessProp)
{
    // Line 726: Create runtimeGroundVisuals from GroundVisualsInfo.GameObject
    this.runtimeGroundVisuals = Object.Instantiate(
        this.GroundVisualsInfo.GameObject,
        this.runtimeGroundedVisualsParentGameObject.transform.position + ...,
        this.runtimeGroundedVisualsParentGameObject.transform
    );

    // Line 727: Create runtimeEquippedVisuals from EquippedVisualsInfo.GameObject
    this.runtimeEquippedVisuals = Object.Instantiate(
        this.EquippedVisualsInfo.GameObject,
        base.transform.position + ...,
        base.transform
    );
}
```

### 2.3 TreasureItem (lines 7-54)

```csharp
public class TreasureItem : Item
{
    // Lines 11-17: GroundVisualsInfo returns tempVisualsInfoGround
    protected override Item.VisualsInfo GroundVisualsInfo
    {
        get { return this.tempVisualsInfoGround; }
    }

    // Lines 20-27: EquippedVisualsInfo returns tempVisualsInfoEqupped
    protected override Item.VisualsInfo EquippedVisualsInfo
    {
        get { return this.tempVisualsInfoEqupped; }
    }

    // Lines 48-53: Serialized fields (set in prefab or at runtime)
    [SerializeField]
    private Item.VisualsInfo tempVisualsInfoGround;

    [SerializeField]
    private Item.VisualsInfo tempVisualsInfoEqupped;
}
```

---

## 3. Normal Props vs Injected Props

### 3.1 Normal Game Props (PropLibrary.SpawnPropPrefab line 479)

```csharp
// Line 479 - Normal props pass testPrefab=null!
await newProp.BuildPrefab(prop, null, null, cancelToken);
```

**Result:**
- `testPrefab == null` → No prefabInstance created
- ComponentInitialize reads VisualsInfo from prefab (baked in Unity editor)
- Creates runtimeGroundVisuals and runtimeEquippedVisuals
- **Single visual source**

### 3.2 Injected Props (PropLibrary.InjectProp line 64)

```csharp
// Line 64 - Injected props CAN pass testPrefab
await newProp.BuildPrefab(prop, testPrefab, testScript, CancellationToken.None);
```

**If testPrefab is provided:**
- Line 168: prefabInstance created from testPrefab
- ComponentInitialize also creates runtimeGroundVisuals from VisualsInfo
- **TWO visual sources = DOUBLE VISUAL BUG**

---

## 4. The Double Visual Bug

### Visual Hierarchy When testPrefab Provided:

```
EndlessProp (root)
├── TreasureItem (baseTypeComponent)
│   ├── prefabInstance (from testPrefab) ← NEVER MANAGED BY VISIBILITY SYSTEM
│   ├── runtimeGroundedVisualsParentGameObject
│   │   └── runtimeGroundVisuals (from GroundVisualsInfo) ← MANAGED
│   └── runtimeEquippedVisuals (from EquippedVisualsInfo) ← MANAGED
```

### Why Double Visual Occurs:

1. `prefabInstance` is always visible (not in visibility system)
2. `runtimeGroundVisuals` is managed by `HandleNetStateChanged()`
3. Both render simultaneously → double visual

### Visibility Management (Item.cs lines 346-359)

```csharp
private void HandleNetStateChanged(...)
{
    // runtimeGroundVisuals visibility managed by state
    runtimeGroundVisuals.SetActive(state == State.Ground || state == State.Tossed || ...);

    // runtimeEquippedVisuals visibility managed by state
    runtimeEquippedVisuals.SetActive(newState.State == State.Equipped);

    // NOTE: prefabInstance is NEVER mentioned - not managed!
}
```

---

## 5. Preview System (PropBasedTool.SpawnPreview)

### How Preview Works (lines 449-463)

```csharp
protected void SpawnPreview(PropLibrary.RuntimePropInfo runtimePropInfo)
{
    // Line 460: Clone the ENTIRE EndlessProp template
    this.propGhostTransform = Object.Instantiate(runtimePropInfo.EndlessProp).GetComponent<Transform>();
}
```

**Key Insight:** Preview clones whatever exists on the template, including:
- prefabInstance (if testPrefab was provided)
- runtimeGroundVisuals (from ComponentInitialize)
- runtimeEquippedVisuals (from ComponentInitialize)

---

## 6. The Correct Fix (v10.47.0)

### Solution: Match Normal Prop Behavior

1. **Pass testPrefab=null** (like normal props at line 479)
2. **Inject visual into GroundVisualsInfo** before ComponentInitialize runs
3. ComponentInitialize creates runtimeGroundVisuals from our injected visual
4. **Single visual source**

### Visual Hierarchy With Fix:

```
EndlessProp (root)
├── TreasureItem (baseTypeComponent)
│   ├── [NO prefabInstance - testPrefab was null]
│   ├── runtimeGroundedVisualsParentGameObject
│   │   └── runtimeGroundVisuals (our visual) ← MANAGED
│   └── runtimeEquippedVisuals (our visual) ← MANAGED
```

### Implementation Details:

**1. InjectProp Call (Plugin.cs line 3573):**
```csharp
// Pass testPrefab=null like normal props
injectPropMethod.Invoke(stageManager, new object[] { prop, null, null, iconSprite });
```

**2. ComponentInitialize PREFIX Hook (Plugin.cs lines 3998-4028):**
```csharp
// Inject our visual into tempVisualsInfoGround BEFORE ComponentInitialize runs
var groundField = AccessTools.Field(_treasureItemType, "tempVisualsInfoGround");
var groundInfo = groundField.GetValue(__instance);
var gameObjectField = visualsInfoType.GetField("GameObject");
gameObjectField.SetValue(groundInfo, _pearlBasketVisualPrefab);
groundField.SetValue(__instance, groundInfo);

// Same for equipped visuals
var equippedField = AccessTools.Field(_treasureItemType, "tempVisualsInfoEqupped");
// ... inject into tempVisualsInfoEqupped ...
```

**3. Preview Works Because:**
- Template has runtimeGroundVisuals (created from our injected visual)
- SpawnPreview clones the template
- Clone has the visual → preview shows correctly

---

## 7. Why Previous Approaches Failed

| Version | Approach | Result | Root Cause |
|---------|----------|--------|------------|
| v10.36.0 | Hide in PREFIX | Preview broken | Modified template before clone |
| v10.37.0 | Hide in POSTFIX | No change | Postfix ran on template, not placed instance |
| v10.38.0 | TrackNonNetworkedObject | Not triggered | Hook didn't fire for our props |
| v10.45.0 | Asset bundle + GroundVisualsInfo | Double visual | Both paths active |
| v10.46.0 | Set GroundVisualsInfo=null | Error visual | Visibility system requires runtimeGroundVisuals |
| v10.47.0 | testPrefab=null + inject VisualsInfo | No double, no preview | Prefab SetActive(false) broke preview |
| v10.48.0 | Same + log fixes | No double, no preview | Same issue - inactive prefab cloned |
| v10.49.0 | Keep prefab ACTIVE + position offset | Working but hacky | Position offset workaround |
| **v10.50.0** | **Use prefab ASSET directly (no Instantiate)** | **WORKING** | **Exactly matches game's pipeline** |

---

## 8. Summary

The correct approach mirrors EXACTLY how normal game props work:

1. **DON'T** pass testPrefab to BuildPrefab (pass null)
2. **DO** inject visual into tempVisualsInfoGround/tempVisualsInfoEqupped before ComponentInitialize
3. **USE PREFAB ASSET DIRECTLY** - don't call Instantiate() on it
4. Let ComponentInitialize call Instantiate() on the prefab asset (same as game does)
5. ComponentInitialize creates the ONLY visual (runtimeGroundVisuals/runtimeEquippedVisuals)
6. Visibility system manages these visuals correctly
7. Preview clones the template which has ACTIVE visuals

**Key Discovery (v10.50.0)**: The game's `VisualsInfo.GameObject` is a **prefab asset reference**, not an
instantiated object. The game's ComponentInitialize is the one that calls `Instantiate()`. By using
the prefab asset directly (without our own Instantiate), we match the game's pipeline exactly.

This is data-driven: the decompiled code shows `tempVisualsInfoGround` is a `[SerializeField]` that
stores a prefab asset reference set in Unity Editor.

---

## 9. Final Working Solution (v10.50.0)

**See**: `DOCUMENTATION_visual_system_fix.md` for complete implementation details.

**Key code change:**
```csharp
// v10.50.0: Use prefab asset directly - NO Instantiate
_pearlBasketVisualPrefab = sourceVisual;  // Just reference the prefab asset from the bundle
```

# Plan: Fix Double Visual Issue (testPrefab + GroundVisualsInfo merged)

## Problem Statement
When the Pearl Basket prop is placed in the world, TWO visuals render simultaneously:
1. **testPrefab visual** - The static prefab passed to BuildPrefab (under baseTypeComponent.transform)
2. **runtimeGroundVisuals** - The rotating visual from GroundVisualsInfo (under runtimeGroundedVisualsParentGameObject.transform)

This causes a "merged" appearance where both models overlap.

## Root Cause Analysis

### Why This Happens
1. **testPrefab is NEVER disabled** - The game code never hides/disables the testPrefab visual
2. **Normal game props don't have this issue** - They use asset bundles where testPrefab and GroundVisualsInfo.GameObject are the SAME prefab reference
3. **Our custom prop has separate prefabs** - testPrefab (for preview) and GroundVisualsInfo (for runtime) are different objects

### Visual Hierarchy (from decompiled code)
```
EndlessProp (root)
├── baseTypeComponent (Item)
│   └── [testPrefab instance] ← ALWAYS VISIBLE (problem!)
│   └── runtimeGroundedVisualsParentGameObject
│       └── [runtimeGroundVisuals] ← Visible when grounded
└── [runtimeEquippedVisuals] ← Visible when equipped
```

### Key Code Locations
- `EndlessProp.BuildPrefab` (line 168): `Object.Instantiate<GameObject>(testPrefab, baseTypeComponent.transform)`
- `Item.ComponentInitialize` (lines 723-737): Creates runtimeGroundVisuals and runtimeEquippedVisuals
- `PropBasedTool.SpawnPreview` (line 449): Clones entire EndlessProp for preview ghost

## Why Previous Fixes Failed

| Version | Approach | Result | Why It Failed |
|---------|----------|--------|---------------|
| v10.36.0 | Hide in PREFIX hook | Preview gone | Template modified → preview clones hidden renderers |
| v10.37.0 | Hide in POSTFIX hook | Nothing changed | POSTFIX runs on template creation, not placement |
| v10.38.0 | TrackNonNetworkedObject | Untested | Correct hook but may need refinement |

## Solution Options

### Option A: TrackNonNetworkedObject Postfix (Recommended)
**Hook**: `Stage.TrackNonNetworkedObject`
**When called**: Only when props are PLACED or LOADED in world (not template, not preview)

```csharp
[HarmonyPostfix]
[HarmonyPatch(typeof(Stage), "TrackNonNetworkedObject")]
public static void TrackNonNetworkedObject_Postfix(object assetId, object instanceId, GameObject newObject)
{
    // Check if this is our custom prop
    var endlessProp = newObject?.GetComponent<EndlessProp>();
    if (endlessProp == null) return;

    // Check prop name matches our injected prop
    if (!IsOurCustomProp(endlessProp)) return;

    // Find and disable testPrefab visual (child of baseTypeComponent, NOT runtimeGroundVisuals)
    var item = endlessProp.GetComponent<Item>();
    if (item == null) return;

    // Get runtimeGroundVisuals via reflection to know what to KEEP
    var runtimeGroundVisuals = GetRuntimeGroundVisuals(item);

    // Disable all renderers under item.transform EXCEPT runtimeGroundVisuals children
    DisableNonRuntimeRenderers(item.transform, runtimeGroundVisuals);
}
```

**Pros**:
- Runs ONLY on placed props
- Preview unaffected (preview is NOT tracked)
- Clean separation of template vs placed instance

**Cons**:
- Need to correctly identify our prop vs others
- Need reflection to access private fields

### Option B: IPropPlacedSubscriber Component
**Interface**: `IPropPlacedSubscriber.PropPlaced()`
**When called**: Only on placed/copied props (not template, not preview)

```csharp
// Add this component to our prop during InjectProp
public class CustomPropVisualFixer : MonoBehaviour, IPropPlacedSubscriber
{
    public void PropPlaced()
    {
        // Hide testPrefab visual here
        // This only runs on placed instances
    }
}
```

**Pros**:
- Self-contained - each prop handles itself
- Uses game's intended extension point

**Cons**:
- Requires adding a component to the prefab
- More complex setup during InjectProp

### Option C: Delay Hiding with Coroutine
Start a coroutine in ComponentInitialize postfix that waits one frame, then checks if this is a placed prop (not preview) and hides.

**Cons**: Fragile, timing-dependent, not recommended.

## Recommended Approach: Option A with Refinements

The v10.38.0 approach is correct but needs refinement:

1. **Identify our prop correctly**: Check prop name or add a marker component during InjectProp
2. **Find testPrefab visual precisely**:
   - Get `baseTypeComponent` (Item) transform
   - Get `runtimeGroundVisuals` via reflection
   - Disable renderers that are children of Item but NOT part of runtimeGroundVisuals
3. **Preserve runtimeGroundVisuals**: The rotating visual should remain visible

## Implementation Steps

1. In `InjectProp`, add a marker component to identify our custom props:
   ```csharp
   prefabRoot.AddComponent<CustomPropMarker>();
   ```

2. In `TrackNonNetworkedObject_Postfix`:
   ```csharp
   // Check for marker
   if (newObject.GetComponent<CustomPropMarker>() == null) return;

   // Get Item component
   var item = newObject.GetComponent<Item>();

   // Get runtimeGroundVisuals transform via reflection
   var runtimeGV = GetFieldValue<GameObject>(item, "runtimeGroundVisuals");
   Transform keepVisibleTransform = runtimeGV?.transform;

   // Iterate item's children, disable renderers NOT under runtimeGroundVisuals
   foreach (Transform child in item.transform)
   {
       if (child == keepVisibleTransform) continue;
       if (IsAncestorOf(keepVisibleTransform, child)) continue;

       foreach (var renderer in child.GetComponentsInChildren<Renderer>())
       {
           renderer.enabled = false;
       }
   }
   ```

## Testing Checklist
- [ ] Preview ghost shows correctly in creator mode
- [ ] Placed prop shows ONLY the rotating visual (not double)
- [ ] Equipped prop shows correctly
- [ ] Other props in the game are unaffected
- [ ] Props loaded from saved levels work correctly

# Visual Architecture Analysis - Data-Driven Solution

## The Root Problem

Our approach was fundamentally wrong. Here's the actual flow:

```
InjectProp(prop, testPrefab, ...)
    │
    └─ BuildPrefab(prop, testPrefab, ...)
        │
        ├─ SetupBaseType(baseTypeDefinition)  ← Creates NEW TreasureItem from base type prefab
        │   └─ TreasureItem has serialized VisualsInfo pointing to ORIGINAL visual prefabs
        │
        ├─ Instantiate(testPrefab) → prefabInstance (child of TreasureItem)
        │
        └─ ComponentInitialize(referenceBase, endlessProp)
            │
            ├─ Reads this.GroundVisualsInfo.GameObject ← ORIGINAL prefab reference!
            │
            └─ Instantiate(originalPrefab) → runtimeGroundVisuals
```

**Key Insight:** The TreasureItem is created fresh from the base type definition, NOT from our modified `_customPrefab`. Our modifications are never used!

## Why Our Fixes Failed

| What We Did | Why It Failed |
|-------------|---------------|
| Modified `_customPrefab.tempVisualsInfoGround` | BuildPrefab creates a NEW TreasureItem, ignores our clone |
| Passed visual as `testPrefab` | Creates prefabInstance (not state-controlled) + runtimeGroundVisuals (original) = double visuals |
| Passed empty `testPrefab` | Still creates runtimeGroundVisuals from ORIGINAL visual prefab |
| Set VisualsInfo.GameObject | On wrong object - our clone, not the actual TreasureItem used |

## The Actual Visual Sources

### Source 1: prefabInstance (from testPrefab/asset bundle)
- Created in `BuildPrefab` line 168
- Is a child of TreasureItem's transform
- **NOT state-controlled** - stays visible regardless of item state
- Purpose: Holds ComponentReferences for Lua scripts

### Source 2: runtimeGroundVisuals (from VisualsInfo)
- Created in `ComponentInitialize` line 726
- Child of `runtimeGroundedVisualsParentGameObject`
- **STATE-CONTROLLED** - SetActive based on Item.State
- Purpose: The actual visible ground prop

### Source 3: runtimeEquippedVisuals (from VisualsInfo)
- Created in `ComponentInitialize` line 727
- Child of Item's transform
- **STATE-CONTROLLED** - shown only when equipped
- Purpose: The item appearance when held

## The Correct Solution

**Hook `Item.ComponentInitialize` with a PREFIX:**

```csharp
[HarmonyPrefix]
[HarmonyPatch(typeof(Item), "ComponentInitialize")]
static void ComponentInitialize_Prefix(Item __instance, EndlessProp endlessProp)
{
    // Check if this is our custom prop
    if (endlessProp.Prop.AssetID == CustomPropGuid)
    {
        // Modify the TreasureItem's VisualsInfo to use our visual
        var groundField = AccessTools.Field(typeof(TreasureItem), "tempVisualsInfoGround");
        var equippedField = AccessTools.Field(typeof(TreasureItem), "tempVisualsInfoEqupped");

        // Get current struct (copy)
        var groundInfo = (Item.VisualsInfo)groundField.GetValue(__instance);
        var equippedInfo = (Item.VisualsInfo)equippedField.GetValue(__instance);

        // Replace GameObject references with our Pearl Basket
        groundInfo.GameObject = _pearlBasketVisualPrefab;  // Has SimpleYawSpin
        equippedInfo.GameObject = _pearlBasketVisualPrefab;

        // Write back the modified structs
        groundField.SetValue(__instance, groundInfo);
        equippedField.SetValue(__instance, equippedInfo);
    }
    // Original method then uses our modified values
}
```

## Why This Works

1. Hook runs BEFORE ComponentInitialize executes
2. We modify the ACTUAL TreasureItem instance that will be used
3. When ComponentInitialize calls `Instantiate(this.GroundVisualsInfo.GameObject, ...)`, it gets our Pearl Basket
4. The instantiated runtimeGroundVisuals is state-controlled properly
5. No double visuals because prefabInstance can be empty (no renderers)

## Required Changes

1. **Remove** all the complex CreateCustomPrefab modifications
2. **Add** Hook on `Item.ComponentInitialize` (prefix)
3. **Keep** the Pearl Basket visual prefab with SimpleYawSpin
4. **Keep** the custom InventoryUsableDefinition (for icon)
5. **Pass** empty testPrefab to InjectProp (for ComponentReferences compatibility)

## Visual Prefab Requirements

The visual prefab (Pearl Basket) should:
- Have SimpleYawSpin component (180 deg/sec)
- Have renderers for the mesh
- NOT have colliders (those come from TreasureItem's structure)
- Be a proper prefab or inactive scene object

## Animation Fix

For the animation issue:
- Don't CREATE a new InventoryUsableDefinition
- Instead, just modify the SPRITE on the original definition from the TreasureItem
- Or: copy the original definition properly, preserving animationTrigger

## Summary

The fix requires hooking at the RIGHT POINT in the flow - when ComponentInitialize runs on the actual TreasureItem that will be used, not on a clone we created that's never used.

---

## Implementation Status (v10.33.1)

**STATUS: FAILED**

The ComponentInitialize hook approach was implemented in v10.33.0/v10.33.1 but resulted in:
- v10.33.0: Prop disappears when picked up, treasure icon in inventory
- v10.33.1: Prop not visible at all

### Known Issues
1. `Prop` on EndlessProp is a **property**, not a field - requires `AccessTools.Property`
2. `VisualsInfo` is a **struct** - value type semantics may cause issues with reflection
3. Asset bundle loading may fail silently
4. The cloned visual prefab may not be properly set up

### Last Working State
v10.31.4 had working pickup with empty placeholder approach, but showed treasure icon instead of custom icon.

# Collectible System Analysis v10.30.0

This document analyzes why custom collectible props aren't pickupable and proposes solutions.

---

## Current State (v10.30.0)

### What Works
- Custom prefab (Pearl Basket) is cloned from Treasure prefab
- TreasureItem and ItemInteractable components are present
- F key prompt appears (PlayerInteractable layer 11 is correct)
- Custom visuals are added to GroundedVisualsParent

### What Doesn't Work
- Treasure mesh still appears (EndlessVisuals not disabled - namespace bug)
- Item is NOT pickupable (network registration issue)

---

## Bug #1: EndlessVisuals Namespace Mismatch

### Location
`Plugin.cs` line 2686

### Problem
```csharp
// Line 399 (CORRECT - cached at init):
_endlessVisualsType = AccessTools.TypeByName("Endless.Gameplay.Scripting.EndlessVisuals");

// Line 2686 (WRONG - used in CreateCustomPrefab):
var endlessVisualsType = AccessTools.TypeByName("Endless.Gameplay.EndlessVisuals");
// Returns null! Missing ".Scripting" in namespace
```

### Fix
Replace line 2686 with:
```csharp
// Use already-cached type instead of wrong namespace lookup
if (_endlessVisualsType != null)
{
    var endlessVisuals = _customPrefab.GetComponent(_endlessVisualsType) as MonoBehaviour;
    if (endlessVisuals != null)
    {
        endlessVisuals.enabled = false;
        Log.LogInfo("[COLLECT] Disabled EndlessVisuals component (prevents default mesh spawn)");
    }
}
```

---

## Bug #2: Network Registration Failure

### Root Cause Analysis

The `Item` class (from decompiled `Item.cs`) requires network initialization:

#### `networkSetup` Flag
```csharp
// Item.cs line 857
private bool networkSetup;  // Defaults to false

// Item.cs line 555-566 - OnNetworkSpawn()
public override void OnNetworkSpawn()
{
    if (base.IsServer)
    {
        this.networkSetup = true;  // Only set here!
        this.netState.Value = new Item.NetState(this.ItemState, ...);
    }
}
```

#### `HandleNetStateChanged` Blocks Everything
```csharp
// Item.cs line 347-352
private void HandleNetStateChanged(Item.NetState oldState, Item.NetState newState)
{
    if (!this.networkSetup)  // Early return when false!
    {
        return;
    }
    // All visual updates, state changes happen below...
}
```

#### `Item.Pickup` Requires Server
```csharp
// Item.cs line 224-229
public Item Pickup(PlayerReferenceManager player)
{
    if (!base.IsServer)
    {
        return null;  // Always returns null for non-server!
    }
    // ...
}
```

### Why Cloning from Resources Fails

| Step | What Happens |
|------|--------------|
| 1 | `Resources.FindObjectsOfTypeAll` finds Treasure prefab |
| 2 | `Instantiate()` clones it |
| 3 | Clone has `networkSetup = false` (field default) |
| 4 | `OnNetworkSpawn()` is never called (not registered with Netcode) |
| 5 | `netState` NetworkVariable is never initialized properly |
| 6 | `IsServer` returns false |
| 7 | All pickup logic fails |

### Evidence from Logs
```
[COLLECT-RESEARCH]   (base) networkSetup = False
```

---

## Proposed Solutions

### Solution A: Force Network State via Reflection (Recommended First Try)

Manually initialize the network state after cloning:

```csharp
// After cloning and before adding to scene
var treasureItem = _customPrefab.GetComponent(_treasureItemType);
if (treasureItem != null)
{
    // Set networkSetup = true
    var networkSetupField = AccessTools.Field(_itemType, "networkSetup");
    if (networkSetupField != null)
    {
        networkSetupField.SetValue(treasureItem, true);
    }

    // Initialize netState with Ground state
    var netStateField = AccessTools.Field(_itemType, "netState");
    if (netStateField != null)
    {
        // Create NetState struct with State.Ground
        var netStateType = netStateField.FieldType; // NetworkVariable<NetState>
        var netStateValue = netStateField.GetValue(treasureItem);

        // Get the Value property
        var valueProperty = netStateType.GetProperty("Value");

        // Create Item.NetState struct
        var itemNetStateType = AccessTools.Inner(_itemType, "NetState");
        // Constructor: NetState(State state, Vector3 pos)
        var ctor = itemNetStateType.GetConstructor(new Type[] {
            AccessTools.Inner(_itemType, "State"),
            typeof(Vector3)
        });

        if (ctor != null)
        {
            var stateEnum = AccessTools.Inner(_itemType, "State");
            var groundValue = Enum.Parse(stateEnum, "Ground");
            var newNetState = ctor.Invoke(new object[] { groundValue, Vector3.zero });
            valueProperty.SetValue(netStateValue, newNetState);
        }
    }
}
```

**Risk:** May cause issues with Netcode sync in multiplayer.

### Solution B: Clone from Scene Instance (Timing Fix)

Wait until after scene loads, then clone from a real networked Treasure:

```csharp
// Instead of cloning in StageManager.Awake, use a coroutine
private static IEnumerator DelayedCloneFromSceneInstance()
{
    // Wait for scene items to spawn
    yield return new WaitForSeconds(1f);

    // Find a SCENE Treasure (not Resources)
    var treasures = UnityEngine.Object.FindObjectsOfType(_treasureItemType);
    foreach (var treasure in treasures)
    {
        var mb = treasure as MonoBehaviour;
        if (mb != null && mb.name.Contains("Treasure(Clone)"))
        {
            // This is a scene-spawned, properly networked instance
            _customPrefab = UnityEngine.Object.Instantiate(mb.gameObject);
            // ...
            break;
        }
    }
}
```

**Risk:** Timing-dependent, may not work if no Treasures exist in scene.

### Solution C: Hook the Pickup Flow (Custom Interaction)

Override `ItemInteractable.AttemptInteract_ServerLogic` for our custom item:

```csharp
public static bool ItemInteractable_AttemptInteract_Prefix(
    object __instance,
    object interactor,
    ref bool __result)
{
    // Check if this is our custom item
    var itemField = AccessTools.Field(_itemInteractableType, "item");
    var item = itemField?.GetValue(__instance) as MonoBehaviour;

    if (item != null)
    {
        var assetIdField = AccessTools.Field(_itemType, "assetID");
        var assetId = assetIdField?.GetValue(item)?.ToString();

        if (assetId == CustomPropGuid)
        {
            // This is our custom item - handle pickup specially
            // Find player inventory and add item directly
            // Return true to indicate successful pickup
            __result = HandleCustomItemPickup(interactor, item);
            return false; // Skip original method
        }
    }

    return true; // Run original for normal items
}

private static bool HandleCustomItemPickup(object interactor, MonoBehaviour item)
{
    // Get PlayerReferenceManager from interactor
    // Directly add to inventory using reflection
    // Destroy the visual object
    // Return success
}
```

**Risk:** Bypasses normal pickup flow, may not sync properly in multiplayer.

### Solution D: Replace Existing Treasure Visuals

Instead of creating a new collectible, find an existing Treasure and swap its visuals:

```csharp
// When a Treasure spawns, check if it should be our custom item
public static void TreasureSpawnPostfix(object __instance)
{
    var mb = __instance as MonoBehaviour;
    if (ShouldBeCustomProp(mb.transform.position))
    {
        // Replace visuals
        ReplaceVisualsWithCustomProp(mb);
    }
}
```

**Risk:** Requires tracking which treasures should be custom props.

---

## Recommended Solution: Proper Network Registration

After analyzing the game's source code, the **proper approach** is to:

### Step 1: Register with NetworkManager

The game uses `NetworkManager.Singleton.AddNetworkPrefab()` in `EndlessProp.BuildPrefab()` (line 239).

After creating our custom prefab:
```csharp
NetworkManager.Singleton.AddNetworkPrefab(_customPrefab);
Log.LogInfo("[COLLECT] Registered custom prefab with NetworkManager");
```

### Step 2: Spawn NetworkObject When Placed

When the prop is placed in the scene (in `StageManager.Awake` or during level load):
```csharp
var instance = UnityEngine.Object.Instantiate(_customPrefab, position, rotation);
var networkObject = instance.GetComponent<NetworkObject>();
if (networkObject != null)
{
    networkObject.Spawn(false);  // false = don't destroy with scene
    Log.LogInfo("[COLLECT] Spawned NetworkObject for custom collectible");
}
```

### Implementation in Plugin.cs

Modify `CreateCustomPrefab()`:

```csharp
// After cloning and setting up the prefab...

// Register with NetworkManager (must have valid NetworkObject component)
var networkObject = _customPrefab.GetComponent<NetworkObject>();
if (networkObject != null && NetworkManager.Singleton != null)
{
    NetworkManager.Singleton.AddNetworkPrefab(_customPrefab);
    Log.LogInfo("[COLLECT] Registered prefab with NetworkManager");
}
```

### Why This Works

From `Item.cs`:
```csharp
// Line 555-566 - OnNetworkSpawn
public override void OnNetworkSpawn()
{
    if (base.IsServer)
    {
        this.networkSetup = true;  // This gets set!
        this.netState.Value = new Item.NetState(this.ItemState, ...);
    }
}
```

When `NetworkObject.Spawn()` is called, Unity Netcode:
1. Registers the object with the network
2. Calls `OnNetworkSpawn()` on all NetworkBehaviours
3. Sets `networkSetup = true`
4. Initializes `netState` with proper value
5. `IsServer` returns true (in single player, we ARE the server)

### Key Insight

The game is designed for multiplayer, where a server hosts and clients connect. In single player:
- The player is BOTH server and client
- `IsServer` returns true
- All network logic works correctly

The issue was that our cloned prefab **never went through the spawn process**.

## Recommended Next Steps

1. **Fix Bug #1 first** (namespace mismatch) - Easy, no risk
2. **Add NetworkManager registration** after creating prefab
3. **Call NetworkObject.Spawn()** when placing the prop instance
4. Test in game

---

## Files Referenced

| File | Purpose |
|------|---------|
| `Plugin.cs:399` | Correct `_endlessVisualsType` cached |
| `Plugin.cs:2686` | Wrong namespace lookup (BUG) |
| `Item.cs:555-566` | `OnNetworkSpawn` - sets `networkSetup` |
| `Item.cs:347-352` | `HandleNetStateChanged` - blocks on `networkSetup` |
| `Item.cs:224-229` | `Item.Pickup` - requires `IsServer` |
| `ItemInteractable.cs:10-14` | Pickup flow entry point |

---

## Version History

| Version | Status |
|---------|--------|
| v10.28.0 | Initial clone approach - treasure mesh visible |
| v10.29.0 | Added hierarchy logging |
| v10.30.0 | Attempted to disable EndlessVisuals (wrong namespace) |
| v10.31.0 | Fixed namespace + tried network state forcing |
| v10.31.4 | Empty placeholder for VisualsInfo - WORKING pickup |
| v10.32.0 | Added rotation (wrong object) + custom icon |
| v10.32.1 | Fixed rotation via GroundVisualsInfo - double visual bug |
| v10.32.2 | Fixed double visual + animation break |

---

## v10.32.x Discovery: Visual System Architecture

### Two Visual Sources (Critical Discovery)

The game creates visuals from TWO separate sources:

1. **`BuildPrefab` testPrefab** (`EndlessProp.cs:168`)
   - Creates `prefabInstance` from the visual prefab passed to `InjectProp`
   - This stays active and visible regardless of item state

2. **`Item.ComponentInitialize` VisualsInfo** (`Item.cs:726-727`)
   - Creates `runtimeGroundVisuals` from `GroundVisualsInfo.GameObject`
   - Creates `runtimeEquippedVisuals` from `EquippedVisualsInfo.GameObject`
   - `runtimeGroundVisuals` gets `SetActive(false)` on pickup (proper behavior)

### Rotation Behavior (SimpleYawSpin)

| Location | Behavior |
|----------|----------|
| On `prefabInstance` | Rotates forever (no state handling) |
| On `runtimeGroundVisuals` | Stops when item picked up (correct) |

**Solution:** Put SimpleYawSpin on `GroundVisualsInfo.GameObject`, pass empty placeholder to `InjectProp`.

### Animation Trigger Requirement

The `InventoryUsableDefinition` has an `animationTrigger` field (line 97) that controls character movement animations after pickup.

**Problem:** Creating a new `GenericUsableDefinition` from scratch doesn't set `animationTrigger`, breaking character movement.

**Solution:** Clone all fields from the original treasure definition, then override only:
- `guid` (our custom GUID)
- `displayName` ("Pearl Basket")
- `sprite` (our custom icon)

### v10.32.2 Final Architecture

```
InjectProp Parameters:
├── prop: EndlessProp with custom data
├── testPrefab: _emptyVisualPlaceholder (creates invisible prefabInstance)
├── runtimeDatabase: null
└── iconSprite: _loadedIcon

Visual Flow:
├── prefabInstance: Empty (no visible mesh)
└── runtimeGroundVisuals: Pearl Basket + SimpleYawSpin (from GroundVisualsInfo)
    └── SetActive(false) on pickup → rotation stops

Definition Flow:
├── Clone original treasure definition fields (preserves animationTrigger)
├── Override: guid, displayName, sprite
└── Character animations work correctly
```

---

## Key Code Locations (v10.32.2)

| Method | Lines | Purpose |
|--------|-------|---------|
| `CreateGroundVisualWithRotation()` | 3007-3077 | Creates visual prefab with SimpleYawSpin |
| `CreateCustomInventoryDefinition()` | 3082-3199 | Clones definition, sets icon |
| `CopyDefinitionFields()` | 3205-3241 | Copies all fields via reflection |
| `InjectProp call` | 3506 | Passes `_emptyVisualPlaceholder` as testPrefab |

---

## v10.33.0: Correct Architecture (ComponentInitialize Hook)

### Root Cause of Previous Failures

The previous approaches failed because:
1. `BuildPrefab` creates a **NEW TreasureItem** from the base type definition
2. Our modified `_customPrefab` clone was **never used** by the game
3. The VisualsInfo modifications were on the wrong object

### Correct Solution: Hook ComponentInitialize

```
InjectProp(prop, emptyPlaceholder, ...)
    │
    └─ BuildPrefab(prop, emptyPlaceholder, ...)
        │
        ├─ SetupBaseType() → Creates FRESH TreasureItem from base type
        │
        ├─ Instantiate(emptyPlaceholder) → Empty prefabInstance (no visuals)
        │
        └─ ComponentInitialize(referenceBase, endlessProp)
            │
            ├─ [HOOK PREFIX] Item_ComponentInitialize_Prefix
            │   ├─ Check if endlessProp.Prop.AssetID == our GUID
            │   ├─ Modify THIS TreasureItem's tempVisualsInfoGround.GameObject
            │   ├─ Modify THIS TreasureItem's tempVisualsInfoEqupped.GameObject
            │   └─ Modify original definition's sprite (preserves animationTrigger)
            │
            └─ [ORIGINAL] Instantiate(modified VisualsInfo) → Our Pearl Basket!
```

### Key Code (v10.33.0)

| Method | Purpose |
|--------|---------|
| `PatchItemComponentInitialize()` | Sets up the Harmony prefix hook |
| `Item_ComponentInitialize_Prefix()` | Injects our visual at the right moment |
| `CreatePearlBasketVisualPrefab()` | Creates the visual with SimpleYawSpin |

### Why This Works (Theory)

1. We hook the **ACTUAL** TreasureItem being initialized (not a clone)
2. We modify its VisualsInfo **BEFORE** the original method instantiates visuals
3. The original method then uses our modified values
4. We modify the **ORIGINAL** definition (preserves animationTrigger)
5. No double visuals because testPrefab is empty

---

## v10.33.1: Failed - Prop Not Visible

### Changes Made
1. Fixed `Prop` access: Changed `AccessTools.Field` to `AccessTools.Property` (Prop is a property on EndlessProp)
2. Fixed method name: `CopyAllDefinitionFields` → `CopyDefinitionFields`
3. Clone definition instead of modifying shared one

### Result
**FAILED** - Prop is not visible at all in the scene.

### Possible Causes (Not Investigated)
1. `_pearlBasketVisualPrefab` may not be created correctly
2. The VisualsInfo struct modification may not be working (structs are value types)
3. The hook may not be firing at all
4. The visual prefab may be inactive or have no renderers
5. Asset bundle loading may have failed silently

### What Was Working Before (v10.31.4)
- Empty placeholder approach with VisualsInfo.GameObject set to empty placeholder
- Pickup was functional
- Custom definition with icon was partially working

---

## Summary of All Approaches Tried

| Version | Approach | Result |
|---------|----------|--------|
| v10.28.0 | Clone from Resources, modify clone | Double visuals (treasure + custom) |
| v10.29.0 | Added hierarchy logging | Same issue |
| v10.30.0 | Disable EndlessVisuals (wrong namespace) | Same issue |
| v10.31.0 | Fixed namespace + network state forcing | Pickup still broken |
| v10.31.4 | Empty placeholder for VisualsInfo | **WORKING pickup**, treasure icon |
| v10.32.0 | Added rotation + custom icon | Double visual bug |
| v10.32.1 | Fixed rotation via GroundVisualsInfo | Double visual bug |
| v10.32.2 | Fixed double visual + animation | Animation break |
| v10.33.0 | Hook ComponentInitialize prefix | Prop disappears on pickup, treasure icon |
| v10.33.1 | Fixed Prop property access + clone definition | **Prop not visible at all** |

---

## Recommended Next Steps

1. **Revert to v10.31.4** - Last known working state for pickup
2. **Add comprehensive logging** to understand asset bundle loading and visual creation
3. **Verify hook is firing** - Check BepInEx logs for `[VISUAL-INJECT]` entries
4. **Debug step by step** - Don't change multiple things at once

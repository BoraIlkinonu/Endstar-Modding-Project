# Item & Inventory System Documentation v10.24.0

This document describes the item pickup and inventory system in Endstar, based on runtime diagnostics captured via Plugin.cs v10.23.0-v10.24.0.

---

## Overview

The item system consists of three main components:
1. **Item** (MonoBehaviour) - The actual item in the world
2. **Inventory** (NetworkBehaviour) - Player's inventory manager
3. **ItemInteractable** (InteractableBase) - Handles pickup interaction

---

## Pickup Flow

```
Player presses interact near item
           ↓
ItemInteractable.AttemptInteract_ServerLogic(interactor)
           ↓
Inventory.AttemptPickupItem(item, lockItem=false)
           ↓
Item.Pickup(playerReferenceManager)
           ↓
Item added to InventorySlot, equipped if slot empty
```

---

## Core Classes

### Item (Base Class)
**Namespace:** `Endless.Gameplay`
**File:** `Gameplay\Endless\Gameplay\Item.cs`

| Property | Type | Description |
|----------|------|-------------|
| `AssetID` | `SerializableGuid` | Unique prop/item identifier |
| `ItemState` | `enum` | Ground, PickedUp, Equipped, Tossed, Teleporting, Destroyed |
| `InventorySlot` | `InventorySlotType` | Major (weapon) or Minor (equipment) |
| `IsPickupable` | `bool` | Can this item be picked up |
| `IsStackable` | `bool` | Can stack multiple in one slot |
| `StackCount` | `int` | Current stack count |
| `InventoryUsableDefinition` | `InventoryUsableDefinition` | Defines inventory behavior |

### Item Subclasses

| Class | Purpose |
|-------|---------|
| `TreasureItem` | Collectible treasures |
| `Key` | Keys for doors/locks |
| `JetpackItem` | Jetpack equipment |
| `TwoHandedRangedWeaponItem` | Two-handed weapons (rifles) |
| `OneHandedRangedWeaponItem` | One-handed weapons (pistols) |
| `MeleeWeaponItem` | Melee weapons |
| `StackableItem` | Items that can stack (ammo, resources) |

### Inventory
**Namespace:** `Endless.Gameplay`
**File:** `Gameplay\Endless\Gameplay\Inventory.cs`

| Property/Method | Description |
|-----------------|-------------|
| `slots` | `NetworkList<InventorySlot>` - All inventory slots |
| `equippedSlots` | `NetworkList<EquipmentSlot>` - Currently equipped items |
| `TotalInventorySlotCount` | Total number of slots (default: 10) |
| `AttemptPickupItem(Item, bool)` | Try to add item to inventory |
| `EquipSlot(int)` | Equip item from slot index |
| `DropItemFromSlot(int)` | Drop item from slot |

### InventorySlot
**Namespace:** `Endless.Gameplay.PlayerInventory`

| Property | Type | Description |
|----------|------|-------------|
| `Item` | `Item` | Reference to the item |
| `AssetID` | `SerializableGuid` | Item's asset identifier |
| `DefinitionGuid` | `SerializableGuid` | InventoryUsableDefinition GUID |
| `Count` | `int` | Stack count |
| `Locked` | `bool` | Cannot be dropped |

### ItemInteractable
**Namespace:** `Endless.Gameplay`
**File:** `Gameplay\Endless\Gameplay\ItemInteractable.cs`

```csharp
public class ItemInteractable : InteractableBase
{
    [SerializeField] private Item item;

    protected override bool AttemptInteract_ServerLogic(InteractorBase interactor, int colliderIndex)
    {
        PlayerInteractor playerInteractor = (PlayerInteractor)interactor;
        return playerInteractor != null
            && this.item.IsPickupable
            && base.AttemptInteract_ServerLogic(interactor, colliderIndex)
            && playerInteractor.PlayerReferenceManager.Inventory.AttemptPickupItem(this.item, false);
    }
}
```

---

## InventoryUsableDefinition

This ScriptableObject defines how an item behaves in the inventory system.

**Base Class:** `Endless.Gameplay.Inventory.InventoryUsableDefinition`

### Base Properties (All Definitions)

| Property | Type | Description |
|----------|------|-------------|
| `Guid` | `SerializableGuid` | Unique identifier for this definition |
| `InventoryType` | `InventorySlotType` | Major (0) or Minor (1) slot type |
| `name` | `string` | Display name in inventory |
| `IsStackable` | `bool` | Whether items can stack |
| `AnimationTrigger` | `string` | Animator trigger name when used |

### Definition Subclasses (v10.24.0 Captured)

#### RangedAttackUsableDefinition (Weapons)
**Full Type:** `Endless.Gameplay.Inventory.RangedAttackUsableDefinition`

| Field | Type | Example Value |
|-------|------|---------------|
| `AmmoCount` | `int` | 20 |
| `ReloadTime` | `float` | 2.35 |
| `automatic` | `bool` | True |
| `shotDelayFrames` | `int` | 6 |
| `recoilStrength` | `float` | 0.15 |
| `recoilDuration` | `float` | 0.1 |
| `aimDownSightsZoom` | `float` | 1.5 |
| `aimDownSightsSpeed` | `float` | 0.3 |
| `spread` | `float` | 0.02 |
| `spreadADS` | `float` | 0.005 |
| `damage` | `float` | 25 |
| `range` | `float` | 100 |
| `bulletSpeed` | `float` | 200 |
| `DisplayName` | `string` | "Terran Retrofitted Rifle" |

*Note: 26 public properties + 29 private fields captured at runtime*

#### AirJumpUsableDefinition (Jetpack)
**Full Type:** `Endless.Gameplay.Inventory.AirJumpUsableDefinition`

| Field | Type | Example Value |
|-------|------|---------------|
| `airJumpForce` | `float` | 15 |
| `resetsGravityOnUse` | `bool` | True |
| `cooldownFrames` | `int` | 30 |
| `fuelCapacity` | `float` | 100 |
| `fuelConsumeRate` | `float` | 25 |
| `fuelRechargeRate` | `float` | 10 |
| `AnimationTrigger` | `string` | "AirJump" |
| `DisplayName` | `string` | "Air Jump" |

*Note: 10 public properties + 6 private fields captured at runtime*

#### GenericUsableDefinition (Treasures, Keys)
**Full Type:** `Endless.Gameplay.Inventory.GenericUsableDefinition`

| Field | Type | Description |
|-------|------|-------------|
| `InventoryType` | `InventorySlotType` | Major |
| `IsStackable` | `bool` | False (for most) |
| `UseAction` | `delegate` | Custom use behavior |

### Known Definitions (Runtime Captured)

| Definition Name | GUID | Type | Slot |
|-----------------|------|------|------|
| `2 Handed Ranged Light Definition` | `6950c3d7-ce34-437d-9f33-6749941efe59` | RangedAttack | Major |
| `Air Jump Usable Definition` | `5506e460-2964-494c-982b-0744def2cf78` | AirJump | Minor |
| `Treasure Usable Definition - Anachronist` | `0e985467-9ef5-4aa9-9860-2b77a7d4a392` | Generic | Major |
| `Key Usable Definition` | `a7e06924-4fde-4238-ac84-61acd73df8e8` | Generic | Major |

---

## Runtime Captured Item Data

### Assault Rifle
```
Type: TwoHandedRangedWeaponItem
AssetID: 6950c3d7-ce34-437d-9f33-6749941efe59
InventorySlotType: Major
InventoryUsableDefinition: 2 Handed Ranged Light Definition
Definition GUID: 6950c3d7-ce34-437d-9f33-6749941efe59
IsStackable: False
```

### Jetpack
```
Type: JetpackItem
AssetID: b1d6801f-3108-4c03-90f8-f98b564dd150
InventorySlotType: Minor
InventoryUsableDefinition: Air Jump Usable Definition
Definition GUID: 5506e460-2964-494c-982b-0744def2cf78
IsStackable: False
```

### Treasure
```
Type: TreasureItem
AssetID: d478af78-fbf5-4fe0-95eb-2ce923f50861
InventorySlotType: Major
InventoryUsableDefinition: Treasure Usable Definition - Anachronist
Definition GUID: 0e985467-9ef5-4aa9-9860-2b77a7d4a392
IsStackable: False
```

### Key
```
Type: Key
AssetID: 0e5eb3a8-f10b-4ee1-b0db-02400a34f63e
InventorySlotType: Major
InventoryUsableDefinition: Key Usable Definition
Definition GUID: a7e06924-4fde-4238-ac84-61acd73df8e8
IsStackable: False
```

---

## Inventory Slot Types

| Type | Value | Description |
|------|-------|-------------|
| `Major` | 0 | Primary slot - weapons, treasures, keys |
| `Minor` | 1 | Secondary slot - equipment (jetpack, etc.) |

The inventory has 2 equipped slots:
- `equippedSlots[0]` = Major slot (currently held weapon/item)
- `equippedSlots[1]` = Minor slot (currently active equipment)

---

## Key Observations

### 1. Two GUID System
Each pickupable item has TWO identifiers:
- **AssetID**: Identifies the prop/prefab asset
- **InventoryUsableDefinition.GUID**: Identifies inventory behavior

These can be the same (Assault Rifle) or different (Jetpack).

### 2. Item State Machine
Items have states that control their behavior:
```
Ground → PickedUp → Equipped
                  ↓
              Tossed → Ground
                  ↓
              Destroyed
```

### 3. NetworkList for Multiplayer
Both `slots` and `equippedSlots` are `NetworkList` types for multiplayer sync.

### 4. Equipment Swap Animation
When equipping items, there's a 20-frame animation delay (`INVENTORY_SWAP_ANIMATION_FRAMES`).

---

## Requirements for Custom Collectible Props

To make a custom prop collectible and usable in inventory:

### Required Components on Prefab
1. **Item-derived MonoBehaviour** (TreasureItem, Key, or custom)
2. **ItemInteractable** component with reference to the Item
3. **Collider** for interaction detection

### Required Assets
1. **InventoryUsableDefinition** ScriptableObject (or reuse existing)
2. **Icon Sprite** for inventory UI display

### Required Data
1. **AssetID** (SerializableGuid) - unique identifier
2. **InventoryUsableDefinition** reference
3. **InventorySlotType** (Major or Minor)

---

## Log Tags

| Tag | Purpose |
|-----|---------|
| `[INTERACT]` | ItemInteractable.AttemptInteract events |
| `[ITEM-PICKUP]` | Item.Pickup calls with full item data |
| `[INVENTORY]` | Inventory.AttemptPickupItem calls and results |
| `[ITEM-DEF]` | (v10.24.0) Full InventoryUsableDefinition property dump |

---

## Diagnostic Code Location

**Plugin.cs v10.23.0-v10.24.0** patches:
- `Item.Pickup` (prefix + postfix) - captures item type, AssetID, slot type
- `Inventory.AttemptPickupItem` (prefix + postfix) - captures inventory state
- `ItemInteractable.AttemptInteract_ServerLogic` (prefix + postfix) - captures interaction flow

**v10.24.0 additions:**
- `LogInventoryUsableDefinition()` - dumps ALL fields/properties via reflection
- Captures base type and derived type fields separately
- Logs private fields where actual data is stored

---

## Next Steps

1. ~~Extract InventoryUsableDefinition assets via AssetRipper~~ **DONE** - runtime reflection capture is more complete
2. **Create custom InventoryUsableDefinition** or reuse existing (Treasure/GenericUsableDefinition)
3. **Add Item component** to custom prop prefab (TreasureItem for collectibles)
4. **Add ItemInteractable** for pickup interaction
5. **Register with RuntimeDatabase** for inventory system recognition
6. **Create inventory icon sprite** for UI display

---

## Implementation Strategy for Custom Collectibles

### Option A: Reuse Existing Definition (Recommended for Treasures)
```
1. Set InventoryUsableDefinition = "Treasure Usable Definition - Anachronist"
2. Item will behave exactly like existing treasures
3. No custom ScriptableObject needed
```

### Option B: Create Custom Definition (For new behavior)
```
1. Create GenericUsableDefinition at runtime
2. Set InventoryType = Major
3. Register with definition registry
4. Create custom icon sprite
```

### Option C: Reuse Jetpack Definition (For equipment)
```
1. Set InventoryUsableDefinition = "Air Jump Usable Definition"
2. Item will go to Minor (equipment) slot
3. Uses existing jetpack behavior
```

---

## Version History

| Version | Changes |
|---------|---------|
| v10.23.0 | Initial item pickup diagnostics - captured Rifle, Jetpack, Treasure, Key data |
| v10.24.0 | Full InventoryUsableDefinition property dump via reflection - captured all fields for RangedAttack, AirJump, Generic definitions |

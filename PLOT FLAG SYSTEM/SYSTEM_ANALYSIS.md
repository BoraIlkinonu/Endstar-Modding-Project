# Endstar Game Systems Analysis

Research conducted to understand runtime behavior before implementing fixes.

---

## 1. RuntimeDatabase System

### Data Structure
```
Dictionary<SerializableGuid, UsableDefinition> usableDefinitionMap
```
- **Key**: `SerializableGuid` - unique identifier for each definition
- **Value**: `UsableDefinition` - base class (polymorphic, includes InventoryUsableDefinition)

### Population (RuntimeDatabase.Start)
```csharp
usableDefinitions = Resources.LoadAll<UsableDefinition>(resourcePath);
foreach (var def in usableDefinitions)
{
    usableDefinitionMap.Add(def.Guid, def);
}
```
- Loaded from Unity Resources folder at startup
- Each definition has a GUID assigned in the editor

### Lookup Methods
```csharp
// DIRECT INDEXER - throws KeyNotFoundException if missing!
public static UsableDefinition GetUsableDefinition(SerializableGuid guid)
{
    return usableDefinitionMap[guid];  // NO TRY-CATCH
}

public static T GetUsableDefinition<T>(SerializableGuid guid)
{
    return (T)usableDefinitionMap[guid];  // NO TRY-CATCH
}
```

### Critical Finding: NO ERROR HANDLING
- If GUID not in dictionary → `KeyNotFoundException` thrown
- No fallback, no null return, no TryGetValue pattern
- Custom props MUST be registered or equip will crash

---

## 2. Inventory Equip Flow (Number Keys 1-9)

### Complete Call Chain
```
Player presses Key "2"
    ↓
PlayerNetworkController.Update()
    ├─ Check: playerInputActions.Player.Inventory2.triggered
    └─ Call: TryEquipFromInventory(1)  // index = key - 1
        ↓
Inventory.EquipSlot(slotIndex=1)
    ├─ Validate: slots[1].Item exists
    ├─ Get: item.InventoryUsableDefinition.InventoryType (Major=0, Minor=1)
    └─ Call: EquipSlot(1, inventoryType)
        ↓
EquipSlot_ServerRPC(slotIndex=1, equipmentSlot)  [Network RPC]
    ├─ Validate: slots[1].Count > 0
    ├─ Validate: slots[1].DefinitionGuid != Empty
    ├─ LOOKUP: RuntimeDatabase.GetUsableDefinition<InventoryUsableDefinition>(DefinitionGuid)
    │          ↑↑↑ THIS IS WHERE IT CRASHES IF GUID NOT REGISTERED ↑↑↑
    ├─ Validate: definition.InventoryType matches equipmentSlot
    └─ Call: ServerEquipSlot(slotIndex, equipmentSlot)
```

### Key Data Structures

**InventorySlot (struct)**
```csharp
bool Locked;
NetworkObjectReference ItemObjectReference;
// Computed:
Item Item                    // Dereferenced from ItemObjectReference
SerializableGuid DefinitionGuid  // Item.InventoryUsableDefinition.Guid
int Count                    // Item.StackCount
```

**Item.InventoryUsableDefinition**
- Set via `[SerializeField]` on the Item prefab
- NOT dynamically looked up - directly references the definition object
- The Item stores a reference to the definition, and the definition has a GUID

### The Registration Problem
When plugin creates custom props:
1. Item is created with a cloned InventoryUsableDefinition
2. The cloned definition has a NEW GUID
3. If that GUID is not added to `RuntimeDatabase.usableDefinitionMap`...
4. ...then `GetUsableDefinition(guid)` throws `KeyNotFoundException`
5. Number key equip fails

---

## 3. Icon/Sprite Display Pipeline

### Where Icons Are Stored
```csharp
// UsableDefinition.cs (base class)
[SerializeField]
private Sprite sprite;

public Sprite Sprite { get { return this.sprite; } }
```
- Sprite is a serialized field on the ScriptableObject
- Inherited by InventoryUsableDefinition
- Assigned in Unity Editor when creating definitions

### UI Rendering Chain
```
UIInventorySlotView (slot container)
    └─ UIItemView (item display)
        └─ Image iconImage (Unity UI Image component)
```

### How Sprite Gets to UI (UIItemView.View method)
```csharp
public void View(Item model)
{
    // Get definition from RuntimeDatabase via AssetID
    InventoryUsableDefinition def =
        RuntimeDatabase.GetUsableDefinitionFromItemAssetID(model.AssetID);

    // Assign sprite to Image component
    this.iconImage.sprite = def.Sprite;
    this.iconImage.gameObject.SetActive(true);
}
```

### GetUsableDefinitionFromItemAssetID Flow
```csharp
public static InventoryUsableDefinition GetUsableDefinitionFromItemAssetID(SerializableGuid assetID)
{
    // 1. Look up prop info from PropLibrary
    PropLibrary.RuntimePropInfo info;
    if (StageManager.Instance.ActivePropLibrary.TryGetRuntimePropInfo(assetID, out info))
    {
        // 2. Get Item component from the prop prefab
        Item item = info.EndlessProp.GetComponentInChildren<Item>();
        if (item != null)
        {
            // 3. Return the Item's definition (which has the Sprite)
            return item.InventoryUsableDefinition;
        }
    }
    return null;
}
```

### Icon Display Path (different from equip!)
- Uses `AssetID` → `PropLibrary` → `Item.InventoryUsableDefinition.Sprite`
- Does NOT go through `RuntimeDatabase.usableDefinitionMap`
- This is why icons might work even if RuntimeDatabase registration fails

---

## 4. SimpleYawSpin Rotation System

### Core Rotation Code
```csharp
// SimpleYawSpin.cs
private void Update()
{
    base.transform.localRotation *= Quaternion.AngleAxis(
        this.spinDegreesPerSecond * Time.deltaTime,
        Vector3.up
    );
}
```

### Key Facts
- **Rotation Type**: `localRotation` (NOT world rotation)
- **Axis**: `Vector3.up` (Y-axis in LOCAL space)
- **Rate**: 180 degrees/second (configurable)
- **Randomization**: Optional random starting rotation

### Transform Hierarchy for Ground Items
```
Item (NetworkObject - Root)
├── transform.localRotation = identity (set in ground state)
│
└── runtimeGroundedVisualsParentGameObject
    ├── parent = Item.transform
    │
    └── runtimeGroundVisuals (instantiated prefab)
        ├── parent = runtimeGroundedVisualsParentGameObject
        └── SimpleYawSpin component HERE
            └── rotates this.transform.localRotation
```

### Effective World Rotation
Since parent chain has identity rotation:
- Local Y-axis = World Y-axis
- Objects spin around vertical (world Y) axis

### If Rotation Appears Wrong
The issue is NOT SimpleYawSpin code - it's the **prefab's axis orientation**:
- Blender uses Z-up by default
- Unity uses Y-up
- If model exported without axis conversion, local Y != visual up
- The model's "up" in local space may point sideways in world space

### VisualsInfo Struct (Critical for Custom Props)
```csharp
// Item.cs - VisualsInfo struct
protected struct VisualsInfo
{
    public GameObject GameObject;  // The prefab to instantiate
    public Vector3 Position;       // Position offset from parent
    public Vector3 Angles;         // Euler angles applied at instantiation!
}
```

### How Game Instantiates Visuals
```csharp
// Item.ComponentInitialize (line 726)
this.runtimeGroundVisuals = Object.Instantiate<GameObject>(
    this.GroundVisualsInfo.GameObject,                                                    // Prefab
    this.runtimeGroundedVisualsParentGameObject.transform.position + this.GroundVisualsInfo.Position,  // Position
    this.runtimeGroundedVisualsParentGameObject.transform.rotation * Quaternion.Euler(this.GroundVisualsInfo.Angles),  // Rotation!
    this.runtimeGroundedVisualsParentGameObject.transform);                               // Parent
```

### Critical: Angles Field
- **Angles is applied as Euler rotation** when instantiating the prefab
- If template TreasureItem has non-zero Angles for its mesh, those get applied to your prefab
- **Fix**: Set `VisualsInfo.Angles = Vector3.zero` when injecting custom prefab
- Your prefab's hierarchy (wrapper → mesh) should already have correct orientation

---

## 5. ColliderInfo Layer System (Collision Control)

### Layer Types and Purpose
```csharp
// ColliderInfo.cs - ColliderType enum
public enum ColliderType
{
    [ColliderTypeInfoAttribute("Normal everyday collisions. Can be hit by attacks", "Default")]
    Default,

    [ColliderTypeInfoAttribute("Characters can use this collider for interacting. This does NOT cause physical collisions!", "PlayerInteractable")]
    Interactable,

    [ColliderTypeInfoAttribute("Used for hit boxes. Can be hit by attacks, but does not block movement.", "HittableColliders")]
    Hittable,

    [ColliderTypeInfoAttribute("Used for collision that only affects the player.", "PlayerOnly")]
    PlayerOnly,

    [ColliderTypeInfoAttribute("Used for World Trigger overlaps.", "WorldTriggers")]
    WorldTrigger,

    [ColliderTypeInfoAttribute("Normal everyday collisions, but does not affect navigation.", "DefaultNoBake")]
    DefaultNoNavBake
}
```

### How Layers Are Set
```csharp
// ColliderInfo.Awake()
private void Awake()
{
    ColliderTypeInfoAttribute infoAttribute = GetInfoAttribute(this.colliderType);
    this.SetLayerOnObjectAndChildren(base.gameObject, infoAttribute.Layer);
}
```

### Critical Finding: PlayerInteractable Layer
- **"PlayerInteractable" layer does NOT cause physical collisions with Character**
- Used for pickupable items that should be interactable but not push the player
- If custom props cause catapulting: their colliders are on wrong layer (e.g., Default)
- **Fix**: Set colliders to `LayerMask.NameToLayer("PlayerInteractable")` at load time

### Layer Numbers (from game)
| Layer Name | Purpose |
|------------|---------|
| Default | Physical collisions, blocks movement |
| PlayerInteractable | Allows interaction, NO physical collision |
| HittableColliders | Can be hit by attacks, doesn't block movement |
| PlayerOnly | Only collides with player character |
| WorldTriggers | Trigger volumes for world events |
| DefaultNoBake | Physical but excluded from navmesh baking |

---

## 6. Holding Animation System (equippedItemParamName)

### How It Works
```csharp
// Item.cs - ToggleLocalVisibility (line 313-329)
public void ToggleLocalVisibility(PlayerReferenceManager playerReferences, bool visible, bool useEquipmentAnimation)
{
    if (this.netState.Value.State != Item.State.Equipped)
        return;

    this.runtimeEquippedVisuals.SetActive(visible);

    if (this.equippedItemParamName != string.Empty)  // KEY CONDITION
    {
        if (useEquipmentAnimation)
        {
            playerReferences.ApperanceController.AppearanceAnimator.Animator.SetInteger(
                this.equippedItemParamName, this.equippedItemID);
        }
        else
        {
            playerReferences.ApperanceController.AppearanceAnimator.Animator.SetInteger(
                this.equippedItemParamName, 0);
        }
    }
}
```

### Field Definition
```csharp
// Item.cs line 836
[SerializeField]
private string equippedItemParamName = string.Empty;  // DEFAULT: empty!
```

### Critical Finding: Empty = No Holding Animation
- **If `equippedItemParamName` is empty string**: NO animator parameter is set → NO holding animation
- **If `equippedItemParamName` has value**: Animator parameter triggers holding pose
- **Default is empty string** - items DON'T hold by default

### Native Items That Use Empty equippedItemParamName
These item types use the default empty string and stay in inventory without holding animation:
1. **TreasureItem** - Plot items, collectibles, keys
2. **ConsumableHealingItem** - Health potions
3. **ThrownBombItem** - Bomb stacks
4. **DashPackItem** - Dash equipment
5. **JetpackItem** - Jetpack equipment

**This is the native mechanism** - not a workaround.

---

## 7. Visual Equipment Slots

### VisualEquipmentSlot Enum
```csharp
// VisualEquipmentSlot.cs
public enum VisualEquipmentSlot
{
    LeftHand,     // Hand bone attachment
    RightHand,    // Hand bone attachment
    BothHands,    // For two-handed items
    Back,         // Back attachment bone
    Hips          // Hips/Lumbar attachment bone
}
```

### Usage
- Determines where equipped item visual attaches to character skeleton
- Items like JetpackItem use `Back` slot
- Weapons use hand slots
- Even if visually attached, animation only plays if `equippedItemParamName` is set

---

## Summary: Issues and Solutions

### Issue 1: Number Keys Not Working
**Root Cause**: `_definitionRegistered` boolean blocked all props after first
**Real Problem**: Second+ prop GUIDs never added to `usableDefinitionMap`
**Fix (v10.53.0)**: Changed from boolean to `HashSet<string>` to track per-GUID registration
**Status**: ✅ FIXED

### Issue 2: Icons Showing White/Blank
**Root Cause**: Asset bundle texture/sprite loading
- Icons load via: PropLibrary → Item.InventoryUsableDefinition.Sprite
- NOT affected by RuntimeDatabase registration
- If white: Check Sprite.texture validity, texture format, import settings
**Status**: Requires asset bundle configuration in Unity

### Issue 3: Rotation Around Wrong Axis
**Root Cause**: `VisualsInfo.Angles` from template being applied to custom prefab
- The game applies `Quaternion.Euler(GroundVisualsInfo.Angles)` at instantiation time
- Template TreasureItem may have non-zero angles for its own mesh
- These angles were being inherited and applied to custom prefabs incorrectly
- SimpleYawSpin code itself is correct (rotates around local Y)

**Fix (v10.55.0)**: Reset `VisualsInfo.Position` and `VisualsInfo.Angles` to zero when injecting custom prefab
- The prefab's hierarchy already has correct orientation (wrapper → mesh)
- No additional rotation offset should be applied by the game
**Status**: ✅ FIXED (uses native instantiation, just with zeroed angles)

### Issue 4: Character Catapulting on Pickup
**Root Cause**: Custom prop colliders on wrong layer (Default instead of PlayerInteractable)
**Real Problem**: Default layer causes physical collision with Character layer
**Fix (v10.54.0)**: Set all custom prop colliders to PlayerInteractable layer at load time
**Status**: ✅ FIXED (uses native game layer system)

### Issue 5: Holding Animation Not Wanted
**Root Cause**: Template TreasureItem prefab has `equippedItemParamName` serialized as `"EquippedItem"`
**Real Problem**: Even though the base class default is empty string, the prefab overrides it
- The serialized value is copied to custom props, causing holding animation to play
**Fix (v10.56.0)**: Explicitly reset `equippedItemParamName` to empty string in Item_ComponentInitialize_Prefix
**Status**: ✅ FIXED (uses native empty-string mechanism)

---

## 8. Plugin Implementation Details

### Plugin Version History

| Version | Date | Fix |
|---------|------|-----|
| v10.53.0 | 2026-01-17 | Number key shortcuts (per-GUID registration) |
| v10.54.0 | 2026-01-17 | Character catapulting (PlayerInteractable layer) |
| v10.55.0 | 2026-01-17 | Rotation axis (Reset VisualsInfo.Angles) |
| v10.56.0 | 2026-01-17 | Holding animation (Reset equippedItemParamName) |

---

### Fix 1: Number Key Shortcuts (v10.53.0)

**Problem**: Only the first custom prop could be equipped via number keys. All subsequent props failed silently.

**Root Cause Analysis**:
```csharp
// BEFORE: Single boolean blocked ALL registrations after first
private static bool _definitionRegistered = false;

private static void RegisterDefinitionWithRuntimeDatabase(object clonedDefinition)
{
    if (_definitionRegistered)  // TRUE after first prop!
    {
        Log.LogInfo("[RUNTIME-DB] Definition already registered, skipping");
        return;  // ALL SUBSEQUENT PROPS SKIPPED
    }
    // ... registration code ...
    _definitionRegistered = true;
}
```

**Why It Failed**:
1. First prop: `_definitionRegistered = false` → registers → sets to `true`
2. Second prop: `_definitionRegistered = true` → skips registration
3. When player presses number key for second prop:
   - `Inventory.EquipSlot_ServerRPC()` calls `RuntimeDatabase.GetUsableDefinition(guid)`
   - GUID not in dictionary → `KeyNotFoundException` thrown
   - Equip fails silently

**Solution** (Plugin.cs line ~4344):
```csharp
// AFTER: HashSet tracks each prop's GUID separately
private static HashSet<string> _registeredDefinitionGuids = new HashSet<string>();

private static void RegisterDefinitionWithRuntimeDatabase(object clonedDefinition, string propId)
{
    if (_registeredDefinitionGuids.Contains(propId))
    {
        Log.LogInfo($"[RUNTIME-DB] Definition {propId} already registered, skipping");
        return;  // Only skip if THIS SPECIFIC prop was registered
    }

    // ... get RuntimeDatabase.usableDefinitionMap via reflection ...
    // ... add clonedDefinition to map with its GUID ...

    _registeredDefinitionGuids.Add(propId);  // Track THIS prop
    Log.LogInfo($"[RUNTIME-DB] Registered definition for prop '{propId}' with GUID {guid}");
}
```

**Log Output When Working**:
```
[RUNTIME-DB] Registered definition for prop 'plot_flag_red' with GUID abc123...
[RUNTIME-DB] Registered definition for prop 'plot_flag_blue' with GUID def456...
[RUNTIME-DB] Registered definition for prop 'plot_flag_green' with GUID ghi789...
```

---

### Fix 2: Character Catapulting (v10.54.0)

**Problem**: Player character was launched into the air when walking near custom props on the ground.

**Root Cause Analysis**:
- Unity physics layers control which objects collide
- Native game items use `PlayerInteractable` layer which does NOT cause physical collisions
- Custom prop prefabs from asset bundle have colliders on `Default` layer
- `Default` layer DOES cause physical collisions with `Character` layer
- Result: Physics engine resolves overlap by pushing character away violently

**Game's Native System** (ColliderInfo.cs):
```csharp
public enum ColliderType
{
    [ColliderTypeInfoAttribute("...", "Default")]
    Default,  // Physical collisions - WRONG for pickups!

    [ColliderTypeInfoAttribute("...does NOT cause physical collisions!", "PlayerInteractable")]
    Interactable,  // No physics - CORRECT for pickups!
}
```

**Solution** (Plugin.cs LoadAssetBundle, line ~3100):
```csharp
// v10.54.0: Fix catapulting - Set colliders to PlayerInteractable layer
int interactableLayer = LayerMask.NameToLayer("PlayerInteractable");
if (interactableLayer != -1)
{
    Collider[] prefabColliders = loadedPrefab.GetComponentsInChildren<Collider>(true);
    foreach (Collider col in prefabColliders)
    {
        col.gameObject.layer = interactableLayer;
        Log.LogInfo($"[BUNDLE] Set collider '{col.name}' to PlayerInteractable layer ({interactableLayer})");
    }
    Log.LogInfo($"[BUNDLE] Set {prefabColliders.Length} colliders to PlayerInteractable layer");
}
```

**Log Output When Working**:
```
[BUNDLE] Set collider 'FlagMesh' to PlayerInteractable layer (14)
[BUNDLE] Set collider 'FlagPole' to PlayerInteractable layer (14)
[BUNDLE] Set 2 colliders to PlayerInteractable layer
```

---

### Fix 3: Rotation Axis (v10.55.0)

**Problem**: Custom props rotated around wrong axis when spinning on ground (appeared to tumble instead of spin upright).

**Root Cause Analysis**:

The issue was NOT the SimpleYawSpin component (which correctly rotates around local Y). The issue was the `VisualsInfo.Angles` field from the template TreasureItem being applied to custom prefabs.

**Game's Instantiation Code** (Item.cs line 726):
```csharp
this.runtimeGroundVisuals = Object.Instantiate<GameObject>(
    this.GroundVisualsInfo.GameObject,
    position + this.GroundVisualsInfo.Position,
    parentRotation * Quaternion.Euler(this.GroundVisualsInfo.Angles),  // ANGLES APPLIED HERE!
    parent
);
```

**What Happened**:
1. Template TreasureItem has `GroundVisualsInfo.Angles = (90, 0, 0)` for its own mesh
2. Plugin replaces `GroundVisualsInfo.GameObject` with custom prefab
3. Plugin did NOT reset `GroundVisualsInfo.Angles`
4. Game applies `Quaternion.Euler(90, 0, 0)` to custom prefab at instantiation
5. Custom prefab's local Y axis now points sideways in world space
6. SimpleYawSpin rotates around local Y → appears to tumble

**User's Unity Setup**:
- Custom prefab already has correct hierarchy: `Wrapper (Y-aligned) → Mesh (rotated to match)`
- The wrapper's local Y = world Y by design
- No additional rotation needed from the game

**Solution** (Plugin.cs Item_ComponentInitialize_Prefix, line ~3740):
```csharp
FieldInfo fieldInfo2 = AccessTools.Field(_treasureItemType, "tempVisualsInfoGround");
if (fieldInfo2 != null)
{
    object value3 = fieldInfo2.GetValue(__instance);
    if (value3 != null)
    {
        FieldInfo field = nestedType.GetField("GameObject");
        FieldInfo posField = nestedType.GetField("Position");
        FieldInfo anglesField = nestedType.GetField("Angles");

        if (field != null)
        {
            field.SetValue(value3, visualPrefab);

            // v10.55.0: Reset Position and Angles to zero
            if (posField != null)
            {
                posField.SetValue(value3, Vector3.zero);
                Log.LogInfo("[VISUAL-INJECT] Reset GroundVisualsInfo.Position to zero");
            }
            if (anglesField != null)
            {
                anglesField.SetValue(value3, Vector3.zero);
                Log.LogInfo("[VISUAL-INJECT] Reset GroundVisualsInfo.Angles to zero");
            }

            fieldInfo2.SetValue(__instance, value3);
        }
    }
}

// Same for EquippedVisualsInfo
FieldInfo fieldInfo3 = AccessTools.Field(_treasureItemType, "tempVisualsInfoEqupped");
// ... identical pattern ...
```

**Log Output When Working**:
```
[VISUAL-INJECT] Reset GroundVisualsInfo.Position to zero
[VISUAL-INJECT] Reset GroundVisualsInfo.Angles to zero
[VISUAL-INJECT] v10.55.0: Injected 'PlotFlag_Red' into GroundVisualsInfo (with zero offset/angles)
[VISUAL-INJECT] Reset EquippedVisualsInfo.Position to zero
[VISUAL-INJECT] Reset EquippedVisualsInfo.Angles to zero
[VISUAL-INJECT] v10.55.0: Injected 'PlotFlag_Red' into EquippedVisualsInfo (with zero offset/angles)
```

---

### Fix 4: Holding Animation (v10.56.0)

**Problem**: Character played holding animation when custom props were equipped, even though they should stay in inventory only (like native TreasureItems).

**Root Cause Analysis**:

**Base Class Default** (Item.cs line 836):
```csharp
[SerializeField]
private string equippedItemParamName = string.Empty;  // Empty = no animation
```

**But Template Prefab Overrides It**:
The TreasureItem prefab used as template has this field serialized with value `"EquippedItem"` (visible in logs).

**How Unity Serialization Works**:
1. Base class declares field with default value
2. Prefab can override via serialized data
3. When we clone the Item component, the serialized value (`"EquippedItem"`) is copied
4. The clone triggers holding animation because field is not empty

**Game's Animation Trigger Code** (Item.cs line 303):
```csharp
if (this.equippedItemParamName != string.Empty)  // NOT empty → animate!
{
    playerReferences.ApperanceController.AppearanceAnimator.Animator.SetInteger(
        this.equippedItemParamName, this.equippedItemID);
}
```

**Solution** (Plugin.cs Item_ComponentInitialize_Prefix, line ~3783):
```csharp
// v10.56.0: Reset equippedItemParamName to empty string to disable holding animation
// Native TreasureItem uses empty string by default, but template prefab may have it set
FieldInfo equippedItemParamField = AccessTools.Field(_itemType, "equippedItemParamName");
if (equippedItemParamField != null)
{
    equippedItemParamField.SetValue(__instance, string.Empty);
    Log.LogInfo("[VISUAL-INJECT] v10.56.0: Reset equippedItemParamName to empty (no holding animation)");
}
```

**Log Output When Working**:
```
[VISUAL-INJECT] v10.56.0: Reset equippedItemParamName to empty (no holding animation)
```

**Before Fix** (from logs):
```
[EQUIP-ANIM] Item's equippedItemParamName: EquippedItem  // NOT empty!
```

**After Fix** (expected):
```
[EQUIP-ANIM] Item's equippedItemParamName:   // Empty string
```

---

### Summary of All Code Locations in Plugin.cs

| Fix | Method | Line Range | Field/Variable Modified |
|-----|--------|------------|------------------------|
| v10.53.0 | `RegisterDefinitionWithRuntimeDatabase` | ~4344-4438 | `_registeredDefinitionGuids` (HashSet) |
| v10.54.0 | `LoadAssetBundle` | ~3100-3117 | `Collider.gameObject.layer` |
| v10.55.0 | `Item_ComponentInitialize_Prefix` | ~3740-3779 | `VisualsInfo.Position`, `VisualsInfo.Angles` |
| v10.56.0 | `Item_ComponentInitialize_Prefix` | ~3783-3790 | `Item.equippedItemParamName` |

---

## 9. Particle Effects System

### Unity ParticleSystem - Standard Unity Component

The game uses **Unity's built-in ParticleSystem** - no custom particle engine.

### SwappableParticleSystem Wrapper

**File:** `ParticleSystems/Components/SwappableParticleSystem.cs`

```csharp
public class SwappableParticleSystem : MonoBehaviour
{
    public bool AutoSpawn { get; private set; } = true;
    public ParticleSystem RuntimeParticleSystem { get; set; }

    [SerializeField]
    private ParticleSystem embeddedParticleSystem;  // DIRECT REFERENCE

    [SerializeField][HideInInspector]
    private AssetReference referencedParticleSystemAsset;  // CLOUD ASSET (not for custom props)
}
```

### Two Loading Modes

| Mode | Use Case | How It Works |
|------|----------|--------------|
| **Embedded** | Custom props | ParticleSystem on prefab, referenced by `embeddedParticleSystem` field |
| **Cloud-Referenced** | Game's cloud assets | Loaded async via AssetID/Version (not for modding) |

### How Game Initializes Particles on Items

**Item.cs lines 730-734:**
```csharp
SwappableParticleSystem[] componentsInChildren =
    this.runtimeEquippedVisuals.GetComponentsInChildren<SwappableParticleSystem>();
for (int i = 0; i < componentsInChildren.Length; i++)
{
    componentsInChildren[i].InitializeWithEmbedded();
}
```

**Key Facts:**
- Uses `GetComponentsInChildren<>()` - searches **entire hierarchy** (root AND all children)
- Particle effect can be on root OR any child - both work
- `InitializeWithEmbedded()` just copies `embeddedParticleSystem` → `RuntimeParticleSystem`

### Two Usage Patterns in Game

**Pattern A: One-Shot Effects (e.g., collection sparkles)**
```csharp
swappableParticleSystem.Spawn(false);  // Instantiates, plays, auto-destroys after 10 sec
```

**Pattern B: Continuous/Looping Effects (e.g., jetpack flames)**
```csharp
swappableParticleSystem.RuntimeParticleSystem.Play();
swappableParticleSystem.RuntimeParticleSystem.Stop();
```

### Unity ParticleSystem Configuration

**The game does NOT programmatically set looping or play-on-awake.**
These must be configured in Unity Editor on the ParticleSystem component.

| Setting | Location | For Auto-Play | For Looping |
|---------|----------|---------------|-------------|
| Play On Awake | Main Module | ☑ Checked | - |
| Looping | Main Module | - | ☑ Checked |

**ParticleSystem Inspector Settings:**
```
Main Module:
├── Duration: [effect duration]
├── Looping: ☑ (for continuous effects)
├── Prewarm: ☐ (optional)
├── Start Delay: 0
├── Play On Awake: ☑ (for auto-start)
└── ... other settings
```

### Prefab Structure for Custom Props with Particles

**Option A: Particle as sibling of mesh (RECOMMENDED)**
```
CustomProp_Prefab (root - Y-aligned wrapper)
├── VisualMesh (your 3D model)
└── ParticleEffect (empty GameObject)
    ├── SwappableParticleSystem (Component)
    │   └── embeddedParticleSystem → [drag ParticleSystem here]
    └── ParticleSystem (Component)
        ├── Play On Awake: ☑
        └── Looping: ☑
```

**Option B: Particle on mesh itself**
```
CustomProp_Prefab (root)
└── VisualMesh
    ├── MeshRenderer, MeshFilter, etc.
    ├── SwappableParticleSystem (Component)
    │   └── embeddedParticleSystem → [drag ParticleSystem here]
    └── ParticleSystem (Component)
```

**Both work** because `GetComponentsInChildren<>()` searches the entire hierarchy.

### Unity Setup Checklist for Particle Effects

1. **Create particle GameObject** as child of your prefab (anywhere in hierarchy)
2. **Add ParticleSystem component** (Unity's built-in)
   - Configure effect (emission, shape, color, size, etc.)
   - Set `Play On Awake = true` for auto-start
   - Set `Looping = true` for continuous effect
3. **Add SwappableParticleSystem component** to SAME GameObject
4. **Drag the ParticleSystem** into `embeddedParticleSystem` field
5. **Build asset bundle** - particle is now embedded in prefab

### Important Notes

- **No separate particle asset needed** - embed on prefab
- **SwappableParticleSystem is required** - game looks for this wrapper, not raw ParticleSystem
- **embeddedParticleSystem field must be set** - drag your ParticleSystem into it
- **Play On Awake handles auto-start** - no code needed for looping effects
- **Position particle GameObject** where you want effect to appear relative to prop

### Examples from Game Code

**ResourcePickup** - One-shot collection effects:
```csharp
// ResourcePickupReferences.cs
public SwappableParticleSystem OneCollectedParticleSystem;
public SwappableParticleSystem FiveCollectedParticleSystem;
public SwappableParticleSystem TenCollectedParticleSystem;

// ResourcePickup.ShowCollectedFX()
swappableParticleSystem.Spawn(false);  // One-shot, destroys after 10 sec
```

**JetpackItem** - Continuous looping effect:
```csharp
// Turn on flames
this.jetpackVisualReferences.InUseParticleSystem.RuntimeParticleSystem.Play();
// Turn off flames
this.jetpackVisualReferences.InUseParticleSystem.RuntimeParticleSystem.Stop();
```

**DashPackItem** - Directional effects:
```csharp
this.dashPackVisualReferences.LeftSideParticleSystem.RuntimeParticleSystem.Play();
this.dashPackVisualReferences.RightSideParticleSystem.RuntimeParticleSystem.Play();
```

---

## Files Referenced

| System | File Path |
|--------|-----------|
| RuntimeDatabase | `DECOMPILED_GAME/Gameplay/.../RuntimeDatabase.cs` |
| UsableDefinition | `DECOMPILED_GAME/Gameplay/.../UsableDefinition.cs` |
| InventoryUsableDefinition | `DECOMPILED_GAME/Gameplay/.../InventoryUsableDefinition.cs` |
| Inventory | `DECOMPILED_GAME/Gameplay/.../Inventory.cs` |
| InventorySlot | `DECOMPILED_GAME/Gameplay/.../InventorySlot.cs` |
| Item | `DECOMPILED_GAME/Gameplay/.../Item.cs` |
| UIItemView | `DECOMPILED_GAME/Gameplay/.../UI/UIItemView.cs` |
| SimpleYawSpin | `DECOMPILED_GAME/Gameplay/.../SimpleYawSpin.cs` |
| PlayerNetworkController | `DECOMPILED_GAME/Gameplay/.../PlayerNetworkController.cs` |
| ColliderInfo | `DECOMPILED_GAME/Props/.../ColliderInfo.cs` |
| VisualEquipmentSlot | `DECOMPILED_GAME/Gameplay/.../VisualEquipmentSlot.cs` |
| TreasureItem | `DECOMPILED_GAME/Gameplay/.../TreasureItem.cs` |
| ConsumableHealingItem | `DECOMPILED_GAME/Gameplay/.../ConsumableHealingItem.cs` |
| DashPackItem | `DECOMPILED_GAME/Gameplay/.../DashPackItem.cs` |
| JetpackItem | `DECOMPILED_GAME/Gameplay/.../JetpackItem.cs` |
| SwappableParticleSystem | `DECOMPILED_GAME/ParticleSystems/.../SwappableParticleSystem.cs` |
| ParticleSystemAsset | `DECOMPILED_GAME/ParticleSystems/.../ParticleSystemAsset.cs` |
| ResourcePickup | `DECOMPILED_GAME/Gameplay/.../ResourcePickup.cs` |
| ResourcePickupReferences | `DECOMPILED_GAME/Props/.../ResourcePickupReferences.cs` |
| EndlessProp | `DECOMPILED_GAME/Gameplay/.../Scripting/EndlessProp.cs` |

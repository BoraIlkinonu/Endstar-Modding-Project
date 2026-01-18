# EndstarPropInjector Plugin Documentation

## Version: 10.67.0
## Status: WORKING

---

## Project Overview

**Goal:** Transform static props in the Endstar game into fully functional collectible items using BepInEx plugin modding with Harmony patches.

**Target Item:** Pearl Basket - converted from a static decorative prop to a collectible treasure item that players can pick up, equip, and see in their inventory.

---

## Technical Architecture

### Plugin Framework
- **BepInEx 5.x** - Unity game modding framework
- **Harmony 2.x** - Runtime method patching library
- **Target Framework:** .NET Framework 4.7.1
- **Unity Version:** Compatible with Endstar's Unity build

### Key Game Systems Involved
1. **StageManager** - Manages props and items in game stages
2. **PropLibrary** - Registry of all props with runtime info
3. **RuntimeDatabase** - Central registry for UsableDefinitions (items, tools, etc.)
4. **Item / TreasureItem** - Base classes for collectible items
5. **AppearanceAnimator** - Character animation controller
6. **SimpleYawSpin** - Component that rotates objects on Y-axis

---

## Issues Solved

### Issue #1: Item Rotation When Equipped (Fixed in v10.33.3)

**Problem:** The Pearl Basket continued rotating (spinning) even after being picked up and equipped by the player. Items should only rotate when sitting on the ground as a visual indicator, not when held by the player.

**Root Cause:** The `SimpleYawSpin` component was being added to the visual prefab, which persisted even after the item was equipped.

**Solution:** Do NOT add `SimpleYawSpin` to the visual prefab. The game's native item system handles ground rotation differently - through the `TreasureItem` component's built-in behavior.

**Code Change:**
```csharp
// REMOVED from CreateVisualPrefab():
// var spin = visual.AddComponent<SimpleYawSpin>();
// spin.rotationSpeed = 45f;
```

**Result:** Items now rotate on ground (via game's native system) but remain static when equipped.

---

### Issue #2: Character Animation Stuck After Pickup (Fixed in v10.34.0)

**Problem:** After picking up the custom Pearl Basket, the player character's movement animations would freeze/stick. The character could still move but the walk/run animations wouldn't play.

**Root Cause Discovery:** User insight - "could it be related to the item definition file being not properly filled out or registered with a system?"

**Technical Analysis:**

The game's `RuntimeDatabase` class maintains a static dictionary for UsableDefinitions:

```csharp
// From RuntimeDatabase.cs (decompiled)
protected static Dictionary<SerializableGuid, UsableDefinition> usableDefinitionMap =
    new Dictionary<SerializableGuid, UsableDefinition>();

public static UsableDefinition GetUsableDefinition(SerializableGuid guid)
{
    return RuntimeDatabase.usableDefinitionMap[guid];  // Direct lookup - throws if not found!
}
```

The dictionary is populated at startup from Unity Resources:
```csharp
private void Start()
{
    RuntimeDatabase.usableDefinitions = Resources.LoadAll<UsableDefinition>(this.usableDefinitionResourcePath);
    foreach (UsableDefinition usableDefinition in RuntimeDatabase.usableDefinitions)
    {
        RuntimeDatabase.usableDefinitionMap.Add(usableDefinition.Guid, usableDefinition);
    }
}
```

**The Problem:** Our custom cloned definition with GUID `11111111-1111-1111-1111-111111111111` was NEVER registered in this dictionary. When the animation system or other game systems tried to look up our custom item's definition, the lookup failed, causing cascading failures in the animation state machine.

**Solution:** Register the cloned UsableDefinition with RuntimeDatabase after cloning.

**Code Added:**
```csharp
private static bool _definitionRegistered = false;

private static void RegisterDefinitionWithRuntimeDatabase(object clonedDefinition)
{
    try
    {
        if (_definitionRegistered)
        {
            Log.LogInfo("[RUNTIME-DB] Definition already registered, skipping");
            return;
        }

        // Find RuntimeDatabase type
        Type runtimeDbType = null;
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            runtimeDbType = assembly.GetType("Endless.Gameplay.RuntimeDatabase");
            if (runtimeDbType != null) break;
        }

        if (runtimeDbType == null)
        {
            Log.LogError("[RUNTIME-DB] Could not find RuntimeDatabase type!");
            return;
        }

        // Get the static usableDefinitionMap field
        var mapField = runtimeDbType.GetField("usableDefinitionMap",
            BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy);

        if (mapField == null)
        {
            // Try protected
            mapField = runtimeDbType.GetField("usableDefinitionMap",
                BindingFlags.NonPublic | BindingFlags.Static);
        }

        if (mapField == null)
        {
            Log.LogError("[RUNTIME-DB] Could not find usableDefinitionMap field!");
            return;
        }

        var mapValue = mapField.GetValue(null);
        if (mapValue == null)
        {
            Log.LogError("[RUNTIME-DB] usableDefinitionMap is null!");
            return;
        }

        // Get the Guid from our cloned definition
        var guidProp = clonedDefinition.GetType().GetProperty("Guid");
        if (guidProp == null)
        {
            Log.LogError("[RUNTIME-DB] Could not find Guid property on definition!");
            return;
        }

        var guid = guidProp.GetValue(clonedDefinition);
        Log.LogInfo($"[RUNTIME-DB] Registering definition with GUID: {guid}");

        // Add to dictionary using indexer
        var dictType = mapValue.GetType();
        var indexerProp = dictType.GetProperty("Item");
        if (indexerProp != null)
        {
            indexerProp.SetValue(mapValue, clonedDefinition, new object[] { guid });
            _definitionRegistered = true;
            Log.LogInfo($"[RUNTIME-DB] Successfully registered definition with GUID {guid} in RuntimeDatabase!");
        }

        // Verify registration
        var containsKeyMethod = dictType.GetMethod("ContainsKey");
        if (containsKeyMethod != null)
        {
            bool exists = (bool)containsKeyMethod.Invoke(mapValue, new object[] { guid });
            Log.LogInfo($"[RUNTIME-DB] Verification - definition exists in map: {exists}");
        }
    }
    catch (Exception ex)
    {
        Log.LogError($"[RUNTIME-DB] Error registering definition: {ex.Message}");
        Log.LogError($"[RUNTIME-DB] Stack trace: {ex.StackTrace}");
    }
}
```

**Called from:** The cloning process after creating the custom definition:
```csharp
// v10.34.0: CRITICAL - Register the cloned definition with RuntimeDatabase
RegisterDefinitionWithRuntimeDatabase(clonedDefinition);
```

**Result:** Character animations now work correctly after picking up custom items.

---

## Custom Item Configuration

### GUID Assignment
```csharp
private static readonly string CUSTOM_ASSET_ID = "custom-pearl-basket-001";
private static readonly string CUSTOM_DEFINITION_GUID = "11111111-1111-1111-1111-111111111111";
```

### Definition Cloning Process
1. Find an existing TreasureUsableDefinition in the game (donor item)
2. Clone all fields via reflection
3. Assign custom GUID
4. Register with RuntimeDatabase
5. Associate with injected prop

### Visual Prefab Creation
```csharp
private static GameObject CreateVisualPrefab(string propName)
{
    var visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
    visual.name = propName + "_Visual";

    // Pink/magenta color for visibility
    var renderer = visual.GetComponent<Renderer>();
    if (renderer != null)
    {
        renderer.material = new Material(Shader.Find("Standard"));
        renderer.material.color = new Color(1f, 0.4f, 0.7f); // Pink
    }

    // Remove default collider (game handles collision)
    var collider = visual.GetComponent<Collider>();
    if (collider != null) UnityEngine.Object.Destroy(collider);

    visual.SetActive(false);
    UnityEngine.Object.DontDestroyOnLoad(visual);

    return visual;
}
```

---

## Key Lessons Learned

### 1. Game Registries Must Be Updated
When creating custom items that clone existing definitions, you must register them with ALL relevant game registries, not just the immediate ones you're working with. The `RuntimeDatabase.usableDefinitionMap` is a critical registry that many game systems depend on.

### 2. Don't Add Rotation Components Manually
The game's `TreasureItem` system handles visual effects like rotation internally. Adding `SimpleYawSpin` manually causes duplicate/conflicting rotation behavior.

### 3. Reflection is Essential for Modding
Many game fields and registries are private/protected. Use reflection with appropriate BindingFlags:
```csharp
BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.FlattenHierarchy
```

### 4. User Insights are Valuable
The animation fix came from the user's insight about item definitions not being "properly registered with a system" - always consider registration/lookup failures when game systems behave unexpectedly after adding custom content.

---

## Version History

| Version | Changes | Status |
|---------|---------|--------|
| 10.33.3 | Removed SimpleYawSpin from visual prefab | Fixed rotation |
| 10.34.0 | Added RuntimeDatabase registration | Fixed animation |

---

## File Structure

```
D:\Endstar Plot Flag\PLOT FLAG SYSTEM\EndstarPropInjector\
├── Plugin.cs                    # Main plugin code
├── EndstarPropInjector.csproj   # Project file
├── DOCUMENTATION.md             # This file
└── bin\
    └── Release\
        └── EndstarPropInjector.dll  # Compiled plugin
```

**Deployment Path:**
```
C:\Endless Studios\Endless Launcher\Endstar\BepInEx\plugins\EndstarPropInjector.dll
```

---

## Diagnostic Logging

The plugin includes extensive logging for debugging:

### Registration Logs
```
[RUNTIME-DB] Registering definition with GUID: 11111111-1111-1111-1111-111111111111
[RUNTIME-DB] Successfully registered definition with GUID ... in RuntimeDatabase!
[RUNTIME-DB] Verification - definition exists in map: True
```

### Item Pickup Logs
```
[TOGGLE-VIS] Item.ToggleLocalVisibility called
[TOGGLE-VIS] Animator EquippedItem = 2
[TOGGLE-VIS] Current state hash: -123456789
```

### Animation State Logs (periodic when moving)
```
[ANIM-STATE] NotSwimming: moving=True, grounded=True, velX=0.50, velZ=0.30
[ANIM-STATE] EquippedItem=2, StateHash=-123456789, NormTime=0.45
```

---

## Build Instructions

```powershell
# Build command
& 'C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe' `
    'D:\Endstar Plot Flag\PLOT FLAG SYSTEM\EndstarPropInjector\EndstarPropInjector.csproj' `
    -p:Configuration=Release -verbosity:minimal

# Deploy command
Copy-Item 'D:\Endstar Plot Flag\PLOT FLAG SYSTEM\EndstarPropInjector\bin\Release\EndstarPropInjector.dll' `
    'C:\Endless Studios\Endless Launcher\Endstar\BepInEx\plugins\' -Force
```

---

## Dependencies (from .csproj)

| Assembly | Purpose |
|----------|---------|
| BepInEx.dll | Plugin framework |
| 0Harmony.dll | Runtime patching |
| UnityEngine.dll | Unity core |
| UnityEngine.CoreModule.dll | Unity core module |
| Gameplay.dll | Game's gameplay systems |
| Props.dll | Game's prop system |
| Shared.dll | Game's shared utilities |
| Assets.dll | Game's asset management |

---

## Summary

The EndstarPropInjector plugin successfully converts static props into collectible items by:

1. **Hooking into StageManager** to inject custom props at stage load
2. **Cloning existing TreasureUsableDefinitions** to create valid item definitions
3. **Registering definitions with RuntimeDatabase** so all game systems can find them
4. **Creating visual prefabs** that work with the game's rendering system
5. **Letting the game handle native behaviors** like ground rotation rather than adding custom components

The two major issues (rotation when equipped, animation stuck after pickup) were both caused by not properly integrating with game systems - either by adding conflicting components or failing to register with required registries.

---

## Issue #3: Particle Effects on Custom Props (Fixed in v10.65.0)

### Problem Description

Custom props with embedded particle effects (like the Pearl with Shell prop) exhibited the following behavior:
- Particles played correctly when prop was on the ground (desired)
- Particles continued playing when item was picked up/equipped (undesired)
- Particles reappeared when item was dropped and picked up again (undesired)

**Requirement:** Particles should play ONLY while the prop is on the ground. After first pickup, particles should be permanently disabled and never reappear.

### Technical Architecture

#### Item Visual System

The `Item` class has TWO separate visual GameObjects:

```
Item
├── runtimeGroundVisuals        [SerializeField] - Shown when item is on Ground/Tossed state
├── runtimeEquippedVisuals      [SerializeField] - Shown when item is Equipped/PickedUp state
└── runtimeGroundedVisualsParentGameObject - Parent container for ground visuals
```

**State-based visibility (from `HandleNetStateChanged`):**
```csharp
// Ground visuals shown when: Ground, Tossed, or Teleporting
runtimeGroundVisuals.SetActive(state == State.Ground || state == State.Tossed || state == State.Teleporting);

// Equipped visuals shown when: Equipped
runtimeEquippedVisuals.SetActive(state == State.Equipped);
```

#### Custom Prop Visual Injection

The plugin injects the same visual prefab into BOTH visual systems:

```csharp
// In TreasureItem_ComponentInitialize_Postfix:
// Inject visual prefab into GroundVisualsInfo.GameObject
field.SetValue(tempVisualsInfoGround, visualPrefab);

// Also inject into EquippedVisualsInfo.GameObject
field.SetValue(tempVisualsInfoEquipped, visualPrefab);
```

This means particles from the prefab exist in BOTH `runtimeGroundVisuals` AND `runtimeEquippedVisuals`.

#### Level Items vs Non-Level Items

When an item is picked up, the behavior differs based on item type:

**Non-Level Items (spawned dynamically):**
```csharp
// Item.Pickup() returns same instance
if (!this.IsLevelItem)
{
    this.netState.Value = new Item.NetState(Item.State.PickedUp, player.NetworkObject);
    return this;  // Same instance returned
}
```

**Level Items (placed in editor):**
```csharp
// Item.Pickup() creates NEW copy from template, destroys original
this.netState.Value = new Item.NetState(Item.State.Destroyed, null);
Item newCopy = Object.Instantiate<GameObject>(runtimePropInfo.EndlessProp.gameObject)
    .GetComponentInChildren<Item>();
// ... initialize new copy ...
return newCopy;  // NEW instance returned
```

### Failed Approaches

#### v10.61.0 - PREFIX with DestroyImmediate
**Problem:** Destroyed particles on `__instance` in PREFIX, but for Level Items, `__instance` is the ORIGINAL item that gets destroyed. The NEW copy (returned in `__result`) has fresh particles from the template.

#### v10.62.0 - POSTFIX with DestroyImmediate
**Problem:** Used `GetComponent<CustomPropMarker>()` which only searches the current GameObject. `CustomPropMarker` is on `EndlessProp` (parent), not `Item` (child).

#### v10.63.0 - POSTFIX with GetComponentInParent + DestroyImmediate
**Problem:** Marker detection fixed, `DestroyImmediate` was called and logged success. But particles still appeared when dropped. Investigation revealed `DestroyImmediate` wasn't actually working as expected in this context.

#### v10.64.0 - SetActive(false) on Ground Visuals Only
**Problem:** Only disabled particles on `runtimeGroundVisuals`. When item was picked up, `runtimeEquippedVisuals` was shown - which still had active particles!

### Solution: v10.65.0 - Disable Particles on BOTH Visual Systems

**Key insight:** Particles exist in BOTH `runtimeGroundVisuals` AND `runtimeEquippedVisuals`. Must disable on both.

#### Implementation

```csharp
// v10.65.0: Disable particles on BOTH ground and equipped visuals
private static void StopAllVisualParticles(object itemInstance, string itemName)
{
    if (itemInstance == null || _itemType == null)
        return;

    // Disable particles on runtimeGroundVisuals
    DisableParticlesOnVisuals(itemInstance, itemName, "runtimeGroundVisuals", "ground");

    // v10.65.0: Also disable particles on runtimeEquippedVisuals
    DisableParticlesOnVisuals(itemInstance, itemName, "runtimeEquippedVisuals", "equipped");
}

private static void DisableParticlesOnVisuals(object itemInstance, string itemName,
    string fieldName, string visualType)
{
    FieldInfo visualsField = AccessTools.Field(_itemType, fieldName);
    GameObject visuals = visualsField?.GetValue(itemInstance) as GameObject;

    if (visuals == null) return;

    // Get all particle systems (including inactive ones)
    ParticleSystem[] particles = visuals.GetComponentsInChildren<ParticleSystem>(true);

    foreach (ParticleSystem ps in particles)
    {
        // Stop and clear any playing particles
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        ps.Clear(true);

        // Disable playOnAwake as backup
        var main = ps.main;
        main.playOnAwake = false;

        // SetActive(false) persists even when parent is activated
        ps.gameObject.SetActive(false);
    }
}
```

#### Why SetActive(false) Works

When a child GameObject has `activeSelf = false`:
1. It stays inactive even when parent's `SetActive(true)` is called
2. Parent activation only affects children that have `activeSelf = true`
3. The particle GameObject remains disabled permanently

**Flow after v10.65.0:**
```
1. Prop placed in level (particles play on ground visuals) ✓
2. Player picks up item
3. POSTFIX runs on returned item (__result)
4. Disables particles on BOTH ground AND equipped visuals
5. Equipped visuals shown - NO particles (disabled) ✓
6. Player drops item
7. Ground visuals shown - NO particles (disabled) ✓
8. Player picks up again
9. Same instance returned (Non-Level Item now)
10. Particles still disabled from step 4 ✓
```

#### Harmony Hook Location

```csharp
// Hooked in manual patching section of Awake()
harmony.Patch(
    AccessTools.Method(_itemType, "Pickup"),
    prefix: new HarmonyMethod(typeof(Plugin), nameof(Item_Pickup_Prefix)),
    postfix: new HarmonyMethod(typeof(Plugin), nameof(Item_Pickup_Postfix))
);
```

**POSTFIX is critical** because for Level Items, `__result` is the NEW copy (not `__instance`).

### Custom Prop Marker

The plugin uses a marker component to identify custom props:

```csharp
public class CustomPropMarker : MonoBehaviour { }
```

**Marker placement:** Added to `EndlessProp.gameObject` (parent of Item)

**Detection in POSTFIX:**
```csharp
// Must use GetComponentInParent because marker is on EndlessProp, not Item
if (resultComponent.GetComponentInParent<CustomPropMarker>() != null)
{
    StopAllVisualParticles(__result, itemName);
}
```

### Diagnostic Logs

```
[ITEM-PICKUP] ===== Item.Pickup CALLED =====
[ITEM-PICKUP] Item: CustomCollectibleProp(Clone) (Type: TreasureItem)
[ITEM-PICKUP] Pickup returned: CustomCollectibleProp(Clone)
[PARTICLE] v10.63.0: Found CustomPropMarker in parent, destroying particles on __result
[PARTICLE] v10.65.0: Disabling 2 particle(s) on 'CustomCollectibleProp(Clone)' ground visuals
[PARTICLE] v10.65.0: Disabled 'PearlEffect' on ground (SetActive false)
[PARTICLE] v10.65.0: Disabled 'PearlSparkleEffect' on ground (SetActive false)
[PARTICLE] v10.65.0: Disabling 2 particle(s) on 'CustomCollectibleProp(Clone)' equipped visuals
[PARTICLE] v10.65.0: Disabled 'PearlEffect' on equipped (SetActive false)
[PARTICLE] v10.65.0: Disabled 'PearlSparkleEffect' on equipped (SetActive false)
```

### Pearl Prefab Structure

The Pearl with Shell prefab contains embedded particle systems:

```yaml
Pearl with Shell.prefab
├── Visual Mesh
├── PearlSparkleEffect (ParticleSystem)
│   └── PearlEffect (ParticleSystem - child)
└── Colliders
```

These are NOT separate props - they are child GameObjects embedded directly in the prefab hierarchy. When `Object.Instantiate()` clones the prefab, all children including particles are cloned.

---

## Issue #4: Particle Effect Prefabs Registered as Separate Props (Fixed in v10.67.0)

### Problem Description

Custom asset bundles with particle effect prefabs (like `PearlEffect.prefab` and `PearlSparkleEffect.prefab`) were showing up as separate props in the prop tool window. These particle effects are meant to be embedded components within the main prop prefab, not standalone placeable props.

**Symptoms:**
- "PearlEffect" appearing as a selectable prop in the prop tool
- "PearlSparkleEffect" appearing as a selectable prop
- Cluttered prop tool with unusable particle-only prefabs
- Users could accidentally spawn particle effects without their parent prop

### Technical Analysis

#### How LoadAssetBundle() Works

The `LoadAssetBundle()` method iterates over ALL `.prefab` assets in the bundle:

```csharp
foreach (KeyValuePair<string, GameObject> item in dictionary)
{
    string key = item.Key;
    GameObject value = item.Value;

    // v10.66.0 and earlier: Created LoadedPropDefinition for EVERY prefab
    LoadedPropDefinition loadedPropDefinition = new LoadedPropDefinition
    {
        PropId = GenerateDeterministicGuid(key),
        DisplayName = name,
        VisualPrefab = value,
        // ...
    };
    _loadedPropDefinitions[loadedPropDefinition.PropId] = loadedPropDefinition;
}
```

This caused particle-only prefabs (which are just ParticleSystem components with no visual mesh) to be registered as props.

#### Game's Native Pattern: SwappableParticleSystem

Research into the game's `ResourcePickup` system revealed how the game handles embedded particles:

**From `ResourcePickupReferences.cs`:**
```csharp
public SwappableParticleSystem OneCollectedParticleSystem { get; private set; }
public SwappableParticleSystem FiveCollectedParticleSystem { get; private set; }
public SwappableParticleSystem TenCollectedParticleSystem { get; private set; }
```

Particles are **child component references** within the main prop, not separate assets. The game uses `ReferenceFilter` enum to categorize props:

```csharp
public enum ReferenceFilter
{
    None = 0,
    NonStatic = 1,
    Npc = 2,
    PhysicsObject = 4,
    InventoryItem = 8,
    Key = 16,
    Resource = 32
}
```

**Key Insight:** There is NO filter for "Particle Effect" because particles are NEVER standalone props in the game's architecture.

### Solution: Filter Particle-Only Prefabs (v10.67.0)

**Logic:** A legitimate prop prefab has a visible mesh at its root level. Particle effect prefabs have ParticleSystem components but NO mesh renderer at the root.

**Filter Criteria:**
1. Check if prefab has `ParticleSystem` components (including children)
2. Check if prefab has `MeshRenderer`, `SkinnedMeshRenderer`, or `MeshFilter` at root
3. If has particles BUT no main mesh visual → skip it (it's a particle effect)
4. If has particles AND main mesh visual → keep it (it's a prop with embedded particles)

#### Implementation

```csharp
// v10.67.0: Filter out particle effect prefabs
// Particle effects have ParticleSystem but no MeshRenderer/SkinnedMeshRenderer at root
// They should be embedded components within props, not separate props
// (like ResourcePickup uses SwappableParticleSystem)
ParticleSystem[] particleSystems = value.GetComponentsInChildren<ParticleSystem>(true);
MeshRenderer meshRenderer = value.GetComponent<MeshRenderer>();
SkinnedMeshRenderer skinnedRenderer = value.GetComponent<SkinnedMeshRenderer>();
MeshFilter meshFilter = value.GetComponent<MeshFilter>();

bool hasParticles = particleSystems != null && particleSystems.Length > 0;
bool hasMainVisual = meshRenderer != null || skinnedRenderer != null || meshFilter != null;

// Skip if it's ONLY a particle effect (has particles but no main mesh visual at root)
if (hasParticles && !hasMainVisual)
{
    Log.LogInfo((object)$"[BUNDLE] v10.67.0: Skipping particle effect prefab '{key}' " +
        $"(has {particleSystems.Length} ParticleSystem(s), no mesh at root)");
    continue;
}
```

**Location:** `Plugin.cs` lines 3998-4014 in `LoadAssetBundle()` method

### Filtering Logic Matrix

| Prefab Type | Has Particles | Has Root Mesh | Action |
|-------------|---------------|---------------|--------|
| Static Mesh Prop | No | Yes | ✓ Keep - Normal prop |
| Prop with Effects | Yes | Yes | ✓ Keep - Prop with embedded particles |
| Particle Effect | Yes | No | ✗ Skip - Particle-only prefab |
| Empty/Broken | No | No | ✓ Keep - Let game handle |

### Diagnostic Logs

```
[BUNDLE] v10.67.0: Skipping particle effect prefab 'PearlEffect' (has 1 ParticleSystem(s), no mesh at root)
[BUNDLE] v10.67.0: Skipping particle effect prefab 'PearlSparkleEffect' (has 1 ParticleSystem(s), no mesh at root)
[BUNDLE] Created definition: 'Pearl with Shell' (ID: abc123..., Icon: pearl_icon)
```

### Result

- Particle effect prefabs no longer appear in the prop tool
- Main prop prefabs with embedded particles work correctly
- Particles still function when the parent prop is placed
- Prop tool is no longer cluttered with unusable particle entries

---

## Version History (Updated)

| Version | Changes | Status |
|---------|---------|--------|
| 10.33.3 | Removed SimpleYawSpin from visual prefab | Fixed rotation |
| 10.34.0 | Added RuntimeDatabase registration | Fixed animation |
| 10.61.0 | PREFIX particle destruction (wrong instance) | Failed |
| 10.62.0 | POSTFIX particle destruction (wrong marker check) | Failed |
| 10.63.0 | GetComponentInParent + DestroyImmediate | Partial (drop worked) |
| 10.64.0 | SetActive(false) on ground visuals only | Partial (drop worked, pickup failed) |
| 10.65.0 | SetActive(false) on BOTH ground AND equipped visuals | **WORKING** |
| 10.66.0 | First Pickup Info Popup system | **WORKING** |
| 10.67.0 | Filter particle effect prefabs from prop registration | **WORKING** |

---

## Key Lessons Learned (Updated)

### 5. Dual Visual Systems
Items have separate visual GameObjects for ground and equipped states. Both must be handled when modifying visual elements like particles.

### 6. SetActive vs Destroy
`SetActive(false)` on child GameObjects is more reliable than `DestroyImmediate` for disabling components. Deactivated children stay inactive even when parent is activated.

### 7. POSTFIX for Modified Return Values
When a method may return a different instance than it received (like Level Item pickup creating a new copy), use POSTFIX and operate on `__result`, not `__instance`.

### 8. Component Hierarchy Awareness
When using marker components, be aware of the GameObject hierarchy. `GetComponent` only searches the current GameObject; `GetComponentInParent` searches up the hierarchy.

---

## HUD System & UI Architecture

This section documents the Endstar game's HUD (Heads-Up Display) system, how the inventory UI works, the resource pickup UI flow, and available hooks for plugin customization.

### HUD Architecture Overview

The HUD is organized as a hierarchical system with centralized management and specialized subsystems.

#### Central Management Classes

| Class | File Path | Purpose |
|-------|-----------|---------|
| `UIScreenManager` | `Shared/UI/UIScreenManager.cs` | Top-level screen navigation with display/close stack management |
| `UIGameplayReferenceManager` | `Gameplay/UI/UIGameplayReferenceManager.cs` | Singleton holding UI container references |
| `UIGameplayVisibilityHandler` | `Gameplay/UI/UIGameplayVisibilityHandler.cs` | Controls HUD visibility during gameplay states |

**UIScreenManager Events:**
```csharp
// Screen system events - can be subscribed to for custom handling
OnScreenSystemOpen
OnScreenSystemClose
```

**UIGameplayReferenceManager Properties:**
```csharp
public RectTransform AnchorContainer { get; }      // Container for anchored UI elements
public RectTransform GameplayWindowContainer { get; }  // Container for gameplay windows
```

#### HUD Subsystems

| Subsystem | Main Class | Purpose |
|-----------|------------|---------|
| Inventory | `UIInventoryView` | Displays inventory slots with items |
| Equipment | `UIEquipmentView` | Shows equipped items in hotbar slots |
| Health | `UIHealthViewHeart` | Pooled heart icons for health display |
| Resources | `UICoinsView` | Coin/resource counter with animation |
| Crosshair | `CrosshairUI` | Aiming crosshair with variants |
| Interaction | `UIInteractionPromptAnchorManager` | World-space interaction prompts |
| Dialogue | `UIDialogueBubbleController` | NPC dialogue bubbles |

---

### Inventory UI System

#### Component Hierarchy

```
UIInventoryView (Singleton)
├── UIInventoryController - Drag/drop handling between slots
└── UIInventorySlotView[] - Individual slot displays
    └── UIInventorySlotController - Slot interaction logic
```

**Key Classes:**

| Class | Role |
|-------|------|
| `UIInventoryView` | Singleton that spawns/despawns inventory slot views from pool |
| `UIInventoryController` | Handles drag/drop between inventory and equipped slots |
| `UIInventorySlotView` | Renders individual slot with item icon |
| `UIInventorySlotController` | Handles click/drag events on slot |

**Access Pattern:**
```csharp
// Get inventory UI singleton
var inventoryUI = UIMonoBehaviourSingleton<UIInventoryView>.Instance;

// Access inventory slots
IReadOnlyList<UIInventorySlotView> slots = inventoryUI.InventorySlots;
```

#### Equipment Display System

```
UIEquipmentView (Singleton)
├── UIEquippedSlotController[] - Equipped slot handlers
│   └── UIEquippedSlotView - Visual display
└── UIEquippedSlotModel[] - Data models with change events
```

**UIEquippedSlotModel Properties:**
```csharp
public int Index { get; }
public InventorySlotType InventorySlotType { get; }
public Item Item { get; set; }
public Inventory Inventory { get; }
public UnityEvent OnChanged { get; }  // Event when slot content changes
```

---

### Resource Pickup → UI Flow

When a resource item (like coins or collectibles) is picked up, this is the complete flow from pickup to UI update:

```
┌──────────────────────────────────────┐
│ ResourcePickup.ApplyPickupResult()   │ ← Triggered when player touches resource
└──────────────────┬───────────────────┘
                   ↓
┌──────────────────────────────────────┐
│ ResourceManager.ResourceCollected()  │ ← Server processes collection
│ (resource, quantity, clientId)       │
└──────────────────┬───────────────────┘
                   ↓
┌──────────────────────────────────────┐
│ Collection Rule Check:               │
│ • Solo - Only collecting player gets │
│ • Duplicated - All players get copy  │
│ • SharedPool - Goes to shared pool   │
└──────────────────┬───────────────────┘
                   ↓
┌──────────────────────────────────────┐
│ NotifyPlayerOfCountChanged()         │ ← Prepares client notification
└──────────────────┬───────────────────┘
                   ↓
┌──────────────────────────────────────┐
│ HandleCountChanged_ClientRpc()       │ ← Network RPC to client(s)
└──────────────────┬───────────────────┘
                   ↓
┌──────────────────────────────────────────────┐
│ OnLocalCoinAmountUpdated.Invoke(amount)      │ ← KEY EVENT for UI update
└──────────────────┬───────────────────────────┘
                   ↓
┌──────────────────────────────────────┐
│ UICoinsView.CountUpTo(amount)        │ ← UI receives and processes
│ ├→ Display() with animation          │
│ └→ CountUpToCoroutine() animates     │
└──────────────────────────────────────┘
```

#### Key Methods in ResourceManager

```csharp
// Server-side collection processing
public void ResourceCollected(ResourceLibraryReference resource, int quantity, ulong clientId)
{
    // Track resource in currentGameResources HashSet
    // Apply collection rules (Solo/Duplicated/SharedPool)
    // Call NotifyPlayerOfCountChanged()
}

// Client notification via RPC
[ClientRpc]
public void HandleCountChanged_ClientRpc(ResourceLibraryReference resource,
    ulong clientId, int amount, ClientRpcParams clientRpcParams)
{
    // Invokes OnLocalCoinAmountUpdated event
    this.OnLocalCoinAmountUpdated.Invoke(amount);
}
```

#### UICoinsView Implementation

```csharp
// In Start() - subscribes to ResourceManager event
NetworkBehaviourSingleton<ResourceManager>.Instance.OnLocalCoinAmountUpdated
    .AddListener(new UnityAction<int>(this.CountUpTo));

// Counter animation
private IEnumerator CountUpToCoroutine()
{
    // Animates from current value to target
    // incrementDuration: 0.1 seconds per increment
    // maxTotalIncrementDuration: 3 seconds max
    // Updates TextMeshProUGUI component
    // Runs tweens for visual feedback
}
```

#### Visual Feedback

**Particle Effects (at pickup location):**
- Quantity 1-4: `OneCollectedParticleSystem`
- Quantity 5-9: `FiveCollectedParticleSystem`
- Quantity 10+: `TenCollectedParticleSystem`

---

### Available Hooks for Customization

The game provides several extensibility points that allow plugins to cooperate with game systems rather than fighting against them.

#### A. UnityEvent-Based Events

Subscribe to these events to react to game state changes:

```csharp
// GameplayManager - Gameplay lifecycle
MonoBehaviourSingleton<GameplayManager>.Instance.OnGameplayStarted.AddListener(MyHandler);
MonoBehaviourSingleton<GameplayManager>.Instance.OnGameplayStopped.AddListener(MyHandler);
MonoBehaviourSingleton<GameplayManager>.Instance.OnGameplayCleanup.AddListener(MyHandler);
MonoBehaviourSingleton<GameplayManager>.Instance.OnLevelLoaded.AddListener(MyLevelHandler);

// PlayerManager - Player join/leave
NetworkBehaviourSingleton<PlayerManager>.Instance.OnNewPlayerRegistered.AddListener(MyPlayerHandler);
NetworkBehaviourSingleton<PlayerManager>.Instance.OnOwnerRegistered.AddListener(MyOwnerHandler);
NetworkBehaviourSingleton<PlayerManager>.Instance.PlayerUnregistered.AddListener(MyLeaveHandler);

// ResourceManager - Resource collection
NetworkBehaviourSingleton<ResourceManager>.Instance.OnLocalCoinAmountUpdated.AddListener(MyResourceHandler);

// HealthComponent - Combat events
healthComponent.OnHealthChanged.AddListener((current, max) => { });
healthComponent.OnHealthLost.AddListener((data) => { });
healthComponent.OnDefeated.AddListener((damage) => { });
healthComponent.OnMaxHealthChanged.AddListener((current, max) => { });

// AppearanceController - Equipment events
appearanceController.OnEquipmentAvailableChanged.AddListener((guid, available) => { });
appearanceController.OnEquipmentInUseChanged.AddListener((guid, inUse) => { });
appearanceController.OnEquipmentCooldownChanged.AddListener((guid, current, max) => { });
```

#### B. Subscriber Interfaces

The game uses interface-based lifecycle hooks. Implement these to integrate with the game loop:

```csharp
// Lifecycle Hook Interfaces
public interface IAwakeSubscriber
{
    void EndlessAwake();  // Called during Awake
}

public interface IStartSubscriber
{
    void EndlessStart();  // Called during Start
}

public interface IUpdateSubscriber
{
    void EndlessUpdate();  // Called every frame
}

public interface ILateUpdateSubscriber
{
    void EndlessLateUpdate();  // Called after Update
}

public interface IFixedUpdateSubscriber
{
    void EndlessFixedUpdate();  // Called at fixed intervals (physics)
}

public interface IGameEndSubscriber
{
    void EndlessGameEnd();  // Called when game ends
}

public interface IPersistantStateSubscriber
{
    bool ShouldSaveAndLoad { get; }
    object GetSaveState();       // Return state to save
    void LoadState(object state); // Receive saved state
}
```

**Example Implementation (from ResourcePickup.cs):**
```csharp
public class ResourcePickup : InstantPickupBase, IGameEndSubscriber
{
    void IGameEndSubscriber.EndlessGameEnd()
    {
        if (base.NetworkManager.IsServer && !base.ShouldSaveAndLoad)
        {
            base.NetworkObject.Despawn(true);
            Object.Destroy(base.gameObject);
        }
    }
}
```

#### C. IScriptInjector Pattern

The game uses Lua scripting with injectable interfaces:

```csharp
public interface IScriptInjector
{
    string LuaObjectName { get; }
    object LuaObject { get; }
    Type LuaObjectType { get; }
    List<Type> EnumTypes { get; }
    bool AllowLuaReference { get; }

    void ScriptInitialize(EndlessScriptComponent endlessScriptComponent);
}

// Usage in ResourcePickup - allows Lua to override pickup behavior
protected override bool ExternalAttemptPickup(Context context)
{
    bool flag;
    // If Lua script defines "AttemptPickup", call it
    return !this.scriptComponent.TryExecuteFunction<bool>("AttemptPickup", out flag,
        new object[] { context }) || flag;
}
```

#### D. EndlessEvent System

Custom UnityEvent wrapper that includes Context:

```csharp
// Base class wraps Context automatically
public class EndlessEvent : UnityEvent<Context>
{
    public new void Invoke(Context context)
    {
        Context.StaticLastContext = context;
        base.Invoke(context);
    }
}

// Generic variants for additional parameters
public class EndlessEvent<T1> : UnityEvent<Context, T1>
public class EndlessEvent<T1, T2> : UnityEvent<Context, T1, T2>
public class EndlessEvent<T1, T2, T3> : UnityEvent<Context, T1, T2, T3>
```

#### E. Network Custom Messaging

Register custom message handlers for multiplayer communication:

```csharp
// Register named message handler
NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler(
    "MyCustomMessage",
    new CustomMessagingManager.HandleNamedMessageDelegate(HandleMyMessage)
);

// Built-in message channels used by the game:
// "NetState"   - Player state updates
// "AiState"    - NPC state sync
// "NetInput"   - Player input
```

#### F. Singleton Access Pattern

Access any game singleton manager:

```csharp
// Network singletons (for networked managers)
NetworkBehaviourSingleton<ResourceManager>.Instance
NetworkBehaviourSingleton<PlayerManager>.Instance
NetworkBehaviourSingleton<GameplayMessagingManager>.Instance

// MonoBehaviour singletons (for non-networked managers)
MonoBehaviourSingleton<GameplayManager>.Instance
MonoBehaviourSingleton<StageManager>.Instance

// UI singletons
UIMonoBehaviourSingleton<UIInventoryView>.Instance
UIMonoBehaviourSingleton<UIEquipmentView>.Instance
```

---

### Harmony Hook Points for Plugin Development

These are recommended methods to hook with Harmony patches:

#### Resource Collection Hooks

```csharp
// Intercept resource collection (server-side)
[HarmonyPrefix]
[HarmonyPatch(typeof(ResourceManager), "ResourceCollected")]
static bool ResourceCollected_Prefix(ResourceLibraryReference resource, int quantity, ulong clientId)
{
    // Modify collection behavior
    return true; // Continue to original
}

// Intercept after network sync (client-side)
[HarmonyPostfix]
[HarmonyPatch(typeof(ResourceManager), "HandleCountChanged_ClientRpc")]
static void HandleCountChanged_Postfix(int amount)
{
    // React to resource count change
}

// Modify UI display
[HarmonyPrefix]
[HarmonyPatch(typeof(UICoinsView), "CountUpTo")]
static bool CountUpTo_Prefix(ref int target)
{
    // Modify displayed amount
    return true;
}
```

#### Inventory/Equipment Hooks

```csharp
// Hook inventory initialization
[HarmonyPostfix]
[HarmonyPatch(typeof(UIInventoryView), "Start")]
static void UIInventoryView_Start_Postfix(UIInventoryView __instance)
{
    // Add custom inventory handling
}

// Hook equipment changes
[HarmonyPostfix]
[HarmonyPatch(typeof(UIEquippedSlotModel), "set_Item")]
static void EquippedSlotModel_SetItem_Postfix(UIEquippedSlotModel __instance, Item value)
{
    // React to equipment changes
}
```

#### Health/Combat Hooks

```csharp
// Don't need Harmony for these - use events directly
healthComponent.OnHealthLost.AddListener(OnPlayerDamaged);
healthComponent.OnDefeated.AddListener(OnPlayerDefeated);
```

---

### Key File Paths Reference

```
D:\Endstar Plot Flag\DECOMPILED_GAME\
├── Gameplay\Gameplay\Endless\Gameplay\
│   ├── UI\
│   │   ├── UIInventoryView.cs          # Inventory slot display
│   │   ├── UIInventoryController.cs    # Inventory drag/drop
│   │   ├── UIInventorySlotView.cs      # Individual slot view
│   │   ├── UIEquipmentView.cs          # Equipment hotbar
│   │   ├── UIEquippedSlotModel.cs      # Equipment slot data
│   │   ├── UICoinsView.cs              # Resource counter UI
│   │   ├── UIHealthViewHeart.cs        # Health heart display
│   │   ├── CrosshairUI.cs              # Crosshair controller
│   │   ├── UIGameplayReferenceManager.cs    # UI container refs
│   │   ├── UIGameplayVisibilityHandler.cs   # HUD visibility
│   │   ├── UIInteractionPromptAnchor.cs     # Interaction prompts
│   │   └── UIDialogueBubbleController.cs    # Dialogue bubbles
│   ├── ResourceManager.cs              # Resource collection logic
│   ├── ResourcePickup.cs               # Resource pickup component
│   ├── GameplayManager.cs              # Gameplay lifecycle
│   ├── PlayerManager.cs                # Player registration
│   ├── HealthComponent.cs              # Health system with events
│   ├── Inventory.cs                    # Network-synced inventory
│   ├── EndlessEvent.cs                 # Custom UnityEvent wrapper
│   ├── IStartSubscriber.cs             # Lifecycle interface
│   ├── IUpdateSubscriber.cs            # Lifecycle interface
│   ├── IGameEndSubscriber.cs           # Lifecycle interface
│   └── IScriptInjector.cs              # Lua script interface
├── Shared\Shared\Endless\Shared\
│   ├── UI\
│   │   ├── UIScreenManager.cs          # Screen navigation
│   │   ├── UIBaseScreenView.cs         # Base screen class
│   │   ├── IUIPresentable.cs           # UI presenter interface
│   │   └── IUIViewable.cs              # UI view interface
│   └── NetworkBehaviourSingleton.cs    # Singleton base class
└── Gameplay\Gameplay\Scripting\
    └── LuaInterfaceEvent.cs            # Lua event system
```

---

### Cooperative Plugin Design Principles

Based on the game's architecture, these are best practices for plugin development:

1. **Subscribe to events instead of patching** when possible. The game provides many UnityEvent hooks.

2. **Use PostFix patches** when you need the result of a method, especially for methods that may return different instances.

3. **Access singletons through the Instance property** rather than caching references that may become stale.

4. **Implement subscriber interfaces** (IStartSubscriber, IUpdateSubscriber, etc.) for proper lifecycle integration.

5. **Use the script injection pattern** for behavior that should be configurable per-prop.

6. **Handle both network contexts** - check `IsServer` and `IsClient` when patching networked methods.

7. **Respect the dual visual system** - remember items have separate ground and equipped visuals.

---

## Game's Native Modal Architecture

This section documents the Endstar game's built-in modal dialog system, which provides a sophisticated stack-based approach to displaying popups, confirmations, and information windows.

### UIModalManager - Stack-Based Modal System

**Location:** `Shared\Shared\Endless\Shared\UI\UIModalManager.cs`

The game uses a centralized modal manager that handles all popup windows through a stack-based navigation system:

```csharp
public class UIModalManager : UIMonoBehaviourSingleton<UIModalManager>
{
    // Stack-based modal history for back navigation
    private readonly Stack<UIModalHistoryEntry> stack = new Stack<UIModalHistoryEntry>();

    // Modal containers keyed by size type
    private readonly Dictionary<UIModalTypes, RectTransform> modalContainers =
        new Dictionary<UIModalTypes, RectTransform>();

    // Currently displayed modal (top of stack)
    public UIBaseModalView SpawnedModal
    {
        get
        {
            if (this.stack.Count <= 0) return null;
            return this.stack.Peek().SpawnedModal;
        }
    }

    // Whether any modal is currently showing
    public bool ModalIsDisplaying => this.stack.Count > 0;
}
```

### Modal Stack Actions (UIModalManagerStackActions)

**Location:** `Shared\Shared\Endless\Shared\UI\UIModalManagerStackActions.cs`

When displaying a modal, you specify how it should interact with the navigation stack:

```csharp
public enum UIModalManagerStackActions
{
    ClearStack,    // Close all modals, start fresh
    PopStack,      // Replace current modal (no back navigation)
    MaintainStack  // Push onto stack (allows back navigation)
}
```

**Usage Patterns:**
- `ClearStack`: Use for standalone popups that don't need back navigation
- `PopStack`: Use when replacing one modal with another (e.g., "Edit" → "Confirm")
- `MaintainStack`: Use for drill-down navigation where user can go back

### Modal Size Types (UIModalTypes)

**Location:** `Shared\Shared\Endless\Shared\UI\UIModalTypes.cs`

Modals are positioned in different screen regions based on their type:

```csharp
public enum UIModalTypes
{
    CenterSmall,    // Small centered popup (confirmations)
    CenterBig,      // Large centered popup (forms, details)
    SideLeft,       // Left-anchored panel
    SideRight,      // Right-anchored panel
    CenterSameSize  // Centered, preserves prefab dimensions
}
```

Each type maps to a different `RectTransform` container in the UI hierarchy, allowing proper positioning and layering.

### Key UIModalManager Methods

```csharp
// Display a simple text modal with buttons
public void DisplayGenericModal(
    string title,              // Modal header text
    Sprite titleIconSprite,    // Optional icon
    string body,               // Body text
    UIModalManagerStackActions stackAction = ClearStack,
    params UIModalGenericViewAction[] buttons)  // Button definitions

// Display any custom modal prefab
public void Display(
    UIBaseModalView modalSource,    // Prefab to spawn
    UIModalManagerStackActions stackAction,
    params object[] modalData)      // Data passed to OnDisplay()

// Display confirmation dialog (returns bool via Task)
public async Task<bool> Confirm(
    string body,
    UIModalManagerStackActions stackAction = MaintainStack)

// Close current modal and clear entire stack
public void CloseAndClearStack()

// Close current modal but preserve back navigation
public void CloseWithoutClearingStack()
```

### UIBaseModalView - Modal Base Class

**Location:** `Shared\Shared\Endless\Shared\UI\UIBaseModalView.cs`

All modal popups inherit from this abstract base class:

```csharp
public abstract class UIBaseModalView : UIGameObject, IPoolableT, IBackable
{
    // Size/position type for this modal
    [SerializeField] private UIModalTypes modalSize;

    // Raycasters to enable/disable for interaction blocking
    [SerializeField] private GraphicRaycaster[] graphicRaycasters;

    // Animation tweens
    [SerializeField] private TweenCollection displayTweens;  // Play on show
    [SerializeField] private TweenCollection closeTweens;    // Play on hide

    // Modal size accessor
    public UIModalTypes ModalSize => this.modalSize;
}
```

### UIBaseModalView Lifecycle

```
┌─────────────────────────────────────────────────────────────┐
│ 1. UIModalManager.Display() called                          │
│    - Spawns modal prefab from pool                          │
│    - Pushes to stack                                        │
└───────────────────────┬─────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────────────┐
│ 2. OnSpawn()                                                │
│    - Called by pool system                                  │
│    - Triggers displayTweens (fade in, scale, etc.)          │
└───────────────────────┬─────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────────────┐
│ 3. OnDisplay(params object[] modalData)                     │
│    - Receives custom data from Display() call               │
│    - Enables graphicRaycasters for interaction              │
│    - Claims BackManager context for back button handling    │
│    - Override to populate UI with data                      │
└───────────────────────┬─────────────────────────────────────┘
                        ↓
                   [ User interacts ]
                        ↓
┌─────────────────────────────────────────────────────────────┐
│ 4. OnBack() [abstract]                                      │
│    - Called when user presses back/escape                   │
│    - Implement to handle back navigation                    │
└───────────────────────┬─────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────────────┐
│ 5. Close()                                                  │
│    - Disables graphicRaycasters                             │
│    - Unclaims BackManager context                           │
│    - Triggers closeTweens (fade out, scale, etc.)           │
└───────────────────────┬─────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────────────┐
│ 6. OnDespawn() → Return to pool                             │
│    - Called when closeTweens complete                       │
│    - Modal is despawned back to object pool                 │
└─────────────────────────────────────────────────────────────┘
```

### Static Events

The modal manager provides static events for monitoring modal state:

```csharp
// Fired when user explicitly closes a modal (via button, back, etc.)
public static Action<UIBaseModalView> OnModalClosedByUser;

// Fired whenever any modal is closed (programmatic or user)
public static Action<UIBaseModalView> OnModalClosed;
```

### Modal History Entry

The stack stores complete modal state for navigation:

```csharp
private struct UIModalHistoryEntry
{
    public readonly UIBaseModalView ModalSource;   // Prefab used to spawn
    public readonly UIBaseModalView SpawnedModal;  // Active instance
    public readonly object[] ModalData;            // Data for OnDisplay()
}
```

This allows the manager to recreate previous modals when navigating back.

### Access Pattern for Plugins

```csharp
// Access the modal manager singleton
var modalManager = UIMonoBehaviourSingleton<UIModalManager>.Instance;

// Check if any modal is showing
if (modalManager.ModalIsDisplaying)
{
    // Get reference to current modal
    UIBaseModalView currentModal = modalManager.SpawnedModal;
}

// Display a simple info popup
modalManager.DisplayGenericModal(
    "Item Collected!",
    itemIcon,
    "You found a rare pearl. These can be traded with merchants.",
    UIModalManagerStackActions.ClearStack,
    new UIModalGenericViewAction(Color.green, "OK", () => {
        modalManager.CloseAndClearStack();
    })
);

// Subscribe to modal events
UIModalManager.OnModalClosed += (modal) => {
    Log.LogInfo($"Modal closed: {modal.GetType().Name}");
};
```

### Modal System File Paths

```
D:\Endstar Plot Flag\DECOMPILED_GAME\Shared\Shared\Endless\Shared\UI\
├── UIModalManager.cs              # Central modal manager singleton
├── UIModalManagerStackActions.cs  # Stack action enum (Clear/Pop/Maintain)
├── UIModalTypes.cs                # Modal size/position enum
├── UIBaseModalView.cs             # Abstract base class for all modals
├── UIGenericModalView.cs          # Simple text+buttons modal
├── UIErrorModalView.cs            # Error display modal
├── UIDisplayModalButton.cs        # Button that triggers a modal
├── UIDisplayGenericModal.cs       # MonoBehaviour to show generic modal
└── UIDisplayAndHideHandler.cs     # Tween-based show/hide controller
```

---

## Item Pickup Flow - Deep Dive

This section provides a detailed analysis of the game's item pickup system, explaining why custom tracking is necessary for "first pickup" detection.

### Item.Pickup() Complete Flow

**Location:** `Gameplay\Gameplay\Endless\Gameplay\Item.cs` (lines 224-246)

The `Pickup()` method handles item collection with different behavior based on item type:

```csharp
public Item Pickup(PlayerReferenceManager player)
{
    // Only server can process pickups
    if (!base.IsServer)
    {
        return null;
    }

    // NON-LEVEL ITEMS: Dynamic items spawned at runtime
    if (!this.IsLevelItem)
    {
        // Simply change state - same instance continues
        this.netState.Value = new Item.NetState(Item.State.PickedUp, player.NetworkObject);
        return this;  // Returns SAME instance
    }

    // LEVEL ITEMS: Items placed in the level editor
    // Mark original as destroyed
    this.netState.Value = new Item.NetState(Item.State.Destroyed, null);

    // Get the prop template
    PropLibrary.RuntimePropInfo runtimePropInfo;
    MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary
        .TryGetRuntimePropInfo(this.assetID, out runtimePropInfo);

    // Notify stage manager of destruction
    MonoBehaviourSingleton<StageManager>.Instance.PropDestroyed(
        MonoBehaviourSingleton<StageManager>.Instance.ActiveStage
            .GetInstanceIdFromGameObject(base.transform.parent.gameObject));

    // Create NEW copy from template
    Item componentInChildren = Object.Instantiate<GameObject>(
        runtimePropInfo.EndlessProp.gameObject).GetComponentInChildren<Item>();

    componentInChildren.scriptComponent = componentInChildren
        .GetComponentInParent<EndlessScriptComponent>();
    componentInChildren.scriptComponent.ShouldSaveAndLoad = false;
    componentInChildren.NetworkObject.Spawn(false);

    this.CopyToItem(componentInChildren);

    componentInChildren.netState.Value = new Item.NetState(
        Item.State.PickedUp, player.NetworkObject);

    return componentInChildren;  // Returns NEW instance
}
```

### Level Items vs Non-Level Items

| Aspect | Non-Level Item | Level Item |
|--------|---------------|------------|
| **Origin** | Spawned dynamically (dropped, spawned by script) | Placed in level editor |
| **IsLevelItem** | `false` | `true` |
| **On Pickup** | State changes, same instance | Original destroyed, new copy created |
| **Return Value** | `this` (same instance) | New `Item` instance |
| **Persistence** | Not saved with level | Can be saved with level state |

### Item State Machine

```csharp
public enum State
{
    Ground,      // Item sitting on ground, can be picked up
    Tossed,      // Item in flight after being thrown
    PickedUp,    // Just picked up, transitioning to inventory
    Equipped,    // Currently equipped/held by player
    Destroyed,   // Level item's original after pickup
    Teleporting  // Item being teleported
}
```

**State Transitions:**
```
Ground ──pickup──→ PickedUp ──equip──→ Equipped
   ↑                                      │
   └──────────────drop────────────────────┘
                    ↓
                  Tossed ──land──→ Ground
```

### OnPickup Event - Critical Finding

**Location:** `Gameplay\Gameplay\Endless\Gameplay\Item.cs` (line 863)

```csharp
public EndlessEvent OnPickup = new EndlessEvent();
```

**Key Finding:** This event fires on **EVERY** pickup, not just the first one.

The event is invoked in `LevelItemPickupFinished()`:
```csharp
public void LevelItemPickupFinished(PlayerReferenceManager player, Item itemCopy)
{
    this.levelItemsPickedUpCopy = itemCopy;
    this.OnPickup.Invoke(player.WorldObject.Context);  // Fires every time
    Object.Destroy(base.transform.parent.gameObject);
}
```

### Why The Game Has No First-Pickup Tracking

After analyzing the decompiled codebase, the game has **NO built-in**:

1. **First-time pickup detection** - No flag like `hasBeenCollectedBefore`
2. **Item collection log/journal** - No persistent record of collected items
3. **"Is new" flag on items** - Items don't track if they're being picked up for the first time
4. **Discovery system** - No unlock/discovery tracking for item types

**Reason:** The game treats items purely as gameplay objects. An item is:
- On the ground → can be picked up
- In inventory → can be equipped/used
- Equipped → can be dropped/used

There's no semantic layer for "player knowledge" or "discovery state."

### Validating the Custom Tracking Approach

Given the game's architecture, the plugin's session-scoped `HashSet<string>` approach is correct:

```csharp
// Plugin tracking
private static HashSet<string> _collectedPropIdsThisSession = new HashSet<string>();

// In Item_Pickup_Postfix
string propId = GetItemPropId(__result);
if (propId != null && !_collectedPropIdsThisSession.Contains(propId))
{
    _collectedPropIdsThisSession.Add(propId);
    ShowFirstPickupPopup(propId);  // Only on first pickup this session
}
```

**Why this works:**
1. Session-scoped (reset when game restarts) matches player expectation
2. Tracks by prop ID, not instance (handles level items creating new copies)
3. Operates on `__result` in POSTFIX (handles new instance from level items)
4. Simple and efficient O(1) lookup

---

## First Pickup Info Popup System (v10.66.0)

### Overview

The First Pickup Info Popup system displays an information window when a player picks up a custom prop type for the first time in a session. This provides a data-driven way to show lore, instructions, or descriptions about custom items.

### Features

- **Session-scoped tracking:** Popup only appears on first pickup of each item type per game session
- **Data-driven configuration:** All item info loaded from JSON config file
- **Custom props only:** Does not trigger for vanilla game items
- **Customizable appearance:** Uses Unity asset bundle for UI prefab
- **Fade animations:** Smooth fade in/out transitions
- **Dynamic content:** Image and text loaded at runtime from external files

### Design Rationale

#### Why Custom Tracking is Required

As documented in the "Item Pickup Flow - Deep Dive" section above, the game's native systems:
- Have **no first-pickup detection** - `OnPickup` fires every time
- Have **no item discovery journal** - no persistent collection tracking
- Create **new instances** for level items - can't use instance-based tracking

The plugin implements session-scoped tracking with a `HashSet<string>` keyed by prop ID.

#### Why Custom Canvas vs Native Modal System

The plugin creates its own Canvas and UI rather than using `UIModalManager.DisplayGenericModal()`. Here's the reasoning:

**Option 1: Custom Canvas (Current Implementation)**

| Pros | Cons |
|------|------|
| Full control over visual design | More code to maintain |
| Custom animations (fade in/out) | Need to handle canvas layering |
| Image support (RawImage for item art) | No stack navigation |
| No dependency on game's modal prefabs | Custom close button handling |
| Works even if game's UI changes | |

**Option 2: UIModalManager.DisplayGenericModal()**

| Pros | Cons |
|------|------|
| Uses game's existing modal system | Text-only (no RawImage for item images) |
| Automatic stack management | Tied to game's modal visual style |
| BackManager integration (Escape key) | Limited customization |
| Consistent with game's UI | May conflict with other modals |

**Decision:** Custom Canvas was chosen because:
1. **Image support** - The popup needs to show item artwork, which `DisplayGenericModal()` doesn't support
2. **Visual consistency** - The popup should feel like a "plugin addition" not a game system message
3. **Independence** - The popup works regardless of changes to the game's modal system
4. **Simplicity** - A simple info popup doesn't need stack navigation

### Alternative: Using the Native Modal System

If you prefer to use the game's modal system (for text-only popups), here's how:

```csharp
// Get the modal manager singleton
var modalManager = UIMonoBehaviourSingleton<UIModalManager>.Instance;

// Display info popup using native system
modalManager.DisplayGenericModal(
    itemInfo.title,                          // Title
    null,                                     // Icon (would need Sprite, not file path)
    itemInfo.description,                     // Body text
    UIModalManagerStackActions.ClearStack,    // No navigation needed
    new UIModalGenericViewAction(
        Color.green,
        "Got it!",
        () => modalManager.CloseAndClearStack()
    )
);
```

**Limitations:**
- No image display (only supports `Sprite` for title icon, not body images)
- Uses game's default modal styling
- Interrupts any existing modal stack

### File Structure

```
BepInEx/plugins/
├── EndstarPropInjector.dll         # Plugin code
├── firstpickupinfo.bundle          # UI prefab asset bundle (from Unity)
├── Config/
│   └── ItemInfoConfig.json         # Item info configuration
└── ItemImages/
    ├── pearl_info.png              # Item-specific images
    └── gem_info.png
```

### Configuration: ItemInfoConfig.json

```json
{
  "version": "1.0",
  "items": [
    {
      "itemId": "pearl-basket-001",
      "title": "Mystic Pearl",
      "description": "A rare pearl found in the depths of the ocean. Ancient sailors believed these pearls held the power to calm storms.",
      "imagePath": "ItemImages/pearl_info.png",
      "showOnFirstPickup": true
    }
  ]
}
```

| Field | Type | Description |
|-------|------|-------------|
| `itemId` | string | Prop ID matching the bundle definition (PropId in LoadedPropDefinition) |
| `title` | string | Display title (logged, future use) |
| `description` | string | Text shown in popup body |
| `imagePath` | string | Relative path to image file from plugins folder |
| `showOnFirstPickup` | bool | Enable/disable popup for this item |

### Unity Asset Bundle Setup

1. **Create folder structure in Unity project:**
   ```
   Assets/
   ├── EndstarPopupBundle/
   │   ├── Editor/
   │   │   └── BuildPopupBundle.cs
   │   ├── Prefabs/
   │   │   └── FirstPickupInfoPopup.prefab
   │   └── Sprites/
   │       ├── CloseButtonIcon.png
   │       ├── PanelBackground.png
   │       └── DefaultItemImage.png
   └── Bundles/                      # Output folder
   ```

2. **Prefab hierarchy:**
   ```
   FirstPickupInfoPopup (RectTransform + CanvasGroup)
   ├── Panel (Image - 9-slice background)
   │   ├── CloseButton (Button)
   │   │   └── CloseIcon (Image)
   │   └── ScrollView (ScrollRect)
   │       ├── Viewport (RectTransform + Mask)
   │       │   └── Content (VerticalLayoutGroup + ContentSizeFitter)
   │       │       ├── ItemImage (RawImage)
   │       │       └── DescriptionText (TextMeshProUGUI)
   │       └── Scrollbar Vertical (optional)
   ```

3. **Root RectTransform settings:**
   - Anchor: Top-Right (1, 1)
   - Pivot: (1, 1)
   - Position: X=-20, Y=-100
   - Size: Width=350, Height=400

4. **Build the bundle:**
   - In Unity: EndstarMod > Build Popup Bundle
   - Copy `Assets/Bundles/firstpickupinfo.bundle` to `BepInEx/plugins/`

### Sprite Specifications

| Sprite | Size | Format | Description |
|--------|------|--------|-------------|
| CloseButtonIcon.png | 32x32 | PNG with transparency | White X shape, 2-3px line width |
| PanelBackground.png | 64x64 | PNG with transparency | Dark rounded rect (#1A1A1F), 9-slice borders |
| DefaultItemImage.png | 256x256 | PNG | Placeholder for missing images |

### Plugin Components

**Configuration Classes:**
```csharp
[Serializable]
public class ItemInfoConfig
{
    public string version;
    public List<ItemInfoEntry> items;
}

[Serializable]
public class ItemInfoEntry
{
    public string itemId;
    public string title;
    public string description;
    public string imagePath;
    public bool showOnFirstPickup;
}
```

**Key Methods:**
- `InitializePopupSystem()` - Loads config and bundle at startup
- `CheckFirstPickupPopup(object itemInstance)` - Called from Item_Pickup_Postfix
- `GetItemPropId(object itemInstance)` - Extracts prop ID from item
- `ShowInfoPopup(ItemInfoEntry entry)` - Instantiates and displays popup
- `HideInfoPopup()` - Closes popup with fade animation
- `LoadAndSetItemImage(string imagePath)` - Loads image from file

**Coroutines:**
- `FadeInPopup()` - 0.25s alpha fade in
- `FadeOutAndDestroy()` - 0.2s alpha fade out, then destroy

### Logging Tags

All popup system logs use the `[POPUP]` tag for easy filtering:
- `[POPUP] Initializing popup system...`
- `[POPUP] Loaded config with N items`
- `[POPUP] Checking first pickup for prop: X`
- `[POPUP] First pickup of X, showing popup`
- `[POPUP] Already collected X this session, skipping popup`

### Testing Procedure

1. Create test ItemInfoConfig.json with your custom prop's ID
2. Add corresponding image to ItemImages folder
3. Build and deploy the asset bundle
4. Launch game
5. Place custom prop in world
6. Pick it up - popup should appear
7. Close popup with X button
8. Pick up same prop type again - no popup (session tracking)
9. Restart game - popup appears again (session reset)

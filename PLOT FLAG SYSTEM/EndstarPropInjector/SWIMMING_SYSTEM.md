# Swimming System Documentation v10.23.0

## Related Documentation
- **[ITEM_INVENTORY_SYSTEM.md](ITEM_INVENTORY_SYSTEM.md)** - Item pickup and inventory system

## Overview

The EndstarPropInjector plugin implements a swimming system for the Endstar game using BepInEx 5.4.23.2 and HarmonyLib for runtime method patching.

**Current Status:** Fully functional swimming system using the game's native (but unused) swimming animations extracted from the AnimatorController.

---

## Native Swimming System Discovery

The game contains a **complete but unused swimming system** in the AnimatorController (`Character_Base_Animator`). This was discovered by extracting and analyzing the animator asset.

### Source File
```
D:\Endstar Plot Flag\PLOT FLAG SYSTEM\EndstarPropInjector\Animator\Character_Base_Animator-sharedassets0.assets-1620.txt
```

### Evidence
- 16 animator layers
- Complete "Swimming" sub-state machine in Base Layer
- Water-related parameters (WaterIn, WaterEnter, WaterDive, etc.)
- Swimming animation clips (Water_Above_*, Water_Under_*)
- AnyState transitions to swimming states

---

## Animator Controller Data

### Swimming State Hashes

| State Name | Hash (Unsigned) | Hash (Signed) | Purpose |
|------------|-----------------|---------------|---------|
| Enter Water | 3061384307 | -1233582989 | Entry from surface |
| Enter Water High | 2940148679 | -1354818617 | Entry from height/fall |
| Above Water Idle | 1260217858 | 1260217858 | Floating at surface |
| Above Water Swim Start | 1966917648 | 1966917648 | Start swimming at surface |
| Above Water Swim Stop | 697086002 | 697086002 | Stop swimming at surface |
| Above Water Swim Blendtree | 3747168042 | -547799254 | Surface movement blendtree |
| Dive | 914991998 | 914991998 | Diving animation |
| Below Water Idle | 1443112800 | 1443112800 | Underwater idle |
| Below Water Swim Start | 1820082062 | 1820082062 | Start swimming underwater |
| Below Water Swim Stop | 88956836 | 88956836 | Stop swimming underwater |
| Below Water Swim Blendtree | 2791254829 | -1503712467 | Underwater movement blendtree |
| Breach | 940887619 | 940887619 | Rising to surface animation |

### Parameter Hashes

| Parameter | Hash (Unsigned) | Hash (Signed) | Type | Purpose |
|-----------|-----------------|---------------|------|---------|
| WaterIn | 1691914667 | 1691914667 | Bool | Player is in water |
| WaterInBelow | 86030555 | 86030555 | Bool | Player is underwater |
| WaterBelow | 119971305 | 119971305 | Bool | Water exists below player |
| WaterEnter | 4206307381 | -88659915 | Trigger | Fired when entering water |
| WaterExit | 973649254 | 973649254 | Trigger | Fired when exiting water |
| WaterDive | 951727400 | 951727400 | Trigger | Fired when diving |
| WaterBreach | 3275916513 | -1019050783 | Trigger | Fired when breaching surface |

### Swimming Layer Structure

```
Base Layer.Swimming
├── Entry
│   ├── → Enter Water (WaterEnter trigger, FallTime < 1)
│   ├── → Enter Water High (WaterEnter trigger, FallTime > 1)
│   ├── → Dive (WaterDive trigger)
│   └── → Breach (WaterBreach trigger)
│
├── Enter Water
│   ├── → Above Water Idle
│   ├── → Below Water Swim Start
│   └── → Below Water Idle
│
├── Enter Water High
│   ├── → Below Water Idle
│   └── → Below Water Swim Start
│
├── Above Water Idle
│   ├── → Above Water Swim Start (movement)
│   ├── → Dive (WaterDive trigger)
│   ├── → Below Water Idle
│   └── → Exit
│
├── Above Water Swim Start
│   ├── → Above Water Swim Blendtree
│   ├── → Above Water Swim Stop
│   ├── → Dive (WaterDive trigger)
│   └── → Exit
│
├── Above Water Swim Blendtree
│   ├── → Above Water Swim Stop
│   └── → Exit
│
├── Above Water Swim Stop
│   ├── → Above Water Idle
│   ├── → Above Water Swim Start
│   ├── → Breach
│   └── → Exit
│
├── Dive
│   ├── → Below Water Idle
│   └── → Below Water Swim Start
│
├── Below Water Idle
│   ├── → Below Water Swim Start (movement)
│   ├── → Above Water Idle
│   ├── → Breach (WaterBreach trigger)
│   └── → Exit
│
├── Below Water Swim Start
│   ├── → Below Water Swim Blendtree
│   └── → Below Water Swim Stop
│
├── Below Water Swim Blendtree
│   ├── → Below Water Swim Stop
│   └── → Exit
│
├── Below Water Swim Stop
│   ├── → Below Water Idle
│   ├── → Below Water Swim Start
│   ├── → Breach
│   └── → Exit
│
└── Breach
    ├── → Above Water Idle
    ├── → Above Water Swim Start
    └── → Exit
```

### Animation Clips (FBX Sources)

```
Assets/Art/Characters/Faction_Terrans_Characters/BASE_ANIMATIONS/Water/AllFactionTests_02_Swimming_Unarmed_01.fbx
├── Water_Above_Start_R
├── Water_Above_Start_L
├── Water_Above_Forward_Start
├── Water_Above_Forward_Loop
├── Water_Above_F_45_R
├── Water_Above_F_45_L
├── Water_Under_Start_R
├── Water_Under_Start_L
├── Water_Under_Start_UP
├── Water_Under_Start_DOWN
├── Water_Under_Forward_Start
├── Water_Under_Forward_Loop
├── Water_Under_Swim_Down_90
├── Water_Under_Swim_UP_90
├── Water_Forward_45_R
├── Water_Forward_45_L
├── Water_Forward_UP
└── Water_Forward_DOWN
```

---

## Implementation (v10.17.0)

### Animation Control Flow

```
┌─────────────────────────────────────────────────────────────────┐
│                    SetAnimationState_Prefix                      │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  ┌──────────────┐     ┌──────────────┐     ┌──────────────┐    │
│  │ NOT IN WATER │────►│ ENTERING     │────►│ IN WATER     │    │
│  │              │     │ WATER        │     │              │    │
│  │ _wasInWater  │     │              │     │ _wasInWater  │    │
│  │ = false      │     │ Fire:        │     │ = true       │    │
│  │              │     │ WaterEnter   │     │              │    │
│  │ Set:         │     │ WaterIn=true │     │ Continuous:  │    │
│  │ WaterIn=false│     │ WaterBelow   │     │ WaterIn=true │    │
│  │ WaterExit    │     │ =true        │     │ WaterBelow   │    │
│  │ trigger      │     │              │     │ =true        │    │
│  └──────────────┘     └──────────────┘     └──────────────┘    │
│                                                    │            │
│                              ┌─────────────────────┘            │
│                              ▼                                  │
│         ┌────────────────────────────────────────┐             │
│         │         DEPTH CHECK                     │             │
│         │    isUnderwater = depth > 1.5m         │             │
│         └────────────────────────────────────────┘             │
│                    │                    │                       │
│            ┌───────┴───────┐    ┌───────┴───────┐              │
│            ▼               ▼    ▼               ▼              │
│    ┌──────────────┐  ┌──────────────┐                          │
│    │ DIVING       │  │ BREACHING    │                          │
│    │ (underwater) │  │ (surface)    │                          │
│    │              │  │              │                          │
│    │ Fire:        │  │ Fire:        │                          │
│    │ WaterDive    │  │ WaterBreach  │                          │
│    │ CrossFade to │  │ CrossFade to │                          │
│    │ HASH_DIVE    │  │ HASH_BREACH  │                          │
│    │ WaterInBelow │  │ WaterInBelow │                          │
│    │ = true       │  │ = false      │                          │
│    └──────────────┘  └──────────────┘                          │
│                                                                  │
│         ┌────────────────────────────────────────┐             │
│         │       MOVEMENT CHECK                    │             │
│         │  isMoving = horizontalVel > 0.1        │             │
│         └────────────────────────────────────────┘             │
│                    │                                            │
│            ┌───────┴───────┐                                   │
│            ▼               │                                    │
│    ┌──────────────┐        │                                   │
│    │ SWIMMING     │        │                                   │
│    │ (moving)     │        │                                   │
│    │              │        │                                   │
│    │ CrossFade to │        │                                   │
│    │ SWIM_START   │        │                                   │
│    │ (above/below │        │                                   │
│    │ based on     │        │                                   │
│    │ depth)       │        │                                   │
│    └──────────────┘        │                                   │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

### Code Implementation

```csharp
// v10.17.0: COMPREHENSIVE NATIVE SWIMMING SYSTEM
// Uses the game's built-in swimming animations via triggers and state hashes

bool isInWater = IsSwimmingActive;

if (!isInWater)
{
    // === EXITING WATER ===
    if (_wasInWater && _cachedAnimator != null)
    {
        _cachedAnimator.SetBool("WaterIn", false);
        _cachedAnimator.SetBool("WaterInBelow", false);
        _cachedAnimator.SetBool("WaterBelow", false);
        _cachedAnimator.SetTrigger("WaterExit");
    }
    _wasInWater = false;
    _wasUnderwater = false;
    return;
}

// === IN WATER ===
grounded = false;
walking = false;

float diveDepth = UnderwaterDepth;
const float DIVE_THRESHOLD = 1.5f;
bool isUnderwater = diveDepth > DIVE_THRESHOLD;
bool isMoving = horizontalVelMagnitude > 0.1f;

if (_cachedAnimator != null)
{
    // === WATER ENTRY ===
    if (!_wasInWater)
    {
        _cachedAnimator.SetBool("WaterIn", true);
        _cachedAnimator.SetBool("WaterBelow", true);
        _cachedAnimator.SetTrigger("WaterEnter");

        if (isUnderwater)
        {
            _cachedAnimator.SetBool("WaterInBelow", true);
            _cachedAnimator.SetTrigger("WaterDive");
        }
        _wasInWater = true;
        _wasUnderwater = isUnderwater;
    }
    else
    {
        // === CONTINUOUS STATE ===
        _cachedAnimator.SetBool("WaterIn", true);
        _cachedAnimator.SetBool("WaterBelow", true);
        _cachedAnimator.SetBool("WaterInBelow", isUnderwater);

        // === DIVE/BREACH ===
        if (isUnderwater != _wasUnderwater)
        {
            if (isUnderwater)
            {
                _cachedAnimator.SetTrigger("WaterDive");
                _cachedAnimator.CrossFadeInFixedTime(HASH_DIVE, 0.3f, 0);
            }
            else
            {
                _cachedAnimator.SetTrigger("WaterBreach");
                _cachedAnimator.CrossFadeInFixedTime(HASH_BREACH, 0.3f, 0);
            }
            _wasUnderwater = isUnderwater;
        }

        // === MOVEMENT ===
        if (isMoving != _isMovingInWater)
        {
            if (isMoving)
            {
                int swimStartHash = isUnderwater ?
                    HASH_BELOW_WATER_SWIM_START : HASH_ABOVE_WATER_SWIM_START;
                _cachedAnimator.CrossFadeInFixedTime(swimStartHash, 0.2f, 0);
            }
            _isMovingInWater = isMoving;
        }
    }
}
```

---

## Core Constants

```csharp
private const float SURFACE_MARGIN = 0.1f;      // Buffer above water surface
private const float SUBMERSION_DEPTH = 0.5f;    // How deep waist stays below surface
private const float JUMP_FORCE = 2.5f;          // Force applied when jumping out
private const float TELEPORT_THRESHOLD = 5f;    // Position delta for teleport detection
private const float AUTO_SINK_FORCE = -2f;      // Downward force above swim ceiling
private const int EXIT_GRACE_DURATION = 30;     // Frames of death protection after exit
private const int JUMP_IMMUNITY_DURATION = 15;  // Frames to ignore re-entry after jump
private const float DIVE_THRESHOLD = 1.5f;      // Depth threshold for underwater state

// v10.19.0: Smooth animation transitions
private const float VELY_LERP_SPEED = 3f;       // Lerp speed for velY smoothing

// v10.22.0: Underwater effect settings
private const float UNDERWATER_BLUR_INTENSITY = 0.35f;  // Blur amount (0 = none, 1 = full)
private static readonly Color UNDERWATER_TINT = new Color(0.4f, 0.7f, 0.9f, 1f);  // Ocean blue
```

---

## Post-Processing Volume System (v10.21.0+)

### Overview

The game uses Unity's Volume system for post-processing effects. When underwater, a local volume applies visual effects. The plugin selectively modifies only the underwater volume while keeping global volumes intact.

### Volumes in Scene (Runtime Discovery)

| Volume Name | Type | Profile | Effects |
|-------------|------|---------|---------|
| **Under Water Volume** | LOCAL | Underwater Volume | Vignette, ColorAdjustments, DepthOfField |
| **Bloom** | GLOBAL | Bloom Profile | Bloom |
| **Volumetric - Early Mid Morning** | GLOBAL | ButoVolumetricFog | Volumetric fog |
| **Color Correction** | GLOBAL | Tonemapping | Tonemapping |

### Underwater Volume Effects (Original Values)

| Effect | Parameter | Value |
|--------|-----------|-------|
| Vignette | intensity | 0.981 |
| Vignette | smoothness | 0.499 |
| ColorAdjustments | postExposure | 0.1 |
| DepthOfField | mode | Bokeh |

### Plugin Modifications (v10.22.0)

When entering water, the plugin:
1. **Reduces blur intensity** - Sets underwater volume weight to 0.35 (was 1.0)
2. **Applies ocean blue tint** - Sets ColorAdjustments.colorFilter to `(0.4, 0.7, 0.9, 1)`
3. **Preserves global volumes** - Bloom, Volumetric, Color Correction remain at full weight

When exiting water:
- Restores original volume weight
- Restores original colorFilter value

### Volume Hierarchy Path

```
Ocean Plane(Clone)
└── Depth Plane(Clone)
    └── Runtime Plane Offset
        └── Plane Parent
            └── Ocean
                └── Under Water Volume  ← LOCAL volume attached here
```

### Implementation

```csharp
// v10.21.0: Selective volume modification
private static void TryReduceBlur()
{
    foreach (var volume in allVolumes)
    {
        string volName = volume.name.ToLower();

        // Only modify underwater-related volumes
        if (volName.Contains("under") && volName.Contains("water"))
        {
            // Set blur intensity
            weightField.SetValue(volume, UNDERWATER_BLUR_INTENSITY);

            // v10.22.0: Apply ocean blue tint
            ApplyUnderwaterColorTint(volume);
        }
        // Keep other volumes (Bloom, Volumetric, Color Correction) intact
    }
}

// v10.22.0: Apply color tint via ColorAdjustments
private static void ApplyUnderwaterColorTint(object volume)
{
    var profile = volume.profile;
    foreach (var component in profile.components)
    {
        if (component is ColorAdjustments)
        {
            // Save original, apply tint
            _originalColorFilter = colorAdjustments.colorFilter.value;
            colorAdjustments.colorFilter.value = UNDERWATER_TINT;
            colorAdjustments.colorFilter.overrideState = true;
        }
    }
}
```

---

## Verified Game Values

| Value | Amount | Source |
|-------|--------|--------|
| Water Surface Y | -0.80 | `DepthPlane.planeParent.position.y` |
| Death Zone Y | -1.80 | `Stage.StageFallOffHeight` |
| KillOffset | -1.00 | `DepthPlaneInfo.KillOffset` |
| Character Height | 1.30 | `CharacterController.height` |
| Character Radius | 0.18 | `CharacterController.radius` |
| Waist Offset | 0.47 | `height * 0.5 - radius` |

---

## Plugin State Machine

```
SURFACE ──────► ENTERING ──────► SWIMMING ──────► EXITING ──────► SURFACE
   │                                  │               │
   │                                  │               │
   └──── (waist < surface) ───────────┘               │
                                                      │
                              (waist < surface) ◄─────┘
                              (if no jump immunity)
```

| State | Condition | IsSwimmingActive | Death Blocked |
|-------|-----------|------------------|---------------|
| SURFACE | Normal gameplay above water | false | NO |
| ENTERING | Waist below surface, feet above | false | YES |
| SWIMMING | Feet below surface | true | YES |
| EXITING | Left water, grace period active | false | YES |

---

## Key Patches

### 1. ProcessFallOffStage_Prefix
**Target:** `PlayerController.ProcessFallOffStage_NetFrame`
- Runs state machine logic
- Blocks death in ENTERING, SWIMMING, EXITING states
- Gets water surface from `DepthPlane.planeParent.position.y`

### 2. ProcessPhysics_Postfix_Swimming
**Target:** `PlayerController.ProcessPhysics_NetFrame`
- Applies swimming physics (ghost mode pattern)
- Neutralizes gravity, controls vertical movement
- Enforces swim ceiling

### 3. ProcessJump_Prefix
**Target:** `PlayerController.ProcessJump_NetFrame`
- Handles Space key jump from water surface
- Applies jump force, transitions to EXITING

### 4. SetAnimationState_Prefix
**Target:** `AppearanceAnimator.SetAnimationState`
- **v10.17.0:** Comprehensive native swimming animation control
- Fires WaterEnter/WaterExit/WaterDive/WaterBreach triggers
- Uses CrossFadeInFixedTime for state transitions
- Manages WaterIn/WaterInBelow/WaterBelow parameters

### 5. DepthPlane_Start_Postfix
**Target:** `DepthPlane.Start`
- Reduces underwater blur effects

### 6. StartTransition_Prefix
**Target:** `Filter.StartTransition`
- Blocks blurred filter (type 3)

---

## Controls

| Key | Action |
|-----|--------|
| Q | Swim up (increase motor) |
| E | Swim down (decrease motor) |
| Space | Jump out of water (when near surface) |
| WASD | Horizontal movement (uses swim blendtrees) |

---

## Log Tags

| Tag | Purpose |
|-----|---------|
| `[SWIM-ANIM]` | Animation state changes and trigger fires |
| `[SWIM-STATE]` | Periodic state logging (every 100 frames) |
| `[ANIM-DIAG]` | Detailed animator diagnostics |
| `[DIVE]` | Dive/breach transition events |
| `[VOLUME-DIAG]` | Post-processing volume discovery and analysis |
| `[BLUR]` | Volume weight modifications |
| `[UNDERWATER]` | Underwater effect changes (color tint, blur) |

---

## Files

| File | Purpose |
|------|---------|
| `Plugin.cs` | Main plugin source |
| `SWIMMING_SYSTEM.md` | This documentation |
| `Animator/Character_Base_Animator-*.txt` | Extracted AnimatorController data |
| `EndstarPropInjector.dll` | Compiled plugin |
| `LogOutput.log` | Runtime logs |

---

## Version History

| Version | Changes |
|---------|---------|
| v10.0.0 | State machine implementation |
| v10.1.0 | Waist-based detection, teleport detection |
| v10.2.0 | Auto-sink force, animation parameter manipulation |
| v10.3.0 | Jump immunity, Space key jump |
| v10.4.x | Various fixes and diagnostics |
| v10.12.0 | Comprehensive animator diagnostics |
| v10.13.x | Direct animator.Play() attempts |
| v10.14.0 | Runtime hash capture + CrossFadeInFixedTime |
| v10.15.0 | Diagnostic build for data collection |
| v10.16.0 | Data-driven hashes from AnimatorController extraction |
| v10.17.0 | **Comprehensive native swimming system** - full integration with game's unused swimming animations using triggers and CrossFade |
| v10.19.0 | **Smooth animation transitions** - added velY lerping (VELY_LERP_SPEED = 3f) for smooth dive/rise pose transitions |
| v10.20.0 | Added comprehensive Volume diagnostic logging (LogAllVolumeData) |
| v10.21.0 | **Fixed flat scene bug** - selective volume modification (only underwater volume, keep Bloom/Volumetric/ColorCorrection intact) |
| v10.22.0 | **Ocean blue tint** - added ColorAdjustments.colorFilter modification, increased blur to 0.35 |
| v10.23.0 | **Item pickup diagnostics** - added patches for Item.Pickup, Inventory.AttemptPickupItem, ItemInteractable; captured item/inventory system data |

---

## Technical Notes

### Hash Conversion
State hashes > 2147483647 (int.MaxValue) require unchecked cast:
```csharp
private static readonly int HASH_ENTER_WATER = unchecked((int)3061384307);
```

### Animator Condition Modes
From AnimatorController transitions:
- `1` = If (bool true) / Trigger consumed
- `2` = If Not (bool false)
- `3` = Greater (float)
- `4` = Less (float)
- `5` = Equals (int)
- `6` = NotEquals (int)

### AnyState Transitions
The swimming layer uses AnyState transitions triggered by:
- `WaterEnter` → Enter Water / Enter Water High (based on FallTime)
- `WaterDive` → Dive
- `WaterBreach` → Breach

### Smooth Animation Transitions (v10.19.0)

The `velY` parameter controls dive/rise animations in the blendtree. Direct assignment caused jerky transitions. Solution: smooth lerping.

```csharp
// v10.19.0: Smooth velY transitions
private const float VELY_LERP_SPEED = 3f;

// In SetAnimationState_Prefix:
float targetVelY = ... // based on vertical velocity
float currentVelY = _cachedAnimator.GetFloat("velY");
float smoothedVelY = Mathf.Lerp(currentVelY, targetVelY, Time.deltaTime * VELY_LERP_SPEED);
_cachedAnimator.SetFloat("velY", smoothedVelY);
```

This provides smooth pose blending when:
- Diving down (velY decreases smoothly)
- Rising up (velY increases smoothly)
- Transitioning to horizontal swim (velY approaches 0)

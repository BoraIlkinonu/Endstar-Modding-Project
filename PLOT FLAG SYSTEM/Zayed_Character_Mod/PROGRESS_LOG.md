# Zayed Character Mod - Progress Log

## 2026-01-09: ANIMATION SYSTEM BREAKTHROUGH

### Summary
Successfully implemented custom animation playback on Endstar characters using legacy AnimationClips and SampleAnimation().

### Technical Details

#### The Problem
- Endstar uses Unity's Animator system with Mecanim
- Custom animations from external sources (FBX) couldn't be played directly
- Initial attempts with Animator.Play() failed due to controller state machine
- Non-legacy clips don't work with SampleAnimation()

#### The Solution
1. **Legacy Clips**: Animation clips must have `legacy=true` flag set in Unity
2. **SampleAnimation()**: Use `AnimationClip.SampleAnimation(gameObject, time)` instead of Animator
3. **Disable Animator**: Must disable the game's Animator component during custom playback
4. **Correct Root**: Animation root must be the parent of the rig (e.g., `CharacterCosmetics_ThePack_Felix_01_A(Clone)`)

#### Plugin Architecture (AnimationInjector v7.0.0)

```
AnimationInjector.dll (43008 bytes)
├── Plugin.cs
│   ├── Plugin : BaseUnityPlugin
│   │   └── Loads bundle, subscribes to sceneLoaded
│   └── AnimationRunner : MonoBehaviour
│       ├── Bundle Loading
│       │   └── Loads clips from generic_animations.bundle
│       ├── Character Detection
│       │   └── Finds AppearanceAnimator -> Animator -> Animation Root
│       ├── Bone Cache
│       │   └── Maps all bone paths under animation root
│       ├── Playback System
│       │   ├── Disables game Animator
│       │   ├── Calls SampleAnimation() in LateUpdate
│       │   └── Restores poses on stop
│       └── Diagnostic Logging
│           ├── Clip properties (legacy, length, frameRate, etc.)
│           ├── Animator state
│           ├── SkinnedMeshRenderer details
│           ├── Bone movement tracking
│           └── Key press logging
```

#### Key Findings

1. **Character Hierarchy**:
   ```
   PlayerAppearance(Clone)
   └── Player Character Appearance Base(Clone)
       └── CharacterCosmetics_ThePack_Felix_01_A(Clone)  <-- Animation Root
           ├── CharacterCosmetics_ThePack_Felix_01_A
           ├── ThePack_Felix_VC_01
           │   └── Armature
           │       └── Rig.Pelvis  <-- Bone hierarchy starts here
           │           ├── Rig.Spine.01
           │           │   └── ... (full skeleton)
           │           ├── Rig.Thigh.L
           │           └── Rig.Thigh.R
           ├── TigerGuy_LOD0
           ├── TigerGuy_LOD1
           └── TigerGuy_LOD2
   ```

2. **Animation Clips** (from generic_animations.bundle):
   - `Run To Dive_Generic` - 1.20s - Jump into water
   - `Swimming_Generic` - 3.23s - Forward swimming
   - `Treading Water_Generic` - 2.13s - Idle in water

3. **Bone Paths**: Clips target paths like `ThePack_Felix_VC_01/Armature/Rig.Pelvis/...`

#### Bundle Creation (Unity Side)

1. Export animations from FBX with Generic rig type
2. In Unity, set clip's Animation Type to "Legacy"
3. Build AssetBundle containing the clips
4. Place bundle in `BepInEx/plugins/generic_animations.bundle`

### Files Modified/Created

| File | Purpose |
|------|---------|
| `AnimationInjector/Plugin.cs` | Main plugin with playback and diagnostics |
| `generic_animations.bundle` | AssetBundle with legacy animation clips |

### Validation Results

- Clips load: YES (legacy=True)
- Animation root found: YES
- Bones move when sampled: YES
- Visual playback: YES

### Next Steps

1. Implement swimming system with water detection
2. Map animations to player input
3. Create modified water plane without blur/health effects
4. Add Y-axis movement (diving)

---

## Technical Reference

### How to Play Custom Animations

```csharp
// 1. Find the animation root (parent of rig)
Transform animRoot = FindAnimationRoot();

// 2. Disable game's animator
Animator gameAnimator = ...;
gameAnimator.enabled = false;

// 3. Sample animation in LateUpdate
void LateUpdate() {
    playbackTime += Time.deltaTime;
    if (playbackTime > clip.length)
        playbackTime %= clip.length;
    clip.SampleAnimation(animRoot.gameObject, playbackTime);
}

// 4. Re-enable animator when done
gameAnimator.enabled = true;
```

### Bundle Path
```
C:\Endless Studios\Endless Launcher\Endstar\BepInEx\plugins\generic_animations.bundle
```

### Log Location
```
C:\Endless Studios\Endless Launcher\Endstar\BepInEx\LogOutput.log
```

---

## 2026-01-09: SWIMMING SYSTEM IMPLEMENTATION

### Summary
Built complete swimming system plugin that:
- Detects water zones and triggers swimming mode
- Maps WASD to XZ movement, Q/E to dive/surface (Y axis)
- Plays appropriate animations (dive entry, swimming, treading)
- Removes underwater blur while keeping water visuals
- Works only in gameplay mode (not Creator)

### Architecture

```
SwimmingSystem.dll
├── Plugin.cs              - BepInEx entry, loads animations
├── SwimmingManager.cs     - Main coordinator
├── SwimmingController.cs  - Movement & input handling
├── SwimAnimator.cs        - Animation state machine
├── WaterZoneDetector.cs   - Water trigger detection
└── SwimConfig.cs          - Configuration constants
```

### Component Details

#### SwimmingManager
- Finds player via AppearanceAnimator reflection
- Builds bone cache for animations
- Creates and coordinates other components
- Handles water enter/exit events

#### SwimmingController
- Captures WASD input for XZ movement
- Captures Q/E for diving/surfacing (Y axis)
- Camera-relative movement direction
- Applies movement via CharacterController or Transform

#### SwimAnimator
- State machine: DiveEntry → Treading ←→ Swimming
- Uses SampleAnimation() in LateUpdate
- Stores/restores original bone poses
- Disables game Animator during swimming

#### WaterZoneDetector
- Attached to water objects automatically
- Detects player via trigger collision
- Disables blur effects on camera
- Disables damage components

### Input Mapping

| Key | Action | Animation |
|-----|--------|-----------|
| W/Up | Swim forward | Swimming_Generic |
| S/Down | Swim backward | Swimming_Generic |
| A/Left | Strafe left | Swimming_Generic |
| D/Right | Strafe right | Swimming_Generic |
| Q | Dive down | Swimming_Generic |
| E | Surface up | Swimming_Generic |
| No input | Idle | Treading Water_Generic |
| Water entry | Auto | Run To Dive_Generic |
| F8 | Toggle swim (debug) | - |

### Animation State Machine

```
┌─────────────────────────────────────────────────┐
│                    SWIMMING                      │
│                                                  │
│   ┌─────────┐    movement    ┌──────────┐      │
│   │ TREADING │──────────────►│ SWIMMING │      │
│   │ (idle)   │◄──────────────│ (moving) │      │
│   └─────────┘    no input    └──────────┘      │
│        ▲                                        │
│        │ after 1.2s                             │
│   ┌─────────┐                                   │
│   │RUN_TO_  │ ◄── Water entry                  │
│   │DIVE     │                                   │
│   └─────────┘                                   │
└─────────────────────────────────────────────────┘
```

### Configuration (SwimConfig.cs)

```csharp
SwimSpeed = 4.0f        // XZ movement speed
DiveSpeed = 2.5f        // Diving speed
SurfaceSpeed = 3.0f     // Surfacing speed
RotationSpeed = 120f    // Turn speed

DiveKey = KeyCode.Q     // Dive down
SurfaceKey = KeyCode.E  // Surface up
```

### Files Created

| File | Size | Purpose |
|------|------|---------|
| `SwimmingSystem/Plugin.cs` | - | BepInEx entry point |
| `SwimmingSystem/SwimmingManager.cs` | - | Main coordinator |
| `SwimmingSystem/SwimmingController.cs` | - | Movement handling |
| `SwimmingSystem/SwimAnimator.cs` | - | Animation state machine |
| `SwimmingSystem/WaterZoneDetector.cs` | - | Water detection |
| `SwimmingSystem/SwimConfig.cs` | - | Configuration |
| `deploy/SwimmingSystem.dll` | - | Compiled plugin |

### Deployment

1. Copy `SwimmingSystem.dll` to `BepInEx/plugins/`
2. Ensure `generic_animations.bundle` is in same folder
3. Run game in gameplay mode (not Creator)
4. Enter water zone or press F8 to test

### Integration with AnimationInjector

SwimmingSystem reuses the proven animation approach from AnimationInjector:
- Same bone cache building
- Same SampleAnimation() method
- Same Animator disable/enable pattern
- Shares the generic_animations.bundle

### Next Steps

1. Test in game with actual water objects
2. Fine-tune movement speeds
3. Add water surface constraint (prevent swimming above water)
4. Add transition animations for water exit
5. Research actual Ocean Plane structure for better integration

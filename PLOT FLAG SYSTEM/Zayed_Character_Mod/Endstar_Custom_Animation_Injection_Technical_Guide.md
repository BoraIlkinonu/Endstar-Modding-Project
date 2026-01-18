# Endstar Animation Injection Project - Complete Documentation

**Project:** Custom Animation Injection for Endstar Game Characters
**Date:** January 9, 2026
**Current Plugin Version:** 6.0.0

---

## Table of Contents

1. [Project Goal](#project-goal)
2. [Problem Statement](#problem-statement)
3. [Research Phase](#research-phase)
4. [Key Discoveries](#key-discoveries)
5. [Solution Evolution](#solution-evolution)
6. [Files Created](#files-created)
7. [Technical Details](#technical-details)
8. [Current Status](#current-status)
9. [How to Use](#how-to-use)
10. [Troubleshooting](#troubleshooting)

---

## Project Goal

Inject custom Mixamo swimming animations into Endstar game characters via BepInEx plugin, allowing the player character (Felix) to perform custom animations triggered by hotkeys.

**Target Animations:**
- Swimming
- Run To Dive
- Treading Water

---

## Problem Statement

### Initial Symptoms
- Custom animations caused character to collapse/get stuck
- Bones appeared to not receive animation data
- Multiple plugin versions (v2.x through v5.x) failed

### Root Cause (Discovered)
1. **Avatar Type Mismatch**: Mixamo animations are Humanoid (use muscle curves), but Endstar's runtime character uses a Generic avatar
2. **Path Resolution Issues**: Animation curve paths must exactly match the runtime hierarchy
3. **Game Animation Override**: Game systems were overwriting bone transforms after our animation applied

---

## Research Phase

### Tools Used

1. **PowerShell Reflection** - Analyzed game DLLs in `C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed\`
2. **Unity Editor Scripts** - Created diagnostic tools to analyze FBX files and animations
3. **BepInEx Logging** - Runtime logging to understand game hierarchy

### Key DLLs Analyzed
- `Gameplay.dll` - Contains `AppearanceAnimator`, `CharacterAnimator`
- `Assets.dll` - Asset loading systems
- `UnityEngine.AnimationModule.dll` - Unity animation APIs

### Game Classes Discovered

**AppearanceAnimator** (Main animation controller)
```csharp
namespace Endless.Gameplay.CharacterAnimation
{
    public class AppearanceAnimator : MonoBehaviour
    {
        private Animator animator;  // The main Animator component
        // Controls character appearance animations
    }
}
```

---

## Key Discoveries

### Discovery 1: Humanoid vs Generic Avatar

**Character FBX (Felix):**
- Animation Type: Humanoid
- isHuman: True (in Unity Editor)
- isHuman: **False** (at Endstar runtime) - Uses Generic!

**Mixamo Animations:**
- isHumanMotion: True
- Use Humanoid muscle curves (no transform paths)
- Cannot play on Generic rigs directly

**Solution:** Convert Humanoid animations to Generic by baking transforms

### Discovery 2: Runtime Hierarchy Structure

```
Player Character Appearance Base(Clone)     <- Has Animator
  CharacterCosmetics_ThePack_Felix_01_A(Clone)
    CharacterCosmetics_ThePack_Felix_01_A   <- Animation root (FBX root)
      ThePack_Felix_VC_01                   <- Skeleton root
        Armature
          Rig.Pelvis
            Rig.Spine.01
              Rig.Spine.02
                Rig.Neck
                Rig.Shoulder.L
                Rig.Shoulder.R
                ...
```

### Discovery 3: Animation Path Requirements

For `SampleAnimation()` or Animation component to work:
- Animation component must be on `CharacterCosmetics_ThePack_Felix_01_A`
- Clip paths must be relative to that transform
- Example path: `ThePack_Felix_VC_01/Armature/Rig.Pelvis`

### Discovery 4: Bone Naming

Endstar uses custom bone names (not standard Unity Humanoid):
- `Rig.Pelvis` (not "Hips")
- `Rig.Spine.01`, `Rig.Spine.02` (not "Spine", "Chest")
- `Rig.UpperArm.L`, `Rig.Forearm.L` (not "LeftUpperArm", "LeftLowerArm")
- `Rig.Finger.Index.L.01` (finger bones)

---

## Solution Evolution

### Version 2.x-4.x (Failed)
- Attempted direct path remapping
- Tried various hierarchy detection methods
- Failed because: Humanoid clips have NO transform paths to remap

### Version 5.0.0 (Partial Success)
- Converted animations to Generic using baked transforms
- Used Animation component with `Play()`
- Result: `Animation.isPlaying = True` but bones didn't move
- Diagnosis: Something overwriting transforms after Animation applies them

### Version 5.1.0 (Debug)
- Added extensive logging
- Confirmed paths match between clips and hierarchy
- Confirmed pelvis not animating despite `isPlaying = True`

### Version 6.0.0 (Current)
- Uses `SampleAnimation()` in `LateUpdate()` for direct bone control
- Samples animation AFTER all other scripts run
- Disables ALL Animators in hierarchy
- Caches bone transforms for restoration on stop

---

## Files Created

### Unity Editor Scripts (D:/Unity_Workshop/Endstar Custom Shader/Assets/Editor/)

#### 1. AnimationReporter.cs
Generates diagnostic reports about FBX files and animations.

**Menu Items:**
- `Endstar/Reports/Full Diagnostic Report` - Comprehensive analysis
- `Endstar/Reports/Animation Path Analysis` - Curve binding details
- `Endstar/Reports/Bone Comparison` - Compare character to animation bones

**Output:** `Assets/Reports/*.txt`

#### 2. HumanoidToGenericConverter.cs
Converts Humanoid animations to Generic by baking transforms frame-by-frame.

**Menu Item:** `Endstar/Convert/Bake Humanoid to Generic`

**Process:**
1. Instantiates character prefab with Humanoid avatar
2. Plays each Humanoid animation
3. Records all bone transforms every frame
4. Creates new AnimationClip with transform curves
5. Saves to `Assets/CustomAnimations/Generic/`

**Output:** `*_Generic.anim` files with paths like:
- `ThePack_Felix_VC_01/Armature/Rig.Pelvis`
- `ThePack_Felix_VC_01/Armature/Rig.Pelvis/Rig.Spine.01`
- etc.

#### 3. GenericBundleBuilder.cs
Builds AssetBundle from Generic animation clips.

**Menu Items:**
- `Endstar/Build/Build Generic Animation Bundle`
- `Endstar/Build/Show Bundle Location`

**Output:** `Assets/CustomAnimations/Bundles/generic_animations.bundle`

### BepInEx Plugin (D:/Endstar Plot Flag/PLOT FLAG SYSTEM/Zayed_Character_Mod/AnimationInjector/)

#### Plugin.cs (v6.0.0)
Main plugin file with two classes:

**Plugin class:**
- Loads on game start
- Sets up bundle path
- Subscribes to scene loading
- Creates AnimationRunner on scene load

**AnimationRunner class:**
- Loads animation bundle
- Finds character hierarchy at runtime
- Handles hotkey input
- Samples animations in LateUpdate

**Hotkeys:**
- `Numpad1` - Play next animation
- `Numpad2` - Play previous animation
- `Numpad3` - Stop animation
- `Numpad0` - Log full hierarchy

### Generated Files

#### Generic Animation Clips (Assets/CustomAnimations/Generic/)
- `Swimming_Generic.anim` (3.23s, 111 curves, 27 bones)
- `Run To Dive_Generic.anim` (1.20s, 111 curves, 27 bones)
- `Treading Water_Generic.anim` (2.13s, 111 curves, 27 bones)

#### Asset Bundle
- `generic_animations.bundle` - Contains all 3 Generic clips
- Location: `BepInEx/plugins/generic_animations.bundle`

#### Reports (Assets/Reports/)
- `full_diagnostic.txt` - Character and animation analysis
- `verify_generic.txt` - Verification of converted clips

---

## Technical Details

### Animation Conversion Process

```
Humanoid Animation (Mixamo)
    |
    | Uses muscle curves (no paths)
    | isHumanMotion = True
    v
HumanoidToGenericConverter
    |
    | 1. Play on Humanoid character
    | 2. Record bone transforms each frame
    | 3. Create transform curves
    v
Generic Animation
    |
    | Has transform paths
    | isHumanMotion = False
    | 111 curves per clip
    v
AssetBundle
    |
    | BuildPipeline.BuildAssetBundles()
    | StandaloneWindows64
    v
BepInEx Plugin
    |
    | AssetBundle.LoadFromFile()
    | SampleAnimation() in LateUpdate
    v
Character bones animated
```

### Plugin Architecture (v6.0.0)

```csharp
// Initialization
Awake() -> Subscribe to SceneManager.sceneLoaded
OnSceneLoaded() -> Create AnimationRunner GameObject

// Runtime loop
Update():
  - Check hotkeys (Numpad1/2/3/0)
  - Find animation root if not found

LateUpdate():
  - If playing: SampleAnimation(clip, time)
  - Runs AFTER all game scripts

// Key methods
FindAnimationRoot():
  - Find AppearanceAnimator via reflection
  - Get Animator from private field
  - Search for transform with ThePack_Felix_VC_01 as child

PlayClip():
  - Set clip.legacy = true
  - Disable all Animators
  - Start playback timer

SampleAndApplyAnimation():
  - clip.SampleAnimation(root.gameObject, time)
```

### Path Resolution

Animation component/SampleAnimation resolves paths relative to the GameObject it's called on:

```
Animation Root: CharacterCosmetics_ThePack_Felix_01_A
Clip Path: ThePack_Felix_VC_01/Armature/Rig.Pelvis

Resolution:
CharacterCosmetics_ThePack_Felix_01_A
  └── ThePack_Felix_VC_01        <- First path segment
        └── Armature             <- Second path segment
              └── Rig.Pelvis     <- Target bone (receives transform data)
```

---

## Current Status

### What Works
- Generic animations created and verified in Unity Editor
- Bundle loads correctly at runtime (3 clips found)
- Hierarchy detection finds correct animation root
- Path verification passes (`ThePack_Felix_VC_01/Armature/Rig.Pelvis` exists)
- Hotkeys detected and processed

### What's Being Tested
- Plugin v6.0.0 with `SampleAnimation()` in `LateUpdate()`
- Direct bone control to override game animation systems

### Known Issues
- Previous versions: Animation.isPlaying=True but bones don't move
- Suspected cause: Game systems resetting bones between frames
- v6.0.0 solution: Apply animation last via LateUpdate

---

## How to Use

### Setup (One-time)

1. **In Unity Editor:**
   ```
   1. Place Mixamo animations in Assets/CustomAnimations/Humanoid/
   2. Run: Endstar -> Convert -> Bake Humanoid to Generic
   3. Run: Endstar -> Build -> Build Generic Animation Bundle
   4. Copy bundle from Assets/CustomAnimations/Bundles/generic_animations.bundle
   ```

2. **Deploy to Game:**
   ```
   Copy files to: C:\Endless Studios\Endless Launcher\Endstar\BepInEx\plugins\
   - AnimationInjector.dll
   - generic_animations.bundle
   ```

### Runtime Usage

1. Launch Endstar
2. Load into the game with a character
3. Wait for `[Runner] Animation root found` in logs
4. Press hotkeys:
   - `Numpad1` - Play/Next animation
   - `Numpad2` - Previous animation
   - `Numpad3` - Stop and restore original pose
   - `Numpad0` - Log hierarchy (debug)

### Checking Logs

Log file: `C:\Endless Studios\Endless Launcher\Endstar\BepInEx\LogOutput.log`

**Success indicators:**
```
[Runner] Initialized - Direct Bone Control v6.0
[Runner] Found 3 clips
[Runner] Animation root found: CharacterCosmetics_ThePack_Felix_01_A
[Runner] ThePack_Felix_VC_01 found as child - PATHS WILL MATCH!
[Runner] GOOD: 'ThePack_Felix_VC_01/Armature/Rig.Pelvis' exists in cache!
[Runner] SUCCESS! Pelvis is animating!
```

**Failure indicators:**
```
[Runner] BAD: '...' NOT in cache!
[Runner] Pelvis NOT animating!
[Runner] Manual sample had no effect - path mismatch likely!
```

---

## Troubleshooting

### Animation doesn't play

1. Check bundle is in plugins folder
2. Check logs for "Found X clips"
3. Verify character is loaded (wait for "Animation root found")

### Character collapses/T-poses

1. Old plugin version - rebuild and redeploy
2. Wrong animation root - check hierarchy logs
3. Path mismatch - verify Generic clips have correct paths

### Pelvis NOT animating

1. Path mismatch between clips and runtime hierarchy
2. Run `Numpad0` to log hierarchy and compare to clip paths
3. Regenerate Generic clips if hierarchy changed

### Bundle not found

1. Verify `generic_animations.bundle` is in `BepInEx/plugins/`
2. Check path in logs matches actual location

---

## File Locations Summary

| File | Location |
|------|----------|
| Plugin Source | `D:/Endstar Plot Flag/PLOT FLAG SYSTEM/Zayed_Character_Mod/AnimationInjector/Plugin.cs` |
| Plugin DLL | `C:/Endless Studios/Endless Launcher/Endstar/BepInEx/plugins/AnimationInjector.dll` |
| Animation Bundle | `C:/Endless Studios/Endless Launcher/Endstar/BepInEx/plugins/generic_animations.bundle` |
| Unity Scripts | `D:/Unity_Workshop/Endstar Custom Shader/Assets/Editor/` |
| Generic Clips | `D:/Unity_Workshop/Endstar Custom Shader/Assets/CustomAnimations/Generic/` |
| Reports | `D:/Unity_Workshop/Endstar Custom Shader/Assets/Reports/` |
| BepInEx Logs | `C:/Endless Studios/Endless Launcher/Endstar/BepInEx/LogOutput.log` |

---

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 2.x-4.x | Jan 2026 | Initial attempts with path remapping (failed) |
| 5.0.0 | Jan 9, 2026 | Generic animations, Animation.Play() approach |
| 5.1.0 | Jan 9, 2026 | Added debug logging for hierarchy |
| 6.0.0 | Jan 9, 2026 | SampleAnimation() in LateUpdate, direct bone control |

---

## Next Steps (If v6.0.0 Fails)

1. **Investigate what resets bones** - Add logging to identify interfering scripts
2. **Try Playables API** - More modern animation system, may bypass issues
3. **Hook game animation directly** - Intercept AppearanceAnimator methods
4. **Alternative: Harmony patches** - Patch game's animation update to inject our data

---

*Documentation generated: January 9, 2026*

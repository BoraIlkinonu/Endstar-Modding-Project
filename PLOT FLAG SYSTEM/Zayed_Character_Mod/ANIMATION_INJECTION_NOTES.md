# Endstar Animation Injection - Research & Development Notes

## Project Goal
Inject custom animations (from Mixamo) into Endstar characters at runtime via BepInEx plugin.

---

## Key Discoveries

### 1. Character Animation Architecture

**Hierarchy Structure:**
```
Player Character Appearance Base(Clone)
  └── CharacterCosmetics_ThePack_Felix_01_A(Clone)
        ├── CharacterCosmetics_ThePack_Felix_01_A
        └── ThePack_Felix_VC_01
              └── Armature
                    ├── Rig.Camera
                    ├── Rig.Pelvis
                    └── Rig.Root
        ├── TigerGuy_LOD0
        ├── TigerGuy_LOD1
        └── TigerGuy_LOD2
```

**Key Finding:** Endstar uses bone names with `Rig.` prefix (e.g., `Rig.Pelvis`, `Rig.Root`), NOT standard Mixamo names (`mixamorig:Hips`, `mixamorig:Spine`).

**Avatar Info:**
- Avatar Name: `Terran_Test_01Avatar`
- Rig Type: **Generic** (NOT Humanoid)
- `Animator.isHuman`: **False**

### 2. Animation System
- Endstar uses **Mecanim** (modern Unity animation system), NOT Legacy Animation
- The main AnimatorController is `Character_Base_Animator` with 60+ parameters and 369 clips
- AnimatorController is applied at runtime to character cosmetics
- Character bundles (like Pearl Diver/Zayed) contain ONLY mesh/textures - NO AnimatorController

### 3. Animator Location
- Found on: `Player Character Appearance Base(Clone)`
- Controller name: `Character_Base_Animator`
- There are 2 Animators in the hierarchy (1 parent, 1 child on cosmetics)

---

## Approaches Tried

### Approach 1: Legacy Animation System
**Implementation:**
- Load AnimationClips from AssetBundle
- Set `clip.legacy = true`
- Add Animation component to character
- Call `Animation.Play(clipName)`

**Result:** ❌ FAILED
- Animation component reports `isPlaying: True`
- Character freezes when Animator disabled
- But no visual animation plays

**Root Cause:** Legacy Animation uses exact bone path matching. Paths like `Armature/Hips` don't match `CharacterCosmetics.../ThePack_Felix.../Armature/Rig.Pelvis`

### Approach 2: Legacy Animation on Armature
**Implementation:**
- Find the `Armature` GameObject deep in hierarchy
- Place Animation component there instead of on root
- Paths would be relative to Armature

**Result:** ❌ FAILED
- Same issue - bone NAMES still don't match
- Mixamo uses `Hips`, Endstar uses `Rig.Pelvis`

### Approach 3: Humanoid Retargeting via Bundle Config
**Implementation:**
- In Unity Editor, set Mixamo FBX Rig type to "Humanoid"
- Rebuild the animation bundle
- Unity should auto-map bones

**Result:** ❌ FAILED (with Legacy Animation)
- Legacy Animation does NOT support Humanoid retargeting
- Only Mecanim supports it

### Approach 4: Playables API
**Implementation:**
- Use `PlayableGraph` and `AnimationClipPlayable`
- Create `AnimationPlayableOutput` targeting the Animator
- Playables API uses Mecanim under the hood
- Should support Humanoid retargeting automatically

**Result:** ❌ FAILED

**Critical Discovery:**
```
Animator avatar: Terran_Test_01Avatar
Animator isHuman: False      ← GENERIC RIG!
Clip isHumanMotion: True     ← Humanoid clip
```

**Root Cause:** Endstar characters use **Generic** avatar, NOT Humanoid. Humanoid retargeting only works when BOTH animator and clip are Humanoid. This is a fundamental incompatibility.

---

## THE CORE PROBLEM

**Endstar uses Generic rig, Mixamo uses Humanoid rig.**

| Component | Rig Type | Bone Names |
|-----------|----------|------------|
| Endstar Character | Generic | `Rig.Pelvis`, `Rig.Root`, `Rig.Spine` |
| Mixamo Animations | Humanoid | `mixamorig:Hips`, `mixamorig:Spine` |

Unity cannot automatically retarget between Generic and Humanoid rigs.

---

## Remaining Options

### Option A: Manual Bone Remapping in Unity Editor
1. Import Mixamo FBX as **Generic** (not Humanoid)
2. In the Animation tab, manually remap bone paths
3. Or write an Editor script to batch-rename bone paths in AnimationClips
4. Rebuild bundle with remapped animations

### Option B: Runtime Curve Remapping
1. At runtime, read animation curves from clip
2. Create new AnimationClip with remapped bone paths
3. Play the remapped clip

**Challenge:** AnimationClip curves are read-only at runtime in built games

### Option C: Create Animations from Scratch
1. Use Endstar's actual bone names
2. Create animations in Blender/Maya targeting `Rig.Pelvis`, etc.
3. Export and bundle

### Option D: Investigate Endstar's Existing Animations
1. Find how Endstar's 369 built-in animations work
2. See if there's a way to swap/override specific animation states
3. Use AnimatorOverrideController if state names are known

---

## Technical Blockers Encountered

### 1. BepInEx MonoBehaviour Update() Issue
**Problem:** `Update()` method on `BaseUnityPlugin` class was never called.

**Solution:** Create a separate `MonoBehaviour` class, spawn it on a new `GameObject` with `DontDestroyOnLoad()`:
```csharp
var go = new GameObject("AnimationInjectorRunner");
Object.DontDestroyOnLoad(go);
var runner = go.AddComponent<AnimationRunner>();
```

### 2. Hotkey Conflicts
**Problem:** F5/F6/F7 keys didn't respond (likely used by game internally).

**Solution:** Changed to Numpad1/Numpad2/Numpad3.

### 3. Bone Name Mismatch
**Problem:** Mixamo bones (`mixamorig:Hips`) vs Endstar bones (`Rig.Pelvis`).

**Potential Solutions:**
- Use Humanoid retargeting (requires both Animator and Clip to be Humanoid)
- Manually remap animation curves in Unity Editor
- Use AnimatorOverrideController at runtime

### 4. Multiple Animators
**Problem:** Disabling one Animator wasn't enough - character kept animating.

**Solution:** Find and disable ALL Animators in both parent and children:
```csharp
var allAnimators = _playerAnimator.GetComponentsInParent<Animator>(true);
var childAnimators = _playerAnimator.GetComponentsInChildren<Animator>(true);
```

---

## Files Created/Modified

### Plugin Files
- `AnimationInjector/Plugin.cs` - Main BepInEx plugin
- `AnimationInjector/AnimationInjector.csproj` - Project file

### Unity Editor Scripts
- `Assets/Editor/BuildCustomAnimationsBundle.cs` - Builds animation AssetBundle

### Output Files
- `custom_animations.bundle` - Contains 3 Mixamo clips:
  - Run To Dive (1.20s)
  - Swimming (3.23s)
  - Treading Water (2.13s)

---

## Diagnostic Logging Added

The plugin logs:
1. Bundle loading status and clip count
2. Character skeleton hierarchy (4 levels deep)
3. Animator count (parent/child)
4. Animator enabled state after disable
5. Animation component isPlaying status
6. Clip properties (legacy, length, wrapMode)
7. **NEW:** Animator.isHuman, Avatar name, Clip.isHumanMotion

---

## Next Steps

1. ~~**Test Playables API approach**~~ - DONE: Failed because Animator.isHuman = False
2. **Option A recommended:** Create Unity Editor script to remap Mixamo bone paths to Endstar bone paths
3. **Research needed:** Get full Endstar bone hierarchy to create mapping table
4. **Alternative:** Extract Endstar's existing swimming/diving animations and modify them

---

## Code Snippets

### Finding Player Animator
```csharp
var animators = FindObjectsOfType<Animator>();
foreach (var anim in animators)
{
    var ctrl = anim.runtimeAnimatorController;
    if (anim.gameObject.name.Contains("Player Character Appearance") &&
        ctrl != null && ctrl.name == "Character_Base_Animator")
    {
        _playerAnimator = anim;
        break;
    }
}
```

### Playables API Animation Playback
```csharp
_graph = PlayableGraph.Create("CustomAnimationGraph");
_graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

_clipPlayable = AnimationClipPlayable.Create(_graph, clip);

var output = AnimationPlayableOutput.Create(_graph, "Animation", _playerAnimator);
output.SetSourcePlayable(_clipPlayable);

_graph.Play();
```

---

## Log File Location
```
C:\Endless Studios\Endless Launcher\Endstar\BepInEx\LogOutput.log
```

---

## Summary Table

| Approach | What We Tried | Result | Why It Failed |
|----------|---------------|--------|---------------|
| Legacy Animation | Play clip with `Animation.Play()` | ❌ | Bone paths don't match hierarchy |
| Legacy on Armature | Move Animation component to Armature | ❌ | Bone names still don't match |
| Humanoid Bundle | Set Mixamo FBX to Humanoid in Unity | ❌ | Legacy Animation doesn't retarget |
| Playables API | Use PlayableGraph with Mecanim | ❌ | Endstar is Generic, not Humanoid |

**Bottom Line:** Cannot use Mixamo animations directly. Must manually remap bone names OR create new animations targeting Endstar's bone structure.

---

## Version History
- v1.0 - Initial attempt with Update() on BaseUnityPlugin (failed)
- v1.1-1.3 - Various fixes for MonoBehaviour lifecycle
- v1.4 - Separate MonoBehaviour pattern (Update works!)
- v1.4+ - Added bone hierarchy logging, multi-animator disable
- Current - Switched to Playables API for Humanoid retargeting support

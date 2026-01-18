# Silhouette Fix Investigation - Complete Documentation

## Problem
Custom character "Zayed" (using TigerGuy mesh) appears as a black silhouette in the party/lobby area, while official characters like Ada render correctly.

---

## Root Cause Analysis

### Deep Scan Comparison (Runtime)

| Property | TigerGuy (BROKEN) | Ada (WORKING) |
|----------|-------------------|---------------|
| Material | ThePack_Felix_VC_01_A | Ada_VC_01_A |
| Shader | Shader Graphs/Endless_Shader_Character_NoFade | Same |
| RenderQueue | **2000** | **2450** |
| PassCount | **5** | **6** |
| GlobalIllumFlags | RealtimeEmissive | EmissiveIsBlack |
| Keywords | **_LIGHT_COOKIES** | **_ALPHATEST_ON** |
| _AlphaClip | **0** | **1** |
| _AlphaToMask | **0** | **1** |

### Shader Passes Difference
**TigerGuy (5 passes):**
- Universal Forward
- ShadowCaster
- DepthOnly
- DepthNormals
- Universal 2D

**Ada (6 passes):**
- Universal Forward
- **GBuffer** ← MISSING FROM TIGERGUY
- ShadowCaster
- DepthOnly
- DepthNormals
- Universal 2D

### Why Runtime Fix Doesn't Work
The shader variant is compiled at build time. TigerGuy's material loads a shader variant WITHOUT the GBuffer pass. Changing material properties at runtime (renderQueue, keywords, _AlphaClip) does NOT load a different shader variant. The missing GBuffer pass is the core issue.

---

## Approaches Attempted

### 1. BepInEx Runtime Material Property Fix
**Versions tried:** v1.0.0 through v3.0.0

**What it does:**
- Uses Harmony to patch `Time.get_deltaTime` (only reliable hook that works)
- Finds TigerGuy meshes via `FindObjectsOfType<SkinnedMeshRenderer>()`
- Sets: renderQueue=2450, _AlphaClip=1, _AlphaToMask=1, enables _ALPHATEST_ON keyword

**Result:** Properties change successfully (verified in logs) but visual remains broken. Shader variant doesn't change at runtime.

**Key Learning:** BepInEx Update()/Start()/Coroutines don't work in this game. Only Harmony patching Time.get_deltaTime works.

### 2. BepInEx Material Swap (Copy from Ada)
**Versions:** v4.0.0, v4.1.0

**What it does:**
- Captures Ada's material (which has correct shader variant)
- Creates new Material from Ada's: `new Material(AdaMaterial)`
- Copies TigerGuy's textures to new material
- Applies to TigerGuy renderer

**Result:** NOT FULLY TESTED - Ada and TigerGuy cannot be visible simultaneously in the same scene. Ada appears in one scene, then gets replaced by TigerGuy when character is selected.

### 3. UnityPy Bundle Modification
**Script:** `fix_material_bundle.py`, `fix_material_proper.py`

**What it does:**
- Loads felix bundle with UnityPy
- Modifies material properties in the bundle
- Saves bundle

**Result:** CORRUPTS BUNDLE. After saving, game throws NullReferenceException. UnityPy's save destroys material properties (45 floats → 0 floats after save).

**WARNING: DO NOT USE UnityPy to modify bundles - it corrupts them.**

### 4. UABEA Bundle Modification
**Process:**
1. Export material dump from UABEA
2. Edit text file to change:
   - `m_ValidKeywords: _ALPHATEST_ON` (was _LIGHT_COOKIES)
   - `m_CustomRenderQueue: 2450` (was 2000)
   - `_AlphaClip: 1` (was 0)
   - `_AlphaToMask: 1` (was 0)
3. Import dump back
4. Save bundle

**Result:** CORRUPTS BUNDLE. Same NullReferenceException as UnityPy. The material changes may break shader variant loading.

---

## Files Created

### BepInEx Plugin
- `D:\Endstar Plot Flag\PLOT FLAG SYSTEM\Zayed_Character_Mod\SilhouetteFix\Plugin.cs`
- `D:\Endstar Plot Flag\PLOT FLAG SYSTEM\Zayed_Character_Mod\SilhouetteFix\SilhouetteFix.csproj`

### Python Scripts (DO NOT USE FOR SAVING)
- `D:\Endstar Plot Flag\PLOT FLAG SYSTEM\scan_material_bundle.py` - Read-only scanning, safe to use
- `D:\Endstar Plot Flag\PLOT FLAG SYSTEM\fix_material_bundle.py` - CORRUPTS BUNDLE
- `D:\Endstar Plot Flag\PLOT FLAG SYSTEM\fix_material_proper.py` - CORRUPTS BUNDLE

### Bundle Location
- `C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\StreamingAssets\aa\StandaloneWindows64\felix_assets_all_8ce5cfff23e50dfcaa729aa03940bfd7.bundle`
- Backup: `.original` extension

### UABEA Dumps
- `D:\Endstar Plot Flag\PLOT FLAG SYSTEM\FixBundle\ThePack_Felix_VC_01_A-CAB-*.txt`

---

## What Would Actually Fix This

### Option 1: Rebuild in Unity (Recommended)
Open the Unity project, set the material correctly:
- Enable Alpha Clipping
- Set RenderQueue to 2450
- Ensure _ALPHATEST_ON keyword is set
- Rebuild Addressables

This ensures the correct shader variant is compiled into the bundle.

### Option 2: Find Correct Shader Variant
If the _ALPHATEST_ON shader variant exists somewhere in the game's shader bundles, reference it instead of modifying material properties.

### Option 3: Custom Shader Replacement
Create a BepInEx plugin that replaces the shader entirely with a custom one that doesn't have the silhouette issue.

---

## Technical Details

### Why Shader Variants Matter
Unity compiles different shader variants based on keywords. A material with _ALPHATEST_ON uses a different compiled shader than one with _LIGHT_COOKIES. The GBuffer pass is only included in the _ALPHATEST_ON variant. You cannot add a missing pass at runtime.

### BepInEx Working Hook
```csharp
[HarmonyPatch(typeof(Time), "get_deltaTime")]
public static class TimePatch
{
    static void Postfix()
    {
        // This runs every frame on main thread
        Plugin.DoWork();
    }
}
```

### Bundle Modification Warning
Both UnityPy and UABEA text dump import/export corrupt the bundle when changing material properties. The game throws:
```
NullReferenceException: Object reference not set to an instance of an object
Endless.Core.UI.UIUserGroupScreenCharacter.HandleCosmeticInstantiated
```

---

## Conclusion
The silhouette issue is caused by the material using wrong shader variant (5 passes instead of 6, missing GBuffer). Runtime property changes don't fix it because shader variants are compiled at build time. Bundle modification tools corrupt the bundle. The only reliable fix is rebuilding in Unity with correct material settings.

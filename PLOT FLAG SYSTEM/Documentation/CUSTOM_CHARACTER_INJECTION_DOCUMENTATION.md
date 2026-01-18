# Endstar Custom Character Injection - Complete Documentation

## Project Overview

**Goal:** Replace Felix character cosmetic in Endstar with a custom character (Pearl Diver from Meshy AI)

**Date:** January 3-4, 2026

**Status:** ✅ SUCCESS - Custom character renders in-game!

---

## QUICK START GUIDE (For Future Characters)

### Critical Requirements
1. **Unity Version:** Must use **exactly 2022.3.62f2** (not f3 or any other version!)
2. **Mesh Pose:** Character mesh must be in **A-pose**, NOT T-pose!
3. **Shader:** Use `Endless_Shader_Character_NoFade` from SDK
4. **Material Keyword:** Must have `_LIGHT_COOKIES` in m_ValidKeywords
5. **No MixMap:** Do NOT use Felix's MixMap texture - causes artifacts
6. **CRC:** Must patch game catalog with Unity's built CRC (NOT zlib.crc32)

### Quick Workflow
1. **Prepare mesh** in Blender → Export as FBX (Humanoid rig)
2. **Import to Unity 2022.3.62f2** project with Endstar SDK
3. **Create material** with Endless_Shader_Character_NoFade, add `_LIGHT_COOKIES` keyword
4. **Set textures:** Only _Albedo (your texture), leave MixMap empty
5. **Build Addressables** → Get CRC from built catalog
6. **Deploy:** Copy bundle + patch game catalog CRC
7. **Change display name** in `sharedassets0.assets` (same-length binary replace)
8. **Replace portrait** in `sharedassets0.assets.resS` (214×251 RGBA, flipped)
9. **Launch game directly** (not via launcher)

---

## Tools & Software Used

| Tool | Version | Purpose | Location |
|------|---------|---------|----------|
| AssetStudio | Custom Build | Extract Unity assets from bundles | `D:\Endstar Plot Flag\PLOT FLAG SYSTEM\AssetStudio-master\` |
| UABEA | Latest | Unity asset bundle editing, material inspection | `D:\Endstar Plot Flag\PLOT FLAG SYSTEM\UABEA\` |
| Blender | 3.5+ | 3D modeling, rigging, FBX export | - |
| Unity | **2022.3.62f2** | Build Addressable bundles (MUST match game version!) | `D:\Unity_Workshop\Endstar Custom Shader\` |
| Unity Addressables | 1.22.3 | Bundle building system | Package Manager |
| Endstar SDK | 7268c9a695 | Shaders and SDK components | `D:\Unity_Workshop\Endless Studios\Endstar\Endstar SDKs\Endstar SDK Updated\` |

---

## File Locations

### Game Files
```
C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\StreamingAssets\aa\StandaloneWindows64\
├── felix_assets_all_8ce5cfff23e50dfcaa729aa03940bfd7.bundle  (target bundle)
├── catalog.json  (addressables catalog)
└── [other character bundles]
```

### Extracted Assets
```
D:\Endstar Plot Flag\PLOT FLAG SYSTEM\Felix\
├── Mesh\
│   ├── TigerGuy_LOD0.obj
│   ├── TigerGuy_LOD1.obj
│   └── TigerGuy_LOD2.obj
├── Texture2D\
│   ├── Tiger_Orange_Albedo.png
│   ├── Tiger_MixMap.png
│   └── Tiger_Orange_Emissive.png
└── Shader\
    └── Shader Graphs_Endless_Shader_Character_NoFade.shader (NOT usable - decompiled)
```

### SDK Shaders (Working)
```
D:\Unity_Workshop\Endless Studios\Endstar\Endstar SDKs\Endstar SDK Updated\Library\PackageCache\com.endless-studios.endstar-sdk@7268c9a695\Shaders\
├── OptimizedShaderPass\
│   └── Endless_Shader_Character_NoFade.shadergraph  (CORRECT shader to use)
├── SSS_Shader.shadersubgraph
├── CustomLighting.hlsl
└── GetMainLight.hlsl
```

### Unity Project
```
D:\Unity_Workshop\Endstar Custom Shader\
├── Assets\
│   ├── Shaders\  (copied from SDK)
│   ├── Textures\  (Felix textures)
│   └── Prefabs\CharacterCosmetics\
│       └── CharacterCosmetics_ThePack_Felix_01_A.prefab
└── Library\com.unity.addressables\aa\Windows\StandaloneWindows64\
    └── [built bundles]
```

### Backup
```
D:\Endstar Plot Flag\PLOT FLAG SYSTEM\felix_original_backup.bundle
```

---

## Phase 1: Asset Extraction

### AssetStudio Compilation
**Issue:** LIBCMT.lib linker errors
**Solution:** Use correct MSVC toolset version
```
vcvarsall.bat -vcvars_ver=14.44
/p:VCToolsVersion=14.44.35207
```

**Issue:** FBX SDK missing
**Solution:** Install FBX SDK 2020.2.1 VS2019 from Autodesk

**Issue:** Internal compiler error (C1001) on x86
**Solution:** Use x64 hosted compiler (`amd64_x86` instead of `x86`)

### Extraction Results
- Felix mesh: 3 LOD levels (TigerGuy_LOD0/1/2)
- Textures: Albedo, MixMap, Emissive
- Shader: Extracted but NOT recompilable (DXBC bytecode, not source)

---

## Phase 2: Shader Analysis

### Critical Finding
Extracted shaders from game bundles are **NOT valid shader files**. They contain:
```
// NOTE: This is *not* a valid shader file
// shader disassembly not supported on DXBC
```

### Solution
Use shaders from Endstar SDK package:
```
com.endless-studios.endstar-sdk@7268c9a695\Shaders\OptimizedShaderPass\Endless_Shader_Character_NoFade.shadergraph
```

### Shader Properties (for reference)
- `_Albedo` - Base color texture
- `Albedo_Tint` - Color tint
- `Mask Map` - Metallic/Roughness/AO packed
- `Normal` - Normal map
- `_Emissive_Map` - Emission texture
- SSS (Subsurface Scattering) settings
- Rim lighting
- `HURT_FLASH` - Damage flash effect

---

## Phase 3: Blender Rigging

### CRITICAL ISSUE: Meshy AI FBX Hierarchy Lock

**Problem:** Pearl Diver mesh imported from Meshy AI FBX was locked in a parent-child hierarchy that could NOT be modified.

**Symptoms:**
- Mesh stuck under Empty container (orange triangle icon)
- Alt+P (Clear Parent) - Did NOT work
- Dragging in Outliner - Did NOT work
- Object menu > Parent > Clear - Did NOT work
- Shift+D duplicate - Did NOT create independent copy
- Deleting parent deleted children too
- Python scripts to change parent - Did NOT work

**Failed Attempts:**
1. Standard unparenting methods
2. Python script to force reparent
3. Joining meshes (Ctrl+J)
4. Various Blender UI approaches

**SOLUTION THAT WORKED:**
1. Export Pearl Diver mesh as OBJ (strips hierarchy)
2. Open NEW Blender file
3. Import Felix's FBX (with armature)
4. Import Pearl Diver OBJ
5. In fresh file, parenting works normally
6. Select mesh, Shift+select Armature
7. Ctrl+P > Armature Deform With Automatic Weights
8. Export as FBX

### FBX Export Settings for Unity
```
Include:
- [x] Selected Objects
- [ ] Visible Objects

Object Types:
- [x] Armature
- [x] Mesh
- [ ] Everything else

Transform:
- Scale: 1.00
- Apply Scalings: FBX Units Scale
- Forward: Z Forward
- Up: Y Up
- [ ] Apply Transform (unchecked)

Geometry:
- Smoothing: Normals Only
- [x] Apply Modifiers
- [x] Triangulate Faces

Armature:
- [ ] Add Leaf Bones (UNCHECKED - critical!)
- Primary Bone Axis: Y Axis
- Secondary Bone Axis: X Axis
- [x] Only Deform Bones

Bake Animation:
- [ ] Bake Animation (unchecked)
```

---

## Phase 4: Unity Setup

### Import Settings
1. Select imported FBX
2. Rig tab > Animation Type: **Humanoid**
3. Click Apply
4. Verify mesh is visible (if not, bones don't match humanoid standard)

### Prefab Structure Required
```
CharacterCosmetics_ThePack_Felix_01_A (root)
└── CharacterCosmetics_ThePack_Felix_01_A
    └── ThePack_Felix_VC_01
        ├── Armature
        ├── TigerGuy_LOD0  (your mesh)
        ├── TigerGuy_LOD1  (your mesh copy)
        └── TigerGuy_LOD2  (your mesh copy)
```

### Critical Components
- **LODGroup** on root
- **Animator** with Humanoid Avatar (no controller needed - game provides it)
- **SkinnedMeshRenderer** on mesh objects

### Addressable Configuration
**Address MUST match exactly:**
```
Assets/Prefabs/CharacterCosmetics/CharacterCosmetics_ThePack_Felix_01_A.prefab
```

**Note:** The actual file location must match the address, OR set Internal Asset Naming Mode to "Dynamic" in Addressables Group settings.

### Build Process
1. Window > Asset Management > Addressables > Groups
2. Build > New Build > Default Build Script
3. Output: `Library\com.unity.addressables\aa\Windows\StandaloneWindows64\`

---

## Phase 5: Bundle Replacement

### Replacement Script
```powershell
# D:\Endstar Plot Flag\PLOT FLAG SYSTEM\replace_bundle.ps1

# Kill any Endstar processes
Get-Process | ForEach-Object {
    if ($_.Name -like "*Endstar*" -or $_.Name -like "*Endless*") {
        Stop-Process -Id $_.Id -Force
    }
}

Start-Sleep -Seconds 2

$source = "D:\Unity_Workshop\Endstar Custom Shader\Library\com.unity.addressables\aa\Windows\StandaloneWindows64\[BUNDLE_NAME].bundle"
$dest = "C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\StreamingAssets\aa\StandaloneWindows64\felix_assets_all_8ce5cfff23e50dfcaa729aa03940bfd7.bundle"

Copy-Item -Path $source -Destination $dest -Force

Write-Host "Bundle replaced successfully!"
```

**Note:** Bundle name changes with each build (hash in filename). Update script accordingly.

### CRITICAL: Bypass Launcher
The Endless Launcher detects modified files and restores originals.

**Solution:** Launch game directly:
```
C:\Endless Studios\Endless Launcher\Endstar\Endstar.exe
```

Do NOT use the launcher after replacing bundles.

---

## Known Issues & Solutions

### 1. Launcher Verification
- Launcher checks file integrity and restores modified bundles
- **Solution:** Launch game directly: `C:\Endless Studios\Endless Launcher\Endstar\Endstar.exe`
- Do NOT use the launcher after replacing bundles

### 2. CRC Mismatch Error (CRITICAL)
**Error:** `CRC Mismatch. Provided XXXXXXXX, calculated YYYYYYYY from data. Will not load AssetBundle`

**Cause:** Game validates bundle checksum against catalog.json

**Solution:** Patch the CRC in catalog.json using the CRC from Unity's built catalog:

**CRITICAL:** Do NOT use Python's `zlib.crc32()` - Unity uses a DIFFERENT CRC algorithm!
You MUST read the CRC from Unity's built catalog instead.

```python
import json, base64, shutil, re

# Step 1: Get CRC from Unity's built catalog (NOT from zlib.crc32!)
unity_catalog = r'D:\Unity_Workshop\Endstar Custom Shader\Library\com.unity.addressables\aa\Windows\catalog.json'
with open(unity_catalog, 'r', encoding='utf-8') as f:
    built_catalog = json.load(f)
extra = base64.b64decode(built_catalog['m_ExtraDataString'])
# Find m_Crc in UTF-16LE encoded data
idx = extra.find('m_Crc'.encode('utf-16-le'))
crc_context = extra[idx:idx+50].decode('utf-16-le', errors='ignore')
new_crc = int(re.search(r'm_Crc":(\d+)', crc_context).group(1))
print(f'Unity CRC: {new_crc}')

# Step 2: Restore original game catalog
backup_path = r'C:\...\catalog_original_backup.json'
catalog_path = r'C:\...\catalog.json'
shutil.copy(backup_path, catalog_path)

# Step 3: Read and decode game catalog
with open(catalog_path, 'r') as f:
    catalog = json.load(f)
extra_data = base64.b64decode(catalog['m_ExtraDataString'])

# Step 4: Replace CRC (UTF-16LE encoded, pad if needed)
old_crc = 1004267194  # Felix's original
old_str = str(old_crc)
new_str = str(new_crc)
if len(new_str) < len(old_str):
    new_str = new_str + ' ' * (len(old_str) - len(new_str))

extra_data = extra_data.replace(old_str.encode('utf-16-le'), new_str.encode('utf-16-le'), 1)

# Step 5: Save
catalog['m_ExtraDataString'] = base64.b64encode(extra_data).decode('ascii')
with open(catalog_path, 'w') as f:
    json.dump(catalog, f, separators=(',', ':'))
```

**Felix's Original CRC:** `1004267194` (hex: `0x3bdbe6ba`)

**WRONG:** `zlib.crc32(bundle_data)` gives different value than Unity's internal CRC!
**RIGHT:** Read CRC from `Library/com.unity.addressables/aa/Windows/catalog.json`

**Important:** Always restore from backup before patching, as catalog changes accumulate and can corrupt data.

### 3. Faded Silhouette / Wrong Material (CRITICAL)
**Symptom:** Character loads but appears as faded/transparent silhouette

**Cause:** FBX material remapping pointing to wrong material file

**Problem Details:**
- FBX files have internal material names (e.g., "Material.001")
- Unity remaps these to external .mat files via `externalObjects` in .fbx.meta
- If remapped to a material missing the **Mask Map** texture, shader renders incorrectly

**Solution:**
1. Check FBX.meta for `externalObjects` section
2. Ensure it points to material WITH Mask Map (e.g., `ThePack_Felix_VC_01_A.mat`)
3. Material MUST have `Texture2D_033410430c1e41f38cbe37cd7347362e` (Mask Map) assigned

**Example FBX.meta fix:**
```yaml
externalObjects:
- first:
    type: UnityEngine:Material
    assembly: UnityEngine.CoreModule
    name: Material.001
  second: {fileID: 2100000, guid: 5bfb4f1a870aac0419ce32c8aa5ffa83, type: 2}  # ThePack_Felix_VC_01_A.mat
```

### 4. Material Texture Slots (Endless_Shader_Character_NoFade)
**Required textures for proper rendering:**

| Slot Name | Property | Purpose |
|-----------|----------|---------|
| `_Albedo` | Base color | Main texture (Pearl Diver texture here) |
| `Texture2D_033410430c1e41f38cbe37cd7347362e` | Mask Map | Metallic/Smoothness/AO - **CRITICAL** |
| `_Emissive_Map` | Emission | Glow effects |
| `_Pre_Integrated_Scattering` | SSS LUT | Subsurface scattering lookup |

**If Mask Map is missing:** Character appears faded/silhouette. Use `Tiger_MixMap.png` as fallback.

### 5. Tilly Fallback
**Symptom:** Game shows Tilly character instead of Felix/custom character

**Causes:**
1. Bundle not loading (CRC mismatch, file not found)
2. Prefab structure incorrect
3. Missing LOD meshes (need TigerGuy_LOD0, LOD1, LOD2)
4. Humanoid rig not configured
5. Bundle built with wrong addressable address

**Debug:** Check game log at:
```
C:\Users\[User]\AppData\LocalLow\Endless Studios\Endstar\Player.log
```

### 6. Shader Compatibility
- Game uses custom `Endless_Shader_Character_NoFade.shadergraph`
- Shader GUID: `e83c5035ac1520d4d822c8bf82b180c5`
- Standard URP shaders will NOT render correctly
- Must use SDK shaders from: `com.endless-studios.endstar-sdk@7268c9a695\Shaders\`

### 7. Bone Weight Transfer
- Automatic weights may not perfectly match for different body shapes
- Some vertices may have no weights (assigned to bone #0)
- May need manual weight painting for better results

### 8. Shader Compilation Mismatch (CRITICAL)
**Symptom:** Custom bundle loads but renders as faded silhouette, while original Felix works fine

**Diagnosis Test:**
1. Restore original Felix bundle and catalog
2. Launch game - if original Felix renders correctly, CRC patching works
3. Issue is specifically with custom bundle content

**Cause:** Our Unity project compiles the Endless_Shader_Character_NoFade.shadergraph differently than the game's built-in version. Even with the same GUID, different shader bytecode causes rendering issues.

**Evidence:**
- Original Felix bundle: 3.6 MB
- Custom bundle: 9.6 MB (includes compiled shader variants)

**Solution:** Remove shader source from Unity project so the game uses its built-in shader instead of our compiled version. The material still references the shader by GUID, but no shader code gets bundled.

### 9. Material Property Issues
**Albedo_Tint Alpha:**
- Default Albedo_Tint in some materials has `a: 0` (alpha = 0)
- This can cause transparency/silhouette issues
- **Fix:** Set `Albedo_Tint: {r: 1, g: 1, b: 1, a: 1}`

### 10. Verification Workflow
Always verify changes by testing with original content first:
```
1. Restore original Felix bundle + catalog
2. Launch game, verify Felix renders correctly
3. If yes: CRC patching works, issue is with custom content
4. If no: CRC patching broken, fix catalog first
```

### 11. Shader Dependency Test
**Test:** Disable shader source in Unity project (rename .shadergraph to .disabled)
- If character shows PINK: Bundle no longer includes shader, game can't find matching GUID
- Conclusion: Game's shader is NOT a separate shared bundle - each character bundle includes its own shader

### 12. Experimentation Log (Silhouette Issue)
**Tested and ruled out:**
- ✗ Wrong material assigned (verified correct material)
- ✗ Albedo_Tint alpha = 0 (fixed to 1, still silhouette)
- ✗ Extreme emission values (removed emission, still silhouette)
- ✗ Missing shader (pink when removed, so shader IS loaded)
- ✗ CRC mismatch (original Felix works with same patching method)

**Still investigating:**
- ? Shader keywords not enabled in material
- ? Shader compiling with different variants than game expects

### 13. Mesh vs Shader Isolation Test (CRITICAL FINDING)
**Test performed:** Used original Felix mesh (from "(merge)" FBX) with our material/shader
**Result:** Still silhouette!

**Conclusion:** The issue is NOT the mesh. Even original Felix mesh shows silhouette when built with our shader.

**Root cause confirmed:** Shader compilation difference between game and our Unity project.

### 14. Unity Version Comparison
| Component | Game | Our Project |
|-----------|------|-------------|
| Unity | 2022.3.62f2 | 2022.3.62f3 |

Versions are nearly identical (one patch apart), ruling out major Unity version incompatibility.

### 15. Shader Compilation Investigation
**Key insight:** Each character bundle includes its own compiled shader (confirmed by pink fallback when shader removed from our project).

**The problem:** Same shader source (SDK) compiles to different bytecode in our project vs game's original bundles.

**Possible causes:**
1. Different shader variant stripping settings
2. Different URP asset configuration
3. Different shader keyword defaults
4. Missing shader include files or subgraphs
5. Different graphics tier settings at build time

### 16. Analysis Tools
**AssetStudio:** Can extract shader metadata from bundles:
- Load bundle → Filter by Type: Shader
- Compare properties, keywords, passes between original and custom bundles
- Note: Compiled shaders are DXBC bytecode, not readable source

**RenderDoc:** Runtime GPU debugging to capture actual shader execution

### 17. Experimental Approaches (TODO)
1. **URP Lit shader test:** Use standard URP/Lit instead of Endless shader to verify if ANY shader works
2. **Shader keyword analysis:** Compare m_ValidKeywords between original and custom materials
3. **URP Asset comparison:** Match render pipeline settings with game's configuration
4. **Shader stripping settings:** Check Project Settings → Graphics for shader variant stripping

### 18. AssetStudio Shader Extraction (CRITICAL ANALYSIS)

**Tool:** AssetStudio → Load original Felix bundle → Filter Type: Shader → Export Raw

**Shader Name:** `Shader Graphs/Endless_Shader_Character_NoFade`

**Properties (from original bundle):**
```
_Albedo ("Albedo", 2D) = "white" { }
Albedo_Tint ("Albedo Tint", Color) = (1,1,1,0)     ← Note: alpha=0 in defaults!
Texture2D_033410430c1e41f38cbe37cd7347362e ("Mask Map", 2D) = "white" { }
Normal ("Normal", 2D) = "bump" { }
_Emissive_Map ("Emissive_Map", 2D) = "black" { }
_Pre_Integrated_Scattering ("Pre_Integrated_Scattering", 2D) = "black" { }
HURT_FLASH ("HURT_FLASH", Float) = 0
Selection_Color ("Selection_Color", Color) = (0,0,0,0)
Vector1_844a91d1120546378bc600c39ce4fc77 ("SSS_Ambient_Intensity", Float) = 0
Vector1_195718c9f68f499782987983967ae661 ("SSS_Intensity", Float) = 0
Vector1_6639e655f62c4bf0878a285950a9ad38 ("SSS_Scale", Float) = 0.01
_Height ("_Height", 2D) = "black" { }
_Height_Intensity ("_Height_Intensity", Float) = 0
_Rim_Intensity ("_Rim_Intensity", Float) = 1
_Rim_Power ("_Rim_Power", Float) = 5
_PS_Multiply ("_PS_Multiply", Float) = 0.5
_PS_Add ("_PS_Add", Float) = 0.5
```

**SUSPICIOUS PASS SETTINGS (Universal Forward):**
```
Pass "Universal Forward" {
  Tags { "LIGHTMODE" = "UniversalForward" }
  Blend Zero Zero, Zero Zero   ← DISABLES OUTPUT!
  ZTest Off                     ← No depth test
  ZWrite Off                    ← No depth write
  Cull Off                      ← Render both sides
```

**Key Observation:** `Blend Zero Zero` mathematically disables all rendering:
- Blend equation: `Final = Src * Zero + Dst * Zero = 0`
- This would make the shader completely invisible

**Possible Explanations:**
1. AssetStudio shows placeholder/default values, not runtime state
2. Shader relies on C# code or material properties to set blend state at runtime
3. Keyword variants change blend state (e.g., `_ALPHATEST_ON`)
4. This might explain why our compiled version renders as silhouette - different blend state initialization

**Shader Keywords Found:**
- `_ADDITIONAL_LIGHTS`
- `_ADDITIONAL_LIGHT_SHADOWS`
- `_LIGHT_COOKIES`
- `_MAIN_LIGHT_SHADOWS`
- `_MAIN_LIGHT_SHADOWS_CASCADE`
- `_MAIN_LIGHT_SHADOWS_SCREEN`
- `_SCREEN_SPACE_OCCLUSION`
- `_SHADOWS_SOFT`
- `INSTANCING_ON`
- `DOTS_INSTANCING_ON`

**Shader Passes:**
1. `Universal Forward` - Main rendering
2. `GBuffer` - Deferred rendering
3. `ShadowCaster` - Shadow map generation
4. `DepthOnly` - Depth prepass
5. `DepthNormals` - Normal/depth buffer
6. `Universal 2D` - 2D fallback

**Next Investigation Steps:**
1. Check if game's shader system overrides blend state at runtime
2. Compare material `m_ValidKeywords` between working vs broken
3. Try forcing blend state in our compiled shader
4. Look for C# components that modify shader properties

### 19. Original Material Extraction via UABEA (CRITICAL DISCOVERY)

**Tool:** UABEA → Load Felix bundle → Find Material type → Export Dump

**Original Material `m_ValidKeywords`:**
```
m_ValidKeywords: ["_LIGHT_COOKIES"]
```

**Comparison - Original vs Our Material:**

| Property | Original Felix | Our Material (Before) | Our Material (After) |
|----------|---------------|----------------------|---------------------|
| `m_ValidKeywords` | `["_LIGHT_COOKIES"]` | `[]` (empty) | `["_LIGHT_COOKIES"]` |
| `Albedo_Tint` alpha | 0 | 1 | 0 (restored) |
| SSS_Intensity (Vector1_195...) | 1 | 0 | 1 |
| SSS_Ambient_Intensity (Vector1_844...) | 1 | 0 | 1 |
| `_Albedo` texture | Tiger_Orange | Tiger_Orange | Pearl_Diver |

**Additional properties in original (not in ours):**
- `Cutoff_Height: 4`
- `Rim_Power: 1` (ours has `_Rim_Power: 5`)
- `Vector1_011b08ae8492454881f21647046d7364: 44.86`
- `Vector1_0805f1a653364d1fb00c9ca1a30046bb: 1.61`
- `Vector1_860473e7aeb247ccbc708bb61c41171d: 0.12`
- `_Emissive_Cracks_Amount: 0`
- `_Outline_Thickness: 0.01`
- `_SSS_Intensity: 0`, `_SSS_Normal_Influence: 0`, `_SSS_Power: 0.5`
- `_Wind_Speed: 0.1`, `_Wind_Strength: 0`
- `Color_a5cdfa342d6a41c08bc8e53de5b78807` (blue-ish)
- `_Emissive_Cracks_Color`, `_SSS_Color`, `_Wind_Direction`

### 20. Material Keyword Fix Attempt (FAILED)

**Action taken:**
1. Added `_LIGHT_COOKIES` to `m_ValidKeywords`
2. Set SSS_Intensity and SSS_Ambient_Intensity to 1
3. Restored Albedo_Tint alpha to 0
4. Changed `_Albedo` texture to Pearl Diver

**Result:** Still silhouette! The keyword fix alone was NOT sufficient.

**Conclusion:** The issue is definitively the **compiled shader bytecode**, not material properties or keywords. Even with identical material settings, our Unity project compiles the shader differently than the game's original.

### 21. Potential Solutions for Shader Bytecode Mismatch

**Option 1: Shader Extraction and Injection (Most Promising)**
1. Use UABEA to extract compiled Shader asset from original Felix bundle
2. Use UABEA to replace our compiled Shader with the original in our custom bundle
3. Keep our custom mesh/texture/material, but use game's working shader bytecode

**Option 2: Standard URP/Lit Shader Test**
1. Replace Endless_Shader_Character_NoFade with standard URP/Lit
2. Test if ANY shader renders correctly
3. If URP/Lit works, issue is specific to Endless shader compilation

**Option 3: Shader Source Modification**
1. Modify shader source to force specific blend states
2. Add explicit `Blend One Zero` in shader passes
3. May require decompiling and analyzing shader graph

**Option 4: Match Build Environment**
1. Identify exact Unity version, URP version, and build settings used by game
2. Match shader stripping settings in Project Settings → Graphics
3. Match URP Asset configuration exactly

**Recommended approach:** Option 1 - Extract original shader and inject into custom bundle

### 22. UABEA Shader Injection Attempt (FAILED)

**What was tried:**
1. Used UABEA to modify our custom bundle
2. Attempted to inject/replace shader from original Felix bundle

**Result:** Bundle grew from 9.6 MB to 14.7 MB (unexpected)

**Critical Problem:** CRC Mismatch
```
Game log: CRC Mismatch. Provided c2321e96, calculated 665041fd
```

**Root cause:** Unity's CRC algorithm ≠ Python's zlib.crc32()
- We used zlib.crc32() = 3258064534 (hex: c2321e96)
- Unity calculated internally = 1716601341 (hex: 665041fd)

**Conclusion:** Cannot modify bundles outside of Unity because:
1. Unity uses proprietary CRC algorithm
2. No way to calculate correct CRC for externally-modified bundles
3. Game rejects bundles with CRC mismatch

### 23. Remaining Options

**Option A: Reverse-engineer Unity's CRC algorithm**
- Research Unity source code or decompile
- Implement in Python for external bundle patching
- Risky: algorithm may change between Unity versions

**Option B: Test with standard URP/Lit shader**
- Replace Endless shader with Unity's built-in URP/Lit
- If it works: confirms issue is Endless shader compilation
- If silhouette: broader issue with our bundle

**Option C: Match exact build environment**
- Use identical Unity version (2022.3.62f2, not f3)
- Match exact URP version and settings
- Match shader stripping settings
- Most likely to produce compatible shader bytecode

**Option D: Disable CRC validation in game**
- Requires game patching/modding
- May violate ToS
- Not recommended

**Current recommendation:** Try Option B (URP/Lit test) then Option C (exact Unity version)

### 24. URP/Lit Shader Test (FAILED - Same Silhouette)

**Test performed:** Changed material shader from Endless_Shader_Character_NoFade to standard URP/Lit

**Result:** Still silhouette! Same as with Endless shader.

**Conclusion:** The issue is NOT specific to the Endless shader. ANY shader compiled by our Unity project shows silhouette.

**Root cause hypothesis:** Unity version mismatch
- Game uses: **2022.3.62f2**
- Our project: **2022.3.62f3**

Even one patch version difference (f2 vs f3) can cause:
- Different shader bytecode compilation
- Different material serialization
- Different asset bundle format

**Next step:** Install exact Unity 2022.3.62f2 and rebuild

### 25. Unity Version Installation

**To install Unity 2022.3.62f2:**
1. Open Unity Hub
2. Installs → Archive → Download Archive
3. Find 2022.3.62 (NOT f3, must be f2)
4. Or direct download: `unityhub://2022.3.62f2`

**After installation:**
1. Open project with 2022.3.62f2
2. Let it upgrade/convert if needed
3. Rebuild Addressables
4. Test again

### 26. Unity 2022.3.62f2 Test Result (PARTIAL SUCCESS!)

**Result:**
- **Lobby:** Still shows silhouette (different rendering path)
- **In-Game:** Pearl Diver character RENDERS! 🎉

**New Issue:** UV mapping appears incorrect - texture not aligned properly on mesh

**Root cause of UV issue:**
- Meshy AI generates mesh with its own UV layout
- The UV coordinates on the mesh may not match expected texture mapping
- Need to verify/fix UVs in Blender

**Lobby silhouette:** Likely uses different shader/rendering path than in-game. May need investigation.

### 27. UV Fixing Workflow

**Option A: Fix UVs in Blender**
1. Open Pearl Diver FBX in Blender
2. UV Editing workspace
3. Verify UVs match texture layout
4. Re-project or adjust UVs as needed
5. Re-export FBX

**Option B: Adjust in Unity Material**
1. Modify texture Scale/Offset in material
2. Trial and error to find correct mapping
3. Quick but imprecise

**Option C: Re-bake texture**
1. Bake texture to match current mesh UVs
2. Requires 3D software with baking capability

### 28. Final Working Configuration

**Material Settings (ThePack_Felix_VC_01_A.mat):**
```yaml
m_Shader: {fileID: -6465566751694194690, guid: e83c5035ac1520d4d822c8bf82b180c5, type: 3}
m_ValidKeywords:
  - _LIGHT_COOKIES
Textures:
  - _Albedo: Your character texture
  - Texture2D_033410430c1e41f38cbe37cd7347362e (MixMap): EMPTY (fileID: 0)
  - Other textures: Leave empty or default
```

**Key Discoveries:**
1. Felix's MixMap (Tiger_MixMap.png) causes visual artifacts on custom meshes
2. Leaving MixMap empty works correctly
3. `_LIGHT_COOKIES` keyword is required for proper rendering
4. Unity version MUST match exactly (2022.3.62f2)

### 29. Automated Deployment Script

Save as `deploy_character.py`:
```python
import json, base64, shutil, os, re, glob

# Configuration
UNITY_PROJECT = r'D:\Unity_Workshop\Endstar Custom Shader'
GAME_AA_PATH = r'C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\StreamingAssets\aa'
TARGET_BUNDLE = 'felix_assets_all_8ce5cfff23e50dfcaa729aa03940bfd7.bundle'
FELIX_ORIGINAL_CRC = '1004267194'

def deploy():
    # Find built bundle
    bundle_pattern = os.path.join(UNITY_PROJECT, 'Library', 'com.unity.addressables',
                                   'aa', 'Windows', 'StandaloneWindows64', '*.bundle')
    bundles = glob.glob(bundle_pattern)
    if not bundles:
        print('ERROR: No bundle found. Build Addressables first.')
        return
    built_bundle = bundles[0]
    print(f'Bundle: {os.path.basename(built_bundle)} ({os.path.getsize(built_bundle):,} bytes)')

    # Get CRC from Unity catalog
    unity_catalog = os.path.join(UNITY_PROJECT, 'Library', 'com.unity.addressables',
                                  'aa', 'Windows', 'catalog.json')
    with open(unity_catalog, 'r') as f:
        unity_cat = json.load(f)
    extra = base64.b64decode(unity_cat['m_ExtraDataString'])
    idx = extra.find('m_Crc'.encode('utf-16-le'))
    context = extra[idx:idx+60].decode('utf-16-le', errors='ignore')
    new_crc = re.search(r'm_Crc.:(\d+)', context).group(1)
    print(f'Unity CRC: {new_crc}')

    # Restore and patch game catalog
    backup_catalog = os.path.join(GAME_AA_PATH, 'catalog_original_backup.json')
    game_catalog = os.path.join(GAME_AA_PATH, 'catalog.json')
    shutil.copy(backup_catalog, game_catalog)

    with open(game_catalog, 'r') as f:
        cat_json = json.load(f)
    extra_data = base64.b64decode(cat_json['m_ExtraDataString'])
    extra_data = extra_data.replace(
        FELIX_ORIGINAL_CRC.encode('utf-16-le'),
        new_crc.encode('utf-16-le'), 1)
    cat_json['m_ExtraDataString'] = base64.b64encode(extra_data).decode('ascii')
    with open(game_catalog, 'w') as f:
        json.dump(cat_json, f, separators=(',', ':'))
    print('CRC patched')

    # Copy bundle
    dest = os.path.join(GAME_AA_PATH, 'StandaloneWindows64', TARGET_BUNDLE)
    shutil.copy(built_bundle, dest)
    print(f'Deployed to {dest}')
    print('\\nLaunch game with: Endstar.exe (NOT via launcher)')

if __name__ == '__main__':
    deploy()
```

### 30. Known Limitations

1. **Lobby/Party shows silhouette** - Uses different rendering path, character only renders properly in-game
2. **Launcher restores files** - Must launch game directly via Endstar.exe
3. **One character at a time** - Each deployment replaces Felix only
4. **No animation retargeting** - Uses Felix's humanoid animations

### 31. Party/Lobby Silhouette Investigation (UNSOLVED)

**Symptom:** Character appears as silhouette in Party screen (character selection) but renders correctly in-game.

**Status:** UNSOLVED - This is a known limitation.

**What was tested:**
- Shader keywords (_LIGHT_COOKIES) - doesn't affect Party
- Metallic/Smoothness values (Vector1 parameters) - must stay at 0, setting to 1 makes character shiny like metal
- Different shaders - same issue

**Important Material Notes:**
```
Vector1_195718c9f68f499782987983967ae661 = 0 (SSS/Metallic - KEEP AT 0!)
Vector1_844a91d1120546378bc600c39ce4fc77 = 0 (SSS/Smoothness - KEEP AT 0!)
Setting these to 1 makes character appear metallic/shiny - NOT what we want!
```

**Root Cause Theory:**
The Party/lobby screen uses a different render pipeline than gameplay. This is common in games for performance - UI character previews often use simplified shaders or render-to-texture with limited passes. The custom character bundle may not include the necessary shader variants for this specific render path.

**Workaround:**
None currently. The character only renders correctly during actual gameplay. Party screen shows silhouette but gameplay works fine.

### 32. Mod Patcher GUI - Complete Documentation

The Mod Patcher is a standalone Windows application that allows end users to easily install custom character mods for Endstar without any technical knowledge.

#### 32.1 Files Created

```
D:\Endstar Plot Flag\PLOT FLAG SYSTEM\
├── EndstarModPatcherGUI.py     - Main GUI source code
├── EndstarModPatcher.exe       - Compiled GUI executable (~31 MB)
├── endless_mod_icon_512x512.png - Source icon image (512x512)
├── endless_mod_icon.ico        - Multi-resolution icon (18 KB)
├── endstar_logo.png            - Logo displayed in GUI (35 KB)
├── PearlDiver.esmod            - Example mod package (7.3 MB)
└── mods\
    └── PearlDiver\
        ├── bundle.bundle       - The Unity asset bundle
        ├── mod.json            - Mod metadata with CRC
        └── bundle.bundle.crc   - CRC value backup
```

#### 32.2 The .esmod File Format

The `.esmod` format is a simple ZIP archive containing exactly two files:

**Structure:**
```
ModName.esmod (ZIP archive)
├── bundle.bundle    - The Unity asset bundle (character data)
└── mod.json         - Mod metadata including CRC
```

**mod.json Format:**
```json
{
  "name": "Pearl Diver",
  "author": "Bora",
  "version": "1.0.0",
  "description": "Custom Pearl Diver character replacing Felix",
  "crc": "1464933460"
}
```

**Field Descriptions:**
| Field | Required | Description |
|-------|----------|-------------|
| `name` | Yes | Display name shown to user |
| `author` | No | Creator's name |
| `version` | No | Mod version (semantic versioning recommended) |
| `description` | No | Brief description of the mod |
| `crc` | **CRITICAL** | Unity's CRC value from built catalog |

**Creating an .esmod file:**
```python
import zipfile
import json

# Create mod.json
mod_info = {
    "name": "My Character",
    "author": "Your Name",
    "version": "1.0.0",
    "description": "Custom character replacing Felix",
    "crc": "1234567890"  # From Unity catalog
}

with open('mod.json', 'w') as f:
    json.dump(mod_info, f, indent=2)

# Create .esmod package
with zipfile.ZipFile('MyCharacter.esmod', 'w', zipfile.ZIP_DEFLATED) as zf:
    zf.write('bundle.bundle')  # Your Unity bundle
    zf.write('mod.json')
```

#### 32.3 GUI Patcher Features

**Auto-Detection:**
The GUI automatically searches for Endstar installation in:
1. Default path: `C:\Endless Studios\Endless Launcher\Endstar\`
2. Common variations (Program Files, Program Files (x86))
3. Windows Registry (uninstall entries)
4. All drives: `D:\`, `E:\`, etc.
5. Common folders: `Endless Studios`, `Games`, `Program Files`

**If auto-detection fails:** User can manually select `Endstar.exe`

**User Interface:**
```
┌────────────────────────────────────────┐
│         [Endstar Logo Image]            │
│            Mod Patcher                  │
│       Custom Character Installer        │
├────────────────────────────────────────┤
│ Mod File                               │
│ ┌──────────────────────────────────┐   │
│ │ Pearl Diver                      │   │
│ │ [Select Mod File (.esmod)]       │   │
│ └──────────────────────────────────┘   │
├────────────────────────────────────────┤
│ [Install Mod] [Uninstall] [Launch]     │
├────────────────────────────────────────┤
│ ⚠ Launch game directly - NOT via       │
│   Endless Launcher!                    │
├────────────────────────────────────────┤
│ Found: C:\...\Endstar [Locate Game]    │
└────────────────────────────────────────┘
```

**Buttons:**
- **Install Mod** - Backs up originals, copies bundle, patches CRC
- **Uninstall** - Restores original Felix bundle and catalog
- **Launch Game** - Starts Endstar.exe directly (bypasses launcher)
- **Locate Game** - Manual game path selection

#### 32.4 Technical Implementation

**Game Detection Algorithm:**
```python
def find_game():
    # 1. Check default paths
    paths = [
        r'C:\Endless Studios\Endless Launcher\Endstar',
        r'C:\Program Files\Endless Studios\...',
        r'C:\Program Files (x86)\Endless Studios\...',
    ]

    # 2. Check Windows Registry
    # HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall
    # Look for "Endless" or "Endstar" in DisplayName

    # 3. Search all drives (A-Z)
    for drive in 'CDEFGHIJ...':
        for folder in ['Endless Studios', 'Games', 'Program Files']:
            check(f'{drive}:\{folder}\...\Endstar.exe')
```

**CRC Patching (Core Logic):**
```python
def patch_catalog_crc(new_crc):
    # 1. Restore original catalog from backup
    shutil.copy(backup_catalog, catalog)

    # 2. Read and decode m_ExtraDataString
    with open(catalog) as f:
        cat = json.load(f)
    extra_data = base64.b64decode(cat['m_ExtraDataString'])

    # 3. Replace Felix's CRC with new CRC (UTF-16LE encoded!)
    old_bytes = '1004267194'.encode('utf-16-le')
    new_bytes = str(new_crc).encode('utf-16-le')
    extra_data = extra_data.replace(old_bytes, new_bytes, 1)

    # 4. Re-encode and save
    cat['m_ExtraDataString'] = base64.b64encode(extra_data).decode('ascii')
    with open(catalog, 'w') as f:
        json.dump(cat, f, separators=(',', ':'))
```

**CRITICAL:** The CRC is stored in UTF-16LE encoding inside a base64-encoded binary blob!

**Backup Strategy:**
```
Before any modification:
- catalog.json → catalog_original_backup.json
- felix_bundle.bundle → felix_bundle.bundle.original

On uninstall:
- Restore both files from backups
```

#### 32.5 Creating the Icon

**Source file:** `endless_mod_icon_512x512.png` (512x512 PNG)

**Create multi-resolution ICO:**
```python
from PIL import Image

img = Image.open('endless_mod_icon_512x512.png').convert('RGBA')
img256 = img.resize((256, 256), Image.LANCZOS)
img48 = img.resize((48, 48), Image.LANCZOS)
img32 = img.resize((32, 32), Image.LANCZOS)
img16 = img.resize((16, 16), Image.LANCZOS)

img256.save('endless_mod_icon.ico', format='ICO',
            append_images=[img48, img32, img16])
```

**Output:** `endless_mod_icon.ico` (~18 KB with 16, 32, 48, 256 pixel sizes)

#### 32.6 Building the Executable

**Requirements:**
```
pip install pyinstaller pillow
```

**Build Command:**
```powershell
cd "D:\Endstar Plot Flag\PLOT FLAG SYSTEM"

# Clean previous build
Remove-Item build, dist -Recurse -Force -ErrorAction SilentlyContinue

# Build GUI patcher with custom icon
pyinstaller --onefile --windowed --icon=endless_mod_icon.ico --name=EndstarModPatcher EndstarModPatcherGUI.py
```

**Build Output:**
```
dist\EndstarModPatcher.exe   - ~31 MB (GUI)
```

**Asset Distribution:**
The following files must be distributed alongside the .exe:
```
EndstarModPatcher.exe
endless_mod_icon.ico  - For taskbar icon (18 KB)
endstar_logo.png      - For GUI logo display (35 KB)
```

**Note on Python 3.14:** PyInstaller's `--add-data` flag has issues with Python 3.14. Assets must be placed in the same folder as the exe rather than bundled inside.

**Windows Icon Cache:** After rebuilding, clear icon cache to see new icon:
```powershell
Remove-Item "$env:LOCALAPPDATA\IconCache.db" -Force -ErrorAction SilentlyContinue
Remove-Item "$env:LOCALAPPDATA\Microsoft\Windows\Explorer\iconcache*" -Force -ErrorAction SilentlyContinue
```

### 33. Complete Distribution Package

**For End Users (What to Distribute):**
```
EndstarModPatcher/
├── EndstarModPatcher.exe      - The GUI installer (~31 MB)
├── endless_mod_icon.ico       - Taskbar icon (18 KB)
├── endless_mod_icon_512x512.png - Source icon (for reference)
├── endstar_logo.png           - GUI logo (35 KB)
├── README.txt                 - User instructions
└── mods/
    └── PearlDiver.esmod       - Example mod (7.3 MB)
```

**README.txt for Users:**
```
ENDSTAR MOD PATCHER
===================

1. Run EndstarModPatcher.exe
2. Click "Select Mod File (.esmod)"
3. Choose the mod you want to install
4. Click "Install Mod"
5. Click "Launch Game"

IMPORTANT: Always launch the game using this patcher
or by running Endstar.exe directly.
DO NOT use the Endless Launcher after installing mods!

To uninstall: Click "Uninstall" to restore original files.
```

**For Mod Creators (How to Package):**
1. Build character in Unity 2022.3.62f2
2. Build Addressables
3. Copy bundle from: `Library/com.unity.addressables/aa/Windows/StandaloneWindows64/`
4. Get CRC from: `Library/com.unity.addressables/aa/Windows/catalog.json`
   - Decode `m_ExtraDataString` (base64)
   - Find `m_Crc` value in UTF-16LE decoded string
5. Create mod.json with the CRC value
6. ZIP bundle.bundle + mod.json → rename to .esmod
7. Distribute the .esmod file

**Package Sizes:**
- Executable: ~12 MB
- Each mod: ~7-10 MB (depends on character complexity)
- Total with one mod: ~20 MB

### 33.1 Mod Patcher Error Handling & Troubleshooting

**Common Errors and Solutions:**

| Error | Cause | Solution |
|-------|-------|----------|
| "Could not find original CRC in catalog" | Catalog already modified or corrupted | Delete catalog.json, restore from backup or reinstall game |
| "Invalid mod file: missing mod.json" | Mod package malformed | Ensure .esmod contains both bundle.bundle AND mod.json |
| "Invalid mod file: missing .bundle" | Bundle not found in package | Check that bundle file has .bundle extension |
| "Could not launch game!" | Endstar.exe not found | Use "Locate Game" to manually select |
| Game shows Tilly instead of mod | CRC mismatch or bundle not copied | Re-run install, check file permissions |

**Debug Mode:**
Run the Python script directly to see detailed error messages:
```powershell
cd "D:\Endstar Plot Flag\PLOT FLAG SYSTEM"
python EndstarModPatcherGUI.py
```

**Verifying Installation:**
```powershell
# Check if bundle was replaced (should show recent timestamp)
Get-Item 'C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\StreamingAssets\aa\StandaloneWindows64\felix_assets_all*.bundle' | Select-Object Length, LastWriteTime

# Check if backups exist
Test-Path 'C:\...\aa\catalog_original_backup.json'
Test-Path 'C:\...\aa\StandaloneWindows64\felix_*.bundle.original'
```

**Game Log Location:**
```
C:\Users\[Username]\AppData\LocalLow\Endless Studios\Endstar\Player.log
```

Look for CRC mismatch errors:
```
CRC Mismatch. Provided XXXXXX, calculated YYYYYY
```

### 33.2 Security Considerations

**What the Mod Patcher Does:**
1. Creates backup copies of original game files
2. Copies mod bundle over Felix's bundle
3. Modifies catalog.json to update CRC checksum
4. Launches Endstar.exe directly

**What it Does NOT Do:**
- Does not modify any .exe files
- Does not inject code into the game process
- Does not bypass anti-cheat (Endstar doesn't have active anti-cheat)
- Does not communicate over network
- Does not require admin privileges (unless game is in protected folder)

**File Integrity:**
The patcher only modifies these two files:
```
catalog.json - CRC value patched
felix_*.bundle - Replaced with mod bundle
```

Original files are backed up and can be fully restored.

### 34. Blender Mesh Workflow (CRITICAL - Complete Step-by-Step)

This section details exactly how to prepare a custom mesh in Blender for Endstar.

**⚠️ CRITICAL: The character mesh MUST be in A-pose, NOT T-pose!**
- Endstar uses A-pose for all character animations
- T-pose meshes will have broken/distorted arm animations
- If your mesh is in T-pose, rotate the arms down ~45° to match A-pose before rigging

#### Step 1: Extract Original Felix from Game

1. Open AssetStudio
2. Load Felix bundle: `felix_assets_all_8ce5cfff23e50dfcaa729aa03940bfd7.bundle`
3. Filter by Type: **Mesh**
4. Select all TigerGuy meshes (LOD0, LOD1, LOD2)
5. Export → Selected Objects → Save as FBX or OBJ
6. Also export the **Prefab** to understand hierarchy

#### Step 2: Original Felix Hierarchy Structure

The original Felix prefab has this hierarchy:
```
CharacterCosmetics_ThePack_Felix_01_A (Empty - ROOT)
├── TigerGuy_Body (Armature/Skeleton)
│   ├── Hips
│   │   ├── Spine
│   │   │   ├── Chest
│   │   │   │   ├── Neck
│   │   │   │   │   └── Head
│   │   │   │   ├── LeftShoulder → LeftArm → LeftForeArm → LeftHand
│   │   │   │   └── RightShoulder → RightArm → RightForeArm → RightHand
│   │   ├── LeftUpLeg → LeftLeg → LeftFoot → LeftToeBase
│   │   └── RightUpLeg → RightLeg → RightFoot → RightToeBase
├── TigerGuy_LOD0 (Mesh - highest detail)
├── TigerGuy_LOD1 (Mesh - medium detail)
└── TigerGuy_LOD2 (Mesh - lowest detail)
```

**CRITICAL:** All mesh objects are direct children of the root Empty, NOT children of the armature.

#### Step 3: Import Files to Blender

1. Start Blender with empty scene (File → New → General, delete default cube)
2. Import Felix FBX:
   ```
   File → Import → FBX (.fbx)
   Select: CharacterCosmetics_ThePack_Felix_01_A.fbx
   ```
3. Import your custom mesh (Pearl Diver OBJ):
   ```
   File → Import → Wavefront (.obj)
   Select: Your_Custom_Character.obj
   ```

#### Step 4: Scale and Position Custom Mesh

1. Select your custom mesh in the Outliner
2. Press `S` to scale, type `0.7` and press Enter (scale to match Felix size)
3. Press `G` to grab/move, adjust position to match Felix's origin
4. Apply transformations: `Ctrl+A` → All Transforms

#### Step 5: Rename Mesh to Match LOD Pattern

**CRITICAL NAMING:**
- Your mesh MUST be named to match Felix's LOD naming pattern
- Rename to: `PearlDiver_LOD0` (or whatever your character name is)

```
In Outliner:
1. Double-click mesh name
2. Change from "Meshy_AI_..." to "PearlDiver_LOD0"
```

#### Step 6: Parent to Root Empty

**THIS IS THE MOST CRITICAL STEP:**

1. Select your mesh (PearlDiver_LOD0)
2. Shift+Click to also select the root Empty (`CharacterCosmetics_ThePack_Felix_01_A`)
3. **Parent the mesh**: `Ctrl+P` → Object (Keep Transform)

The hierarchy should now look like:
```
CharacterCosmetics_ThePack_Felix_01_A (Empty - ROOT)
├── TigerGuy_Body (Armature)
├── TigerGuy_LOD0 (Original mesh - can delete)
├── TigerGuy_LOD1 (Original mesh - can delete)
├── TigerGuy_LOD2 (Original mesh - can delete)
└── PearlDiver_LOD0 (YOUR NEW MESH) ← MUST be here!
```

#### Step 7: Optional - Delete Original Meshes

If replacing Felix completely:
1. Select TigerGuy_LOD0, LOD1, LOD2
2. Press `X` → Delete

#### Step 8: Verify Hierarchy

Run this in Blender's Python console to verify:
```python
for obj in bpy.data.objects:
    parent_name = obj.parent.name if obj.parent else "ROOT"
    print(f"{obj.name} [{obj.type}] -> parent: {parent_name}")
```

Expected output:
```
CharacterCosmetics_ThePack_Felix_01_A [EMPTY] -> parent: ROOT
TigerGuy_Body [ARMATURE] -> parent: CharacterCosmetics_ThePack_Felix_01_A
PearlDiver_LOD0 [MESH] -> parent: CharacterCosmetics_ThePack_Felix_01_A
```

#### Step 9: Export FBX for Unity

1. Select the root Empty (`CharacterCosmetics_ThePack_Felix_01_A`)
2. File → Export → FBX (.fbx)
3. Export settings:
   - Selected Objects: ON
   - Object Types: Empty, Armature, Mesh
   - Add Leaf Bones: OFF
   - Bake Animation: OFF (unless you have animations)
4. Save as: `CharacterCosmetics_ThePack_Felix_01_A_with_PearlDiver.fbx`

### 35. Blender Automation Script

Save this as `prepare_custom_character.py` and run in Blender:

```python
import bpy
import os

# Configuration
FELIX_FBX = r'D:\Endstar Plot Flag\PLOT FLAG SYSTEM\Felix\CharacterCosmetics_ThePack_Felix_01_A.fbx'
CUSTOM_OBJ = r'D:\Endstar Plot Flag\PLOT FLAG SYSTEM\Your_Custom_Character.obj'
CUSTOM_NAME = 'PearlDiver'
SCALE = 0.7
OUTPUT_FBX = r'D:\Endstar Plot Flag\PLOT FLAG SYSTEM\CharacterCosmetics_ThePack_Felix_01_A_with_Custom.fbx'

# Clear scene
bpy.ops.wm.read_factory_settings(use_empty=True)

# Import Felix FBX
bpy.ops.import_scene.fbx(filepath=FELIX_FBX)

# Import custom OBJ
bpy.ops.wm.obj_import(filepath=CUSTOM_OBJ)

# Find objects
root_empty = None
custom_mesh = None
for obj in bpy.data.objects:
    if obj.name == 'CharacterCosmetics_ThePack_Felix_01_A' and obj.type == 'EMPTY':
        root_empty = obj
    if 'Meshy' in obj.name or obj.type == 'MESH' and 'Tiger' not in obj.name:
        custom_mesh = obj

if not root_empty or not custom_mesh:
    print("ERROR: Could not find required objects!")
else:
    # Rename and configure custom mesh
    custom_mesh.name = f'{CUSTOM_NAME}_LOD0'
    custom_mesh.scale = (SCALE, SCALE, SCALE)

    # Parent to root empty
    custom_mesh.parent = root_empty
    custom_mesh.matrix_parent_inverse = root_empty.matrix_world.inverted()

    # Apply transforms
    bpy.ops.object.select_all(action='DESELECT')
    custom_mesh.select_set(True)
    bpy.context.view_layer.objects.active = custom_mesh
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)

    # Delete original Felix meshes
    for obj in list(bpy.data.objects):
        if 'TigerGuy' in obj.name and obj.type == 'MESH':
            bpy.data.objects.remove(obj, do_unlink=True)

    # Export FBX
    bpy.ops.export_scene.fbx(
        filepath=OUTPUT_FBX,
        object_types={'EMPTY', 'ARMATURE', 'MESH'},
        add_leaf_bones=False,
        bake_anim=False
    )

    print(f"SUCCESS! Exported to: {OUTPUT_FBX}")

# Print final hierarchy
print("\nFinal Hierarchy:")
for obj in sorted(bpy.data.objects, key=lambda x: x.name):
    parent_name = obj.parent.name if obj.parent else "ROOT"
    print(f"  {obj.name} [{obj.type}] -> parent: {parent_name}")
```

### 36. Unity Import & Addressables Setup

#### Step 1: Import FBX to Unity

1. Open Unity 2022.3.62f2 (MUST be this exact version!)
2. Drag FBX to `Assets/Pearl Diver/` folder
3. Select FBX in Project window
4. In Inspector → Model tab:
   - Scale Factor: 1
   - Import Visibility: ON
   - Import Cameras: OFF
   - Import Lights: OFF

#### Step 2: Setup Prefab

1. Drag FBX to Hierarchy to create instance
2. Create new material: `Assets/Pearl Diver/ThePack_Felix_VC_01_A.mat`
3. Assign material to mesh renderer
4. Drag instance back to `Assets/Prefabs/CharacterCosmetics/` as Prefab
5. Name: `CharacterCosmetics_ThePack_Felix_01_A.prefab`

#### Step 3: Configure Material (CRITICAL!)

In the material file, ensure these settings:
```yaml
m_Shader: Endless_Shader_Character_NoFade (from SDK)
m_ValidKeywords:
  - _LIGHT_COOKIES
_Albedo: Your texture
Texture2D_033410430c1e41f38cbe37cd7347362e (MixMap): EMPTY!
Vector1_195718c9f68f499782987983967ae661: 0 (KEEP AT 0 - not 1!)
Vector1_844a91d1120546378bc600c39ce4fc77: 0 (KEEP AT 0 - not 1!)
```

**WARNING:** Setting Vector1 parameters to 1 makes the character metallic/shiny!

#### Step 4: Setup Addressables

1. Window → Asset Management → Addressables → Groups
2. Select your prefab in Project
3. Check "Addressable" in Inspector
4. Set Address to: `Assets/Prefabs/CharacterCosmetics/CharacterCosmetics_ThePack_Felix_01_A.prefab`
5. Build → New Build → Default Build Script

#### Step 5: Get CRC & Deploy

1. Find built catalog: `Library/com.unity.addressables/aa/Windows/catalog.json`
2. Decode `m_ExtraDataString` (base64 + UTF-16LE) to find `m_Crc` value
3. Run `deploy_character.py` or use the mod installer

---

## Catalog Structure Reference

### Felix in Game Catalog
```
Bundle Index: 8
Bundle Path: {RuntimePath}\StandaloneWindows64\felix_assets_all_8ce5cfff23e50dfcaa729aa03940bfd7.bundle

Prefab Index: 84 (approx)
Prefab Path: Assets/Prefabs/CharacterCosmetics/CharacterCosmetics_ThePack_Felix_01_A.prefab
```

### Character Naming Convention
- Prefab: `CharacterCosmetics_[Faction]_[Name]_[Version].prefab`
- Bundle: `[name]_assets_all_[hash].bundle`

---

## Quick Reference Commands

### Verify Bundle Replaced
```powershell
Get-Item 'C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\StreamingAssets\aa\StandaloneWindows64\felix_assets_all_8ce5cfff23e50dfcaa729aa03940bfd7.bundle' | Select-Object Length, LastWriteTime
```

### Compare Bundle Sizes
```
Original Felix: 3,644,563 bytes
Custom Bundle:  ~3,877,xxx bytes (varies)
```

### Check Built Catalog
```powershell
Get-Content 'D:\Unity_Workshop\Endstar Custom Shader\Library\com.unity.addressables\aa\Windows\catalog.json' | ConvertFrom-Json | Select-Object -ExpandProperty m_InternalIds
```

---

## Files Created During This Project

| File | Purpose |
|------|---------|
| `replace_bundle.ps1` | Automates bundle replacement |
| `export_uv_layout.py` | Visualize UV mapping |
| `decode_catalog.py` | Analyze catalog binary data |
| `Felix_UV_Layout.png` | UV wireframe on texture |
| `Felix_UV_Wireframe.png` | Clean UV wireframe |
| `felix_original_backup.bundle` | Original Felix bundle backup |
| `TEXTURE_SWAP_GUIDE.md` | Texture replacement guide |
| `CHARACTER_INJECTION_WORKFLOW.md` | Initial workflow doc |
| `ASSETSTUDIO_EXTRACTION_GUIDE.md` | Asset extraction guide |

---

## Current Status Summary

**✅ FULLY WORKING (January 4, 2026)**

**What Works:**
- ✅ Asset extraction with AssetStudio
- ✅ Blender rigging workflow (OBJ import method)
- ✅ Unity Addressables bundle building
- ✅ CRC patching in game catalog (must use Unity's CRC from built catalog)
- ✅ Bundle replacement and loading
- ✅ Bypassing launcher by running Endstar.exe directly
- ✅ Material keyword extraction via UABEA
- ✅ `_LIGHT_COOKIES` keyword enables proper rendering
- ✅ **Unity 2022.3.62f2 produces compatible shader bytecode**
- ✅ **Custom character renders correctly in-game!**
- ✅ **Display name modification** (binary replace in sharedassets0.assets)
- ✅ **Portrait sprite replacement** (binary replace in sharedassets0.assets.resS)

**What Doesn't Work:**
- ⚠️ Lobby shows silhouette (different render path - cosmetic only)
- ⚠️ Launcher restores modified files (must launch directly)
- ⚠️ Names must be same length (5 chars) without UABEA repacking

**Key Success Factors:**
1. Unity version **2022.3.62f2** (exact match to game)
2. `_LIGHT_COOKIES` in material keywords
3. NO MixMap texture (leave empty)
4. Use Unity's CRC from built catalog, NOT zlib.crc32

---

## Shader Injection Workflow (UABEA)

### Step 1: Extract Original Shader from Felix Bundle
1. Open UABEA
2. **File → Open** → Load original Felix bundle backup:
   ```
   D:\Endstar Plot Flag\PLOT FLAG SYSTEM\felix_original_backup.bundle
   ```
3. In asset list, find the **Shader** asset (Type = Shader)
4. Select it → **Export Raw** → Save as `original_shader.dat`

### Step 2: Open Custom Bundle
1. **File → Open** → Load our built bundle:
   ```
   D:\Unity_Workshop\Endstar Custom Shader\Library\com.unity.addressables\aa\Windows\StandaloneWindows64\defaultlocalgroup_assets_all_XXXX.bundle
   ```
2. Find the **Shader** asset in asset list

### Step 3: Replace Shader
1. Select our Shader asset
2. **Import Raw** → Select `original_shader.dat`
3. **File → Save** to save modified bundle

### Step 4: Deploy and Test
1. Copy modified bundle to game directory (replacing Felix bundle)
2. Update CRC in catalog if bundle size changed
3. Launch game and test

### Important Notes:
- Shader PathID must match between original and replacement
- Material references shader by PathID, so IDs must align
- May need to also copy shader dependencies (if any)
- CRC will change after modification - must re-patch catalog

---

## Future Improvements Needed

1. **Shader compatibility** - Match shader compilation to game's expectations
2. **Permanent launcher bypass** - Find way to prevent launcher from restoring files
3. **Automated pipeline** - Script entire process from Blender to game
4. **Weight painting** - Better bone weight transfer for accurate deformation
5. **Testing framework** - Verify character loads correctly before deployment

---

## 37. Party/Lobby Silhouette - Deep Investigation (January 4, 2026)

### Investigation Summary

Used UnityPy to analyze and compare the original Felix bundle with the modded Pearl Diver bundle.

### Bundle Comparison Results

**Asset Structure:** IDENTICAL
| Asset Type | Original | Modded |
|------------|----------|--------|
| GameObject | 64 | 64 |
| Material | 1 | 1 |
| Mesh | 3 | 3 |
| Shader | 2 | 2 |
| Texture2D | 2 | 2 |
| SkinnedMeshRenderer | 3 | 3 |

**Material Properties:** IDENTICAL
- Both have `_LIGHT_COOKIES` keyword
- Both have same float properties (no differences)
- Both have same texture slot configuration
- `_Albedo`: Has texture
- `_Pre_Integrated_Scattering`: Has texture
- MixMap (`Texture2D_033410430c1e41f38cbe37cd7347362e`): Empty

### Root Cause Analysis

The lobby/party area uses a **completely different rendering system** than gameplay:

1. **Gameplay rendering**: Uses the character bundle's material and mesh directly → Works correctly
2. **Lobby rendering**: Uses a separate UI preview system that may:
   - Use simplified shaders for performance
   - Render to a texture for UI display
   - Apply silhouette/outline effect intentionally for unowned characters
   - Load character data from a different source

### Character Selection UI Investigation

Searched all game assets for character icons/portraits:
- **Bundle assets**: Only contain 3D models, no 2D icons
- **globalgamemanagers.assets**: Only engine textures, no character icons
- **Catalog addresses**: Only CharacterCosmetics prefabs, no icon assets

**Conclusion**: Character selection icons are either:
1. Generated dynamically from 3D models at runtime
2. Downloaded from game server
3. Hardcoded in game DLLs (Data.dll, Assets.dll)

### What Would Be Needed to Fix

**For Silhouette Issue:**
1. Identify the lobby's render pipeline code in game DLLs
2. Find which shader/material the lobby preview uses
3. Either:
   - Modify the lobby rendering system (requires DLL patching)
   - Provide correct assets the lobby expects (unknown format)

**For Character Selection UI:**
1. Find where character metadata (name, icon) is stored
2. Add custom character entry to that data source
3. This likely requires server-side changes or DLL modification

### Files Analyzed

```
Original Bundle: felix_assets_all_...bundle.original (3.6 MB)
Modded Bundle: felix_assets_all_...bundle (8.2 MB)
Catalog: catalog.json (98 KB)
Game Managers: globalgamemanagers.assets
```

### Status: UNSOLVED

The silhouette issue cannot be fixed through bundle modification alone. The lobby uses a different rendering system that is controlled by game code, not the character bundle.

**Recommended Approach for Future:**
1. Use IL2CPP decompilation tools to analyze game logic
2. Find the lobby character preview rendering code
3. Identify what data/assets it expects
4. Potentially use runtime patching (BepInEx/MelonLoader) to override behavior

---

## 38. Game Code Analysis - Character Cosmetics System (January 4, 2026)

### Overview

Used dotPeek to decompile and analyze the game's DLLs to understand how the character cosmetics system works. This revealed the complete architecture of character selection, display, and rendering.

### Key DLLs Analyzed

| DLL | Size | Contents |
|-----|------|----------|
| **Gameplay.dll** | 1.6 MB | Character rendering, cosmetics system, appearance controllers |
| **Creator.dll** | 1.0 MB | UI systems, character selection views |
| **Shared.dll** | 834 KB | Shared data types, utilities |
| **Data.dll** | 192 KB | Data structures |

### Core Classes Discovered

#### 1. `CharacterCosmeticsDefinition` (ScriptableObject)
**Location:** `Endless.Gameplay` namespace in Gameplay.dll

This is the main data class for each character cosmetic:

```csharp
[CreateAssetMenu(menuName = "ScriptableObject/Character Cosmetics/Character Cosmetics Definition")]
public class CharacterCosmeticsDefinition : ScriptableObject
{
    [SerializeField] private string displayName;                    // "Felix", "Tilly", etc.
    [SerializeField] private string assetId;                        // GUID string
    [SerializeField] private AssetReferenceGameObject assetReference;  // Addressables reference
    [SerializeField] private Sprite portraitSprite;                 // 2D icon for UI

    public string DisplayName => this.displayName;
    public SerializableGuid AssetId => (SerializableGuid)this.assetId;
    public Sprite PortraitSprite => this.portraitSprite;

    public AsyncOperationHandle<GameObject> Instantiate(Transform parent = null, bool isInstantiatedInWorldSpace = false)
    {
        // Loads 3D model via Unity Addressables
        return this.assetReference.InstantiateAsync(parent, isInstantiatedInWorldSpace);
    }
}
```

**Key Properties:**
- `displayName` - The name shown in character selection UI
- `assetId` - GUID that uniquely identifies this character
- `assetReference` - Unity Addressables reference to the 3D prefab bundle
- `portraitSprite` - 2D sprite shown in character selection grid

#### 2. `CharacterCosmeticsList` (ScriptableObject)
**Location:** `Endless.Gameplay` namespace

Master list containing all available character cosmetics:

```csharp
public class CharacterCosmeticsList : ScriptableObject
{
    [SerializeField] private List<CharacterCosmeticsDefinition> cosmetics;
    private Dictionary<SerializableGuid, CharacterCosmeticsDefinition> definitionMap;

    public IReadOnlyList<CharacterCosmeticsDefinition> Cosmetics => this.cosmetics;

    public bool TryGetDefinition(SerializableGuid assetId, out CharacterCosmeticsDefinition definition)
    {
        // Lookup by GUID
    }
}
```

#### 3. `CharacterCosmeticsDefinitionUtility` (Static Class)
**Location:** `Endless.Gameplay` namespace

Manages the player's selected character via PlayerPrefs:

```csharp
public static class CharacterCosmeticsDefinitionUtility
{
    public static Action<SerializableGuid> ClientCharacterCosmeticsDefinitionAssetSetAction;

    public static SerializableGuid GetClientCharacterVisualId()
    {
        // Reads from PlayerPrefs "Character Visual"
        return (SerializableGuid)PlayerPrefs.GetString("Character Visual");
    }

    public static void SetClientCharacterVisualId(SerializableGuid id)
    {
        // Saves to PlayerPrefs "Character Visual"
        PlayerPrefs.SetString("Character Visual", (string)id);
    }
}
```

**Important:** Player's selected character is stored in `PlayerPrefs` with key `"Character Visual"` as a GUID string.

#### 4. `UICharacterCosmeticsDefinitionPortraitView` (MonoBehaviour)
**Location:** `Endless.Gameplay.UI` namespace

Displays character portrait and name in the UI:

```csharp
public class UICharacterCosmeticsDefinitionPortraitView : UIGameObject
{
    [SerializeField] private Image portraitImage;
    [SerializeField] private TextMeshProUGUI displayNameText;
    [SerializeField] private CharacterCosmeticsList characterCosmeticsList;
    [SerializeField] private CharacterCosmeticsDefinition fallbackCharacterCosmeticsDefinition;

    public void Display(SerializableGuid characterCosmeticsDefinitionAssetId)
    {
        CharacterCosmeticsDefinition definition;
        if (characterCosmeticsList.TryGetDefinition(assetId, out definition))
        {
            this.portraitImage.sprite = definition.PortraitSprite;  // <-- Icon comes from here
            this.displayNameText.text = definition.DisplayName;     // <-- Name comes from here
        }
    }
}
```

#### 5. `AppearanceController` (MonoBehaviour)
**Location:** `Endless.Gameplay` namespace

Handles 3D character rendering and cosmetics instantiation:

```csharp
public class AppearanceController : MonoBehaviour
{
    private void UpdateCharacterCosmetics(CharacterCosmeticsDefinition cosmeticsDefinition)
    {
        // Instantiates the 3D model from the Addressables bundle
        cosmeticsDefinition.Instantiate(parent).Completed += HandleCosmeticInstantiation;
    }

    private void ApplyCosmeticsGameObject(GameObject cosmetics)
    {
        // CRITICAL: Shader replacement happens here!
        foreach (Renderer renderer in componentsInChildren)
        {
            foreach (Material material in renderer.materials)
            {
                // Game explicitly swaps shader at runtime
                string name = material.shader.name == "Shader Graphs/Endless_Shader"
                    ? "Shader Graphs/Endless_Shader_Character_NoFade"
                    : material.shader.name;
                material.shader = Shader.Find(name);
            }
        }
    }
}
```

**Important Discovery:** The game explicitly calls `Shader.Find()` to replace shaders at runtime! If our shader name doesn't match `"Shader Graphs/Endless_Shader"` or `"Shader Graphs/Endless_Shader_Character_NoFade"`, it won't be swapped correctly.

#### 6. `PlayerReferenceManager` (NetworkBehaviour)
**Location:** `Endless.Gameplay` namespace

Manages character cosmetics over the network:

```csharp
public abstract class PlayerReferenceManager : EndlessNetworkBehaviour
{
    private NetworkVariable<SerializableGuid> characterVisualId;
    [SerializeField] private CharacterCosmeticsDefinition fallbackCharacterCosmeticsDefinition;

    public CharacterCosmeticsDefinition CharacterCosmetics
    {
        get
        {
            CharacterCosmeticsDefinition definition;
            return DefaultContentManager.Instance.DefaultCharacterCosmetics
                .TryGetDefinition(this.CharacterVisualId, out definition)
                    ? definition
                    : this.fallbackCharacterCosmeticsDefinition;
        }
    }

    [ServerRpc]
    public void UpdateCharacterVisualId_ServerRpc(SerializableGuid newValue)
    {
        this.characterVisualId.Value = newValue;
    }
}
```

### Character Selection Flow

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                        CHARACTER SELECTION FLOW                              │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  1. Player opens character selection UI                                      │
│     └─> UICharacterCosmeticsDefinitionSelector loads CharacterCosmeticsList │
│                                                                              │
│  2. UI displays all characters                                               │
│     └─> UICharacterCosmeticsDefinitionPortraitView.Display()                │
│         └─> portraitImage.sprite = definition.PortraitSprite                │
│         └─> displayNameText.text = definition.DisplayName                   │
│                                                                              │
│  3. Player selects a character                                               │
│     └─> CharacterCosmeticsDefinitionUtility.SetClientCharacterVisualId()    │
│         └─> PlayerPrefs.SetString("Character Visual", GUID)                 │
│                                                                              │
│  4. Game sends selection to server                                           │
│     └─> PlayerReferenceManager.UpdateCharacterVisualId_ServerRpc(GUID)      │
│                                                                              │
│  5. Character 3D model loads                                                 │
│     └─> AppearanceController.UpdateCharacterCosmetics()                     │
│         └─> CharacterCosmeticsDefinition.Instantiate()                      │
│             └─> assetReference.InstantiateAsync() [Addressables]            │
│                                                                              │
│  6. Model appears in game                                                    │
│     └─> AppearanceController.ApplyCosmeticsGameObject()                     │
│         └─> Shader.Find() to replace shaders                                │
│         └─> AppearanceAnimator.InitializeCosmetics()                        │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

### Data Storage Locations

| Data | Storage Location | Modifiable? |
|------|------------------|-------------|
| 3D Character Model | `felix_assets_all_*.bundle` in StreamingAssets/aa | ✅ Yes (our current approach) |
| Character Display Name | `CharacterCosmeticsDefinition` ScriptableObject | ⚠️ Requires editing game assets |
| Character Portrait Icon | `CharacterCosmeticsDefinition` ScriptableObject | ⚠️ Requires editing game assets |
| Character GUID | `CharacterCosmeticsDefinition` ScriptableObject | ⚠️ Requires editing game assets |
| Addressable Reference | `CharacterCosmeticsDefinition` ScriptableObject | ⚠️ Requires editing game assets |
| Character List | `CharacterCosmeticsList` ScriptableObject | ⚠️ Requires editing game assets |
| Selected Character | `PlayerPrefs "Character Visual"` | ✅ Yes (client-side) |

### Why Silhouette Appears in Lobby

Based on code analysis, the silhouette likely appears because:

1. **Different Rendering Path:** The lobby may use a simplified preview system that doesn't call `AppearanceController.ApplyCosmeticsGameObject()` which does the shader swap.

2. **Shader.Find() Failure:** If the lobby's preview system calls `Shader.Find("Shader Graphs/Endless_Shader_Character_NoFade")` and our bundle's shader has a slightly different name or isn't registered, it returns null and uses a fallback (silhouette).

3. **Missing Shader Variants:** The lobby preview might use shader variants that aren't included in our custom bundle.

### Why Character Name/Icon Are Wrong

The bundle replacement ONLY changes the 3D model. The `CharacterCosmeticsDefinition` ScriptableObject that contains `displayName` ("Felix") and `portraitSprite` (Felix's icon) is stored in the game's main assets, NOT in the character bundle.

### Solutions

#### Solution 1: Full ScriptableObject Modification (Complex)

1. Find `CharacterCosmeticsDefinition` for Felix in game assets
2. Use UABEA or AssetRipper to modify:
   - `displayName` → "Pearl Diver"
   - `portraitSprite` → Custom Pearl Diver icon
3. Repack and replace the game asset file

**Challenges:**
- Need to find which file contains the ScriptableObject
- May require modifying `globalgamemanagers.assets` or similar
- CRC/hash validation may need bypassing

#### Solution 2: Runtime Patching with BepInEx/MelonLoader (Recommended)

1. Install BepInEx or MelonLoader mod framework
2. Create a plugin that hooks into:
   - `UICharacterCosmeticsDefinitionPortraitView.Display()` - Override sprite/name
   - `AppearanceController.ApplyCosmeticsGameObject()` - Fix shader issues
3. Replace data at runtime without modifying game files

**Advantages:**
- No permanent file modification
- Easier to update when game patches
- Can dynamically swap multiple characters
- Can fix shader issues at runtime

#### Solution 3: Custom CharacterCosmeticsDefinition Asset

1. Create a new `CharacterCosmeticsDefinition` for Pearl Diver
2. Add it to the `CharacterCosmeticsList`
3. This would require modifying the serialized asset files

### Key Code References

**Files to examine further:**
- `DefaultContentManager` - Contains `DefaultCharacterCosmetics` list
- `RuntimeDatabase` - May contain character definition lookups
- `MonoBehaviourSingleton<DefaultContentManager>.Instance.DefaultCharacterCosmetics`

**PlayerPrefs key:**
```
"Character Visual" = GUID string of selected character
```

**Felix's data (to find in game assets):**
- Asset path: `Assets/Prefabs/CharacterCosmetics/CharacterCosmetics_ThePack_Felix_01_A.prefab`
- Bundle: `felix_assets_all_8ce5cfff23e50dfcaa729aa03940bfd7.bundle`

---

## 39. Summary: What We Can and Cannot Modify

### ✅ CAN Modify (Current Approach)

| Component | Method | Result |
|-----------|--------|--------|
| 3D Character Model | Replace bundle file | ✅ Works in gameplay |
| Character Textures | Include in bundle | ✅ Works |
| Character Materials | Include in bundle | ✅ Works |
| Catalog CRC | Patch catalog.json | ✅ Works |
| Character Name in UI | Binary replace in sharedassets0.assets | ✅ Works (same-length names) |
| Character Portrait Icon | Binary replace in sharedassets0.assets.resS | ✅ Works |

### ⚠️ CANNOT Modify (Without Additional Tools)

| Component | Why | Workaround |
|-----------|-----|------------|
| Different-length Names | Binary replace shifts data | Use UABEA for proper repacking |
| Lobby Silhouette | Different render path in game code | BepInEx hook to fix shader |
| Character Selection List | Stored in CharacterCosmeticsList | BepInEx hook or add new entry |

### Current Limitations Summary

1. **In-Game Rendering:** ✅ WORKS - Custom character appears correctly during gameplay
2. **Lobby/Party Preview:** ⚠️ Shows silhouette - Different render pipeline
3. **Character Name:** ✅ WORKS - Shows "Zayed" (binary replaced)
4. **Character Icon:** ✅ WORKS - Shows custom portrait (binary replaced in .resS)
5. **Character Selection:** ✅ WORKS - Listed with custom name and portrait

---

## Contact/Notes

This documentation created for future agents working on Endstar character modding.

Key insight: The Meshy AI FBX hierarchy issue is a major blocker. Always use fresh Blender file with OBJ import to avoid hierarchy lock problems.

---

## 40. ScriptableObject Modification - SUCCESS (January 4, 2026)

### Overview

Successfully modified the `CharacterCosmeticsDefinition` ScriptableObject to change the character display name from "Felix" to "Zayed" (an Emirati name).

### Location of ScriptableObjects

**File:** `C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\sharedassets0.assets`

**CharacterCosmeticsList offset:** ~293,010,576 bytes
- Contains 46 (0x2E) PPtr references to CharacterCosmeticsDefinition objects

**Felix's CharacterCosmeticsDefinition offset:** ~293,017,760 bytes

### Entry Structure (Binary Analysis)

Each `CharacterCosmeticsDefinition` entry follows this pattern:

```
[4 bytes]  Prefab name length
[N bytes]  Prefab name string (e.g., "CharacterCosmetics_ThePack_Felix_01_A")
[padding]  Null bytes for alignment
[4 bytes]  Display name length  ← THIS IS WHAT WE MODIFY
[N bytes]  Display name string (e.g., "Felix" → "Zayed")
[padding]  Null bytes for alignment
[4 bytes]  Asset ID length (always 36 = UUID)
[36 bytes] Asset ID GUID (e.g., "2d286d66-9860-4883-ad5c-5243f24031c7")
[padding]  Null bytes
[32 bytes] Asset Reference GUID
[...]      Additional data (sprite reference, etc.)
```

### Felix Entry Details

| Field | Value |
|-------|-------|
| Prefab Name | `CharacterCosmetics_ThePack_Felix_01_A` |
| Display Name | `Zayed` (was `Felix`) |
| Asset ID | `2d286d66-9860-4883-ad5c-5243f24031c7` |
| Asset Reference | `887fc0bc3d8628043ae39f42a772f01f` |

### Modification Process

```python
# Python script to modify display name
path = r"sharedassets0.assets"

with open(path, 'rb') as f:
    content = bytearray(f.read())

# Find length-prefixed "Felix" string
search = b'\x05\x00\x00\x00Felix'  # 5 = length of "Felix"
pos = content.find(search)

# Replace with same-length name (CRITICAL: must be same length!)
old_name = b'Felix'
new_name = b'Zayed'  # 5 characters

for i in range(len(new_name)):
    content[pos + 4 + i] = new_name[i]

with open(path, 'wb') as f:
    f.write(content)
```

### Critical Constraints

1. **Same Length Names Only:** Without proper asset repacking, replacement names MUST be exactly the same length as the original
   - "Felix" (5 chars) → "Zayed" (5 chars) ✅
   - "Felix" (5 chars) → "Pearl Diver" (11 chars) ❌ Would corrupt file

2. **Backup First:** Always backup `sharedassets0.assets` before modification

3. **No Launcher:** Must run `Endstar.exe` directly (launcher may restore files)

### Result

✅ **SUCCESS** - Character selection UI now shows "Zayed" instead of "Felix"

### Files Modified

| File | Size | Modification |
|------|------|--------------|
| `sharedassets0.assets` | ~297 MB | Changed display name at offset 293,017,764 |

### Backup Location

```
C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\sharedassets0.assets.backup_original
```

### Remaining Issues

1. **Portrait Sprite:** Still shows Felix's icon (requires sprite replacement - more complex)
2. **Lobby Silhouette:** Still appears (different render path issue)

### Next Steps for Full Custom Character

To fully replace Felix with a custom character, need to:

1. ✅ Replace 3D model bundle (already done - Pearl Diver mesh)
2. ✅ Change display name in ScriptableObject (done - "Zayed")
3. ⬜ Replace portrait sprite (requires finding sprite asset in sharedassets0 and replacing)
4. ⬜ Fix lobby silhouette (may require BepInEx runtime patching)

### For Different Length Names (UABEA Required)

If the custom name has different length than "Felix":

1. Open `sharedassets0.assets` in UABEA
2. Find the MonoBehaviour for Felix's CharacterCosmeticsDefinition
3. Edit the `displayName` field properly
4. Save - UABEA handles length adjustments automatically
5. May need to recalculate file checksums

---

## 41. Portrait Sprite Replacement - SUCCESS (January 4, 2026)

### Overview

Successfully replaced Felix's portrait sprite with a custom Zayed portrait using direct binary replacement in the streaming resource file.

### Key Discovery

Portrait textures are stored in `.resS` (resource stream) files, NOT in the main `.assets` file. The `.assets` file only contains metadata and a pointer to the texture data location.

### File Locations

| File | Purpose |
|------|---------|
| `sharedassets0.assets` | Texture metadata (name, size, format, offset pointer) |
| `sharedassets0.assets.resS` | Actual texture pixel data (175 MB) |

### Felix Portrait Texture Details

| Property | Value |
|----------|-------|
| Texture Name | `CoreCharacterVisualFelix1A` |
| PathID | 777 |
| Sprite PathID | 1868 |
| Size | 214 × 251 pixels |
| Format | RGBA32 (format 4) |
| Data Location | `sharedassets0.assets.resS` |
| Offset in .resS | 114,478,704 bytes |
| Data Size | 214,856 bytes (214 × 251 × 4) |

### Replacement Process

**CRITICAL:** Do NOT use UnityPy's `env.file.save()` - it corrupts the file. Use direct binary replacement instead.

```python
from PIL import Image

# Configuration
ress_path = r"sharedassets0.assets.resS"
new_portrait = r"zayed_portrait_214x251.png"
offset = 114478704
size = 214856  # 214 * 251 * 4 bytes

# Load and prepare new image
img = Image.open(new_portrait)
img = img.resize((214, 251), Image.LANCZOS)  # Must match exact size!
img = img.convert('RGBA')

# Unity stores textures bottom-up (flip vertically)
img_flipped = img.transpose(Image.FLIP_TOP_BOTTOM)

# Get raw RGBA bytes
rgba_data = img_flipped.tobytes('raw', 'RGBA')

# Direct binary replacement in .resS file
with open(ress_path, 'rb') as f:
    content = bytearray(f.read())

content[offset:offset+size] = rgba_data

with open(ress_path, 'wb') as f:
    f.write(content)
```

### Critical Requirements

1. **Exact Size Match:** New image MUST be exactly 214×251 pixels
2. **RGBA Format:** Image must be RGBA (4 channels)
3. **Vertical Flip:** Unity stores textures bottom-up - flip before saving
4. **Binary Patch Only:** Don't use UnityPy save - it corrupts the file
5. **Backup First:** Always backup `.resS` file before modification

### Why UnityPy Save Failed

UnityPy's `env.file.save()` rewrites the entire `.assets` file, which:
- Changes internal offsets and alignments
- May not properly update references to streaming data
- Can break the relationship between `.assets` and `.resS` files

Direct binary replacement only changes the exact texture bytes, leaving everything else intact.

### Backup Locations

```
sharedassets0.assets.backup_original      (main asset file backup)
sharedassets0.assets.resS.backup          (texture data backup)
```

### Result

✅ **SUCCESS** - Character selection UI now shows custom Zayed portrait

---

## 42. Lobby Silhouette Investigation (January 4, 2026)

### Problem

Custom character appears as a silhouette/shadow in the lobby/party area, while rendering correctly during gameplay.

### Investigation Findings

#### 1. Shader Analysis

Checked both original and modded bundles:
```
Original Bundle Shader: Shader Graphs/Endless_Shader_Character_NoFade
Modded Bundle Shader:   Shader Graphs/Endless_Shader_Character_NoFade
```
**Conclusion:** Shader name is already correct - not the cause.

#### 2. Game Code Analysis

Searched `Gameplay.dll` for rendering-related code:

| Search Term | Occurrences | Notes |
|-------------|-------------|-------|
| `AppearanceController` | 18 | Main gameplay character controller |
| `ApplyCosmeticsGameObject` | 1 | Shader swap happens here |
| `UICharacterVisualsReference` | 6 | UI character display |
| `Cosmetic3DIconRenderer` | 3 | 3D icon rendering for cosmetics |
| `NpcOutline` | 4 | NPC outline effect (not for players) |

#### 3. Shader Swap Code (from decompiled Gameplay.dll)

```csharp
private void ApplyCosmeticsGameObject(GameObject cosmetics)
{
    foreach (Renderer renderer in componentsInChildren)
    {
        foreach (Material material in renderer.materials)
        {
            string name = material.shader.name == "Shader Graphs/Endless_Shader"
                ? "Shader Graphs/Endless_Shader_Character_NoFade"
                : material.shader.name;
            material.shader = Shader.Find(name);
        }
    }
}
```

**Key Insight:** This shader swap only occurs in `AppearanceController` which is used for gameplay - NOT for lobby preview.

#### 4. Root Cause

The lobby uses a **different rendering path**:
- Gameplay: `AppearanceController` → shader swap → correct rendering
- Lobby: `UICharacterVisualsReferencePresenter` or `Cosmetic3DIconRenderer` → NO shader swap → silhouette

The lobby might:
- Use different camera/lighting setup
- Apply post-processing that doesn't work with our shader
- Use a simplified render path without proper material setup
- Have environment/lighting that reveals shader incompatibility

### Solution Approach: BepInEx Runtime Patching

Since we cannot modify the lobby rendering code directly, we use BepInEx to hook into the game at runtime.

---

## 43. BepInEx Installation (January 4, 2026)

### What is BepInEx?

BepInEx (Bepis Injector Extensible) is a plugin framework for Unity games that allows:
- Runtime code injection
- Method hooking/patching with Harmony
- Plugin loading system
- Console/logging for debugging

### Installation

**Game Type:** Unity Mono (not IL2CPP)
**BepInEx Version:** 5.4.23.2 (x64)

#### Files Installed

```
C:\Endless Studios\Endless Launcher\Endstar\
├── BepInEx/                    ← Plugin framework folder
│   ├── core/                   ← BepInEx core libraries
│   ├── plugins/                ← Plugin DLLs go here
│   └── patchers/               ← Preloader patchers
├── winhttp.dll                 ← Unity doorstop (loads BepInEx)
├── doorstop_config.ini         ← Doorstop configuration
└── [original game files]
```

#### Download Source

```
https://github.com/BepInEx/BepInEx/releases/download/v5.4.23.2/BepInEx_win_x64_5.4.23.2.zip
```

### First Run Initialization

When game runs for first time with BepInEx:
1. BepInEx creates additional folders (`config/`, `cache/`)
2. Generates `BepInEx.cfg` configuration file
3. Creates `LogOutput.log` for debugging

### Plugin Development

Plugins are C# DLLs that:
1. Reference `BepInEx.dll` and `0Harmony.dll`
2. Use `[BepInPlugin]` attribute
3. Implement `BaseUnityPlugin` class
4. Use Harmony to patch game methods

### Planned Silhouette Fix Plugin

```csharp
// Concept for fixing lobby silhouette
[HarmonyPatch(typeof(UICharacterVisualsReferencePresenter))]
class LobbyCharacterPatch
{
    [HarmonyPostfix]
    [HarmonyPatch("LoadCharacter")]  // or similar method
    static void FixShader(GameObject character)
    {
        // Apply same shader fix as AppearanceController
        foreach (var renderer in character.GetComponentsInChildren<Renderer>())
        {
            foreach (var material in renderer.materials)
            {
                if (material.shader.name.Contains("Endless"))
                {
                    material.shader = Shader.Find("Shader Graphs/Endless_Shader_Character_NoFade");
                }
            }
        }
    }
}
```

### Status

| Step | Status |
|------|--------|
| Download BepInEx | ✅ Complete |
| Extract to game | ✅ Complete |
| First run initialization | ⬜ Pending |
| Create fix plugin | ⬜ Pending |
| Test fix | ⬜ Pending |

# Endstar Custom Character - Blender Setup Guide

This guide covers how to prepare a custom character mesh in Blender that Endstar will accept.

---

## Requirements Overview

| Requirement | Details |
|-------------|---------|
| **Hierarchy** | Must match original character structure |
| **Scale** | Match original character proportions |
| **Armature** | Optional - can use static mesh |
| **Naming** | Must include LOD suffix (`_LOD0`) |
| **Parent** | Must be parented to root empty |
| **Materials** | Use Endless_Shader_Character_NoFade compatible |

---

## Step 1: Extract Original Character

### Using AssetStudio

1. Open AssetStudio
2. File → Load File → Select target bundle (e.g., `felix_assets_all_*.bundle`)
3. Export → All assets
4. Find the FBX file for reference

### Key Files to Extract

- `CharacterCosmetics_ThePack_Felix_01_A.fbx` - The mesh/rig
- Textures (albedo, normal, metallic, etc.)

---

## Step 2: Import Original to Blender

```
File → Import → FBX
```

### Original Felix Hierarchy

```
CharacterCosmetics_ThePack_Felix_01_A  [EMPTY - ROOT]
├── CharacterCosmetics_ThePack_Felix_01_A  [ARMATURE]
│   └── root
│       ├── pelvis
│       │   ├── spine_01
│       │   │   ├── spine_02
│       │   │   │   └── spine_03
│       │   │   │       ├── clavicle_l
│       │   │   │       ├── clavicle_r
│       │   │   │       └── neck_01
│       │   ├── thigh_l
│       │   ├── thigh_r
│       │   └── ...
└── TigerGuy_TW_LOD0  [MESH - child of EMPTY, NOT armature]
```

### Critical Structure Points

1. **Root Empty**: Top-level object is an EMPTY named exactly `CharacterCosmetics_ThePack_Felix_01_A`
2. **Armature**: Child of root empty (for animated characters)
3. **Meshes**: Children of ROOT EMPTY, NOT children of armature
4. **LOD Suffix**: Mesh names must end with `_LOD0`

---

## Step 3: Prepare Custom Mesh

### Import Your Custom Model

```
File → Import → OBJ/FBX/GLTF
```

### Rename Mesh

Your custom mesh MUST have `_LOD0` suffix:

```
# Example:
Meshy_AI_Pearl_Diver  →  PearlDiver_LOD0
```

### Correct Scale

Match the original character's proportions:

```python
# In Blender Python console:
import bpy

# Scale to approximately match Felix
custom_mesh = bpy.data.objects['PearlDiver_LOD0']
custom_mesh.scale = (0.7, 0.7, 0.7)  # Adjust as needed

# Apply scale
bpy.ops.object.select_all(action='DESELECT')
custom_mesh.select_set(True)
bpy.context.view_layer.objects.active = custom_mesh
bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
```

### Scale Guidelines

| Original Character | Approximate Scale Factor |
|-------------------|-------------------------|
| Felix (Tiger) | 1.0 (reference) |
| Tilly | ~0.9 |
| Larger characters | 1.1-1.3 |

**Tip:** Import original Felix FBX alongside your model to visually compare sizes.

---

## Step 4: Set Up Hierarchy

### Remove Original Meshes (Keep Structure)

1. Select original character meshes (e.g., `TigerGuy_TW_LOD0`)
2. Delete them (X → Delete)
3. Keep the root empty and armature

### Parent Custom Mesh to Root Empty

```python
import bpy

# Find objects
root_empty = bpy.data.objects['CharacterCosmetics_ThePack_Felix_01_A']
custom_mesh = bpy.data.objects['PearlDiver_LOD0']

# Parent custom mesh to root empty
custom_mesh.parent = root_empty
custom_mesh.matrix_parent_inverse = root_empty.matrix_world.inverted()
```

### Manual Method

1. Select your custom mesh
2. Shift+Click the root empty (parent selection)
3. Ctrl+P → Object (Keep Transform)

### Final Hierarchy Should Be

```
CharacterCosmetics_ThePack_Felix_01_A  [EMPTY - ROOT]
├── CharacterCosmetics_ThePack_Felix_01_A  [ARMATURE]
│   └── (bone hierarchy...)
└── PearlDiver_LOD0  [YOUR CUSTOM MESH]
```

---

## Step 5: Materials

### Option A: Use Original Materials

If keeping original textures, the materials should transfer with the FBX import.

### Option B: Custom Materials

Create materials that will work with Unity's Endless_Shader_Character_NoFade:

**Required Maps:**
| Map Type | Unity Property | Notes |
|----------|---------------|-------|
| Albedo | _BaseMap | RGB color |
| Normal | _BumpMap | Tangent space normal |
| Metallic | _MetallicGlossMap | R=Metallic, A=Smoothness |
| Emission | _EmissionMap | Optional |

**In Blender:**
1. Create Principled BSDF material
2. Connect textures to appropriate inputs
3. These will export with the FBX

---

## Step 6: Export FBX

### Export Settings

```
File → Export → FBX (.fbx)
```

**Critical Settings:**

| Setting | Value |
|---------|-------|
| Path Mode | Copy (embed textures) |
| Apply Scalings | FBX All |
| Forward | -Z Forward |
| Up | Y Up |
| Apply Transform | ✓ Checked |

**Include:**
- ✓ Empty
- ✓ Armature
- ✓ Mesh

**Armature:**
- ✓ Add Leaf Bones: **UNCHECKED** (important!)
- Deform Bones Only: depends on your rig

### Export Path

Save to your Unity project:
```
D:\Unity_Workshop\Endstar Custom Shader\Assets\Pearl Diver\CharacterCosmetics_ThePack_Felix_01_A.fbx
```

---

## Step 7: Unity Import Settings

In Unity, select the imported FBX and configure:

### Model Tab
| Setting | Value |
|---------|-------|
| Scale Factor | 1 |
| Convert Units | ✓ |
| Import BlendShapes | ✓ if used |

### Rig Tab
| Setting | Value |
|---------|-------|
| Animation Type | Humanoid (if rigged) or None |
| Avatar Definition | Create From This Model |

### Materials Tab
| Setting | Value |
|---------|-------|
| Material Creation Mode | Import via MaterialDescription |
| Location | Use External Materials |

---

## Common Issues & Solutions

### Mesh Invisible in Game

**Cause:** Wrong hierarchy - mesh not parented to root empty
**Fix:** Ensure mesh is direct child of the root empty object

### Character Facing Wrong Direction

**Cause:** Wrong export orientation
**Fix:** In Blender, rotate mesh 180° on Z before export, or adjust FBX export settings

### Character Too Large/Small

**Cause:** Scale mismatch
**Fix:** Compare with original Felix mesh, adjust scale in Blender, apply transforms

### Character Floats Above Ground

**Cause:** Origin point not at feet
**Fix:** Set origin to geometry bottom or match original character's origin

### Mesh Appears as Silhouette

**Cause:** Shader missing GBuffer pass
**Fix:** See URP settings in main protocol (m_RenderingMode: 1)

### Animations Don't Work

**Cause:** Armature/bone naming mismatch
**Fix:** Use identical bone names as original character, or use static mesh

---

## Complete Blender Script

Save as `setup_custom_character.py` and run in Blender:

```python
"""
Endstar Custom Character Setup Script
Run in Blender after importing both:
1. Original Felix FBX
2. Your custom mesh (OBJ/FBX)
"""

import bpy

# Configuration - EDIT THESE
CUSTOM_MESH_NAME = "Meshy_AI_Pearl_Diver_0103103333_texture"  # Your mesh name
NEW_MESH_NAME = "PearlDiver_LOD0"  # Final name (must end with _LOD0)
CUSTOM_SCALE = (0.7, 0.7, 0.7)  # Scale to match Felix size
ROOT_EMPTY_NAME = "CharacterCosmetics_ThePack_Felix_01_A"

def setup_character():
    # Find objects
    custom_mesh = bpy.data.objects.get(CUSTOM_MESH_NAME)
    root_empty = bpy.data.objects.get(ROOT_EMPTY_NAME)

    if not custom_mesh:
        print(f"ERROR: Custom mesh '{CUSTOM_MESH_NAME}' not found!")
        print("Available objects:", [o.name for o in bpy.data.objects])
        return

    if not root_empty:
        print(f"ERROR: Root empty '{ROOT_EMPTY_NAME}' not found!")
        print("Did you import the original Felix FBX?")
        return

    # Rename custom mesh
    custom_mesh.name = NEW_MESH_NAME
    print(f"Renamed: {CUSTOM_MESH_NAME} -> {NEW_MESH_NAME}")

    # Apply scale
    custom_mesh.scale = CUSTOM_SCALE
    bpy.ops.object.select_all(action='DESELECT')
    custom_mesh.select_set(True)
    bpy.context.view_layer.objects.active = custom_mesh
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    print(f"Applied scale: {CUSTOM_SCALE}")

    # Delete original meshes (keep structure)
    for obj in list(bpy.data.objects):
        if 'TigerGuy' in obj.name and obj.type == 'MESH':
            print(f"Removing original mesh: {obj.name}")
            bpy.data.objects.remove(obj, do_unlink=True)

    # Parent to root empty
    custom_mesh.parent = root_empty
    custom_mesh.matrix_parent_inverse = root_empty.matrix_world.inverted()
    print(f"Parented to: {ROOT_EMPTY_NAME}")

    # Print final hierarchy
    print("\n=== FINAL HIERARCHY ===")
    for obj in bpy.data.objects:
        parent_name = obj.parent.name if obj.parent else "ROOT"
        print(f"  {obj.name} [{obj.type}] -> parent: {parent_name}")

    print("\n=== SETUP COMPLETE ===")
    print("Now export as FBX to your Unity project!")

# Run
setup_character()
```

---

## Naming Convention Summary

| Object Type | Naming Pattern | Example |
|-------------|---------------|---------|
| Root Empty | `CharacterCosmetics_[Faction]_[Name]_[Version]` | `CharacterCosmetics_ThePack_Felix_01_A` |
| Armature | Same as root empty | `CharacterCosmetics_ThePack_Felix_01_A` |
| Mesh | `[YourName]_LOD0` | `PearlDiver_LOD0` |
| Bundle | `[name]_assets_all_[hash].bundle` | `felix_assets_all_8ce5cfff23e50dfcaa729aa03940bfd7.bundle` |

---

## Checklist Before Export

- [ ] Custom mesh renamed with `_LOD0` suffix
- [ ] Mesh parented to root empty (NOT armature)
- [ ] Scale matches original character
- [ ] Origin point at appropriate location
- [ ] Materials assigned
- [ ] Transforms applied (Ctrl+A → All Transforms)
- [ ] Original meshes removed
- [ ] Export settings correct (no leaf bones, proper orientation)

---

## Next Steps After Blender

1. Import FBX into Unity project
2. Create prefab from imported model
3. Configure Addressables
4. Build bundle
5. Use mod_installer.py to deploy

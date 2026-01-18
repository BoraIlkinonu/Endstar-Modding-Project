# Adding Custom Props to Endstar

Complete guide for adding custom collectible props (Treasure, Keys, Resources) to Endstar.

---

## Overview

There are two approaches:

| Approach | Props Count | Complexity | CRC Patching |
|----------|-------------|------------|--------------|
| **Replacement** | Up to 3 (per type) | Easy | Not required |
| **BepInEx Plugin** | Unlimited | Advanced | Not required |

This document covers **Prop Replacement** in detail.

---

## Part 1: Prop Replacement Workflow

Replace existing props (Anachronists, Ancient Aliens, Terran variants) with custom models.

### Available Treasure Variants

| Variant | Asset Prefix | Can Replace With |
|---------|-------------|------------------|
| Anachronists | `Anachronists_Treasure` | Your Prop 1 |
| Ancient Aliens | `AncientAliens_Treasure` | Your Prop 2 |
| Terran | `Terran_Treasure` | Your Prop 3 |

---

## Part 2: Extract Original Prop

### Step 1: Open AssetStudio

```
D:\Endstar Plot Flag\PLOT FLAG SYSTEM\Tools\AssetStudio-master\AssetStudioGUI\bin\Release\net6.0-windows\AssetStudioGUI.exe
```

### Step 2: Load Game Assets

1. File → **Load Folder**
2. Select: `C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\`
3. Wait for loading to complete (2-5 minutes)

### Step 3: Export Original Mesh

1. Filter → Type → select **Mesh**
2. Search: `Anachronists_Treasure`
3. Select the mesh
4. Export → Selected assets → save as **OBJ**

**Output:** `Extracted_Props\Mesh\Anachronists_Treasure.obj`

### Step 4: Export Original Textures (Optional)

Can also use UnityPy for texture extraction:

```python
import UnityPy
import os

assets_path = r'C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\sharedassets0.assets'
output_dir = r'D:\Endstar Plot Flag\PLOT FLAG SYSTEM\Extracted_Props'

os.makedirs(output_dir, exist_ok=True)

env = UnityPy.load(assets_path)

for obj in env.objects:
    if obj.type.name == 'Texture2D':
        data = obj.read()
        if 'Treasure' in data.m_Name:
            img = data.image
            img.save(os.path.join(output_dir, f'{data.m_Name}.png'))
            print(f'Exported: {data.m_Name}')
```

**Note:** UnityPy is safe for extraction, but **NEVER use it for writing/importing**.

---

## Part 3: Prepare Custom Mesh in Blender

### Step 1: Import Original for Reference

```
File → Import → Wavefront (.obj)
Select: Extracted_Props\Mesh\Anachronists_Treasure.obj
```

Note the original dimensions:
- Anachronists Treasure (Book): ~0.37 x 0.19 x 0.49

### Step 2: Import Your Custom Model

```
File → Import → FBX/OBJ/GLTF
Select your custom model
```

### Step 3: Compare Sizes

In Blender Python console:
```python
import bpy

# Get dimensions of both meshes
for obj in bpy.data.objects:
    if obj.type == 'MESH':
        print(f'{obj.name}: {obj.dimensions.x:.3f} x {obj.dimensions.y:.3f} x {obj.dimensions.z:.3f}')
```

### Step 4: Scale Custom Mesh

Calculate scale factor:
```python
# Example: Original max dimension = 0.49, Custom max = 1.92
scale_factor = 0.49 / 1.92  # = 0.255
```

Apply scale in Blender:
```python
import bpy

custom_mesh = bpy.data.objects['YourMeshName']

# Scale to match original
custom_mesh.scale = (0.255, 0.255, 0.255)

# Apply scale transform
bpy.ops.object.select_all(action='DESELECT')
custom_mesh.select_set(True)
bpy.context.view_layer.objects.active = custom_mesh
bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
```

### Step 5: Center Origin

```python
bpy.ops.object.origin_set(type='ORIGIN_GEOMETRY', center='BOUNDS')
custom_mesh.location = (0, 0, 0)
```

### Step 6: Rename Mesh

The mesh name must match the original exactly:
```python
custom_mesh.name = 'Anachronists_Treasure'
```

### Step 7: Export for UABEA

```
File → Export → Wavefront (.obj)
Save as: Extracted_Props\Mesh\Anachronists_Treasure_REPLACEMENT.obj
```

**Export Settings:**
- Forward: -Z
- Up: Y
- Apply Modifiers: ✓
- Include UVs: ✓
- Include Normals: ✓

---

## Part 4: Prepare Textures

### Required Textures

| Game Texture | Your Texture | Purpose |
|--------------|-------------|---------|
| `Anachronists_Treasure_D.png` | albedo.png | Diffuse color |
| `Anachronists_Treasure_N.png` | normal.png | Normal map |
| `Anachronists_Treasure_Mix.png` | metallic.png | Metallic/Roughness |
| `Anachronists_Treasure_E.png` | emission.png | Glow (can be black) |

### Resize to 2048x2048

```python
from PIL import Image
import os

src_dir = r'path\to\your\textures'
out_dir = r'D:\Endstar Plot Flag\PLOT FLAG SYSTEM\Extracted_Props\Textures_Prepared'

os.makedirs(out_dir, exist_ok=True)

mapping = {
    'albedo.png': 'Anachronists_Treasure_D.png',
    'normal.png': 'Anachronists_Treasure_N.png',
    'metallic.png': 'Anachronists_Treasure_Mix.png',
    'emission.png': 'Anachronists_Treasure_E.png',  # or roughness
}

for src_name, dst_name in mapping.items():
    src_path = os.path.join(src_dir, src_name)
    dst_path = os.path.join(out_dir, dst_name)

    if os.path.exists(src_path):
        img = Image.open(src_path)
        img_resized = img.resize((2048, 2048), Image.LANCZOS)
        if img_resized.mode == 'RGBA':
            img_resized = img_resized.convert('RGB')
        img_resized.save(dst_path)
        print(f'Saved: {dst_name}')
```

### If No Emission Texture

Create a black 2048x2048 image:
```python
from PIL import Image
img = Image.new('RGB', (2048, 2048), (0, 0, 0))
img.save('Anachronists_Treasure_E.png')
```

---

## Part 5: Import with UABEA

### Step 1: Backup Original Files

```batch
copy "C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\sharedassets0.assets" "sharedassets0.assets.backup"
copy "C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\sharedassets0.assets.resS" "sharedassets0.assets.resS.backup"
```

### Step 2: Open UABEA

```
D:\Endstar Plot Flag\PLOT FLAG SYSTEM\Tools\UABEA\UABEAvalonia.exe
```

### Step 3: Load sharedassets0.assets

```
File → Open → C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\sharedassets0.assets
```

### Step 4: Replace Textures

For each texture:
1. Search for `Anachronists_Treasure_D` (or _N, _Mix, _E)
2. Select the Texture2D asset
3. Click **Plugins** → **Edit Texture**
4. Load your prepared texture
5. Click **Save**

### Step 5: Replace Mesh (if supported)

1. Search for `Anachronists_Treasure` (type: Mesh)
2. Select the mesh asset
3. **Plugins** → **Import Mesh** (if available)
4. Or use **Import Raw** for advanced replacement

**Note:** Mesh import in UABEA may require additional plugins or raw data manipulation.

### Step 6: Save Changes

```
File → Save
```

---

## Part 6: Test in Game

1. Launch game **directly** (not through launcher):
   ```
   C:\Endless Studios\Endless Launcher\Endstar\Endstar.exe
   ```

2. Find a level with Treasure props

3. Verify your custom prop appears correctly

---

## Troubleshooting

### Prop appears invisible
- Check mesh scale matches original
- Verify texture dimensions are 2048x2048

### Prop has wrong orientation
- Adjust rotation in Blender before export
- Check export settings (Forward/Up axis)

### Textures look wrong
- Ensure UV mapping is correct
- Check texture format (RGB, not RGBA for most)

### Game crashes on load
- Restore from backup
- Check UABEA saved without errors

---

## File Locations Summary

| File | Path |
|------|------|
| Original Assets | `C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\sharedassets0.assets` |
| Extracted Mesh | `Extracted_Props\Mesh\Anachronists_Treasure.obj` |
| Prepared Textures | `Extracted_Props\Textures_Prepared\` |
| Replacement Mesh | `Extracted_Props\Mesh\Anachronists_Treasure_REPLACEMENT.obj` |
| UABEA Tool | `Tools\UABEA\UABEAvalonia.exe` |
| AssetStudio | `Tools\AssetStudio-master\...\AssetStudioGUI.exe` |

---

## Tool Warnings

| Tool | Extract | Modify |
|------|---------|--------|
| **UnityPy** | OK | **CORRUPTS FILES** |
| **UABEA** | OK | **USE THIS** |
| **AssetStudio** | OK | Read-only |

**NEVER use UnityPy to write/import assets.**

---

## Part 7: Multiple Props (BepInEx Plugin)

For more than 3 props, you need a BepInEx plugin. See:
- `Zayed_Character_Mod\CustomProps\CustomPropsPlugin.cs` - Plugin template
- Requires hooking into `PropLibrary` and `PropLoader`
- Build custom asset bundle with Unity

### Plugin Architecture

```
CustomPropsPlugin/
├── CustomPropsPlugin.cs    # Main plugin
├── CustomPropsPlugin.csproj
└── custom_props.bundle     # Asset bundle with props

Plugin loads bundle → Registers props → Patches spawning system
```

---

## Example: Pearl Basket Replacing Anachronists Treasure

### Files Used

```
Extracted_Props\
├── Pearl Basket - Low poly textured\
│   ├── Pearl Basket.fbx
│   ├── albedo.png
│   ├── normal.png
│   ├── metallic.png
│   └── roughness.png
├── Mesh\
│   ├── Anachronists_Treasure.obj (original - book)
│   └── Anachronists_Treasure_REPLACEMENT.obj (scaled basket)
└── Textures_Prepared\
    ├── Anachronists_Treasure_D.png
    ├── Anachronists_Treasure_N.png
    ├── Anachronists_Treasure_Mix.png
    └── Anachronists_Treasure_E.png
```

### Scale Applied

- Original Book: 0.37 x 0.19 x 0.49
- Pearl Basket: 1.25 x 1.19 x 1.92
- Scale Factor: 0.253 (25%)
- Final Basket: 0.32 x 0.30 x 0.49

---

## Changelog

- **2026-01-06:** Initial documentation created
- **2026-01-06:** Added Blender scaling workflow
- **2026-01-06:** Added BepInEx plugin section for unlimited props

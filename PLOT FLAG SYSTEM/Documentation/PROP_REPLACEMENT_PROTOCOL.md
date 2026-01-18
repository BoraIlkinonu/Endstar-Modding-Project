# Endstar Prop Replacement Protocol

Complete step-by-step guide for replacing existing props with custom models and textures.

---

## Overview

**What this does:** Replace an existing prop (e.g., Anachronists Treasure book) with your custom model (e.g., Pearl Basket).

**Limitation:** Maximum 3 props per type (Anachronists, Ancient Aliens, Terran variants).

**Advantage:** No CRC patching required - sharedassets0.assets is NOT validated.

---

## Prerequisites

| Tool | Purpose | Path |
|------|---------|------|
| AssetStudio | Extract original mesh | `Tools\AssetStudio-master\...\AssetStudioGUI.exe` |
| Blender | Scale/prepare mesh | `C:\Program Files\Blender Foundation\Blender 3.5\` |
| UABEA | Import into game | `Tools\UABEA\UABEAvalonia.exe` |
| Python + Pillow | Resize textures | `pip install Pillow` |

---

## Phase 1: Extract Original Prop

### Step 1.1: Launch AssetStudio

```
D:\Endstar Plot Flag\PLOT FLAG SYSTEM\Tools\AssetStudio-master\AssetStudioGUI\bin\Release\net6.0-windows\AssetStudioGUI.exe
```

### Step 1.2: Load Game Data Folder

1. File → **Load Folder**
2. Navigate to: `C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\`
3. Click "Select Folder"
4. **Wait 2-5 minutes** for loading to complete

### Step 1.3: Export Original Mesh

1. Filter → Type → check only **Mesh**
2. In search box, type: `Anachronists_Treasure`
3. Select the mesh in the list
4. Export → Selected assets
5. Save as: `D:\Endstar Plot Flag\PLOT FLAG SYSTEM\Extracted_Props\Mesh\Anachronists_Treasure.obj`

### Step 1.4: Note Original Dimensions

Open in Blender or note from AssetStudio:
- **Anachronists Treasure (Book):** ~0.37 x 0.19 x 0.49 units

---

## Phase 2: Prepare Custom Mesh in Blender

### Step 2.1: Open Blender

```
"C:\Program Files\Blender Foundation\Blender 3.5\blender.exe"
```

### Step 2.2: Import Original for Reference

```
File → Import → Wavefront (.obj)
Select: D:\Endstar Plot Flag\PLOT FLAG SYSTEM\Extracted_Props\Mesh\Anachronists_Treasure.obj
```

### Step 2.3: Import Your Custom Model

```
File → Import → FBX (or OBJ/GLTF)
Select: Your custom model file
```

**Example:** `Pearl Basket - Low poly textured\Pearl Basket.fbx`

### Step 2.4: Compare Dimensions

In Blender, select each mesh and check Properties panel → Dimensions, or use Python console:

```python
import bpy
for obj in bpy.data.objects:
    if obj.type == 'MESH':
        d = obj.dimensions
        print(f'{obj.name}: {d.x:.3f} x {d.y:.3f} x {d.z:.3f}')
```

**Example comparison:**
| Mesh | Dimensions | Max Dimension |
|------|------------|---------------|
| Original Book | 0.37 x 0.19 x 0.49 | 0.49 |
| Pearl Basket | 1.25 x 1.19 x 1.92 | 1.92 |

### Step 2.5: Calculate Scale Factor

```
Scale Factor = Original Max / Custom Max
Scale Factor = 0.49 / 1.92 = 0.255
```

### Step 2.6: Apply Scale to Custom Mesh

Select your custom mesh, then:

```python
import bpy

# Get your custom mesh (adjust name as needed)
custom = bpy.data.objects['Pearl Basket']  # or whatever your mesh is named

# Apply scale
custom.scale = (0.255, 0.255, 0.255)

# Select and apply transform
bpy.ops.object.select_all(action='DESELECT')
custom.select_set(True)
bpy.context.view_layer.objects.active = custom
bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)

print(f'New dimensions: {custom.dimensions.x:.3f} x {custom.dimensions.y:.3f} x {custom.dimensions.z:.3f}')
```

### Step 2.7: Center Origin

```python
bpy.ops.object.origin_set(type='ORIGIN_GEOMETRY', center='BOUNDS')
custom.location = (0, 0, 0)
```

### Step 2.8: Rename Mesh to Match Original

**Critical:** The mesh name must match exactly.

```python
custom.name = 'Anachronists_Treasure'
custom.data.name = 'Anachronists_Treasure'
```

### Step 2.9: Delete Original Reference Mesh

Select the original book mesh and delete it (X → Delete).

### Step 2.10: Export Replacement Mesh

```
File → Export → Wavefront (.obj)
```

**Save to:** `D:\Endstar Plot Flag\PLOT FLAG SYSTEM\Extracted_Props\Mesh\Anachronists_Treasure_REPLACEMENT.obj`

**Export Settings:**
| Setting | Value |
|---------|-------|
| Forward | -Z Forward |
| Up | Y Up |
| Apply Modifiers | ✓ |
| Include UVs | ✓ |
| Include Normals | ✓ |
| Triangulate Faces | ✓ |

---

## Phase 3: Prepare Textures

### Step 3.1: Identify Your Texture Files

Your custom model should have:
| Your File | Purpose |
|-----------|---------|
| albedo.png | Main color/diffuse |
| normal.png | Surface detail bumps |
| metallic.png | Metallic areas |
| roughness.png | Surface roughness |
| emission.png | Glowing areas (optional) |

### Step 3.2: Resize and Rename Textures

Game textures are **2048x2048**. Run this Python script:

```python
from PIL import Image
import os

# Configure paths
src_dir = r'D:\Endstar Plot Flag\PLOT FLAG SYSTEM\Extracted_Props\Pearl Basket - Low poly textured'
out_dir = r'D:\Endstar Plot Flag\PLOT FLAG SYSTEM\Extracted_Props\Textures_Prepared'

os.makedirs(out_dir, exist_ok=True)

# Mapping: your texture name → game texture name
mapping = {
    'albedo.png': 'Anachronists_Treasure_D.png',      # Diffuse
    'normal.png': 'Anachronists_Treasure_N.png',      # Normal
    'metallic.png': 'Anachronists_Treasure_Mix.png',  # Metallic/Mix
}

for src_name, dst_name in mapping.items():
    src_path = os.path.join(src_dir, src_name)
    dst_path = os.path.join(out_dir, dst_name)

    if os.path.exists(src_path):
        img = Image.open(src_path)
        original_size = img.size

        # Resize to 2048x2048
        img_resized = img.resize((2048, 2048), Image.LANCZOS)

        # Convert RGBA to RGB if needed
        if img_resized.mode == 'RGBA':
            img_resized = img_resized.convert('RGB')

        img_resized.save(dst_path)
        print(f'{src_name} ({original_size}) → {dst_name} (2048x2048)')
    else:
        print(f'NOT FOUND: {src_name}')

# Create black emission texture if not provided
emission_path = os.path.join(out_dir, 'Anachronists_Treasure_E.png')
if not os.path.exists(emission_path):
    black = Image.new('RGB', (2048, 2048), (0, 0, 0))
    black.save(emission_path)
    print(f'Created black emission texture')

print(f'\nTextures saved to: {out_dir}')
```

### Step 3.3: Verify Prepared Textures

You should have:
```
Textures_Prepared\
├── Anachronists_Treasure_D.png   (2048x2048, diffuse)
├── Anachronists_Treasure_N.png   (2048x2048, normal)
├── Anachronists_Treasure_Mix.png (2048x2048, metallic)
└── Anachronists_Treasure_E.png   (2048x2048, emission)
```

---

## Phase 4: Backup Original Game Files

**CRITICAL: Always backup before modifying.**

```batch
mkdir "D:\Endstar Plot Flag\PLOT FLAG SYSTEM\ORIGINAL_GAME_BACKUPS"

copy "C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\sharedassets0.assets" "D:\Endstar Plot Flag\PLOT FLAG SYSTEM\ORIGINAL_GAME_BACKUPS\sharedassets0.assets.ORIGINAL"

copy "C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\sharedassets0.assets.resS" "D:\Endstar Plot Flag\PLOT FLAG SYSTEM\ORIGINAL_GAME_BACKUPS\sharedassets0.assets.resS.ORIGINAL"
```

---

## Phase 5: Import with UABEA

### Step 5.1: Launch UABEA

```
D:\Endstar Plot Flag\PLOT FLAG SYSTEM\Tools\UABEA\UABEAvalonia.exe
```

### Step 5.2: Open sharedassets0.assets

```
File → Open
Navigate to: C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\
Select: sharedassets0.assets
```

### Step 5.3: Replace Textures

**For each texture (_D, _N, _Mix, _E):**

1. In the search/filter box, type: `Anachronists_Treasure_D`
2. Find the entry with Type = **Texture2D**
3. Select it
4. Click **Plugins** → **Edit Texture**
5. In the texture editor, click **Load**
6. Select your prepared texture: `Textures_Prepared\Anachronists_Treasure_D.png`
7. Click **Save** to apply

**Repeat for:**
- `Anachronists_Treasure_N` → load `Anachronists_Treasure_N.png`
- `Anachronists_Treasure_Mix` → load `Anachronists_Treasure_Mix.png`
- `Anachronists_Treasure_E` → load `Anachronists_Treasure_E.png`

### Step 5.4: Replace Mesh (Advanced)

**Note:** UABEA mesh import requires the mesh plugin or raw data editing.

1. Search for: `Anachronists_Treasure`
2. Find entry with Type = **Mesh**
3. Click **Plugins** → **Import Mesh** (if available)
4. Select: `Mesh\Anachronists_Treasure_REPLACEMENT.obj`

**If no mesh import plugin:**
- Mesh replacement requires raw asset editing
- Or use the texture-only approach (keeps original shape)

### Step 5.5: Save Modified Assets

```
File → Save
```

UABEA will save changes to `sharedassets0.assets`.

**Note:** The `.resS` file (texture stream data) may also be modified automatically.

---

## Phase 6: Test in Game

### Step 6.1: Launch Game Directly

**Important:** Do NOT use the launcher (it may restore original files).

```
"C:\Endless Studios\Endless Launcher\Endstar\Endstar.exe"
```

### Step 6.2: Find Treasure Prop

- Play a level that contains Anachronists Treasure
- Verify your custom prop appears
- Check textures display correctly

---

## Phase 7: Restore Original (If Needed)

```batch
copy /Y "D:\Endstar Plot Flag\PLOT FLAG SYSTEM\ORIGINAL_GAME_BACKUPS\sharedassets0.assets.ORIGINAL" "C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\sharedassets0.assets"

copy /Y "D:\Endstar Plot Flag\PLOT FLAG SYSTEM\ORIGINAL_GAME_BACKUPS\sharedassets0.assets.resS.ORIGINAL" "C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\sharedassets0.assets.resS"
```

---

## Quick Reference: Replaceable Props

### Treasure Variants

| Variant | Mesh Name | Textures |
|---------|-----------|----------|
| Anachronists | `Anachronists_Treasure` | `Anachronists_Treasure_D/N/Mix/E` |
| Ancient Aliens | `AncientAliens_Treasure` | `AncientAliens_Treasure_D/N/Mix/E` |
| Terran | `Terran_Treasure` | `Terran_Treasure_D/N/Mix/E` |

### Other Prop Types

| Type | Variants | Notes |
|------|----------|-------|
| Key | Multiple | Door/chest keys |
| Resource | Multiple | Crafting materials |

---

## Troubleshooting

| Issue | Cause | Solution |
|-------|-------|----------|
| Prop invisible | Wrong mesh scale | Re-scale in Blender to match original |
| Textures wrong | Wrong dimensions | Ensure 2048x2048 |
| Game crashes | Corrupted asset | Restore backup, retry import |
| Changes not visible | Launcher restored files | Launch Endstar.exe directly |
| UABEA won't save | File locked | Close other programs accessing file |

---

## Tool Warnings

| Tool | Extract | Import/Modify |
|------|---------|---------------|
| AssetStudio | ✓ YES | ✗ Read-only |
| UABEA | ✓ YES | ✓ **USE THIS** |
| UnityPy | ✓ YES | ✗ **CORRUPTS FILES** |

**NEVER use UnityPy to write/import assets.**

---

## Example: Pearl Basket Replacement

### Input Files
```
Pearl Basket - Low poly textured\
├── Pearl Basket.fbx      (1.25 x 1.19 x 1.92)
├── albedo.png            (2048x2048)
├── normal.png            (2048x2048)
├── metallic.png          (2048x2048)
└── roughness.png         (2048x2048)
```

### Scaling Applied
- Scale factor: 0.255 (25%)
- Final size: 0.32 x 0.30 x 0.49

### Output Files
```
Extracted_Props\
├── Mesh\
│   └── Anachronists_Treasure_REPLACEMENT.obj
└── Textures_Prepared\
    ├── Anachronists_Treasure_D.png
    ├── Anachronists_Treasure_N.png
    ├── Anachronists_Treasure_Mix.png
    └── Anachronists_Treasure_E.png
```

---

## Changelog

- **2026-01-06:** Created complete replacement protocol
- **2026-01-06:** Added UABEA import steps
- **2026-01-06:** Added Blender scaling workflow

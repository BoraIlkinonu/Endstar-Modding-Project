# Endstar Prop Reskinning Guide

## Overview

Props like Treasure, Keys, and Resources can be reskinned by modifying `sharedassets0.assets`. Unlike character bundles, **sharedassets does NOT have CRC validation**, making prop modification simpler.

---

## Treasure Prop Structure

Each Treasure variant has these components:

| Component | Asset Name Pattern | Type |
|-----------|-------------------|------|
| Mesh | `{Faction}_Treasure` | Mesh |
| Diffuse Texture | `{Faction}_Treasure_D` | Texture2D |
| Normal Map | `{Faction}_Treasure_N` | Texture2D |
| Metallic Mix | `{Faction}_Treasure_Mix` | Texture2D |
| Emission | `{Faction}_Treasure_E` | Texture2D |
| Material | `{Faction}_Treasure_mat` | Material |
| Icon | `IconCoreProp{Faction}_Treasure` | Sprite |
| Definition | `Treasure Usable Definition - {Faction}` | ScriptableObject |

### Available Treasure Factions

| Faction | Full Name |
|---------|-----------|
| Anachronists | Medieval/fantasy themed |
| Ancient Aliens | Sci-fi alien themed |
| Terran | Human/Earth themed |

---

## Method 1: Texture-Only Reskin (Easiest)

Replace textures while keeping original mesh.

### Requirements
- UABEA (Unity Asset Bundle Extractor Avalonia)
- Image editor (Photoshop, GIMP, etc.)
- Original texture dimensions must be preserved

### Step 1: Extract Original Textures

1. Open UABEA
2. File → Open → Select `sharedassets0.assets`
3. Search for `Anachronists_Treasure_D` (or your target)
4. Select texture → Plugins → Export Texture
5. Export as PNG

### Step 2: Create Custom Texture

1. Open exported texture in image editor
2. Modify while keeping EXACT same dimensions
3. Save as PNG

### Step 3: Import Custom Texture

1. In UABEA, select the texture asset
2. Plugins → Edit Texture
3. Load your custom PNG
4. File → Save

### Texture Map Guide

| Map | Purpose | Format |
|-----|---------|--------|
| `_D` | Diffuse/Albedo | RGB color |
| `_N` | Normal | Tangent space (blue-ish) |
| `_Mix` | R=Metallic, G=AO, B=Detail, A=Smoothness | Channel-packed |
| `_E` | Emission glow | RGB color |

---

## Method 2: Full Mesh + Texture Reskin

Replace both mesh and textures.

### Requirements
- UABEA
- AssetStudio (for extraction)
- Blender (for mesh editing)

### Step 1: Extract Original Mesh

1. Open AssetStudio
2. File → Load File → `sharedassets0.assets`
3. Filter by `Mesh` type
4. Find `Anachronists_Treasure`
5. Export → Selected assets (as FBX)

### Step 2: Modify in Blender

1. Import extracted FBX
2. Replace/modify mesh
3. **Keep same vertex count if possible** (safer)
4. Export as FBX

### Step 3: Import Modified Mesh

1. In UABEA, find the mesh asset
2. Plugins → Import mesh (if available)
3. Or use hex editing for advanced replacement

### Important Notes

- Mesh replacement is more complex than textures
- Vertex count changes may cause issues
- UV mapping must match texture layout
- Test thoroughly after changes

---

## Method 3: Icon Replacement

Change the inventory/UI icon for the prop.

### Icon Specifications

| Property | Value |
|----------|-------|
| Asset Name | `IconCoreProp{Faction}_Treasure` |
| Type | Sprite (from Texture2D) |
| Typical Size | 128x128 or 256x256 |
| Format | RGBA |

### Steps

1. In UABEA, find icon texture
2. Export original to get dimensions
3. Create replacement at same size
4. Import using Plugins → Edit Texture

---

## Backup Protocol

**ALWAYS backup before modifying:**

```bash
# Windows
copy "C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\sharedassets0.assets" "sharedassets0.assets.backup"
copy "C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\sharedassets0.assets.resS" "sharedassets0.assets.resS.backup"
```

### Restore Original

```bash
copy /Y "sharedassets0.assets.backup" "C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\sharedassets0.assets"
copy /Y "sharedassets0.assets.resS.backup" "C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\sharedassets0.assets.resS"
```

---

## Why No CRC Patching Needed

Unlike Addressable bundles (character assets), `sharedassets0.assets` is:
- NOT listed in `catalog.json`
- NOT validated by the Addressables system
- Loaded directly by Unity at startup

This means modifications persist without CRC patches.

---

## File Locations

| File | Contents |
|------|----------|
| `sharedassets0.assets` | Asset metadata, materials, mesh data |
| `sharedassets0.assets.resS` | Texture pixel data (stream) |
| `resources.assets` | ScriptableObjects, definitions |

---

## Recommended Workflow for Treasure Reskin

### Quick Texture Swap

1. Backup `sharedassets0.assets` and `.resS`
2. Open `sharedassets0.assets` in UABEA
3. Find `Anachronists_Treasure_D`
4. Export texture → modify → reimport
5. Repeat for `_N`, `_Mix`, `_E` if desired
6. Save and test in game

### Full Custom Prop

1. Backup all files
2. Extract mesh with AssetStudio
3. Modify mesh in Blender
4. Export all textures from UABEA
5. Create new textures matching UVs
6. Import mesh (if supported) and textures
7. Save and test

---

## Troubleshooting

### Texture appears corrupted
- Ensure dimensions match exactly
- Use correct format (PNG recommended)
- Check if `.resS` file was also updated

### Mesh doesn't load
- Vertex count mismatch may cause issues
- Check bone/weight data if rigged
- Ensure asset type matches original

### Changes don't appear in game
- Verify correct file was modified
- Check if game launcher restored files
- Launch `Endstar.exe` directly (not launcher)

### Game crashes on load
- Restore from backup
- Check UABEA logs for errors
- Verify asset wasn't corrupted during import

---

## Tool Download Links

| Tool | Purpose | Link |
|------|---------|------|
| UABEA | Asset editing | https://github.com/nesrak1/UABEA |
| AssetStudio | Asset extraction | https://github.com/Perfare/AssetStudio |

---

## WARNING: Do NOT Use UnityPy for Modifications

**UnityPy corrupts assets when writing/importing.** While UnityPy can extract textures, it should NEVER be used to modify or reimport assets back into the game.

| Tool | Extraction | Modification |
|------|------------|--------------|
| UnityPy | OK | **NO - CORRUPTS FILES** |
| UABEA | OK | **YES - RECOMMENDED** |
| AssetStudio | OK | No (read-only) |

**Always use UABEA for any asset modifications.**

---

## Comparison: Props vs Characters

| Aspect | Characters | Props |
|--------|-----------|-------|
| Storage | Addressable bundles | sharedassets0.assets |
| CRC Validation | Yes (catalog.json) | No |
| Modification Tool | UABEA + CRC patch | UABEA only |
| Complexity | Higher | Lower |
| Risk | Moderate | Lower |

---

## Next Steps After Reskin

1. Test prop appears correctly in game
2. Verify pickup functionality works
3. Check inventory icon displays
4. Document your changes for others

---

## Example: Creating a Custom UAE Treasure

1. Export `Anachronists_Treasure_D` texture
2. Replace with UAE cultural pattern
3. Export `_N` and create matching normal map
4. Export `_E` and add gold emission glow
5. Import all modified textures
6. Test collection in-game

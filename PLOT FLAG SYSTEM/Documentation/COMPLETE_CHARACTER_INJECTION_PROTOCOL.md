# Complete Custom Character Injection Protocol for Endstar

## Executive Summary

This document describes the **VERIFIED WORKING** protocol for injecting custom character cosmetics into Endstar.

**VERIFIED 2026-01-04:** Successfully injected Pearl Diver mesh replacing Felix. Character renders correctly in lobby (no silhouette).

---

## Critical Configuration (MUST DO FIRST)

### Unity URP Settings - BOTH Files Required

#### File 1: URP-HighFidelity.asset
**Path:** `D:\Unity_Workshop\Endstar Custom Shader\Assets\Settings\URP-HighFidelity.asset`

```yaml
m_PrefilteringModeDeferredRendering: 1   # Was 0 - enables GBuffer shader variants
```

#### File 2: URP-HighFidelity-Renderer.asset
**Path:** `D:\Unity_Workshop\Endstar Custom Shader\Assets\Settings\URP-HighFidelity-Renderer.asset`

```yaml
m_RenderingMode: 1   # Was 0 (Forward) - MUST be 1 (Deferred) for GBuffer pass
```

**WHY:** The game's Menu Camera uses deferred rendering for character display. Without GBuffer pass, characters appear as black silhouettes.

---

## Bundle Information

### Felix Bundle (Target for Replacement)
| Property | Value |
|----------|-------|
| **Original CRC** | `1004267194` |
| **Hash** | `8ce5cfff23e50dfcaa729aa03940bfd7` |
| **Filename** | `felix_assets_all_8ce5cfff23e50dfcaa729aa03940bfd7.bundle` |
| **Original Size** | 3,644,563 bytes |

### ALL 45 Bundles Have CRC Validation
Every character bundle requires catalog CRC patching when replaced.

---

## Complete Deployment Protocol

### Step 1: Configure Unity Project (One-Time Setup)

1. Close Unity completely
2. Edit both URP files (see Critical Configuration above)
3. Delete shader cache: `Library\ShaderCache\`
4. Delete addressables cache: `Library\com.unity.addressables\`
5. Reopen Unity

### Step 2: Build Addressables in Unity

1. Open Unity project
2. Window → Asset Management → Addressables → Groups
3. Build → New Build → Default Build Script
4. Output appears in: `Library\com.unity.addressables\aa\Windows\`

### Step 3: Get New CRC from Built Catalog

Run this Python script to extract CRC:

```python
import json
import base64
import re

catalog_path = r'D:\Unity_Workshop\Endstar Custom Shader\Library\com.unity.addressables\aa\Windows\catalog.json'

with open(catalog_path, 'r') as f:
    catalog = json.load(f)

extra_data = base64.b64decode(catalog['m_ExtraDataString'])

# Try both UTF-16LE alignments
for offset in [0, 1]:
    text = extra_data[offset:].decode('utf-16-le', errors='ignore')
    pattern = r'"m_Hash":"([a-f0-9]{32})"[^}]*"m_Crc":(\d+)[^}]*"m_BundleSize":(\d+)'
    matches = re.findall(pattern, text)
    for hash_val, crc, size in matches:
        print(f"Hash: {hash_val}, CRC: {crc}, Size: {size}")
```

**Record the new CRC value** (e.g., `2057978164`)

### Step 4: Copy and Rename Bundle

```powershell
# Source: Unity build output (may have different name like "defaultlocalgroup_*.bundle")
# Destination: Must match Felix's original filename

Copy-Item 'D:\Unity_Workshop\Endstar Custom Shader\Library\com.unity.addressables\aa\Windows\StandaloneWindows64\defaultlocalgroup_assets_all_*.bundle' `
  -Destination 'C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\StreamingAssets\aa\StandaloneWindows64\felix_assets_all_8ce5cfff23e50dfcaa729aa03940bfd7.bundle' `
  -Force
```

### Step 5: Patch Game Catalog CRC

**CRITICAL:** Use ORIGINAL backup catalog as source, not the current game catalog.

```python
import json
import base64

# Read ORIGINAL catalog backup
with open(r'D:\Endstar Plot Flag\PLOT FLAG SYSTEM\ORIGINAL_GAME_BACKUPS\catalog.json.ORIGINAL', 'r') as f:
    catalog = json.load(f)

# Decode m_ExtraDataString
extra_data = base64.b64decode(catalog['m_ExtraDataString'])

# Replace CRC (UTF-16LE encoded)
old_crc = '1004267194'.encode('utf-16-le')  # Felix's original CRC
new_crc = '2057978164'.encode('utf-16-le')  # Your new bundle's CRC - CHANGE THIS

pos = extra_data.find(old_crc)
if pos == -1:
    print('ERROR: Old CRC not found!')
else:
    patched = extra_data[:pos] + new_crc + extra_data[pos+len(old_crc):]
    catalog['m_ExtraDataString'] = base64.b64encode(patched).decode('ascii')

    # Write to game folder
    with open(r'C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\StreamingAssets\aa\catalog.json', 'w') as f:
        json.dump(catalog, f, separators=(',', ':'))

    print('Catalog patched successfully')
```

### Step 6: Verify Deployment

```python
import json
import base64

# Read patched game catalog
with open(r'C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\StreamingAssets\aa\catalog.json', 'r') as f:
    catalog = json.load(f)

extra_data = base64.b64decode(catalog['m_ExtraDataString'])

# Find Felix hash and show context
felix_hash = '8ce5cfff23e50dfcaa729aa03940bfd7'.encode('utf-16-le')
pos = extra_data.find(felix_hash)
if pos > 0:
    context = extra_data[pos:pos+200].decode('utf-16-le', errors='ignore')
    print(f'Felix entry: {context[:150]}')
    # Should show your NEW CRC, not 1004267194
```

### Step 7: Launch and Test

```powershell
Start-Process 'C:\Endless Studios\Endless Launcher\Endstar\Endstar.exe'
```

Check Felix in the character selection lobby.

---

## File Locations

### Backups (CRITICAL - Never Modify)
```
D:\Endstar Plot Flag\PLOT FLAG SYSTEM\ORIGINAL_GAME_BACKUPS\
└── catalog.json.ORIGINAL    # Original game catalog - use as patching source
```

### Game Installation
```
C:\Endless Studios\Endless Launcher\Endstar\
├── Endstar_Data\
│   └── StreamingAssets\aa\
│       ├── catalog.json                              # Addressables catalog (patch this)
│       └── StandaloneWindows64\
│           └── felix_assets_all_*.bundle             # Character bundles
└── BepInEx\
    └── LogOutput.log                                 # Debug logs
```

### Unity Project
```
D:\Unity_Workshop\Endstar Custom Shader\
├── Assets\
│   ├── Prefabs\CharacterCosmetics\                   # Character prefabs
│   └── Settings\
│       ├── URP-HighFidelity.asset                    # Pipeline settings
│       └── URP-HighFidelity-Renderer.asset           # Renderer settings
└── Library\com.unity.addressables\aa\Windows\        # Build output
    ├── catalog.json
    └── StandaloneWindows64\*.bundle
```

---

## Troubleshooting

### Character Shows as Black Silhouette
| Check | Solution |
|-------|----------|
| PassCount < 6 | Set `m_RenderingMode: 1` in URP-HighFidelity-Renderer.asset |
| Missing GBuffer | Set `m_PrefilteringModeDeferredRendering: 1` in URP-HighFidelity.asset |
| Shader not recompiled | Delete `Library\ShaderCache\` and rebuild |

### Character Not Loading
| Check | Solution |
|-------|----------|
| Wrong filename | Must be exactly `felix_assets_all_8ce5cfff23e50dfcaa729aa03940bfd7.bundle` |
| CRC mismatch | Verify catalog CRC matches your bundle's actual CRC |
| Wrong folder | Bundle must be in `StreamingAssets\aa\StandaloneWindows64\` |

### CRC Verification Failed
| Check | Solution |
|-------|----------|
| Used wrong source | Always patch from ORIGINAL backup, not current catalog |
| CRC not found | Old CRC must be UTF-16LE encoded in m_ExtraDataString |
| extract_all_crc.py shows wrong value | Script has caching bug - verify with direct Python decode |

---

## Quick Reference Commands

### Extract CRC from Unity catalog
```bash
python extract_all_crc.py "D:\Unity_Workshop\...\catalog.json"
```

### Verify catalog patch (direct decode)
```python
# Shows actual CRC in game catalog - more reliable than extract_all_crc.py
import json, base64
cat = json.load(open(r'C:\...\catalog.json'))
data = base64.b64decode(cat['m_ExtraDataString'])
hash_bytes = '8ce5cfff23e50dfcaa729aa03940bfd7'.encode('utf-16-le')
pos = data.find(hash_bytes)
print(data[pos:pos+200].decode('utf-16-le', errors='ignore'))
```

---

## Summary Checklist

- [ ] URP-HighFidelity.asset: `m_PrefilteringModeDeferredRendering: 1`
- [ ] URP-HighFidelity-Renderer.asset: `m_RenderingMode: 1`
- [ ] Deleted ShaderCache before build
- [ ] Built Addressables in Unity
- [ ] Extracted new CRC from Unity catalog
- [ ] Copied bundle with correct filename
- [ ] Patched catalog FROM ORIGINAL BACKUP
- [ ] Verified new CRC appears in patched catalog
- [ ] Tested in game

---

---

## Part 2: Character Identity (Name & Portrait)

After deploying the 3D model bundle, you need to change the character's display name and portrait.

### Requirements

- Character names must be **same length** for binary patching (or use UABEA for different lengths)
- Portrait must be exactly **214×251 pixels**, RGBA format

### Files to Modify

| File | Purpose |
|------|---------|
| `sharedassets0.assets` | Contains display name |
| `sharedassets0.assets.resS` | Contains portrait texture data |

### Felix → Zayed Identity Patch

Both names are 5 characters - perfect for binary replacement.

#### Step 1: Patch Display Name

```python
# Find and replace "Felix" with "Zayed" in sharedassets0.assets
path = r"C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\sharedassets0.assets"

with open(path, 'rb') as f:
    content = bytearray(f.read())

# Search for length-prefixed "Felix" string
search = b'\x05\x00\x00\x00Felix'  # 5 = length
pos = content.find(search)

if pos != -1:
    # Replace name bytes (after 4-byte length prefix)
    new_name = b'Zayed'
    for i, byte in enumerate(new_name):
        content[pos + 4 + i] = byte

    with open(path, 'wb') as f:
        f.write(content)
```

**Felix entry location:** offset ~293,017,760 bytes

#### Step 2: Patch Portrait

```python
from PIL import Image

# Configuration
ress_path = r"C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\sharedassets0.assets.resS"
portrait_path = r"D:\Endstar Plot Flag\PLOT FLAG SYSTEM\Zayed_Character_Mod\Portraits\zayed_portrait_214x251.png"
offset = 114478704
size = 214856  # 214 * 251 * 4 bytes

# Load and prepare image
img = Image.open(portrait_path)
img = img.resize((214, 251), Image.LANCZOS)
img = img.convert('RGBA')

# Unity stores textures bottom-up - FLIP VERTICALLY
img_flipped = img.transpose(Image.FLIP_TOP_BOTTOM)
rgba_data = img_flipped.tobytes('raw', 'RGBA')

# Binary replace in .resS file
with open(ress_path, 'rb') as f:
    content = bytearray(f.read())

content[offset:offset+size] = rgba_data

with open(ress_path, 'wb') as f:
    f.write(content)
```

### Portrait Texture Details

| Property | Value |
|----------|-------|
| Texture Name | `CoreCharacterVisualFelix1A` |
| Size | 214 × 251 pixels |
| Format | RGBA32 |
| Offset in .resS | 114,478,704 bytes |
| Data Size | 214,856 bytes |

### Automated Script

Use `patch_character_identity.py` for automated patching:

```bash
python "D:\Endstar Plot Flag\PLOT FLAG SYSTEM\patch_character_identity.py"
```

This script:
1. Creates backups automatically
2. Patches display name (Felix → Zayed)
3. Patches portrait texture
4. Verifies changes

### Critical Notes

1. **Same-Length Names Only:** Binary patching requires same character count
   - Felix (5) → Zayed (5) ✅
   - Felix (5) → Pearl Diver (11) ❌ Use UABEA instead

2. **Run Directly:** Launch `Endstar.exe` directly, NOT through launcher
   - Launcher may restore original files

3. **Backup First:** Script creates backups in `ORIGINAL_GAME_BACKUPS/`

4. **Portrait Flip:** Unity stores textures bottom-up - always flip vertically

---

## Complete Deployment Checklist

### One-Time Setup
- [ ] URP-HighFidelity.asset: `m_PrefilteringModeDeferredRendering: 1`
- [ ] URP-HighFidelity-Renderer.asset: `m_RenderingMode: 1`

### Per-Deployment
- [ ] Delete `Library\ShaderCache\` before build
- [ ] Build Addressables in Unity
- [ ] Extract new CRC from Unity catalog
- [ ] Copy bundle with Felix filename
- [ ] Patch game catalog CRC (from ORIGINAL backup)
- [ ] Verify CRC patched correctly
- [ ] Patch display name (sharedassets0.assets)
- [ ] Patch portrait (sharedassets0.assets.resS)
- [ ] Launch Endstar.exe directly (not launcher)

---

## Deployment Scripts

| Script | Purpose |
|--------|---------|
| `deploy_felix_bundle.py` | Deploy 3D model bundle + CRC patch |
| `patch_character_identity.py` | Patch name + portrait |
| `extract_all_crc.py` | Extract CRC values from catalog |

### Full Deployment Command

```bash
# Step 1: Deploy bundle
python "D:\Endstar Plot Flag\PLOT FLAG SYSTEM\deploy_felix_bundle.py"

# Step 2: Patch identity
python "D:\Endstar Plot Flag\PLOT FLAG SYSTEM\patch_character_identity.py"

# Step 3: Launch game
start "" "C:\Endless Studios\Endless Launcher\Endstar\Endstar.exe"
```

---

## Version History

| Date | Change |
|------|--------|
| 2026-01-04 | **SUCCESS** - Full character replacement: 3D model + name + portrait |
| 2026-01-04 | Added: Character identity patching (name + portrait) |
| 2026-01-04 | Added: Automated deployment scripts |
| 2026-01-04 | **SUCCESS** - Pearl Diver renders correctly in lobby |
| 2026-01-04 | Fixed: m_RenderingMode must be 1 (Deferred), not just prefiltering |
| 2026-01-04 | Fixed: ALL 45 bundles have CRC (previous docs were wrong) |
| 2026-01-04 | Fixed: extract_all_crc.py UTF-16LE alignment bug |

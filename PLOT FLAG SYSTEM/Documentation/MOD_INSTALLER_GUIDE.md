# Endstar Mod Installer Guide

## Quick Start

### Install a Mod
```bash
python mod_installer.py install ZayedMod
```

### Uninstall (Restore Originals)
```bash
python mod_installer.py uninstall
```

### Check Status
```bash
python mod_installer.py status
```

---

## Mod Package Structure

```
MyMod/
├── mod.json                    # Required: Mod configuration
├── characters/
│   ├── felix/                  # Character folder (matches original name)
│   │   ├── bundle.bundle       # Required: 3D model bundle
│   │   ├── portrait.png        # Optional: Portrait image (214x251)
│   │   └── config.json         # Optional: Character metadata
│   ├── tilly/                  # Another character replacement
│   │   ├── bundle.bundle
│   │   └── portrait.png
│   └── ...
└── README.md                   # Optional: Documentation
```

---

## mod.json Format

```json
{
  "name": "My Custom Characters",
  "version": "1.0.0",
  "author": "Your Name",
  "description": "Description of your mod",
  "characters": [
    {
      "replaces": "Felix",
      "newName": "Zayed",
      "bundle": "characters/felix/bundle.bundle",
      "portrait": "characters/felix/portrait.png",
      "crc": "2057978164"
    },
    {
      "replaces": "Tilly",
      "newName": "Fatima",
      "bundle": "characters/tilly/bundle.bundle",
      "portrait": "characters/tilly/portrait.png",
      "crc": "1234567890"
    }
  ]
}
```

### Character Entry Fields

| Field | Required | Description |
|-------|----------|-------------|
| `replaces` | Yes | Original character name (must match CHARACTER_DATABASE) |
| `newName` | Yes | New display name (MUST be same length for binary patching) |
| `bundle` | Yes | Path to bundle file within mod folder |
| `portrait` | No | Path to portrait image (214x251 PNG) |
| `crc` | Yes | CRC of your built bundle (from Unity catalog) |

---

## Adding Multiple Characters

### Step 1: Build Each Character in Unity

For each custom character:
1. Create prefab with correct path: `Assets/Prefabs/CharacterCosmetics/CharacterCosmetics_*.prefab`
2. Configure Addressables group
3. Build Addressables
4. Note the CRC from the built catalog

### Step 2: Get Target Character Info

Each original character has:
- Hash (bundle filename)
- Original CRC
- Name length (for binary patching)
- Portrait offset in .resS file

**Currently Supported Characters:**

| Character | Hash | Original CRC | Name Length |
|-----------|------|--------------|-------------|
| Felix | 8ce5cfff23e50dfcaa729aa03940bfd7 | 1004267194 | 5 |
| Tilly | fc45ee0b8231bf3537e606898810529f | 2007923026 | 5 |

*More characters can be added to CHARACTER_DATABASE in mod_installer.py*

### Step 3: Choose Matching Names

Binary name patching requires **same character count**:

| Original (5 chars) | Valid Replacements |
|--------------------|-------------------|
| Felix | Zayed, Ahmed, Saeed, Hamad |
| Tilly | Fatma, Marya, Noura, Layla |

For different-length names, use UABEA to edit `sharedassets0.assets` instead.

### Step 4: Create Mod Package

```
MyMultiCharacterMod/
├── mod.json
├── characters/
│   ├── felix/
│   │   ├── bundle.bundle
│   │   └── portrait.png
│   └── tilly/
│       ├── bundle.bundle
│       └── portrait.png
└── README.md
```

### Step 5: Configure mod.json

```json
{
  "name": "UAE Character Pack",
  "version": "1.0.0",
  "characters": [
    {
      "replaces": "Felix",
      "newName": "Zayed",
      "bundle": "characters/felix/bundle.bundle",
      "portrait": "characters/felix/portrait.png",
      "crc": "2057978164"
    },
    {
      "replaces": "Tilly",
      "newName": "Fatma",
      "bundle": "characters/tilly/bundle.bundle",
      "portrait": "characters/tilly/portrait.png",
      "crc": "9876543210"
    }
  ]
}
```

---

## How the Installer Works

### Install Process

1. **Backup** - Creates backups of original files (once)
2. **Bundle** - Copies and renames bundle to match original filename
3. **CRC Patch** - Patches catalog.json with new CRC (from ORIGINAL backup)
4. **Name Patch** - Binary replaces display name in sharedassets0.assets
5. **Portrait** - Binary replaces portrait texture in sharedassets0.assets.resS
6. **State** - Saves install state for uninstall

### Uninstall Process

1. Restores `catalog.json` from backup
2. Restores `sharedassets0.assets` from backup
3. Restores `sharedassets0.assets.resS` from backup
4. Restores original bundles from backup
5. Removes install state file

---

## Getting CRC for Your Bundle

After building in Unity, extract CRC from the catalog:

```python
import json
import base64
import re

catalog_path = r'D:\Unity_Workshop\Endstar Custom Shader\Library\com.unity.addressables\aa\Windows\catalog.json'

with open(catalog_path, 'r') as f:
    catalog = json.load(f)

extra_data = base64.b64decode(catalog['m_ExtraDataString'])

for offset in [0, 1]:
    text = extra_data[offset:].decode('utf-16-le', errors='ignore')
    pattern = r'"m_Hash":"([a-f0-9]{32})"[^}]*"m_Crc":(\d+)'
    for hash_val, crc in re.findall(pattern, text):
        print(f"Hash: {hash_val}, CRC: {crc}")
```

---

## Adding New Characters to DATABASE

Edit `mod_installer.py` and add to `CHARACTER_DATABASE`:

```python
CHARACTER_DATABASE = {
    # ... existing entries ...

    "NewCharacter": {
        "hash": "abc123...",           # From bundle filename
        "original_crc": "123456789",   # From original game catalog
        "bundle_name": "newchar_assets_all_abc123.bundle",
        "name_length": 12,             # len("NewCharacter")
        "name_search": b'\x0C\x00\x00\x00NewCharacter',  # 0x0C = 12
        "portrait_offset": 12345678,   # Find using AssetStudio
        "portrait_size": 214856,       # 214 * 251 * 4
    },
}
```

### Finding Portrait Offset

1. Open `sharedassets0.assets` in AssetStudio
2. Find texture named `CoreCharacterVisual[CharName]*`
3. Note the `m_StreamData.offset` value
4. Add to CHARACTER_DATABASE

---

## Troubleshooting

### "CRC not found in catalog"
- Ensure you're patching from ORIGINAL backup catalog
- Run `python mod_installer.py uninstall` first, then reinstall

### "Name length mismatch"
- Choose a replacement name with same character count
- Or use UABEA for different-length names

### "Portrait offset not configured"
- Character's portrait offset needs to be added to CHARACTER_DATABASE
- Use AssetStudio to find the offset

### Launcher restores original files
- Always launch `Endstar.exe` directly
- Don't use the Endless Launcher

---

## File Locations

| File | Purpose |
|------|---------|
| `mod_installer.py` | Main installer script |
| `ORIGINAL_GAME_BACKUPS/` | Backup storage |
| `ORIGINAL_GAME_BACKUPS/install_state.json` | Tracks what's installed |
| `ZayedMod/` | Example mod package |

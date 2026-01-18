# Endstar Mod Deployment Protocol

## CRITICAL WARNINGS

### DO NOT USE SilhouetteFix Plugin

**The SilhouetteFix BepInEx plugin CAUSES TEXTURE ARTIFACTS.** It was a failed attempt that broke rendering. DELETE IT.

The custom character works correctly WITHOUT any BepInEx plugins. No runtime material swapping is needed.

Location to delete (if exists):
```
C:\Endless Studios\Endless Launcher\Endstar\BepInEx\plugins\SilhouetteFix.dll
```

---

## CRITICAL RULES

### 1. CRC Patching - MUST PRESERVE BYTE LENGTH

When patching the catalog CRC, the new CRC string MUST be padded to match the original CRC length.

**Original Felix CRC:** `1004267194` (10 characters)

If your new CRC is shorter (e.g., `500919581` = 9 characters), you MUST pad with spaces:

```python
old_str = '1004267194'  # 10 chars
new_str = '500919581'   # 9 chars
# PAD IT:
new_str = new_str + ' ' * (len(old_str) - len(new_str))  # Now "500919581 " = 10 chars
```

**WHY:** The catalog's m_ExtraDataString is a base64-encoded binary blob. Changing its length corrupts the entire catalog structure, causing "Invalid path in AssetBundleProvider: ''" errors.

### 2. CRC Source - ONLY FROM UNITY CATALOG

**NEVER** use Python's `zlib.crc32()` - Unity uses a different algorithm.

**ALWAYS** read the CRC from Unity's built catalog:

```python
import json, base64, re

unity_catalog = r'D:\Unity_Workshop\Endstar Custom Shader\Library\com.unity.addressables\aa\Windows\catalog.json'
with open(unity_catalog, 'r') as f:
    catalog = json.load(f)

extra = base64.b64decode(catalog['m_ExtraDataString'])
idx = extra.find('m_Crc'.encode('utf-16-le'))
context = extra[idx:idx+60].decode('utf-16-le', errors='ignore')
crc = re.search(r'm_Crc.:(\d+)', context).group(1)
print(f'Unity CRC: {crc}')
```

### 3. MixMap - DO NOT USE

The `Texture2D_033410430c1e41f38cbe37cd7347362e` slot (MixMap) must be EMPTY (`{fileID: 0}`).

### 4. Material Setup

Required settings in `ThePack_Felix_VC_01_A.mat`:

```yaml
m_ValidKeywords:
- _LIGHT_COOKIES          # REQUIRED for proper rendering

m_TexEnvs:
- _Albedo:
    m_Texture: {fileID: 2800000, guid: YOUR_TEXTURE_GUID, type: 3}
    m_Scale: {x: 1, y: 1}
    m_Offset: {x: 0, y: 0}

- Texture2D_033410430c1e41f38cbe37cd7347362e:  # MixMap
    m_Texture: {fileID: 0}  # MUST BE EMPTY
```

---

## DEPLOYMENT WORKFLOW

### Step 1: Build Bundle in Unity

1. Open Unity 2022.3.62f2 (EXACT version)
2. Window → Asset Management → Addressables → Groups
3. Build → New Build → Default Build Script
4. Wait for build to complete

### Step 2: Get CRC from Unity Catalog

Location: `Library/com.unity.addressables/aa/Windows/catalog.json`

```python
import json, base64, re

with open(r'D:\Unity_Workshop\Endstar Custom Shader\Library\com.unity.addressables\aa\Windows\catalog.json', 'r') as f:
    catalog = json.load(f)

extra = base64.b64decode(catalog['m_ExtraDataString'])
idx = extra.find('m_Crc'.encode('utf-16-le'))
context = extra[idx:idx+60].decode('utf-16-le', errors='ignore')
crc = re.search(r'm_Crc.:(\d+)', context).group(1)
print(f'CRC: {crc}')
```

### Step 3: Update mod.json with CRC

```json
{
  "name": "Pearl Diver",
  "author": "Bora",
  "version": "1.0.0",
  "description": "Custom Pearl Diver character replacing Felix",
  "crc": "500919581"
}
```

### Step 4: Create .esmod Package

```python
import zipfile, json

# Create mod.json
mod_info = {
    "name": "Pearl Diver",
    "crc": "500919581"  # FROM UNITY CATALOG
}

with open('mod.json', 'w') as f:
    json.dump(mod_info, f, indent=2)

# Package
with zipfile.ZipFile('PearlDiver.esmod', 'w', zipfile.ZIP_DEFLATED) as z:
    z.write('path/to/bundle.bundle', 'bundle.bundle')
    z.write('mod.json', 'mod.json')
```

### Step 5: Deploy to Game

**Option A: Use Mod Patcher GUI**
1. Run EndstarModPatcher.exe
2. Select PearlDiver.esmod
3. Click Install

**Option B: Manual Deployment**

```python
import shutil, json, base64

# Paths
GAME_AA = r'C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\StreamingAssets\aa'
BUNDLE_NAME = 'felix_assets_all_8ce5cfff23e50dfcaa729aa03940bfd7.bundle'
ORIGINAL_CRC = '1004267194'
NEW_CRC = '500919581'

# 1. Restore original catalog from backup
shutil.copy(
    r'D:\Endstar Plot Flag\PLOT FLAG SYSTEM\ORIGINAL_GAME_BACKUPS\catalog.json.ORIGINAL',
    f'{GAME_AA}/catalog.json'
)

# 2. Copy bundle
shutil.copy(
    'path/to/your/bundle.bundle',
    f'{GAME_AA}/StandaloneWindows64/{BUNDLE_NAME}'
)

# 3. Patch CRC with proper padding
with open(f'{GAME_AA}/catalog.json', 'r') as f:
    catalog = json.load(f)

extra_data = base64.b64decode(catalog['m_ExtraDataString'])

# CRITICAL: Pad new CRC to match original length
new_crc_padded = NEW_CRC + ' ' * (len(ORIGINAL_CRC) - len(NEW_CRC))

old_bytes = ORIGINAL_CRC.encode('utf-16-le')
new_bytes = new_crc_padded.encode('utf-16-le')

extra_data = extra_data.replace(old_bytes, new_bytes, 1)
catalog['m_ExtraDataString'] = base64.b64encode(extra_data).decode('ascii')

with open(f'{GAME_AA}/catalog.json', 'w') as f:
    json.dump(catalog, f, separators=(',', ':'))
```

### Step 6: Launch Game

**IMPORTANT:** Launch directly, NOT through launcher:
```
C:\Endless Studios\Endless Launcher\Endstar\Endstar.exe
```

The launcher may restore original files.

---

## BACKUP LOCATIONS

```
D:\Endstar Plot Flag\PLOT FLAG SYSTEM\ORIGINAL_GAME_BACKUPS\
├── catalog.json.ORIGINAL
└── felix_assets_all_8ce5cfff23e50dfcaa729aa03940bfd7.bundle.ORIGINAL
```

---

## ERROR REFERENCE

### "CRC Mismatch. Provided X, calculated Y"
- **Cause:** mod.json CRC doesn't match actual bundle
- **Fix:** Get CRC from Unity catalog, not zlib.crc32()

### "Invalid path in AssetBundleProvider: ''"
- **Cause:** Catalog corrupted by CRC patching (length mismatch)
- **Fix:** Restore original catalog, re-patch with proper padding

### Texture artifacts (dark streaks on character)
- **Cause:** SilhouetteFix BepInEx plugin
- **Fix:** DELETE the SilhouetteFix.dll plugin. Character works without it.

---

## FILE LOCATIONS

| File | Location |
|------|----------|
| Unity Project | `D:\Unity_Workshop\Endstar Custom Shader` |
| Built Bundle | `Library\com.unity.addressables\aa\Windows\StandaloneWindows64\*.bundle` |
| Built Catalog | `Library\com.unity.addressables\aa\Windows\catalog.json` |
| Game Bundles | `C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\StreamingAssets\aa\StandaloneWindows64\` |
| Game Catalog | `C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\StreamingAssets\aa\catalog.json` |
| Material | `Assets\Pearl Diver\ThePack_Felix_VC_01_A.mat` |
| Mod Patcher | `D:\Endstar Plot Flag\PLOT FLAG SYSTEM\Zayed_Character_Mod\` |

---

## CONSTANTS

```
TARGET_BUNDLE = 'felix_assets_all_8ce5cfff23e50dfcaa729aa03940bfd7.bundle'
ORIGINAL_FELIX_CRC = '1004267194'
UNITY_VERSION = '2022.3.62f2'
```

---

## FAILED APPROACHES - DO NOT USE

### SilhouetteFix BepInEx Plugin
- **What it did:** Runtime material swapping to "fix" silhouette rendering
- **Result:** CAUSED texture artifacts (dark streaks on character)
- **Status:** DELETED. Do not recreate.

The character renders correctly without any plugins. No runtime fixes needed.

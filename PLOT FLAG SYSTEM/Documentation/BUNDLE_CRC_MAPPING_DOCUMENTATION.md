# Endstar Bundle CRC Mapping Documentation

**VERIFIED FROM ORIGINAL BACKUP CATALOG**
**Date: 2026-01-04**
**CORRECTED: 2026-01-04** - ALL bundles have CRC validation (previous script had a bug)

## Overview

The Endstar game uses Unity Addressables to load character cosmetic bundles. The catalog (`catalog.json`) contains 45 character bundles, and **ALL 45 bundles have CRC validation enabled**.

## Critical Information

**ALL bundles have CRC validation. There are NO exceptions.**

The previous analysis script had a UTF-16LE alignment bug that missed half the CRC entries. The fixed script confirms all 45 bundles have CRC.

**Felix bundle (index 8):**
- Hash: `8ce5cfff23e50dfcaa729aa03940bfd7`
- CRC: `1004267194`
- Size: 3,644,563 bytes

**Implications:**
- Bundle replacement REQUIRES catalog CRC patching for ALL bundles
- You must update the CRC in catalog.json to match your built bundle's CRC
- The hash in the filename must match the original

---

## Complete Bundle Inventory (ALL 45 bundles have CRC)

| Index | Character | Hash | CRC | Size |
|-------|-----------|------|-----|------|
| 0 | Ada | 73a1b9c89be7a95cad39ec00f27c6846 | 309941667 | 10,445,140 |
| 1 | CBC | 992508d28bc468283a5068857b539d33 | 1368571175 | 27,582,551 |
| 2 | Charlemagne | dd31eafa17680b4134249f7a0e6df328 | 3203802575 | 3,967,870 |
| 3 | ConglomerateGrenadierJumper | 5b19ccc5353691fe2422f02a33794be4 | 4177886816 | 3,389,089 |
| 4 | DarkElfFemale | 562c6bf99643a51d0c9a34e99cb1d558 | 3777456336 | 3,929,391 |
| 5 | Duelist | e4e1db0fc3777d915620f7e5e2532219 | 1846673551 | 2,806,540 |
| 6 | Duir | e6e8762187ffb04d2cdaec582b10cf3a | 525012614 | 4,454,119 |
| 7 | Fearn | 0663876ee1550c7865d58fd4ac48a005 | 3463694441 | 4,463,339 |
| **8** | **Felix** | **8ce5cfff23e50dfcaa729aa03940bfd7** | **1004267194** | **3,644,563** |
| 9 | Female Captain | f613129a6e412de9e4745e6f2dd93bb7 | 3615511362 | 3,479,720 |
| 10 | Gamestar Base | ae5343d6faa766e3e98f8358d192e41a | 3635907785 | 3,637,431 |
| 11 | Gamestar Ninja | b09e28269b14976056b8e9f348756cd4 | 1761013629 | 4,033,235 |
| 12 | Gamestar Shooter | 121f1ff8c0b7c8994c1606082d8a4cb2 | 625731700 | 3,327,666 |
| 13 | Gawain | 872dc0e9e23ad663b798f4f4155ee6d9 | 3266939288 | 4,920,839 |
| 14 | Gnoll | 120db24d882dff231ea3b87d3e88bf82 | 3488483599 | 5,626,591 |
| 15 | Han | fea1535235f25c654c04602d276eac91 | 2411852328 | 3,077,121 |
| 16 | Haunt | 12a1546a4662dd50e76a444da0afddbd | 2516597327 | 3,573,193 |
| 17 | Hekamede | 5fdd9c09bfc008d27d0ba3ed65988638 | 459918815 | 3,702,282 |
| 18 | Hilda | 2490c9f31fd88042b7a792527071ee78 | 559865424 | 4,243,006 |
| 19 | Inquisitor | 95fab30c182bcc7121553ffa66252020 | 2805434557 | 3,679,408 |
| 20 | Male Soldier | 29a041da62cb5dd3a4de51cdf6be90b3 | 599231820 | 10,101,702 |
| 21 | Mochi | 8496abd323c7ea278b2015ad5614b707 | 3695538495 | 9,526,244 |
| 22 | Mummy | a7ede76cc5b1eba735b203123e8133e5 | 1792742037 | 3,955,658 |
| 23 | Newall | 5d2d33644664f23ca4bac660fd8e5002 | 21441855 | 4,103,977 |
| 24 | Orc | 5c65789f2ddad58dc2d5c6b245e2153a | 716571679 | 4,127,082 |
| 25 | OrcFemale | 8477c89779fe035979cc2783492fe7d0 | 2141554075 | 4,120,356 |
| 26 | Penningbert | 15716dba10784aac9d6b8a7ec8093559 | 4241433271 | 4,381,911 |
| 27 | Regina | 9df15b839fc6adcf28fdfe8d3528f404 | 1172740251 | 3,534,040 |
| 28 | Riga | c6024c0c6256496f6eadf57e41a48f57 | 949854559 | 4,124,954 |
| 29 | Roberto | 8401fd893192ddd25799bc6a50407bdc | 1970206642 | 10,538,600 |
| 30 | Scully | caf6ff7c7f6c7a915e1fae8af36dd9f8 | 2154489528 | 4,115,339 |
| 31 | Sniper Human | d00d84b21e4a2c3ef5971bfa72574935 | 587662862 | 2,929,117 |
| 32 | Sniper Kit | 49485d29c5fc9e4f7a21b1c01bf8356f | 918870749 | 2,770,132 |
| 33 | Test Cosmetic | 3942df4a24b9d8e8a5214549b6ea8763 | 2358773511 | 8,225,052 |
| 34 | TheBlacksmith | f2c4af4fea7c8e2f2ad986106b9c104d | 1763062043 | 4,021,998 |
| 35 | Tilly | fc45ee0b8231bf3537e606898810529f | 2007923026 | 2,832,947 |
| 36 | Tonks | c18c1a08c83cfaa6f8e23ae075ae6264 | 449487612 | 4,467,972 |
| 37 | Truger | 4833a0e5a4511fc334bf63459c3b0022 | 133702871 | 3,685,612 |
| 38 | Typhon | 7557884a6b99dd4a8490904814bedede | 1026274068 | 4,080,599 |
| 39 | UAE M 1 | 25a5eccc55954714df59c0146cebe570 | 3360433146 | 3,466,046 |
| 40 | Vladimir | 3c558966bf66ab3b5fcb77e80a73b3f6 | 680393459 | 4,119,508 |
| 41 | Waverly | 538a880713d425ee4ded091ead26b328 | 3574370851 | 4,467,530 |
| 42 | Wellington | eb73677ddd53312db758ee827762e715 | 2589164990 | 9,887,035 |
| 43 | Xavier | 2b93e769a77f31bd5eeb77ff21b8a581 | 301138929 | 3,593,355 |
| 44 | Zephyr | ddc90d2b7b70c2121974896f40d1f7bc | 1227927785 | 3,852,673 |

---

## Bundle Replacement Protocol

**ALL bundles require CRC patching.** There are no exceptions.

### For ANY bundle (including Felix):

1. **Build the bundle in Unity** with matching internal asset paths
2. **Get the CRC** from Unity's built catalog:
   ```
   Library/com.unity.addressables/aa/Windows/catalog.json
   ```
   Decode `m_ExtraDataString` (base64 + UTF-16LE) and find `m_Crc` value.

3. **Rename the output bundle** to match the original filename (hash must match)

4. **Copy to game folder:**
   ```
   C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\StreamingAssets\aa\StandaloneWindows64\
   ```

5. **Patch the game catalog CRC:**
   - Find the original CRC in game's catalog.json (in m_ExtraDataString, UTF-16LE encoded)
   - Replace with your new bundle's CRC
   - Re-encode as base64

### Felix-specific details:

- Original CRC: `1004267194`
- Hash: `8ce5cfff23e50dfcaa729aa03940bfd7`
- Bundle filename: `felix_assets_all_8ce5cfff23e50dfcaa729aa03940bfd7.bundle`
- Prefab path: `Assets/Prefabs/CharacterCosmetics/CharacterCosmetics_ThePack_Felix_01_A.prefab`

---

## Catalog Structure

### File Location
```
C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\StreamingAssets\aa\catalog.json
```

### Key Fields

- **m_InternalIds**: Array of all asset paths (bundles + prefabs + scenes)
- **m_ExtraDataString**: Base64-encoded blob containing CRC data in UTF-16LE JSON format
- **m_EntryDataString**: Maps internal IDs to their data entries
- **m_KeyDataString**: Contains addressable keys

### m_ExtraDataString Format

After base64 decoding, contains multiple JSON objects in UTF-16LE:
```json
{
  "m_Hash": "992508d28bc468283a5068857b539d33",
  "m_Crc": 1368571175,
  "m_Timeout": 0,
  "m_ChunkedTransfer": false,
  "m_RedirectLimit": -1,
  "m_RetryCount": 0,
  "m_BundleName": "...",
  "m_AssetLoadMode": 0,
  "m_BundleSize": 12345678,
  ...
}
```

---

## Silhouette Fix Root Cause

The silhouette issue was caused by **missing GBuffer shader pass** in custom-built bundles.

### Technical Details

- Menu Camera uses layer 24 ("UI Background")
- URP pipeline requires 6 shader passes for proper rendering
- Custom bundles had only 5 passes (missing GBuffer)
- `m_PrefilteringModeDeferredRendering: 0` in URP settings was stripping the GBuffer pass

### Fix Applied

Changed in `D:\Unity_Workshop\Endstar Custom Shader\Assets\Settings\URP-HighFidelity.asset`:
```yaml
m_PrefilteringModeDeferredRendering: 1  # Was 0
```

This preserves the GBuffer pass in shader compilation.

---

## Original Game Backups

**CRITICAL: DO NOT OVERWRITE THESE FILES**

Location: `D:\Endstar Plot Flag\PLOT FLAG SYSTEM\ORIGINAL_GAME_BACKUPS\`

| File | MD5 Hash | Size |
|------|----------|------|
| felix_assets_all_8ce5cfff23e50dfcaa729aa03940bfd7.bundle.ORIGINAL | C5BA5D2FD3EC78B87FC3278004CD9C3B | 3,644,563 bytes |
| catalog.json.ORIGINAL | 2D17A5FF9B5D9C551458D97CBA117B0C | 98,080 bytes |

### Restore Instructions

To restore the game to original state:
```powershell
# Restore Felix bundle
Copy-Item "D:\Endstar Plot Flag\PLOT FLAG SYSTEM\ORIGINAL_GAME_BACKUPS\felix_assets_all_8ce5cfff23e50dfcaa729aa03940bfd7.bundle.ORIGINAL" "C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\StreamingAssets\aa\StandaloneWindows64\felix_assets_all_8ce5cfff23e50dfcaa729aa03940bfd7.bundle"

# Restore catalog
Copy-Item "D:\Endstar Plot Flag\PLOT FLAG SYSTEM\ORIGINAL_GAME_BACKUPS\catalog.json.ORIGINAL" "C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\StreamingAssets\aa\catalog.json"
```

---

## Files and Tools

### Analysis Script
```
D:\Endstar Plot Flag\PLOT FLAG SYSTEM\analyze_catalog.py
```

### Camera Dump Plugin
```
D:\Endstar Plot Flag\PLOT FLAG SYSTEM\Zayed_Character_Mod\CameraDump\Plugin.cs
```
- Press F8 in-game to dump camera/material info
- Logs to BepInEx/LogOutput.log

### URP Settings
```
D:\Unity_Workshop\Endstar Custom Shader\Assets\Settings\URP-HighFidelity.asset
```

---

## Summary

1. **ALL 45 bundles have CRC protection** - catalog patching is REQUIRED for all bundle replacements
2. **Felix CRC**: `1004267194` (hash: `8ce5cfff23e50dfcaa729aa03940bfd7`)
3. **Silhouette fix** - set `m_PrefilteringModeDeferredRendering: 1` in URP settings
4. **Bundle filename** - must match original hash exactly for Addressables to find it
5. **CRC must match** - the CRC in catalog.json must match the actual bundle's CRC or the game rejects it
6. **Previous analysis was wrong** - the old script had a UTF-16LE alignment bug that missed half the CRC entries

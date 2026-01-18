# Endstar Custom Character Injection Workflow

## Overview

This document provides the complete workflow for injecting a custom character cosmetic into Endstar.

## Prerequisites

- [x] AssetStudio compiled and ready
- [x] Catalog structure analyzed
- [x] FBX SDK 2020.2.1 installed
- [ ] Unity 2022.3.62f2 installed (exact version required)
- [ ] Character model (FBX with humanoid rig)

## Phase 1: Extract Reference Character

### Using AssetStudio:

1. **Load Bundle**
   ```
   File > Load file
   Path: C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\StreamingAssets\aa\StandaloneWindows64\felix_assets_all_8ce5cfff23e50dfcaa729aa03940bfd7.bundle
   ```

2. **Set Assembly Folder**
   ```
   C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed
   ```

3. **Export Assets**
   - Export FBX model (Scene Hierarchy > Model > Export)
   - Export textures (Asset List > Select textures > Export)

4. **Document Structure**
   - Note bone hierarchy
   - Note material assignments
   - Note animator setup

## Phase 2: Create Unity Project

### Project Setup:

1. **Install Unity 2022.3.62f2** (exact version!)
   - Download from Unity Hub

2. **Create New Project**
   - Name: `EndstarCustomCharacter`
   - Template: 3D (URP or Standard)

3. **Install Addressables**
   ```
   Window > Package Manager > Add package
   com.unity.addressables version 1.22.3
   ```

### Import Character:

1. Import your custom FBX model
2. Configure rig as **Humanoid**
3. Ensure bone naming matches Endstar conventions

### Create Prefab:

1. Create folder: `Assets/Prefabs/CharacterCosmetics/`
2. Create prefab: `CharacterCosmetic_Custom_[YourName].prefab`
3. Add required components:
   - Animator with humanoid avatar
   - SkinnedMeshRenderer with materials

## Phase 3: Build Addressable Bundle

### Configure Addressables:

1. **Window > Asset Management > Addressables > Groups**
2. Create new group: `custom_character`
3. Add your prefab to the group
4. Set address: `Assets/Prefabs/CharacterCosmetics/CharacterCosmetic_Custom_[YourName].prefab`

### Build Bundle:

1. **Build > New Build > Default Build Script**
2. Output: `Library/com.unity.addressables/aa/Windows/StandaloneWindows64/`
3. Rename bundle: `custom_[yourname]_assets_all_[hash].bundle`

## Phase 4: Inject into Catalog

### Modify catalog.json:

Run the injection script:
```bash
python character_injector.py
# Choose option 5: Create Injection Package
```

This will:
1. Backup existing catalog
2. Add bundle reference to m_InternalIds
3. Add prefab path to m_InternalIds
4. Generate custom GUID

### Manual Steps:

The binary data (m_KeyDataString, m_BucketDataString, m_EntryDataString) requires special handling.

**Option A: Simple Approach (Recommended)**
- Replace an existing character's bundle with yours
- Keep the same prefab path
- Use the existing GUID

**Option B: Full Injection**
- Use Unity Addressables to rebuild catalog
- Include your asset in the build

## Phase 5: Modify resources.assets (Optional)

If adding a completely new character (not replacing):

1. **Open in UABE:**
   ```
   C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\resources.assets
   ```

2. **Find CharacterCosmeticsDefinition:**
   - Look for existing definitions
   - Export one as template

3. **Create New Definition:**
   ```json
   {
     "assetId": "<your-custom-guid>",
     "assetReference": {
       "m_AssetGUID": "<bundle-guid>",
       "m_SubObjectName": "Assets/Prefabs/CharacterCosmetics/..."
     }
   }
   ```

4. **Import back into resources.assets**

## Phase 6: Deploy and Test

### Copy Files:

1. Copy your bundle to:
   ```
   C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\StreamingAssets\aa\StandaloneWindows64\
   ```

2. Replace catalog.json (backup first!)

3. Set your character GUID:
   ```bash
   python test_cosmetic_guid.py --set "<your-custom-guid>"
   ```

### Test:

1. Launch Endstar
2. Check if your character loads
3. If Tilly shows instead, check:
   - Bundle file exists
   - Catalog entries correct
   - GUID matches

## Troubleshooting

| Issue | Cause | Solution |
|-------|-------|----------|
| Game crashes | Invalid bundle format | Verify Unity version matches |
| Tilly shows | GUID not found | Check catalog entries |
| Invisible character | Wrong prefab path | Verify path in catalog |
| T-pose | Missing animator | Add AnimatorController |
| Wrong scale | Scale mismatch | Adjust in Unity |

## File Summary

| File | Location | Purpose |
|------|----------|---------|
| `character_injector.py` | This folder | Catalog modification tool |
| `test_cosmetic_guid.py` | This folder | Registry GUID setter |
| `analyze_characters.py` | This folder | Catalog analysis |
| `decode_catalog.py` | This folder | Binary data decoder |
| `AssetStudioGUI.exe` | AssetStudio-master/... | Asset extraction |

## Distribution

To share with friends:
1. Your custom bundle file
2. Modified catalog.json
3. The GUID to use
4. Instructions to copy files

Players without the mod will see Tilly (fallback).

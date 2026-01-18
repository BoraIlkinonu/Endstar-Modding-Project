# Endstar Custom Player Character Cosmetics
## Complete Technical Documentation

**Date:** January 2026
**Game Version:** Unity 2022.3.62f2
**Addressables Version:** 1.22.3

---

## Table of Contents

1. [Overview](#1-overview)
2. [How Character Cosmetics Work in Endstar](#2-how-character-cosmetics-work-in-endstar)
3. [SDK Investigation](#3-sdk-investigation)
4. [Decompiled Code Analysis](#4-decompiled-code-analysis)
5. [Addressables System Analysis](#5-addressables-system-analysis)
6. [Multiplayer Considerations](#6-multiplayer-considerations)
7. [Validation Testing](#7-validation-testing)
8. [Implementation Approach](#8-implementation-approach)
9. [Step-by-Step Guide](#9-step-by-step-guide)
10. [Tools and Scripts](#10-tools-and-scripts)
11. [File Locations Reference](#11-file-locations-reference)
12. [Limitations and Caveats](#12-limitations-and-caveats)

---

## 1. Overview

### Goal
Enable custom player character visuals in Endstar that:
- Can be seen by the player using them
- Can be seen by other players (friends) in multiplayer
- Work within the game's existing systems

### Conclusion
Custom character cosmetics **ARE possible** through game modding, but:
- NOT supported by the official SDK
- Requires manual injection of assets
- Friends must have the same mod files installed
- Players without the mod see a fallback character (Tilly)

---

## 2. How Character Cosmetics Work in Endstar

### System Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                     CHARACTER COSMETICS FLOW                     │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  PlayerPrefs                CharacterCosmeticsList               │
│  ┌──────────────┐          ┌──────────────────────┐             │
│  │ "Character   │          │ List of all          │             │
│  │  Visual"     │──────────│ CharacterCosmetics   │             │
│  │ = GUID       │  lookup  │ Definition objects   │             │
│  └──────────────┘          └──────────┬───────────┘             │
│                                       │                          │
│                                       ▼                          │
│                     CharacterCosmeticsDefinition                 │
│                     ┌──────────────────────────┐                │
│                     │ - displayName            │                │
│                     │ - assetId (GUID)         │                │
│                     │ - assetReference ────────┼──┐             │
│                     │ - portraitSprite         │  │             │
│                     └──────────────────────────┘  │             │
│                                                   │             │
│                            Addressables           ▼             │
│                     ┌──────────────────────────────────┐        │
│                     │ catalog.json                     │        │
│                     │ ┌────────────────────────────┐   │        │
│                     │ │ Key: prefab path           │   │        │
│                     │ │ Value: bundle location     │   │        │
│                     │ └────────────────────────────┘   │        │
│                     └──────────────────────────────────┘        │
│                                    │                             │
│                                    ▼                             │
│                     ┌──────────────────────────────┐            │
│                     │ .bundle file                 │            │
│                     │ Contains: Prefab, Mesh,      │            │
│                     │ Materials, Textures, etc.    │            │
│                     └──────────────────────────────┘            │
│                                    │                             │
│                                    ▼                             │
│                     ┌──────────────────────────────┐            │
│                     │ Instantiated Character       │            │
│                     │ in Game World                │            │
│                     └──────────────────────────────┘            │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

### Key Components

| Component | Type | Location | Purpose |
|-----------|------|----------|---------|
| PlayerPrefs "Character Visual" | Registry | `HKCU\Software\Endless Studios\Endstar` | Stores selected cosmetic GUID |
| CharacterCosmeticsList | Class (Gameplay.dll) | Compiled in DLL | Contains all available cosmetic definitions |
| CharacterCosmeticsDefinition | ScriptableObject | resources.assets or bundles | Defines a single cosmetic with GUID and asset reference |
| catalog.json | JSON | StreamingAssets/aa/ | Maps Addressable keys to bundle locations |
| .bundle files | AssetBundle | StreamingAssets/aa/StandaloneWindows64/ | Contains actual character assets |

---

## 3. SDK Investigation

### What the Endstar SDK Supports

| Asset Type | SDK Support | Upload Pipeline |
|------------|-------------|-----------------|
| Props | ✅ Yes | Cloud upload via GraphQL |
| Terrain Tiles | ✅ Yes | Cloud upload via GraphQL |
| Particle Systems | ✅ Yes | Cloud upload via GraphQL |
| NPCs | ✅ Yes | Cloud upload via GraphQL |
| **Character Cosmetics** | ❌ No | Not exposed |

### SDK Location
```
D:\Unity_Workshop\Endless Studios\Endstar\Endstar SDKs\Endstar SDK Updated
```

### SDK Package DLLs
```
Library\PackageCache\com.endless-studios.endstar-sdk@7268c9a695\DLLs\
├── Assets.dll
├── Authentication.dll
├── Editor.ExportUtility.dll
├── Editor.Internal.dll
├── Editor.ParticleSystemSDK.dll
├── Editor.PropSDK.dll           # Props have SDK
├── Editor.TerrainCosmetics.dll  # Terrain has SDK
├── Editor.Widgets.dll
├── Props.dll
├── Runtime.dll
├── Shared.DataTypes.dll
└── ... (no CharacterCosmetics SDK)
```

### Why Character Cosmetics Aren't in SDK

The prop upload system uses:
```csharp
asset.AssetType = "prop";
CloudService.CreateAssetAsync(asset);
```

Character cosmetics would need:
```csharp
asset.AssetType = "character-cosmetic"; // Not implemented server-side
```

The server-side infrastructure for character cosmetics as user-generated content does not exist.

---

## 4. Decompiled Code Analysis

### CharacterCosmeticsDefinition (from Gameplay.dll)

```csharp
[CreateAssetMenu(
    menuName = "ScriptableObject/Character Cosmetics/Character Cosmetics Definition",
    fileName = "Character Cosmetics")]
public class CharacterCosmeticsDefinition : ScriptableObject
{
    [SerializeField] private string displayName;
    [SerializeField] private string assetId;                    // GUID string
    [SerializeField] private AssetReferenceGameObject assetReference;  // Addressables ref
    [SerializeField] private Sprite portraitSprite;

    public string DisplayName => this.displayName;
    public SerializableGuid AssetId => (SerializableGuid)this.assetId;
    public bool IsMissingAsset => !this.assetReference.RuntimeKeyIsValid();
    public bool IsLoaded => this.assetReference.IsDone;
    public Sprite PortraitSprite => this.portraitSprite;

    public AsyncOperationHandle<GameObject> Instantiate(Transform parent = null, bool isInstantiatedInWorldSpace = false)
    {
        AsyncOperationHandle<GameObject> handle = this.assetReference.InstantiateAsync(parent, isInstantiatedInWorldSpace);
        handle.Completed += HandleObjectInstantiationComplete;
        return handle;
    }

    private static void HandleObjectInstantiationComplete(AsyncOperationHandle<GameObject> handle)
    {
        handle.Result.AddComponent<SelfCleanup>();
    }
}
```

### CharacterCosmeticsDefinitionUtility (from Gameplay.dll)

```csharp
public static class CharacterCosmeticsDefinitionUtility
{
    private const string CHARACTER_VISUAL_KEY = "Character Visual";

    public static SerializableGuid GetClientCharacterVisualId()
    {
        return PlayerPrefs.GetString("Character Visual");
    }

    public static void SetClientCharacterVisualId(SerializableGuid characterCosmeticsDefinitionAssetId)
    {
        PlayerPrefs.SetString("Character Visual", (string)characterCosmeticsDefinitionAssetId);

        // Fire event for immediate visual update
        Action<SerializableGuid> action = ClientCharacterCosmeticsDefinitionAssetSetAction;
        if (action != null)
            action(characterCosmeticsDefinitionAssetId);
    }

    public static Action<SerializableGuid> ClientCharacterCosmeticsDefinitionAssetSetAction;
}
```

### Key Insight

The character visual is stored client-side in PlayerPrefs. The `assetReference` uses Unity Addressables to load the character prefab from local bundles - NOT from cloud servers like props.

---

## 5. Addressables System Analysis

### Catalog Location
```
C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\StreamingAssets\aa\catalog.json
```

### Catalog Structure

```json
{
    "m_LocatorId": "AddressablesMainContentCatalog",
    "m_BuildResultHash": "...",
    "m_ProviderIds": [...],
    "m_InternalIds": [
        // Bundle references (indices 0-44)
        "{RuntimePath}\\StandaloneWindows64\\felix_assets_all_xxx.bundle",

        // Prefab addresses (indices 45-89)
        "Assets/Prefabs/CharacterCosmetics/CharacterCosmetics_ThePack_Felix_01_A.prefab",
        ...
    ],
    "m_KeyDataString": "...",      // Base64 encoded binary - maps keys to buckets
    "m_BucketDataString": "...",   // Base64 encoded binary - maps buckets to entries
    "m_EntryDataString": "...",    // Base64 encoded binary - maps entries to internal IDs
    "m_resourceTypes": [...]
}
```

### Character Bundles (45 total)

| Character | Bundle File |
|-----------|-------------|
| Ada | ada_assets_all_73a1b9c89be7a95cad39ec00f27c6846.bundle |
| Felix | felix_assets_all_8ce5cfff23e50dfcaa729aa03940bfd7.bundle |
| Hilda | hilda_assets_all_2490c9f31fd88042b7a792527071ee78.bundle |
| Mochi | mochi_assets_all_8496abd323c7ea278b2015ad5614b707.bundle |
| ... | ... |
| test_cosmetic | test_cosmetic_assets_all_3942df4a24b9d8e8a5214549b6ea8763.bundle |

### Character Prefab Paths (Examples)

```
Assets/Prefabs/CharacterCosmetics/CharacterCosmetics_ThePack_Felix_01_A.prefab
Assets/Prefabs/CharacterCosmetics/CharacterCosmetics_ThePack_Mochi_01_A.prefab
Assets/Prefabs/CharacterCosmetics/CharacterCosmetics_AncientAliens_Hilda_VC_01_A.prefab
Assets/Prefabs/CharacterCosmetics/CharacterCosmetics_Armature_Test_01.prefab  // Test prefab exists!
```

### Bundle Location
```
C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\StreamingAssets\aa\StandaloneWindows64\
```

---

## 6. Multiplayer Considerations

### The Problem

Endstar is a multiplayer game. Character visuals must be synchronized across all players.

### How Sync Works

```
┌─────────────┐         ┌─────────────┐         ┌─────────────┐
│  Player A   │         │   Server    │         │  Player B   │
│  (You)      │         │             │         │  (Friend)   │
└──────┬──────┘         └──────┬──────┘         └──────┬──────┘
       │                       │                       │
       │ "My GUID: abc-123"    │                       │
       │──────────────────────>│                       │
       │                       │                       │
       │                       │ "Player A uses abc-123"
       │                       │──────────────────────>│
       │                       │                       │
       │                       │         Player B's game tries to
       │                       │         load GUID abc-123 from
       │                       │         THEIR local files
       │                       │                       │
```

### Scenarios

| Your Mod Status | Friend's Mod Status | What Friend Sees |
|-----------------|---------------------|------------------|
| Has custom char | Has same mod | Your custom character ✅ |
| Has custom char | No mod | Tilly (fallback) ⚠️ |
| Has custom char | Different mod | Tilly (fallback) ⚠️ |

### Key Insight

The server does NOT validate cosmetic GUIDs - it just passes them through. The validation happens client-side. If a GUID is unknown, the client falls back to Tilly.

---

## 7. Validation Testing

### Test Performed

We set a fake/invalid GUID to test how the game handles unknown cosmetics.

### Test Script Used

```python
# test_cosmetic_guid.py
import winreg

REGISTRY_PATH = r"Software\Endless Studios\Endstar"
KEY_NAME = "Character Visual_h1713213608"

def set_guid(new_guid):
    with winreg.OpenKey(winreg.HKEY_CURRENT_USER, REGISTRY_PATH, 0, winreg.KEY_SET_VALUE) as key:
        value = (new_guid + '\x00').encode('utf-8')
        winreg.SetValueEx(key, KEY_NAME, 0, winreg.REG_BINARY, value)
```

### Test Procedure

1. Read current GUID: `571e855b-57f9-4130-a3c3-90bad1184c04` (Rashed)
2. Set fake GUID: `00000000-0000-0000-0000-000000000001`
3. Launch game
4. Observe result

### Test Results

| Observation | Result |
|-------------|--------|
| Game crashed | ❌ No |
| Server kicked | ❌ No |
| Character invisible | ❌ No |
| Showed fallback (Tilly) | ✅ Yes |
| Game fully functional | ✅ Yes |

### Conclusion

- Client-side validation exists (unknown GUID → fallback to Tilly)
- Server-side validation does NOT exist (game works normally)
- Custom cosmetics will work if properly injected into client files
- Friends need the same mod files to see custom characters

### Restoration

```bash
python test_cosmetic_guid.py --restore "571e855b-57f9-4130-a3c3-90bad1184c04"
```

Character immediately changed back to Rashed while in lobby (instant, no restart needed).

---

## 8. Implementation Approach

### Two Approaches

#### Approach A: Full Custom Character (Complex)

Create entirely new character from scratch:

1. Build character prefab in Unity 2022.3.62f2
2. Create Addressable bundle
3. Modify catalog.json (complex binary-encoded data)
4. Inject CharacterCosmeticsDefinition via UABE
5. Add to CharacterCosmeticsList

**Difficulty:** High
**Risk:** Catalog modification may break if encoded incorrectly

#### Approach B: Asset Replacement (Easier)

Replace assets inside existing character bundle:

1. Extract existing bundle with AssetStudio
2. Replace mesh, textures, materials with your own
3. Repack bundle with same structure
4. Use existing character's GUID

**Difficulty:** Medium
**Risk:** Lower - no catalog modification needed

### Recommended: Approach B

Replacing assets in an existing bundle is safer because:
- No catalog.json modification required
- CharacterCosmeticsDefinition already exists
- GUID already registered in CharacterCosmeticsList
- Just need to replace the visual assets

---

## 9. Step-by-Step Guide

### Prerequisites

| Tool | Version | Purpose |
|------|---------|---------|
| Unity | 2022.3.62f2 | Match game version |
| Addressables | 1.22.3 | Bundle building |
| AssetStudio | Latest | Extract existing assets |
| UABE | Latest | Modify Unity assets (if needed) |

### Approach B: Asset Replacement

#### Step 1: Choose Target Character

Pick a character you don't use (e.g., Tilly, Test Cosmetic).

Example - using `test_cosmetic`:
- Bundle: `test_cosmetic_assets_all_3942df4a24b9d8e8a5214549b6ea8763.bundle`
- This is likely a dev test character

#### Step 2: Extract Original Assets

1. Download AssetStudio from:
   - https://github.com/Razviar/assetstudio (2025 updated)
   - https://github.com/zhangjiequan/AssetStudio

2. Open AssetStudio
3. File → Load File → Select:
   ```
   C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\StreamingAssets\aa\StandaloneWindows64\test_cosmetic_assets_all_3942df4a24b9d8e8a5214549b6ea8763.bundle
   ```

4. Export all assets:
   - Export → All assets
   - Note the structure: meshes, materials, textures, prefab

#### Step 3: Prepare Your Character

In Unity 2022.3.62f2:

1. Import your character model (FBX)
2. Ensure it has:
   - Compatible rig/skeleton (humanoid)
   - Proper materials
   - Same component structure as original

3. Export as:
   - Mesh (.mesh or .fbx)
   - Textures (.png)
   - Materials (.mat)

#### Step 4: Repack Bundle

**Option A: Using Unity**

1. Create Unity project with Addressables 1.22.3
2. Import extracted assets
3. Replace with your assets (keep same names/structure)
4. Build Addressable bundle
5. Rename to match original bundle name

**Option B: Using UABE**

1. Open bundle in UABE
2. Replace individual assets (mesh, textures)
3. Save modified bundle

#### Step 5: Install Modified Bundle

1. Backup original:
   ```
   copy test_cosmetic_assets_all_xxx.bundle test_cosmetic_assets_all_xxx.bundle.backup
   ```

2. Replace with your modified bundle

#### Step 6: Set Your GUID

Find the GUID for your target character, then:

```bash
python test_cosmetic_guid.py --set "<target-character-guid>"
```

#### Step 7: Test

1. Launch Endstar
2. Your custom character should appear
3. Test in multiplayer with friends who have the mod

### Approach A: Full Custom Character (Advanced)

If you need a completely new character slot:

#### Step 1: Build Addressable Bundle

```
Unity Project/
├── Assets/
│   └── Prefabs/
│       └── CharacterCosmetics/
│           └── CharacterCosmetic_Custom_YourName.prefab
```

Configure Addressables:
- Group: `custom_yourname`
- Address: `Assets/Prefabs/CharacterCosmetics/CharacterCosmetic_Custom_YourName.prefab`

Build → New Build → Default Build Script

#### Step 2: Modify catalog.json

The catalog uses binary-encoded data strings. You need to:

1. Decode `m_KeyDataString`, `m_BucketDataString`, `m_EntryDataString`
2. Add your bundle and prefab entries
3. Re-encode

**WARNING:** This is complex. The safest method is to rebuild the entire catalog in Unity with your asset included.

#### Step 3: Create CharacterCosmeticsDefinition

Using UABE:

1. Open `resources.assets` or the asset file containing definitions
2. Find existing CharacterCosmeticsDefinition
3. Clone/copy it
4. Modify:
   - `assetId`: Your new GUID
   - `displayName`: Your character name
   - `assetReference`: Path to your prefab
5. Save

#### Step 4: Update CharacterCosmeticsList

The list needs to include your new definition. This may require:
- Modifying the serialized list in assets
- Or patching Gameplay.dll (advanced)

---

## 10. Tools and Scripts

### Scripts Created

All scripts located in: `D:\Endstar Plot Flag\PLOT FLAG SYSTEM\`

#### test_cosmetic_guid.py

Manage character visual GUID in PlayerPrefs.

```bash
# Read current GUID
python test_cosmetic_guid.py

# Set fake GUID for testing
python test_cosmetic_guid.py --set-fake

# Restore specific GUID
python test_cosmetic_guid.py --restore "571e855b-57f9-4130-a3c3-90bad1184c04"

# Set custom GUID
python test_cosmetic_guid.py --set "your-custom-guid-here"
```

#### parse_catalog.py

Analyze catalog.json structure.

```bash
python parse_catalog.py
```

#### parse_catalog2.py

Deep analysis of catalog keys and cosmetic entries.

```bash
python parse_catalog2.py
```

#### add_custom_character.py

Helper for adding custom characters (partial implementation).

```bash
python add_custom_character.py
```

#### get_unity_version.py

Detect Unity version from game files.

```bash
python get_unity_version.py
```

### External Tools

| Tool | Download | Purpose |
|------|----------|---------|
| AssetStudio | [GitHub - Razviar](https://github.com/Razviar/assetstudio) | Extract Unity assets |
| UABE | [GitHub](https://github.com/SeriousCache/UABE) | Modify Unity assets |
| Unity Hub | [Unity](https://unity.com/download) | Install Unity 2022.3.62f2 |

---

## 11. File Locations Reference

### Game Files

```
C:\Endless Studios\Endless Launcher\Endstar\
├── Endstar.exe
├── UnityPlayer.dll
└── Endstar_Data\
    ├── app.info                           # Company/Product name
    ├── globalgamemanagers                 # Unity version info
    ├── resources.assets                   # Compiled Resources (ScriptableObjects here)
    ├── sharedassets0.assets               # Shared assets
    ├── level0                             # Main scene
    ├── Managed\
    │   ├── Gameplay.dll                   # Character cosmetics code
    │   └── ...
    └── StreamingAssets\
        └── aa\
            ├── catalog.json               # Addressables catalog
            ├── settings.json              # Addressables settings
            └── StandaloneWindows64\
                ├── felix_assets_all_xxx.bundle
                ├── test_cosmetic_assets_all_xxx.bundle
                └── ... (45 character bundles)
```

### PlayerPrefs (Registry)

```
HKEY_CURRENT_USER\Software\Endless Studios\Endstar\
└── Character Visual_h1713213608    # Current character GUID
```

### SDK Location

```
D:\Unity_Workshop\Endless Studios\Endstar\Endstar SDKs\Endstar SDK Updated\
├── Assets\
│   └── Scripts\
│       └── CharacterVisualChanger.cs    # Custom script (won't work for this purpose)
└── Library\
    └── PackageCache\
        └── com.endless-studios.endstar-sdk@7268c9a695\
            └── DLLs\
                ├── Editor.PropSDK.dll
                └── ... (no character cosmetics SDK)
```

---

## 12. Limitations and Caveats

### Technical Limitations

| Limitation | Impact | Workaround |
|------------|--------|------------|
| No SDK support | Can't upload to cloud | Local mod files only |
| Catalog is binary-encoded | Hard to modify | Use Unity to rebuild catalog |
| CharacterCosmeticsList compiled | Hard to add new entries | Replace existing character assets |
| Different platforms | Need separate bundles | Build for Windows only (or all platforms) |

### Multiplayer Limitations

| Scenario | Behavior |
|----------|----------|
| You have mod, friend doesn't | Friend sees Tilly |
| Different mod versions | May see Tilly or errors |
| Official game update | May break mod (bundle hash changes) |

### Legal/ToS Considerations

- Modifying game files may violate Terms of Service
- Use at your own risk
- Don't distribute copyrighted content
- Don't use for cheating/exploits

### Maintenance

- Game updates may break mods
- Bundle hashes change with updates
- Catalog structure may change
- Need to re-apply mod after updates

---

## Appendix A: Known Character GUIDs

To find a character's GUID, you can:
1. Select that character in-game
2. Read the GUID from registry:
   ```bash
   python test_cosmetic_guid.py
   ```

Example found:
- Rashed: `571e855b-57f9-4130-a3c3-90bad1184c04`

---

## Appendix B: Catalog Analysis Output

```
=== CATALOG KEYS ===
m_LocatorId: AddressablesMainContentCatalog
m_InternalIds: list of 147 items

=== ALL COSMETIC INTERNAL IDS ===
[8] felix_assets_all_8ce5cfff23e50dfcaa729aa03940bfd7.bundle
[33] test_cosmetic_assets_all_3942df4a24b9d8e8a5214549b6ea8763.bundle
[84] Assets/Prefabs/CharacterCosmetics/CharacterCosmetics_ThePack_Felix_01_A.prefab
...

Total bundles: 45
Total cosmetic prefabs: 48
```

---

## Appendix C: Quick Reference

### Check Current Character
```bash
python test_cosmetic_guid.py
```

### Set Custom Character
```bash
python test_cosmetic_guid.py --set "your-guid-here"
```

### Restore Original
```bash
python test_cosmetic_guid.py --restore "571e855b-57f9-4130-a3c3-90bad1184c04"
```

### Bundle Location
```
C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\StreamingAssets\aa\StandaloneWindows64\
```

### Catalog Location
```
C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\StreamingAssets\aa\catalog.json
```

---

*Document generated from investigation session, January 2026*

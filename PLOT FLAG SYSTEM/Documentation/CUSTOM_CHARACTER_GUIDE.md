# Custom Character Cosmetics for Endstar

## Requirements

- **Unity 2022.3.62f2** (exact version)
- **Addressables 1.22.3** package
- **UABE** (Unity Assets Bundle Extractor)
- Your custom character model (FBX with rig/animations)

---

## Step 1: Create Unity Project

1. Install Unity 2022.3.62f2 via Unity Hub
2. Create new 3D project: `EndstarCustomCosmetics`
3. Install Addressables package:
   - Window > Package Manager
   - Add package: `com.unity.addressables` version `1.22.3`

---

## Step 2: Setup Character Prefab

1. Import your character model (FBX)
2. Create prefab at: `Assets/Prefabs/CharacterCosmetics/CharacterCosmetic_Custom_YourName.prefab`
3. Make sure it has:
   - Animator component
   - Same bone structure as existing characters (important for animations)

---

## Step 3: Configure Addressables

1. Window > Asset Management > Addressables > Groups
2. Create new group: `custom_character`
3. Add your prefab to this group
4. Set address to: `Assets/Prefabs/CharacterCosmetics/CharacterCosmetic_Custom_YourName.prefab`

---

## Step 4: Build Addressable Bundle

1. Window > Asset Management > Addressables > Groups
2. Build > New Build > Default Build Script
3. Find your bundle in: `Library/com.unity.addressables/aa/Windows/StandaloneWindows64/`
4. Rename to: `custom_yourname_assets_all_<hash>.bundle`

---

## Step 5: Modify catalog.json

Use the helper script to add your bundle to the catalog.

---

## Step 6: Create CharacterCosmeticsDefinition

Use UABE to:
1. Extract an existing CharacterCosmeticsDefinition from resources.assets
2. Clone it
3. Modify assetId to your new GUID
4. Modify assetReference to point to your prefab address
5. Inject back

---

## Step 7: Test

1. Copy your bundle to: `Endstar_Data/StreamingAssets/aa/StandaloneWindows64/`
2. Replace catalog.json with modified version
3. Set your custom GUID in PlayerPrefs
4. Launch game

---

## Distribution

Share with friends:
- Your custom bundle file
- Modified catalog.json
- Modified resources.assets (with CharacterCosmeticsDefinition)
- The custom GUID to set

Players without mod will see Tilly (fallback character).

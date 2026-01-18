"""
Endstar Custom Character Cosmetic Injector
Modifies catalog.json to add a custom character bundle
"""

import json
import base64
import uuid
import shutil
import os
from datetime import datetime

GAME_PATH = r"C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\StreamingAssets\aa"
CATALOG_PATH = os.path.join(GAME_PATH, "catalog.json")
BUNDLE_PATH = os.path.join(GAME_PATH, "StandaloneWindows64")

def generate_guid():
    """Generate a new GUID for the custom cosmetic"""
    return str(uuid.uuid4())

def backup_catalog():
    """Backup original catalog"""
    backup_path = CATALOG_PATH + f".backup_{datetime.now().strftime('%Y%m%d_%H%M%S')}"
    shutil.copy(CATALOG_PATH, backup_path)
    print(f"Backup created: {backup_path}")
    return backup_path

def load_catalog():
    """Load the catalog.json"""
    with open(CATALOG_PATH, 'r') as f:
        return json.load(f)

def save_catalog(catalog, output_path=None):
    """Save the modified catalog"""
    path = output_path or CATALOG_PATH
    with open(path, 'w') as f:
        json.dump(catalog, f, separators=(',', ':'))
    print(f"Saved catalog to: {path}")

def add_bundle_to_catalog(catalog, bundle_name, prefab_path):
    """
    Add a new bundle and prefab to the catalog.

    NOTE: This is a simplified approach. The full implementation would need to:
    1. Decode m_KeyDataString, m_BucketDataString, m_EntryDataString (binary encoded)
    2. Add entries to the decoded data
    3. Re-encode them

    For now, we'll just add to m_InternalIds and document what else needs to happen.
    """

    internal_ids = catalog['m_InternalIds']

    # Add bundle reference
    bundle_ref = "{UnityEngine.AddressableAssets.Addressables.RuntimePath}\\StandaloneWindows64\\" + bundle_name
    if bundle_ref not in internal_ids:
        internal_ids.append(bundle_ref)
        bundle_idx = len(internal_ids) - 1
        print(f"Added bundle at index {bundle_idx}: {bundle_name}")
    else:
        bundle_idx = internal_ids.index(bundle_ref)
        print(f"Bundle already exists at index {bundle_idx}")

    # Add prefab reference
    if prefab_path not in internal_ids:
        internal_ids.append(prefab_path)
        prefab_idx = len(internal_ids) - 1
        print(f"Added prefab at index {prefab_idx}: {prefab_path}")
    else:
        prefab_idx = internal_ids.index(prefab_path)
        print(f"Prefab already exists at index {prefab_idx}")

    print("\n" + "="*60)
    print("WARNING: This script only adds to m_InternalIds.")
    print("You also need to modify the encoded data strings:")
    print("  - m_KeyDataString (maps keys to bucket indices)")
    print("  - m_BucketDataString (maps buckets to entry indices)")
    print("  - m_EntryDataString (maps entries to internal IDs)")
    print("\nFor a complete implementation, use the Addressables")
    print("package in Unity to rebuild the catalog with your asset.")
    print("="*60 + "\n")

    return bundle_idx, prefab_idx

def analyze_existing_character(catalog, character_name):
    """Analyze an existing character to understand the structure"""
    print(f"\n=== Analyzing: {character_name} ===\n")

    internal_ids = catalog['m_InternalIds']

    for i, id in enumerate(internal_ids):
        if character_name.lower() in id.lower():
            print(f"  [{i}] {id}")

def main():
    print("="*60)
    print("  Endstar Custom Character Cosmetic Tool")
    print("="*60)

    # Generate GUID for custom character
    custom_guid = generate_guid()
    print(f"\nGenerated GUID for your custom character:")
    print(f"  {custom_guid}")
    print("\nSave this GUID! You'll need it to select your character.")

    # Backup catalog
    print("\n--- Backing up catalog ---")
    backup_catalog()

    # Load catalog
    print("\n--- Loading catalog ---")
    catalog = load_catalog()
    print(f"Loaded catalog with {len(catalog['m_InternalIds'])} internal IDs")

    # Analyze Felix as reference
    analyze_existing_character(catalog, "felix")

    # Example: Add custom character
    print("\n--- Example: Adding custom character ---")
    print("Bundle name: custom_mycharacter_assets_all_abc123.bundle")
    print("Prefab path: Assets/Prefabs/CharacterCosmetics/CharacterCosmetic_Custom_MyCharacter.prefab")

    # Uncomment to actually modify:
    # add_bundle_to_catalog(
    #     catalog,
    #     "custom_mycharacter_assets_all_abc123.bundle",
    #     "Assets/Prefabs/CharacterCosmetics/CharacterCosmetic_Custom_MyCharacter.prefab"
    # )
    # save_catalog(catalog, CATALOG_PATH + ".modified")

    print("\n--- Next Steps ---")
    print("1. Build your character as an Addressable bundle in Unity 2022.3.62f2")
    print("2. Copy bundle to:", BUNDLE_PATH)
    print("3. Rebuild catalog using Unity Addressables (recommended)")
    print("   OR use UABE to modify the catalog binary data")
    print("4. Use UABE to add CharacterCosmeticsDefinition to resources.assets")
    print(f"5. Set your GUID in PlayerPrefs: {custom_guid}")
    print("6. Test the game!")

if __name__ == "__main__":
    main()

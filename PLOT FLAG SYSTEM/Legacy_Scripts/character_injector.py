"""
Endstar Custom Character Injector
Complete tool for injecting custom character cosmetics into Endstar
"""

import json
import base64
import struct
import os
import shutil
import uuid
from datetime import datetime

# Paths
GAME_PATH = r"C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data"
STREAMING_PATH = os.path.join(GAME_PATH, "StreamingAssets", "aa")
CATALOG_PATH = os.path.join(STREAMING_PATH, "catalog.json")
BUNDLE_PATH = os.path.join(STREAMING_PATH, "StandaloneWindows64")

class CatalogDecoder:
    """Decode Unity Addressables catalog binary data"""

    @staticmethod
    def decode_bucket_data(data_string):
        """Decode m_BucketDataString - maps key indices to entry indices"""
        data = base64.b64decode(data_string)
        buckets = []
        offset = 0

        while offset < len(data):
            # Each bucket: 4 bytes offset, 4 bytes count
            if offset + 8 > len(data):
                break
            entry_offset = struct.unpack('<I', data[offset:offset+4])[0]
            entry_count = struct.unpack('<I', data[offset+4:offset+8])[0]
            buckets.append({'offset': entry_offset, 'count': entry_count})
            offset += 8

        return buckets

    @staticmethod
    def decode_entry_data(data_string):
        """Decode m_EntryDataString - maps entries to internal IDs"""
        data = base64.b64decode(data_string)
        entries = []
        offset = 0

        while offset < len(data):
            if offset + 28 > len(data):
                break
            # Entry structure: internal_id_idx(4), provider_idx(4), dependency_key_idx(4),
            #                  dep_hash(4), data_idx(4), primary_key_idx(4), resource_type_idx(4)
            entry = {
                'internal_id_idx': struct.unpack('<i', data[offset:offset+4])[0],
                'provider_idx': struct.unpack('<i', data[offset+4:offset+8])[0],
                'dependency_key_idx': struct.unpack('<i', data[offset+8:offset+12])[0],
                'dep_hash': struct.unpack('<i', data[offset+12:offset+16])[0],
                'data_idx': struct.unpack('<i', data[offset+16:offset+20])[0],
                'primary_key_idx': struct.unpack('<i', data[offset+20:offset+24])[0],
                'resource_type_idx': struct.unpack('<i', data[offset+24:offset+28])[0]
            }
            entries.append(entry)
            offset += 28

        return entries

    @staticmethod
    def decode_key_data(data_string):
        """Decode m_KeyDataString - the actual keys (strings, GUIDs, etc.)"""
        data = base64.b64decode(data_string)
        keys = []
        offset = 0

        while offset < len(data):
            if offset >= len(data):
                break
            # First byte is type indicator
            key_type = data[offset]
            offset += 1

            if key_type == 0:  # String
                # Length-prefixed string
                if offset >= len(data):
                    break
                str_len = data[offset]
                offset += 1
                if offset + str_len > len(data):
                    break
                key_str = data[offset:offset+str_len].decode('utf-8', errors='ignore')
                keys.append({'type': 'string', 'value': key_str})
                offset += str_len
            elif key_type == 4:  # Int32
                if offset + 4 > len(data):
                    break
                value = struct.unpack('<i', data[offset:offset+4])[0]
                keys.append({'type': 'int32', 'value': value})
                offset += 4
            else:
                # Unknown type, try to skip
                keys.append({'type': f'unknown_{key_type}', 'value': None})
                break

        return keys


def backup_catalog():
    """Create timestamped backup of catalog.json"""
    backup_path = CATALOG_PATH + f".backup_{datetime.now().strftime('%Y%m%d_%H%M%S')}"
    shutil.copy(CATALOG_PATH, backup_path)
    print(f"Backup created: {backup_path}")
    return backup_path


def load_catalog():
    """Load catalog.json"""
    with open(CATALOG_PATH, 'r') as f:
        return json.load(f)


def save_catalog(catalog, output_path=None):
    """Save modified catalog"""
    path = output_path or CATALOG_PATH
    with open(path, 'w') as f:
        json.dump(catalog, f, separators=(',', ':'))
    print(f"Saved catalog to: {path}")


def list_character_bundles():
    """List all character cosmetic bundles"""
    print("\n=== Character Cosmetic Bundles ===\n")

    bundles = []
    for f in os.listdir(BUNDLE_PATH):
        if f.endswith('.bundle'):
            # Character bundles typically have character names
            lower = f.lower()
            if any(name in lower for name in ['felix', 'rashed', 'mira', 'tilly', 'kira', 'bun', 'lumi',
                                               'nix', 'quinn', 'riley', 'cosmetic', 'pack']):
                bundles.append(f)
                print(f"  {f}")

    print(f"\nTotal character bundles: {len(bundles)}")
    return bundles


def analyze_catalog():
    """Analyze catalog structure for character cosmetics"""
    print("\n=== Catalog Analysis ===\n")

    catalog = load_catalog()
    internal_ids = catalog['m_InternalIds']

    print(f"Total Internal IDs: {len(internal_ids)}")

    # Find character cosmetic entries
    cosmetic_entries = []
    for i, id in enumerate(internal_ids):
        if 'CharacterCosmetic' in id or 'charactercosmetic' in id.lower():
            cosmetic_entries.append((i, id))

    print(f"\nCharacter Cosmetic Entries ({len(cosmetic_entries)}):")
    for idx, entry in cosmetic_entries[:10]:  # Show first 10
        print(f"  [{idx}] {entry}")
    if len(cosmetic_entries) > 10:
        print(f"  ... and {len(cosmetic_entries) - 10} more")

    # Find bundle entries
    bundle_entries = []
    for i, id in enumerate(internal_ids):
        if '.bundle' in id.lower():
            bundle_entries.append((i, id))

    print(f"\nBundle Entries ({len(bundle_entries)}):")
    for idx, entry in bundle_entries[:10]:
        print(f"  [{idx}] {entry}")
    if len(bundle_entries) > 10:
        print(f"  ... and {len(bundle_entries) - 10} more")

    return catalog


def find_character_by_name(catalog, name):
    """Find all entries related to a specific character"""
    internal_ids = catalog['m_InternalIds']
    results = []

    for i, id in enumerate(internal_ids):
        if name.lower() in id.lower():
            results.append((i, id))

    return results


def generate_custom_character_guid():
    """Generate a new GUID for custom character"""
    return str(uuid.uuid4())


def create_injection_package(custom_bundle_name, custom_prefab_path, custom_guid):
    """
    Create the injection package structure.

    This creates the necessary files and modifications needed to inject
    a custom character into Endstar.
    """

    print("\n=== Creating Injection Package ===\n")

    # Create output directory
    output_dir = os.path.join(os.path.dirname(__file__), "injection_package")
    os.makedirs(output_dir, exist_ok=True)

    # Load and backup catalog
    backup_catalog()
    catalog = load_catalog()

    # Add bundle to internal IDs
    bundle_ref = "{UnityEngine.AddressableAssets.Addressables.RuntimePath}\\StandaloneWindows64\\" + custom_bundle_name
    internal_ids = catalog['m_InternalIds']

    if bundle_ref not in internal_ids:
        internal_ids.append(bundle_ref)
        bundle_idx = len(internal_ids) - 1
        print(f"Added bundle reference at index {bundle_idx}")
    else:
        bundle_idx = internal_ids.index(bundle_ref)
        print(f"Bundle reference exists at index {bundle_idx}")

    # Add prefab path
    if custom_prefab_path not in internal_ids:
        internal_ids.append(custom_prefab_path)
        prefab_idx = len(internal_ids) - 1
        print(f"Added prefab path at index {prefab_idx}")
    else:
        prefab_idx = internal_ids.index(custom_prefab_path)
        print(f"Prefab path exists at index {prefab_idx}")

    # Save modified catalog
    modified_catalog_path = os.path.join(output_dir, "catalog.json")
    save_catalog(catalog, modified_catalog_path)

    # Create instructions file
    instructions = f"""
=== CUSTOM CHARACTER INJECTION INSTRUCTIONS ===

Custom Character GUID: {custom_guid}
Bundle Name: {custom_bundle_name}
Prefab Path: {custom_prefab_path}

STEP 1: Copy Bundle
Copy your custom bundle to:
  {BUNDLE_PATH}\\{custom_bundle_name}

STEP 2: Replace Catalog
Replace the game's catalog.json with the modified one:
  Source: {modified_catalog_path}
  Destination: {CATALOG_PATH}

STEP 3: Add CharacterCosmeticsDefinition (UABE)
Use UABE to:
1. Open: {GAME_PATH}\\resources.assets
2. Export an existing CharacterCosmeticsDefinition as template
3. Modify:
   - assetId: {custom_guid}
   - assetReference.m_AssetGUID: <your bundle GUID>
   - assetReference.m_SubObjectName: {custom_prefab_path}
4. Import the modified asset

STEP 4: Set Your Character
Run: python test_cosmetic_guid.py --set "{custom_guid}"

STEP 5: Test
Launch Endstar and verify your character loads!

=== TROUBLESHOOTING ===
- If game crashes: Restore catalog.json.backup_*
- If Tilly shows: Bundle not found or GUID mismatch
- If invisible: Prefab path incorrect
"""

    instructions_path = os.path.join(output_dir, "INJECTION_INSTRUCTIONS.txt")
    with open(instructions_path, 'w') as f:
        f.write(instructions)

    print(f"\nInjection package created at: {output_dir}")
    print(f"Instructions saved to: {instructions_path}")

    return output_dir


def main():
    """Main menu"""
    while True:
        print("\n" + "="*60)
        print("  ENDSTAR CUSTOM CHARACTER INJECTOR")
        print("="*60)
        print("\n1. List Character Bundles")
        print("2. Analyze Catalog Structure")
        print("3. Find Character By Name")
        print("4. Generate Custom Character GUID")
        print("5. Create Injection Package")
        print("6. Backup Catalog")
        print("0. Exit")

        choice = input("\nChoice: ").strip()

        if choice == '1':
            list_character_bundles()
        elif choice == '2':
            analyze_catalog()
        elif choice == '3':
            name = input("Enter character name: ").strip()
            catalog = load_catalog()
            results = find_character_by_name(catalog, name)
            print(f"\nFound {len(results)} entries for '{name}':")
            for idx, entry in results:
                print(f"  [{idx}] {entry}")
        elif choice == '4':
            guid = generate_custom_character_guid()
            print(f"\nGenerated GUID: {guid}")
            print("Save this GUID! You'll need it to select your character.")
        elif choice == '5':
            print("\n--- Create Injection Package ---")
            bundle = input("Custom bundle filename (e.g., custom_mychar.bundle): ").strip()
            prefab = input("Prefab path (e.g., Assets/Prefabs/CharacterCosmetics/CharacterCosmetic_Custom_MyChar.prefab): ").strip()
            guid = input("Custom GUID (leave blank to generate): ").strip()
            if not guid:
                guid = generate_custom_character_guid()
                print(f"Generated GUID: {guid}")
            create_injection_package(bundle, prefab, guid)
        elif choice == '6':
            backup_catalog()
        elif choice == '0':
            break
        else:
            print("Invalid choice")


if __name__ == "__main__":
    main()

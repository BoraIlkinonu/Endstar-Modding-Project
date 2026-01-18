import json
import base64
import re
import os
import shutil

catalog_path = r"C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\StreamingAssets\aa\catalog.json"
backup_path = r"C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\StreamingAssets\aa\catalog_original_backup.json"

# Restore original first
if os.path.exists(backup_path):
    shutil.copy(backup_path, catalog_path)
    print("Restored original catalog")

# Read catalog
with open(catalog_path, 'r', encoding='utf-8') as f:
    catalog = json.load(f)

# Find felix in internal IDs
print("=== INTERNAL IDs (bundles) ===")
for i, internal_id in enumerate(catalog['m_InternalIds']):
    if 'bundle' in internal_id.lower():
        print(f"  [{i}] {internal_id}")
        if 'felix' in internal_id.lower():
            print(f"       ^ FELIX at index {i}")

# Felix is at index 8 in internal IDs
# The extra data should have bundle options in the same order

# Decode extra data
extra_data_b64 = catalog['m_ExtraDataString']
extra_data = base64.b64decode(extra_data_b64)

# Find all CRCs
extra_str = extra_data.decode('utf-16-le', errors='ignore')
crcs = re.findall(r'"m_Crc":(\d+)', extra_str)
print(f"\nFound {len(crcs)} CRCs in extra data")

# The extra data entries correspond to bundles
# Felix at internal ID index 8 = extra data index 8

# But wait - there are 44+ bundles but only 22 CRCs
# Let me check the m_InternalIds more carefully

bundles = [x for x in catalog['m_InternalIds'] if 'bundle' in x.lower()]
print(f"\nTotal bundles in m_InternalIds: {len(bundles)}")

# Find felix's position among bundles only
for i, b in enumerate(bundles):
    if 'felix' in b.lower():
        print(f"Felix is bundle #{i} in the bundle list")
        felix_bundle_idx = i
        break

# CRC index should match bundle index
if felix_bundle_idx < len(crcs):
    old_crc = int(crcs[felix_bundle_idx])
    print(f"Felix's CRC at index {felix_bundle_idx}: {old_crc}")

    # Our bundle's CRC from error: 0x2ff5dd23 = 804642083
    # But let's calculate it properly

    # Actually, let me try changing THAT specific CRC
    new_crc = 804642083  # 0x2ff5dd23 from the error "calculated"

    print(f"Changing CRC from {old_crc} to {new_crc}")

    # Replace in extra data
    old_pattern = f'"m_Crc":{old_crc}'.encode('utf-16-le')
    new_pattern = f'"m_Crc":{new_crc}'.encode('utf-16-le')

    if old_pattern in extra_data:
        extra_data = extra_data.replace(old_pattern, new_pattern, 1)
        print("Replaced CRC successfully!")
    else:
        print("CRC pattern not found - trying different format")
        # Try with spaces or different encoding
        old_str = str(old_crc)
        new_str = str(new_crc)
        # Pad to same length
        if len(new_str) < len(old_str):
            new_str = new_str + ' ' * (len(old_str) - len(new_str))

        old_bytes = old_str.encode('utf-16-le')
        new_bytes = new_str.encode('utf-16-le')

        if old_bytes in extra_data:
            extra_data = extra_data.replace(old_bytes, new_bytes, 1)
            print("Replaced CRC number!")
        else:
            print("Still not found")
else:
    print(f"Felix index {felix_bundle_idx} out of range for CRCs (have {len(crcs)})")

# Deploy our bundle
bundle_src = r"D:\Unity_Workshop\Endstar Custom Shader\Library\com.unity.addressables\aa\Windows\StandaloneWindows64\defaultlocalgroup_assets_all_89aecf0d389953239779b1e21b10bd51.bundle"
bundle_dst = r"C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\StreamingAssets\aa\StandaloneWindows64\felix_assets_all_8ce5cfff23e50dfcaa729aa03940bfd7.bundle"
shutil.copy(bundle_src, bundle_dst)
print(f"\nDeployed our bundle")

# Save modified catalog
new_extra_data_b64 = base64.b64encode(extra_data).decode('ascii')
catalog['m_ExtraDataString'] = new_extra_data_b64

with open(catalog_path, 'w', encoding='utf-8') as f:
    json.dump(catalog, f, separators=(',', ':'))

print("Saved catalog")
print("\n=== Launch game to test ===")

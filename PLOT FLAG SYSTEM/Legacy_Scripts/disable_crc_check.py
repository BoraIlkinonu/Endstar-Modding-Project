import json
import base64
import re
import os
import shutil

catalog_path = r"C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\StreamingAssets\aa\catalog.json"
backup_path = r"C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\StreamingAssets\aa\catalog_original_backup.json"

# Make sure we have original backup
if not os.path.exists(backup_path):
    shutil.copy(catalog_path, backup_path)
    print(f"Created backup: {backup_path}")

# Read catalog
with open(catalog_path, 'r', encoding='utf-8') as f:
    catalog = json.load(f)

# Decode m_ExtraDataString
extra_data_b64 = catalog['m_ExtraDataString']
extra_data = base64.b64decode(extra_data_b64)

print(f"Extra data size: {len(extra_data)} bytes")

# The extra data contains UTF-16LE encoded JSON with bundle options
# We want to:
# 1. Find felix bundle entry
# 2. Set m_UseCrcForCachedBundles to false
# 3. Or change the CRC to match our bundle

# Let's decode and analyze
extra_str = extra_data.decode('utf-16-le', errors='ignore')

# Find all UseCrc patterns
use_crc_matches = re.findall(r'"m_UseCrcForCachedBundles":(true|false)', extra_str)
print(f"Found {len(use_crc_matches)} m_UseCrcForCachedBundles entries")
print(f"Values: {use_crc_matches}")

# Try to disable all CRC checks by replacing true with false
# This is a broad approach but should work

# The data format in extra has multiple bundle option JSONs
# Let's find patterns and replace

# Replace m_UseCrcForCachedBundles:true with false
old_pattern = '"m_UseCrcForCachedBundles":true'.encode('utf-16-le')
new_pattern = '"m_UseCrcForCachedBundles":fals'.encode('utf-16-le')  # Keep same length

# Actually, 'true' and 'false' have different lengths - this won't work
# Let's try a different approach - change the CRC value itself

# Find felix bundle by looking for its hash or name
felix_hash = "8ce5cfff23e50dfcaa729aa03940bfd7"

# Let's find where felix's options are in the extra data
# The bundles are ordered, felix is at index 8

# Let's look at bundle names in order
bundle_names = re.findall(r'"m_BundleName":"([^"]+)"', extra_str)
print(f"\nBundle names ({len(bundle_names)}):")
for i, name in enumerate(bundle_names):
    print(f"  [{i}] {name}")
    if '8ce5cfff' in name or 'felix' in name.lower():
        print(f"      ^ FELIX FOUND at index {i}")

# Find all CRCs
crcs = re.findall(r'"m_Crc":(\d+)', extra_str)
print(f"\nCRCs ({len(crcs)}):")
for i, crc in enumerate(crcs):
    print(f"  [{i}] {crc}")

# The bundle at index 8 should be felix
# Let's replace its CRC

# Our bundle's CRC from the error: calculated 2ff5dd23 = 804642083
new_crc = 804642083

# Find which index is felix
felix_idx = -1
for i, name in enumerate(bundle_names):
    if '8ce5cfff' in name:
        felix_idx = i
        break

if felix_idx >= 0 and felix_idx < len(crcs):
    old_crc = int(crcs[felix_idx])
    print(f"\nFelix at index {felix_idx}")
    print(f"Old CRC: {old_crc}")
    print(f"New CRC: {new_crc}")

    # Replace the CRC
    old_crc_pattern = f'"m_Crc":{old_crc}'.encode('utf-16-le')
    new_crc_pattern = f'"m_Crc":{new_crc}'.encode('utf-16-le')

    # Pad to same length if needed
    if len(old_crc_pattern) != len(new_crc_pattern):
        # Use spaces to pad (won't affect JSON parsing)
        diff = len(old_crc_pattern) - len(new_crc_pattern)
        if diff > 0:
            # Old is longer, pad new
            new_crc_pattern = new_crc_pattern[:-2] + b' ' * (diff // 2) + new_crc_pattern[-2:]
        else:
            # New is longer, need different approach
            print(f"Warning: New CRC string is longer, trying without padding")

    if old_crc_pattern in extra_data:
        extra_data = extra_data.replace(old_crc_pattern, new_crc_pattern, 1)
        print("Replaced CRC!")
    else:
        print("Old CRC pattern not found in binary data")
else:
    print(f"Could not find felix bundle (index={felix_idx})")

# Now copy our bundle over
bundle_src = r"D:\Unity_Workshop\Endstar Custom Shader\Library\com.unity.addressables\aa\Windows\StandaloneWindows64\defaultlocalgroup_assets_all_89aecf0d389953239779b1e21b10bd51.bundle"
bundle_dst = r"C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\StreamingAssets\aa\StandaloneWindows64\felix_assets_all_8ce5cfff23e50dfcaa729aa03940bfd7.bundle"

shutil.copy(bundle_src, bundle_dst)
print(f"\nCopied our bundle to game folder")

# Re-encode and save catalog
new_extra_data_b64 = base64.b64encode(extra_data).decode('ascii')
catalog['m_ExtraDataString'] = new_extra_data_b64

with open(catalog_path, 'w', encoding='utf-8') as f:
    json.dump(catalog, f, separators=(',', ':'))

print("Saved modified catalog")
print("\n=== DONE - Launch game to test ===")

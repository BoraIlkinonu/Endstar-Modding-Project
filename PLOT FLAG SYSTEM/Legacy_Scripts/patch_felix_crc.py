import json
import base64
import re
import os
import shutil

catalog_path = r"C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\StreamingAssets\aa\catalog.json"
backup_path = r"C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\StreamingAssets\aa\catalog_original_backup.json"

# Restore from backup first to get clean state
if os.path.exists(backup_path):
    shutil.copy(backup_path, catalog_path)
    print("Restored catalog from backup")

# Read catalog
with open(catalog_path, 'r', encoding='utf-8') as f:
    catalog = json.load(f)

# Decode m_ExtraDataString
extra_data_b64 = catalog['m_ExtraDataString']
extra_data = base64.b64decode(extra_data_b64)

# The error message said:
# Provided 3bdbe6ba, calculated 2ff5dd23
# Provided = what catalog says (1004422842 decimal)
# Calculated = our bundle's actual CRC (804019491 decimal)

old_crc_hex = "3bdbe6ba"
old_crc_dec = int(old_crc_hex, 16)  # 1004422842

new_crc_hex = "2ff5dd23"
new_crc_dec = int(new_crc_hex, 16)  # 804019491

print(f"Old CRC (from error): 0x{old_crc_hex} = {old_crc_dec}")
print(f"New CRC (our bundle): 0x{new_crc_hex} = {new_crc_dec}")

# The extra data appears to be: header + multiple UTF-16LE JSON objects
# Let's decode and find all m_Crc values

# Skip header bytes (variable length type descriptors)
# Look for JSON patterns

# Find all m_Crc values in the data
extra_str = extra_data.decode('utf-16-le', errors='ignore')
crc_matches = re.findall(r'"m_Crc":(\d+)', extra_str)
print(f"\nFound {len(crc_matches)} CRC values in catalog")

# Also look for bundle names/hashes to correlate
bundle_matches = re.findall(r'"m_BundleName":"([^"]+)"', extra_str)
print(f"Found {len(bundle_matches)} bundle names")

# Print CRCs and bundles
for i, (crc, bundle) in enumerate(zip(crc_matches, bundle_matches)):
    if 'felix' in bundle.lower() or crc == str(old_crc_dec):
        print(f"  [{i}] CRC={crc}, Bundle={bundle}")

# The CRC for felix should be at a specific index
# Let's find felix by looking for its hash
felix_hash = "8ce5cfff23e50dfcaa729aa03940bfd7"

# Search for this hash in bundle names
for i, bundle in enumerate(bundle_matches):
    if felix_hash[:12] in bundle:  # First 12 chars of hash
        print(f"\nFelix bundle found at index {i}: {bundle}")
        print(f"Corresponding CRC: {crc_matches[i]}")

# Now replace the old CRC with new CRC in UTF-16LE format
old_crc_pattern = f'"m_Crc":{old_crc_dec}'.encode('utf-16-le')
new_crc_pattern = f'"m_Crc":{new_crc_dec}'.encode('utf-16-le')

if old_crc_pattern in extra_data:
    print(f"\nFound exact CRC pattern for {old_crc_dec}")
    extra_data = extra_data.replace(old_crc_pattern, new_crc_pattern)
    print(f"Replaced: {old_crc_dec} -> {new_crc_dec}")
else:
    print(f"\nCRC {old_crc_dec} not found directly. Trying alternative search...")

    # Search for just the number
    old_num = str(old_crc_dec).encode('utf-16-le')
    new_num = str(new_crc_dec).encode('utf-16-le')

    if old_num in extra_data:
        # Make sure we're replacing the right one by checking context
        idx = extra_data.find(old_num)
        context = extra_data[max(0,idx-50):idx+len(old_num)+10]
        print(f"Found {old_crc_dec} at offset {idx}")
        print(f"Context: {context.decode('utf-16-le', errors='ignore')}")

        # Only replace if it's in the right context (m_Crc)
        if b'm\x00_\x00C\x00r\x00c' in context or b'C\x00r\x00c' in context:
            extra_data = extra_data.replace(old_num, new_num, 1)
            print(f"Replaced CRC number")
        else:
            print("Context doesn't look like m_Crc, not replacing")
    else:
        print(f"CRC number {old_crc_dec} not found in extra data")

        # Last resort: search for hex bytes
        print("\nSearching entire extra data for felix-related CRCs...")
        # The CRC might be stored differently

# Re-encode and save
new_extra_data_b64 = base64.b64encode(extra_data).decode('ascii')
catalog['m_ExtraDataString'] = new_extra_data_b64

with open(catalog_path, 'w', encoding='utf-8') as f:
    json.dump(catalog, f, separators=(',', ':'))

print("\n=== Done. Testing game... ===")

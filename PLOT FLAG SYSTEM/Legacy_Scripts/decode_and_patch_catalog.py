import json
import base64
import struct
import os
import shutil

catalog_path = r"C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\StreamingAssets\aa\catalog.json"
backup_path = r"C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\StreamingAssets\aa\catalog_original_backup.json"

# Backup original
if not os.path.exists(backup_path):
    shutil.copy(catalog_path, backup_path)
    print(f"Created backup: {backup_path}")

# Read catalog
with open(catalog_path, 'r', encoding='utf-8') as f:
    catalog = json.load(f)

print("=== CATALOG ANALYSIS ===")
print(f"Internal IDs count: {len(catalog['m_InternalIds'])}")

# Find felix bundle index
felix_bundle_idx = -1
for i, internal_id in enumerate(catalog['m_InternalIds']):
    if 'felix_assets_all' in internal_id:
        felix_bundle_idx = i
        print(f"Felix bundle at index {i}: {internal_id}")
        break

if felix_bundle_idx == -1:
    print("Felix bundle not found!")
    exit(1)

# Decode m_ExtraDataString
extra_data_b64 = catalog['m_ExtraDataString']
extra_data = base64.b64decode(extra_data_b64)

print(f"\nm_ExtraDataString decoded length: {len(extra_data)} bytes")

# The extra data contains serialized AssetBundleRequestOptions objects
# Format: type info followed by JSON-like data

# Let's look for CRC patterns
# CRC 0x3bdbe6ba = 1004422842 in decimal
# Our CRC 0x2ff5dd23 = 804019491 in decimal

old_crc = 1004422842
new_crc = 804019491  # 0x2ff5dd23

# Convert to bytes (little-endian uint32)
old_crc_bytes = struct.pack('<I', old_crc)
new_crc_bytes = struct.pack('<I', new_crc)

print(f"Old CRC bytes: {old_crc_bytes.hex()}")
print(f"New CRC bytes: {new_crc_bytes.hex()}")

# Search for old CRC in extra_data
if old_crc_bytes in extra_data:
    print("Found old CRC in extra data (binary)!")
    extra_data = extra_data.replace(old_crc_bytes, new_crc_bytes)
    print("Replaced binary CRC")
else:
    print("Old CRC not found as binary in extra data")

# Also search for the decimal string representation
old_crc_str = str(old_crc).encode('utf-16-le')
new_crc_str = str(new_crc).encode('utf-16-le')

# The JSON inside is UTF-16 encoded
extra_str = extra_data.decode('latin-1')  # Use latin-1 to preserve bytes

# Look for the CRC in the string
if str(old_crc) in extra_str or f'"m_Crc":{old_crc}' in extra_str:
    print(f"Found CRC {old_crc} in string format")

# Let's try to find and replace the CRC pattern in the raw bytes
# Looking for pattern like "m_Crc":XXXXXXXXX in UTF-16LE
search_pattern = f'"m_Crc":{old_crc}'.encode('utf-16-le')
replace_pattern = f'"m_Crc":{new_crc}'.encode('utf-16-le')

if search_pattern in extra_data:
    print("Found m_Crc pattern in UTF-16LE!")
    extra_data = extra_data.replace(search_pattern, replace_pattern)
    print(f"Replaced CRC: {old_crc} -> {new_crc}")
else:
    # Try without quotes
    search_pattern2 = f'm_Crc":{old_crc}'.encode('utf-16-le')
    if search_pattern2 in extra_data:
        replace_pattern2 = f'm_Crc":{new_crc}'.encode('utf-16-le')
        extra_data = extra_data.replace(search_pattern2, replace_pattern2)
        print(f"Replaced CRC (alt pattern): {old_crc} -> {new_crc}")
    else:
        print("CRC pattern not found. Searching hex dump...")
        # Print some context around where felix might be
        extra_hex = extra_data.hex()
        # Search for recognizable patterns

        # Actually let's look at the raw data for felix bundle's extra info
        # The bundles are indexed, felix is at index 8 (from earlier analysis)
        print("\nLooking for CRC in extra data...")

        # Try finding the decimal CRC directly
        old_crc_utf16 = str(old_crc).encode('utf-16-le')
        if old_crc_utf16 in extra_data:
            new_crc_utf16 = str(new_crc).encode('utf-16-le')
            extra_data = extra_data.replace(old_crc_utf16, new_crc_utf16)
            print(f"Replaced UTF-16LE CRC: {old_crc} -> {new_crc}")
        else:
            print(f"CRC {old_crc} not found in UTF-16LE format")

            # Let's decode the whole thing and look manually
            try:
                # Skip initial type info bytes and try to find JSON
                for i in range(len(extra_data)):
                    try:
                        chunk = extra_data[i:].decode('utf-16-le')
                        if 'm_Crc' in chunk:
                            print(f"Found m_Crc at offset {i}")
                            # Extract context
                            start = chunk.find('m_Crc')
                            context = chunk[start:start+50]
                            print(f"Context: {context}")
                            break
                    except:
                        continue
            except Exception as e:
                print(f"Error during search: {e}")

# Re-encode extra data
new_extra_data_b64 = base64.b64encode(extra_data).decode('ascii')
catalog['m_ExtraDataString'] = new_extra_data_b64

# Save modified catalog
with open(catalog_path, 'w', encoding='utf-8') as f:
    json.dump(catalog, f, separators=(',', ':'))

print("\n=== CATALOG PATCHED ===")
print("Restart the game to test.")

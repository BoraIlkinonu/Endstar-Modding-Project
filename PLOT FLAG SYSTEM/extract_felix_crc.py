import json
import base64
import re
import sys

# Force UTF-8 output
sys.stdout.reconfigure(encoding='utf-8')

backup_path = r'C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\StreamingAssets\aa\catalog_backup.json'

with open(backup_path, 'r', encoding='utf-8') as f:
    catalog = json.load(f)

extra_data = base64.b64decode(catalog['m_ExtraDataString'])

print("=== ANALYZING EXTRA DATA STRUCTURE ===")
print(f"Total ExtraData length: {len(extra_data)} bytes")

# The format is:
# - 1 byte: 0x07 (type marker)
# - 1 byte: type1 string length
# - type1 string (ASCII)
# - 1 byte: type2 string length
# - type2 string (ASCII)
# - Then: series of data blocks, each starting with 2-byte length (LE) + UTF-16LE JSON

# Parse header
pos = 0
marker = extra_data[pos]
pos += 1

type1_len = extra_data[pos]
pos += 1
type1 = extra_data[pos:pos+type1_len].decode('ascii')
pos += type1_len

type2_len = extra_data[pos]
pos += 1
type2 = extra_data[pos:pos+type2_len].decode('ascii')
pos += type2_len

print(f"Header marker: 0x{marker:02x}")
print(f"Type1: {type1}")
print(f"Type2: {type2}")
print(f"Data starts at offset: {pos}")

# Now read all data blocks
print("\n=== ALL BUNDLE DATA BLOCKS ===")
block_num = 0
bundles = [(i, id) for i, id in enumerate(catalog['m_InternalIds']) if id.endswith('.bundle')]

while pos < len(extra_data) and block_num < 50:
    # Read 2-byte length
    if pos + 2 > len(extra_data):
        break

    block_len = int.from_bytes(extra_data[pos:pos+2], 'little')
    pos += 2

    if block_len == 0 or pos + block_len > len(extra_data):
        print(f"Block {block_num}: Invalid length {block_len} at offset {pos-2}")
        break

    # Read UTF-16LE JSON
    json_data = extra_data[pos:pos+block_len]
    pos += block_len

    try:
        json_text = json_data.decode('utf-16-le')

        # Extract key fields
        crc_match = re.search(r'"m_Crc":(\d+)', json_text)
        hash_match = re.search(r'"m_Hash":"([a-f0-9]+)"', json_text)
        size_match = re.search(r'"m_BundleSize":(\d+)', json_text)

        crc = crc_match.group(1) if crc_match else "N/A"
        hash_val = hash_match.group(1) if hash_match else "N/A"
        size = size_match.group(1) if size_match else "N/A"

        bundle_name = bundles[block_num][1].split('\\')[-1] if block_num < len(bundles) else "UNKNOWN"

        marker_str = " <<< FELIX" if block_num == 8 else ""
        print(f"[{block_num:2}] {bundle_name}")
        print(f"      Hash: {hash_val}")
        print(f"      CRC: {crc}")
        print(f"      Size: {size}{marker_str}")

    except Exception as e:
        print(f"Block {block_num}: Error decoding - {e}")

    block_num += 1

print(f"\nTotal blocks read: {block_num}")

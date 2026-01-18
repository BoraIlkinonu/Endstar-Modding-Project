import json
import base64
import re
import os

catalog_path = r'C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\StreamingAssets\aa\catalog.json'

with open(catalog_path, 'r', encoding='utf-8') as f:
    catalog = json.load(f)

# Decode m_ExtraDataString
extra_data = base64.b64decode(catalog['m_ExtraDataString'])
print(f'ExtraDataString length: {len(extra_data)} bytes')

# The extra data starts with a type header, then has UTF-16LE JSON blocks
# Find all JSON objects by looking for patterns
text = extra_data.decode('utf-16-le', errors='ignore')

# Find all JSON-like structures with m_Hash and m_Crc
pattern = r'"m_Hash":"([a-f0-9]+)"[^}]*"m_Crc":(\d+)'
matches = re.findall(pattern, text)

print(f'\n=== CRC ENTRIES FOUND ({len(matches)}) ===')
crc_map = {}
for i, (hash_val, crc) in enumerate(matches):
    print(f'[{i}] Hash: {hash_val} -> CRC: {crc}')
    crc_map[hash_val] = crc

# Get all bundles
bundles = [(i, id) for i, id in enumerate(catalog['m_InternalIds']) if id.endswith('.bundle')]
print(f'\n=== TOTAL BUNDLES: {len(bundles)} ===')

# Create mapping
print('\n=== BUNDLE TO CRC MAPPING ===')
mapping_results = []

for idx, bundle_path in bundles:
    # Extract hash from bundle filename
    match = re.search(r'_([a-f0-9]{32})\.bundle$', bundle_path)
    if match:
        bundle_hash = match.group(1)
        bundle_name = os.path.basename(bundle_path.replace('\\', '/'))

        # Try to find matching CRC by comparing hash prefixes
        found_crc = None
        matched_stored_hash = None

        for stored_hash, crc in crc_map.items():
            # Check if stored hash is a prefix of bundle hash or vice versa
            if bundle_hash.startswith(stored_hash) or stored_hash.startswith(bundle_hash[:len(stored_hash)]):
                found_crc = crc
                matched_stored_hash = stored_hash
                break

        result = {
            'index': idx,
            'bundle_name': bundle_name,
            'bundle_hash': bundle_hash,
            'crc': found_crc,
            'matched_hash': matched_stored_hash
        }
        mapping_results.append(result)

        if found_crc:
            print(f'[{idx:2}] {bundle_name}')
            print(f'      Bundle Hash: {bundle_hash}')
            print(f'      Stored Hash: {matched_stored_hash}')
            print(f'      CRC: {found_crc}')
        else:
            print(f'[{idx:2}] {bundle_name}')
            print(f'      Bundle Hash: {bundle_hash}')
            print(f'      CRC: *** NOT FOUND ***')

# Summary
found_count = sum(1 for r in mapping_results if r['crc'])
print(f'\n=== SUMMARY ===')
print(f'Total bundles: {len(bundles)}')
print(f'Bundles with CRC: {found_count}')
print(f'Bundles without CRC: {len(bundles) - found_count}')

# List bundles without CRC
print(f'\n=== BUNDLES WITHOUT CRC ===')
for r in mapping_results:
    if not r['crc']:
        print(f"  [{r['index']}] {r['bundle_name']}")

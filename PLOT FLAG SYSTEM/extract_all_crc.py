import json
import base64
import re
import sys

sys.stdout.reconfigure(encoding='utf-8')

# Use the verified ORIGINAL backup catalog
backup_path = r'D:\Endstar Plot Flag\PLOT FLAG SYSTEM\ORIGINAL_GAME_BACKUPS\catalog.json.ORIGINAL'

with open(backup_path, 'r', encoding='utf-8') as f:
    catalog = json.load(f)

extra_data = base64.b64decode(catalog['m_ExtraDataString'])

# The ExtraDataString has an ASCII header followed by UTF-16LE JSON blocks.
# Decoding from offset 0 misses some entries due to alignment issues.
# Solution: Try both offset=0 and offset=1 to catch all entries.

all_matches = {}

for offset in [0, 1]:
    text = extra_data[offset:].decode('utf-16-le', errors='ignore')
    # Find all complete JSON objects with hash, crc, size
    pattern = r'"m_Hash":"([a-f0-9]{32})"[^}]*"m_Crc":(\d+)[^}]*"m_BundleSize":(\d+)'
    matches = re.findall(pattern, text)
    for hash_val, crc, size in matches:
        if hash_val not in all_matches:
            all_matches[hash_val] = (crc, size)

print("=== ALL CRC ENTRIES (EXTRACTED FROM ORIGINAL BACKUP CATALOG) ===")
print(f"Found {len(all_matches)} entries\n")

# Get bundle list
bundles = [(i, id.split('\\')[-1]) for i, id in enumerate(catalog['m_InternalIds']) if id.endswith('.bundle')]
bundle_by_hash = {}
for idx, name in bundles:
    # Extract hash from filename
    hash_match = re.search(r'_([a-f0-9]{32})\.bundle$', name)
    if hash_match:
        bundle_by_hash[hash_match.group(1)] = (idx, name)

# Map CRC entries to bundles
print("Index | Bundle Name | Hash | CRC | Size")
print("-" * 120)

found_bundles = []
for hash_val, (crc, size) in sorted(all_matches.items(), key=lambda x: bundle_by_hash.get(x[0], (999, ''))[0]):
    if hash_val in bundle_by_hash:
        idx, name = bundle_by_hash[hash_val]
        marker = " <<< FELIX" if idx == 8 else ""
        print(f"[{idx:2}] | {name[:60]:<60} | {hash_val} | {crc:>12} | {int(size):>10}{marker}")
        found_bundles.append(hash_val)
    else:
        print(f"[??] | UNKNOWN HASH | {hash_val} | {crc:>12} | {int(size):>10}")

# Check which bundles are missing
print("\n=== BUNDLES WITHOUT CRC ENTRIES ===")
for idx, name in bundles:
    hash_match = re.search(r'_([a-f0-9]{32})\.bundle$', name)
    if hash_match:
        h = hash_match.group(1)
        if h not in all_matches:
            marker = " <<< FELIX" if idx == 8 else ""
            print(f"[{idx:2}] {name}{marker}")

print(f"\n=== SUMMARY ===")
print(f"Total bundles: {len(bundles)}")
print(f"Bundles WITH CRC: {len(all_matches)}")
print(f"Bundles WITHOUT CRC: {len(bundles) - len(all_matches)}")

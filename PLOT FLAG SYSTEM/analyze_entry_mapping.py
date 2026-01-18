import json
import base64
import struct
import os

backup_path = r'C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\StreamingAssets\aa\catalog_backup.json'

with open(backup_path, 'r', encoding='utf-8') as f:
    catalog = json.load(f)

# Decode m_EntryDataString
entry_data = base64.b64decode(catalog['m_EntryDataString'])
print(f'EntryDataString length: {len(entry_data)} bytes')
print(f'EntryDataString raw bytes (first 100): {entry_data.hex()[:200]}')

# Parse entry count
entry_count = struct.unpack('<I', entry_data[0:4])[0]
print(f'\nEntry count: {entry_count}')

# Each entry is 7 integers (28 bytes):
# InternalIdIndex, ProviderIndex, DependencyKey, DepHash, DataIndex, PrimaryKeyIndex, ResourceType
entry_size = 7 * 4

# Get bundle indices
bundles = [(i, id) for i, id in enumerate(catalog['m_InternalIds']) if id.endswith('.bundle')]
bundle_indices = {idx: name for idx, name in bundles}

print('\n=== ALL ENTRIES ===')
for i in range(entry_count):
    start = 4 + i * entry_size
    end = start + entry_size
    if end > len(entry_data):
        print(f'WARNING: Entry {i} exceeds data bounds')
        break

    fields = struct.unpack('<7i', entry_data[start:end])
    internal_id_idx, provider_idx, dep_key, dep_hash, data_idx, primary_key_idx, res_type = fields

    internal_id = catalog['m_InternalIds'][internal_id_idx] if internal_id_idx < len(catalog['m_InternalIds']) else 'INVALID'
    is_bundle = internal_id.endswith('.bundle')

    print(f'Entry[{i}]:')
    print(f'  InternalIdIdx: {internal_id_idx} -> {os.path.basename(internal_id)}')
    print(f'  ProviderIdx: {provider_idx}')
    print(f'  DataIdx: {data_idx}')
    print(f'  IsBundle: {is_bundle}')

    # Check if this is Felix (index 8)
    if internal_id_idx == 8:
        print(f'  *** THIS IS FELIX ***')

# Now let's decode m_ExtraDataString and see what DataIdx maps to
print('\n\n=== EXTRA DATA STRUCTURE ===')
extra_data = base64.b64decode(catalog['m_ExtraDataString'])
print(f'ExtraDataString length: {len(extra_data)} bytes')

# The extra data starts with type information, then has JSON blocks
# Let's look at the raw structure
print(f'First 500 bytes: {extra_data[:500]}')

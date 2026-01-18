"""Deep analysis of Endstar catalog binary data"""
import json
import base64
import struct
import os

CATALOG_PATH = r"C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\StreamingAssets\aa\catalog.json"

with open(CATALOG_PATH, 'r') as f:
    catalog = json.load(f)

internal_ids = catalog['m_InternalIds']

print('=== DECODING CATALOG BINARY DATA ===')
print()

# Decode m_EntryDataString
entry_data = base64.b64decode(catalog['m_EntryDataString'])
print(f'm_EntryDataString: {len(entry_data)} bytes')

# Each entry is 28 bytes
entry_size = 28
num_entries = len(entry_data) // entry_size
print(f'Number of entries: {num_entries}')

print()
print('=== ENTRY STRUCTURE ===')
print('Each entry (28 bytes):')
print('  - internal_id_idx (4 bytes): Index into m_InternalIds')
print('  - provider_idx (4 bytes): Index into m_ProviderIds')
print('  - dependency_key_idx (4 bytes): Dependency key')
print('  - dep_hash (4 bytes): Dependency hash')
print('  - data_idx (4 bytes): Extra data index')
print('  - primary_key_idx (4 bytes): Primary key index in m_KeyDataString')
print('  - resource_type_idx (4 bytes): Type index in m_ResourceTypes')

print()
print('=== FELIX ENTRIES ===')

# Find felix entries by looking at all entries and checking internal_id_idx
for i in range(num_entries):
    offset = i * entry_size
    internal_id_idx = struct.unpack('<i', entry_data[offset:offset+4])[0]
    provider_idx = struct.unpack('<i', entry_data[offset+4:offset+8])[0]
    dependency_key_idx = struct.unpack('<i', entry_data[offset+8:offset+12])[0]
    dep_hash = struct.unpack('<i', entry_data[offset+12:offset+16])[0]
    data_idx = struct.unpack('<i', entry_data[offset+16:offset+20])[0]
    primary_key_idx = struct.unpack('<i', entry_data[offset+20:offset+24])[0]
    resource_type_idx = struct.unpack('<i', entry_data[offset+24:offset+28])[0]

    # Check if this entry references felix
    if 0 <= internal_id_idx < len(internal_ids):
        internal_id = internal_ids[internal_id_idx]
        if 'felix' in internal_id.lower():
            print(f'\nEntry #{i}:')
            print(f'  internal_id_idx: {internal_id_idx} -> {internal_id[:80]}...')
            print(f'  provider_idx: {provider_idx}')
            print(f'  dependency_key_idx: {dependency_key_idx}')
            print(f'  dep_hash: {dep_hash}')
            print(f'  data_idx: {data_idx}')
            print(f'  primary_key_idx: {primary_key_idx}')
            print(f'  resource_type_idx: {resource_type_idx}')

# Show providers
print()
print('=== PROVIDERS ===')
for i, provider in enumerate(catalog['m_ProviderIds']):
    print(f'[{i}] {provider}')

# Show resource types
print()
print('=== RESOURCE TYPES ===')
for i, rt in enumerate(catalog['m_ResourceTypes']):
    print(f'[{i}] {rt["m_AssemblyName"]} :: {rt["m_ClassName"]}')

# Decode key data
print()
print('=== KEY DATA ANALYSIS ===')
key_data = base64.b64decode(catalog['m_KeyDataString'])
print(f'm_KeyDataString: {len(key_data)} bytes')

# Try to extract keys
# Keys are variable length - first byte is type, then data
offset = 0
keys = []
while offset < len(key_data):
    key_type = key_data[offset]
    offset += 1

    if key_type == 0:  # ASCII string
        str_len = key_data[offset]
        offset += 1
        key_str = key_data[offset:offset+str_len].decode('utf-8', errors='replace')
        keys.append(('string', key_str))
        offset += str_len
    elif key_type == 4:  # Int32
        if offset + 4 <= len(key_data):
            value = struct.unpack('<i', key_data[offset:offset+4])[0]
            keys.append(('int32', value))
            offset += 4
        else:
            break
    elif key_type == 1:  # Possible UTF-16 string
        if offset < len(key_data):
            str_len = key_data[offset]
            offset += 1
            if offset + str_len*2 <= len(key_data):
                key_str = key_data[offset:offset+str_len*2].decode('utf-16-le', errors='replace')
                keys.append(('utf16', key_str))
                offset += str_len*2
            else:
                break
        else:
            break
    else:
        # Unknown type, try to continue
        keys.append(('unknown', key_type))
        break  # Stop on unknown to avoid corruption

print(f'Parsed {len(keys)} keys')
print()
print('=== SAMPLE KEYS (First 20) ===')
for i, (ktype, kval) in enumerate(keys[:20]):
    if ktype == 'string':
        print(f'[{i}] {ktype}: {kval[:60]}...' if len(str(kval)) > 60 else f'[{i}] {ktype}: {kval}')
    else:
        print(f'[{i}] {ktype}: {kval}')

# Look for GUID-like strings
print()
print('=== POTENTIAL GUIDS IN KEYS ===')
for i, (ktype, kval) in enumerate(keys):
    if ktype == 'string' and len(kval) == 36 and kval.count('-') == 4:
        print(f'[{i}] {kval}')

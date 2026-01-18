import json
import base64
import os
import shutil

catalog_path = r"C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\StreamingAssets\aa\catalog.json"
backup_path = r"C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\StreamingAssets\aa\catalog_original_backup.json"

# Restore original
if os.path.exists(backup_path):
    shutil.copy(backup_path, catalog_path)
    print("Restored original catalog")

with open(catalog_path, 'r', encoding='utf-8') as f:
    catalog = json.load(f)

extra_data = base64.b64decode(catalog['m_ExtraDataString'])

# From error: Provided 3bdbe6ba = 1004267194 decimal
# Our bundle: calculated 2ff5dd23 = 804642083 decimal
old_crc = 1004267194
new_crc = 804642083

print(f"Searching for CRC: {old_crc}")

# Search in UTF-16LE format
old_crc_utf16 = str(old_crc).encode('utf-16-le')
new_crc_utf16 = str(new_crc).encode('utf-16-le')

print(f"Old bytes: {old_crc_utf16.hex()}")
print(f"New bytes: {new_crc_utf16.hex()}")

# They have different lengths (10 vs 9 digits)
# 1004267194 = 10 digits
# 804642083 = 9 digits
# Need to pad with space to keep same length

new_crc_padded = str(new_crc) + ' '  # Add space to match length
new_crc_utf16_padded = new_crc_padded.encode('utf-16-le')

print(f"Padded new bytes: {new_crc_utf16_padded.hex()}")
print(f"Lengths: old={len(old_crc_utf16)}, new={len(new_crc_utf16_padded)}")

if old_crc_utf16 in extra_data:
    print("Found old CRC in extra data!")
    idx = extra_data.find(old_crc_utf16)
    context = extra_data[max(0,idx-40):idx+len(old_crc_utf16)+20]
    print(f"Context: {context}")

    # Replace
    extra_data = extra_data.replace(old_crc_utf16, new_crc_utf16_padded, 1)
    print("Replaced CRC!")
else:
    print("Old CRC not found in UTF-16LE format")

    # Try ASCII
    old_crc_ascii = str(old_crc).encode('ascii')
    if old_crc_ascii in extra_data:
        print("Found in ASCII!")
    else:
        print("Not found in ASCII either")

        # Search byte by byte for the decimal value
        print(f"\nSearching for decimal {old_crc} in data...")
        extra_str = extra_data.decode('utf-16-le', errors='ignore')
        if str(old_crc) in extra_str:
            print(f"Found {old_crc} in decoded string!")
            # Do replacement on string level
            extra_str = extra_str.replace(str(old_crc), str(new_crc) + ' ', 1)
            extra_data = extra_str.encode('utf-16-le')
            print("Replaced in string!")
        else:
            print(f"{old_crc} not found anywhere")

# Deploy bundle
bundle_src = r"D:\Unity_Workshop\Endstar Custom Shader\Library\com.unity.addressables\aa\Windows\StandaloneWindows64\defaultlocalgroup_assets_all_89aecf0d389953239779b1e21b10bd51.bundle"
bundle_dst = r"C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\StreamingAssets\aa\StandaloneWindows64\felix_assets_all_8ce5cfff23e50dfcaa729aa03940bfd7.bundle"
shutil.copy(bundle_src, bundle_dst)
print("Deployed bundle")

# Save catalog
catalog['m_ExtraDataString'] = base64.b64encode(extra_data).decode('ascii')
with open(catalog_path, 'w', encoding='utf-8') as f:
    json.dump(catalog, f, separators=(',', ':'))

print("Saved catalog")
print("\nLaunch game to test!")

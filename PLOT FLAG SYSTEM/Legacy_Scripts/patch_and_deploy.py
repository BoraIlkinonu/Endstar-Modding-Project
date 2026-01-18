import json, base64, shutil, re, os, glob

# Paths
unity_project = r'D:\Unity_Workshop\Endstar Custom Shader'
game_path = r'C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\StreamingAssets\aa\StandaloneWindows64'
backup_folder = r'D:\Endstar Plot Flag\PLOT FLAG SYSTEM'

# Find built bundle
bundle_pattern = os.path.join(unity_project, 'Library', 'com.unity.addressables', 'aa', 'Windows', 'StandaloneWindows64', '*.bundle')
bundles = glob.glob(bundle_pattern)
if not bundles:
    print(f'ERROR: No bundle found at {bundle_pattern}')
    exit(1)
built_bundle = bundles[0]
print(f'Built bundle: {built_bundle}')

# Unity catalog
unity_catalog = os.path.join(unity_project, 'Library', 'com.unity.addressables', 'aa', 'Windows', 'catalog.json')
print(f'Unity catalog: {unity_catalog}')

# Get CRC from Unity catalog
with open(unity_catalog, 'r') as f:
    unity_cat = json.load(f)
extra = base64.b64decode(unity_cat['m_ExtraDataString'])
extra_str = extra.decode('utf-16-le', errors='ignore')
match = re.search(r'"m_Crc":(\d+)', extra_str)
if not match:
    print(f'ERROR: Could not find CRC in Unity catalog')
    print(f'Extra string sample: {extra_str[:500]}')
    exit(1)
new_crc = match.group(1)
print(f'Unity CRC: {new_crc}')

# Game catalog
game_catalog = os.path.join(game_path, 'catalog.json')
backup_catalog = os.path.join(backup_folder, 'catalog_original_backup.json')

# Check for backup or create one
if not os.path.exists(backup_catalog):
    if os.path.exists(game_catalog):
        shutil.copy(game_catalog, backup_catalog)
        print(f'Created backup at {backup_catalog}')
    else:
        print(f'ERROR: Game catalog not found at {game_catalog}')
        exit(1)

# Restore from backup
shutil.copy(backup_catalog, game_catalog)
print('Restored catalog from backup')

# Patch CRC
with open(game_catalog, 'r') as f:
    cat_json = json.load(f)
extra_data = base64.b64decode(cat_json['m_ExtraDataString'])

# Felix's original CRC
old_crc = '1004267194'
old_bytes = old_crc.encode('utf-16-le')
new_bytes = new_crc.encode('utf-16-le')

# Pad if new CRC is shorter
if len(new_bytes) < len(old_bytes):
    new_bytes = new_bytes + b' ' * (len(old_bytes) - len(new_bytes))

if old_bytes not in extra_data:
    print(f'ERROR: Original CRC {old_crc} not found in catalog')
    exit(1)

extra_data = extra_data.replace(old_bytes, new_bytes, 1)
cat_json['m_ExtraDataString'] = base64.b64encode(extra_data).decode('ascii')

with open(game_catalog, 'w') as f:
    json.dump(cat_json, f, separators=(',', ':'))
print('CRC patched in catalog')

# Copy bundle
dest_bundle = os.path.join(game_path, 'felix_assets_all_8ce5cfff23e50dfcaa729aa03940bfd7.bundle')
shutil.copy(built_bundle, dest_bundle)
print(f'Bundle copied to {dest_bundle}')

print('\nDone! Launch the game to test.')

"""
Endstar Custom Character Deployment Script
Deploys Unity Addressables bundle to game and patches CRC in catalog.

Usage: python deploy_character.py
"""

import json, base64, shutil, os, re, glob

# Configuration
UNITY_PROJECT = r'D:\Unity_Workshop\Endstar Custom Shader'
GAME_AA_PATH = r'C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\StreamingAssets\aa'
TARGET_BUNDLE = 'felix_assets_all_8ce5cfff23e50dfcaa729aa03940bfd7.bundle'
FELIX_ORIGINAL_CRC = '1004267194'

def deploy():
    print("=== Endstar Character Deployment ===\n")

    # Find built bundle
    bundle_pattern = os.path.join(UNITY_PROJECT, 'Library', 'com.unity.addressables',
                                   'aa', 'Windows', 'StandaloneWindows64', '*.bundle')
    bundles = glob.glob(bundle_pattern)
    if not bundles:
        print('ERROR: No bundle found. Build Addressables first in Unity.')
        return False
    built_bundle = bundles[0]
    print(f'Bundle: {os.path.basename(built_bundle)}')
    print(f'Size: {os.path.getsize(built_bundle):,} bytes')

    # Get CRC from Unity catalog
    unity_catalog = os.path.join(UNITY_PROJECT, 'Library', 'com.unity.addressables',
                                  'aa', 'Windows', 'catalog.json')
    with open(unity_catalog, 'r') as f:
        unity_cat = json.load(f)
    extra = base64.b64decode(unity_cat['m_ExtraDataString'])
    idx = extra.find('m_Crc'.encode('utf-16-le'))
    if idx == -1:
        print('ERROR: Could not find CRC in Unity catalog')
        return False
    context = extra[idx:idx+60].decode('utf-16-le', errors='ignore')
    match = re.search(r'm_Crc.:(\d+)', context)
    if not match:
        print('ERROR: Could not parse CRC from Unity catalog')
        return False
    new_crc = match.group(1)
    print(f'Unity CRC: {new_crc}')

    # Check for backup catalog
    backup_catalog = os.path.join(GAME_AA_PATH, 'catalog_original_backup.json')
    game_catalog = os.path.join(GAME_AA_PATH, 'catalog.json')

    if not os.path.exists(backup_catalog):
        print(f'\nCreating backup of original catalog...')
        shutil.copy(game_catalog, backup_catalog)

    # Restore and patch game catalog
    print('\nRestoring catalog from backup...')
    shutil.copy(backup_catalog, game_catalog)

    with open(game_catalog, 'r') as f:
        cat_json = json.load(f)
    extra_data = base64.b64decode(cat_json['m_ExtraDataString'])

    old_bytes = FELIX_ORIGINAL_CRC.encode('utf-16-le')
    new_bytes = new_crc.encode('utf-16-le')

    if old_bytes not in extra_data:
        print(f'ERROR: Felix CRC {FELIX_ORIGINAL_CRC} not found in catalog')
        return False

    extra_data = extra_data.replace(old_bytes, new_bytes, 1)
    cat_json['m_ExtraDataString'] = base64.b64encode(extra_data).decode('ascii')

    with open(game_catalog, 'w') as f:
        json.dump(cat_json, f, separators=(',', ':'))
    print('CRC patched in catalog')

    # Copy bundle
    dest = os.path.join(GAME_AA_PATH, 'StandaloneWindows64', TARGET_BUNDLE)
    shutil.copy(built_bundle, dest)
    print(f'Bundle copied to game')

    print('\n=== Deployment Complete ===')
    print('Launch game with: Endstar.exe (NOT via launcher)')
    print(f'Game path: C:\\Endless Studios\\Endless Launcher\\Endstar\\Endstar.exe')
    return True

if __name__ == '__main__':
    deploy()

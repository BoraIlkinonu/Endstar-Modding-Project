"""
Endstar Felix Bundle Deployment Script
======================================
Deploys a custom character bundle replacing Felix.

Usage:
    python deploy_felix_bundle.py

Prerequisites:
    1. Unity project built with Addressables
    2. URP settings configured for Deferred rendering
    3. Original catalog backup exists
"""

import json
import base64
import shutil
import os
from pathlib import Path

# Configuration
UNITY_PROJECT = Path(r'D:\Unity_Workshop\Endstar Custom Shader')
GAME_FOLDER = Path(r'C:\Endless Studios\Endless Launcher\Endstar')
BACKUP_FOLDER = Path(r'D:\Endstar Plot Flag\PLOT FLAG SYSTEM\ORIGINAL_GAME_BACKUPS')

# Felix bundle info
FELIX_HASH = '8ce5cfff23e50dfcaa729aa03940bfd7'
FELIX_ORIGINAL_CRC = '1004267194'
FELIX_BUNDLE_NAME = f'felix_assets_all_{FELIX_HASH}.bundle'

# Paths
UNITY_CATALOG = UNITY_PROJECT / 'Library' / 'com.unity.addressables' / 'aa' / 'Windows' / 'catalog.json'
UNITY_BUNDLES = UNITY_PROJECT / 'Library' / 'com.unity.addressables' / 'aa' / 'Windows' / 'StandaloneWindows64'
GAME_CATALOG = GAME_FOLDER / 'Endstar_Data' / 'StreamingAssets' / 'aa' / 'catalog.json'
GAME_BUNDLES = GAME_FOLDER / 'Endstar_Data' / 'StreamingAssets' / 'aa' / 'StandaloneWindows64'
ORIGINAL_CATALOG = BACKUP_FOLDER / 'catalog.json.ORIGINAL'


def extract_crc_from_catalog(catalog_path: Path) -> dict:
    """Extract all CRC values from a catalog's m_ExtraDataString."""
    import re

    with open(catalog_path, 'r') as f:
        catalog = json.load(f)

    extra_data = base64.b64decode(catalog['m_ExtraDataString'])
    results = {}

    # Try both UTF-16LE alignments
    for offset in [0, 1]:
        text = extra_data[offset:].decode('utf-16-le', errors='ignore')
        pattern = r'"m_Hash":"([a-f0-9]{32})"[^}]*"m_Crc":(\d+)'
        for hash_val, crc in re.findall(pattern, text):
            results[hash_val] = crc

    return results


def find_unity_bundle() -> Path:
    """Find the built bundle in Unity output folder."""
    if not UNITY_BUNDLES.exists():
        raise FileNotFoundError(f"Unity bundles folder not found: {UNITY_BUNDLES}")

    bundles = list(UNITY_BUNDLES.glob('*.bundle'))
    if not bundles:
        raise FileNotFoundError(f"No bundles found in {UNITY_BUNDLES}")

    # Return the first bundle (should only be one for single-prefab builds)
    return bundles[0]


def patch_catalog_crc(old_crc: str, new_crc: str):
    """Patch the game catalog with new CRC, starting from original backup."""
    print(f"Patching CRC: {old_crc} -> {new_crc}")

    # Read ORIGINAL backup
    with open(ORIGINAL_CATALOG, 'r') as f:
        catalog = json.load(f)

    # Decode m_ExtraDataString
    extra_data = base64.b64decode(catalog['m_ExtraDataString'])

    # Replace CRC (UTF-16LE encoded)
    old_bytes = old_crc.encode('utf-16-le')
    new_bytes = new_crc.encode('utf-16-le')

    pos = extra_data.find(old_bytes)
    if pos == -1:
        raise ValueError(f"Old CRC {old_crc} not found in catalog!")

    print(f"Found CRC at position {pos}")

    # Replace
    patched = extra_data[:pos] + new_bytes + extra_data[pos+len(old_bytes):]
    catalog['m_ExtraDataString'] = base64.b64encode(patched).decode('ascii')

    # Write to game folder
    with open(GAME_CATALOG, 'w') as f:
        json.dump(catalog, f, separators=(',', ':'))

    print(f"Catalog saved to {GAME_CATALOG}")


def verify_patch():
    """Verify the CRC was patched correctly."""
    with open(GAME_CATALOG, 'r') as f:
        catalog = json.load(f)

    extra_data = base64.b64decode(catalog['m_ExtraDataString'])

    # Find Felix hash
    felix_bytes = FELIX_HASH.encode('utf-16-le')
    pos = extra_data.find(felix_bytes)

    if pos > 0:
        context = extra_data[pos:pos+200].decode('utf-16-le', errors='ignore')
        print(f"Felix entry: {context[:150]}")
        return True
    else:
        print("ERROR: Felix hash not found in patched catalog!")
        return False


def main():
    print("=" * 60)
    print("Endstar Felix Bundle Deployment")
    print("=" * 60)

    # Step 1: Find Unity bundle
    print("\n[1/5] Finding Unity bundle...")
    try:
        unity_bundle = find_unity_bundle()
        print(f"Found: {unity_bundle.name}")
    except FileNotFoundError as e:
        print(f"ERROR: {e}")
        print("Have you built Addressables in Unity?")
        return False

    # Step 2: Extract new CRC
    print("\n[2/5] Extracting CRC from Unity catalog...")
    if not UNITY_CATALOG.exists():
        print(f"ERROR: Unity catalog not found: {UNITY_CATALOG}")
        return False

    crc_map = extract_crc_from_catalog(UNITY_CATALOG)
    if not crc_map:
        print("ERROR: No CRC entries found in Unity catalog!")
        return False

    # Get the first (and likely only) CRC
    new_hash, new_crc = list(crc_map.items())[0]
    print(f"New bundle - Hash: {new_hash}, CRC: {new_crc}")

    # Step 3: Copy and rename bundle
    print("\n[3/5] Copying bundle to game folder...")
    dest_bundle = GAME_BUNDLES / FELIX_BUNDLE_NAME
    shutil.copy2(unity_bundle, dest_bundle)
    print(f"Copied: {unity_bundle.name} -> {FELIX_BUNDLE_NAME}")
    print(f"Size: {dest_bundle.stat().st_size:,} bytes")

    # Step 4: Patch catalog
    print("\n[4/5] Patching game catalog...")
    if not ORIGINAL_CATALOG.exists():
        print(f"ERROR: Original catalog backup not found: {ORIGINAL_CATALOG}")
        return False

    patch_catalog_crc(FELIX_ORIGINAL_CRC, new_crc)

    # Step 5: Verify
    print("\n[5/5] Verifying patch...")
    if not verify_patch():
        return False

    print("\n" + "=" * 60)
    print("DEPLOYMENT COMPLETE")
    print("=" * 60)
    print(f"\nBundle: {FELIX_BUNDLE_NAME}")
    print(f"CRC: {FELIX_ORIGINAL_CRC} -> {new_crc}")
    print(f"\nLaunch game to test:")
    print(f'  start "" "{GAME_FOLDER / "Endstar.exe"}"')

    return True


if __name__ == '__main__':
    success = main()
    exit(0 if success else 1)

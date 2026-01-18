"""
Endstar Mod Patcher
====================
A simple mod installer for Endstar custom characters.

For Users:
  1. Place your character mod folders in the 'mods' directory
  2. Run this script to install/uninstall mods
  3. Launch Endstar directly (NOT via Endless Launcher)

For Mod Creators:
  Each mod folder should contain:
  - bundle.bundle     : The Unity Addressables bundle
  - manifest.json     : Mod metadata (name, original_bundle, original_crc, new_crc)

Usage:
  python EndstarModPatcher.py install
  python EndstarModPatcher.py uninstall
  python EndstarModPatcher.py launch
"""

import os
import sys
import json
import base64
import shutil
import subprocess
from pathlib import Path

# Default paths - users can override these
GAME_PATH = Path(r'C:\Endless Studios\Endless Launcher\Endstar')
GAME_AA_PATH = GAME_PATH / 'Endstar_Data' / 'StreamingAssets' / 'aa'
GAME_EXE = GAME_PATH / 'Endstar.exe'
MODS_PATH = Path(__file__).parent / 'mods'

def get_config():
    """Load or create config file."""
    config_path = Path(__file__).parent / 'config.json'
    default_config = {
        'game_path': str(GAME_PATH),
        'mods_path': str(MODS_PATH)
    }

    if config_path.exists():
        with open(config_path, 'r') as f:
            return json.load(f)
    else:
        with open(config_path, 'w') as f:
            json.dump(default_config, f, indent=2)
        return default_config

def create_backup(game_aa_path):
    """Create backup of original catalog if not exists."""
    catalog = game_aa_path / 'catalog.json'
    backup = game_aa_path / 'catalog_original_backup.json'

    if not backup.exists():
        if catalog.exists():
            print(f"Creating backup: {backup}")
            shutil.copy(catalog, backup)
            return True
        else:
            print("ERROR: catalog.json not found!")
            return False
    return True

def restore_backup(game_aa_path):
    """Restore original catalog from backup."""
    catalog = game_aa_path / 'catalog.json'
    backup = game_aa_path / 'catalog_original_backup.json'

    if backup.exists():
        print("Restoring original catalog...")
        shutil.copy(backup, catalog)
        return True
    else:
        print("WARNING: No backup found to restore")
        return False

def patch_catalog_crc(game_aa_path, old_crc, new_crc):
    """Patch CRC in game catalog using UTF-16LE encoding."""
    catalog_path = game_aa_path / 'catalog.json'

    with open(catalog_path, 'r') as f:
        cat_json = json.load(f)

    extra_data = base64.b64decode(cat_json['m_ExtraDataString'])

    old_bytes = old_crc.encode('utf-16-le')
    new_bytes = new_crc.encode('utf-16-le')

    if old_bytes not in extra_data:
        print(f"ERROR: CRC {old_crc} not found in catalog")
        return False

    extra_data = extra_data.replace(old_bytes, new_bytes, 1)
    cat_json['m_ExtraDataString'] = base64.b64encode(extra_data).decode('ascii')

    with open(catalog_path, 'w') as f:
        json.dump(cat_json, f, separators=(',', ':'))

    print(f"Patched CRC: {old_crc} -> {new_crc}")
    return True

def load_mods(mods_path):
    """Load all mods from mods directory."""
    mods = []
    mods_dir = Path(mods_path)

    if not mods_dir.exists():
        mods_dir.mkdir(parents=True)
        print(f"Created mods directory: {mods_dir}")
        return mods

    for mod_dir in mods_dir.iterdir():
        if mod_dir.is_dir():
            manifest_path = mod_dir / 'manifest.json'
            if manifest_path.exists():
                with open(manifest_path, 'r') as f:
                    manifest = json.load(f)
                manifest['path'] = mod_dir
                mods.append(manifest)
                print(f"Found mod: {manifest.get('name', mod_dir.name)}")

    return mods

def install_mod(mod, game_aa_path):
    """Install a single mod."""
    print(f"\nInstalling: {mod.get('name', 'Unknown')}")

    # Get mod files
    mod_path = mod['path']
    bundle_src = mod_path / 'bundle.bundle'

    if not bundle_src.exists():
        # Try to find any .bundle file
        bundles = list(mod_path.glob('*.bundle'))
        if bundles:
            bundle_src = bundles[0]
        else:
            print(f"ERROR: No bundle file found in {mod_path}")
            return False

    # Destination path
    target_bundle = mod.get('original_bundle', 'felix_assets_all_8ce5cfff23e50dfcaa729aa03940bfd7.bundle')
    bundle_dest = game_aa_path / 'StandaloneWindows64' / target_bundle

    # Backup original bundle if needed
    bundle_backup = game_aa_path / 'StandaloneWindows64' / f"{target_bundle}.original"
    if not bundle_backup.exists() and bundle_dest.exists():
        shutil.copy(bundle_dest, bundle_backup)
        print(f"Backed up original bundle")

    # Copy bundle
    shutil.copy(bundle_src, bundle_dest)
    print(f"Installed bundle: {bundle_src.name} -> {target_bundle}")

    # Patch CRC if provided
    if 'original_crc' in mod and 'new_crc' in mod:
        patch_catalog_crc(game_aa_path, mod['original_crc'], mod['new_crc'])

    return True

def install_all_mods():
    """Install all mods from mods directory."""
    config = get_config()
    game_aa_path = Path(config['game_path']) / 'Endstar_Data' / 'StreamingAssets' / 'aa'
    mods_path = Path(config['mods_path'])

    if not game_aa_path.exists():
        print(f"ERROR: Game not found at {game_aa_path}")
        return False

    mods = load_mods(mods_path)
    if not mods:
        print("\nNo mods found! Place mod folders in:", mods_path)
        print("\nEach mod folder should contain:")
        print("  - bundle.bundle or *.bundle file")
        print("  - manifest.json with mod info")
        return False

    # Create backup first
    if not create_backup(game_aa_path):
        return False

    # Restore from backup to start fresh
    restore_backup(game_aa_path)

    # Install each mod
    success = True
    for mod in mods:
        if not install_mod(mod, game_aa_path):
            success = False

    if success:
        print("\n=== All mods installed successfully ===")
        print("Launch game directly with: Endstar.exe")
        print("(NOT via Endless Launcher)")

    return success

def uninstall_all_mods():
    """Restore original game files."""
    config = get_config()
    game_aa_path = Path(config['game_path']) / 'Endstar_Data' / 'StreamingAssets' / 'aa'

    # Restore catalog
    restore_backup(game_aa_path)

    # Restore original bundles
    bundles_path = game_aa_path / 'StandaloneWindows64'
    for backup in bundles_path.glob('*.original'):
        original_name = backup.stem  # Remove .original
        original_path = bundles_path / original_name
        print(f"Restoring: {original_name}")
        shutil.copy(backup, original_path)

    print("\n=== Mods uninstalled, original files restored ===")
    return True

def launch_game():
    """Launch Endstar directly."""
    config = get_config()
    game_exe = Path(config['game_path']) / 'Endstar.exe'

    if not game_exe.exists():
        print(f"ERROR: Game not found at {game_exe}")
        return False

    print(f"Launching: {game_exe}")
    subprocess.Popen([str(game_exe)], cwd=str(game_exe.parent))
    return True

def create_mod_template():
    """Create a template mod folder for creators."""
    config = get_config()
    mods_path = Path(config['mods_path'])
    template_path = mods_path / '_template'

    template_path.mkdir(parents=True, exist_ok=True)

    manifest = {
        "name": "My Custom Character",
        "author": "Your Name",
        "version": "1.0.0",
        "description": "Description of your character mod",
        "original_bundle": "felix_assets_all_8ce5cfff23e50dfcaa729aa03940bfd7.bundle",
        "original_crc": "1004267194",
        "new_crc": "YOUR_BUNDLE_CRC_HERE"
    }

    with open(template_path / 'manifest.json', 'w') as f:
        json.dump(manifest, f, indent=2)

    readme = """# Mod Template

## How to Create a Character Mod

1. Build your character in Unity 2022.3.62f2 with Addressables
2. Copy the built .bundle file here and rename to bundle.bundle
3. Edit manifest.json:
   - Update 'name', 'author', 'description'
   - Set 'new_crc' to the CRC from your Unity catalog.json

## Getting the CRC
After building Addressables in Unity, find:
  Library/com.unity.addressables/aa/Windows/catalog.json

Decode m_ExtraDataString (base64) and find m_Crc value.

## Files Required
- bundle.bundle : Your built Unity bundle
- manifest.json : This metadata file
"""

    with open(template_path / 'README.txt', 'w') as f:
        f.write(readme)

    print(f"Created mod template at: {template_path}")

def print_help():
    """Print usage help."""
    print("""
Endstar Mod Patcher
===================

Commands:
  install     - Install all mods from mods folder
  uninstall   - Restore original game files
  launch      - Launch Endstar directly
  template    - Create a mod template folder
  help        - Show this help

Examples:
  python EndstarModPatcher.py install
  python EndstarModPatcher.py install launch
  python EndstarModPatcher.py uninstall

For mod creators, run 'template' to create a mod folder structure.
""")

def main():
    print("=== Endstar Mod Patcher ===\n")

    args = sys.argv[1:] if len(sys.argv) > 1 else ['help']

    for arg in args:
        if arg == 'install':
            install_all_mods()
        elif arg == 'uninstall':
            uninstall_all_mods()
        elif arg == 'launch':
            launch_game()
        elif arg == 'template':
            create_mod_template()
        elif arg == 'help':
            print_help()
        else:
            print(f"Unknown command: {arg}")
            print_help()

if __name__ == '__main__':
    main()

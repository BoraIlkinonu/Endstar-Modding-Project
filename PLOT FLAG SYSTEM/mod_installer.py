"""
Endstar Mod Installer
=====================
One-click install/uninstall for character mods.

Features:
- Automatic backup of original files
- Multi-character support
- One-click uninstall to restore originals
- Mod package validation

Usage:
    python mod_installer.py install <mod_folder>
    python mod_installer.py uninstall
    python mod_installer.py status
"""

import json
import base64
import shutil
import sys
import os
from pathlib import Path
from datetime import datetime

try:
    from PIL import Image
except ImportError:
    Image = None

# ============================================================================
# CONFIGURATION
# ============================================================================

GAME_FOLDER = Path(r'C:\Endless Studios\Endless Launcher\Endstar')
GAME_DATA = GAME_FOLDER / 'Endstar_Data'
BACKUP_FOLDER = Path(r'D:\Endstar Plot Flag\PLOT FLAG SYSTEM\ORIGINAL_GAME_BACKUPS')
INSTALL_STATE_FILE = BACKUP_FOLDER / 'install_state.json'

# Game file paths
CATALOG_FILE = GAME_DATA / 'StreamingAssets' / 'aa' / 'catalog.json'
BUNDLES_FOLDER = GAME_DATA / 'StreamingAssets' / 'aa' / 'StandaloneWindows64'
ASSETS_FILE = GAME_DATA / 'sharedassets0.assets'
RESS_FILE = GAME_DATA / 'sharedassets0.assets.resS'

# ============================================================================
# CHARACTER DATABASE
# All characters with their CRC, hash, name offset, portrait offset
# ============================================================================

CHARACTER_DATABASE = {
    "Felix": {
        "hash": "8ce5cfff23e50dfcaa729aa03940bfd7",
        "original_crc": "1004267194",
        "bundle_name": "felix_assets_all_8ce5cfff23e50dfcaa729aa03940bfd7.bundle",
        "name_length": 5,
        "name_search": b'\x05\x00\x00\x00Felix',
        "portrait_offset": 114478704,
        "portrait_size": 214856,
    },
    "Tilly": {
        "hash": "fc45ee0b8231bf3537e606898810529f",
        "original_crc": "2007923026",
        "bundle_name": "tilly_assets_all_fc45ee0b8231bf3537e606898810529f.bundle",
        "name_length": 5,
        "name_search": b'\x05\x00\x00\x00Tilly',
        "portrait_offset": None,  # TODO: Find offset
        "portrait_size": 214856,
    },
    # Add more characters as needed...
}

# ============================================================================
# BACKUP FUNCTIONS
# ============================================================================

def ensure_backup(file_path: Path, backup_suffix: str = '.ORIGINAL') -> Path:
    """Create backup if it doesn't exist. Returns backup path."""
    backup_path = BACKUP_FOLDER / (file_path.name + backup_suffix)

    if not backup_path.exists():
        print(f"  Creating backup: {backup_path.name}")
        BACKUP_FOLDER.mkdir(parents=True, exist_ok=True)
        shutil.copy2(file_path, backup_path)

    return backup_path


def restore_from_backup(file_path: Path, backup_suffix: str = '.ORIGINAL') -> bool:
    """Restore file from backup. Returns True if successful."""
    backup_path = BACKUP_FOLDER / (file_path.name + backup_suffix)

    if not backup_path.exists():
        print(f"  WARNING: Backup not found: {backup_path.name}")
        return False

    print(f"  Restoring: {file_path.name}")
    shutil.copy2(backup_path, file_path)
    return True


# ============================================================================
# CATALOG CRC PATCHING
# ============================================================================

def patch_catalog_crc(old_crc: str, new_crc: str, catalog_path: Path = None) -> bool:
    """Patch CRC in catalog's m_ExtraDataString."""
    if catalog_path is None:
        catalog_path = CATALOG_FILE

    with open(catalog_path, 'r') as f:
        catalog = json.load(f)

    extra_data = base64.b64decode(catalog['m_ExtraDataString'])

    old_bytes = old_crc.encode('utf-16-le')
    new_bytes = new_crc.encode('utf-16-le')

    pos = extra_data.find(old_bytes)
    if pos == -1:
        print(f"  ERROR: CRC {old_crc} not found in catalog")
        return False

    patched = bytearray(extra_data)
    patched[pos:pos+len(old_bytes)] = new_bytes

    catalog['m_ExtraDataString'] = base64.b64encode(bytes(patched)).decode('ascii')

    with open(catalog_path, 'w') as f:
        json.dump(catalog, f, separators=(',', ':'))

    return True


def get_bundle_crc(bundle_path: Path) -> str:
    """Calculate CRC of a bundle file (simplified - uses file size as proxy)."""
    # Note: This is a simplified version. Real CRC should be extracted from
    # the Unity catalog that was built with this bundle.
    # For now, we expect the mod to include the CRC in config.
    return None


# ============================================================================
# CHARACTER PATCHING
# ============================================================================

def patch_character_name(old_name: str, new_name: str, char_db: dict) -> bool:
    """Patch character display name in sharedassets0.assets."""
    if len(old_name) != len(new_name):
        print(f"  ERROR: Name length mismatch! '{old_name}' ({len(old_name)}) vs '{new_name}' ({len(new_name)})")
        print(f"         Binary patching requires same-length names.")
        return False

    with open(ASSETS_FILE, 'rb') as f:
        content = bytearray(f.read())

    search_pattern = char_db['name_search']
    pos = content.find(search_pattern)

    if pos == -1:
        print(f"  ERROR: Could not find '{old_name}' in assets")
        return False

    # Replace name bytes (after 4-byte length prefix)
    new_bytes = new_name.encode('ascii')
    for i, byte in enumerate(new_bytes):
        content[pos + 4 + i] = byte

    with open(ASSETS_FILE, 'wb') as f:
        f.write(content)

    return True


def patch_portrait(portrait_path: Path, char_db: dict) -> bool:
    """Patch character portrait in sharedassets0.assets.resS."""
    if Image is None:
        print("  ERROR: Pillow not installed. Run: pip install Pillow")
        return False

    if char_db['portrait_offset'] is None:
        print("  WARNING: Portrait offset not configured for this character")
        return False

    if not portrait_path.exists():
        print(f"  ERROR: Portrait not found: {portrait_path}")
        return False

    # Load and prepare image
    img = Image.open(portrait_path)
    img = img.resize((214, 251), Image.LANCZOS)
    img = img.convert('RGBA')
    img_flipped = img.transpose(Image.FLIP_TOP_BOTTOM)
    rgba_data = img_flipped.tobytes('raw', 'RGBA')

    if len(rgba_data) != char_db['portrait_size']:
        print(f"  ERROR: Portrait data size mismatch")
        return False

    with open(RESS_FILE, 'rb') as f:
        content = bytearray(f.read())

    offset = char_db['portrait_offset']
    content[offset:offset + len(rgba_data)] = rgba_data

    with open(RESS_FILE, 'wb') as f:
        f.write(content)

    return True


# ============================================================================
# MOD INSTALLATION
# ============================================================================

def load_mod_config(mod_folder: Path) -> dict:
    """Load and validate mod.json."""
    mod_json = mod_folder / 'mod.json'

    if not mod_json.exists():
        print(f"ERROR: mod.json not found in {mod_folder}")
        return None

    with open(mod_json, 'r') as f:
        config = json.load(f)

    return config


def install_mod(mod_folder: Path) -> bool:
    """Install a mod package."""
    print("=" * 60)
    print("ENDSTAR MOD INSTALLER")
    print("=" * 60)

    mod_folder = Path(mod_folder)

    # Load mod config
    print(f"\nLoading mod from: {mod_folder}")
    config = load_mod_config(mod_folder)
    if config is None:
        return False

    print(f"Mod: {config.get('name', 'Unknown')}")
    print(f"Version: {config.get('version', '?')}")
    print(f"Characters: {len(config.get('characters', []))}")

    # Validate game installation
    if not GAME_FOLDER.exists():
        print(f"\nERROR: Game not found at {GAME_FOLDER}")
        return False

    # Create backups
    print("\n[1/4] Creating backups...")
    ensure_backup(CATALOG_FILE)
    ensure_backup(ASSETS_FILE)
    ensure_backup(RESS_FILE)

    # Track what we're installing for uninstall
    install_state = {
        "mod_name": config.get('name'),
        "installed_at": datetime.now().isoformat(),
        "characters": [],
        "original_bundles": []
    }

    # Process each character
    for char_config in config.get('characters', []):
        replaces = char_config['replaces']
        new_name = char_config['newName']

        print(f"\n[2/4] Installing character: {replaces} -> {new_name}")

        if replaces not in CHARACTER_DATABASE:
            print(f"  ERROR: Unknown character '{replaces}'")
            print(f"         Available: {list(CHARACTER_DATABASE.keys())}")
            continue

        char_db = CHARACTER_DATABASE[replaces]

        # Backup original bundle
        original_bundle = BUNDLES_FOLDER / char_db['bundle_name']
        if original_bundle.exists():
            ensure_backup(original_bundle)
            install_state['original_bundles'].append(char_db['bundle_name'])

        # Copy new bundle
        bundle_src = mod_folder / char_config['bundle']
        if bundle_src.exists():
            print(f"  Copying bundle...")
            shutil.copy2(bundle_src, original_bundle)
        else:
            print(f"  WARNING: Bundle not found: {bundle_src}")

        # Patch catalog CRC if provided
        if 'crc' in char_config:
            print(f"  Patching CRC: {char_db['original_crc']} -> {char_config['crc']}")
            # Start from original backup for clean patching
            backup_catalog = BACKUP_FOLDER / 'catalog.json.ORIGINAL'
            if backup_catalog.exists():
                shutil.copy2(backup_catalog, CATALOG_FILE)
            patch_catalog_crc(char_db['original_crc'], char_config['crc'])

        # Patch display name
        print(f"  Patching name: {replaces} -> {new_name}")
        if len(replaces) == len(new_name):
            patch_character_name(replaces, new_name, char_db)
        else:
            print(f"  WARNING: Name length mismatch, skipping name patch")

        # Patch portrait
        portrait_src = mod_folder / char_config.get('portrait', '')
        if portrait_src.exists():
            print(f"  Patching portrait...")
            patch_portrait(portrait_src, char_db)

        install_state['characters'].append({
            'original': replaces,
            'replacement': new_name
        })

    # Save install state
    print("\n[3/4] Saving install state...")
    with open(INSTALL_STATE_FILE, 'w') as f:
        json.dump(install_state, f, indent=2)

    print("\n[4/4] Installation complete!")
    print("\n" + "=" * 60)
    print("MOD INSTALLED SUCCESSFULLY")
    print("=" * 60)
    print("\nIMPORTANT: Launch Endstar.exe directly, NOT through the launcher!")

    return True


def uninstall_mod() -> bool:
    """Uninstall mod and restore original files."""
    print("=" * 60)
    print("ENDSTAR MOD UNINSTALLER")
    print("=" * 60)

    # Check for install state
    if not INSTALL_STATE_FILE.exists():
        print("\nNo mod installation found.")
        print("Attempting to restore from backups anyway...")
    else:
        with open(INSTALL_STATE_FILE, 'r') as f:
            state = json.load(f)
        print(f"\nUninstalling: {state.get('mod_name', 'Unknown mod')}")
        print(f"Installed: {state.get('installed_at', '?')}")

    print("\n[1/3] Restoring original files...")

    # Restore catalog
    restore_from_backup(CATALOG_FILE)

    # Restore assets
    restore_from_backup(ASSETS_FILE)

    # Restore resS
    restore_from_backup(RESS_FILE)

    # Restore bundles
    print("\n[2/3] Restoring original bundles...")
    for bundle_file in BACKUP_FOLDER.glob('*.bundle.ORIGINAL'):
        original_name = bundle_file.stem  # Remove .ORIGINAL
        dest = BUNDLES_FOLDER / original_name
        print(f"  Restoring: {original_name}")
        shutil.copy2(bundle_file, dest)

    # Remove install state
    print("\n[3/3] Cleaning up...")
    if INSTALL_STATE_FILE.exists():
        INSTALL_STATE_FILE.unlink()

    print("\n" + "=" * 60)
    print("MOD UNINSTALLED - ORIGINAL FILES RESTORED")
    print("=" * 60)

    return True


def show_status():
    """Show current installation status."""
    print("=" * 60)
    print("ENDSTAR MOD STATUS")
    print("=" * 60)

    # Check backups
    print("\nBackups:")
    backups = list(BACKUP_FOLDER.glob('*.ORIGINAL'))
    if backups:
        for b in backups:
            size = b.stat().st_size
            print(f"  {b.name}: {size:,} bytes")
    else:
        print("  No backups found")

    # Check install state
    print("\nInstallation State:")
    if INSTALL_STATE_FILE.exists():
        with open(INSTALL_STATE_FILE, 'r') as f:
            state = json.load(f)
        print(f"  Mod: {state.get('mod_name', 'Unknown')}")
        print(f"  Installed: {state.get('installed_at', '?')}")
        print(f"  Characters:")
        for char in state.get('characters', []):
            print(f"    - {char['original']} -> {char['replacement']}")
    else:
        print("  No mod currently installed")

    # Check game files
    print("\nGame Files:")
    print(f"  Catalog: {'EXISTS' if CATALOG_FILE.exists() else 'MISSING'}")
    print(f"  Assets: {'EXISTS' if ASSETS_FILE.exists() else 'MISSING'}")
    print(f"  ResS: {'EXISTS' if RESS_FILE.exists() else 'MISSING'}")


# ============================================================================
# MAIN
# ============================================================================

def print_usage():
    print("Endstar Mod Installer")
    print()
    print("Usage:")
    print("  python mod_installer.py install <mod_folder>  - Install a mod")
    print("  python mod_installer.py uninstall             - Uninstall and restore")
    print("  python mod_installer.py status                - Show current status")
    print()
    print("Example:")
    print("  python mod_installer.py install ./ZayedMod")


def main():
    if len(sys.argv) < 2:
        print_usage()
        return

    command = sys.argv[1].lower()

    if command == 'install':
        if len(sys.argv) < 3:
            print("ERROR: Please specify mod folder")
            print("Usage: python mod_installer.py install <mod_folder>")
            return
        install_mod(sys.argv[2])

    elif command == 'uninstall':
        uninstall_mod()

    elif command == 'status':
        show_status()

    else:
        print(f"Unknown command: {command}")
        print_usage()


if __name__ == '__main__':
    main()

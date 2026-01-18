"""
Endstar Mod Installer
=====================
Complete automated installer for Endstar character mods.
No Python knowledge required - just double-click!

Features:
- Auto-detects game installation
- Backs up original files
- Installs all mods from 'mods' folder
- Patches game catalog with correct CRCs
- Creates desktop shortcut to launch modded game
- Restore original files with one click
"""

import os
import sys
import json
import base64
import shutil
import ctypes
import subprocess
from pathlib import Path
import winreg

# ============================================================
# CONFIGURATION
# ============================================================

DEFAULT_GAME_PATHS = [
    Path(r'C:\Endless Studios\Endless Launcher\Endstar'),
    Path(r'C:\Program Files\Endless Studios\Endless Launcher\Endstar'),
    Path(r'C:\Program Files (x86)\Endless Studios\Endless Launcher\Endstar'),
]

SCRIPT_DIR = Path(__file__).parent
MODS_DIR = SCRIPT_DIR / 'mods'
CONFIG_FILE = SCRIPT_DIR / 'installer_config.json'

# ============================================================
# UTILITY FUNCTIONS
# ============================================================

def is_admin():
    """Check if running with admin privileges."""
    try:
        return ctypes.windll.shell32.IsUserAnAdmin()
    except:
        return False

def find_game_path():
    """Auto-detect Endstar installation."""
    # Check default paths
    for path in DEFAULT_GAME_PATHS:
        if (path / 'Endstar.exe').exists():
            return path

    # Try to find via registry (Steam)
    try:
        with winreg.OpenKey(winreg.HKEY_LOCAL_MACHINE,
                           r"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall") as key:
            for i in range(1000):
                try:
                    subkey_name = winreg.EnumKey(key, i)
                    with winreg.OpenKey(key, subkey_name) as subkey:
                        name, _ = winreg.QueryValueEx(subkey, "DisplayName")
                        if "Endstar" in name:
                            path, _ = winreg.QueryValueEx(subkey, "InstallLocation")
                            if Path(path).exists():
                                return Path(path)
                except:
                    continue
    except:
        pass

    return None

def load_config():
    """Load or create configuration."""
    if CONFIG_FILE.exists():
        with open(CONFIG_FILE, 'r') as f:
            return json.load(f)
    return {}

def save_config(config):
    """Save configuration."""
    with open(CONFIG_FILE, 'w') as f:
        json.dump(config, f, indent=2)

def create_backup(game_path):
    """Create backups of original game files."""
    aa_path = game_path / 'Endstar_Data' / 'StreamingAssets' / 'aa'

    # Backup catalog
    catalog = aa_path / 'catalog.json'
    backup_catalog = aa_path / 'catalog_original_backup.json'
    if catalog.exists() and not backup_catalog.exists():
        shutil.copy(catalog, backup_catalog)
        print(f"  [OK] Backed up catalog.json")

    # Backup Felix bundle
    bundles_path = aa_path / 'StandaloneWindows64'
    for bundle_file in bundles_path.glob('*.bundle'):
        backup_file = bundles_path / f"{bundle_file.name}.original"
        if not backup_file.exists():
            shutil.copy(bundle_file, backup_file)

    return True

def restore_originals(game_path):
    """Restore all original game files."""
    aa_path = game_path / 'Endstar_Data' / 'StreamingAssets' / 'aa'

    # Restore catalog
    backup_catalog = aa_path / 'catalog_original_backup.json'
    catalog = aa_path / 'catalog.json'
    if backup_catalog.exists():
        shutil.copy(backup_catalog, catalog)
        print(f"  [OK] Restored catalog.json")

    # Restore bundles
    bundles_path = aa_path / 'StandaloneWindows64'
    for backup_file in bundles_path.glob('*.original'):
        original_name = backup_file.stem  # Remove .original
        original_path = bundles_path / original_name
        shutil.copy(backup_file, original_path)
        print(f"  [OK] Restored {original_name}")

    return True

def patch_catalog_crc(game_path, old_crc, new_crc):
    """Patch CRC in game catalog using UTF-16LE encoding."""
    aa_path = game_path / 'Endstar_Data' / 'StreamingAssets' / 'aa'
    catalog_path = aa_path / 'catalog.json'

    with open(catalog_path, 'r') as f:
        cat_json = json.load(f)

    extra_data = base64.b64decode(cat_json['m_ExtraDataString'])
    old_bytes = old_crc.encode('utf-16-le')
    new_bytes = new_crc.encode('utf-16-le')

    if old_bytes not in extra_data:
        return False

    extra_data = extra_data.replace(old_bytes, new_bytes, 1)
    cat_json['m_ExtraDataString'] = base64.b64encode(extra_data).decode('ascii')

    with open(catalog_path, 'w') as f:
        json.dump(cat_json, f, separators=(',', ':'))

    return True

def install_mod(mod_path, game_path):
    """Install a single mod."""
    manifest_path = mod_path / 'manifest.json'
    if not manifest_path.exists():
        print(f"  [SKIP] No manifest.json in {mod_path.name}")
        return False

    with open(manifest_path, 'r') as f:
        manifest = json.load(f)

    mod_name = manifest.get('name', mod_path.name)
    print(f"\n  Installing: {mod_name}")

    # Find bundle file
    bundle_file = mod_path / 'bundle.bundle'
    if not bundle_file.exists():
        bundles = list(mod_path.glob('*.bundle'))
        if bundles:
            bundle_file = bundles[0]
        else:
            print(f"    [ERROR] No bundle file found!")
            return False

    # Copy bundle
    aa_path = game_path / 'Endstar_Data' / 'StreamingAssets' / 'aa'
    target_bundle = manifest.get('original_bundle', 'felix_assets_all_8ce5cfff23e50dfcaa729aa03940bfd7.bundle')
    dest_path = aa_path / 'StandaloneWindows64' / target_bundle

    shutil.copy(bundle_file, dest_path)
    print(f"    [OK] Installed bundle")

    # Patch CRC
    if 'original_crc' in manifest and 'new_crc' in manifest:
        if patch_catalog_crc(game_path, manifest['original_crc'], manifest['new_crc']):
            print(f"    [OK] Patched CRC: {manifest['original_crc']} -> {manifest['new_crc']}")
        else:
            print(f"    [WARN] Could not patch CRC")

    return True

def create_launcher(game_path, output_path):
    """Create a launcher batch file."""
    content = f'''@echo off
title Endstar (Modded)
echo Launching Endstar with mods...
start "" "{game_path / 'Endstar.exe'}"
'''
    launcher_path = output_path / 'LaunchModdedEndstar.bat'
    with open(launcher_path, 'w') as f:
        f.write(content)
    return launcher_path

def create_desktop_shortcut(game_path):
    """Create desktop shortcut for modded game."""
    try:
        import winshell
        from win32com.client import Dispatch

        desktop = Path(winshell.desktop())
        shortcut_path = desktop / 'Endstar (Modded).lnk'

        shell = Dispatch('WScript.Shell')
        shortcut = shell.CreateShortCut(str(shortcut_path))
        shortcut.Targetpath = str(game_path / 'Endstar.exe')
        shortcut.WorkingDirectory = str(game_path)
        shortcut.IconLocation = str(game_path / 'Endstar.exe')
        shortcut.Description = 'Launch Endstar with custom character mods'
        shortcut.save()

        return shortcut_path
    except ImportError:
        return None

def clear_screen():
    os.system('cls' if os.name == 'nt' else 'clear')

def print_header():
    print("=" * 60)
    print("           ENDSTAR MOD INSTALLER")
    print("=" * 60)
    print()

def print_menu():
    print("\nOptions:")
    print("  [1] Install Mods")
    print("  [2] Uninstall Mods (Restore Original)")
    print("  [3] Launch Game (Modded)")
    print("  [4] Change Game Path")
    print("  [5] Exit")
    print()

# ============================================================
# MAIN INSTALLER
# ============================================================

def main():
    clear_screen()
    print_header()

    # Load config or detect game
    config = load_config()
    game_path = None

    if 'game_path' in config:
        game_path = Path(config['game_path'])
        if not (game_path / 'Endstar.exe').exists():
            game_path = None

    if not game_path:
        print("Detecting Endstar installation...")
        game_path = find_game_path()

    if not game_path:
        print("\n[!] Could not find Endstar installation!")
        print("Please enter the path to Endstar folder:")
        user_path = input("> ").strip().strip('"')
        game_path = Path(user_path)

        if not (game_path / 'Endstar.exe').exists():
            print("\n[ERROR] Invalid path! Endstar.exe not found.")
            input("Press Enter to exit...")
            return

    # Save config
    config['game_path'] = str(game_path)
    save_config(config)

    print(f"\nGame found: {game_path}")

    # Check mods directory
    if not MODS_DIR.exists():
        MODS_DIR.mkdir(parents=True)
        print(f"\n[!] Created mods folder: {MODS_DIR}")
        print("    Place your mod folders here and run again.")

    mods = [d for d in MODS_DIR.iterdir() if d.is_dir() and (d / 'manifest.json').exists()]
    print(f"\nMods found: {len(mods)}")
    for mod in mods:
        with open(mod / 'manifest.json') as f:
            manifest = json.load(f)
        print(f"  - {manifest.get('name', mod.name)}")

    while True:
        print_menu()
        choice = input("Select option: ").strip()

        if choice == '1':
            # INSTALL MODS
            print("\n" + "=" * 40)
            print("INSTALLING MODS")
            print("=" * 40)

            if not mods:
                print("\n[!] No mods found in mods folder!")
                continue

            # Create backups
            print("\nCreating backups...")
            create_backup(game_path)

            # Restore from backup first to start fresh
            print("\nRestoring original files...")
            restore_originals(game_path)

            # Install each mod
            success_count = 0
            for mod in mods:
                if install_mod(mod, game_path):
                    success_count += 1

            # Create launcher
            launcher = create_launcher(game_path, SCRIPT_DIR)

            print("\n" + "=" * 40)
            print(f"INSTALLATION COMPLETE!")
            print(f"  Mods installed: {success_count}/{len(mods)}")
            print("=" * 40)
            print("\nIMPORTANT: Launch game with LaunchModdedEndstar.bat")
            print("           or run Endstar.exe directly!")
            print("           (NOT via Endless Launcher)")

        elif choice == '2':
            # UNINSTALL MODS
            print("\n" + "=" * 40)
            print("UNINSTALLING MODS")
            print("=" * 40)

            confirm = input("\nRestore original game files? (y/n): ").strip().lower()
            if confirm == 'y':
                restore_originals(game_path)
                print("\n[OK] Original files restored!")

        elif choice == '3':
            # LAUNCH GAME
            print("\nLaunching Endstar...")
            exe_path = game_path / 'Endstar.exe'
            subprocess.Popen([str(exe_path)], cwd=str(game_path))
            print("[OK] Game launched!")

        elif choice == '4':
            # CHANGE GAME PATH
            print("\nCurrent path:", game_path)
            new_path = input("Enter new path: ").strip().strip('"')
            if new_path:
                new_game_path = Path(new_path)
                if (new_game_path / 'Endstar.exe').exists():
                    game_path = new_game_path
                    config['game_path'] = str(game_path)
                    save_config(config)
                    print("[OK] Path updated!")
                else:
                    print("[ERROR] Endstar.exe not found at that path!")

        elif choice == '5':
            print("\nGoodbye!")
            break

        else:
            print("\n[!] Invalid option. Please enter 1-5.")

if __name__ == '__main__':
    try:
        main()
    except KeyboardInterrupt:
        print("\n\nInstaller cancelled.")
    except Exception as e:
        print(f"\n[ERROR] {e}")
        import traceback
        traceback.print_exc()

    input("\nPress Enter to exit...")

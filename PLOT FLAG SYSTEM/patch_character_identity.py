"""
Endstar Character Identity Patcher
==================================
Changes character name and portrait for Felix -> Zayed

Files modified:
  - sharedassets0.assets (display name)
  - sharedassets0.assets.resS (portrait texture)
"""

import os
import shutil
from pathlib import Path

try:
    from PIL import Image
except ImportError:
    print("ERROR: PIL/Pillow required. Install with: pip install Pillow")
    exit(1)

# Configuration
GAME_DATA = Path(r'C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data')
PORTRAIT_SOURCE = Path(r'D:\Endstar Plot Flag\PLOT FLAG SYSTEM\Zayed_Character_Mod\Portraits\zayed_portrait_214x251.png')
BACKUP_FOLDER = Path(r'D:\Endstar Plot Flag\PLOT FLAG SYSTEM\ORIGINAL_GAME_BACKUPS')

# File paths
ASSETS_FILE = GAME_DATA / 'sharedassets0.assets'
RESS_FILE = GAME_DATA / 'sharedassets0.assets.resS'

# Portrait texture location
PORTRAIT_OFFSET = 114478704
PORTRAIT_SIZE = 214856  # 214 * 251 * 4 bytes
PORTRAIT_WIDTH = 214
PORTRAIT_HEIGHT = 251

# Name configuration
OLD_NAME = b'Felix'
NEW_NAME = b'Zayed'


def backup_file(file_path: Path, backup_suffix: str = '.ORIGINAL'):
    """Create backup if it doesn't exist."""
    backup_path = BACKUP_FOLDER / (file_path.name + backup_suffix)
    if not backup_path.exists():
        print(f"Creating backup: {backup_path}")
        shutil.copy2(file_path, backup_path)
    else:
        print(f"Backup already exists: {backup_path}")
    return backup_path


def patch_display_name():
    """Change display name from Felix to Zayed in sharedassets0.assets."""
    print("\n=== Patching Display Name ===")

    if len(OLD_NAME) != len(NEW_NAME):
        print(f"ERROR: Name lengths must match! '{OLD_NAME.decode()}' ({len(OLD_NAME)}) vs '{NEW_NAME.decode()}' ({len(NEW_NAME)})")
        return False

    # Backup
    backup_file(ASSETS_FILE)

    # Read file
    with open(ASSETS_FILE, 'rb') as f:
        content = bytearray(f.read())

    # Find length-prefixed string pattern
    # Format: [4-byte length][string bytes]
    length_prefix = len(OLD_NAME).to_bytes(4, 'little')
    search_pattern = length_prefix + OLD_NAME

    pos = content.find(search_pattern)
    if pos == -1:
        print(f"ERROR: Could not find '{OLD_NAME.decode()}' in assets file!")
        return False

    print(f"Found '{OLD_NAME.decode()}' at offset {pos}")

    # Replace name bytes (after the 4-byte length prefix)
    for i, byte in enumerate(NEW_NAME):
        content[pos + 4 + i] = byte

    # Verify
    verify_pattern = length_prefix + NEW_NAME
    if content[pos:pos+len(verify_pattern)] == verify_pattern:
        print(f"Replaced with '{NEW_NAME.decode()}'")
    else:
        print("ERROR: Verification failed!")
        return False

    # Write
    with open(ASSETS_FILE, 'wb') as f:
        f.write(content)

    print(f"Saved: {ASSETS_FILE}")
    return True


def patch_portrait():
    """Replace portrait texture in sharedassets0.assets.resS."""
    print("\n=== Patching Portrait ===")

    if not PORTRAIT_SOURCE.exists():
        print(f"ERROR: Portrait image not found: {PORTRAIT_SOURCE}")
        return False

    # Backup
    backup_file(RESS_FILE)

    # Load and prepare image
    print(f"Loading: {PORTRAIT_SOURCE}")
    img = Image.open(PORTRAIT_SOURCE)

    # Resize if needed
    if img.size != (PORTRAIT_WIDTH, PORTRAIT_HEIGHT):
        print(f"Resizing from {img.size} to ({PORTRAIT_WIDTH}, {PORTRAIT_HEIGHT})")
        img = img.resize((PORTRAIT_WIDTH, PORTRAIT_HEIGHT), Image.LANCZOS)

    # Convert to RGBA
    img = img.convert('RGBA')

    # Unity stores textures bottom-up (flip vertically)
    img_flipped = img.transpose(Image.FLIP_TOP_BOTTOM)

    # Get raw RGBA bytes
    rgba_data = img_flipped.tobytes('raw', 'RGBA')

    if len(rgba_data) != PORTRAIT_SIZE:
        print(f"ERROR: Data size mismatch! Expected {PORTRAIT_SIZE}, got {len(rgba_data)}")
        return False

    # Read .resS file
    print(f"Reading: {RESS_FILE}")
    with open(RESS_FILE, 'rb') as f:
        content = bytearray(f.read())

    print(f"File size: {len(content):,} bytes")
    print(f"Portrait offset: {PORTRAIT_OFFSET:,}")
    print(f"Portrait size: {PORTRAIT_SIZE:,} bytes")

    # Replace texture data
    content[PORTRAIT_OFFSET:PORTRAIT_OFFSET + PORTRAIT_SIZE] = rgba_data

    # Write
    with open(RESS_FILE, 'wb') as f:
        f.write(content)

    print(f"Saved: {RESS_FILE}")
    return True


def main():
    print("=" * 60)
    print("Endstar Character Identity Patcher")
    print("=" * 60)
    print(f"\nChanging: {OLD_NAME.decode()} -> {NEW_NAME.decode()}")
    print(f"Portrait: {PORTRAIT_SOURCE.name}")

    # Verify files exist
    if not ASSETS_FILE.exists():
        print(f"\nERROR: Assets file not found: {ASSETS_FILE}")
        return False

    if not RESS_FILE.exists():
        print(f"\nERROR: ResS file not found: {RESS_FILE}")
        return False

    # Ensure backup folder exists
    BACKUP_FOLDER.mkdir(parents=True, exist_ok=True)

    # Patch name
    if not patch_display_name():
        return False

    # Patch portrait
    if not patch_portrait():
        return False

    print("\n" + "=" * 60)
    print("PATCHING COMPLETE")
    print("=" * 60)
    print("\nCharacter identity changed to 'Zayed'")
    print("\nIMPORTANT: Run Endstar.exe directly, NOT through the launcher!")
    print("           (Launcher may restore original files)")

    return True


if __name__ == '__main__':
    success = main()
    exit(0 if success else 1)

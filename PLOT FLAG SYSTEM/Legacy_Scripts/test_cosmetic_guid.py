import winreg
import sys

REGISTRY_PATH = r"Software\Endless Studios\Endstar"
KEY_NAME = "Character Visual_h1713213608"

def read_current_guid():
    """Read current Character Visual GUID"""
    try:
        with winreg.OpenKey(winreg.HKEY_CURRENT_USER, REGISTRY_PATH) as key:
            value, _ = winreg.QueryValueEx(key, KEY_NAME)
            # Value is bytes, decode it
            if isinstance(value, bytes):
                guid = value.decode('utf-8').rstrip('\x00')
            else:
                guid = str(value)
            print(f"Current GUID: {guid}")
            return guid
    except Exception as e:
        print(f"Error reading: {e}")
        return None

def set_guid(new_guid):
    """Set a new Character Visual GUID"""
    try:
        with winreg.OpenKey(winreg.HKEY_CURRENT_USER, REGISTRY_PATH, 0, winreg.KEY_SET_VALUE) as key:
            # Unity stores as bytes with null terminator
            value = (new_guid + '\x00').encode('utf-8')
            winreg.SetValueEx(key, KEY_NAME, 0, winreg.REG_BINARY, value)
            print(f"Set GUID to: {new_guid}")
            return True
    except Exception as e:
        print(f"Error setting: {e}")
        return False

def backup_and_set_fake():
    """Backup current GUID and set a fake one"""
    current = read_current_guid()
    if current:
        print(f"\n*** BACKUP YOUR CURRENT GUID: {current} ***")
        print("(Save this to restore later!)\n")

        # Set a completely fake GUID
        fake_guid = "00000000-0000-0000-0000-000000000001"
        set_guid(fake_guid)

        print("\n=== TEST INSTRUCTIONS ===")
        print("1. Launch Endstar")
        print("2. Observe what happens:")
        print("   - Game crashes? → Client validates GUIDs")
        print("   - Default character? → Client validates, falls back")
        print("   - Invisible/broken? → No validation, might work!")
        print("   - Server kicks you? → Server validates GUIDs")
        print("\n3. To restore, run:")
        print(f'   python test_cosmetic_guid.py --restore "{current}"')

def restore_guid(guid):
    """Restore a saved GUID"""
    set_guid(guid)
    print("GUID restored!")

if __name__ == "__main__":
    print("=== Endstar Character Visual GUID Test ===\n")

    if len(sys.argv) > 1:
        if sys.argv[1] == "--set-fake":
            backup_and_set_fake()
        elif sys.argv[1] == "--restore" and len(sys.argv) > 2:
            restore_guid(sys.argv[2])
        elif sys.argv[1] == "--set" and len(sys.argv) > 2:
            set_guid(sys.argv[2])
        else:
            print("Usage:")
            print("  python test_cosmetic_guid.py              # Read current GUID")
            print("  python test_cosmetic_guid.py --set-fake   # Set fake GUID for testing")
            print("  python test_cosmetic_guid.py --restore <guid>  # Restore a GUID")
            print("  python test_cosmetic_guid.py --set <guid>      # Set specific GUID")
    else:
        read_current_guid()

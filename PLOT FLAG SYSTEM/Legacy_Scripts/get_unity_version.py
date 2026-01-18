import struct
import os

def get_unity_version(game_path):
    """Extract Unity version from game files"""

    # Try globalgamemanagers
    ggm_path = os.path.join(game_path, "globalgamemanagers")
    if os.path.exists(ggm_path):
        with open(ggm_path, 'rb') as f:
            data = f.read(100)
            # Look for version string pattern like "2022.3.10f1"
            text = data.decode('utf-8', errors='ignore')
            import re
            match = re.search(r'20\d{2}\.\d+\.\d+[a-z]\d+', text)
            if match:
                print(f"Unity Version: {match.group()}")
                return match.group()

    # Try level0
    level0_path = os.path.join(game_path, "level0")
    if os.path.exists(level0_path):
        with open(level0_path, 'rb') as f:
            data = f.read(500)
            text = data.decode('utf-8', errors='ignore')
            import re
            match = re.search(r'20\d{2}\.\d+\.\d+[a-z]\d+', text)
            if match:
                print(f"Unity Version: {match.group()}")
                return match.group()

    # Check UnityPlayer.dll file info
    player_dll = os.path.join(game_path, "..", "UnityPlayer.dll")
    if os.path.exists(player_dll):
        # Read some bytes and look for version
        with open(player_dll, 'rb') as f:
            data = f.read(50000)
            text = data.decode('utf-8', errors='ignore')
            import re
            match = re.search(r'20\d{2}\.\d+\.\d+[a-z]\d+', text)
            if match:
                print(f"Unity Version: {match.group()}")
                return match.group()

    print("Could not determine Unity version")
    return None

if __name__ == "__main__":
    game_data = r"C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data"
    get_unity_version(game_data)

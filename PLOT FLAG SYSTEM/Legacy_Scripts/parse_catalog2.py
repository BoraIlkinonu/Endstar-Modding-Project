import json
import base64

# Read the catalog
with open(r"C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\StreamingAssets\aa\catalog.json", 'r') as f:
    catalog = json.load(f)

# Decode KeyDataString to see actual keys
print("=== DECODING KEY DATA ===")
try:
    key_data = base64.b64decode(catalog['m_KeyDataString'])
    # Find readable strings
    text = key_data.decode('utf-8', errors='ignore')
    # Extract strings that look like asset keys
    import re
    strings = re.findall(r'[A-Za-z0-9_\-\.\/]{10,}', text)
    cosmetic_keys = [s for s in strings if 'cosmetic' in s.lower() or 'character' in s.lower()]
    print(f"Found {len(cosmetic_keys)} cosmetic-related keys:")
    for k in cosmetic_keys[:20]:
        print(f"  {k}")
except Exception as e:
    print(f"Error decoding: {e}")

# Find all internal IDs related to cosmetics
print("\n=== ALL COSMETIC INTERNAL IDS ===")
for i, id in enumerate(catalog['m_InternalIds']):
    if 'cosmetic' in id.lower() or 'character' in id.lower():
        print(f"  [{i}] {id}")

# Print resource types
print("\n=== RESOURCE TYPES ===")
for rt in catalog['m_resourceTypes'][:10]:
    print(f"  {rt}")

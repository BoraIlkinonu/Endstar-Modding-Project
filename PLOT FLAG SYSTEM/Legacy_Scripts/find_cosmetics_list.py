import json

# Read the catalog
with open(r"C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\StreamingAssets\aa\catalog.json", 'r') as f:
    catalog = json.load(f)

# Find CharacterCosmeticsList or CharacterCosmeticsDefinition
print("=== SEARCHING FOR COSMETICS LIST/DEFINITION ===")
for i, id in enumerate(catalog['m_InternalIds']):
    lower = id.lower()
    if 'list' in lower or 'definition' in lower or 'scriptable' in lower:
        print(f"  [{i}] {id}")

# Search in key data
import base64
import re
try:
    key_data = base64.b64decode(catalog['m_KeyDataString'])
    text = key_data.decode('utf-8', errors='ignore')
    strings = re.findall(r'[A-Za-z0-9_\-\.\/]{5,}', text)
    for s in strings:
        if 'list' in s.lower() or 'definition' in s.lower():
            print(f"  KEY: {s}")
except:
    pass

# Look for ScriptableObjects or asset references
print("\n=== ALL SCRIPTABLEOBJECT/ASSET REFERENCES ===")
for i, id in enumerate(catalog['m_InternalIds']):
    if '.asset' in id.lower():
        print(f"  [{i}] {id}")

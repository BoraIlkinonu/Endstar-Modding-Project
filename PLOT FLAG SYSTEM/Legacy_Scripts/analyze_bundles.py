import json

with open(r"C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\StreamingAssets\aa\catalog.json", 'r') as f:
    catalog = json.load(f)

# List all bundles
print("=== ALL BUNDLES ===")
bundles = [id for id in catalog['m_InternalIds'] if '.bundle' in id]
for i, b in enumerate(bundles):
    name = b.split('\\')[-1] if '\\' in b else b
    print(f"  {i}: {name}")

print(f"\nTotal bundles: {len(bundles)}")

# Look for gamestar/core bundles that might contain list
print("\n=== POTENTIAL CORE BUNDLES ===")
for id in catalog['m_InternalIds']:
    lower = id.lower()
    if '.bundle' in lower and ('core' in lower or 'game' in lower or 'base' in lower or 'shared' in lower or 'common' in lower):
        print(f"  {id}")

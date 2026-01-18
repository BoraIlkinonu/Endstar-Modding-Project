import json
import sys

# Read the catalog
with open(r"C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\StreamingAssets\aa\catalog.json", 'r') as f:
    catalog = json.load(f)

# Print structure
print("=== CATALOG KEYS ===")
for key in catalog.keys():
    val = catalog[key]
    if isinstance(val, list):
        print(f"{key}: list of {len(val)} items")
    elif isinstance(val, dict):
        print(f"{key}: dict with {len(val)} keys")
    else:
        print(f"{key}: {type(val).__name__} = {str(val)[:100]}")

# Look for character/cosmetic related entries
print("\n=== SEARCHING FOR COSMETICS ===")
def search_dict(d, depth=0):
    results = []
    if isinstance(d, dict):
        for k, v in d.items():
            if isinstance(k, str) and ('cosmetic' in k.lower() or 'character' in k.lower() or 'felix' in k.lower()):
                results.append((k, str(v)[:200]))
            results.extend(search_dict(v, depth+1))
    elif isinstance(d, list):
        for item in d[:50]:  # Limit to prevent huge output
            results.extend(search_dict(item, depth+1))
    elif isinstance(d, str):
        if 'cosmetic' in d.lower() or 'felix' in d.lower():
            results.append(("string", d[:200]))
    return results

results = search_dict(catalog)
for r in results[:30]:
    print(f"  {r[0]}: {r[1]}")

# Print m_InternalIds if it exists (usually contains asset paths)
if 'm_InternalIds' in catalog:
    print("\n=== INTERNAL IDS (first 20) ===")
    for i, id in enumerate(catalog['m_InternalIds'][:20]):
        print(f"  {i}: {id}")

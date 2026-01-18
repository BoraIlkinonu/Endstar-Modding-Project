"""Analyze character cosmetics in Endstar catalog"""
import json
import os

CATALOG_PATH = r"C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\StreamingAssets\aa\catalog.json"

with open(CATALOG_PATH, 'r') as f:
    catalog = json.load(f)

internal_ids = catalog['m_InternalIds']

print('=== CATALOG STRUCTURE ANALYSIS ===')
print(f'Total Internal IDs: {len(internal_ids)}')
print()

# Find all character cosmetic prefabs
print('=== CHARACTER COSMETIC PREFABS ===')
prefabs = []
for i, id in enumerate(internal_ids):
    if 'CharacterCosmetic' in id and '.prefab' in id:
        prefabs.append((i, id))
        print(f'[{i}] {id}')

print(f'\nTotal prefabs: {len(prefabs)}')

print()
print('=== CHARACTER BUNDLES ===')
bundles = []
for i, id in enumerate(internal_ids):
    if '.bundle' in id:
        # Extract just the bundle name from path
        bundle_name = id.split('\\')[-1] if '\\' in id else id
        bundles.append((i, bundle_name, id))

# Show character-related bundles
char_names = ['felix', 'tilly', 'rashed', 'mira', 'kira', 'bun', 'lumi', 'nix', 'quinn', 'riley', 'pack']
for i, bundle_name, full_path in bundles:
    if any(name in bundle_name.lower() for name in char_names):
        print(f'[{i}] {bundle_name}')

print(f'\nTotal bundles: {len(bundles)}')

# Analyze one character in detail
print()
print('=== DETAILED ANALYSIS: FELIX ===')
felix_entries = []
for i, id in enumerate(internal_ids):
    if 'felix' in id.lower():
        felix_entries.append((i, id))
        print(f'[{i}] {id}')

print(f'\nFelix has {len(felix_entries)} catalog entries')

# Show the relationship between bundle index and prefab indices
print()
print('=== BUNDLE-PREFAB MAPPING (Felix) ===')
felix_bundle_idx = None
felix_prefab_indices = []
for i, id in enumerate(internal_ids):
    if 'felix' in id.lower():
        if '.bundle' in id:
            felix_bundle_idx = i
            print(f'Bundle Index: {i}')
        elif '.prefab' in id:
            felix_prefab_indices.append(i)
            print(f'Prefab Index: {i} -> {id.split("/")[-1]}')

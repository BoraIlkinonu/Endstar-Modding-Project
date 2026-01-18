"""
Check the hierarchy of the saved blend file
"""

import bpy

BLEND_PATH = r"D:\Endstar Plot Flag\PLOT FLAG SYSTEM\Meshy_AI_Pearl_Diver_0103103333_texture_obj\Meshy_AI_Pearl_Diver_0103103333_texture_obj\CharacterCosmetics_ThePack_Felix_01_A_with_PearlDiver.blend"

# Open the blend file
bpy.ops.wm.open_mainfile(filepath=BLEND_PATH)

print("\n" + "=" * 70)
print("HIERARCHY IN SAVED BLEND FILE:")
print("=" * 70)

def print_hierarchy(obj, indent=0):
    prefix = "  " * indent
    info = f"{prefix}- {obj.name} [{obj.type}]"
    if obj.scale != (1.0, 1.0, 1.0):
        info += f" scale={obj.scale[0]:.2f}"
    print(info)

    for child in sorted(obj.children, key=lambda x: x.name):
        print_hierarchy(child, indent + 1)

# Find root objects
roots = [obj for obj in bpy.data.objects if obj.parent is None]
for root in sorted(roots, key=lambda x: x.name):
    print_hierarchy(root)

print("\n" + "=" * 70)
print("VERIFICATION:")
print("=" * 70)

# Check if PearlDiver exists and its parent
pearl_diver = None
for obj in bpy.data.objects:
    if "PearlDiver" in obj.name:
        pearl_diver = obj
        break

if pearl_diver:
    print(f"PearlDiver found: {pearl_diver.name}")
    print(f"  Parent: {pearl_diver.parent.name if pearl_diver.parent else 'NONE'}")
    print(f"  Parent type: {pearl_diver.parent.type if pearl_diver.parent else 'N/A'}")

    # Check siblings
    if pearl_diver.parent:
        siblings = [c.name for c in pearl_diver.parent.children if c != pearl_diver]
        print(f"  Siblings: {siblings}")

        # Check if TigerGuy meshes are siblings
        tiger_siblings = [s for s in siblings if "TigerGuy" in s]
        if tiger_siblings:
            print(f"\n  SUCCESS: PearlDiver is sibling of TigerGuy meshes!")
        else:
            print(f"\n  FAIL: PearlDiver is NOT sibling of TigerGuy meshes")
else:
    print("ERROR: PearlDiver not found in scene!")

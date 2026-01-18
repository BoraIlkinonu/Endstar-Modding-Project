"""
Inspect FBX hierarchy in detail
"""

import bpy

FBX_PATH = r"D:\Endstar Plot Flag\PLOT FLAG SYSTEM\Meshy_AI_Pearl_Diver_0103103333_texture_obj\CharacterCosmetics_ThePack_Felix_01_A.fbx"

# Clear scene
bpy.ops.wm.read_factory_settings(use_empty=True)

# Import FBX
bpy.ops.import_scene.fbx(filepath=FBX_PATH)

print("\n" + "=" * 70)
print("DETAILED HIERARCHY:")
print("=" * 70)

def print_hierarchy(obj, indent=0):
    prefix = "  " * indent
    info = f"{prefix}- {obj.name}"
    info += f" [type: {obj.type}]"
    if obj.type == 'ARMATURE':
        info += f" (bones: {len(obj.data.bones)})"
    print(info)

    # If it's an armature, show the bones
    if obj.type == 'ARMATURE':
        for bone in obj.data.bones:
            if bone.parent is None:  # Root bones only
                print_bone(bone, indent + 1)

    # Print children
    for child in sorted(obj.children, key=lambda x: x.name):
        print_hierarchy(child, indent + 1)

def print_bone(bone, indent):
    prefix = "  " * indent
    print(f"{prefix}[BONE] {bone.name}")
    for child_bone in bone.children:
        print_bone(child_bone, indent + 1)

# Find root objects
roots = [obj for obj in bpy.data.objects if obj.parent is None]
for root in sorted(roots, key=lambda x: x.name):
    print_hierarchy(root)

print("\n" + "=" * 70)
print("ALL OBJECTS FLAT LIST:")
print("=" * 70)
for obj in sorted(bpy.data.objects, key=lambda x: x.name):
    parent = obj.parent.name if obj.parent else "ROOT"
    print(f"  {obj.name} -> parent: {parent}, type: {obj.type}")

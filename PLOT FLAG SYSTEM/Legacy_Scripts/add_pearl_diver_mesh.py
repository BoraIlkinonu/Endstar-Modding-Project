"""
Blender Python script to add Pearl Diver mesh to Felix FBX hierarchy.

Run with:
    blender --background --python add_pearl_diver_mesh.py
"""

import bpy
import os

# File paths
FBX_PATH = r"D:\Endstar Plot Flag\PLOT FLAG SYSTEM\Meshy_AI_Pearl_Diver_0103103333_texture_obj\CharacterCosmetics_ThePack_Felix_01_A.fbx"
OBJ_PATH = r"D:\Endstar Plot Flag\PLOT FLAG SYSTEM\Meshy_AI_Pearl_Diver_0103103333_texture_obj\Meshy_AI_Pearl_Diver_0103103333_texture_obj\Meshy_AI_Pearl_Diver_0103103333_texture.obj"
OUTPUT_FBX = r"D:\Endstar Plot Flag\PLOT FLAG SYSTEM\Meshy_AI_Pearl_Diver_0103103333_texture_obj\Meshy_AI_Pearl_Diver_0103103333_texture_obj\CharacterCosmetics_ThePack_Felix_01_A_with_PearlDiver.fbx"

def main():
    # Clear the scene
    bpy.ops.wm.read_factory_settings(use_empty=True)

    print("=" * 60)
    print("Step 1: Importing Felix FBX...")
    print("=" * 60)

    # Import the Felix FBX
    bpy.ops.import_scene.fbx(filepath=FBX_PATH)

    # Print hierarchy before modification
    print("\nHierarchy after FBX import:")
    for obj in bpy.data.objects:
        parent_name = obj.parent.name if obj.parent else "None"
        print(f"  {obj.name} (type: {obj.type}, parent: {parent_name})")

    # Find ThePack_Felix_VC_01 (the armature) - we want to parent the mesh as a child of this
    # So it becomes a sibling of TigerGuy_LOD0/1/2
    target_parent = None

    for obj in bpy.data.objects:
        if "ThePack_Felix_VC_01" in obj.name and obj.type == 'ARMATURE':
            target_parent = obj
            break

    if not target_parent:
        print("\nERROR: Could not find ThePack_Felix_VC_01 armature!")
        print("Available objects:")
        for obj in bpy.data.objects:
            print(f"  - {obj.name} (type: {obj.type})")
        return False

    print(f"\nFound target parent (armature): {target_parent.name}")
    print(f"TigerGuy meshes are children of this armature - PearlDiver will be too")

    print("\n" + "=" * 60)
    print("Step 2: Importing Pearl Diver OBJ...")
    print("=" * 60)

    # Import the OBJ
    bpy.ops.wm.obj_import(filepath=OBJ_PATH)

    # Find the newly imported mesh (it will be the selected object)
    pearl_diver_mesh = None
    for obj in bpy.context.selected_objects:
        if obj.type == 'MESH':
            pearl_diver_mesh = obj
            break

    # If not found in selection, look for any new mesh objects
    if not pearl_diver_mesh:
        for obj in bpy.data.objects:
            if obj.type == 'MESH' and "Meshy" in obj.name or "Pearl" in obj.name or "texture" in obj.name.lower():
                pearl_diver_mesh = obj
                break

    if not pearl_diver_mesh:
        print("\nERROR: Could not find imported Pearl Diver mesh!")
        print("Available objects after import:")
        for obj in bpy.data.objects:
            print(f"  - {obj.name} (type: {obj.type})")
        return False

    print(f"Found Pearl Diver mesh: {pearl_diver_mesh.name}")

    print("\n" + "=" * 60)
    print("Step 3: Reparenting and scaling mesh...")
    print("=" * 60)

    # Rename to something cleaner
    pearl_diver_mesh.name = "PearlDiver_LOD0"
    print(f"Renamed mesh to: {pearl_diver_mesh.name}")

    # Set scale to 0.7
    pearl_diver_mesh.scale = (0.7, 0.7, 0.7)
    print(f"Set scale to: {pearl_diver_mesh.scale[:]}")

    # Parent to ThePack_Felix_VC_01 (keep transform)
    pearl_diver_mesh.parent = target_parent
    pearl_diver_mesh.matrix_parent_inverse = target_parent.matrix_world.inverted()
    print(f"Parented to: {target_parent.name}")

    # Print final hierarchy
    print("\n" + "=" * 60)
    print("Final Hierarchy:")
    print("=" * 60)

    def print_hierarchy(obj, indent=0):
        print("  " * indent + f"- {obj.name} (type: {obj.type}, scale: {obj.scale[:]})")
        for child in obj.children:
            print_hierarchy(child, indent + 1)

    # Find root objects and print hierarchy
    for obj in bpy.data.objects:
        if obj.parent is None:
            print_hierarchy(obj)

    print("\n" + "=" * 60)
    print("Step 4: Saving .blend file...")
    print("=" * 60)

    # Save as .blend file first
    BLEND_PATH = r"D:\Endstar Plot Flag\PLOT FLAG SYSTEM\Meshy_AI_Pearl_Diver_0103103333_texture_obj\Meshy_AI_Pearl_Diver_0103103333_texture_obj\CharacterCosmetics_ThePack_Felix_01_A_with_PearlDiver.blend"
    bpy.ops.wm.save_as_mainfile(filepath=BLEND_PATH)
    print(f"Saved .blend file to: {BLEND_PATH}")

    print("\n" + "=" * 60)
    print("Step 5: Exporting FBX...")
    print("=" * 60)

    # Select all objects for export
    bpy.ops.object.select_all(action='SELECT')

    # Export as FBX
    bpy.ops.export_scene.fbx(
        filepath=OUTPUT_FBX,
        use_selection=False,
        apply_scale_options='FBX_SCALE_ALL',
        bake_space_transform=False,
        object_types={'EMPTY', 'ARMATURE', 'MESH'},
        use_mesh_modifiers=True,
        mesh_smooth_type='FACE',
        add_leaf_bones=False,
        primary_bone_axis='Y',
        secondary_bone_axis='X',
        armature_nodetype='NULL',
        bake_anim=False
    )

    print(f"\nExported to: {OUTPUT_FBX}")
    print("\nDONE!")
    return True

if __name__ == "__main__":
    success = main()
    if not success:
        print("\nScript failed!")

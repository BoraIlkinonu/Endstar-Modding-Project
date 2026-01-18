"""
Fix hierarchy: Make PearlDiver sibling of Armature object under ThePack_Felix_VC_01
"""
import bpy

FBX_PATH = r"D:\Endstar Plot Flag\PLOT FLAG SYSTEM\Meshy_AI_Pearl_Diver_0103103333_texture_obj\CharacterCosmetics_ThePack_Felix_01_A.fbx"
OBJ_PATH = r"D:\Endstar Plot Flag\PLOT FLAG SYSTEM\Meshy_AI_Pearl_Diver_0103103333_texture_obj\Meshy_AI_Pearl_Diver_0103103333_texture_obj\Meshy_AI_Pearl_Diver_0103103333_texture.obj"
BLEND_PATH = r"D:\Endstar Plot Flag\PLOT FLAG SYSTEM\Meshy_AI_Pearl_Diver_0103103333_texture_obj\Meshy_AI_Pearl_Diver_0103103333_texture_obj\CharacterCosmetics_ThePack_Felix_01_A_with_PearlDiver.blend"
OUTPUT_FBX = r"D:\Endstar Plot Flag\PLOT FLAG SYSTEM\Meshy_AI_Pearl_Diver_0103103333_texture_obj\Meshy_AI_Pearl_Diver_0103103333_texture_obj\CharacterCosmetics_ThePack_Felix_01_A_with_PearlDiver.fbx"

# Clear scene
bpy.ops.wm.read_factory_settings(use_empty=True)

# Import FBX
bpy.ops.import_scene.fbx(filepath=FBX_PATH)

# Import OBJ
bpy.ops.wm.obj_import(filepath=OBJ_PATH)

# Find objects
armature = None
pearl_diver = None
parent_empty = None

for obj in bpy.data.objects:
    if obj.type == 'ARMATURE':
        armature = obj
    if "Meshy" in obj.name or "texture" in obj.name.lower():
        pearl_diver = obj
    if obj.name == "CharacterCosmetics_ThePack_Felix_01_A" and obj.type == 'EMPTY':
        parent_empty = obj

# Get the parent of armature (ThePack_Felix_VC_01's parent)
thepack_parent = armature.parent if armature else None

# Create new empty to be ThePack_Felix_VC_01
new_thepack = bpy.data.objects.new("ThePack_Felix_VC_01_new", None)
bpy.context.collection.objects.link(new_thepack)
new_thepack.parent = thepack_parent
new_thepack.matrix_parent_inverse = thepack_parent.matrix_world.inverted() if thepack_parent else None

# Rename armature to just "Armature" and parent to new empty
armature.name = "Armature"
armature.parent = new_thepack
armature.matrix_parent_inverse = new_thepack.matrix_world.inverted()

# Move TigerGuy meshes to new parent
for obj in list(bpy.data.objects):
    if "TigerGuy" in obj.name:
        obj.parent = new_thepack
        obj.matrix_parent_inverse = new_thepack.matrix_world.inverted()

# Setup Pearl Diver
pearl_diver.name = "PearlDiver_LOD0"
pearl_diver.scale = (0.7, 0.7, 0.7)
pearl_diver.parent = new_thepack
pearl_diver.matrix_parent_inverse = new_thepack.matrix_world.inverted()

# Delete old ThePack_Felix_VC_01 if it exists and is empty
for obj in list(bpy.data.objects):
    if "ThePack_Felix_VC_01" in obj.name and obj != new_thepack and obj != armature:
        if len(obj.children) == 0:
            bpy.data.objects.remove(obj)

# Rename new empty
new_thepack.name = "ThePack_Felix_VC_01"

# Print result
print("\n" + "=" * 60)
print("FINAL HIERARCHY:")
print("=" * 60)
for obj in bpy.data.objects:
    parent = obj.parent.name if obj.parent else "ROOT"
    print(f"  {obj.name} [{obj.type}] -> parent: {parent}")

# Save
bpy.ops.wm.save_as_mainfile(filepath=BLEND_PATH)
print(f"\nSaved: {BLEND_PATH}")

# Export FBX
bpy.ops.export_scene.fbx(
    filepath=OUTPUT_FBX,
    use_selection=False,
    object_types={'EMPTY', 'ARMATURE', 'MESH'},
    add_leaf_bones=False,
    bake_anim=False
)
print(f"Exported: {OUTPUT_FBX}")

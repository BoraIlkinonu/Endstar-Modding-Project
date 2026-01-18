import bpy

# Clear scene
bpy.ops.wm.read_factory_settings(use_empty=True)

# Import the pearl diver FBX
bpy.ops.import_scene.fbx(filepath=r'D:\Unity_Workshop\Endstar Custom Shader\Assets\Pearl Diver\pearl diver.fbx')

print("=== BEFORE FIX ===")
for obj in bpy.data.objects:
    parent_name = obj.parent.name if obj.parent else "ROOT"
    print(f"  {obj.name} [{obj.type}] -> parent: {parent_name}")

# Find key objects
armature = None
pearl_mesh = None
felix_empty = None  # ThePack_Felix_VC_01

for obj in bpy.data.objects:
    if obj.type == 'ARMATURE':
        armature = obj
    if 'Meshy' in obj.name or 'Pearl' in obj.name or 'texture' in obj.name.lower():
        if obj.type == 'MESH':
            pearl_mesh = obj
    if 'ThePack_Felix_VC_01' in obj.name and obj.type == 'EMPTY':
        felix_empty = obj

print(f"\nFound Armature: {armature.name if armature else 'NOT FOUND'}")
print(f"Found Pearl Mesh: {pearl_mesh.name if pearl_mesh else 'NOT FOUND'}")
print(f"Found Felix Empty: {felix_empty.name if felix_empty else 'NOT FOUND'}")

if not armature:
    print("ERROR: No armature found!")
    exit(1)

if not pearl_mesh:
    # Try to find any mesh that's not TigerGuy
    for obj in bpy.data.objects:
        if obj.type == 'MESH' and 'TigerGuy' not in obj.name:
            pearl_mesh = obj
            print(f"Using mesh: {pearl_mesh.name}")
            break

if not pearl_mesh:
    print("ERROR: No Pearl Diver mesh found!")
    exit(1)

# Get the parent empty (ThePack_Felix_VC_01) - same parent as armature
target_parent = armature.parent
print(f"Target parent for mesh: {target_parent.name if target_parent else 'ROOT'}")

# Clear any existing parent on pearl mesh
bpy.context.view_layer.objects.active = pearl_mesh
pearl_mesh.select_set(True)

# Store world matrix before unparenting
world_matrix = pearl_mesh.matrix_world.copy()

# Clear parent but keep transform
bpy.ops.object.parent_clear(type='CLEAR_KEEP_TRANSFORM')

# Rename mesh to match LOD naming convention
pearl_mesh.name = "TigerGuy_LOD0"
print(f"Renamed mesh to: {pearl_mesh.name}")

# Parent mesh to the same parent as armature (ThePack_Felix_VC_01)
if target_parent:
    pearl_mesh.parent = target_parent
    pearl_mesh.matrix_parent_inverse = target_parent.matrix_world.inverted()

# Now parent mesh to armature with automatic weights
bpy.ops.object.select_all(action='DESELECT')
pearl_mesh.select_set(True)
armature.select_set(True)
bpy.context.view_layer.objects.active = armature

# Parent with automatic weights
try:
    bpy.ops.object.parent_set(type='ARMATURE_AUTO')
    print("Successfully parented with automatic weights!")
except Exception as e:
    print(f"Auto weights failed: {e}")
    # Try without weights
    bpy.ops.object.parent_set(type='ARMATURE')
    print("Parented to armature without auto weights")

# Verify armature modifier exists
has_armature_mod = False
for mod in pearl_mesh.modifiers:
    if mod.type == 'ARMATURE':
        has_armature_mod = True
        mod.object = armature
        print(f"Armature modifier found and set to: {armature.name}")

if not has_armature_mod:
    mod = pearl_mesh.modifiers.new(name='Armature', type='ARMATURE')
    mod.object = armature
    print("Added Armature modifier")

# Delete any other meshes (TigerGuy originals if they exist separately)
meshes_to_delete = []
for obj in bpy.data.objects:
    if obj.type == 'MESH' and obj != pearl_mesh:
        if 'TigerGuy' in obj.name or 'Tiger' in obj.name:
            meshes_to_delete.append(obj)

for obj in meshes_to_delete:
    print(f"Deleting old mesh: {obj.name}")
    bpy.data.objects.remove(obj, do_unlink=True)

# Create LOD1 and LOD2 as duplicates
bpy.ops.object.select_all(action='DESELECT')
pearl_mesh.select_set(True)
bpy.context.view_layer.objects.active = pearl_mesh

# Duplicate for LOD1
bpy.ops.object.duplicate()
lod1 = bpy.context.active_object
lod1.name = "TigerGuy_LOD1"

# Duplicate for LOD2
bpy.ops.object.duplicate()
lod2 = bpy.context.active_object
lod2.name = "TigerGuy_LOD2"

print("\n=== AFTER FIX ===")
for obj in bpy.data.objects:
    parent_name = obj.parent.name if obj.parent else "ROOT"
    print(f"  {obj.name} [{obj.type}] -> parent: {parent_name}")

# Save blend file
output_blend = r'D:\Endstar Plot Flag\PLOT FLAG SYSTEM\pearl_diver_fixed.blend'
bpy.ops.wm.save_as_mainfile(filepath=output_blend)
print(f"\nSaved blend file: {output_blend}")

# Export FBX
output_fbx = r'D:\Unity_Workshop\Endstar Custom Shader\Assets\Pearl Diver\pearl_diver_fixed.fbx'
bpy.ops.export_scene.fbx(
    filepath=output_fbx,
    use_selection=False,
    object_types={'ARMATURE', 'MESH', 'EMPTY'},
    add_leaf_bones=False,
    bake_anim=False,
    mesh_smooth_type='FACE',
    use_mesh_modifiers=True,
    path_mode='COPY',
    embed_textures=False
)
print(f"Exported FBX: {output_fbx}")

print("\nDONE! Now in Unity:")
print("1. Select pearl_diver_fixed.fbx")
print("2. Rig -> Animation Type: Humanoid")
print("3. Click Apply")
print("4. Update prefab to use new FBX")
print("5. Rebuild Addressables")

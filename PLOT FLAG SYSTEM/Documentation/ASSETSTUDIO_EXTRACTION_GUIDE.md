# AssetStudio Character Extraction Guide

## Step 1: Open Felix Bundle in AssetStudio

1. In AssetStudio GUI, go to **File > Load file**
2. Navigate to: `C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\StreamingAssets\aa\StandaloneWindows64`
3. Select: `felix_assets_all_8ce5cfff23e50dfcaa729aa03940bfd7.bundle`
4. Click **Open**

## Step 2: Set Assembly Folder (Important!)

When prompted "Select the assembly folder", navigate to:
```
C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed
```
This allows AssetStudio to properly deserialize MonoBehaviours.

## Step 3: View Asset List

1. Click the **Asset List** tab
2. You should see assets like:
   - **GameObject** (the character prefab)
   - **Mesh** (character mesh)
   - **Material** (character materials)
   - **Texture2D** (character textures)
   - **AnimatorController** (animation setup)
   - **AnimationClip** (animations)

## Step 4: View Scene Hierarchy

1. Click the **Scene Hierarchy** tab
2. Expand the tree to see the prefab structure:
   ```
   CharacterCosmetics_ThePack_Felix_01_A
   ├── Armature
   │   ├── Hips
   │   │   ├── Spine
   │   │   │   └── Chest → Head
   │   │   ├── LeftUpLeg → LeftFoot
   │   │   └── RightUpLeg → RightFoot
   └── Body (SkinnedMeshRenderer)
   ```

## Step 5: Export All Assets

1. Go to **Export > All assets**
2. Choose output folder (e.g., `D:\Endstar Plot Flag\PLOT FLAG SYSTEM\FelixExtracted`)
3. Select formats:
   - **Mesh**: OBJ or FBX
   - **Texture**: PNG
   - **Animator**: FBX (includes animations)

## Step 6: Export Model with Animations

1. In **Scene Hierarchy**, select the root GameObject
2. Go to **Model > Export selected objects**
3. This exports the FBX with skeleton and mesh

## Key Components to Note

### Required Components on Character Prefab:

1. **Animator**
   - Controller: Reference to AnimatorController
   - Avatar: Humanoid avatar for animation retargeting

2. **SkinnedMeshRenderer**
   - Mesh: The character mesh
   - Materials: Character materials
   - Bones: Bone references for skinning

3. **Custom Scripts** (from Gameplay.dll):
   - May include character-specific behaviors
   - Check MonoBehaviour components

### Naming Convention

Characters follow this pattern:
- Prefab: `CharacterCosmetics_[Faction]_[Name]_[Version].prefab`
- Bundle: `[name]_assets_all_[hash].bundle`

Examples:
- `CharacterCosmetics_ThePack_Felix_01_A.prefab`
- `felix_assets_all_8ce5cfff23e50dfcaa729aa03940bfd7.bundle`

## After Extraction

Once you've extracted Felix, you'll have:
1. FBX model with skeleton
2. Textures (PNG)
3. Materials (JSON/data)
4. AnimatorController info

Use this as a template for your custom character!

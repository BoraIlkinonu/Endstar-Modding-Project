import UnityPy
import os
import shutil

BUNDLE_PATH = r"C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\StreamingAssets\aa\StandaloneWindows64\felix_assets_all_8ce5cfff23e50dfcaa729aa03940bfd7.bundle"
ORIGINAL_PATH = BUNDLE_PATH + ".original"

def fix_material():
    # First restore from original
    if os.path.exists(ORIGINAL_PATH):
        print(f"Restoring from original: {ORIGINAL_PATH}")
        shutil.copy2(ORIGINAL_PATH, BUNDLE_PATH)
    else:
        print("ERROR: Original backup not found!")
        return

    print(f"\nLoading bundle: {BUNDLE_PATH}")
    env = UnityPy.load(BUNDLE_PATH)

    modified = False

    for obj in env.objects:
        if obj.type.name == "Material":
            data = obj.read()
            mat_name = data.m_Name

            if "Felix" in mat_name or "ThePack" in mat_name:
                print(f"\nMaterial: {mat_name}")
                print(f"  Before - RenderQueue: {data.m_CustomRenderQueue}")
                print(f"  Before - Keywords: {data.m_ShaderKeywords}")

                # Verify properties exist
                if hasattr(data, 'm_SavedProperties'):
                    props = data.m_SavedProperties
                    float_count = len(props.m_Floats) if hasattr(props, 'm_Floats') and props.m_Floats else 0
                    color_count = len(props.m_Colors) if hasattr(props, 'm_Colors') and props.m_Colors else 0
                    tex_count = len(props.m_TexEnvs) if hasattr(props, 'm_TexEnvs') and props.m_TexEnvs else 0
                    print(f"  Properties: {float_count} floats, {color_count} colors, {tex_count} textures")

                # Apply fixes
                data.m_CustomRenderQueue = 2450
                data.m_ShaderKeywords = "_ALPHATEST_ON"

                print(f"  After - RenderQueue: {data.m_CustomRenderQueue}")
                print(f"  After - Keywords: {data.m_ShaderKeywords}")

                # Save using typetree
                obj.save_typetree(data)
                modified = True
                print("  Material updated!")

    if modified:
        print(f"\nSaving bundle...")

        # Save the bundle
        with open(BUNDLE_PATH, "wb") as f:
            f.write(env.file.save())

        print("Bundle saved!")

        # Verify the save worked
        print("\nVerifying saved bundle...")
        env2 = UnityPy.load(BUNDLE_PATH)
        for obj2 in env2.objects:
            if obj2.type.name == "Material":
                data2 = obj2.read()
                if "Felix" in data2.m_Name:
                    print(f"  Verified - RenderQueue: {data2.m_CustomRenderQueue}")
                    print(f"  Verified - Keywords: {data2.m_ShaderKeywords}")
                    if hasattr(data2, 'm_SavedProperties'):
                        props2 = data2.m_SavedProperties
                        float_count = len(props2.m_Floats) if hasattr(props2, 'm_Floats') and props2.m_Floats else 0
                        print(f"  Verified - Floats: {float_count}")

if __name__ == "__main__":
    fix_material()

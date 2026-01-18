import UnityPy
import os
import shutil

BUNDLE_PATH = r"C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\StreamingAssets\aa\StandaloneWindows64\felix_assets_all_8ce5cfff23e50dfcaa729aa03940bfd7.bundle"
BACKUP_PATH = BUNDLE_PATH + ".backup_material"

def fix_material():
    # Create backup if not exists
    if not os.path.exists(BACKUP_PATH):
        print(f"Creating backup: {BACKUP_PATH}")
        shutil.copy2(BUNDLE_PATH, BACKUP_PATH)

    print(f"Loading bundle: {BUNDLE_PATH}")
    env = UnityPy.load(BUNDLE_PATH)

    modified = False

    for obj in env.objects:
        if obj.type.name == "Material":
            data = obj.read()
            mat_name = data.m_Name

            print(f"\nFound material: {mat_name}")

            # Check if this is the Felix material
            if "Felix" in mat_name or "ThePack" in mat_name:
                print(f"  Current RenderQueue: {data.m_CustomRenderQueue}")
                print(f"  Current Keywords: {data.m_ShaderKeywords}")

                # Fix RenderQueue
                if data.m_CustomRenderQueue != 2450:
                    print(f"  -> Setting RenderQueue to 2450")
                    data.m_CustomRenderQueue = 2450
                    modified = True

                # Fix shader keywords
                keywords = data.m_ShaderKeywords if data.m_ShaderKeywords else ""
                if "_ALPHATEST_ON" not in keywords:
                    if keywords:
                        new_keywords = keywords + " _ALPHATEST_ON"
                    else:
                        new_keywords = "_ALPHATEST_ON"
                    print(f"  -> Setting Keywords to: {new_keywords}")
                    data.m_ShaderKeywords = new_keywords
                    modified = True

                # Remove _LIGHT_COOKIES if present
                if "_LIGHT_COOKIES" in data.m_ShaderKeywords:
                    new_keywords = data.m_ShaderKeywords.replace("_LIGHT_COOKIES", "").strip()
                    print(f"  -> Removing _LIGHT_COOKIES, new keywords: {new_keywords}")
                    data.m_ShaderKeywords = new_keywords
                    modified = True

                # Save changes back to object
                obj.save(data)
                print(f"  Material modified!")

    if modified:
        print(f"\nSaving modified bundle...")

        # Get the container/bundle
        for path, container in env.container.items():
            print(f"  Container: {path}")

        # Save the bundle
        with open(BUNDLE_PATH, "wb") as f:
            f.write(env.file.save())

        print(f"Bundle saved successfully!")
    else:
        print("\nNo modifications needed.")

if __name__ == "__main__":
    fix_material()

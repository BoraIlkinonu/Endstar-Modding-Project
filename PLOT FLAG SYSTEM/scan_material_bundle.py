import UnityPy
import json

BUNDLE_PATH = r"C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\StreamingAssets\aa\StandaloneWindows64\felix_assets_all_8ce5cfff23e50dfcaa729aa03940bfd7.bundle"

def scan_materials():
    print(f"Loading bundle: {BUNDLE_PATH}")
    env = UnityPy.load(BUNDLE_PATH)

    for obj in env.objects:
        if obj.type.name == "Material":
            data = obj.read()
            mat_name = data.m_Name

            print(f"\n{'='*60}")
            print(f"Material: {mat_name}")
            print(f"{'='*60}")

            # Basic properties
            print(f"  m_CustomRenderQueue: {data.m_CustomRenderQueue}")
            print(f"  m_ShaderKeywords: {data.m_ShaderKeywords}")

            # Shader reference
            if hasattr(data, 'm_Shader') and data.m_Shader:
                shader = data.m_Shader.read()
                print(f"  Shader: {shader.m_Name if hasattr(shader, 'm_Name') else 'Unknown'}")

            # Saved properties
            if hasattr(data, 'm_SavedProperties'):
                props = data.m_SavedProperties

                # Floats
                if hasattr(props, 'm_Floats') and props.m_Floats:
                    print(f"\n  Float Properties:")
                    for item in props.m_Floats:
                        if hasattr(item, 'first') and hasattr(item, 'second'):
                            print(f"    {item.first}: {item.second}")

                # Colors
                if hasattr(props, 'm_Colors') and props.m_Colors:
                    print(f"\n  Color Properties:")
                    for item in props.m_Colors:
                        if hasattr(item, 'first') and hasattr(item, 'second'):
                            col = item.second
                            if hasattr(col, 'r'):
                                print(f"    {item.first}: RGBA({col.r:.2f}, {col.g:.2f}, {col.b:.2f}, {col.a:.2f})")

                # Textures
                if hasattr(props, 'm_TexEnvs') and props.m_TexEnvs:
                    print(f"\n  Texture Properties:")
                    for item in props.m_TexEnvs:
                        if hasattr(item, 'first') and hasattr(item, 'second'):
                            tex_env = item.second
                            tex_name = "NULL"
                            if hasattr(tex_env, 'm_Texture') and tex_env.m_Texture:
                                try:
                                    tex = tex_env.m_Texture.read()
                                    tex_name = tex.m_Name if hasattr(tex, 'm_Name') else "Unknown"
                                except:
                                    tex_name = "Error reading"
                            print(f"    {item.first}: {tex_name}")

if __name__ == "__main__":
    scan_materials()

"""
Export UV layout from OBJ file to a visual guide image
"""
import re
from PIL import Image, ImageDraw

def parse_obj_uvs(obj_path):
    """Parse UV coordinates from OBJ file"""
    uvs = []
    faces_uv = []

    with open(obj_path, 'r') as f:
        for line in f:
            line = line.strip()
            if line.startswith('vt '):
                # UV coordinate: vt u v
                parts = line.split()
                u = float(parts[1])
                v = float(parts[2])
                uvs.append((u, v))
            elif line.startswith('f '):
                # Face: f v/vt/vn v/vt/vn v/vt/vn ...
                parts = line.split()[1:]
                face_uvs = []
                for part in parts:
                    indices = part.split('/')
                    if len(indices) >= 2 and indices[1]:
                        uv_idx = int(indices[1]) - 1  # OBJ is 1-indexed
                        face_uvs.append(uv_idx)
                if face_uvs:
                    faces_uv.append(face_uvs)

    return uvs, faces_uv

def draw_uv_layout(uvs, faces_uv, output_path, size=2048, texture_path=None):
    """Draw UV layout as image"""

    # Create image with texture background if provided
    if texture_path:
        try:
            img = Image.open(texture_path).convert('RGBA')
            img = img.resize((size, size))
        except:
            img = Image.new('RGBA', (size, size), (40, 40, 40, 255))
    else:
        img = Image.new('RGBA', (size, size), (40, 40, 40, 255))

    draw = ImageDraw.Draw(img)

    # Draw UV wireframe
    for face in faces_uv:
        if len(face) >= 3:
            points = []
            for uv_idx in face:
                if 0 <= uv_idx < len(uvs):
                    u, v = uvs[uv_idx]
                    # Convert UV to pixel coordinates
                    # UV origin is bottom-left, image origin is top-left
                    x = int(u * size)
                    y = int((1 - v) * size)  # Flip V
                    points.append((x, y))

            # Draw polygon edges
            if len(points) >= 3:
                for i in range(len(points)):
                    p1 = points[i]
                    p2 = points[(i + 1) % len(points)]
                    draw.line([p1, p2], fill=(0, 255, 0, 200), width=1)

    img.save(output_path)
    print(f"UV layout saved to: {output_path}")

def main():
    obj_path = r"D:\Endstar Plot Flag\PLOT FLAG SYSTEM\Felix\Mesh\TigerGuy_LOD0.obj"
    texture_path = r"D:\Endstar Plot Flag\PLOT FLAG SYSTEM\Felix\Texture2D\Tiger_Orange_Albedo.png"
    output_path = r"D:\Endstar Plot Flag\PLOT FLAG SYSTEM\Felix_UV_Layout.png"

    print("Parsing OBJ file...")
    uvs, faces_uv = parse_obj_uvs(obj_path)
    print(f"Found {len(uvs)} UV coordinates and {len(faces_uv)} faces")

    print("Drawing UV layout...")
    draw_uv_layout(uvs, faces_uv, output_path, size=2048, texture_path=texture_path)

    # Also save without texture background
    output_path_clean = r"D:\Endstar Plot Flag\PLOT FLAG SYSTEM\Felix_UV_Wireframe.png"
    draw_uv_layout(uvs, faces_uv, output_path_clean, size=2048, texture_path=None)
    print(f"Clean wireframe saved to: {output_path_clean}")

if __name__ == "__main__":
    main()

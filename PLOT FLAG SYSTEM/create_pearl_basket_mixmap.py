"""
Create a MixMap texture for Pearl Basket prop matching Endstar's format.

MixMap Channel Layout (from Anachronists_Treasure analysis):
  R = Metallic
  G = AO (Ambient Occlusion) - Pearl Basket doesn't have AO, use white (255)
  B = Unused (constant white 255)
  A = Smoothness (inverted Roughness)
"""
from PIL import Image
import numpy as np
import os

# Input paths (Pearl Basket textures)
input_dir = r"D:\Endstar Plot Flag\PLOT FLAG SYSTEM\Extracted_Props\Pearl Basket - Low poly textured"
metallic_path = os.path.join(input_dir, "metallic.png")
roughness_path = os.path.join(input_dir, "roughness.png")

# Also copy to Unity project
unity_textures_dir = r"D:\Unity_Workshop\Endstar Custom Shader\Assets\CustomProps\Textures"

# Output path
output_path = os.path.join(input_dir, "PearlBasket_Mix.png")
unity_output_path = os.path.join(unity_textures_dir, "PearlBasket_Mix.png")

print("=== Creating Pearl Basket MixMap ===")
print()

# Load metallic texture
if os.path.exists(metallic_path):
    metallic_img = Image.open(metallic_path).convert('L')  # Grayscale
    print(f"Loaded Metallic: {metallic_path}")
    print(f"  Size: {metallic_img.size}")
else:
    print(f"WARNING: Metallic texture not found: {metallic_path}")
    metallic_img = None

# Load roughness texture
if os.path.exists(roughness_path):
    roughness_img = Image.open(roughness_path).convert('L')  # Grayscale
    print(f"Loaded Roughness: {roughness_path}")
    print(f"  Size: {roughness_img.size}")
else:
    print(f"WARNING: Roughness texture not found: {roughness_path}")
    roughness_img = None

# Determine output size (use metallic or roughness size)
if metallic_img:
    size = metallic_img.size
elif roughness_img:
    size = roughness_img.size
else:
    print("ERROR: No input textures found!")
    exit(1)

print(f"\nOutput size: {size}")

# Resize images if needed
if metallic_img and metallic_img.size != size:
    metallic_img = metallic_img.resize(size, Image.LANCZOS)
if roughness_img and roughness_img.size != size:
    roughness_img = roughness_img.resize(size, Image.LANCZOS)

# Convert to numpy arrays
metallic_arr = np.array(metallic_img) if metallic_img else np.zeros(size[::-1], dtype=np.uint8)
roughness_arr = np.array(roughness_img) if roughness_img else np.full(size[::-1], 128, dtype=np.uint8)

# Create channels
print("\nCreating channels:")

# R = Metallic (direct copy)
r_channel = metallic_arr
print(f"  R (Metallic): min={r_channel.min()}, max={r_channel.max()}, mean={r_channel.mean():.1f}")

# G = AO (we don't have AO, use white = 255 = no occlusion)
# Could generate simple AO from mesh but for now use constant
g_channel = np.full(size[::-1], 255, dtype=np.uint8)
print(f"  G (AO): constant 255 (no occlusion)")

# B = Unused (constant white 255)
b_channel = np.full(size[::-1], 255, dtype=np.uint8)
print(f"  B (Unused): constant 255")

# A = Smoothness (inverted Roughness: smoothness = 255 - roughness)
a_channel = 255 - roughness_arr
print(f"  A (Smoothness): min={a_channel.min()}, max={a_channel.max()}, mean={a_channel.mean():.1f}")

# Stack channels into RGBA
mixmap_arr = np.stack([r_channel, g_channel, b_channel, a_channel], axis=2)

# Create image
mixmap_img = Image.fromarray(mixmap_arr, mode='RGBA')

# Save
mixmap_img.save(output_path)
print(f"\n=== OUTPUT ===")
print(f"Saved: {output_path}")

# Also save to Unity project
os.makedirs(unity_textures_dir, exist_ok=True)
mixmap_img.save(unity_output_path)
print(f"Saved: {unity_output_path}")

# Summary
print(f"\n=== SUMMARY ===")
print(f"MixMap created with channels:")
print(f"  R = Metallic (from metallic.png)")
print(f"  G = AO (constant 255, no occlusion)")
print(f"  B = Unused (constant 255)")
print(f"  A = Smoothness (inverted roughness.png)")
print(f"\nSize: {size[0]}x{size[1]}")
print(f"Output format: RGBA PNG")

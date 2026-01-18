"""
Analyze the MixMap texture from Endstar to understand channel layout.
Typically MixMaps pack: R=Metallic, G=AO, B=Detail/Height, A=Smoothness
"""
from PIL import Image
import numpy as np
import os

# Path to the game's MixMap texture
mixmap_path = r"D:\Endstar Plot Flag\PLOT FLAG SYSTEM\Extracted_Props\Anachronists_Treasure_Mix.png"

if not os.path.exists(mixmap_path):
    print(f"ERROR: File not found: {mixmap_path}")
    exit(1)

# Load the image
img = Image.open(mixmap_path)
print(f"Image: {mixmap_path}")
print(f"Size: {img.size}")
print(f"Mode: {img.mode}")
print(f"Format: {img.format}")
print()

# Convert to numpy array
arr = np.array(img)
print(f"Array shape: {arr.shape}")
print()

# Analyze each channel
channels = ['R (Red)', 'G (Green)', 'B (Blue)', 'A (Alpha)']
channel_data = {}

for i, name in enumerate(channels):
    if i < arr.shape[2]:
        channel = arr[:, :, i]
        channel_data[name] = {
            'min': int(np.min(channel)),
            'max': int(np.max(channel)),
            'mean': float(np.mean(channel)),
            'std': float(np.std(channel)),
            'unique_count': len(np.unique(channel))
        }

print("=== CHANNEL ANALYSIS ===")
print()
for name, data in channel_data.items():
    print(f"{name}:")
    print(f"  Min: {data['min']}, Max: {data['max']}")
    print(f"  Mean: {data['mean']:.2f}, StdDev: {data['std']:.2f}")
    print(f"  Unique values: {data['unique_count']}")

    # Interpretation based on values
    if data['mean'] < 30:
        interpretation = "Likely METALLIC (mostly non-metal, low values)"
    elif data['mean'] > 200:
        interpretation = "Likely SMOOTHNESS or AO (high values = smooth/no occlusion)"
    elif data['std'] < 20:
        interpretation = "Uniform channel - could be constant value"
    else:
        interpretation = "Variable channel - could be AO, Detail, or Height"

    print(f"  Interpretation: {interpretation}")
    print()

# Save individual channels as grayscale images for visual inspection
output_dir = r"D:\Endstar Plot Flag\PLOT FLAG SYSTEM\Extracted_Props\MixMap_Channels"
os.makedirs(output_dir, exist_ok=True)

channel_names = ['R_channel', 'G_channel', 'B_channel', 'A_channel']
for i, name in enumerate(channel_names):
    if i < arr.shape[2]:
        channel_img = Image.fromarray(arr[:, :, i], mode='L')
        output_path = os.path.join(output_dir, f"Anachronists_Treasure_Mix_{name}.png")
        channel_img.save(output_path)
        print(f"Saved: {output_path}")

print()
print("=== COMMON MIXMAP LAYOUTS ===")
print("Layout 1 (Unity Standard): R=Metallic, G=AO, B=DetailMask, A=Smoothness")
print("Layout 2 (URP): R=Metallic, G=Occlusion, B=Detail, A=Smoothness")
print("Layout 3 (HDRP): R=Metallic, G=AO, B=DetailMask, A=Smoothness")
print()
print("Based on analysis, most likely layout for this texture:")

# Determine most likely layout
r_data = channel_data.get('R (Red)', {})
g_data = channel_data.get('G (Green)', {})
b_data = channel_data.get('B (Blue)', {})
a_data = channel_data.get('A (Alpha)', {})

print(f"  R channel (mean={r_data.get('mean', 0):.0f}): ", end="")
if r_data.get('mean', 0) < 50:
    print("METALLIC (low = non-metallic)")
else:
    print("Unknown")

print(f"  G channel (mean={g_data.get('mean', 0):.0f}): ", end="")
if g_data.get('mean', 0) > 150:
    print("AO/OCCLUSION (high = no occlusion)")
else:
    print("AO/OCCLUSION (has shadow areas)")

print(f"  B channel (mean={b_data.get('mean', 0):.0f}): ", end="")
print("DETAIL/HEIGHT/EMISSION MASK")

print(f"  A channel (mean={a_data.get('mean', 0):.0f}): ", end="")
if a_data.get('mean', 0) > 100:
    print("SMOOTHNESS (high = smooth/glossy)")
else:
    print("SMOOTHNESS (low = rough)")

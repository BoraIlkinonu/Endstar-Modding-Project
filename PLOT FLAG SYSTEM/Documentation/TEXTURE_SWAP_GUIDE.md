# Felix Texture Swap Guide

## Tools Ready
- UABEA: `D:\Endstar Plot Flag\PLOT FLAG SYSTEM\UABEA\UABEAvalonia.exe`
- Original textures: `D:\Endstar Plot Flag\PLOT FLAG SYSTEM\Felix\Texture2D\`

## Step 1: Edit Your Texture

1. Open `Tiger_Orange_Albedo.png` in Photoshop/GIMP/Paint.NET
2. Modify the colors, patterns, etc.
3. **Keep the same dimensions** (check original size)
4. Save as PNG

## Step 2: Backup Original Bundle

Before modifying, backup the original:
```
Source: C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\StreamingAssets\aa\StandaloneWindows64\felix_assets_all_8ce5cfff23e50dfcaa729aa03940bfd7.bundle

Backup to: D:\Endstar Plot Flag\PLOT FLAG SYSTEM\felix_original.bundle
```

## Step 3: Replace Texture with UABEA

1. **Launch UABEA**
   ```
   D:\Endstar Plot Flag\PLOT FLAG SYSTEM\UABEA\UABEAvalonia.exe
   ```

2. **Open the bundle**
   - File > Open
   - Navigate to: `C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\StreamingAssets\aa\StandaloneWindows64\`
   - Select: `felix_assets_all_8ce5cfff23e50dfcaa729aa03940bfd7.bundle`

3. **Open the .assets file inside**
   - You'll see a list of files in the bundle
   - Double-click the main `.assets` file (usually `CAB-xxx`)

4. **Find the texture**
   - Look for `Texture2D` type assets
   - Find `Tiger_Orange_Albedo`

5. **Replace the texture**
   - Select the texture asset
   - Plugins > Edit Texture (or right-click > Plugins > Edit)
   - Click "Load" and select your modified PNG
   - Click "Save"

6. **Save the bundle**
   - File > Save
   - Or File > Save As (to a new location first for safety)

## Step 4: Test In Game

1. Make sure the modified bundle is in the game folder
2. Launch Endstar
3. Your Felix should now have the new texture!

## Troubleshooting

| Issue | Solution |
|-------|----------|
| Texture looks wrong | Check PNG dimensions match original |
| Game crashes | Restore backup bundle |
| No change visible | Clear game cache, verify bundle replaced |
| UABEA won't open bundle | Try "File > Open As Bundle File" |

## Quick Texture Info

| Texture | Purpose | Modify? |
|---------|---------|---------|
| Tiger_Orange_Albedo.png | Main color/appearance | YES |
| Tiger_MixMap.png | Metal/Rough/AO | Optional |
| Tiger_Orange_Emissive.png | Glow effects | Optional |

## Notes

- The Albedo texture is a UV atlas - all body parts are laid out on one image
- Keep the UV layout identical - only change colors/patterns
- Test with small changes first before major edits

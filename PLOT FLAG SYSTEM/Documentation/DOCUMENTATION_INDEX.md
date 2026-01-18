# Endstar Modding Documentation Index

## Quick Reference

| Document | Purpose |
|----------|---------|
| [COMPLETE_CHARACTER_INJECTION_PROTOCOL.md](../COMPLETE_CHARACTER_INJECTION_PROTOCOL.md) | **Master guide** - Full protocol with CRC mapping |
| [BUNDLE_CRC_MAPPING_DOCUMENTATION.md](../BUNDLE_CRC_MAPPING_DOCUMENTATION.md) | Bundle CRC validation status for all 45 characters |
| [PROP_RESKIN_GUIDE.md](PROP_RESKIN_GUIDE.md) | **Prop reskinning** - Texture-only modification |
| [PROP_REPLACEMENT_PROTOCOL.md](PROP_REPLACEMENT_PROTOCOL.md) | **Prop replacement** - Full protocol with UABEA import |
| [CUSTOM_PROPS_PLUGIN_DESIGN.md](CUSTOM_PROPS_PLUGIN_DESIGN.md) | **Add new props** - BepInEx plugin for unlimited custom props |
| [SILHOUETTE_FIX_INVESTIGATION.md](SILHOUETTE_FIX_INVESTIGATION.md) | Root cause analysis of silhouette issue |
| [CHARACTER_INJECTION_WORKFLOW.md](CHARACTER_INJECTION_WORKFLOW.md) | Step-by-step injection workflow |

---

## Key Findings Summary

### ALL Bundles Have CRC Validation

**CORRECTED 2026-01-04:** ALL 45 bundles have CRC validation. The previous analysis script had a UTF-16LE alignment bug that missed half the CRC entries.

- Felix CRC: `1004267194`
- Felix Hash: `8ce5cfff23e50dfcaa729aa03940bfd7`

**Implication:** Bundle replacement REQUIRES catalog CRC patching for ALL bundles, including Felix.

### Silhouette Issue Root Cause

Custom character appears as silhouette because the shader has 5 passes instead of 6 (missing GBuffer).

**Fix:** In Unity URP settings, set BOTH:
- `URP-HighFidelity.asset`: `m_PrefilteringModeDeferredRendering: 1`
- `URP-HighFidelity-Renderer.asset`: `m_RenderingMode: 1` (Deferred)

### Props (Treasure) - No CRC Validation

**DISCOVERED 2026-01-06:** Props in `sharedassets0.assets` do NOT have CRC validation.

- Treasure assets: `Anachronists_Treasure`, `Ancient Aliens`, `Terran` variants
- Components: Mesh (`_Treasure`), Textures (`_D`, `_N`, `_Mix`, `_E`), Material (`_mat`)
- Modification: Use UABEA directly - no catalog patching required
- Much simpler than character injection

### Tool Warning: UnityPy Corrupts Files

**DO NOT use UnityPy for modifications.** UnityPy is fine for extraction/reading, but corrupts assets when writing back.

| Tool | Read/Extract | Write/Modify |
|------|--------------|--------------|
| UnityPy | OK | **CORRUPTS** |
| UABEA | OK | **USE THIS** |
| AssetStudio | OK | Read-only |

---

## Documentation Files

### Master Guides
- **COMPLETE_CHARACTER_INJECTION_PROTOCOL.md** - Complete protocol with all technical details
- **BUNDLE_CRC_MAPPING_DOCUMENTATION.md** - Full CRC mapping for all bundles

### Investigation Reports
- **SILHOUETTE_FIX_INVESTIGATION.md** - Root cause analysis
- **CUSTOM_CHARACTER_INJECTION_DOCUMENTATION.md** - Earlier investigation notes

### Workflow Guides
- **CHARACTER_INJECTION_WORKFLOW.md** - Step-by-step injection process
- **BLENDER_CHARACTER_SETUP_GUIDE.md** - Blender mesh preparation guide
- **MOD_INSTALLER_GUIDE.md** - Mod installer usage and multi-character mods
- **PROP_RESKIN_GUIDE.md** - Treasure and collectible prop reskinning (textures only)
- **PROP_REPLACEMENT_PROTOCOL.md** - Full prop replacement with Blender + UABEA workflow
- **CUSTOM_PROPS_PLUGIN_DESIGN.md** - BepInEx plugin for adding unlimited new props
- **ASSETSTUDIO_EXTRACTION_GUIDE.md** - Asset extraction instructions
- **TEXTURE_SWAP_GUIDE.md** - Texture replacement guide

### Reference Documentation
- **Endstar_Wiki_Documentation.md** - Game wiki information
- **Endstar_API_Documentation.md** - API documentation
- **Endstar Props Properties_Parameters_IMPROVED.md** - Props parameters

---

## Tool Locations

| Tool | Path | Purpose |
|------|------|---------|
| AssetStudio | `Tools\AssetStudio-master\AssetStudioGUI\bin\Release\` | Asset extraction |
| UABEA | `Tools\UABEA\` | Bundle editing |
| analyze_catalog.py | `analyze_catalog.py` | Catalog CRC analysis |
| extract_all_crc.py | `extract_all_crc.py` | Extract all CRC entries |
| **deploy_felix_bundle.py** | `deploy_felix_bundle.py` | **Deploy bundle + CRC patch** |
| **patch_character_identity.py** | `patch_character_identity.py` | **Patch name + portrait** |
| CameraDump plugin | `Zayed_Character_Mod\CameraDump\` | Runtime debugging |
| SilhouetteFix plugin | `Zayed_Character_Mod\SilhouetteFix\` | Material fix attempts |

---

## Critical File Paths

### Game Installation
```
C:\Endless Studios\Endless Launcher\Endstar\
├── Endstar_Data\
│   ├── StreamingAssets\aa\catalog.json
│   ├── StreamingAssets\aa\StandaloneWindows64\  (bundles)
│   └── Managed\  (assemblies)
└── BepInEx\plugins\  (mods)
```

### Unity Project
```
D:\Unity_Workshop\Endstar Custom Shader\
├── Assets\Settings\URP-HighFidelity.asset  (URP settings)
└── Assets\Prefabs\CharacterCosmetics\  (character prefabs)
```

---

## Changelog

- **2026-01-06:** Added CUSTOM_PROPS_PLUGIN_DESIGN.md - BepInEx plugin architecture for unlimited props
- **2026-01-06:** Added PROP_REPLACEMENT_PROTOCOL.md - Complete prop replacement with UABEA import
- **2026-01-06:** WARNING: UnityPy corrupts files when writing - use UABEA for all modifications
- **2026-01-06:** Added PROP_RESKIN_GUIDE.md - Treasure/collectible modification without CRC patching
- **2026-01-06:** Discovered sharedassets0.assets has NO CRC validation (simpler prop modding)
- **2026-01-04:** **FULL SUCCESS** - Complete character replacement working (3D model + name + portrait)
- **2026-01-04:** Added `deploy_felix_bundle.py` - automated bundle deployment
- **2026-01-04:** Added `patch_character_identity.py` - automated name/portrait patching
- **2026-01-04:** Fixed silhouette: requires BOTH `m_PrefilteringModeDeferredRendering: 1` AND `m_RenderingMode: 1`
- **2026-01-04:** MAJOR CORRECTION - ALL 45 bundles have CRC validation. The extract_all_crc.py script had a UTF-16LE alignment bug that missed half the entries. Fixed.
- **2026-01-04:** Felix CRC confirmed: `1004267194`
- **2026-01-04:** Identified silhouette root cause: missing GBuffer pass
- **2026-01-04:** Created master protocol document

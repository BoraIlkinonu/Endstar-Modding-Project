//////////////////////////////////////////
//
// NOTE: This is *not* a valid shader file
//
///////////////////////////////////////////
Shader "Shader Graphs/Endless_Shader_Character_NoFade" {
    Properties {
        _Albedo ("Albedo", 2D) = "white" { }
        Albedo_Tint ("Albedo Tint", Color) = (1,1,1,0)
        Texture2D_033410430c1e41f38cbe37cd7347362e ("Mask Map", 2D) = "white" { }
        Texture2D_0ec0db81fb95430191ba9b122120a097 ("Normal", 2D) = "bump" { }
        Vector1_844a91d1120546378bc600c39ce4fc77 ("Metallic", Range(0, 1)) = 0
        Vector1_195718c9f68f499782987983967ae661 ("Smoothness", Range(0, 1)) = 0
        _Emissive_Map ("Emissive Map", 2D) = "white" { }
        _EmissionColor ("Emissive Color", Color) = (0,0,0,0)
        Vector1_6639e655f62c4bf0878a285950a9ad38 ("Clip Threshold", Range(0, 1)) = 0.01
        _Height ("Height", 2D) = "white" { }
        _Height_Intensity ("Height Intensity", Float) = 0
        _Emissive_Cracks_Color ("Emissive Cracks Color", Color) = (0,23.96863,5.834309,0)
        _Emissive_Cracks_Amount ("Emissive Cracks Amount", Range(0, 1)) = 0
        [Toggle(_LIGHT_COOKIES)] _LIGHT_COOKIES ("_LIGHT_COOKIES", Float) = 1
        _SSS_Color ("SSS Color", Color) = (1,0.3929976,0.2358489,0)
        _SSS_Power ("SSS Power", Range(0.5, 10)) = 0.5
        _SSS_Intensity ("SSS Intensity", Range(0, 100)) = 0
        _SSS_Normal_Influence ("SSS Normal Influence", Range(0, 1)) = 0
        HURT_FLASH ("HurtFlash", Range(0, 1)) = 0
        _Rim_Intensity ("Rim Intensity", Float) = 1
        _Rim_Power ("Rim Power", Float) = 5
        Selection_Color ("SelectionColor", Color) = (8,0,0,0)
        _Pre_Integrated_Scattering ("Pre Integrated Scattering", 2D) = "white" { }
        _PS_Multiply ("PS - Multiply", Float) = 0.5
        _PS_Add ("PS - Add", Float) = 0.5
        _WorkflowMode ("_WorkflowMode", Float) = 1
        _CastShadows ("_CastShadows", Float) = 1
        _ReceiveShadows ("_ReceiveShadows", Float) = 1
        _Surface ("_Surface", Float) = 0
        _Blend ("_Blend", Float) = 0
        _AlphaClip ("_AlphaClip", Float) = 1
        _BlendModePreserveSpecular ("_BlendModePreserveSpecular", Float) = 0
        _SrcBlend ("_SrcBlend", Float) = 1
        _DstBlend ("_DstBlend", Float) = 0
        [ToggleUI] _ZWrite ("_ZWrite", Float) = 1
        _ZWriteControl ("_ZWriteControl", Float) = 0
        _ZTest ("_ZTest", Float) = 4
        _Cull ("_Cull", Float) = 2
        _AlphaToMask ("_AlphaToMask", Float) = 1
        _QueueOffset ("_QueueOffset", Float) = 0
        _QueueControl ("_QueueControl", Float) = -1
        unity_Lightmaps ("unity_Lightmaps", 2DArray) = "" { }
        unity_LightmapsInd ("unity_LightmapsInd", 2DArray) = "" { }
        unity_ShadowMasks ("unity_ShadowMasks", 2DArray) = "" { }
    }
    SubShader {
        Tags { "DisableBatching" = "False" "QUEUE" = "AlphaTest" "RenderPipeline" = "UniversalPipeline" "RenderType" = "Opaque" "ShaderGraphShader" = "true" "ShaderGraphTargetId" = "UniversalLitSubTarget" "UniversalMaterialType" = "Lit" }
        Pass {
            Name "Universal Forward"
            Tags { "DisableBatching" = "False" "LIGHTMODE" = "UniversalForward" "QUEUE" = "AlphaTest" "RenderPipeline" = "UniversalPipeline" "RenderType" = "Opaque" "ShaderGraphShader" = "true" "ShaderGraphTargetId" = "UniversalLitSubTarget" "UniversalMaterialType" = "Lit" }
            Blend Zero Zero, Zero Zero
            ZTest Off
            ZWrite Off
            Cull Off
            GpuProgramID 32578
            PlayerProgram "vp" {
                SubProgram "d3d11 " {
                    Keywords { "_ADDITIONAL_LIGHTS" "_MAIN_LIGHT_SHADOWS" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "_ADDITIONAL_LIGHTS" "_LIGHT_COOKIES" "_MAIN_LIGHT_SHADOWS" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "INSTANCING_ON" "_ADDITIONAL_LIGHTS" "_MAIN_LIGHT_SHADOWS" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "INSTANCING_ON" "_ADDITIONAL_LIGHTS" "_LIGHT_COOKIES" "_MAIN_LIGHT_SHADOWS" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "INSTANCING_ON" "_ADDITIONAL_LIGHTS" "_LIGHT_COOKIES" "_MAIN_LIGHT_SHADOWS_SCREEN" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "INSTANCING_ON" "_ADDITIONAL_LIGHTS" "_MAIN_LIGHT_SHADOWS_SCREEN" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "_ADDITIONAL_LIGHTS" "_LIGHT_COOKIES" "_MAIN_LIGHT_SHADOWS_SCREEN" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "_ADDITIONAL_LIGHTS" "_MAIN_LIGHT_SHADOWS_SCREEN" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "INSTANCING_ON" "_ADDITIONAL_LIGHTS" "_LIGHT_COOKIES" "_MAIN_LIGHT_SHADOWS_CASCADE" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "INSTANCING_ON" "_ADDITIONAL_LIGHTS" "_MAIN_LIGHT_SHADOWS_CASCADE" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "_ADDITIONAL_LIGHTS" "_LIGHT_COOKIES" "_MAIN_LIGHT_SHADOWS_CASCADE" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "_ADDITIONAL_LIGHTS" "_MAIN_LIGHT_SHADOWS_CASCADE" }
                    "// shader disassembly not supported on DXBC"
                }
            }
            PlayerProgram "fp" {
                SubProgram "d3d11 " {
                    Keywords { "INSTANCING_ON" "_ADDITIONAL_LIGHTS" "_ADDITIONAL_LIGHT_SHADOWS" "_DBUFFER_MRT3" "_LIGHT_COOKIES" "_MAIN_LIGHT_SHADOWS_SCREEN" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" "_SCREEN_SPACE_OCCLUSION" "_SHADOWS_SOFT" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "INSTANCING_ON" "_ADDITIONAL_LIGHTS" "_ADDITIONAL_LIGHT_SHADOWS" "_DBUFFER_MRT3" "_LIGHT_COOKIES" "_MAIN_LIGHT_SHADOWS_SCREEN" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" "_SCREEN_SPACE_OCCLUSION" "_SHADOWS_SOFT" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "INSTANCING_ON" "_ADDITIONAL_LIGHTS" "_ADDITIONAL_LIGHT_SHADOWS" "_DBUFFER_MRT3" "_MAIN_LIGHT_SHADOWS_SCREEN" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" "_SCREEN_SPACE_OCCLUSION" "_SHADOWS_SOFT" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "INSTANCING_ON" "_ADDITIONAL_LIGHTS" "_ADDITIONAL_LIGHT_SHADOWS" "_DBUFFER_MRT3" "_MAIN_LIGHT_SHADOWS_SCREEN" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" "_SCREEN_SPACE_OCCLUSION" "_SHADOWS_SOFT" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "_ADDITIONAL_LIGHTS" "_ADDITIONAL_LIGHT_SHADOWS" "_DBUFFER_MRT3" "_LIGHT_COOKIES" "_MAIN_LIGHT_SHADOWS_SCREEN" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" "_SCREEN_SPACE_OCCLUSION" "_SHADOWS_SOFT" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "_ADDITIONAL_LIGHTS" "_ADDITIONAL_LIGHT_SHADOWS" "_DBUFFER_MRT3" "_LIGHT_COOKIES" "_MAIN_LIGHT_SHADOWS_SCREEN" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" "_SCREEN_SPACE_OCCLUSION" "_SHADOWS_SOFT" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "_ADDITIONAL_LIGHTS" "_ADDITIONAL_LIGHT_SHADOWS" "_DBUFFER_MRT3" "_MAIN_LIGHT_SHADOWS_SCREEN" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" "_SCREEN_SPACE_OCCLUSION" "_SHADOWS_SOFT" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "_ADDITIONAL_LIGHTS" "_ADDITIONAL_LIGHT_SHADOWS" "_DBUFFER_MRT3" "_MAIN_LIGHT_SHADOWS_SCREEN" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" "_SCREEN_SPACE_OCCLUSION" "_SHADOWS_SOFT" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "INSTANCING_ON" "_ADDITIONAL_LIGHTS" "_ADDITIONAL_LIGHT_SHADOWS" "_DBUFFER_MRT3" "_LIGHT_COOKIES" "_MAIN_LIGHT_SHADOWS_CASCADE" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" "_SCREEN_SPACE_OCCLUSION" "_SHADOWS_SOFT" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "INSTANCING_ON" "_ADDITIONAL_LIGHTS" "_ADDITIONAL_LIGHT_SHADOWS" "_DBUFFER_MRT3" "_LIGHT_COOKIES" "_MAIN_LIGHT_SHADOWS_CASCADE" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" "_SCREEN_SPACE_OCCLUSION" "_SHADOWS_SOFT" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "INSTANCING_ON" "_ADDITIONAL_LIGHTS" "_ADDITIONAL_LIGHT_SHADOWS" "_DBUFFER_MRT3" "_MAIN_LIGHT_SHADOWS_CASCADE" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" "_SCREEN_SPACE_OCCLUSION" "_SHADOWS_SOFT" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "INSTANCING_ON" "_ADDITIONAL_LIGHTS" "_ADDITIONAL_LIGHT_SHADOWS" "_DBUFFER_MRT3" "_MAIN_LIGHT_SHADOWS_CASCADE" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" "_SCREEN_SPACE_OCCLUSION" "_SHADOWS_SOFT" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "_ADDITIONAL_LIGHTS" "_ADDITIONAL_LIGHT_SHADOWS" "_DBUFFER_MRT3" "_LIGHT_COOKIES" "_MAIN_LIGHT_SHADOWS_CASCADE" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" "_SCREEN_SPACE_OCCLUSION" "_SHADOWS_SOFT" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "_ADDITIONAL_LIGHTS" "_ADDITIONAL_LIGHT_SHADOWS" "_DBUFFER_MRT3" "_LIGHT_COOKIES" "_MAIN_LIGHT_SHADOWS_CASCADE" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" "_SCREEN_SPACE_OCCLUSION" "_SHADOWS_SOFT" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "_ADDITIONAL_LIGHTS" "_ADDITIONAL_LIGHT_SHADOWS" "_DBUFFER_MRT3" "_MAIN_LIGHT_SHADOWS_CASCADE" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" "_SCREEN_SPACE_OCCLUSION" "_SHADOWS_SOFT" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "_ADDITIONAL_LIGHTS" "_ADDITIONAL_LIGHT_SHADOWS" "_DBUFFER_MRT3" "_MAIN_LIGHT_SHADOWS_CASCADE" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" "_SCREEN_SPACE_OCCLUSION" "_SHADOWS_SOFT" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "INSTANCING_ON" "_ADDITIONAL_LIGHTS" "_ADDITIONAL_LIGHT_SHADOWS" "_DBUFFER_MRT3" "_LIGHT_COOKIES" "_MAIN_LIGHT_SHADOWS" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" "_SCREEN_SPACE_OCCLUSION" "_SHADOWS_SOFT" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "INSTANCING_ON" "_ADDITIONAL_LIGHTS" "_ADDITIONAL_LIGHT_SHADOWS" "_DBUFFER_MRT3" "_LIGHT_COOKIES" "_MAIN_LIGHT_SHADOWS" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" "_SCREEN_SPACE_OCCLUSION" "_SHADOWS_SOFT" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "INSTANCING_ON" "_ADDITIONAL_LIGHTS" "_ADDITIONAL_LIGHT_SHADOWS" "_DBUFFER_MRT3" "_MAIN_LIGHT_SHADOWS" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" "_SCREEN_SPACE_OCCLUSION" "_SHADOWS_SOFT" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "INSTANCING_ON" "_ADDITIONAL_LIGHTS" "_ADDITIONAL_LIGHT_SHADOWS" "_DBUFFER_MRT3" "_MAIN_LIGHT_SHADOWS" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" "_SCREEN_SPACE_OCCLUSION" "_SHADOWS_SOFT" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "_ADDITIONAL_LIGHTS" "_ADDITIONAL_LIGHT_SHADOWS" "_DBUFFER_MRT3" "_LIGHT_COOKIES" "_MAIN_LIGHT_SHADOWS" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" "_SCREEN_SPACE_OCCLUSION" "_SHADOWS_SOFT" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "_ADDITIONAL_LIGHTS" "_ADDITIONAL_LIGHT_SHADOWS" "_DBUFFER_MRT3" "_LIGHT_COOKIES" "_MAIN_LIGHT_SHADOWS" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" "_SCREEN_SPACE_OCCLUSION" "_SHADOWS_SOFT" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "_ADDITIONAL_LIGHTS" "_ADDITIONAL_LIGHT_SHADOWS" "_DBUFFER_MRT3" "_LIGHT_COOKIES" "_MAIN_LIGHT_SHADOWS" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" "_SCREEN_SPACE_OCCLUSION" "_SHADOWS_SOFT" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "_ADDITIONAL_LIGHTS" "_ADDITIONAL_LIGHT_SHADOWS" "_DBUFFER_MRT3" "_MAIN_LIGHT_SHADOWS" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" "_SCREEN_SPACE_OCCLUSION" "_SHADOWS_SOFT" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "INSTANCING_ON" "_ADDITIONAL_LIGHTS" "_ADDITIONAL_LIGHT_SHADOWS" "_DBUFFER_MRT3" "_LIGHT_COOKIES" "_MAIN_LIGHT_SHADOWS_SCREEN" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" "_SCREEN_SPACE_OCCLUSION" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "INSTANCING_ON" "_ADDITIONAL_LIGHTS" "_ADDITIONAL_LIGHT_SHADOWS" "_DBUFFER_MRT3" "_LIGHT_COOKIES" "_MAIN_LIGHT_SHADOWS_SCREEN" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" "_SCREEN_SPACE_OCCLUSION" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "INSTANCING_ON" "_ADDITIONAL_LIGHTS" "_ADDITIONAL_LIGHT_SHADOWS" "_DBUFFER_MRT3" "_MAIN_LIGHT_SHADOWS_SCREEN" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" "_SCREEN_SPACE_OCCLUSION" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "INSTANCING_ON" "_ADDITIONAL_LIGHTS" "_ADDITIONAL_LIGHT_SHADOWS" "_DBUFFER_MRT3" "_MAIN_LIGHT_SHADOWS_SCREEN" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" "_SCREEN_SPACE_OCCLUSION" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "_ADDITIONAL_LIGHTS" "_ADDITIONAL_LIGHT_SHADOWS" "_DBUFFER_MRT3" "_LIGHT_COOKIES" "_MAIN_LIGHT_SHADOWS_SCREEN" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" "_SCREEN_SPACE_OCCLUSION" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "_ADDITIONAL_LIGHTS" "_ADDITIONAL_LIGHT_SHADOWS" "_DBUFFER_MRT3" "_LIGHT_COOKIES" "_MAIN_LIGHT_SHADOWS_SCREEN" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" "_SCREEN_SPACE_OCCLUSION" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "_ADDITIONAL_LIGHTS" "_ADDITIONAL_LIGHT_SHADOWS" "_DBUFFER_MRT3" "_MAIN_LIGHT_SHADOWS_SCREEN" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" "_SCREEN_SPACE_OCCLUSION" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "_ADDITIONAL_LIGHTS" "_ADDITIONAL_LIGHT_SHADOWS" "_DBUFFER_MRT3" "_MAIN_LIGHT_SHADOWS_SCREEN" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" "_SCREEN_SPACE_OCCLUSION" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "INSTANCING_ON" "_ADDITIONAL_LIGHTS" "_ADDITIONAL_LIGHT_SHADOWS" "_DBUFFER_MRT3" "_LIGHT_COOKIES" "_MAIN_LIGHT_SHADOWS_CASCADE" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" "_SCREEN_SPACE_OCCLUSION" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "INSTANCING_ON" "_ADDITIONAL_LIGHTS" "_ADDITIONAL_LIGHT_SHADOWS" "_DBUFFER_MRT3" "_LIGHT_COOKIES" "_MAIN_LIGHT_SHADOWS_CASCADE" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" "_SCREEN_SPACE_OCCLUSION" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "INSTANCING_ON" "_ADDITIONAL_LIGHTS" "_ADDITIONAL_LIGHT_SHADOWS" "_DBUFFER_MRT3" "_MAIN_LIGHT_SHADOWS_CASCADE" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" "_SCREEN_SPACE_OCCLUSION" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "INSTANCING_ON" "_ADDITIONAL_LIGHTS" "_ADDITIONAL_LIGHT_SHADOWS" "_DBUFFER_MRT3" "_MAIN_LIGHT_SHADOWS_CASCADE" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" "_SCREEN_SPACE_OCCLUSION" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "_ADDITIONAL_LIGHTS" "_ADDITIONAL_LIGHT_SHADOWS" "_DBUFFER_MRT3" "_LIGHT_COOKIES" "_MAIN_LIGHT_SHADOWS_CASCADE" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" "_SCREEN_SPACE_OCCLUSION" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "_ADDITIONAL_LIGHTS" "_ADDITIONAL_LIGHT_SHADOWS" "_DBUFFER_MRT3" "_LIGHT_COOKIES" "_MAIN_LIGHT_SHADOWS_CASCADE" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" "_SCREEN_SPACE_OCCLUSION" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "_ADDITIONAL_LIGHTS" "_ADDITIONAL_LIGHT_SHADOWS" "_DBUFFER_MRT3" "_MAIN_LIGHT_SHADOWS_CASCADE" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" "_SCREEN_SPACE_OCCLUSION" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "_ADDITIONAL_LIGHTS" "_ADDITIONAL_LIGHT_SHADOWS" "_DBUFFER_MRT3" "_MAIN_LIGHT_SHADOWS_CASCADE" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" "_SCREEN_SPACE_OCCLUSION" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "INSTANCING_ON" "_ADDITIONAL_LIGHTS" "_ADDITIONAL_LIGHT_SHADOWS" "_DBUFFER_MRT3" "_LIGHT_COOKIES" "_MAIN_LIGHT_SHADOWS" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" "_SCREEN_SPACE_OCCLUSION" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "INSTANCING_ON" "_ADDITIONAL_LIGHTS" "_ADDITIONAL_LIGHT_SHADOWS" "_DBUFFER_MRT3" "_LIGHT_COOKIES" "_MAIN_LIGHT_SHADOWS" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" "_SCREEN_SPACE_OCCLUSION" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "INSTANCING_ON" "_ADDITIONAL_LIGHTS" "_ADDITIONAL_LIGHT_SHADOWS" "_DBUFFER_MRT3" "_MAIN_LIGHT_SHADOWS" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" "_SCREEN_SPACE_OCCLUSION" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "INSTANCING_ON" "_ADDITIONAL_LIGHTS" "_ADDITIONAL_LIGHT_SHADOWS" "_DBUFFER_MRT3" "_MAIN_LIGHT_SHADOWS" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" "_SCREEN_SPACE_OCCLUSION" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "_ADDITIONAL_LIGHTS" "_ADDITIONAL_LIGHT_SHADOWS" "_DBUFFER_MRT3" "_LIGHT_COOKIES" "_MAIN_LIGHT_SHADOWS" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" "_SCREEN_SPACE_OCCLUSION" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "_ADDITIONAL_LIGHTS" "_ADDITIONAL_LIGHT_SHADOWS" "_DBUFFER_MRT3" "_LIGHT_COOKIES" "_MAIN_LIGHT_SHADOWS" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" "_SCREEN_SPACE_OCCLUSION" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "_ADDITIONAL_LIGHTS" "_ADDITIONAL_LIGHT_SHADOWS" "_DBUFFER_MRT3" "_LIGHT_COOKIES" "_MAIN_LIGHT_SHADOWS" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" "_SCREEN_SPACE_OCCLUSION" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "_ADDITIONAL_LIGHTS" "_ADDITIONAL_LIGHT_SHADOWS" "_DBUFFER_MRT3" "_MAIN_LIGHT_SHADOWS" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" "_SCREEN_SPACE_OCCLUSION" }
                    "// shader disassembly not supported on DXBC"
                }
            }
        }
        Pass {
            Name "GBuffer"
            Tags { "DisableBatching" = "False" "LIGHTMODE" = "UniversalGBuffer" "QUEUE" = "AlphaTest" "RenderPipeline" = "UniversalPipeline" "RenderType" = "Opaque" "ShaderGraphShader" = "true" "ShaderGraphTargetId" = "UniversalLitSubTarget" "UniversalMaterialType" = "Lit" }
            Blend Zero Zero, Zero Zero
            ZTest Off
            ZWrite Off
            Cull Off
            GpuProgramID 69736
            PlayerProgram "vp" {
                SubProgram "d3d11 " {
                    Keywords { "_MAIN_LIGHT_SHADOWS" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "_LIGHT_COOKIES" "_MAIN_LIGHT_SHADOWS" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "INSTANCING_ON" "_MAIN_LIGHT_SHADOWS" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "INSTANCING_ON" "_LIGHT_COOKIES" "_MAIN_LIGHT_SHADOWS" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "_MAIN_LIGHT_SHADOWS" "_MIXED_LIGHTING_SUBTRACTIVE" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "_LIGHT_COOKIES" "_MAIN_LIGHT_SHADOWS" "_MIXED_LIGHTING_SUBTRACTIVE" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "INSTANCING_ON" "_MAIN_LIGHT_SHADOWS" "_MIXED_LIGHTING_SUBTRACTIVE" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "INSTANCING_ON" "_LIGHT_COOKIES" "_MAIN_LIGHT_SHADOWS" "_MIXED_LIGHTING_SUBTRACTIVE" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "_MAIN_LIGHT_SHADOWS_CASCADE" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "_LIGHT_COOKIES" "_MAIN_LIGHT_SHADOWS_CASCADE" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "INSTANCING_ON" "_MAIN_LIGHT_SHADOWS_CASCADE" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "INSTANCING_ON" "_LIGHT_COOKIES" "_MAIN_LIGHT_SHADOWS_CASCADE" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "_MAIN_LIGHT_SHADOWS_CASCADE" "_MIXED_LIGHTING_SUBTRACTIVE" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "_LIGHT_COOKIES" "_MAIN_LIGHT_SHADOWS_CASCADE" "_MIXED_LIGHTING_SUBTRACTIVE" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "INSTANCING_ON" "_MAIN_LIGHT_SHADOWS_CASCADE" "_MIXED_LIGHTING_SUBTRACTIVE" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "INSTANCING_ON" "_LIGHT_COOKIES" "_MAIN_LIGHT_SHADOWS_CASCADE" "_MIXED_LIGHTING_SUBTRACTIVE" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "_MAIN_LIGHT_SHADOWS_SCREEN" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "_LIGHT_COOKIES" "_MAIN_LIGHT_SHADOWS_SCREEN" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "INSTANCING_ON" "_MAIN_LIGHT_SHADOWS_SCREEN" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "INSTANCING_ON" "_LIGHT_COOKIES" "_MAIN_LIGHT_SHADOWS_SCREEN" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "_MAIN_LIGHT_SHADOWS_SCREEN" "_MIXED_LIGHTING_SUBTRACTIVE" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "_LIGHT_COOKIES" "_MAIN_LIGHT_SHADOWS_SCREEN" "_MIXED_LIGHTING_SUBTRACTIVE" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "INSTANCING_ON" "_MAIN_LIGHT_SHADOWS_SCREEN" "_MIXED_LIGHTING_SUBTRACTIVE" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "INSTANCING_ON" "_LIGHT_COOKIES" "_MAIN_LIGHT_SHADOWS_SCREEN" "_MIXED_LIGHTING_SUBTRACTIVE" }
                    "// shader disassembly not supported on DXBC"
                }
            }
            PlayerProgram "fp" {
                SubProgram "d3d11 " {
                    Keywords { "INSTANCING_ON" "_DBUFFER_MRT3" "_LIGHT_COOKIES" "_MAIN_LIGHT_SHADOWS_SCREEN" "_MIXED_LIGHTING_SUBTRACTIVE" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" "_SHADOWS_SOFT" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "INSTANCING_ON" "_DBUFFER_MRT3" "_MAIN_LIGHT_SHADOWS_SCREEN" "_MIXED_LIGHTING_SUBTRACTIVE" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" "_SHADOWS_SOFT" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "_DBUFFER_MRT3" "_LIGHT_COOKIES" "_MAIN_LIGHT_SHADOWS_SCREEN" "_MIXED_LIGHTING_SUBTRACTIVE" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" "_SHADOWS_SOFT" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "_DBUFFER_MRT3" "_MAIN_LIGHT_SHADOWS_SCREEN" "_MIXED_LIGHTING_SUBTRACTIVE" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" "_SHADOWS_SOFT" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "INSTANCING_ON" "_DBUFFER_MRT3" "_LIGHT_COOKIES" "_MAIN_LIGHT_SHADOWS_SCREEN" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" "_SHADOWS_SOFT" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "INSTANCING_ON" "_DBUFFER_MRT3" "_MAIN_LIGHT_SHADOWS_SCREEN" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" "_SHADOWS_SOFT" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "_DBUFFER_MRT3" "_LIGHT_COOKIES" "_MAIN_LIGHT_SHADOWS_SCREEN" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" "_SHADOWS_SOFT" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "_DBUFFER_MRT3" "_MAIN_LIGHT_SHADOWS_SCREEN" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" "_SHADOWS_SOFT" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "INSTANCING_ON" "_DBUFFER_MRT3" "_LIGHT_COOKIES" "_MAIN_LIGHT_SHADOWS_CASCADE" "_MIXED_LIGHTING_SUBTRACTIVE" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" "_SHADOWS_SOFT" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "INSTANCING_ON" "_DBUFFER_MRT3" "_MAIN_LIGHT_SHADOWS_CASCADE" "_MIXED_LIGHTING_SUBTRACTIVE" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" "_SHADOWS_SOFT" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "_DBUFFER_MRT3" "_LIGHT_COOKIES" "_MAIN_LIGHT_SHADOWS_CASCADE" "_MIXED_LIGHTING_SUBTRACTIVE" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" "_SHADOWS_SOFT" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "_DBUFFER_MRT3" "_MAIN_LIGHT_SHADOWS_CASCADE" "_MIXED_LIGHTING_SUBTRACTIVE" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" "_SHADOWS_SOFT" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "INSTANCING_ON" "_DBUFFER_MRT3" "_LIGHT_COOKIES" "_MAIN_LIGHT_SHADOWS_CASCADE" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" "_SHADOWS_SOFT" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "INSTANCING_ON" "_DBUFFER_MRT3" "_MAIN_LIGHT_SHADOWS_CASCADE" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" "_SHADOWS_SOFT" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "_DBUFFER_MRT3" "_LIGHT_COOKIES" "_MAIN_LIGHT_SHADOWS_CASCADE" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" "_SHADOWS_SOFT" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "_DBUFFER_MRT3" "_MAIN_LIGHT_SHADOWS_CASCADE" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" "_SHADOWS_SOFT" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "INSTANCING_ON" "_DBUFFER_MRT3" "_LIGHT_COOKIES" "_MAIN_LIGHT_SHADOWS" "_MIXED_LIGHTING_SUBTRACTIVE" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" "_SHADOWS_SOFT" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "INSTANCING_ON" "_DBUFFER_MRT3" "_MAIN_LIGHT_SHADOWS" "_MIXED_LIGHTING_SUBTRACTIVE" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" "_SHADOWS_SOFT" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "_DBUFFER_MRT3" "_LIGHT_COOKIES" "_MAIN_LIGHT_SHADOWS" "_MIXED_LIGHTING_SUBTRACTIVE" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" "_SHADOWS_SOFT" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "_DBUFFER_MRT3" "_MAIN_LIGHT_SHADOWS" "_MIXED_LIGHTING_SUBTRACTIVE" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" "_SHADOWS_SOFT" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "INSTANCING_ON" "_DBUFFER_MRT3" "_LIGHT_COOKIES" "_MAIN_LIGHT_SHADOWS" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" "_SHADOWS_SOFT" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "INSTANCING_ON" "_DBUFFER_MRT3" "_MAIN_LIGHT_SHADOWS" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" "_SHADOWS_SOFT" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "_DBUFFER_MRT3" "_LIGHT_COOKIES" "_MAIN_LIGHT_SHADOWS" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" "_SHADOWS_SOFT" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "_DBUFFER_MRT3" "_MAIN_LIGHT_SHADOWS" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" "_SHADOWS_SOFT" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "_DBUFFER_MRT3" "_MAIN_LIGHT_SHADOWS" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "_DBUFFER_MRT3" "_LIGHT_COOKIES" "_MAIN_LIGHT_SHADOWS" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "INSTANCING_ON" "_DBUFFER_MRT3" "_MAIN_LIGHT_SHADOWS" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "INSTANCING_ON" "_DBUFFER_MRT3" "_LIGHT_COOKIES" "_MAIN_LIGHT_SHADOWS" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "_DBUFFER_MRT3" "_MAIN_LIGHT_SHADOWS" "_MIXED_LIGHTING_SUBTRACTIVE" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "_DBUFFER_MRT3" "_LIGHT_COOKIES" "_MAIN_LIGHT_SHADOWS" "_MIXED_LIGHTING_SUBTRACTIVE" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "INSTANCING_ON" "_DBUFFER_MRT3" "_MAIN_LIGHT_SHADOWS" "_MIXED_LIGHTING_SUBTRACTIVE" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "INSTANCING_ON" "_DBUFFER_MRT3" "_LIGHT_COOKIES" "_MAIN_LIGHT_SHADOWS" "_MIXED_LIGHTING_SUBTRACTIVE" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "_DBUFFER_MRT3" "_MAIN_LIGHT_SHADOWS_CASCADE" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "_DBUFFER_MRT3" "_LIGHT_COOKIES" "_MAIN_LIGHT_SHADOWS_CASCADE" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "INSTANCING_ON" "_DBUFFER_MRT3" "_MAIN_LIGHT_SHADOWS_CASCADE" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "INSTANCING_ON" "_DBUFFER_MRT3" "_LIGHT_COOKIES" "_MAIN_LIGHT_SHADOWS_CASCADE" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "_DBUFFER_MRT3" "_MAIN_LIGHT_SHADOWS_CASCADE" "_MIXED_LIGHTING_SUBTRACTIVE" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "_DBUFFER_MRT3" "_LIGHT_COOKIES" "_MAIN_LIGHT_SHADOWS_CASCADE" "_MIXED_LIGHTING_SUBTRACTIVE" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "INSTANCING_ON" "_DBUFFER_MRT3" "_MAIN_LIGHT_SHADOWS_CASCADE" "_MIXED_LIGHTING_SUBTRACTIVE" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "INSTANCING_ON" "_DBUFFER_MRT3" "_LIGHT_COOKIES" "_MAIN_LIGHT_SHADOWS_CASCADE" "_MIXED_LIGHTING_SUBTRACTIVE" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "_DBUFFER_MRT3" "_MAIN_LIGHT_SHADOWS_SCREEN" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "_DBUFFER_MRT3" "_LIGHT_COOKIES" "_MAIN_LIGHT_SHADOWS_SCREEN" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "INSTANCING_ON" "_DBUFFER_MRT3" "_MAIN_LIGHT_SHADOWS_SCREEN" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "INSTANCING_ON" "_DBUFFER_MRT3" "_LIGHT_COOKIES" "_MAIN_LIGHT_SHADOWS_SCREEN" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "_DBUFFER_MRT3" "_MAIN_LIGHT_SHADOWS_SCREEN" "_MIXED_LIGHTING_SUBTRACTIVE" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "_DBUFFER_MRT3" "_LIGHT_COOKIES" "_MAIN_LIGHT_SHADOWS_SCREEN" "_MIXED_LIGHTING_SUBTRACTIVE" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "INSTANCING_ON" "_DBUFFER_MRT3" "_MAIN_LIGHT_SHADOWS_SCREEN" "_MIXED_LIGHTING_SUBTRACTIVE" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "INSTANCING_ON" "_DBUFFER_MRT3" "_LIGHT_COOKIES" "_MAIN_LIGHT_SHADOWS_SCREEN" "_MIXED_LIGHTING_SUBTRACTIVE" "_REFLECTION_PROBE_BLENDING" "_REFLECTION_PROBE_BOX_PROJECTION" }
                    "// shader disassembly not supported on DXBC"
                }
            }
        }
        Pass {
            Name "ShadowCaster"
            Tags { "DisableBatching" = "False" "LIGHTMODE" = "SHADOWCASTER" "QUEUE" = "AlphaTest" "RenderPipeline" = "UniversalPipeline" "RenderType" = "Opaque" "ShaderGraphShader" = "true" "ShaderGraphTargetId" = "UniversalLitSubTarget" "UniversalMaterialType" = "Lit" }
            ColorMask 0 0
            Cull Off
            GpuProgramID 163174
            PlayerProgram "vp" {
                SubProgram "d3d11 " {
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "_LIGHT_COOKIES" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "INSTANCING_ON" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "INSTANCING_ON" "_LIGHT_COOKIES" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "_CASTING_PUNCTUAL_LIGHT_SHADOW" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "_CASTING_PUNCTUAL_LIGHT_SHADOW" "_LIGHT_COOKIES" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "INSTANCING_ON" "_CASTING_PUNCTUAL_LIGHT_SHADOW" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "INSTANCING_ON" "_CASTING_PUNCTUAL_LIGHT_SHADOW" "_LIGHT_COOKIES" }
                    "// shader disassembly not supported on DXBC"
                }
            }
            PlayerProgram "fp" {
                SubProgram "d3d11 " {
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "_LIGHT_COOKIES" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "INSTANCING_ON" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "INSTANCING_ON" "_LIGHT_COOKIES" }
                    "// shader disassembly not supported on DXBC"
                }
            }
        }
        Pass {
            Name "DepthOnly"
            Tags { "DisableBatching" = "False" "LIGHTMODE" = "DepthOnly" "QUEUE" = "AlphaTest" "RenderPipeline" = "UniversalPipeline" "RenderType" = "Opaque" "ShaderGraphShader" = "true" "ShaderGraphTargetId" = "UniversalLitSubTarget" "UniversalMaterialType" = "Lit" }
            ColorMask B 0
            Cull Off
            GpuProgramID 215402
            PlayerProgram "vp" {
                SubProgram "d3d11 " {
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "_LIGHT_COOKIES" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "INSTANCING_ON" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "INSTANCING_ON" "_LIGHT_COOKIES" }
                    "// shader disassembly not supported on DXBC"
                }
            }
            PlayerProgram "fp" {
                SubProgram "d3d11 " {
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "_LIGHT_COOKIES" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "INSTANCING_ON" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "INSTANCING_ON" "_LIGHT_COOKIES" }
                    "// shader disassembly not supported on DXBC"
                }
            }
        }
        Pass {
            Name "DepthNormals"
            Tags { "DisableBatching" = "False" "LIGHTMODE" = "DepthNormals" "QUEUE" = "AlphaTest" "RenderPipeline" = "UniversalPipeline" "RenderType" = "Opaque" "ShaderGraphShader" = "true" "ShaderGraphTargetId" = "UniversalLitSubTarget" "UniversalMaterialType" = "Lit" }
            Cull Off
            GpuProgramID 319403
            PlayerProgram "vp" {
                SubProgram "d3d11 " {
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "_LIGHT_COOKIES" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "INSTANCING_ON" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "INSTANCING_ON" "_LIGHT_COOKIES" }
                    "// shader disassembly not supported on DXBC"
                }
            }
            PlayerProgram "fp" {
                SubProgram "d3d11 " {
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "_LIGHT_COOKIES" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "INSTANCING_ON" }
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "INSTANCING_ON" "_LIGHT_COOKIES" }
                    "// shader disassembly not supported on DXBC"
                }
            }
        }
        Pass {
            Name "Universal 2D"
            Tags { "DisableBatching" = "False" "LIGHTMODE" = "Universal2D" "QUEUE" = "AlphaTest" "RenderPipeline" = "UniversalPipeline" "RenderType" = "Opaque" "ShaderGraphShader" = "true" "ShaderGraphTargetId" = "UniversalLitSubTarget" "UniversalMaterialType" = "Lit" }
            Blend Zero Zero, Zero Zero
            ZTest Off
            ZWrite Off
            Cull Off
            GpuProgramID 528506
            PlayerProgram "vp" {
                SubProgram "d3d11 " {
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "_LIGHT_COOKIES" }
                    "// shader disassembly not supported on DXBC"
                }
            }
            PlayerProgram "fp" {
                SubProgram "d3d11 " {
                    "// shader disassembly not supported on DXBC"
                }
                SubProgram "d3d11 " {
                    Keywords { "_LIGHT_COOKIES" }
                    "// shader disassembly not supported on DXBC"
                }
            }
        }
    }
Fallback "Hidden/Shader Graph/FallbackError"
CustomEditor "UnityEditor.ShaderGraph.GenericShaderGraphMaterialGUI"
}
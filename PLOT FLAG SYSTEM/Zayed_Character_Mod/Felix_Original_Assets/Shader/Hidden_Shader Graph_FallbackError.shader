//////////////////////////////////////////
//
// NOTE: This is *not* a valid shader file
//
///////////////////////////////////////////
Shader "Hidden/Shader Graph/FallbackError" {
    Properties {
    }
    SubShader {
        Tags { "ShaderGraphShader" = "true" "ShaderGraphTargetId" = "FallbackError" }
        Pass {
            Tags { "ShaderGraphShader" = "true" "ShaderGraphTargetId" = "FallbackError" }
            GpuProgramID 24356
            PlayerProgram "vp" {
                SubProgram "d3d11 " {
                    "// shader disassembly not supported on DXBC"
                }
            }
            PlayerProgram "fp" {
                SubProgram "d3d11 " {
                    "// shader disassembly not supported on DXBC"
                }
            }
        }
    }
}
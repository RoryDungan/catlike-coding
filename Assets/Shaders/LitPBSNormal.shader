// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Lit (PBS, normal map)"
{
    Properties
    {
        _Tint ("Tint", Color) = (1, 1, 1, 1)
        _MainTex ("Albedo", 2D) = "white" {}

        [NoScaleOffset] _NormalMap ("Normals", 2D) = "bump" {}
        _BumpScale ("Bump Scale", Float) = 1

        [NoScaleOffset] _MetallicMap("Metallic", 2D) = "white" {}
        [Gamma] _Metallic ("Metallic", Range(0, 1)) = 0
        _Smoothness ("Smoothness", Range(0, 1)) = 0.5

        _DetailTex ("Detial Albedo", 2D) = "gray" {}
        [NoScaleOffset] _DetailNormalMap ("Detail Normals", 2D) = "bump" {}
        _DetailBumpScale ("Detail Bump Scale", Float) = 1
    }
    SubShader
    {
        Pass
        {
            Tags {
                "LightMode" = "ForwardBase"
            }

            CGPROGRAM
            #pragma target 3.0

            #pragma multi_compile _ SHADOWS_SCREEN
            #pragma multi_compile _ VERTEXLIGHT_ON
            #pragma shader_feature _ _METALLIC_MAP
            #pragma shader_feature _ _SMOOTHNESS_ALBEDO _SMOOTHNESS_METALLIC

            #pragma vertex vert
            #pragma fragment frag

            #define FORWARD_BASE_PASS

            #include "LightingNormal.cginc"

            ENDCG
        }

        Pass {
            Tags {
                "LightMode" = "ForwardAdd"
            }

            Blend One One
            ZWrite Off

            CGPROGRAM
            #pragma target 3.0

            #pragma multi_compile_fwdadd_fullshadows
            #pragma shader_feature _ _METALLIC_MAP
            #pragma shader_feature _ _SMOOTHNESS_ALBEDO _SMOOTHNESS_METALLIC

            #pragma vertex vert
            #pragma fragment frag

            #include "LightingNormal.cginc"

            ENDCG
        }

        Pass {
            Tags {
                "LightMode" = "ShadowCaster"
            }

            CGPROGRAM

            #pragma target 3.0

            #pragma multi_compile_shadowcaster

            #pragma vertex ShadowVertexProgram
            #pragma fragment ShadowFragmentProgram

            #include "Shadows.cginc"

            ENDCG
        }
    }
    CustomEditor "LightingShaderGUI"
}

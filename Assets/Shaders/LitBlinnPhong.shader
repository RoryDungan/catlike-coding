﻿// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Lit (Blinn-Phong)"
{
    Properties
    {
        _Tint ("Tint", Color) = (1, 1, 1, 1)
        _MainTex ("Albedo", 2D) = "white" {}
        [Gamma]_Metallic ("Metallic", Range(0, 1)) = 0
        _Smoothness ("Smoothness", Range(0, 1)) = 0.5
    }
    SubShader
    {
        Pass
        {
            Tags {
                "LightMode" = "ForwardBase"
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityStandardBRDF.cginc"
            #include "UnityStandardUtils.cginc"

            float4 _Tint;
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Metallic;
            float _Smoothness;

            struct VertexData
            {
                float4 position : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Interpolators
            {
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
            };

            Interpolators vert (VertexData v)
            {
                Interpolators i;
                i.position = UnityObjectToClipPos(v.position);
                i.worldPos = mul(unity_ObjectToWorld, v.position);
                i.normal = UnityObjectToWorldNormal(v.normal);
                i.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return i;
            }

            fixed4 frag (Interpolators i) : SV_TARGET
            {
                // return float4(i.uv, 1, 1);
                float3 lightColor = _LightColor0.rgb;
                float3 normal = normalize(i.normal);
                float3 lightDir = _WorldSpaceLightPos0.xyz;
                float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
                float3 halfVector = normalize(lightDir + viewDir);

                float3 albedo = tex2D(_MainTex, i.uv).rgb * _Tint.rgb;

                float3 specularTint = albedo * _Metallic;
                float oneMinusReflectivity = 1 - _Metallic;
                albedo = DiffuseAndSpecularFromMetallic(
                    albedo, _Metallic, specularTint, oneMinusReflectivity
                );

                float3 diffuse =
                    albedo *
                    lightColor *
                    DotClamped(normal, lightDir);

                float3 specular = specularTint * lightColor * pow(
                    DotClamped(halfVector, normal),
                    _Smoothness * 100
                );

                return float4(diffuse + specular, 1);
                // return float4(0.5 + normal * 0.5, 1);
                // return tex2D(_MainTex, i.uv) * _Tint;
            }
            ENDCG
        }
    }
}

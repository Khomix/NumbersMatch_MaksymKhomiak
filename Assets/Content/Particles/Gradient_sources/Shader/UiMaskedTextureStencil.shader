Shader "UI/MaskedShineAdditiveStencil"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _MaskTex ("Mask Texture", 2D) = "white" {}
        _MaskTex_Tiling ("Mask Tiling and Offset", Vector) = (1, 1, 0, 0)
        _MaskTex_ScrollSpeed ("Mask Scroll Speed", Vector) = (0, 0, 0, 0) 
        _Alpha ("Alpha", Float) = 1
        
         // Required stencil properties for Unity UI masking
        [HideInInspector]_Stencil ("Stencil ID", Float) = 0
        [HideInInspector]_StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector]_StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector]_StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector]_StencilReadMask ("Stencil Read Mask", Float) = 255
        [HideInInspector]_ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        // Stencil settings for Unity UI Mask compatibility
        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Pass
        {
            Blend One One
            ZWrite Off
            Cull Off
            ColorMask [_ColorMask]

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 maskUv : TEXCOORD1;
                float4 color : COLOR;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            TEXTURE2D(_MaskTex);
            SAMPLER(sampler_MaskTex);

            float4 _MaskTex_Tiling;
            float4 _MaskTex_ScrollSpeed;
            float _Alpha;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.uv = v.uv;

                float2 movingOffset = _MaskTex_ScrollSpeed.xy * _Time.y;
                o.maskUv = v.uv * _MaskTex_Tiling.xy + _MaskTex_Tiling.zw + movingOffset;

                o.color = v.color;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                float mainTexAlpha = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv).a;
                float maskValue = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, i.maskUv).r;

                maskValue *= mainTexAlpha;
                float4 resultColor = i.color * maskValue;

                resultColor.rgb = resultColor.rgb * mainTexAlpha + resultColor.rgb;
                resultColor.a = mainTexAlpha * maskValue;

                return float4(resultColor.rgb * _Alpha, resultColor.a);
            }
            ENDHLSL
        }
    }
}
Shader "UI/Blur"
{
    Properties
    {
        [PerRendererData] _MainTex ("Main Texture", 2D) = "white" {}
        _BlurSize ("Blur Size", Range(0, 5)) = 1.0
        _TintColor ("Tint Color", Color) = (1,1,1,1)
        
        [Header(Stencil)]
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile __ UNITY_UI_CLIP_RECT

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
                float4 worldPosition : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            float _BlurSize;
            fixed4 _TintColor;
            float4 _ClipRect;

            v2f vert (appdata v)
            {
                v2f o;
                o.worldPosition = v.vertex;
                o.vertex = UnityObjectToClipPos(o.worldPosition);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color * _TintColor;
                
                #ifdef UNITY_HALF_TEXEL_OFFSET
                o.vertex.xy += (_ScreenParams.zw-1.0) * float2(-1,1);
                #endif
                
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 5x5高斯核权重（已归一化）
                static const float weight[5][5] = {
                    {0.003765, 0.015019, 0.023792, 0.015019, 0.003765},
                    {0.015019, 0.059912, 0.094907, 0.059912, 0.015019},
                    {0.023792, 0.094907, 0.150342, 0.094907, 0.023792},
                    {0.015019, 0.059912, 0.094907, 0.059912, 0.015019},
                    {0.003765, 0.015019, 0.023792, 0.015019, 0.003765}
                };

                half4 color = 0;
                float2 texelSize = _MainTex_TexelSize.xy * _BlurSize;

                // 二维高斯采样
                for (int x = -2; x <= 2; x++) {
                    for (int y = -2; y <= 2; y++) {
                        float2 offset = float2(x, y) * texelSize;
                        color += tex2D(_MainTex, i.uv + offset) * weight[x + 2][y + 2];
                    }
                }

                // UI颜色混合
                color *= i.color;

                // Clip Rect处理
                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(i.worldPosition.xy, _ClipRect);
                #endif

                return color;
            }
            ENDCG
        }
    }
}
Shader "AxibugEmuOnline/XMBBackGround"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15


        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
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

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;           
            float4 _MainTex_TexelSize;           

            float3 lerp(float3 color1, float3 color2, float value)
            {
	            return value * (color2 - color1) + color1;
            }
            float wave(float x, float frequency, float speed, float midHeight, float maxHeight)
            {
                return (sin(frequency * (x + speed * (((1. - (pow(cos(0.002 * (_Time.y + 400.)), 2.) + 1.) / 2.) + .1) * 2048.))) * (maxHeight - midHeight)) + midHeight;
            }
            float percentHigh(float currentY, float waveHeight, float maxHeight, float power)
            {
                float percentWave = max(waveHeight - currentY, 0.0) / maxHeight;
                return pow(1.0 - percentWave, power);
            }
            float waveColor(float2 uv, float waveHeight, float maxHeight, float frequency, float power)
            {
                float percentWave = percentHigh(uv.y, waveHeight, maxHeight, power);
                return clamp(percentWave + 0.8, 0.0, 1.0);
            }

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);

                OUT.color = v.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float2 uv= IN.texcoord;
                // Lerped background
                float3 blue = float3(0, 0.4, 1);
                float3 blue2 = float3(0, 0.7, 1);
                float amount = (uv.x + uv.y) / 2.0;
                float3 bg = lerp(blue2, blue, amount);
    
                // Overlayed sine waves
                float midHeight1 = 0.4;
                float maxHeight1 = 0.5 + wave(0.0, 4.0, 0.02, 0.0, 0.02);
                float power1 = 50.0; //Higher power means thinner line
                float frequency1 = 2.0 + wave(0.0, 3.0, 0.03, 0.0, 0.02);
                float speed1 = 0.4 + wave(0.0, 2.2, 0.04, 0.0, 0.01);
                float waveHeight1 = wave(uv.x, frequency1, speed1, midHeight1, maxHeight1);
                float waveCol1 = waveColor(uv, waveHeight1, maxHeight1, frequency1, power1);
    
                float midHeight2 = 0.42;
                float maxHeight2 = 0.54 + wave(0.0, 3.0, 0.04, 0.0, 0.02);
                float power2 = 50.0; //Higher power means thinner line
                float frequency2 = 2.1 + wave(0.0, 4.0, 0.05, 0.0, 0.02);
                float speed2 = 0.3 + wave(0.0, 2.0, 0.02, 0.0, 0.01);
                float waveHeight2 = wave(uv.x, frequency2, speed2, midHeight2, maxHeight2);
                float waveCol2 = waveColor(uv, waveHeight2, maxHeight2, frequency2, power2);
    
                float3 col = bg;
                if (uv.y < waveHeight1)
                    col = waveCol1 * col;
                if (uv.y < waveHeight2)
                    col = waveCol2 * col;

                // Output to screen
                fixed4 fragColor = float4(col,1.0);

                #ifdef UNITY_UI_CLIP_RECT
                fragColor.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip (fragColor.a - 0.001);
                #endif

                return fragColor;
            }



        ENDCG
        }
    }
}
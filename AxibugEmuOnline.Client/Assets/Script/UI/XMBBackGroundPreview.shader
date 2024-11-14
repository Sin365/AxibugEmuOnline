Shader "AxibugEmuOnline/XMBBackGroundPreview"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15


        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0

        [Space(10)]
        _Color1("Color1",Color) = (0,0.4,1,1)
        _Color2("Color2",Color) = (0,0.7,1,1)

        [Header(Wave1)]
        _MidHeight1("MidHeight",Float) = 0.4
        _MaxHeight1("MaxHeigh",Float) = 0.5        
        _Power1("Power",Float)=50.0
        _Frequency1("Frequency",Float)=2.0
        _Speed1("Speed",Float)=0.4

        [Header(Wave2)]
        _MidHeight2("MidHeight",Float) = 0.42
        _MaxHeight2("MaxHeigh",Float) = 0.54       
        _Power2("Power",Float)=50.0
        _Frequency2("Frequency",Float)=2.1
        _Speed2("Speed",Float)=0.3
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

            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;     
            sampler2D _MainTex;

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

                OUT.texcoord =v.texcoord;

                OUT.color = v.color * _Color;
                return OUT;
            }

            float3 _Color1;
            float3 _Color2;

            float _MidHeight1;
            float _MaxHeight1;
            float _Power1;
            float _Frequency1;
            float _Speed1;

            float _MidHeight2;
            float _MaxHeight2;
            float _Power2;
            float _Frequency2;
            float _Speed2;

            fixed4 frag(v2f IN) : SV_Target
            {
                float2 uv= IN.texcoord;
                // Lerped background
                
                float amount = (uv.x + uv.y) / 2.0;
                float3 bg = lerp(_Color2, _Color1, amount);
                
                // Overlayed sine waves
                float maxHeight1 = _MaxHeight1 + wave(0, 4.0,0.02, 0.0, 0.02);
                float power1 = _Power1; //Higher power means thinner line
                float frequency1 = _Frequency1 + wave(0.0, 3.0, 0.03, 0.0, 0.02);
                float speed1 = _Speed1 + wave(0.0, 2.2, 0.04, 0.0, 0.01);
                float waveHeight1 = wave(uv.x, frequency1, speed1, _MidHeight1, maxHeight1);
                float waveCol1 = waveColor(uv, waveHeight1, maxHeight1, frequency1, power1);
    
                float midHeight2 = _MidHeight2;
                float maxHeight2 = _MaxHeight2 + wave(0.0, 3.0, 0.04, 0.0, 0.02);
                float power2 = _Power2; //Higher power means thinner line
                float frequency2 = _Frequency2 + wave(0.0, 4.0, 0.05, 0.0, 0.02);
                float speed2 = _Speed2 + wave(0.0, 2.0, 0.02, 0.0, 0.01);
                float waveHeight2 = wave(uv.x, frequency2, speed2, midHeight2, maxHeight2);
                float waveCol2 = waveColor(uv, waveHeight2, maxHeight2, frequency2, power2);
    
                float3 col = bg;
                col = lerp(col, waveCol1 * col, step(uv.y, waveHeight1));
                col = lerp(col, waveCol2 * col, step(uv.y, waveHeight2));

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
Shader "Filter/RetroArch/Glow/threshold"
{
    Properties
    {
        GLOW_WHITEPOINT("Glow Whitepoint",float) = 1.0
        GLOW_ROLLOFF("Glow Rolloff",float) = 3.0
    }
    SubShader
    {
        Pass
        {
            CGPROGRAM

            #pragma vertex vert_img
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "../FilterChain.cginc"

            float GLOW_WHITEPOINT;
            float GLOW_ROLLOFF;

            struct v2f
            {
                float4 pos : POSITION;
                float2 uv : TEXCOORD0;
            };

            fixed4 frag (v2f IN) : SV_Target
            {
                vec3 color  = 1.15 * texture(Source, vTexCoord).rgb;
                vec3 factor = clamp(color / GLOW_WHITEPOINT, 0.0, 1.0);

                return  vec4(pow(factor, vec3(GLOW_ROLLOFF,GLOW_ROLLOFF,GLOW_ROLLOFF)), 1.0);
            }
            ENDCG
        }
    }
}




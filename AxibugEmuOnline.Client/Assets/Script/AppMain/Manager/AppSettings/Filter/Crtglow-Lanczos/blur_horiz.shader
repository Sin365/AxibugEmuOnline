Shader "Filter/RetroArch/Glow/blur_horiz"
{
    Properties
    {
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

            #define GLOW_FALLOFF 0.35
            #define TAPS 4
            #define kernel(x) exp(-GLOW_FALLOFF * (x) * (x))

            struct v2f
            {
                float4 pos : POSITION;
                float2 uv : TEXCOORD0;
            };

            fixed4 frag (v2f IN) : SV_Target
            {
                vec3 col = vec3(0,0,0);
                float dx = 4.0 * SourceSize.z; // Mipmapped

                float k_total = 0.0;
                for (int i = -TAPS; i <= TAPS; i++)
                {
                    float k  = kernel(i);
                    k_total += k;
                    col     += k * texture(Source, vTexCoord + vec2(float(i) * dx, 0.0)).rgb;
                }

                return vec4(col / k_total, 1.0);
            }
            ENDCG
        }
    }
}




Shader "Filter/RetroArch/Glow/Gauss_vert"
{
    Properties
    {
        BOOST("Color Boost",float) = 1.0
    }
    SubShader
    {
        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "../FilterChain.cginc"

            float BOOST;

            struct v2f
            {
                float4 pos : POSITION;
                float2 uv : TEXCOORD0;
                float3 custom : COLOR;
            };

            v2f vert( appdata_img v )
            {
                v2f o;
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.pos = UnityObjectToClipPos (v.vertex);
                o.uv = v.texcoord;

                o.custom.xy = o.uv * SourceSize.xy - vec2(0.0, 0.5);
                o.custom.z = SourceSize.w;

                return o;
            }

            #define CRT_GEOM_BEAM 1

            vec3 beam(vec3 color, float dist)
            {
                #if CRT_GEOM_BEAM
                    vec3 wid     = 2.0 + 2.0 * pow(color, vec3(4,4,4));
                    float weight = abs(dist) * 3.333333333;
                    vec3 weights = vec3(weight,weight,weight);

                    return 2.0 * color * exp(-pow(weights * rsqrt(0.5 * wid), wid)) / (0.6 + 0.2 * wid);
                #else
                    float reciprocal_width = 4.0;
                    vec3 x = dist * reciprocal_width;

                    return 2.0 * color * exp(-0.5 * x * x) * reciprocal_width;
                #endif
            }

            fixed4 frag (v2f IN) : SV_Target
            {
                float2 data_pix_no = IN.custom.xy;
                float data_one = IN.custom.z;

                vec2  texel = floor(data_pix_no);
                float phase = data_pix_no.y - texel.y;
                vec2  tex   = vec2(texel + 0.5) * SourceSize.zw;

                vec3 top    = texture(Source, tex + vec2(0.0, 0 * data_one)).rgb;
                vec3 bottom = texture(Source, tex + vec2(0.0, 1 * data_one)).rgb;

                float dist0 = phase;
                float dist1 = 1.0 - phase;

                vec3 scanline = vec3(0,0,0);

                scanline += beam(top,    dist0);
                scanline += beam(bottom, dist1);

                return vec4(BOOST * scanline * 0.869565217391304, 1.0);
            }
            ENDCG
        }
    }
}




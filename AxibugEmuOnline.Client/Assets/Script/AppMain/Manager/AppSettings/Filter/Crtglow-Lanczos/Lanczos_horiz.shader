Shader "Filter/RetroArch/Glow/Lanczos_horiz"
{
    Properties
    {

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

            struct v2f
            {
                float4 pos : POSITION;
                float2 uv : TEXCOORD0;
                float2 pixNoAndDataOne : COLOR;
            };

            v2f vert( appdata_img v )
            {
                v2f o;
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.pos = UnityObjectToClipPos (v.vertex);
                o.uv = v.texcoord;

                o.pixNoAndDataOne.x = o.uv.x * SourceSize.x;
                o.pixNoAndDataOne.y = SourceSize.z;

                return o;
            }

            fixed4 frag (v2f IN) : SV_Target
            {
                float data_pix_no = IN.pixNoAndDataOne.x;
                float data_one = IN.pixNoAndDataOne.y;

                float texel      = floor(data_pix_no);
                float phase      = data_pix_no - texel;
                float base_phase = phase - 0.5;
                vec2 tex         = vec2((texel + 0.5) * SourceSize.z, vTexCoord.y);

                vec3 col = vec3(0,0,0);
                for (int i = -2; i <= 2; i++)
                {
                    float phase = base_phase - float(i);
                    if (abs(phase) < 2.0)
                    {
                        float g = sinc(phase) * sinc(0.5 * phase);
                        col += texture(Source, tex + vec2(float(i) * data_one, 0.0)).rgb * g;
                    }
                }

                return vec4(col, 1.0);
            }
            ENDCG
        }
    }
}




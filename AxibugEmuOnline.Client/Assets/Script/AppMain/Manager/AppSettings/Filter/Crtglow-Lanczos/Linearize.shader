Shader "Filter/RetroArch/Glow/Linearize"
{
    Properties
    {
        INPUT_GAMMA("Input Gamma",float) = 4.5
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

            float INPUT_GAMMA;

            struct v2f
            {
                float4 pos : POSITION;
                float2 uv : TEXCOORD0;
            };

            fixed4 frag (v2f IN) : SV_Target
            {
                vec3 color = texture(Source, vTexCoord);

                vec4 FragColor  = vec4(pow(color, vec3(INPUT_GAMMA,INPUT_GAMMA,INPUT_GAMMA)), 1.0);

                return FragColor;
            }
            ENDCG
        }
    }
}




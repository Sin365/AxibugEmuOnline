
Shader "Filter/LCDPostEffect"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
    }
    SubShader
    {
        Pass
        {
            CGPROGRAM

            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float2 _iResolution;
            
            float4 mainImage( float2 fragCoord )
            {
                float4 fragColor;
                // Get pos relative to 0-1 screen space
                float2 uv = fragCoord.xy / _iResolution.xy;;
    
                // Map texture to 0-1 space
                float4 texColor = tex2D(_MainTex,uv);
    
                // Default lcd colour (affects brightness)
                float pb = 0.4;
                float4 lcdColor = float4(pb,pb,pb,1.0);
    
                // Change every 1st, 2nd, and 3rd vertical strip to RGB respectively
                int px = fragCoord.x%3.0;
	            if (px == 1) lcdColor.r = 1.0;
                else if (px == 2) lcdColor.g = 1.0;
                else lcdColor.b = 1.0;
    
                // Darken every 3rd horizontal strip for scanline
                float sclV = 0.25;
                if (fragCoord.y%3.0 == 0) lcdColor.rgb = float3(sclV,sclV,sclV);
    
                fragColor = texColor*lcdColor;

                return fragColor;
            }

            struct v2f
            {
                float4 pos : POSITION;
                float2 uv : TEXCOORD0;
            };

            fixed4 frag (v2f i) : SV_Target
            {
                float2 pos = _iResolution.xy*i.uv;
                fixed4 col = mainImage(pos);
                return col;
            }
            ENDCG
        }
    }
}




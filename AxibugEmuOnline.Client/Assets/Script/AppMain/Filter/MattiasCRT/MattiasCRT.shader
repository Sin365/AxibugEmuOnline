
Shader "Filter/MattiasCRT"
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

            #pragma shader_feature_local _QUALITY_LOW _QUALITY_MID _QUALITY_HIGH
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float2 _iResolution;
            
            float2 curve(float2 uv)
            {
                uv = (uv - 0.5) * 2.0;
                uv *= 1.1;	
                uv.x *= 1.0 + pow((abs(uv.y) / 5.0), 2.0);
                uv.y *= 1.0 + pow((abs(uv.x) / 4.0), 2.0);
                uv  = (uv / 2.0) + 0.5;
                uv =  uv *0.92 + 0.04;
                return uv;
            }

            float4 mainImage( float2 fragCoord )
            {
                float4 fragColor = float4(0,0,0,1);
                
                float2 q = fragCoord.xy / _iResolution.xy;
                float2 uv = q;
                uv = curve( uv ); 
                float x =  sin(0.3*_Time+uv.y*21.0)*sin(0.7*_Time+uv.y*29.0)*sin(0.3+0.33*_Time+uv.y*31.0)*0.0017;

                float3 col;
                #if _QUALITY_LOW
                col = tex2D(_MainTex,uv);
                #elif _QUALITY_MID
                col = tex2D(_MainTex,float2(x+uv.x+0.001,uv.y+0.001))+0.05;
                float3 tmpColor2 = tex2D(_MainTex,0.75*float2(x+0.025, -0.027)+float2(uv.x+0.001,uv.y+0.001));
                col.r+=tmpColor2.x*0.08;
                col.g+=tmpColor2.y*0.05;
                col.b+=tmpColor2.z*0.08;
                #else
                col.r = tex2D(_MainTex,float2(x+uv.x+0.001,uv.y+0.001)).x+0.05;
                col.g = tex2D(_MainTex,float2(x+uv.x+0.000,uv.y-0.002)).y+0.05;
                col.b = tex2D(_MainTex,float2(x+uv.x-0.002,uv.y+0.000)).z+0.05;
                col.r += 0.08*tex2D(_MainTex,0.75*float2(x+0.025, -0.027)+float2(uv.x+0.001,uv.y+0.001)).x;
                col.b += 0.08*tex2D(_MainTex,0.75*float2(x+-0.02, -0.018)+float2(uv.x-0.002,uv.y+0.000)).z;
                col.g += 0.05*tex2D(_MainTex,0.75*float2(x+-0.022, -0.02)+float2(uv.x+0.000,uv.y-0.002)).y;
                #endif


                col = clamp(col*0.6+0.4*col*col*1.0,0.0,1.0);

                float vig = (0.0 + 1.0*16.0*uv.x*uv.y*(1.0-uv.x)*(1.0-uv.y));
                col *= pow(vig,0.3);

                col *= float3(0.95,1.05,0.95);
                col *= 2.8;

                float scans = clamp( 0.35+0.35*sin(3.5*_Time+uv.y*_iResolution.y*1.5), 0.0, 1.0);
                
                float s = pow(scans,1.7);
                col = col*( 0.4+0.7*s) ;

                col *= 1.0+0.01*sin(110.0*_Time);
                if (uv.x < 0.0 || uv.x > 1.0)
                    col *= 0.0;
                if (uv.y < 0.0 || uv.y > 1.0)
                    col *= 0.0;
                
                
                col*=1.0-0.65*clamp((fragCoord.x % 2.0 -1.0 )*2.0,0.0,1.0);
                
                float comp = smoothstep( 0.1, 0.9, sin(_Time) );

                fragColor = float4(col,1.0);

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




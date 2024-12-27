Shader "Filter/RetroArch/MattiasCRTWithGlow"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white"
        _gamma("Input Gamma option: 2.4/2.0/2.6/0.02",float) = 11
        _horiz_gauss_width("Gaussian Width option: 0.5/0.4/0.6/0.02",float) = 0.4
        _BOOST("Color Boost option: 1.0/0.5/1.5/0.02",float) = 0.4
        _GLOW_WHITEPOINT("Glow Whitepoint option: 1.0/0.5/1.1/0.02",float) = 0.4
        _GLOW_ROLLOFF("Glow Rolloff option: 3.0 1.2 6.0 0.1",float) = 2.2
        _BLOOM_STRENGTH("Glow Strength option: 0.45/0.0/0.8/0.05",float) = 0.45
        _OUTPUT_GAMMA("Monitor Gamma option: 2.2/1.8/2.6/0.02",float) = 2.2
    }
    SubShader
    {        
        Pass
        {
            Name "linearize"

            CGPROGRAM

            #include "UnityCG.cginc"
            #pragma vertex vert_img
            #pragma fragment frag

            half _gamma;
            sampler2D _MainTex;
            float2 _iResolution;

            struct v2f
            {
                float4 pos : POSITION;
                float2 uv : TEXCOORD0;
            };


            fixed4 frag(v2f i) : SV_Target
            {
                fixed3 color = tex2D(_MainTex,i.uv); 
                fixed4 result = fixed4(pow(color,_gamma),1.0);
                return result;
            }
            ENDCG
        }
                
        Pass
        {
            Name "MattiasCRT"

            CGPROGRAM

            #include "UnityCG.cginc"
            #pragma vertex vert_img
            #pragma fragment frag

            sampler2D _MainTex;
            float2 _iResolution;

            struct v2f
            {
                float4 pos : POSITION;
                float2 uv : TEXCOORD0;
            };

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
                col.r = tex2D(_MainTex,float2(x+uv.x+0.001,uv.y+0.001)).x+0.05;
                col.g = tex2D(_MainTex,float2(x+uv.x+0.000,uv.y-0.002)).y+0.05;
                col.b = tex2D(_MainTex,float2(x+uv.x-0.002,uv.y+0.000)).z+0.05;
                col.r += 0.08*tex2D(_MainTex,0.75*float2(x+0.025, -0.027)+float2(uv.x+0.001,uv.y+0.001)).x;
                col.b += 0.08*tex2D(_MainTex,0.75*float2(x+-0.02, -0.018)+float2(uv.x-0.002,uv.y+0.000)).z;
                col.g += 0.05*tex2D(_MainTex,0.75*float2(x+-0.022, -0.02)+float2(uv.x+0.000,uv.y-0.002)).y;               


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

            fixed4 frag(v2f i) : SV_Target
            {
                float2 pos = _iResolution.xy*i.uv;
                fixed4 col = mainImage(pos);
                return col;
            }
            ENDCG
        }

        Pass
        {
            Name "gauss_horiz"

            CGPROGRAM

            #include "UnityCG.cginc"
            #pragma vertex vert_img
            #pragma fragment frag

            #define INV_SQRT_2_PI 0.38

            sampler2D _MainTex;
            float2 _iResolution;
            float _horiz_gauss_width;

            struct v2f
            {
                float4 pos : POSITION;
                float2 uv : TEXCOORD0;
            };

            fixed4 gauss_horiz(v2f IN){
                float one = 1.0/_iResolution.x;
                float pix_no = _iResolution.x*IN.uv.x;
                float texel = floor(pix_no);
                float phase = pix_no-texel;
                float base_phase = phase - 0.5;
                float2 tex = float2((texel+0.5)/_iResolution.x,IN.uv.y);

                #define TEX(off_x) tex2D(_MainTex, IN.uv + float2(float(off_x) * one, 0.0)).rgb

                float3 col = float3(0,0,0);
                for(int i=-2;i<=2;i++){
                    float phase = base_phase - float(i);
                    float g = INV_SQRT_2_PI * exp(-0.5 * phase * phase / (_horiz_gauss_width * _horiz_gauss_width)) / _horiz_gauss_width;
                    col+=TEX(i)*g;
                }

                return fixed4(col,1);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 color=gauss_horiz(i);

                return color;
            }
            ENDCG
        }

        Pass
        {
            Name "gauss_vert"

            CGPROGRAM

            #pragma shader_feature_local _CRT_GEOM_ON _CRT_GEOM_OFF

            #include "UnityCG.cginc"
            #pragma vertex vert_img
            #pragma fragment frag

            sampler2D _MainTex;
            float2 _iResolution;
            float _BOOST;

            struct v2f
            {
                float4 pos : POSITION;
                float2 uv : TEXCOORD0;
            };

            float3 beam(float3 color,float dist){
                #if CRT_GEOM_BEAM
                float3 wid = 2.0 + 2.0 * pow(color, 4.0);
                float3 weights = float3(abs(dist) / 0.3);
                return 2.0 * color * exp(-pow(weights * rsqrt(0.5 * wid), wid)) / (0.6 + 0.2 * wid);
                #else
                const float width = 0.25;
                float3 x = dist / width;
                return 2.0 * color * exp(-0.5 * x * x) / width;
                #endif
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float2 pix_no = IN.uv*_iResolution - float2(0,0.5);
                float one = 1.0/ _iResolution.y;

                #define TEX(off_y) tex2D(_MainTex, IN.uv + float2(0.0, off_y * one)).rgb

                float2 texel = floor(pix_no);
                float phase = pix_no.y - texel.y;
                float2 tex = float2(texel + 0.5) / _iResolution;

                float3 top = TEX(0);
                float3 bottom = TEX(1);

                float dist0 = phase;
                float dist1 = 1.0 - phase;

                float3 scanline = float3(0,0,0);

                scanline += beam(top, dist0);
                scanline += beam(bottom, dist1);

                return float4(_BOOST * scanline / 1.15, 1.0);
            }
            ENDCG
        }

        Pass
        {
            Name "threshold"

            CGPROGRAM

            #include "UnityCG.cginc"
            #pragma vertex vert_img
            #pragma fragment frag

            sampler2D _MainTex;
            float2 _iResolution;

            float _GLOW_WHITEPOINT;
            float _GLOW_ROLLOFF;

            struct v2f
            {
                float4 pos : POSITION;
                float2 uv : TEXCOORD0;
            };

            float4 frag(v2f IN) : SV_Target
            {
                float3 color = 1.15*tex2D(_MainTex,IN.uv).rgb;
                float3 factor = saturate(color / _GLOW_WHITEPOINT);
                return float4(pow(factor,_GLOW_ROLLOFF),1.0);
            }
            ENDCG
        }

        Pass
        {
            Name "blur_horiz"

            CGPROGRAM

            #include "UnityCG.cginc"
            #pragma vertex vert_img
            #pragma fragment frag

            sampler2D _MainTex;
            float2 _iResolution;

            struct v2f
            {
                float4 pos : POSITION;
                float2 uv : TEXCOORD0;
            };

            #include "blur_params.cginc"
            #define kernel(x) exp(-GLOW_FALLOFF * (x) * (x))

            float4 frag(v2f IN) : SV_Target
            {
                float3 col = float3(0,0,0);
                float dx = 4.0 / _iResolution.x; // Mipmapped

                float k_total = 0.0;
                for (int i = -TAPS; i <= TAPS; i++)
                {
                    float k = kernel(i);
                    k_total += k;
                    col += k * tex2D(_MainTex, IN.uv + float2(float(i) * dx, 0.0)).rgb;
                }
                return float4(col / k_total, 1.0);
            }
            ENDCG
        }

        Pass
        {
            Name "blur_vert"

            CGPROGRAM

            #include "UnityCG.cginc"
            #pragma vertex vert_img
            #pragma fragment frag

            sampler2D _MainTex;
            float2 _iResolution;

            struct v2f
            {
                float4 pos : POSITION;
                float2 uv : TEXCOORD0;
            };

            #include "blur_params.cginc"
            #define kernel(x) exp(-GLOW_FALLOFF * (x) * (x))

            float4 frag(v2f IN) : SV_Target
            {
                float3 col = float3(0,0,0);
                float dy = 1.0 / _iResolution.y;

                float k_total = 0.0;
                for (int i = -TAPS; i <= TAPS; i++)
                {
                    float k = kernel(i);
                    k_total += k;
                    col += k * tex2D(_MainTex, IN.uv + float2(0.0, float(i) * dy)).rgb;
                }
                return float4(col / k_total, 1.0);
            }
            ENDCG
        }
        
        Pass
        {
            Name "resolve"

            CGPROGRAM

            #include "UnityCG.cginc"
            #pragma vertex vert_img
            #pragma fragment frag

            sampler2D _MainTex;
            sampler2D _Source;
            float2 _iResolution;

            float _BLOOM_STRENGTH;
            float _OUTPUT_GAMMA;

            struct v2f
            {
                float4 pos : POSITION;
                float2 uv : TEXCOORD0;
            };

            float4 frag(v2f IN) : SV_Target
            {
                #if 1
                float3 source = _BLOOM_STRENGTH * tex2D(_MainTex,IN.uv).rgb;
                #else
                float3 source = 1.15 * tex2D(_Source,IN.uv).rgb;
                float3 bloom = tex2D(_MainTex,IN.uv).rgb;
                source += _BLOOM_STRENGTH * bloom;
                #endif
                return float4(pow(saturate(source), 1.0 / _OUTPUT_GAMMA), 1.0);
            }
            ENDCG
        }
    }
}




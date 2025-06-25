Shader "Filter/RealisticCRT"
{
    Properties
    {
            _MainTex ("Texture", 2D) = "white" {}
        
            scan_line_amount ("scan_line_amount",Range(0,1)) = 1.0
            warp_amount ("warp_amount",Range(0,5)) = 0.1
            noise_amount ("noise_amount",Range(0.0, 0.3)) = 0.03
            interference_amount ("interference_amount",Range(0.0, 1.0)) = 0.2
            grille_amount ("grille_amount",Range(0.0, 1.0)) = 0.1
            grille_size ("grille_size",Range(1.0, 5.0)) = 1.0
            vignette_amount ("vignette_amount",Range(0.0, 2.0)) = 0.6
            vignette_intensity ("vignette_intensity",Range (0.0, 1.0)) = 0.4
            aberation_amount ("aberation_amount",Range(0.0, 1.0)) = 0.5
            roll_line_amount ("roll_line_amount",Range(0.0, 1.0)) = 0.3
            roll_speed ("roll_speed",Range(-8.0, 8.0)) = 1.0
            scan_line_strength ("scan_line_strength",Range(-12.0, -1.0))= -8.0
            pixel_strength ("pixel_strength",Range(-4.0, 0.0))= -2.0
    }
    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct v2f
            {
                float4 pos : POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float2 _iResolution;
            
            #define resolution float2(320.0,180.0)
            #define PI 3.14159268

            float scan_line_amount;
            float warp_amount;
            float noise_amount;
            float interference_amount;
            float grille_amount;
            float grille_size;
            float vignette_amount;
            float vignette_intensity;
            float aberation_amount;
            float roll_line_amount;
            float roll_speed;
            float scan_line_strength;
            float pixel_strength;

            float random(float2 uv){
                return frac(cos(uv.x * 83.4827 + uv.y * 92.2842) * 43758.5453123);
            }

            float3 fetch_pixel(float2 uv, float2 off)
            {
                float2 pos = floor(uv * resolution + off) / resolution + float2(0.5,0.5) / resolution;

                float noise = 0.0;
                if(noise_amount > 0.0){
                    noise = random(pos + frac(_Time.x)) * noise_amount;
                }

                if(max(abs(pos.x - 0.5), abs(pos.y - 0.5)) > 0.5){
                    return float3(0.0, 0.0, 0.0);
                }

                float3 clr = tex2D(_MainTex,pos).rgb + noise;
                return clr;
            }

            // Distance in emulated pixels to nearest texel.
            float2 Dist(float2 pos){ 
                pos = pos * resolution;
                return - ((pos - floor(pos)) - float2(0.5,0.5));
            }
                
            // 1D Gaussian.
            float Gaus(float pos, float scale){ return exp2(scale * pos * pos); }

            // 3-tap Gaussian filter along horz line.
            float3 Horz3(float2 pos, float off){
                float3 b = fetch_pixel(pos, float2(-1.0, off));
                float3 c = fetch_pixel(pos, float2( 0.0, off));
                float3 d = fetch_pixel(pos, float2( 1.0, off));
                float dst = Dist(pos).x;
                
                // Convert distance to weight.
                float scale = pixel_strength;
                float wb = Gaus(dst - 1.0, scale);
                float wc = Gaus(dst + 0.0, scale);
                float wd = Gaus(dst + 1.0, scale);
                
                // Return filtered sample.
                return (b * wb + c * wc + d * wd) / (wb + wc + wd);
            }

            // Return scanline weight.
            float Scan(float2 pos, float off){
                float dst = Dist(pos).y;
                
                return Gaus(dst + off, scan_line_strength);
            }

            // Allow nearest three lines to effect pixel.
            float3 Tri(float2 pos){
                float3 clr = fetch_pixel(pos, float2(0.0,0.0));
                if(scan_line_amount > 0.0){
                    float3 a = Horz3(pos,-1.0);
                    float3 b = Horz3(pos, 0.0);
                    float3 c = Horz3(pos, 1.0);

                    float wa = Scan(pos,-1.0);
                    float wb = Scan(pos, 0.0);
                    float wc = Scan(pos, 1.0);

                    float3 scanlines = a * wa + b * wb + c * wc;
                    clr = lerp(clr, scanlines, scan_line_amount);
                }
                return clr;
            }

            // Takes in the UV and warps the edges, creating the spherized effect
            float2 warp(float2 uv){
                float2 delta = uv - 0.5;
                float delta2 = dot(delta.xy, delta.xy);
                float delta4 = delta2 * delta2;
                float delta_offset = delta4 * warp_amount;
                
                float2 warped = uv + delta * delta_offset;
                return (warped - 0.5) / lerp(1.0,1.2,warp_amount/5.0) + 0.5;
            }

            float vignette(float2 uv){
                uv *= 1.0 - uv.xy;
                float vignette = uv.x * uv.y * 15.0;
                return pow(vignette, vignette_intensity * vignette_amount);
            }

            float floating_mod(float a, float b){
                return a - b * floor(a/b);
            }

            float3 grille(float2 uv){
                float unit = PI / 3.0;
                float scale = 2.0*unit/grille_size;
                float r = smoothstep(0.5, 0.8, cos(uv.x*scale - unit));
                float g = smoothstep(0.5, 0.8, cos(uv.x*scale + unit));
                float b = smoothstep(0.5, 0.8, cos(uv.x*scale + 3.0*unit));
                return lerp(float3(1.0,1.0,1.0), float3(r,g,b), grille_amount);
            }

            float roll_line(float2 uv){
                float x = uv.y * 3.0 - _Time.x * roll_speed;
                float f = cos(x) * cos(x * 2.35 + 1.1) * cos(x * 4.45 + 2.3);
                float roll_line = smoothstep(0.5, 0.9, f);
                return roll_line * roll_line_amount;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 pix = _iResolution.xy*i.uv;
                float2 pos = warp(i.uv);
                
                float tLine = 0.0;
                if(roll_line_amount > 0.0){
                    tLine = roll_line(pos);
                } 

                float2 sq_pix = floor(pos * resolution) / resolution + float2(0.5,0.5) / resolution;
                if(interference_amount + roll_line_amount > 0.0){
                    float interference = random(sq_pix.yy + frac(_Time.x));
                    pos.x += (interference * (interference_amount + tLine * 6.0)) / resolution.x;
                }

                float3 clr = Tri(pos);
                if(aberation_amount > 0.0){
                    float chromatic = aberation_amount + tLine * 2.0;
                    float2 chromatic_x = float2(chromatic,0.0) / resolution.x;
                    float2 chromatic_y = float2(0.0, chromatic/2.0) / resolution.y;
                    float r = Tri(pos - chromatic_x).r;
                    float g = Tri(pos + chromatic_y).g;
                    float b = Tri(pos + chromatic_x).b;
                    clr = float3(r,g,b);
                }
                
                if(grille_amount > 0.0)clr *= grille(pix);
                clr *= 1.0 + scan_line_amount * 0.6 + tLine * 3.0 + grille_amount * 2.0;
                if(vignette_amount > 0.0)clr *= vignette(pos);
                
                return fixed4(clr,1.0);
            }
            ENDCG
        }
    }
}

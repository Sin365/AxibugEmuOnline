Shader "Filter/RetroArch/Glow/resolve"
{
    Properties
    {
        BLOOM_STRENGTH("BLOOM_STRENGTH",float) = 0.45
        OUTPUT_GAMMA("Monitor Gamma",float) = 2.2
        CURVATURE("Curvature",float) = 0
        warpX("Curvature X-Axis",float) = 0.031
        warpY("Curvature Y-Axis",float) = 0.041     
        cornersize("Corner Size",float)    = 0.01   
        cornersmooth("Corner Smoothness",float)     = 1000
        noise_amt("Noise Amount",float) = 1.0
        shadowMask("Mask Effect",float) = 0.0
        maskDark("maskDark",float) = 0.5
        maskLight("maskLight",float) = 1.5
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

            #define iTime mod(float(FrameCount) / 60.0, 600.0)
            #define fragCoord (vTexCoord.xy * OutputSize.xy)
            // For debugging
            #define BLOOM_ONLY 0

            #define CRT_PASS CRTPass

            sampler2D CRTPass;
            float BLOOM_STRENGTH;
            float OUTPUT_GAMMA;
            float CURVATURE;
            float warpX;
            float warpY;
            float cornersize;
            float cornersmooth;
            float noise_amt;
            float shadowMask;
            float maskDark;
            float maskLight;

            struct v2f
            {
                float4 pos : POSITION;
                float2 uv : TEXCOORD0;
            };

            

            // Convert from linear to sRGB.
            vec4 Srgb(vec4 c){
                float temp = 1.0/2.2;
                return pow(c, vec4(temp,temp,temp,temp));
            }
            // Convert from sRGB to linear.
            float Linear(float c){return pow(c, 2.2);}

            float rand(vec2 n) { 
            	return fract(sin(dot(n, vec2(12.9898, 4.1414))) * 43758.5453);
            }

            float noise(vec2 p){
                vec2 ip = floor(p);
                vec2 u = fract(p);
                u = u*u*(3.0-2.0*u);
                
                float res = mix(
                    mix(rand(ip),rand(ip+vec2(1.0,0.0)),u.x),
                    mix(rand(ip+vec2(0.0,1.0)),rand(ip+vec2(1.0,1.0)),u.x),u.y);
                return res*res;
            }

            const vec2 corner_aspect   = vec2(1.0,  0.75);

            float corner(vec2 coord)
            {
                coord = (coord - vec2(0.5,0.5)) + vec2(0.5, 0.5);
                coord = min(coord, vec2(1.0,1.0) - coord) * corner_aspect;
                vec2 cdist = vec2(cornersize,cornersize);
                coord = (cdist - min(coord, cdist));
                float dist = sqrt(dot(coord, coord));
                
                return clamp((cdist.x - dist)*cornersmooth, 0.0, 1.0);
            }

            vec2 Warp(float Distortion,vec2 position,vec2 texCoord){
                float offset_x = noise(sin(position.xy) * float(mod(FrameCount, 361.)));
                float offset_y = noise(cos(position.yx) * float(mod(FrameCount, 873.)));
                vec2 noisecoord = texCoord + vec2(offset_x, offset_y) * 0.001 * noise_amt;
                vec2 curvedCoords = noisecoord * 2.0 - 1.0;
                float curvedCoordsDistance = sqrt(curvedCoords.x*curvedCoords.x+curvedCoords.y*curvedCoords.y);

                curvedCoords = curvedCoords / curvedCoordsDistance;
                
                float temp = 1.0-(curvedCoordsDistance/1.4142135623730950488016887242097);
                float2 powP1 = float2(temp,temp);
                float powP2 = 1.0 / (1.0 + Distortion * 0.2 );
                curvedCoords = curvedCoords * (1.0-pow(powP1,powP2));

                curvedCoords = curvedCoords / (1.0-pow(vec2(0.29289321881345247559915563789515,0.29289321881345247559915563789515),(1.0/(vec2(1.0,1.0)+Distortion*0.2))));

                curvedCoords = curvedCoords * 0.5 + 0.5;
                return curvedCoords;
            }

            // Shadow mask.
            vec3 Mask(vec2 pos)
            {
                vec3 mask = vec3(maskDark, maskDark, maskDark);
            
                // Very compressed TV style shadow mask.
                if (shadowMask == 1.0)
                {
                    float line = maskLight;
                    float odd = 0.0;
                
                    if (fract(pos.x*0.166666666) < 0.5) odd = 1.0;
                    if (fract((pos.y + odd) * 0.5) < 0.5) line = maskDark;  
                
                    pos.x = fract(pos.x*0.333333333);
            
                    if      (pos.x < 0.333) mask.r = maskLight;
                    else if (pos.x < 0.666) mask.g = maskLight;
                    else                    mask.b = maskLight;
                    mask*=line;  
                }
            
                // Aperture-grille.
                else if (shadowMask == 2.0)
                {
                    pos.x = fract(pos.x*0.333333333);
            
                    if      (pos.x < 0.333) mask.r = maskLight;
                    else if (pos.x < 0.666) mask.g = maskLight;
                    else                    mask.b = maskLight;
                }
            
                // Stretched VGA style shadow mask (same as prior shaders).
                else if (params.shadowMask == 3.0)
                {
                    pos.x += pos.y*3.0;
                    pos.x  = fract(pos.x*0.166666666);
            
                    if      (pos.x < 0.333) mask.r = params.maskLight;
                    else if (pos.x < 0.666) mask.g = params.maskLight;
                    else                    mask.b = params.maskLight;
                }
            
                // VGA style shadow mask.
                else if (params.shadowMask == 4.0)
                {
                    pos.xy  = floor(pos.xy*vec2(1.0, 0.5));
                    pos.x  += pos.y*3.0;
                    pos.x   = fract(pos.x*0.166666666);
            
                    if      (pos.x < 0.333) mask.r = params.maskLight;
                    else if (pos.x < 0.666) mask.g = params.maskLight;
                    else                    mask.b = params.maskLight;
                }
            
                return mask;
            }

            fixed4 frag (v2f IN) : SV_Target
            {
                float Distortion = vec2(warpX, warpY) * 15;

                float4 FragColor = float4(0,0,0,1);
                vec2 pp = vTexCoord.xy;
                pp = (params.CURVATURE > 0.5) ? Warp(IN.pos,pp) : pp;
                
                #if BLOOM_ONLY
                    vec3 source = BLOOM_STRENGTH * texture(Source, pp).rgb;
                #else
                
                    vec3 source = 1.15 * texture(CRT_PASS, pp).rgb;
                    vec3 bloom  = texture(Source, pp).rgb;
                    source     += params.BLOOM_STRENGTH * bloom;
                #endif
                    FragColor = vec4(pow(clamp(source, 0.0, 1.0), vec3(1.0 / params.OUTPUT_GAMMA)), 1.0);
                    /* TODO/FIXME - hacky clamp fix */
                    if ( pp.x > 0.0001 && pp.x < 0.9999 && pp.y > 0.0001 && pp.y < 0.9999)
                        FragColor.rgb = FragColor.rgb;
                    else
                FragColor.rgb = vec3(0.0);
                FragColor.rgb *= (params.CURVATURE > 0.5) ? corner(pp) : 1.0;
                if (params.shadowMask > 0.0)
                        FragColor.rgb = pow(pow(FragColor.rgb, vec3(2.2)) * Mask(vTexCoord.xy * global.OutputSize.xy * 1.000001), vec3(1.0 / 2.2));

                return FragColor;
            }
            ENDCG
        }
    }
}




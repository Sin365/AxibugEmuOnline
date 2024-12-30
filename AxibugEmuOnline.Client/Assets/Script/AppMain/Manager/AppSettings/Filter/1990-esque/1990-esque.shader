
Shader "Filter/1990-esque"
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

            #define vec2 float2
            #define vec3 float3
            #define vec4 float4
            #define mod(x,y) x%y

            #define MASK_SCALE	600.0
            #define MASK_STRENGTH			0.35

            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float2 _iResolution;

            struct v2f
            {
                float4 pos : POSITION;
                float2 uv : TEXCOORD0;
            };

            vec3 TriadMask( vec2 pos ) {
                vec3 col = vec3(0,0,0);
                float h = sqrt(3.0)/2.0;
                pos.x = mod(pos.x, 3.0);
                pos.y = mod(pos.y, h*2.0);
                col += vec3(1,0,0) * max( 1.0-distance(pos, vec2(0.0,0.0)   ), 0.0 );
                col += vec3(0,1,0) * max( 1.0-distance(pos, vec2(1.0,0.0)   ), 0.0 );
                col += vec3(0,0,1) * max( 1.0-distance(pos, vec2(2.0,0.0)   ), 0.0 );
                col += vec3(1,0,0) * max( 1.0-distance(pos, vec2(3.0,0.0)   ), 0.0 );
                col += vec3(0,1,0) * max( 1.0-distance(pos, vec2(-0.5,h)    ), 0.0 );
                col += vec3(0,0,1) * max( 1.0-distance(pos, vec2(0.5,h)     ), 0.0 );
                col += vec3(1,0,0) * max( 1.0-distance(pos, vec2(1.5,h)     ), 0.0 );
                col += vec3(0,1,0) * max( 1.0-distance(pos, vec2(2.5,h)     ), 0.0 );
                col += vec3(0,0,1) * max( 1.0-distance(pos, vec2(3.5,h)     ), 0.0 );
                col += vec3(1,0,0) * max( 1.0-distance(pos, vec2(0.0,h*2.0) ), 0.0 );
                col += vec3(0,1,0) * max( 1.0-distance(pos, vec2(1.0,h*2.0) ), 0.0 );
                col += vec3(0,0,1) * max( 1.0-distance(pos, vec2(2.0,h*2.0) ), 0.0 );
                col += vec3(1,0,0) * max( 1.0-distance(pos, vec2(3.0,h*2.0) ), 0.0 );
                col - min(col, 1.0);
                col = smoothstep(0.4, 0.65, col);
                return col;
            }


            vec3 SlotMask( vec2 pos ) {
                vec3 col = vec3(0,0,0);
                col += vec3(1,0,0) * max( (1.0 - distance(mod(pos.x,3.0),0.5) ), 0.0 );
                col += vec3(0,1,0) * max( (1.0 - distance(mod(pos.x,3.0),1.5) ), 0.0 );
                col += vec3(0,0,1) * max( (1.0 - distance(mod(pos.x,3.0),2.5) ), 0.0 );
                col += vec3(1,0,0) * max( (1.0 - distance(mod(pos.x,3.0),3.5) ), 0.0 );
                float b = pos.y;
                b += (frac(pos.x/6.0) > 0.5) ? 1.5 : 0.0;
                b = mod(b,3.0);
                col -= (b<0.35) ? 1.0 : 0.0;
                col = smoothstep(0.6, 0.7, col);
                return col;
            }


            vec3 TriadMask_PixelPerfect( vec2 pos ) {
                float f = frac( pos.x/6.0 + floor(pos.y/2.0)/2.0);
                vec3 col = (f<=0.333) ? vec3(1,0,0) : ( (f<0.667) ? vec3(0,1,0) : vec3(0,0,1) ); 
                return col;
            }


            vec3 SlotMask_PixelPerfect( vec2 pos ) {
                pos /= 1.0 + floor( _iResolution.y / 1440.0 );
                float glow = 0.5;
                float f = mod(pos.x,3.0);
                vec3 col = vec3( (f<=1.0), (f>1.0&&f<=2.0), (f>2.0) );
                col += vec3( (f<1.5 || f>=2.5), (f>0.5 && f<=2.5), (f>1.5 || f<=0.5) ) * glow;
                col *= ( mod(pos.y+(frac(pos.x/6.0)>0.5?1.5:0.0),3.0)<1.0 ) ? glow : 1.0;
                col /= 1.0+glow;
                return col;
            }


            vec3 ApertureGrille_PixelPerfect( vec2 pos ) {
                pos /= 1.0 + floor( _iResolution.y / 1440.0 );
                float f = frac(pos.x/3.0);
                vec3 col = vec3( (f<=0.333), (f>0.333&&f<0.667), (f>=0.667) );
                return col;
            }

            fixed4 frag (v2f IN) : SV_Target
            {
                vec2 pos = IN.uv*MASK_SCALE;
                float2 fragCoord = _iResolution*pos;

                //vec3 col = TriadMask( pos );
                //vec3 col = SlotMask( pos );
                //vec3 col = TriadMask_PixelPerfect( fragCoord );
                vec3 col = SlotMask_PixelPerfect( fragCoord );
                //vec3 col = ApertureGrille_PixelPerfect( fragCoord );
                
                float temp = 1-MASK_STRENGTH;
                float temp2 = 1+MASK_STRENGTH;
                col = lerp( vec3(temp,temp,temp), vec3(temp2,temp2,temp2), col);
                
                //if (fract(fragCoord.x/200.0)<0.02) col = vec3(10.0);
                //if (fract(fragCoord.y/200.0)<0.02) col = vec3(10.0);
                
                return vec4(col,1.0);
            }
            ENDCG
        }

        Pass
        {
            CGPROGRAM

            #define vec2 float2
            #define vec3 float3
            #define vec4 float4

            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float2 _iResolution;
            uniform sampler2D _ShadowMaskTex;

            struct v2f
            {
                float4 pos : POSITION;
                float2 uv : TEXCOORD0;
            };

            #define gaussian(a,b)	exp2((a)*(a)*-(b))
            #define SCANLINES
            #define SCREEN_SHAPE
            #define LIGHT_EFFECTS

            //#define CURVE_MASK_TO_SCREEN
            #define SCREEN_CURVE_RADIUS		5.0
            #define SCREEN_CORNER_RADIUS	0.1
            #define BRIGHTNESS      		2.5
            #define PIXEL_SHARPNESS   		3.0
            #define LINE_SHARPNESS			6.0

            #define IRES		vec2(256,239)
            #define PI 3.14159265

            vec2 curveScreen( vec2 uv ) {
                float r = PI*0.5/SCREEN_CURVE_RADIUS;
                float d = 1.0-cos(uv.x*r)*cos(uv.y*r);		//distance to screen
                float s = cos(r);							//scale factor to re-fit window
                return uv / (1.0-d) * s;
            }


            float discardCorners( vec2 pos ) {
                pos = abs(pos);
                pos.x = pos.x*1.333-0.333;											// 4:3 aspect ratio correction
                if( min(pos.x, pos.y) < 1.0-SCREEN_CORNER_RADIUS ) return 1.0;		// not near corner -- break early
                float d = distance( pos, vec2(1.0-SCREEN_CORNER_RADIUS,1.0-SCREEN_CORNER_RADIUS) );
                return float( d<SCREEN_CORNER_RADIUS );
            }


            vec3 getSample( vec2 pos, vec2 off ) {
                //get nearest emulated sample
                pos = floor(pos*IRES) + vec2(0.5,0.5) + off;
                pos/=IRES;
                vec3 col = vec3(0,0,0);
                if ( pos.x>=0.0 && pos.x<=IRES.x && pos.y>=0.0 && pos.y<=IRES.y ) {
                    col = tex2D( _MainTex, pos).rgb;
                    col = pow( ( (col + 0.055) / 1.055), vec3(2.4,2.4,2.4) );		// SRGB => linear
                }
                return col;
            }


            vec3 getScanline( vec2 pos, float off ) {
                // 3-tap gaussian filter to get colour at arbitrary point along scanline
                float d = 0.5-frac(pos.x*IRES.x);
                vec3 ca = getSample( pos, vec2(-1.0, off ) );
                vec3 cb = getSample( pos, vec2( 0.0, off ) );
                vec3 cc = getSample( pos, vec2( 1.0, off ) );
                float wa = gaussian( d-1.0, PIXEL_SHARPNESS );
                float wb = gaussian( d,     PIXEL_SHARPNESS );
                float wc = gaussian( d+1.0, PIXEL_SHARPNESS );
                return ( ca*wa + cb*wb + cc*wc ) / ( wa+wb+wc);
            }


            vec3 getScreenColour( vec2 pos ) {
                //Get influence of 3 nearest scanlines
                float d = 0.5-frac(pos.y*IRES.y);
                vec3 ca = getScanline( pos,-1.0 ) * gaussian( d-1.0, LINE_SHARPNESS );
                vec3 cb = getScanline( pos, 0.0 ) * gaussian( d,     LINE_SHARPNESS );
                vec3 cc = getScanline( pos, 1.0 ) * gaussian( d+1.0, LINE_SHARPNESS );
                return ca + cb + cc;
            }


            vec3 ACESFilm( vec3 x ) {
                return clamp((x*(2.51*x + 0.03)) / (x*(2.43*x + 0.59) + 0.14), 0.0, 1.0);
            }

            fixed4 frag (v2f IN) : SV_Target
            {   
                vec2 pos = IN.uv;
                pos = pos*2.0 - 1.0;
                //pos.x *= _iResolution.x/_iResolution.y*0.75;						// 4:3 aspect

                #ifdef SCREEN_SHAPE
                pos = curveScreen(pos);											// curve screen
                #endif
                
                vec3 col = vec3(0,0,0);
                
                if(max( abs(pos.x), abs(pos.y) )<1.0) {							// skip everything if we're beyond the screen edge
                    
                    col = vec3(1,1,1);
                    
                    #ifdef SCREEN_SHAPE
                    col *= discardCorners(pos);
                    #endif
                    
                    #ifdef LIGHT_EFFECTS
                    col *= 1.0 - sqrt(length(pos)*0.25);						// vignette
                    #endif

                    pos = pos*0.5 + 0.5;

                    #if 1
                    col *= getScreenColour( pos );
                    #else
                    col = getSample( pos, vec2(0,0) );
                    #endif
                    
                    
                    col *= tex2D( _ShadowMaskTex, pos).rgb;				// draw mask flat (moire fix)
                    
                    #ifdef LIGHT_EFFECTS
                    col *= BRIGHTNESS;
                    col = ACESFilm(col);
                    #endif
                
                    col = pow( col, vec3(1.0/2.4,1.0/2.4,1.0/2.4) ) * 1.055 - 0.055;			// linear => SRGB
                }
                
                return vec4( col, 1.0 );
            }
            ENDCG
        }
    }
}




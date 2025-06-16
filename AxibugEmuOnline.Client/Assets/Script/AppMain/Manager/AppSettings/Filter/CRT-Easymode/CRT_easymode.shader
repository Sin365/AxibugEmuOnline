
Shader "Filter/crt-easymode"
{
    Properties
    {
        BRIGHT_BOOST("BRIGHT_BOOST",Float) = 1.2
        DILATION("DILATION",Float) = 1.0
        GAMMA_INPUT("GAMMA_INPUT",Float) = 2.0
        GAMMA_OUTPUT("GAMMA_OUTPUT",Float) = 1.8
        MASK_SIZE("MASK_SIZE",Float) = 1.0
        MASK_STAGGER("MASK_STAGGER",Float) = 0.0
        MASK_STRENGTH("MASK_STRENGTH",Float) = 0.3
        MASK_DOT_HEIGHT("MASK_DOT_HEIGHT",Float) = 1.0
        MASK_DOT_WIDTH("MASK_DOT_WIDTH",Float) = 1.0
        SCANLINE_CUTOFF("SCANLINE_CUTOFF",Float) = 400.0
        SCANLINE_BEAM_WIDTH_MAX("SCANLINE_BEAM_WIDTH_MAX",Float) = 1.5
        SCANLINE_BEAM_WIDTH_MIN("SCANLINE_BEAM_WIDTH_MIN",Float) = 1.5
        SCANLINE_BRIGHT_MAX("SCANLINE_BRIGHT_MAX",Float) = 0.65
        SCANLINE_BRIGHT_MIN("SCANLINE_BRIGHT_MIN",Float) = 0.35
        SCANLINE_STRENGTH("SCANLINE_STRENGTH",Float) = 1.0
        SHARPNESS_H("SHARPNESS_H",Float) = 0.5
        SHARPNESS_V("SHARPNESS_V",Float) = 1.0
    }
    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment main
            #include "UnityCG.cginc"
            #include "../FilterChain.cginc"           

            float BRIGHT_BOOST;
            float DILATION;
            float GAMMA_INPUT;
            float GAMMA_OUTPUT;
            float MASK_SIZE;
            float MASK_STAGGER;
            float MASK_STRENGTH;
            float MASK_DOT_HEIGHT;
            float MASK_DOT_WIDTH;
            float SCANLINE_CUTOFF;
            float SCANLINE_BEAM_WIDTH_MAX;
            float SCANLINE_BEAM_WIDTH_MIN;
            float SCANLINE_BRIGHT_MAX;
            float SCANLINE_BRIGHT_MIN;
            float SCANLINE_STRENGTH;
            float SHARPNESS_H;
            float SHARPNESS_V;  

            struct v2f
            {
                float4 pos : POSITION;
                float2 uv : TEXCOORD0;
            };

            #define TEX2D(c) dilate(tex2D(Source, c))
            #define PI 3.141592653589

            vec4 dilate(vec4 col)
            {
                vec4 x = mix(vec4(1.0,1.0,1.0,1.0), col, DILATION);

                return col * x;
            }

            float curve_distance(float x, float sharp)
            {

            /*
                apply half-circle s-curve to distance for sharper (more pixelated) interpolation
                single line formula for Graph Toy:
                0.5 - sqrt(0.25 - (x - step(0.5, x)) * (x - step(0.5, x))) * sign(0.5 - x)
            */

                float x_step = step(0.5, x);
                float curve = 0.5 - sqrt(0.25 - (x - x_step) * (x - x_step)) * sign(0.5 - x);

                return mix(x, curve, sharp);
            }

            mat4x4 get_color_matrix(vec2 co, vec2 dx)
            {
                return mat4x4(TEX2D(co - dx), TEX2D(co), TEX2D(co + dx), TEX2D(co + 2.0 * dx));
            }

            vec3 filter_lanczos(vec4 coeffs, mat4x4 color_matrix)
            {
                vec4 col        = mul(color_matrix,coeffs);
                vec4 sample_min = min(color_matrix[1], color_matrix[2]);
                vec4 sample_max = max(color_matrix[1], color_matrix[2]);

                col = clamp(col, sample_min, sample_max);

                return col.rgb;
            }

            vec4 main (v2f IN) : SV_Target
            {
                vec2 dx = vec2(SourceSize.z, 0.0);
                vec2 dy = vec2(0.0, SourceSize.w);
                vec2 pix_co = vTexCoord * SourceSize.xy - vec2(0.5, 0.5);
                vec2 tex_co = (floor(pix_co) + vec2(0.5, 0.5)) * SourceSize.zw;
                vec2 dist = fract(pix_co);
                float curve_x;
                vec3 col, col2;

                #if TRUE
                curve_x = curve_distance(dist.x, SHARPNESS_H * SHARPNESS_H);

                vec4 coeffs = PI * vec4(1.0 + curve_x, curve_x, 1.0 - curve_x, 2.0 - curve_x);

                coeffs = FIX(coeffs);
                coeffs = 2.0 * sin(coeffs) * sin(coeffs * 0.5) / (coeffs * coeffs);
                coeffs /= dot(coeffs, vec4(1.0));

                col = filter_lanczos(coeffs, get_color_matrix(tex_co, dx));
                col2 = filter_lanczos(coeffs, get_color_matrix(tex_co + dy, dx));
                #else
                curve_x = curve_distance(dist.x, SHARPNESS_H);

                col = mix(TEX2D(tex_co).rgb, TEX2D(tex_co + dx).rgb, curve_x);
                col2 = mix(TEX2D(tex_co + dy).rgb, TEX2D(tex_co + dx + dy).rgb, curve_x);
                #endif

                col = mix(col, col2, curve_distance(dist.y, SHARPNESS_V));
                col = pow(col, vec3(GAMMA_INPUT / (DILATION + 1.0),GAMMA_INPUT / (DILATION + 1.0),GAMMA_INPUT / (DILATION + 1.0))); 

                float luma = dot(vec3(0.2126, 0.7152, 0.0722), col);
                float bright = (max(col.r, max(col.g, col.b)) + luma) * 0.5;
                float scan_bright = clamp(bright, SCANLINE_BRIGHT_MIN, SCANLINE_BRIGHT_MAX);
                float scan_beam = clamp(bright * SCANLINE_BEAM_WIDTH_MAX, SCANLINE_BEAM_WIDTH_MIN, SCANLINE_BEAM_WIDTH_MAX);
                float scan_weight = 1.0 - pow(cos(vTexCoord.y * 2.0 * PI * SourceSize.y) * 0.5 + 0.5, scan_beam) * SCANLINE_STRENGTH;

                float mask = 1.0 - MASK_STRENGTH;
                vec2 mod_fac = floor(vTexCoord * OutputSize.xy * SourceSize.xy / (SourceSize.xy * vec2(MASK_SIZE, MASK_DOT_HEIGHT * MASK_SIZE)));
                int dot_no = int(mod((mod_fac.x + mod(mod_fac.y, 2.0) * MASK_STAGGER) / MASK_DOT_WIDTH, 3.0));
                vec3 mask_weight;

                if (dot_no == 0) mask_weight = vec3(1.0, mask, mask);
                else if (dot_no == 1) mask_weight = vec3(mask, 1.0, mask);
                else mask_weight = vec3(mask, mask, 1.0);

                if (SourceSize.y >= SCANLINE_CUTOFF)
                scan_weight = 1.0;

                col2 = col.rgb;
                col *= vec3(scan_weight,scan_weight,scan_weight);
                col = mix(col, col2, scan_bright);
                col *= mask_weight;
                col = pow(col, vec3(1.0 / GAMMA_OUTPUT,1.0 / GAMMA_OUTPUT,1.0 / GAMMA_OUTPUT));

                col = col* BRIGHT_BOOST;

                vec4 FragColor = vec4(col, 1.0);

                return FragColor;
            }
            ENDCG
        }
    }
}




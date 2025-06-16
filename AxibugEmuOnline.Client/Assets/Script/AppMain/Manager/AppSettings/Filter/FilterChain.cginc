#define vec2 float2
#define vec3 float3
#define vec4 float4
#define texture(tex,uv) tex2D(tex,uv)
#define vTexCoord IN.uv
#define fract frac
#define mix lerp
#define mod(x,y) x%y
#define mat4x4 float4x4

float sinc(float x) { if (x == 0) { return 1.0; } else { return sin(x) / x; } }

sampler2D Original;
float4 OriginalSize;
sampler2D Source;
float4 SourceSize;
float4 OutputSize;
float FrameCount;
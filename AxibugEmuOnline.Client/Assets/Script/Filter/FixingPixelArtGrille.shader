
Shader "PostEffect/FixingPixcelArtGrille"
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

            #pragma shader_feature_local _MASKSTYLE_TVSTYLE _MASKSTYLE_APERTUREGRILLE _MASKSTYLE_STRETCHEDVGA _MASKSTYLE_VGASTYLE
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;

//
// PUBLIC DOMAIN CRT STYLED SCAN-LINE SHADER
//
//   by Timothy Lottes
//
// This is more along the style of a really good CGA arcade monitor.
// With RGB inputs instead of NTSC.
// The shadow mask example has the mask rotated 90 degrees for less chromatic aberration.
//
// Left it unoptimized to show the theory behind the algorithm.
//
// It is an example what I personally would want as a display option for pixel art games.
// Please take and use, change, or whatever.
//

float2 _iResolution = float2(1920,1080);

// Emulated input resolution.
  // Optimize for resize.
float2 _res = float2(272.0,240.0);

// Hardness of scanline.
//  -8.0 = soft
// -16.0 = medium
float _hardScan = -10.0;

// Hardness of pixels in scanline.
// -2.0 = soft
// -4.0 = hard
float _hardPix =-2.0;

// Hardness of short vertical bloom. 
//  -1.0 = wide to the point of clipping (bad)
//  -1.5 = wide
//  -4.0 = not very wide at all
float _hardBloomScan = -4.0;

// Hardness of short horizontal bloom.
//  -0.5 = wide to the point of clipping (bad)
//  -1.0 = wide
//  -2.0 = not very wide at all
float _hardBloomPix = -1.5;

// Amount of small bloom effect.
//  1.0/1.0 = only bloom
//  1.0/16.0 = what I think is a good amount of small bloom
//  0.0     = no bloom
float _bloomAmount = 1.0/16.0;

// Display warp.
// 0.0 = none
// 1.0/8.0 = extreme 
float2 _warp =  float2(1.0/64.0,1.0/24.0);

// Amount of shadow mask.
float _maskDark = 0.5;
float _maskLight = 1.5;

//------------------------------------------------------------------------

float fract(float x){
  return x-floor(x);
}

// sRGB to Linear.
// Assuing using sRGB typed textures this should not be needed.
float ToLinear1(float c){return(c<=0.04045)?c/12.92:pow((c+0.055)/1.055,2.4);}
float3 ToLinear(float3 c){return float3(ToLinear1(c.r),ToLinear1(c.g),ToLinear1(c.b));}

// Linear to sRGB.
// Assuing using sRGB typed textures this should not be needed.
float ToSrgb1(float c){return(c<0.0031308?c*12.92:1.055*pow(c,0.41666)-0.055);}
float3 ToSrgb(float3 c){return float3(ToSrgb1(c.r),ToSrgb1(c.g),ToSrgb1(c.b));}


 float3 Test(float3 c){return c*(1.0/64.0)+c*c*c;}

// Nearest emulated sample given floating point position and texel offset.
// Also zero's off screen.
float3 Fetch( float2 pos,float2 off){
  pos=floor(pos*_res+off)/_res;
  if(max(abs(pos.x-0.5),abs(pos.y-0.5))>0.5)return float3(0.0,0.0,0.0);
  return Test(ToLinear(tex2D(_MainTex,pos.xy).rgb));}

// Distance in emulated pixels to nearest texel.
float2 Dist(float2 pos){pos=pos*_res;return -((pos-floor(pos))-float2(0.5,0.5));}
    
// 1D Gaussian.
float Gaus(float pos,float scale){return exp2(scale*pos*pos);}

// 3-tap Gaussian filter along horz line.
float3 Horz3(float2 pos,float off){
  float3 b=Fetch(pos,float2(-1.0,off));
  float3 c=Fetch(pos,float2( 0.0,off));
  float3 d=Fetch(pos,float2( 1.0,off));
  float dst=Dist(pos).x;
  // Convert distance to weight.
  float scale=_hardPix;
  float wb=Gaus(dst-1.0,scale);
  float wc=Gaus(dst+0.0,scale);
  float wd=Gaus(dst+1.0,scale);
  // Return filtered sample.
  return (b*wb+c*wc+d*wd)/(wb+wc+wd);}

// 5-tap Gaussian filter along horz line.
float3 Horz5(float2 pos,float off){
  float3 a=Fetch(pos,float2(-2.0,off));
  float3 b=Fetch(pos,float2(-1.0,off));
  float3 c=Fetch(pos,float2( 0.0,off));
  float3 d=Fetch(pos,float2( 1.0,off));
  float3 e=Fetch(pos,float2( 2.0,off));
  float dst=Dist(pos).x;
  // Convert distance to weight.
  float scale=_hardPix;
  float wa=Gaus(dst-2.0,scale);
  float wb=Gaus(dst-1.0,scale);
  float wc=Gaus(dst+0.0,scale);
  float wd=Gaus(dst+1.0,scale);
  float we=Gaus(dst+2.0,scale);
  // Return filtered sample.
  return (a*wa+b*wb+c*wc+d*wd+e*we)/(wa+wb+wc+wd+we);}

// 7-tap Gaussian filter along horz line.
float3 Horz7(float2 pos,float off){
  float3 a=Fetch(pos,float2(-3.0,off));
  float3 b=Fetch(pos,float2(-2.0,off));
  float3 c=Fetch(pos,float2(-1.0,off));
  float3 d=Fetch(pos,float2( 0.0,off));
  float3 e=Fetch(pos,float2( 1.0,off));
  float3 f=Fetch(pos,float2( 2.0,off));
  float3 g=Fetch(pos,float2( 3.0,off));
  float dst=Dist(pos).x;
  // Convert distance to weight.
  float scale=_hardBloomPix;
  float wa=Gaus(dst-3.0,scale);
  float wb=Gaus(dst-2.0,scale);
  float wc=Gaus(dst-1.0,scale);
  float wd=Gaus(dst+0.0,scale);
  float we=Gaus(dst+1.0,scale);
  float wf=Gaus(dst+2.0,scale);
  float wg=Gaus(dst+3.0,scale);
  // Return filtered sample.
  return (a*wa+b*wb+c*wc+d*wd+e*we+f*wf+g*wg)/(wa+wb+wc+wd+we+wf+wg);}

// Return scanline weight.
float Scan(float2 pos,float off){
  float dst=Dist(pos).y;
  return Gaus(dst+off,_hardScan);}

// Return scanline weight for bloom.
float BloomScan(float2 pos,float off){
  float dst=Dist(pos).y;
  return Gaus(dst+off,_hardBloomScan);}

// Allow nearest three lines to effect pixel.
float3 Tri(float2 pos){
  float3 a=Horz3(pos,-1.0);
  float3 b=Horz5(pos, 0.0);
  float3 c=Horz3(pos, 1.0);
  float wa=Scan(pos,-1.0);
  float wb=Scan(pos, 0.0);
  float wc=Scan(pos, 1.0);
  return a*wa+b*wb+c*wc;}

// Small bloom.
float3 Bloom(float2 pos){
  float3 a=Horz5(pos,-2.0);
  float3 b=Horz7(pos,-1.0);
  float3 c=Horz7(pos, 0.0);
  float3 d=Horz7(pos, 1.0);
  float3 e=Horz5(pos, 2.0);
  float wa=BloomScan(pos,-2.0);
  float wb=BloomScan(pos,-1.0);
  float wc=BloomScan(pos, 0.0);
  float wd=BloomScan(pos, 1.0);
  float we=BloomScan(pos, 2.0);
  return a*wa+b*wb+c*wc+d*wd+e*we;}

// Distortion of scanlines, and end of screen alpha.
float2 Warp(float2 pos){
  
  pos=pos*2.0-1.0;    
  pos*=float2(1.0+(pos.y*pos.y)*_warp.x,1.0+(pos.x*pos.x)*_warp.y);
  return pos*0.5+0.5;}
   
#if defined(_MASKSTYLE_TVSTYLE)
    // Very compressed TV style shadow mask.
    float3 Mask(float2 pos){
      float lineee=_maskLight;
      float odd=0.0;
      if(fract(pos.x/6.0)<0.5)odd=1.0;
      if(fract((pos.y+odd)/2.0)<0.5) lineee=_maskDark;  
      pos.x=fract(pos.x/3.0);
      float3 mask=float3(_maskDark,_maskDark,_maskDark);
      if(pos.x<0.333)mask.r=_maskLight;
      else if(pos.x<0.666)mask.g=_maskLight;
      else mask.b=_maskLight;
      mask*=lineee;
      return mask;
  } 
#elif defined(_MASKSTYLE_APERTUREGRILLE)
    // Aperture-grille.
    float3 Mask(float2 pos){
    pos.x=fract(pos.x/3.0);
    float3 mask=float3(_maskDark,_maskDark,_maskDark);
    if(pos.x<0.333)mask.r=_maskLight;
    else if(pos.x<0.666)mask.g=_maskLight;
    else mask.b=_maskLight;
    return mask;}   
#elif defined(_MASKSTYLE_STRETCHEDVGA)
    // Stretched VGA style shadow mask (same as prior shaders).
    float3 Mask(float2 pos){
    pos.x+=pos.y*3.0;
    float3 mask=float3(_maskDark,_maskDark,_maskDark);
    pos.x=fract(pos.x/6.0);
    if(pos.x<0.333)mask.r=_maskLight;
    else if(pos.x<0.666)mask.g=_maskLight;
    else mask.b=_maskLight;
    return mask;}   
#elif defined(_MASKSTYLE_VGASTYLE)
// VGA style shadow mask.
float3 Mask(float2 pos){
  pos.xy=floor(pos.xy*float2(1.0,0.5));
  pos.x+=pos.y*3.0;
  float3 mask=float3(_maskDark,_maskDark,_maskDark);
  pos.x=fract(pos.x/6.0);
  if(pos.x<0.333)mask.r=_maskLight;
  else if(pos.x<0.666)mask.g=_maskLight;
  else mask.b=_maskLight;
  return mask;}   
#endif



// Draw dividing bars.
float Bar(float pos,float bar){pos-=bar;return pos*pos<4.0?0.0:1.0;}

// Entry.
float4 mainImage(float2 fragCoord){

float4 fragColor = float4(1,1,1,1);
float2 pos=Warp(fragCoord.xy/_iResolution.xy);

  fragColor.rgb=Tri(pos)*Mask(fragCoord.xy); 

  	fragColor.rgb+=Bloom(pos)*_bloomAmount;    
  
  fragColor.a=1.0;  
  fragColor.rgb=ToSrgb(fragColor.rgb);

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




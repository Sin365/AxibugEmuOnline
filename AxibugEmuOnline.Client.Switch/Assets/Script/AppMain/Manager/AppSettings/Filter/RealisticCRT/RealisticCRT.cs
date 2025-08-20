using Assets.Script.AppMain.Filter;
using UnityEngine;

namespace AxibugEmuOnline.Client.Filters
{
    public class RealisticCRT : FilterEffect
    {
        public override string Name => nameof(RealisticCRT);

        protected override string ShaderName => "Filter/RealisticCRT";

        [Range(0, 1)] public FloatParameter scan_line_amount = 1.0f;
        [Range(0, 5)] public FloatParameter warp_amount = 0.1f;
        [Range(0.0f, 0.3f)] public FloatParameter noise_amount = 0.03f;
        [Range(0.0f, 1.0f)] public FloatParameter interference_amount = 0.2f;
        [Range(0.0f, 1.0f)] public FloatParameter grille_amount = 0.1f;
        [Range(1.0f, 5.0f)] public FloatParameter grille_size = 1.0f;
        [Range(0.0f, 2.0f)] public FloatParameter vignette_amount = 0.6f;
        [Range(0.0f, 1.0f)] public FloatParameter vignette_intensity = 0.4f;
        [Range(0.0f, 1.0f)] public FloatParameter aberation_amount = 0.5f;
        [Range(0.0f, 1.0f)] public FloatParameter roll_line_amount = 0.3f;
        [Range(-8.0f, 8.0f)] public FloatParameter roll_speed = 1.0f;
        [Range(-12.0f, -1.0f)] public FloatParameter scan_line_strength = -8.0f;
        [Range(-4.0f, 0.0f)] public FloatParameter pixel_strength = 2.0f;

        int scan_line_amount_p = Shader.PropertyToID("scan_line_amount");
        int warp_amount_p = Shader.PropertyToID("warp_amount");
        int noise_amount_p = Shader.PropertyToID("noise_amount");
        int interference_amount_p = Shader.PropertyToID("interference_amount");
        int grille_amount_p = Shader.PropertyToID("grille_amount");
        int grille_size_p = Shader.PropertyToID("grille_size");
        int vignette_amount_p = Shader.PropertyToID("vignette_amount");
        int vignette_intensity_p = Shader.PropertyToID("vignette_intensity");
        int aberation_amount_p = Shader.PropertyToID("aberation_amount");
        int roll_line_amount_p = Shader.PropertyToID("roll_line_amount");
        int roll_speed_p = Shader.PropertyToID("roll_speed");
        int scan_line_strength_p = Shader.PropertyToID("scan_line_strength");
        int pixel_strength_p = Shader.PropertyToID("pixel_strength");

        protected override void OnRenderer(Material renderMat, Texture src, RenderTexture result)
        {
            renderMat.SetFloat(scan_line_amount_p, scan_line_amount);
            renderMat.SetFloat(warp_amount_p, warp_amount);
            renderMat.SetFloat(noise_amount_p, noise_amount);
            renderMat.SetFloat(interference_amount_p, interference_amount);
            renderMat.SetFloat(grille_amount_p, grille_amount);
            renderMat.SetFloat(grille_size_p, grille_size);
            renderMat.SetFloat(vignette_amount_p, vignette_amount);
            renderMat.SetFloat(vignette_intensity_p, vignette_intensity);
            renderMat.SetFloat(aberation_amount_p, aberation_amount);
            renderMat.SetFloat(roll_line_amount_p, roll_line_amount);
            renderMat.SetFloat(roll_speed_p, roll_speed);
            renderMat.SetFloat(scan_line_strength_p, scan_line_strength);
            renderMat.SetFloat(pixel_strength_p, pixel_strength);


            Graphics.Blit(src, result, renderMat);
        }
    }
}

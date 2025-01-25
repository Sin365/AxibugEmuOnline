using Assets.Script.AppMain.Filter;
using System.ComponentModel;
using UnityEngine;
using Random = UnityEngine.Random;

namespace AxibugEmuOnline.Client.Filters
{

    public class SimpleCRT : FilterEffect
    {
        public override string Name => nameof(SimpleCRT);

        protected override string ShaderName => "Filter/yunoda-3DCG/SimpleCRT";

        [Range(0, 1000)][Description("White Noise Freq")] public IntParameter whiteNoiseFrequency = new IntParameter(1);
        [Range(0, 1)][Description("White Noise Time Left (sec)")] public FloatParameter whiteNoiseLength = new FloatParameter(0.1f);
        private float whiteNoiseTimeLeft;

        [Range(0, 1000)][Description("Screen Jump Freq")] public IntParameter screenJumpFrequency = 1;
        [Range(0, 1f)][Description("Screen Jump Length")] public FloatParameter screenJumpLength = 0.2f;
        [Range(0, 1f)][Description("Jump Min")] public FloatParameter screenJumpMinLevel = 0.1f;
        [Range(0, 1f)][Description("Jump Max")] public FloatParameter screenJumpMaxLevel = 0.9f;
        private float screenJumpTimeLeft;

        [Range(0, 1f)][Description("Flickering Strength")] public FloatParameter flickeringStrength = 0.002f;
        [Range(0, 333f)][Description("Flickering Cycle")] public FloatParameter flickeringCycle = 111f;

        [Description("Slip Page")] public BoolParameter isSlippage = true;
        [Description("Slip Noise")] public BoolParameter isSlippageNoise = true;
        [Range(0, 1)][Description("Slip Strength")] public FloatParameter slippageStrength = 0.005f;
        [Range(0, 100)][Description("Slip Intervalw")] public float slippageInterval = 1f;
        [Range(0, 330)][Description("Slip Scroll Speed")] public float slippageScrollSpeed = 33f;
        [Range(0, 100f)][Description("Slip Size")] public FloatParameter slippageSize = 11f;

        [Range(0, 1f)][Description("Chromatic Aberration Strength")] public FloatParameter chromaticAberrationStrength = 0.005f;
        [Description("Chromatic Aberration")] public bool isChromaticAberration = true;

        [Description("Multiple Ghost")] public BoolParameter isMultipleGhost = true;
        [Range(0, 1f)][Description("Multiple Ghost Strength")] public FloatParameter multipleGhostStrength = 0.01f;

        [Description("Scanline")] public BoolParameter isScanline = true;
        [Description("Monochrome")] public BoolParameter isMonochrome = false;

        [Description("Letter Box")] public bool isLetterBox = false;
        public bool isLetterBoxEdgeBlur = false;
        [Description("Letter Box Type")] public FilterParameter<LeterBoxType> letterBoxType = default(LeterBoxType);
        public enum LeterBoxType
        {
            Black,
            Blur
        }

        #region Properties in shader
        private int _WhiteNoiseOnOff;
        private int _ScanlineOnOff;
        private int _MonochormeOnOff;
        private int _ScreenJumpLevel;
        private int _FlickeringStrength;
        private int _FlickeringCycle;
        private int _SlippageStrength;
        private int _SlippageSize;
        private int _SlippageInterval;
        private int _SlippageScrollSpeed;
        private int _SlippageNoiseOnOff;
        private int _SlippageOnOff;
        private int _ChromaticAberrationStrength;
        private int _ChromaticAberrationOnOff;
        private int _MultipleGhostOnOff;
        private int _MultipleGhostStrength;
        private int _LetterBoxOnOff;
        private int _LetterBoxType;
        private int _LetterBoxEdgeBlurOnOff;
        private int _DecalTex;
        private int _DecalTexOnOff;
        private int _DecalTexPos;
        private int _DecalTexScale;
        private int _FilmDirtOnOff;
        private int _FilmDirtTex;
        #endregion

        protected override void OnInit(Material renderMat)
        {
            base.OnInit(renderMat);

            _WhiteNoiseOnOff = Shader.PropertyToID("_WhiteNoiseOnOff");
            _ScanlineOnOff = Shader.PropertyToID("_ScanlineOnOff");
            _MonochormeOnOff = Shader.PropertyToID("_MonochormeOnOff");
            _ScreenJumpLevel = Shader.PropertyToID("_ScreenJumpLevel");
            _FlickeringStrength = Shader.PropertyToID("_FlickeringStrength");
            _FlickeringCycle = Shader.PropertyToID("_FlickeringCycle");
            _SlippageStrength = Shader.PropertyToID("_SlippageStrength");
            _SlippageSize = Shader.PropertyToID("_SlippageSize");
            _SlippageInterval = Shader.PropertyToID("_SlippageInterval");
            _SlippageScrollSpeed = Shader.PropertyToID("_SlippageScrollSpeed");
            _SlippageNoiseOnOff = Shader.PropertyToID("_SlippageNoiseOnOff");
            _SlippageOnOff = Shader.PropertyToID("_SlippageOnOff");
            _ChromaticAberrationStrength = Shader.PropertyToID("_ChromaticAberrationStrength");
            _ChromaticAberrationOnOff = Shader.PropertyToID("_ChromaticAberrationOnOff");
            _MultipleGhostOnOff = Shader.PropertyToID("_MultipleGhostOnOff");
            _MultipleGhostStrength = Shader.PropertyToID("_MultipleGhostStrength");
            _LetterBoxOnOff = Shader.PropertyToID("_LetterBoxOnOff");
            _LetterBoxType = Shader.PropertyToID("_LetterBoxType");
            _DecalTex = Shader.PropertyToID("_DecalTex");
            _DecalTexOnOff = Shader.PropertyToID("_DecalTexOnOff");
            _DecalTexPos = Shader.PropertyToID("_DecalTexPos");
            _DecalTexScale = Shader.PropertyToID("_DecalTexScale");
            _FilmDirtOnOff = Shader.PropertyToID("_FilmDirtOnOff");
            _FilmDirtTex = Shader.PropertyToID("_FilmDirtTex");
        }

        protected override void OnRenderer(Material renderMat, Texture src, RenderTexture result)
        {
            SetShaderParameter(renderMat);

            Graphics.Blit(src, result, renderMat);
        }

        private void SetShaderParameter(Material material)
        {
            ///////White noise
            whiteNoiseTimeLeft -= 0.01f;
            if (whiteNoiseTimeLeft <= 0)
            {
                if (Random.Range(0, 1000) < whiteNoiseFrequency)
                {
                    material.SetInteger(_WhiteNoiseOnOff, 1);
                    whiteNoiseTimeLeft = whiteNoiseLength;
                }
                else
                {
                    material.SetInteger(_WhiteNoiseOnOff, 0);
                }
            }
            //////

            material.SetInteger(_LetterBoxOnOff, isLetterBox ? 0 : 1);
            //material.SetInteger(_LetterBoxEdgeBlurOnOff, isLetterBoxEdgeBlur ? 0 : 1); 
            material.SetInteger(_LetterBoxType, (int)letterBoxType.GetValue());

            material.SetInteger(_ScanlineOnOff, isScanline ? 1 : 0);
            material.SetInteger(_MonochormeOnOff, isMonochrome ? 1 : 0);
            material.SetFloat(_FlickeringStrength, flickeringStrength);
            material.SetFloat(_FlickeringCycle, flickeringCycle);
            material.SetFloat(_ChromaticAberrationStrength, chromaticAberrationStrength);
            material.SetInteger(_ChromaticAberrationOnOff, isChromaticAberration ? 1 : 0);
            material.SetInteger(_MultipleGhostOnOff, isMultipleGhost ? 1 : 0);
            material.SetFloat(_MultipleGhostStrength, multipleGhostStrength);

            //////Slippage
            material.SetInteger(_SlippageOnOff, isSlippage ? 1 : 0);
            material.SetFloat(_SlippageInterval, slippageInterval);
            material.SetFloat(_SlippageNoiseOnOff, isSlippageNoise ? Random.Range(0, 1f) : 1);
            material.SetFloat(_SlippageScrollSpeed, slippageScrollSpeed);
            material.SetFloat(_SlippageStrength, slippageStrength);
            material.SetFloat(_SlippageSize, slippageSize);
            //////

            //////Screen Jump Noise
            screenJumpTimeLeft -= 0.01f;
            if (screenJumpTimeLeft <= 0)
            {
                if (Random.Range(0, 1000) < screenJumpFrequency)
                {
                    var level = Random.Range(screenJumpMinLevel, screenJumpMaxLevel);
                    material.SetFloat(_ScreenJumpLevel, level);
                    screenJumpTimeLeft = screenJumpLength;
                }
                else
                {
                    material.SetFloat(_ScreenJumpLevel, 0);
                }
            }
        }
    }
}

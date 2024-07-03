using System;

namespace MyNes.Core
{
    public class NTSCPaletteGenerator
    {
    	public const float default_saturation = 1.496f;

    	public const float default_hue_tweak = 0f;

    	public const float default_contrast = 1.016f;

    	public const float default_brightness = 1.075f;

    	public const float default_gamma = 1.975f;

    	private const float black = 0.518f;

    	private const float white = 1.962f;

    	private const float attenuation = 0.746f;

    	public static float saturation = 2f;

    	public static float hue_tweak = 0f;

    	public static float contrast = 1.4f;

    	public static float brightness = 1.07f;

    	public static float gamma = 2f;

    	private static float[] levels = new float[8] { 0.35f, 0.518f, 0.962f, 1.55f, 1.094f, 1.506f, 1.962f, 1.962f };

    	private static int wave(int p, int color)
    	{
    		if ((color + p + 8) % 12 >= 6)
    		{
    			return 0;
    		}
    		return 1;
    	}

    	private static float gammafix(float f, float gamma)
    	{
    		return (float)((f < 0f) ? 0.0 : Math.Pow(f, 2.2f / gamma));
    	}

    	private static int clamp(float v)
    	{
    		return (int)((v < 0f) ? 0f : ((v > 255f) ? 255f : v));
    	}

    	public static int MakeRGBcolor(int pixel)
    	{
    		int num = pixel & 0xF;
    		int num2 = ((num >= 14) ? 1 : ((pixel >> 4) & 3));
    		float[] array = new float[2]
    		{
    			levels[num2 + ((num == 0) ? 4 : 0)],
    			levels[num2 + ((num <= 12) ? 4 : 0)]
    		};
    		float num3 = 0f;
    		float num4 = 0f;
    		float num5 = 0f;
    		for (int i = 0; i < 12; i++)
    		{
    			float num6 = array[wave(i, num)];
    			if ((((uint)pixel & 0x40u) != 0 && wave(i, 12) == 1) || (((uint)pixel & 0x80u) != 0 && wave(i, 4) == 1) || (((uint)pixel & 0x100u) != 0 && wave(i, 8) == 1))
    			{
    				num6 *= 0.746f;
    			}
    			float num7 = (num6 - 0.518f) / 1.444f;
    			num7 = (num7 - 0.5f) * contrast + 0.5f;
    			num7 *= brightness / 12f;
    			num3 += num7;
    			num4 += (float)((double)num7 * Math.Cos(Math.PI / 6.0 * (double)((float)i + hue_tweak)));
    			num5 += (float)((double)num7 * Math.Sin(Math.PI / 6.0 * (double)((float)i + hue_tweak)));
    		}
    		num4 *= saturation;
    		num5 *= saturation;
    		return 65536 * clamp(255f * gammafix(num3 + 0.946882f * num4 + 0.623557f * num5, gamma)) + 256 * clamp(255f * gammafix(num3 - 245f / (328f * (float)Math.E) * num4 - 0.635691f * num5, gamma)) + clamp(255f * gammafix(num3 - 1.108545f * num4 + 1.709007f * num5, gamma));
    	}

    	public static int[] GeneratePalette()
    	{
    		int[] array = new int[512];
    		for (int i = 0; i < 512; i++)
    		{
    			array[i] = MakeRGBcolor(i) | -16777216;
    		}
    		return array;
    	}

    	public static int[] GeneratePaletteGBR()
    	{
    		int[] array = new int[512];
    		for (int i = 0; i < 512; i++)
    		{
    			int num = MakeRGBcolor(i);
    			byte b = (byte)((num & 0xFF0000) >> 16);
    			byte b2 = (byte)((num & 0xFF00) >> 8);
    			byte b3 = (byte)((uint)num & 0xFFu);
    			array[i] = -16777216 | (b3 << 16) | (b2 << 8) | b;
    		}
    		return array;
    	}
    }
}

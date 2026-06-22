using MAME.Core;
using System.Collections.Generic;
using UnityEngine;

public class UniResources : IResources
{
    public const string ResourceRoot = "MAME/emu/";

    public byte[] mcu
    {
        get
        {
            MameMainMotion.CheckCanStep(-311, System.Reflection.MethodBase.GetCurrentMethod().Name);
            return Resources.Load<TextAsset>(ResourceRoot + "cus64-64a1.mcu").bytes;
        }
    }

    public byte[] sfix
    {
        get
        {
            MameMainMotion.CheckCanStep(-310, System.Reflection.MethodBase.GetCurrentMethod().Name);
            return Resources.Load<TextAsset>(ResourceRoot + "sfix.sfix").bytes;
        }
    }

    public byte[] _000_lo
    {
        get
        {
            MameMainMotion.CheckCanStep(-309, System.Reflection.MethodBase.GetCurrentMethod().Name);
            return Resources.Load<TextAsset>(ResourceRoot + "000-lo.lo").bytes;
        }
    }

    public byte[] sm1
    {
        get
        {
            MameMainMotion.CheckCanStep(-308, System.Reflection.MethodBase.GetCurrentMethod().Name);
            return Resources.Load<TextAsset>(ResourceRoot + "sm1.sm1").bytes;
        }
    }

    public byte[] mainbios
    {
        get
        {
            MameMainMotion.CheckCanStep(-307, System.Reflection.MethodBase.GetCurrentMethod().Name);
            return Resources.Load<TextAsset>(ResourceRoot + "neogeo_mainbios.rom").bytes;
        }
    }

    public byte[] pgmmainbios
    {
        get
        {
            MameMainMotion.CheckCanStep(-306, System.Reflection.MethodBase.GetCurrentMethod().Name);
            return Resources.Load<TextAsset>(ResourceRoot + "pgm_mainbios.rom").bytes;
        }
    }

    public byte[] pgmvideobios
    {
        get
        {
            MameMainMotion.CheckCanStep(-305, System.Reflection.MethodBase.GetCurrentMethod().Name);
            return Resources.Load<TextAsset>(ResourceRoot + "pgm_t01s.rom").bytes;
        }
    }

    public byte[] pgmaudiobios
    {
        get
        {
            MameMainMotion.CheckCanStep(-304, System.Reflection.MethodBase.GetCurrentMethod().Name);
            return Resources.Load<TextAsset>(ResourceRoot + "pgm_m01s.rom").bytes;
        }
    }

    public byte[] _1
    {
        get
        {
            MameMainMotion.CheckCanStep(-303, System.Reflection.MethodBase.GetCurrentMethod().Name);
            return Resources.Load<TextAsset>(ResourceRoot + "1.png").bytes;
        }
    }

    public byte[] readme
    {
        get
        {
            MameMainMotion.CheckCanStep(-302, System.Reflection.MethodBase.GetCurrentMethod().Name);
            return Resources.Load<TextAsset>(ResourceRoot + "readme.txt").bytes;
        }
    }

    public string mame
    {
        get
        {
            MameMainMotion.CheckCanStep(-719, System.Reflection.MethodBase.GetCurrentMethod().Name);
            //return Resources.Load<TextAsset>(ResourceRoot + "mame.xml").text;//ok
            return UMAME.instance.MAME_XML.text;
        }
    }


    public bool getnvram(string sName, out byte[] data)
    {
        MameMainMotion.CheckCanStep(-300, System.Reflection.MethodBase.GetCurrentMethod().Name);
        TextAsset asset = Resources.Load<TextAsset>(ResourceRoot + "nvram/" + sName + ".nv");
        if (asset == null)
        {
            data = null;
            return false;
        }
        data = asset.bytes;
        return true;
    }
    public List<RomInfo> GetGameDB()
    {
        MameMainMotion.CheckCanStep(-719, System.Reflection.MethodBase.GetCurrentMethod().Name);
        return MAMEScriptable.GetLoadToMAMECore();
    }
}
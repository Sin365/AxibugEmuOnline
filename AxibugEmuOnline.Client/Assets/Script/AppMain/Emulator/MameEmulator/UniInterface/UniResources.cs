using MAME.Core;
using UnityEngine;

public class UniResources : IResources
{
    const string ResourceRoot = "MAME/emu/";

    public byte[] mcu => Resources.Load<TextAsset>(ResourceRoot + "cus64-64a1.mcu").bytes;

    public byte[] sfix => Resources.Load<TextAsset>(ResourceRoot + "sfix.sfix").bytes;

    public byte[] _000_lo => Resources.Load<TextAsset>(ResourceRoot + "000-lo.lo").bytes;

    public byte[] sm1 => Resources.Load<TextAsset>(ResourceRoot + "sm1.sm1").bytes;

    public byte[] mainbios => Resources.Load<TextAsset>(ResourceRoot + "neogeo_mainbios.rom").bytes;

    public byte[] pgmmainbios => Resources.Load<TextAsset>(ResourceRoot + "pgm_mainbios.rom").bytes;

    public byte[] pgmvideobios => Resources.Load<TextAsset>(ResourceRoot + "pgm_t01s.rom").bytes;

    public byte[] pgmaudiobios => Resources.Load<TextAsset>(ResourceRoot + "pgm_m01s.rom").bytes;

    public byte[] _1 =>  Resources.Load<TextAsset>(ResourceRoot + "1.png").bytes;

    public byte[] readme => Resources.Load<TextAsset>(ResourceRoot + "readme.txt").bytes;

    public string mame => Resources.Load<TextAsset>(ResourceRoot + "mame.xml").text;//ok
}
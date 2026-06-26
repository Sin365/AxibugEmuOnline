using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using UnityEditor;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

[CreateAssetMenu(fileName = "MameDB", menuName = "Scriptable Objects/MAMEScriptable")]
public class MAMEScriptable : ScriptableObject
{
    [SerializeField]
    public List<AxiMAMEInfoSingle> GameList = new List<AxiMAMEInfoSingle>();
    [Serializable]
    public class AxiMAMEInfoSingle
    {
        public string Name, Board;
        public string Parent;
        public string Direction;
        public string Description;
        public string Year;
        public string Manufacturer;
        public string M1Default, M1Stop, M1Min, M1Max, M1Subtype;
    }

    public static MAMEScriptable data;
    static string SavePath => UniResources.ResourceRoot + "MameDB";
    static MAMEScriptable GetScriptableObjInst()
    {
        if (data == null)
            data = UnityEngine.Resources.Load<MAMEScriptable>(SavePath);
        return data;
    }

    public static List<MAME.Core.RomInfo> GetLoadToMAMECore()
    {
        var mamedata = GetScriptableObjInst();
        List<MAME.Core.RomInfo> romlist = new List<MAME.Core.RomInfo>();
        foreach (var single in mamedata.GameList)
        {
            MAME.Core.RomInfo rom = new MAME.Core.RomInfo();
            rom.Name = single.Name;
            rom.Board = single.Board;
            rom.Parent = single.Parent;
            rom.Direction = single.Direction;
            rom.Description = single.Description;
            rom.Year = single.Year;
            rom.Manufacturer = single.Manufacturer;
            romlist.Add(rom);
        }
        return romlist;
    }

#if UNITY_EDITOR

    [UnityEditor.MenuItem("模拟器Tools/写入mame.xml到Scriptable")]
    public static void LoadROMXMLToScriptable()//这些xml读取规则和字段赋值是从mame.core核心搬出来的 保持一致
    {
        string tmp = UnityEngine.Resources.Load<UnityEngine.TextAsset>(UniResources.ResourceRoot + "mame.xml").text;
        XElement xe = XElement.Parse(tmp);
        IEnumerable<XElement> elements = from ele in xe.Elements("game") select ele;
        CotrGameListByElements(elements);

    }

    static void CotrGameListByElements(IEnumerable<XElement> elements)
    {
        var assetsObj = UnityEngine.Resources.Load<MAMEScriptable>(SavePath);
        assetsObj.GameList = new List<AxiMAMEInfoSingle>();
        foreach (var ele in elements)
        {
            AxiMAMEInfoSingle rom = new AxiMAMEInfoSingle();
            rom.Name = ele.Attribute("name").Value;
            rom.Board = ele.Attribute("board").Value;
            rom.Parent = ele.Element("parent").Value;
            rom.Direction = ele.Element("direction").Value;
            rom.Description = ele.Element("description").Value;
            rom.Year = ele.Element("year").Value;
            rom.Manufacturer = ele.Element("manufacturer").Value;
            assetsObj.GameList.Add(rom);
        }
        // 标记为脏（告诉Unity数据变了，需要序列化）
        EditorUtility.SetDirty(assetsObj);

        // 立即保存单个资产到磁盘（Unity 2020+ 推荐，轻量）
        // 如果不调这个，通常只在退出Unity或手动Ctrl+S时才会真正写盘
        AssetDatabase.SaveAssetIfDirty(assetsObj);
    }
#endif
}
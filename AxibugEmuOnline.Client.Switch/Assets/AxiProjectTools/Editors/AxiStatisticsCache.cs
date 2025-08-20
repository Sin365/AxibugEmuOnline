#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


public class AxiStatisticsCache : ScriptableObject
{
	public List<AxiStatisticsDatas> caches = new List<AxiStatisticsDatas>();
}

[Serializable]
public class AxiStatisticsDatas
{
	/// <summary>
	/// [0]Sence [1]Prefab
	/// </summary>
	public int type;
	public string FullPath;
    public List<AxiStatistics_Node> nodes = new List<AxiStatistics_Node>();
}

[Serializable]
public class AxiStatistics_Node
{
	public string Name;
    public List<AxiStatistics_Node_Link> link = new List<AxiStatistics_Node_Link>();
    public int LinkHash;
    public string LinkFullStr;
    //public string NodeFullPath;
    //   /// <summary>
    //   /// 表示相同路径只有一个
    //   /// </summary>
    //   public bool NodeIdxOnlyOne;
    /// <summary>
    /// 表示相同路径是第几个下标
    /// </summary>
    //public int NodeIdx;
    public List<AxiStatistics_Node_Component> components = new List<AxiStatistics_Node_Component>();
}

[Serializable]
public class AxiStatistics_Node_Link
{
    public string Name;
    public bool OnlyOne;
    public int Idx;
    public int NodeHash;
}

[Serializable]
public class AxiStatistics_Node_Component
{
    public string type;
    /// <summary>
    /// 表示相同组件只有一个
    /// </summary>
    public bool ComTypeOnlyOne;
    /// <summary>
    /// 表示相同组件是第几个下标
    /// </summary>
    public int ComIdxNum;
    //Rigboody
    public bool simulated;
    public float gravityScale;
    public bool isKinematic;
	//BoxCollider2D
	public Vector2 center;
	public Vector2 size;
}

#endif
﻿#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


public class AxiPrefabCache : ScriptableObject
{
    public List<AxiPrefabCache_Com2GUID> caches = new List<AxiPrefabCache_Com2GUID>();
}

[Serializable]
public class AxiPrefabCache_Com2GUID
{
    public string SrcFullName;
    public string SrcName;
	public string GUID;
	public string ToName;
	public string ToPATH;
	public string ToGUID;
	public MonoScript monoScript;
}
#endif
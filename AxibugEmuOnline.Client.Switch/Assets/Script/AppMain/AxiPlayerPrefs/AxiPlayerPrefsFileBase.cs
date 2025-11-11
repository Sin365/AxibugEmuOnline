using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class AxiPlayerPrefsFileBase : IAxiPlayerPrefs
{
    protected static string AxiPlayerPrefsFilePath => AxiPlayerPrefs.SaveDataRootDirPath + "/AxiPlayerPrefs.dat";

    Dictionary<string, AxiPlayerPrefsKeyValye> m_keyval = new Dictionary<string, AxiPlayerPrefsKeyValye>();
	Func<Dictionary<string, AxiPlayerPrefsKeyValye>> m_LoadFunc;
	Action<Dictionary<string, AxiPlayerPrefsKeyValye>> m_SaveFunc;
	bool bDirty = false;

	[Serializable]
	public class AxiPlayerPrefsAllData
	{
		public int version;
		public List<AxiPlayerPrefsKeyValye> datalist;
	}

	[Serializable]
	public class AxiPlayerPrefsKeyValye
	{
		public string key;
		public int intval;
		public string strval;
		public float floatval;
	}

	public AxiPlayerPrefsFileBase(Func<Dictionary<string, AxiPlayerPrefsKeyValye>> load, Action<Dictionary<string, AxiPlayerPrefsKeyValye>> save)
	{
		m_LoadFunc = load;
		m_SaveFunc = save;
		Load();
		AxiPlayerPrefsMono.SetInvoke(Save, 15);
	}
	public static Dictionary<string, AxiPlayerPrefsKeyValye> JsonStrToData(string dataStr)
	{
		AxiPlayerPrefsAllData alldata = UnityEngine.JsonUtility.FromJson<AxiPlayerPrefsAllData>(dataStr);
		Dictionary<string, AxiPlayerPrefsKeyValye> data = new Dictionary<string, AxiPlayerPrefsKeyValye>();
		foreach (var item in alldata.datalist)
		{
			data.Add(item.key, item);
		}
		return data;
	}

	public static string DataToJsonStr(Dictionary<string, AxiPlayerPrefsKeyValye> data)
	{
		return UnityEngine.JsonUtility.ToJson(new AxiPlayerPrefsAllData() { version = 1, datalist = data.Values.ToList() });
	}

	AxiPlayerPrefsKeyValye GetByKey(string key, bool NonAutoCreate, out bool IsNew)
	{
		//Debug.Log($"GetByKey=>{key}");
		if (!m_keyval.ContainsKey(key))
		{
			IsNew = true;
			if (!NonAutoCreate)
				return null;
			m_keyval.Add(key, new AxiPlayerPrefsKeyValye() { key = key });
		}
		else
			IsNew = false;
		return m_keyval[key];
	}

	public void Load()
	{
		m_keyval = m_LoadFunc.Invoke();
	}

	public void Save()
	{
		if (bDirty)
		{
			Debug.Log("Auto AxiPlayerPrefs.");
			bDirty = false;
			m_SaveFunc.Invoke(m_keyval);
		}
	}

	public float GetFloat(string key, float defaultValue)
	{
		bool IsNew;
        AxiPlayerPrefsKeyValye kv = GetByKey(key, true, out IsNew);
		if (IsNew)
			kv.floatval = defaultValue;
		return kv.floatval;
	}

	public int GetInt(string key, int defaultValue)
    {
        bool IsNew;
        AxiPlayerPrefsKeyValye kv = GetByKey(key, true, out IsNew);
		if (IsNew)
			kv.intval = defaultValue;
		return kv.intval;
	}

	public string GetString(string key, string defaultValue)
    {
        bool IsNew;
        AxiPlayerPrefsKeyValye kv = GetByKey(key, true, out IsNew);
		if (IsNew)
			kv.strval = defaultValue;
		return kv.strval;
	}

	public float GetFloat(string key)
	{
		bool val;
        AxiPlayerPrefsKeyValye kv = GetByKey(key, false, out val);
		if (kv != null) return kv.floatval;
		return default(float);
	}

	public int GetInt(string key)
    {
        bool val;
        AxiPlayerPrefsKeyValye kv = GetByKey(key, false, out val);
		if (kv != null) return kv.intval;
		return default(int);
	}

	public string GetString(string key)
    {
        bool val;
        AxiPlayerPrefsKeyValye kv = GetByKey(key, false, out val);
		if (kv != null) return kv.strval;
		return string.Empty;
	}


	public void SetInt(string key, int value)
    {
        bool val;
        AxiPlayerPrefsKeyValye kv = GetByKey(key, true, out val);
		if (kv.intval == value)
			return;
		kv.intval = value;
		bDirty = true;
	}

	public void SetString(string key, string value)
    {
        bool val;
        AxiPlayerPrefsKeyValye kv = GetByKey(key, true, out val);
		if (string.Equals(kv.strval, value))
			return;
		kv.strval = value;
		bDirty = true;
	}

	public void SetFloat(string key, float value)
    {
        bool val;
        AxiPlayerPrefsKeyValye kv = GetByKey(key, true, out val);
		if (kv.floatval == value)
			return;
		kv.floatval = value;
		bDirty = true;
	}
	public void DeleteAll()
	{
		m_keyval.Clear();
		bDirty = true;
	}

}
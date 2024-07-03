using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace MyNes.Core
{
    public abstract class ISettings
    {
    	protected string filePath;

    	protected FieldInfo[] Fields;

    	public ISettings(string filePath)
    	{
    		this.filePath = filePath;
    	}

    	public virtual void LoadSettings()
    	{
    		Fields = GetType().GetFields();
    		if (!File.Exists(filePath))
    		{
    			return;
    		}
    		string[] array = File.ReadAllLines(filePath);
    		for (int i = 0; i < array.Length; i++)
    		{
    			string[] array2 = array[i].Split('=');
    			if (array2 != null && array2.Length == 2)
    			{
    				SetField(array2[0], array2[1]);
    			}
    		}
    	}

    	public virtual void SaveSettings()
    	{
    		Fields = GetType().GetFields();
    		List<string> list = new List<string>();
    		FieldInfo[] fields = Fields;
    		foreach (FieldInfo fieldInfo in fields)
    		{
    			if (fieldInfo.IsPublic)
    			{
    				list.Add(fieldInfo.Name + "=" + GetFieldValue(fieldInfo));
    			}
    		}
    		File.WriteAllLines(filePath, list.ToArray());
    	}

    	protected virtual void SetField(string fieldName, string val)
    	{
    		for (int i = 0; i < Fields.Length; i++)
    		{
    			if (!(Fields[i].Name == fieldName))
    			{
    				continue;
    			}
    			if (Fields[i].FieldType == typeof(string))
    			{
    				Fields[i].SetValue(this, val);
    			}
    			else if (Fields[i].FieldType == typeof(bool))
    			{
    				Fields[i].SetValue(this, val == "1");
    			}
    			else if (Fields[i].FieldType == typeof(int))
    			{
    				int result = 0;
    				if (int.TryParse(val, out result))
    				{
    					Fields[i].SetValue(this, result);
    				}
    			}
    			else if (Fields[i].FieldType == typeof(float))
    			{
    				float result2 = 0f;
    				if (float.TryParse(val, out result2))
    				{
    					Fields[i].SetValue(this, result2);
    				}
    			}
    			else if (Fields[i].FieldType == typeof(string[]))
    			{
    				string[] value = val.Split(new string[1] { "*" }, StringSplitOptions.RemoveEmptyEntries);
    				Fields[i].SetValue(this, value);
    			}
    			else
    			{
    				Tracer.WriteLine("Unknown setting type = " + Fields[i].FieldType);
    			}
    			break;
    		}
    	}

    	protected virtual string GetFieldValue(string fieldName)
    	{
    		for (int i = 0; i < Fields.Length; i++)
    		{
    			if (Fields[i].Name == fieldName)
    			{
    				return GetFieldValue(Fields[i]);
    			}
    		}
    		return "";
    	}

    	protected virtual string GetFieldValue(FieldInfo field)
    	{
    		object value = field.GetValue(this);
    		if (field.FieldType == typeof(string))
    		{
    			return value.ToString();
    		}
    		if (field.FieldType == typeof(bool))
    		{
    			if (!(bool)value)
    			{
    				return "0";
    			}
    			return "1";
    		}
    		if (field.FieldType == typeof(int))
    		{
    			return value.ToString();
    		}
    		if (field.FieldType == typeof(float))
    		{
    			return value.ToString();
    		}
    		if (field.FieldType == typeof(string[]))
    		{
    			string text = "";
    			string[] array = (string[])value;
    			if (array != null)
    			{
    				string[] array2 = array;
    				foreach (string text2 in array2)
    				{
    					text = text + text2 + "*";
    				}
    			}
    			if (text.Length > 0)
    			{
    				return text.Substring(0, text.Length - 1);
    			}
    			return "";
    		}
    		return "";
    	}
    }
}

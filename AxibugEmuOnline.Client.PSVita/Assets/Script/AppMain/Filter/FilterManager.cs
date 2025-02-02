﻿using AxibugEmuOnline.Client.ClientCore;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static AxibugEmuOnline.Client.FilterEffect;

namespace AxibugEmuOnline.Client
{
	public class FilterManager
	{
		private List<Filter> m_filters;
		private Dictionary<EnumPlatform, Filter> m_filterPlatforms = new Dictionary<EnumPlatform, Filter>();
		private AlphaWraper m_previewFilterWraper;
		FilterRomSetting m_filterRomSetting;
		/// <summary>
		/// 滤镜列表
		/// </summary>
		public IReadOnlyList<Filter> Filters => m_filters;

		public FilterManager(CanvasGroup filterPreview, CanvasGroup mainBg)
		{
//#if UNITY_PSP2
//			m_filters = new List<Filter>();
//			m_filterRomSetting = new FilterRomSetting();
//			m_previewFilterWraper = new AlphaWraper(mainBg, filterPreview, false);
//			return;
//#endif

			m_filters = new List<Filter>
			{
				//new Filter(new FixingPixelArtGrille()),
				new Filter(new LCDPostEffect()),
				new Filter(new MattiasCRT()),
			};
			var json = PlayerPrefs.GetString(nameof(FilterRomSetting));
			m_filterRomSetting = JsonUtility.FromJson<FilterRomSetting>(json) ?? new FilterRomSetting();

			m_previewFilterWraper = new AlphaWraper(mainBg, filterPreview, false);
			ShutDownFilterPreview();
			ShutDownFilter();
		}

		private RenderTexture result = null;
		public Texture ExecuteFilterRender(Texture src)
		{
			if (result == null)
			{
				result = RenderTexture.GetTemporary(Screen.width, Screen.height);
			}
			else if (result.width != Screen.width || result.height != Screen.height)
			{
				RenderTexture.ReleaseTemporary(result);
				result = RenderTexture.GetTemporary(Screen.width, Screen.height);
			}



			bool anyFilterEnable = false;
			foreach (var filter in Filters)
			{
				if (!filter.m_setting.Enable.GetValue()) continue;
				filter.m_setting.Render(src, result);
				anyFilterEnable = true;
			}

			if (anyFilterEnable)
				return result;
			else
				return src;
		}

		/// <summary> 关闭滤镜预览 </summary>
		public void ShutDownFilterPreview()
		{
			m_previewFilterWraper.On = false;
		}

		/// <summary> 开启滤镜预览 </summary>
		public void EnableFilterPreview()
		{
			m_previewFilterWraper.On = true;
		}

		/// <summary>
		/// 打开滤镜
		/// </summary>
		/// <param name="filter"></param>
		public void EnableFilter(Filter filter)
		{
			foreach (var selfFiler in Filters)
			{
				if (selfFiler != filter) selfFiler.m_setting.Enable.Override(false);
				else selfFiler.m_setting.Enable.Override(true);
			}
		}

		/// <summary>
		/// 关闭滤镜效果
		/// </summary>
		public void ShutDownFilter()
		{
			//关闭所有后处理效果
			foreach (var filter in Filters)
				filter.m_setting.Enable.Override(false);
		}

		/// <summary>
		/// 为指定rom设置滤镜以及滤镜的预设
		/// </summary>
		/// <param name="rom">rom对象</param>
		/// <param name="filter">滤镜</param>
		/// <param name="preset">滤镜预设</param>
		public void SetupFilter(RomFile rom, Filter filter, FilterPreset preset)
		{
			m_filterRomSetting.Setup(rom, filter, preset);

			string json = m_filterRomSetting.ToJson();
			PlayerPrefs.SetString(nameof(FilterRomSetting), json);
		}

		/// <summary>
		/// 获得指定rom配置的滤镜设置
		/// </summary>
		/// <param name="rom">rom对象</param>
		/// <returns>此元组任意内任意成员都有可能为空</returns>
		public GetFilterSetting_result GetFilterSetting(RomFile rom)
		{
			var value = m_filterRomSetting.Get(rom);
			Filter filter = null;
			FilterPreset preset = null;

			//filter = Filters.FirstOrDefault(f => f.Name == value.filterName);
			//if (filter != null)
			//{
			//    string presetName = value.presetName;
			//    preset = filter.Presets.FirstOrDefault(p => p.Name == presetName);
			//}

			filter = Filters.FirstOrDefault(f => f.Name == value.Item1);
			if (filter != null)
			{
				string presetName = value.Item2;
				preset = filter.Presets.FirstOrDefault(p => p.Name == presetName);
			}

			return new GetFilterSetting_result()
			{
				filter = filter,
				preset = preset
			};
		}

		public struct GetFilterSetting_result
		{
			public Filter filter;
			public FilterPreset preset;
		}

		public class Filter
		{
			public string Name => m_setting.Name;
			public IReadOnlyCollection<EditableParamerter> Paramerters => m_setting.EditableParam;
			/// <summary> 滤镜预设 </summary>
			public List<FilterPreset> Presets = new List<FilterPreset>();
			/// <summary> 滤镜默认预设 </summary>
			public FilterPreset DefaultPreset = new FilterPreset("DEFAULT");

			internal FilterEffect m_setting;

			public Filter(FilterEffect setting)
			{
				m_setting = setting;

				loadPresets();
			}

			private void loadPresets()
			{
				var json = PlayerPrefs.GetString($"Filter_{Name}_PresetList", string.Empty);
				var loadedPresets = JsonUtility.FromJson<FilterPresetList>(json);
				if (loadedPresets == null) return;
				else Presets = loadedPresets.presets;
			}

			public void SavePresets()
			{
				var json = JsonUtility.ToJson(new FilterPresetList(Presets));
				PlayerPrefs.SetString($"Filter_{Name}_PresetList", json);
			}

			public MsgBool CreatePreset(string presetName, out FilterPreset newPreset)
			{
				newPreset = null;
				if (string.IsNullOrWhiteSpace(presetName)) return "名称不能为空";
				if (Presets.Count(p => p.Name == presetName) != 0) return "名称重复";

				newPreset = new FilterPreset(presetName);
				Presets.Add(newPreset);

				SavePresets();

				return true;
			}

			public void RemovePreset(FilterPreset preset)
			{
				if (!Presets.Remove(preset)) return;
				SavePresets();

				EventInvoker.RaiseFilterPresetRemoved(this, preset);
			}

			public void ResetPreset()
			{
				foreach (var param in Paramerters)
				{
					param.ResetToDefault();
				}
			}

			public void ApplyPreset(FilterPreset preset)
			{
				foreach (var param in Paramerters)
				{
					var value = preset.GetParamValue(param.Name, param.ValueType);
					if (value == null)
						param.ResetToDefault();
					else
						param.Apply(value);
				}

			}


		}

		[Serializable]
		private class FilterPresetList
		{
			public List<FilterPreset> presets;

			public FilterPresetList(List<FilterPreset> presets)
			{
				this.presets = presets;
				foreach (var preset in presets)
				{
					preset.ReadyForJson();
				}
			}
		}

		[Serializable]
		public class FilterPreset
		{
			[SerializeField]
			public string Name;
			[SerializeField]
			private List<string> m_paramName = new List<string>();
			[SerializeField]
			private List<string> m_valueJson = new List<string>();

			private bool m_cacheReady = false;
			private Dictionary<string, string> m_paramName2ValueJson;
			public FilterPreset(string presetName)
			{
				Name = presetName;
			}

			public void ReadyForJson()
			{
				prepareCache();

				m_paramName = m_paramName2ValueJson.Keys.ToList();
				m_valueJson = m_paramName2ValueJson.Values.ToList();
			}

			public string GetParamValueJson(string paramName)
			{
				prepareCache();
				string value;
				m_paramName2ValueJson.TryGetValue(paramName, out value);
				return value;
			}

			public object GetParamValue(string paramName, Type valueType)
			{
				var rawStr = GetParamValueJson(paramName);
				if (rawStr == null) return null;

				if (valueType == typeof(float))
				{
					float floatVal;
					float.TryParse(rawStr, out floatVal);
					return floatVal;
				}
				else if (valueType.IsEnum)
				{
					var names = Enum.GetNames(valueType);
					var values = Enum.GetValues(valueType);

					for (int i = 0; i < names.Length; i++)
					{
						if (names[i].Equals(rawStr))
						{
							return values.GetValue(i);
						}
					}
					return null;
				}
				else
				{
					App.log.Error($"尚未支持的滤镜参数类型{valueType}");
					return null;
				}
			}

			public void SetParamValue(string paramName, Type valueType, object value)
			{
				prepareCache();
				m_paramName2ValueJson[paramName] = value.ToString();
			}

			private void prepareCache()
			{
				if (m_cacheReady) return;

				m_paramName2ValueJson = new Dictionary<string, string>();
				for (int i = 0; i < m_paramName.Count; i++)
				{
					m_paramName2ValueJson[m_paramName[i]] = m_valueJson[i];
				}

				m_cacheReady = true;
			}
		}

		[Serializable]
		public class FilterRomSetting
		{
			[SerializeField]
			private List<int> m_romID;
			[SerializeField]
			private List<Item> m_items;

			bool m_cacheReady = false;
			Dictionary<int, Item> m_cache;

			public void Setup(RomFile rom, Filter filter, FilterPreset preset)
			{
				prepareCache();

				if (filter == null)
					m_cache.Remove(rom.ID);
				else
					m_cache[rom.ID] = new Item { FilterName = filter.Name, PresetName = preset != null ? preset.Name : null };
			}

			public string ToJson()
			{
				prepareCache();
				m_romID = m_cache.Keys.ToList();
				m_items = m_cache.Values.ToList();

				return JsonUtility.ToJson(this);
			}

			public ValueTuple<string, string> Get(RomFile rom)
			{
				prepareCache();

				Item item;
				m_cache.TryGetValue(rom.ID, out item);
				return new ValueTuple<string, string>(item.FilterName, item.PresetName);
			}

			private void prepareCache()
			{
				if (m_cacheReady) return;

				if (m_items == null) m_items = new List<Item>();
				if (m_romID == null) m_romID = new List<int>();
				m_cache = new Dictionary<int, Item>();
				for (int i = 0; i < m_romID.Count && i < m_items.Count; i++)
				{
					m_cache[m_romID[i]] = m_items[i];
				}

				m_cacheReady = true;
			}

			[Serializable]
			struct Item
			{
				public string FilterName;
				public string PresetName;
			}
		}
	}
}

using AxibugEmuOnline.Client.ClientCore;
using AxibugProtobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using static AxibugEmuOnline.Client.FilterEffect;

namespace AxibugEmuOnline.Client.Settings
{
    public class FilterManager
    {
        private List<Filter> m_filters;
        private Dictionary<RomPlatformType, Filter> m_filterPlatforms = new Dictionary<RomPlatformType, Filter>();

        private AlphaWraper m_previewFilterWraper;
        FilterRomSetting m_filterRomSetting;
        /// <summary>
        /// 滤镜列表
        /// </summary>
        public IReadOnlyList<Filter> Filters => m_filters;

        public FilterManager()
        {
            loadFilters();
            var json = AxiPlayerPrefs.GetString(nameof(FilterRomSetting));
            m_filterRomSetting = JsonUtility.FromJson<FilterRomSetting>(json) ?? new FilterRomSetting();
        }

        private void loadFilters()
        {
            m_filters = new List<Filter>();

            var effectBaseType = typeof(FilterEffect);
            foreach (var type in effectBaseType.Assembly.GetTypes())
            {
                if (type.IsAbstract) continue;
                if (type.IsInterface) continue;
                if (!effectBaseType.IsAssignableFrom(type)) continue;
                var stripAtt = type.GetCustomAttribute<StripAttribute>();
                if (stripAtt != null && stripAtt.NeedStrip) continue;
                var effect = Activator.CreateInstance(type) as FilterEffect;
                m_filters.Add(new Filter(effect));
            }
        }

        private RenderTexture result = null;
        public void ExecuteFilterRender(Texture src, RawImage renderGraphic)
        {
            //获得激活的滤镜
            Filter activeFilter = null;
            foreach (var filter in Filters)
            {
                if (!filter.m_setting.Enable) continue;
                activeFilter = filter;
                break;
            }

            if (activeFilter == null)
            {
                renderGraphic.texture = src;
                return;
            }

            var resolution = GetRawImageScreenResolution(renderGraphic);
            int resWidth = (int)(resolution.x * activeFilter.m_setting.RenderScale.GetValue());
            int resHeight = (int)(resolution.y * activeFilter.m_setting.RenderScale.GetValue());

            if (result == null)
            {
                result = RenderTexture.GetTemporary(resWidth, resHeight);
            }
            else if (result.width != resWidth || result.height != resHeight)
            {
                RenderTexture.ReleaseTemporary(result);
                result = RenderTexture.GetTemporary(resWidth, resHeight);
            }

            activeFilter.m_setting.Render(src, result);

            renderGraphic.texture = result;
        }

        Vector2 GetRawImageScreenResolution(RawImage rawImage)
        {
            // 获取 RawImage 的 RectTransform
            RectTransform rectTransform = rawImage.rectTransform;

            // 获取 RawImage 在屏幕上的四个顶点的世界坐标
            Vector3[] corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);

            // 左下角和右上角的屏幕坐标
            Vector2 bottomLeft = RectTransformUtility.WorldToScreenPoint(rawImage.canvas.worldCamera, corners[0]);
            Vector2 topRight = RectTransformUtility.WorldToScreenPoint(rawImage.canvas.worldCamera, corners[2]);

            // 计算宽度和高度
            float width = Mathf.Abs(topRight.x - bottomLeft.x);
            float height = Mathf.Abs(topRight.y - bottomLeft.y);

            return new Vector2(width, height);
        }


        /// <summary> 关闭滤镜预览 </summary>
        public void ShutDownFilterPreview()
        {
            if (m_previewFilterWraper == null) m_previewFilterWraper = new AlphaWraper(Initer.XMBBg, Initer.FilterPreview, false);
            m_previewFilterWraper.On = false;
        }

        /// <summary> 开启滤镜预览 </summary>
        public void EnableFilterPreview()
        {
            if (m_previewFilterWraper == null) m_previewFilterWraper = new AlphaWraper(Initer.XMBBg, Initer.FilterPreview, false);
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
                if (selfFiler != filter) selfFiler.m_setting.Enable = false;
                else selfFiler.m_setting.Enable = true;
            }
        }

        /// <summary>
        /// 关闭滤镜效果
        /// </summary>
        public void ShutDownFilter()
        {
            //关闭所有后处理效果
            foreach (var filter in Filters)
                filter.m_setting.Enable = false;
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
            AxiPlayerPrefs.SetString(nameof(FilterRomSetting), json);
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
                if (presetName == filter.DefaultPreset.Name) preset = filter.DefaultPreset;
                else preset = filter.Presets.FirstOrDefault(p => p.Name == presetName);
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
                var json = AxiPlayerPrefs.GetString($"Filter_{Name}_PresetList", string.Empty);
                var loadedPresets = JsonUtility.FromJson<FilterPresetList>(json);
                if (loadedPresets == null) return;
                else Presets = loadedPresets.presets;
            }

            public void SavePresets()
            {
                var json = JsonUtility.ToJson(new FilterPresetList(Presets));
                AxiPlayerPrefs.SetString($"Filter_{Name}_PresetList", json);
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
                else if (valueType == typeof(int))
                {
                    int intVal;
                    int.TryParse(rawStr, out intVal);
                    return intVal;
                }
                else if (valueType == typeof(bool))
                {
                    bool boolVal;
                    bool.TryParse(rawStr, out boolVal);
                    return boolVal;
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

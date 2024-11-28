using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using static AxibugEmuOnline.Client.FilterEffect;

namespace AxibugEmuOnline.Client
{
    public class FilterManager
    {
        private PostProcessProfile m_filterPorfile;
        private List<Filter> m_filters;
        private Dictionary<EnumPlatform, Filter> m_filterPlatforms = new Dictionary<EnumPlatform, Filter>();
        private AlphaWraper m_previewFilterWraper;
        /// <summary>
        /// 滤镜列表
        /// </summary>
        public IReadOnlyList<Filter> Filters => m_filters;

        public FilterManager(PostProcessVolume filterVolume, CanvasGroup filterPreview, CanvasGroup mainBg)
        {
            m_filterPorfile = filterVolume.profile;
            m_filters = m_filterPorfile.settings.Where(setting => setting is FilterEffect).Select(setting => new Filter(setting as FilterEffect)).ToList();

            m_previewFilterWraper = new AlphaWraper(mainBg, filterPreview, false);
            ShutDownFilterPreview();
            ShutDownFilter();
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
                if (selfFiler != filter) selfFiler.m_setting.enabled.Override(false);
                else selfFiler.m_setting.enabled.Override(true);
            }
        }

        /// <summary>
        /// 关闭滤镜效果
        /// </summary>
        public void ShutDownFilter()
        {
            //关闭所有后处理效果
            foreach (var setting in m_filterPorfile.settings)
                setting.enabled.Override(false);
        }

        public class Filter
        {
            public string Name => m_setting.Name;
            public IReadOnlyCollection<EditableParamerter> Paramerters => m_setting.EditableParam;
            /// <summary> 滤镜预设 </summary>
            public List<FilterPreset> Presets = new List<FilterPreset>();

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

            private void savePresets()
            {
                var json = JsonUtility.ToJson(new FilterPresetList { presets = Presets });
                PlayerPrefs.SetString($"Filter_{Name}_PresetList", json);
            }

            public MsgBool CreatePreset(string presetName,out FilterPreset newPreset)
            {
                newPreset = null;
                if (Presets.Count(p => p.Name == presetName) != 0) return "名称重复";

                newPreset = new FilterPreset(presetName);
                Presets.Add(newPreset);

                savePresets();

                return true;
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
                    var json = preset.GetParamValueJson(param.Name);
                    if (string.IsNullOrEmpty(json))
                        param.ResetToDefault();
                    else
                        param.Apply(json);
                }

            }
        }

        [Serializable]
        private class FilterPresetList
        {
            public List<FilterPreset> presets;
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

            public string GetParamValueJson(string paramName)
            {
                prepareCache();

                m_paramName2ValueJson.TryGetValue(paramName, out var value);
                return value;
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
    }
}

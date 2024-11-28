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

            internal FilterEffect m_setting;

            public Filter(FilterEffect setting)
            {
                m_setting = setting;
            }

            internal IReadOnlyCollection<EditableParamerter> Paramerters => m_setting.EditableParam;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Rendering.PostProcessing;
using static AxibugEmuOnline.Client.FilterEffect;

namespace AxibugEmuOnline.Client
{
    public class FilterManager
    {
        private PostProcessProfile m_filterPorfile;
        private List<Filter> m_filters;

        /// <summary>
        /// 滤镜列表
        /// </summary>
        public IReadOnlyList<Filter> Filters => m_filters;

        public FilterManager(PostProcessVolume filterVolume)
        {
            m_filterPorfile = filterVolume.profile;
            m_filters = m_filterPorfile.settings.Where(setting=>setting is FilterEffect).Select(setting => new Filter(setting as FilterEffect)).ToList();

            ShutDownFilter();
        }
        
        /// <summary>
        /// 打开滤镜
        /// </summary>
        /// <param name="filter"></param>
        public void EnableFilter(Filter filter)
        {
            foreach(var selfFiler in Filters)
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

        public struct Filter
        {
            public bool Empty => m_setting == null;
            public readonly string Name => m_setting.name;

            internal FilterEffect m_setting;

            public Filter(FilterEffect setting)
            {
                m_setting = setting;
            }

            internal IReadOnlyCollection<EditableParamerter> Paramerters => m_setting.EditableParam;

            public override readonly int GetHashCode()
            {
                return m_setting.GetHashCode();
            }

            public override readonly bool Equals(object obj)
            {
                if(obj == null) return false;
                if (!(obj is Filter)) return false;

                return ((Filter)obj).GetHashCode() == GetHashCode();
            }

            public static bool operator ==(Filter left, Filter right)
            {
                return left.GetHashCode() == right.GetHashCode();
            }

            public static bool operator !=(Filter left, Filter right)
            {
                return !(left == right);
            }
        }
    }
}

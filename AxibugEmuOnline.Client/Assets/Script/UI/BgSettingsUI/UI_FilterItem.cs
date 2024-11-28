using AxibugEmuOnline.Client.ClientCore;
using AxibugEmuOnline.Client.UI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static AxibugEmuOnline.Client.FilterManager;

namespace AxibugEmuOnline.Client
{
    /// <summary>
    /// 背景颜色设置UI
    /// </summary>
    public class UI_FilterItem : MenuItem, IVirtualItem
    {
        public int Index { get; set; }
        public Filter Datacontext { get; private set; }

        public void SetData(object data)
        {
            Datacontext = data as Filter;

            UpdateView();
        }

        private void UpdateView()
        {
            SetBaseInfo(Datacontext.Name, $"参数数量:{Datacontext.Paramerters.Count}", null);
        }

        public void SetDependencyProperty(object data)
        {
            SetSelectState(data is ThirdMenuRoot tr && tr.SelectIndex == Index);

            if (m_select)
            {
                App.filter.EnableFilterPreview();
                App.filter.EnableFilter(Datacontext);
            }
        }

        public void Release() { }

        public override bool OnEnterItem()
        {
            var opts = new List<OptionMenu>();
            opts.Add(new Opt_CreatePreset(Datacontext));
            opts.AddRange(Datacontext.Presets.Select(p => new Opt_Presets(Datacontext, p)));

            OptionUI.Instance.Pop(opts, onClose: () =>
            {
                App.filter.EnableFilterPreview();
                Datacontext.ResetPreset();
                App.filter.EnableFilter(Datacontext);
            });
            return false;
        }


        public class Opt_CreatePreset : ExecuteMenu
        {
            private Filter m_filter;

            public Opt_CreatePreset(Filter filter) : base("创建滤镜预设", Resources.LoadAll<Sprite>("Icons/XMB-Icons/misc")[0])
            {
                m_filter = filter;
            }

            public override void OnFocus()
            {
                m_filter.ResetPreset();
                App.filter.EnableFilter(m_filter);
            }

            public override void OnExcute(OptionUI optionUI, ref bool cancelHide)
            {
                cancelHide = true;

                OverlayManager.Input((presetName) =>
                {
                    var result = m_filter.CreatePreset(presetName, out var newPreset);
                    if (!result) OverlayManager.PopMsg(result);
                    else optionUI.AddOptionMenuWhenPoping(new Opt_Presets(m_filter, newPreset));
                }, "为预设设置一个名称", string.Empty);
            }
        }
        public class Opt_Presets : ExecuteMenu
        {
            private Filter m_filter;
            private FilterPreset m_preset;

            public Opt_Presets(Filter filter, FilterPreset preset) : base(preset.Name, null)
            {
                m_filter = filter;
                m_preset = preset;
            }

            public override void OnFocus()
            {
                m_filter.ApplyPreset(m_preset);
                App.filter.EnableFilter(m_filter);
            }

            public override void OnExcute(OptionUI optionUI, ref bool cancelHide)
            {

            }
        }
    }
}

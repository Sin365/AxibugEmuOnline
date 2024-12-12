using AxibugEmuOnline.Client.ClientCore;
using AxibugEmuOnline.Client.UI;
using System;
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

            OverlayManager.PopSideBar(opts, onClose: () =>
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
                    if (!result) OverlayManager.PopTip(result);
                    else optionUI.AddOptionMenuWhenPoping(new Opt_Presets(m_filter, newPreset));
                }, "为预设设置一个名称", string.Empty);
            }
        }
        public class Opt_Presets : ExpandMenu
        {
            private Filter m_filter;
            private FilterPreset m_preset;
            private OptionUI_MenuItem m_ui;
            private List<OptionMenu> m_menu;

            public Opt_Presets(Filter filter, FilterPreset preset) : base(preset.Name, null)
            {
                m_filter = filter;
                m_preset = preset;

                m_menu = new List<OptionMenu>();
                m_menu.Add(new Opt_Delete(m_filter, m_preset));
                foreach (var p in m_filter.Paramerters)
                {
                    m_menu.Add(new Opt_ParamEditor(m_filter, p, m_preset));
                }
            }

            public override void OnShow(OptionUI_MenuItem ui)
            {
                EventInvoker.OnFilterPresetRemoved += EventInvoker_OnFilterPresetRemoved;
                m_ui = ui;
                base.OnShow(ui);
            }

            public override void OnHide()
            {
                EventInvoker.OnFilterPresetRemoved -= EventInvoker_OnFilterPresetRemoved;
            }

            private void EventInvoker_OnFilterPresetRemoved(Filter filter, FilterPreset removedPreset)
            {
                if (filter != m_filter || m_preset != removedPreset) return;
                m_ui.OptionUI.RemoveItem(m_ui);
            }

            public override void OnFocus()
            {
                m_filter.ApplyPreset(m_preset);
                App.filter.EnableFilter(m_filter);
            }

            protected override List<OptionMenu> GetOptionMenus()
            {
                return m_menu;
            }

            public class Opt_ParamEditor : ValueSetMenu
            {
                private Filter m_filter;
                private FilterEffect.EditableParamerter m_param;
                private FilterPreset m_preset;

                public override bool Visible => m_param.ValueType.IsEnum || m_param.ValueType == typeof(float);

                public Opt_ParamEditor(Filter filter, FilterEffect.EditableParamerter editParam, FilterPreset preset)
                    : base(editParam.Name)
                {
                    m_filter = filter;
                    m_param = editParam;
                    m_preset = preset;
                }

                public override Type ValueType => m_param.ValueType;

                public override object ValueRaw => m_preset.GetParamValue(m_param.Name, ValueType) ?? m_param.Value;

                public override void OnValueChanged(object newValue)
                {
                    m_preset.SetParamValue(m_param.Name, ValueType, newValue);
                    m_filter.SavePresets();
                    m_param.Apply(newValue);
                }

                public override object Min => m_param.MinValue;

                public override object Max => m_param.MaxValue;
            }

            public class Opt_Delete : ExecuteMenu
            {
                private Filter m_filter;
                private FilterPreset m_preset;

                public Opt_Delete(Filter filter, FilterPreset preset) : base("删除预设", null)
                {
                    m_filter = filter;
                    m_preset = preset;
                }

                public override void OnExcute(OptionUI optionUI, ref bool cancelHide)
                {
                    m_filter.RemovePreset(m_preset);
                }
            }


        }
    }
}

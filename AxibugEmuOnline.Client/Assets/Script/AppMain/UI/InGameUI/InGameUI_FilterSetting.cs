using AxibugEmuOnline.Client.ClientCore;
using System.Collections.Generic;
using System.Linq;
using static AxibugEmuOnline.Client.FilterManager;

namespace AxibugEmuOnline.Client
{
    public class InGameUI_FilterSetting : ExpandMenu
    {
        private InGameUI m_gameUI;

        public InGameUI_FilterSetting(InGameUI gameUI) : base("滤镜", null)
        {
            m_gameUI = gameUI;
        }

        protected override List<OptionMenu> GetOptionMenus()
        {
            List<OptionMenu> menus = new List<OptionMenu>();
            menus.Add(new FilterNone(m_gameUI.RomFile));
            menus.AddRange(App.settings.Filter.Filters.Select(f => new FilterMenu(m_gameUI.RomFile, f) as OptionMenu));
            return menus;
        }

        public class FilterNone : ExecuteMenu
        {
            private RomFile m_rom;

            public override bool IsApplied => App.settings.Filter.GetFilterSetting(m_rom).filter == null;
            public FilterNone(RomFile rom) : base("取消滤镜", null)
            {
                m_rom = rom;

                var currentFilterSetting = App.settings.Filter.GetFilterSetting(m_rom);
            }

            public override void OnExcute(OptionUI optionUI, ref bool cancelHide)
            {
                App.settings.Filter.ShutDownFilter();
                App.settings.Filter.SetupFilter(m_rom, null, null);
            }
        }

        public class FilterMenu : ExpandMenu
        {
            private Filter m_filter;
            private List<OptionMenu> m_presetsMenuItems;

            public override bool IsApplied
            {
                get
                {
                    foreach (FilterPresetMenu preset in m_presetsMenuItems)
                    {
                        if (preset.IsApplied) return true;
                    }

                    return false;
                }
            }

            public FilterMenu(RomFile rom, Filter filter) : base(filter.Name, null)
            {
                m_filter = filter;
                m_presetsMenuItems = new List<OptionMenu> { new FilterPresetMenu(rom, m_filter, m_filter.DefaultPreset) };
                m_presetsMenuItems.AddRange(m_filter.Presets.Select(preset => new FilterPresetMenu(rom, m_filter, preset)));
            }

            protected override List<OptionMenu> GetOptionMenus()
            {
                return m_presetsMenuItems;
            }
        }

        public class FilterPresetMenu : ExecuteMenu
        {
            private FilterPreset m_preset;
            private RomFile m_rom;
            private Filter m_filter;

            public override bool IsApplied
            {
                get
                {
                    var setting = App.settings.Filter.GetFilterSetting(m_rom);
                    return setting.filter == m_filter && setting.preset == m_preset;
                }
            }


            public FilterPresetMenu(RomFile rom, Filter filter, FilterPreset preset) : base(preset.Name, null)
            {
                m_preset = preset;
                m_rom = rom;
                m_filter = filter;
            }

            public override void OnExcute(OptionUI optionUI, ref bool cancelHide)
            {
                m_filter.ApplyPreset(m_preset);
                App.settings.Filter.EnableFilter(m_filter);

                App.settings.Filter.SetupFilter(m_rom, m_filter, m_preset);
            }
        }
    }
}

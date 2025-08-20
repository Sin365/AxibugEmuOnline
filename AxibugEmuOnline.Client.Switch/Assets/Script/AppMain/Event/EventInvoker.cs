using AxibugEmuOnline.Client.Settings;

namespace AxibugEmuOnline.Client
{
    public static class EventInvoker
    {
        public delegate void OnFilterPresetRemovedHandle(FilterManager.Filter filter, FilterManager.FilterPreset removedPreset);
        public static event OnFilterPresetRemovedHandle OnFilterPresetRemoved;
        public static void RaiseFilterPresetRemoved(FilterManager.Filter filter, FilterManager.FilterPreset removedPreset)
            => OnFilterPresetRemoved.Invoke(filter, removedPreset);
    }
}

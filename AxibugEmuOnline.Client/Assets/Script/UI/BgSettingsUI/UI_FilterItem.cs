using AxibugEmuOnline.Client.ClientCore;
using AxibugEmuOnline.Client.UI;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using System.Linq;
using UnityEngine;

namespace AxibugEmuOnline.Client
{
    /// <summary>
    /// ±≥æ∞—’…´…Ë÷√UI
    /// </summary>
    public class UI_FilterItem : MenuItem, IVirtualItem
    {
        public int Index { get; set; }
        public FilterManager.Filter Datacontext { get; private set; }

        public void SetData(object data)
        {
            Datacontext = (FilterManager.Filter)data;

            UpdateView();
        }

        private void Setting_OnColorChanged(XMBColor color)
        {
            UpdateView();
        }

        private void UpdateView()
        {
        }

        public void SetDependencyProperty(object data)
        {
            SetSelectState(data is ThirdMenuRoot tr && tr.SelectIndex == Index);
        }

        public void Release()
        {
        }
        public override bool OnEnterItem()
        {
            return false;
        }
    }
}

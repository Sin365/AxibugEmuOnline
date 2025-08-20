using AxibugEmuOnline.Client.InputDevices;
using UnityEngine;
using UnityEngine.UI;

namespace AxibugEmuOnline.Client.UI
{
    /// <summary>
    /// 设备信息条 ItemUI
    /// </summary>
    public class DevicesInfoItem : MonoBehaviour
    {
        [SerializeField]
        Image UI_Icon;

        public InputDevice_D Datacontext { get; private set; }

        internal void SetData(InputDevice_D device)
        {
            Datacontext = device;

            string resourcePath = $"Icons/DevicesIcons/{device.PadType}";
            UI_Icon.sprite = Resources.Load<Sprite>(resourcePath);
        }
    }
}
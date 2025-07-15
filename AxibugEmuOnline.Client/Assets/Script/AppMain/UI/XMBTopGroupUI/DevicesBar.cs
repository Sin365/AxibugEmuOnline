using AxibugEmuOnline.Client.ClientCore;
using AxibugEmuOnline.Client.InputDevices;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AxibugEmuOnline.Client.UI
{
    /// <summary>
    /// xmb信息条,设备信息条UI
    /// </summary>
    public class DevicesBar : MonoBehaviour
    {
        [SerializeField]
        DevicesInfoItem ITEM_TEMPLATE;
        List<DevicesInfoItem> m_runtimeItemUI = new List<DevicesInfoItem>();

        private void Awake()
        {
            ITEM_TEMPLATE.gameObject.SetActiveEx(false);
        }

        void OnEnable()
        {
            App.input.OnDeviceConnected += Input_OnDeviceConnected;
            App.input.OnDeviceLost += Input_OnDeviceLost;
            foreach (var device in App.input.GetDevices())
            {
                AddDeviceItemUI(device);
            }
        }

        private void OnDisable()
        {
            App.input.OnDeviceConnected -= Input_OnDeviceConnected;
            App.input.OnDeviceLost -= Input_OnDeviceLost;
            foreach (var itemUI in m_runtimeItemUI)
            {
                Destroy(itemUI.gameObject);
            }
            m_runtimeItemUI.Clear();
        }

        private void AddDeviceItemUI(InputDevice_D device)
        {
            var newItemUI = GameObject.Instantiate(ITEM_TEMPLATE.gameObject, ITEM_TEMPLATE.transform.parent).GetComponent<DevicesInfoItem>();
            newItemUI.gameObject.SetActiveEx(true);
            newItemUI.SetData(device);
            m_runtimeItemUI.Add(newItemUI);
        }

        void Input_OnDeviceConnected(InputDevice_D connectDevice)
        {
            AddDeviceItemUI(connectDevice);
        }
        private void Input_OnDeviceLost(InputDevice_D lostDevice)
        {
            var targetUI = m_runtimeItemUI.FirstOrDefault(itemUI => itemUI.Datacontext == lostDevice);
            Destroy(targetUI.gameObject);
            m_runtimeItemUI.Remove(targetUI);
        }
    }
}

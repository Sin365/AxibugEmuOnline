using AxibugEmuOnline.Client.InputDevices;
using System;

namespace AxibugEmuOnline.Client.Settings
{
    /// <summary>
    /// 在所有<see cref="EmuCoreBinder{T}"/>的派生类中实现此接口以支持一种设备的绑定
    /// <para>一种<see cref="EmuCoreBinder{T}"/>
    /// 一个<see cref="EmuCoreBinder{T}.ControllerBinder"/>可以与多种设备建立绑定,但设备类型不可重复</para>
    /// </summary>
    public interface IDeviceBinder<ENUM, DEVICE>
        where ENUM : Enum
        where DEVICE : InputDevice_D
    {
        void Bind(DEVICE device, EmuCoreBinder<ENUM>.ControllerBinder controller);
    }
}
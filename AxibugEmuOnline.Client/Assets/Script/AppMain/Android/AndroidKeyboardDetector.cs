#if UNITY_ANDROID
using System;
using UnityEngine;

public static class AndroidKeyboardDetector
{
    private static AndroidJavaClass _detectorClass;
    
    static AndroidKeyboardDetector()
    {
        try
        {
            _detectorClass = new AndroidJavaClass("com.unity.keyboarddetector.PhysicalKeyboardDetector");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to initialize AndroidKeyboardDetector: {e}");
        }
    }
    
    /// <summary>
    /// 判断是否为物理键盘 - 不使用字符串匹配
    /// </summary>
    public static bool IsPhysicalKeyboard(int deviceId)
    {
        if (_detectorClass == null || !Application.isMobilePlatform)
        {
            return false;
        }
        
        try
        {
            return _detectorClass.CallStatic<bool>("isPhysicalKeyboard", deviceId);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Android native detection failed: {e}");
            return false;
        }
    }
    
    /// <summary>
    /// 高级检测 - 结合多种硬件特征
    /// </summary>
    public static bool IsPhysicalKeyboardAdvanced(int deviceId)
    {
        if (_detectorClass == null || !Application.isMobilePlatform)
        {
            return false;
        }
        
        try
        {
            return _detectorClass.CallStatic<bool>("isPhysicalKeyboardAdvanced", deviceId);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Advanced detection failed: {e}");
            return false;
        }
    }


    /// <summary>
    /// 高级检测 - 判断键盘数量
    /// </summary>
    public static bool IsExternalPhysicalKeyboard(int deviceId)
    {
        if (_detectorClass == null || !Application.isMobilePlatform)
        {
            return false;
        }

        try
        {
            return _detectorClass.CallStatic<bool>("isExternalPhysicalKeyboard", deviceId);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Advanced detection failed: {e}");
            return false;
        }
    }
}

// 使用示例
public class InputDeviceManager : MonoBehaviour
{
    private void OnEnable()
    {
        UnityEngine.InputSystem.InputSystem.onDeviceChange += OnDeviceChange;
    }
    
    private void OnDisable()
    {
        UnityEngine.InputSystem.InputSystem.onDeviceChange -= OnDeviceChange;
    }
    
    private void OnDeviceChange(UnityEngine.InputSystem.InputDevice device, 
                               UnityEngine.InputSystem.InputDeviceChange change)
    {
        if (change == UnityEngine.InputSystem.InputDeviceChange.Added)
        {
            if (device is UnityEngine.InputSystem.Keyboard)
            {
                // 只使用设备ID，不使用字符串
                bool isPhysical = AndroidKeyboardDetector.IsPhysicalKeyboard(device.deviceId);
                
                if (isPhysical)
                {
                    Debug.Log($"✅ 物理键盘已连接: ID={device.deviceId}");
                    // 启用物理键盘控制
                }
                else
                {
                    Debug.Log($"❌ 虚拟键盘已过滤: ID={device.deviceId}");
                    // 禁用或忽略
                }
            }
        }
    }
}
#endif
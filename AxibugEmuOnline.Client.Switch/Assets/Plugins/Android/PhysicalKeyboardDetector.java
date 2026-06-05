package com.unity.keyboarddetector;

import android.content.Context;
import android.view.InputDevice;
import android.os.Build;

public class PhysicalKeyboardDetector {
    
    /**
     * 判断是否为物理键盘 - 完全不使用字符串匹配
     * @param deviceId 设备ID
     * @return true 如果是物理键盘
     */
    public static boolean isPhysicalKeyboard(int deviceId) {
        InputDevice device = InputDevice.getDevice(deviceId);
        if (device == null) {
            return false;
        }
        
        // 1. 检查设备源类型（最关键）
        int sources = device.getSources();
        
        // 如果设备源包含触摸屏，绝对不是物理键盘
        if ((sources & InputDevice.SOURCE_TOUCHSCREEN) == InputDevice.SOURCE_TOUCHSCREEN) {
            return false;
        }
        
        // 如果设备源不包含键盘，不是键盘设备
        if ((sources & InputDevice.SOURCE_KEYBOARD) != InputDevice.SOURCE_KEYBOARD) {
            return false;
        }
        
        // 2. 检查键盘类型
        int keyboardType = device.getKeyboardType();
        
        // KEYBOARD_TYPE_ALPHABETIC = 2 (物理全键盘)
        // KEYBOARD_TYPE_NON_ALPHABETIC = 1 (数字键盘等)
        // KEYBOARD_TYPE_NONE = 0 (不是键盘)
        if (keyboardType != InputDevice.KEYBOARD_TYPE_ALPHABETIC) {
            return false;
        }
        
        // 3. 检查是否是虚拟设备（通过设备ID范围判断）
        if (isVirtualDeviceByIds(device)) {
            return false;
        }
        
        // 4. 检查按键布局（物理键盘有特定的按键布局）
        if (!hasPhysicalKeyLayout(device)) {
            return false;
        }
        
        // 5. 检查设备是否支持多点触控（物理键盘不支持）
        if (device.getMotionRanges().size() > 0) {
            // 物理键盘通常没有运动范围（没有触摸、鼠标等）
            // 但有些游戏手柄也可能被识别为键盘，需要进一步排除
            return false;
        }
        
        return true;
    }
    
    /**
     * 通过设备ID和控制器编号判断是否为虚拟设备
     */
    private static boolean isVirtualDeviceByIds(InputDevice device) {
        int id = device.getId();
        
        // Android 虚拟设备通常使用特定的ID范围
        // 虚拟设备ID通常小于某个阈值（不同设备可能不同）
        if (id < 0) {
            return true;
        }
        
        // 检查控制器编号
        int controllerNumber = device.getControllerNumber();
        if (controllerNumber == 0) {
            // 控制器编号为0通常是内置设备（如电源键、音量键）
            return true;
        }
        
        return false;
    }
    
    /**
     * 检查是否有物理键盘的按键布局
     */
    private static boolean hasPhysicalKeyLayout(InputDevice device) {
        // 物理键盘必须支持字母键
        int[] requiredKeys = {
            android.view.KeyEvent.KEYCODE_A,
            android.view.KeyEvent.KEYCODE_B,
            android.view.KeyEvent.KEYCODE_C,
            android.view.KeyEvent.KEYCODE_SPACE,
            android.view.KeyEvent.KEYCODE_ENTER,
            android.view.KeyEvent.KEYCODE_SHIFT_LEFT,
            android.view.KeyEvent.KEYCODE_ALT_LEFT
        };
        
        boolean[] keyResults = device.hasKeys(requiredKeys);
        
        // 统计支持的按键数量
        int supportedCount = 0;
        for (boolean supported : keyResults) {
            if (supported) {
                supportedCount++;
            }
        }
        
        // 物理键盘应该支持大部分这些按键
        return supportedCount >= 5;
    }
    
    /**
     * 备用方法：通过更多硬件特征判断
     */
    public static boolean isPhysicalKeyboardAdvanced(int deviceId) {
        InputDevice device = InputDevice.getDevice(deviceId);
        if (device == null) {
            return false;
        }
        
        // 检查设备是否有振动功能（物理键盘通常没有）
        if (device.getVibrator().hasVibrator()) {
            return false;
        }
        
        // 检查设备是否有灯光（物理键盘可能有背光）
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            if (device.hasKeys(android.view.KeyEvent.KEYCODE_CAPS_LOCK)[0] &&
                device.hasKeys(android.view.KeyEvent.KEYCODE_NUM_LOCK)[0]) {
                // 物理键盘通常有Caps Lock和Num Lock
                return true;
            }
        }
        
        return false;
    }
	
	//判断键盘数量
	public static boolean isExternalPhysicalKeyboard(int deviceId) {
		InputDevice device = InputDevice.getDevice(deviceId);
		if (device == null) return false;
		
		// 检查是否是外部设备（通过 USB 或蓝牙连接）
		String descriptor = device.getDescriptor();
		if (descriptor == null) return false;
		
		// 外部物理键盘通常通过 USB 或蓝牙连接
		// 可以通过设备描述符的特征判断，但这仍然涉及字符串
		// 所以我们改用以下方法：
		
		// 1. 检查设备是否支持键盘布局
		if (device.getKeyboardType() != InputDevice.KEYBOARD_TYPE_ALPHABETIC) {
			return false;
		}
		
		// 2. 检查设备是否有多个按键
		int[] testKeys = new int[26]; // A-Z
		for (int i = 0; i < 26; i++) {
			testKeys[i] = android.view.KeyEvent.KEYCODE_A + i;
		}
		
		boolean[] results = device.hasKeys(testKeys);
		int letterCount = 0;
		for (boolean result : results) {
			if (result) letterCount++;
		}
		
		// 物理键盘应该支持大部分字母键
		return letterCount >= 20;
	}
}
using AxibugProtobuf;
using System;
using System.Collections.Generic;

namespace AxibugEmuOnline.Client.Settings
{
    /// <summary>
    /// 管理键位映射设置
    /// </summary>
    public class KeyMapperSetting
    {
        Dictionary<RomPlatformType, InternalEmuCoreBinder> m_binders = new Dictionary<RomPlatformType, InternalEmuCoreBinder>();
        Dictionary<Type, InternalEmuCoreBinder> m_bindersByType = new Dictionary<Type, InternalEmuCoreBinder>();
        public KeyMapperSetting()
        {
            //反射拿所有核心的键位绑定
            var baseType = typeof(InternalEmuCoreBinder);
            foreach (var t in baseType.Assembly.ExportedTypes)
            {
                if (t.IsAbstract) continue;
                if (!baseType.IsAssignableFrom(t)) continue;

                var binderIns = Activator.CreateInstance(t) as InternalEmuCoreBinder;
                m_binders.Add(binderIns.Platform, binderIns);
                m_bindersByType.Add(binderIns.GetType(), binderIns);
            }
        }

        public T GetBinder<T>() where T : InternalEmuCoreBinder
        {
            InternalEmuCoreBinder binder;
            m_bindersByType.TryGetValue(typeof(T), out binder);
            return binder as T;
        }

        public T GetBinder<T>(RomPlatformType romType) where T : InternalEmuCoreBinder
        {
            InternalEmuCoreBinder binder;
            m_binders.TryGetValue(romType, out binder);
            return binder as T;
        }
    }
}
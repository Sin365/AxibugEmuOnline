using System;
using System.Collections;
using UnityEngine;

namespace Assets.Script.AppMain.Filter
{
    public abstract class FilterParameter
    {
        public abstract Type ValueType { get; }

        object m_overrideValue;
        protected object m_defaultValue;
        public object Value
        {
            get => m_overrideValue ?? m_defaultValue;
            set => m_overrideValue = value;
        }
    }

    public class FilterParameter<T> : FilterParameter
    {
        public override Type ValueType => typeof(T);
        public void Override(T value)
        {
            Value = value;
        }
        public T GetValue() => (T)Value;

        public FilterParameter(T defaultValue)
        {
            m_defaultValue = defaultValue;
        }
    }

    public class BoolParameter : FilterParameter<bool>
    {
        public BoolParameter(bool defaultValue) : base(defaultValue) { }
    }

    public class Vector2Parameter : FilterParameter<Vector2>
    {
        public Vector2Parameter(Vector2 defaultValue) : base(defaultValue) { }
    }

    public class FloatParameter : FilterParameter<float>
    {
        public FloatParameter(float defaultValue) : base(defaultValue) { }
    }
}
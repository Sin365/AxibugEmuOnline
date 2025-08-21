﻿using System;
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

        public static implicit operator T(FilterParameter<T> value)
        {
            return value.GetValue();
        }

        public static implicit operator FilterParameter<T>(T value)
        {
            return new FilterParameter<T>(value);
        }
    }

    public class BoolParameter : FilterParameter<bool>
    {
        public BoolParameter(bool defaultValue) : base(defaultValue) { }

        public static implicit operator bool(BoolParameter value)
        {
            return value.GetValue();
        }

        public static implicit operator BoolParameter(bool value)
        {
            return new BoolParameter(value);
        }
    }

    public class Vector2Parameter : FilterParameter<Vector2>
    {
        public Vector2Parameter(Vector2 defaultValue) : base(defaultValue) { }

        public static implicit operator Vector2(Vector2Parameter value)
        {
            return value.GetValue();
        }

        public static implicit operator Vector2Parameter(Vector2 value)
        {
            return new Vector2Parameter(value);
        }
    }

    public class FloatParameter : FilterParameter<float>
    {
        public FloatParameter(float defaultValue) : base(defaultValue) { }

        public static implicit operator float(FloatParameter value)
        {
            return value.GetValue();
        }

        public static implicit operator FloatParameter(float value)
        {
            return new FloatParameter(value);
        }
    }

    public class IntParameter : FilterParameter<int>
    {
        public IntParameter(int defaultValue) : base(defaultValue) { }

        public static implicit operator int(IntParameter value)
        {
            return value.GetValue();
        }

        public static implicit operator IntParameter(int value)
        {
            return new IntParameter(value);
        }
    }
}
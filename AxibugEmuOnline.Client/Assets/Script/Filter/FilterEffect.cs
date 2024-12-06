using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace AxibugEmuOnline.Client
{
    public abstract class FilterEffect : PostProcessEffectSettings
    {
        private List<EditableParamerter> m_editableParamList;

        public IReadOnlyCollection<EditableParamerter> EditableParam => m_editableParamList.AsReadOnly();

        public abstract string Name { get; }

        public FilterEffect()
        {
            GetEditableFilterParamters();
        }
        protected void GetEditableFilterParamters()
        {
            var parameters = (from t in GetType().GetFields(BindingFlags.Instance | BindingFlags.Public)
                              where t.FieldType.IsSubclassOf(typeof(ParameterOverride))
                              where t.DeclaringType.IsSubclassOf(typeof(FilterEffect))
                              orderby t.MetadataToken
                              select t);

            m_editableParamList = new List<EditableParamerter>();
            foreach (var param in parameters)
            {
                var paramObj = (ParameterOverride)param.GetValue(this);
                var rangeAtt = param.GetCustomAttribute<RangeAttribute>();
                float min = 0;
                float max = 10;
                if (rangeAtt != null)
                {
                    min = rangeAtt.min; max = rangeAtt.max;
                }

                var editableParam = new EditableParamerter(param.Name, paramObj, min, max);
                m_editableParamList.Add(editableParam);
            }
        }

        public class EditableParamerter
        {
            private ParameterOverride m_paramObject;
            private FieldInfo valueFieldInfo;

            public Type ValueType { get; private set; }
            public string Name { get; private set; }
            public object Value
            {
                get => valueFieldInfo.GetValue(m_paramObject);
                set
                {
                    valueFieldInfo.SetValue(m_paramObject, value);
                    m_paramObject.overrideState = true;
                }
            }
            public object MinValue { get; private set; }
            public object MaxValue { get; private set; }

            public EditableParamerter(string name, ParameterOverride paramObject, object minValue, object maxValue)
            {
                m_paramObject = paramObject;
                Name = name;

                var paramType = paramObject.GetType();

                valueFieldInfo = paramType.GetField("value", BindingFlags.Public | BindingFlags.Instance);
                if (valueFieldInfo != null)
                {
                    ValueType = valueFieldInfo.FieldType;
                }
                else
                {
                    ValueType = typeof(object);
                }

                MinValue = minValue;
                MaxValue = maxValue;
            }

            public void ResetToDefault() => m_paramObject.overrideState = false;


            public void Apply(object overrideValue)
            {
                Value = overrideValue;
            }
        }
    }
}

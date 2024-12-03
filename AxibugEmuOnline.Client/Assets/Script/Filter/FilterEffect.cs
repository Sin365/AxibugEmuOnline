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
            m_editableParamList = parameters.Select(p => new EditableParamerter(p.Name, (ParameterOverride)p.GetValue(this))).ToList();
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
                }
            }
            public EditableParamerter(string name, ParameterOverride paramObject)
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
            }

            public void ResetToDefault() => m_paramObject.overrideState = false;

            public string Serilized()
            {
                return JsonUtility.ToJson(Value);
            }

            public void Apply(string json)
            {
                var overrideValue = JsonUtility.FromJson(json, ValueType);
                Value = overrideValue;
            }
        }
    }
}

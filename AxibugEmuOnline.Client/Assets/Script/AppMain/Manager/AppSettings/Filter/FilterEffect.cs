using Assets.Script.AppMain.Filter;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace AxibugEmuOnline.Client
{
    public abstract class FilterEffect
    {
        public bool Enable { get; set; }

        [Range(0.1f, 4f)]
        public FloatParameter RenderScale;

        public abstract string Name { get; }
        public IReadOnlyCollection<EditableParamerter> EditableParam => m_editableParamList.AsReadOnly();

        List<EditableParamerter> m_editableParamList;
        Material m_material;
        protected virtual float m_defaultRenderScale { get => 1f; }
        protected abstract string ShaderName { get; }
        private static int m_iResolutionID = Shader.PropertyToID("_iResolution");

        public FilterEffect()
        {
            RenderScale = new FloatParameter(m_defaultRenderScale);
            GetEditableFilterParamters();

            Init();
        }

        protected virtual void Init()
        {
            if (ShaderName != null)
            {
                var shader = Shader.Find(ShaderName);
                if (shader != null) m_material = new Material(Shader.Find(ShaderName));
            }

            OnInit(m_material);
        }

        protected virtual void OnInit(Material renderMat) { }

        void GetEditableFilterParamters()
        {
            var parameters = (from t in GetType().GetFields(BindingFlags.Instance | BindingFlags.Public)
                              where t.FieldType.IsSubclassOf(typeof(FilterParameter))
                              orderby t.MetadataToken
                              select t);

            m_editableParamList = new List<EditableParamerter>();
            foreach (var param in parameters)
            {
                var paramObj = (FilterParameter)param.GetValue(this);
                var rangeAtt = param.GetCustomAttribute<RangeAttribute>();
                float min = 0;
                float max = 10;
                if (rangeAtt != null) { min = rangeAtt.min; max = rangeAtt.max; }

                var descrip = param.GetCustomAttribute<DescriptionAttribute>();
                string name = descrip != null ? descrip.Description : param.Name;

                var editableParam = new EditableParamerter(name, paramObj, min, max);
                m_editableParamList.Add(editableParam);
            }
        }

        public virtual void Render(Texture src, RenderTexture result)
        {
            if (m_material != null) m_material.SetVector(m_iResolutionID, new Vector4(result.width, result.height));
            OnRenderer(m_material, src, result);
        }

        protected abstract void OnRenderer(Material renderMat, Texture src, RenderTexture result);

        public class EditableParamerter
        {
            private FilterParameter m_paramObject;

            public Type ValueType => m_paramObject.ValueType;
            public string Name { get; private set; }
            public object Value
            {
                get => m_paramObject.Value;
                set => m_paramObject.Value = value;
            }

            public object MinValue { get; private set; }
            public object MaxValue { get; private set; }

            public EditableParamerter(string name, FilterParameter paramObject, float minValue, float maxValue)
            {
                m_paramObject = paramObject;
                Name = name;

                var paramType = paramObject.GetType();

                if (paramObject.ValueType == typeof(int))
                {
                    MinValue = (int)minValue;
                    MaxValue = (int)maxValue;
                }
                else if (paramObject.ValueType == typeof(float))
                {
                    MinValue = minValue;
                    MaxValue = maxValue;
                }
            }

            public void ResetToDefault() => m_paramObject.Value = null;


            public void Apply(object overrideValue)
            {
                Value = overrideValue;
            }
        }

        [AttributeUsage(AttributeTargets.Class)]
        internal class StripAttribute : Attribute
        {
            HashSet<RuntimePlatform> m_stripPlats;
            /// <summary>
            /// 指示一个滤镜是否会在指定的平台被剔除
            /// </summary>
            /// <param name="strip">会被剔除的平台</param>
            public StripAttribute(params RuntimePlatform[] stripPlatform)
            {
                m_stripPlats = new HashSet<RuntimePlatform>(stripPlatform);
            }

            public bool NeedStrip => m_stripPlats.Contains(Application.platform);
        }
    }
}

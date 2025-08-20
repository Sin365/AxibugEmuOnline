using System;
using System.Reflection;

namespace Essgee.Extensions
{
    public static class GeneralExtensionMethods
    {
        // https://www.c-sharpcorner.com/UploadFile/ff2f08/deep-copy-of-object-in-C-Sharp/
        public static T CloneObject<T>(this T source)
        {
            var type = source.GetType();
            var target = (T)Activator.CreateInstance(type);

            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (property.CanWrite)
                {
                    if (property.PropertyType.IsValueType || property.PropertyType.IsEnum || property.PropertyType.Equals(typeof(string)) || property.PropertyType.Equals(typeof(Type)))
                        property.SetValue(target, property.GetValue(source, null), null);
                    else
                    {
                        object objPropertyValue = property.GetValue(source, null);
                        if (objPropertyValue == null)
                            property.SetValue(target, null, null);
                        else
                            property.SetValue(target, objPropertyValue.CloneObject(), null);
                    }
                }
            }
            return target;
        }
    }
}

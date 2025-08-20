//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;

//namespace Essgee.Utilities
//{
//    public class InterfaceDictionaryConverter<TInterface> : JsonConverter
//    {
//        public override bool CanConvert(Type objectType)
//        {
//            return (objectType == typeof(TInterface));
//        }

//        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
//        {
//            if (!objectType.IsGenericType || objectType.GetGenericTypeDefinition() != typeof(Dictionary<,>)) throw new InvalidOperationException("Can only deserialize dictionaries");

//            var dictionary = (System.Collections.IDictionary)Activator.CreateInstance(objectType);
//            //不再读取配置
//            var jObject = JObject.Load(reader);
//            foreach (var child in jObject.Children())
//            {
//                Type type = Assembly.GetExecutingAssembly().GetTypes().FirstOrDefault(y => typeof(TInterface).IsAssignableFrom(y) && !y.IsInterface && !y.IsAbstract && y.Name == child.Path);
//                if (type != null)
//                    dictionary.Add(child.Path, JsonConvert.DeserializeObject(child.First.ToString(), type));
//            }

//            return dictionary;
//        }

//        public override bool CanWrite
//        {
//            get { return false; }
//        }

//        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
//        {
//            throw new NotImplementedException();
//        }
//    }
//}

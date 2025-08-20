//using Newtonsoft.Json;
//using System;

//namespace Essgee.Utilities
//{
//    public class TypeNameJsonConverter : JsonConverter
//    {
//        readonly string searchNamespace;

//        public TypeNameJsonConverter(string searchNamespace)
//        {
//            this.searchNamespace = searchNamespace;
//        }

//        public override bool CanConvert(Type objectType)
//        {
//            // TODO: maybe actually check things?
//            return true;
//        }

//        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
//        {
//            if (value is Type)
//            {
//                var type = (value as Type);
//                if (type.Namespace != searchNamespace) throw new JsonSerializationException();
//                writer.WriteValue(type.Name);
//            }
//            else
//                throw new JsonSerializationException();
//        }

//        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
//        {
//            var type = Type.GetType($"{searchNamespace}.{reader.Value}");
//            if (type != null) return type;
//            else throw new JsonSerializationException();
//        }
//    }
//}

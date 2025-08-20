//using Newtonsoft.Json;
//using System;

//namespace Essgee.Utilities
//{
//    public class HexadecimalJsonConverter : JsonConverter
//    {
//        public override bool CanConvert(Type objectType)
//        {
//            // TODO: maybe actually check things?
//            return true;
//        }

//        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
//        {
//            switch (Type.GetTypeCode(value.GetType()))
//            {
//                case TypeCode.Byte:
//                case TypeCode.SByte:
//                    writer.WriteValue($"0x{value:X2}");
//                    break;
//                case TypeCode.UInt16:
//                case TypeCode.Int16:
//                    writer.WriteValue($"0x{value:X4}");
//                    break;
//                case TypeCode.UInt32:
//                case TypeCode.Int32:
//                    writer.WriteValue($"0x{value:X8}");
//                    break;
//                case TypeCode.UInt64:
//                case TypeCode.Int64:
//                    writer.WriteValue($"0x{value:X16}");
//                    break;
//                default:
//                    throw new JsonSerializationException();
//            }
//        }

//        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
//        {
//            if ((reader.Value is string value) && value.StartsWith("0x"))
//                return Convert.ChangeType(Convert.ToUInt64(value, 16), objectType);
//            else
//                throw new JsonSerializationException();
//        }
//    }
//}

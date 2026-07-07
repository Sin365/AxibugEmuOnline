using AxibugProtobuf;
using Google.Protobuf;

namespace AxibugEmuOnline.Web.Common
{
    public static class ProtoBufHelper
    {

        public static long GetDBUseUID(this Protobuf_Token_Struct msg)
        {
            return msg.ParentUID > 0 ? msg.ParentUID : msg.UID;
        }

        public static byte[] Serizlize(IMessage msg)
        {
            return msg.ToByteArray();
        }

        public static T DeSerizlize<T>(byte[] bytes)
        {
            var msgType = typeof(T);
            object msg = Activator.CreateInstance(msgType);
            ((IMessage)msg).MergeFrom(bytes);
            return (T)msg;
        }
    }

}

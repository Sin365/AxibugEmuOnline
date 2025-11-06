using AxibugEmuOnline.Client.Network;
using Google.Protobuf;
using HaoYueNet.ClientNetwork;
using System;

namespace AxibugEmuOnline.Client.Common
{
    public static class ProtoBufHelper
    {
        private static ProtobufferMsgPool _msgPool = new ProtobufferMsgPool();

        public static void RentSerizlizeData(IMessage msg, out byte[] data, out int usedlength)
        {
            usedlength = msg.CalculateSize();
            data = BytesArrayPool.RentBuffer(usedlength);
            msg.WriteTo(data.AsSpan(0, usedlength));
        }
        public static void ReturnSerizlizeData(byte[] data)
        {
            BytesArrayPool.ReturnBuffer(data);
        }

        public static byte[] Serizlize(IMessage msg)
        {
            return msg.ToByteArray();
        }
        public static T DeSerizlizeFromPool<T>(byte[] bytes)
        {
            var msgType = typeof(T);
            object msg = _msgPool.Get(msgType);
            ((IMessage)msg).MergeFrom(bytes);
            return (T)msg;
        }
        public static IMessage DeSerizlizeFromPool(byte[] bytes, Type protoType)
        {
            var msgType = protoType;
            object msg = _msgPool.Get(msgType);
            ((IMessage)msg).MergeFrom(bytes);
            return (IMessage)msg;
        }
        public static void ReleaseToPool(IMessage msg)
        {
            _msgPool.Release(msg);
        }
    }
}

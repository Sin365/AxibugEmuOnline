using AxibugEmuOnline.Client.ClientCore;
using Google.Protobuf;
using System;
using System.Collections.Generic;

namespace AxibugEmuOnline.Client.Network
{
    public class ProtobufferMsgPool
    {
        Dictionary<Type, Queue<IResetable>> _pool = new Dictionary<Type, Queue<IResetable>>();

        public IMessage Get(Type msgType)
        {
            if (!_pool.TryGetValue(msgType, out var queue))
            {
                queue = new Queue<IResetable>();
                _pool[msgType] = queue;
            }

            if (queue.Count == 0)
            {
                var msgIns = Activator.CreateInstance(msgType) as IMessage;
                return msgIns;
            }
            else
            {
                var msgIns = queue.Dequeue();
                msgIns.Reset();

                return msgIns as IMessage;
            }
        }

        public void Release(IMessage msg)
        {
            if (msg is IResetable resetableMsg)
            {
                var msgType = msg.GetType();
                if (!_pool.TryGetValue(msgType, out var queue))
                {
                    queue = new Queue<IResetable>();
                    _pool[msgType] = queue;
                }

                queue.Enqueue(resetableMsg);
            }
            else
            {
                App.log.Error("[NET]" + msg.GetType() + "}未生成Resetable代码");
            }
        }
    }

    public interface IResetable
    {
        void Reset();
    }
}
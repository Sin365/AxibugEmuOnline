using System;

namespace Essgee.EventArguments
{
    public class SaveExtraDataEventArgs : EventArgs
    {
        public ExtraDataTypes DataType { get; private set; }
        public ExtraDataOptions Options { get; private set; }
        public string Description { get; private set; }
        public object Data { get; private set; }

        public static SaveExtraDataEventArgs Create(ExtraDataTypes type, ExtraDataOptions option, string desc, object data)
        {
            var eventArgs = ObjectPoolAuto.Acquire<SaveExtraDataEventArgs>();
            eventArgs.DataType = type;
            eventArgs.Options = option;
            eventArgs.Description = desc;
            eventArgs.Data = data;
            return eventArgs;
        }
    }
    public static class SaveExtraDataEventArgsEx
    {
        public static void Release(this SaveExtraDataEventArgs eventArgs)
        {
            ObjectPoolAuto.Release(eventArgs);
        }
    }
}

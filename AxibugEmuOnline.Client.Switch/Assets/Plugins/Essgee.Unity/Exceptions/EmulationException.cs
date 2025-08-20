using System;

namespace Essgee.Exceptions
{
    [Serializable]
    public class EmulationException : Exception
    {
        public EmulationException() : base() { }
        public EmulationException(string message) : base(message) { }
        public EmulationException(string message, Exception innerException) : base(message, innerException) { }
        public EmulationException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}

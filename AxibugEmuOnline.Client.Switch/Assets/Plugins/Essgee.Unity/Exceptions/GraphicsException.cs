using System;

namespace Essgee.Exceptions
{
    [Serializable]
    public class GraphicsException : Exception
    {
        public GraphicsException() : base() { }
        public GraphicsException(string message) : base(message) { }
        public GraphicsException(string message, Exception innerException) : base(message, innerException) { }
        public GraphicsException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}

using System;

namespace Essgee.Exceptions
{
    [Serializable]
    public class CartridgeLoaderException : Exception
    {
        public CartridgeLoaderException() : base() { }
        public CartridgeLoaderException(string message) : base(message) { }
        public CartridgeLoaderException(string message, Exception innerException) : base(message, innerException) { }
        public CartridgeLoaderException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}

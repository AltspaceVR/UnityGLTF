 using System;

namespace GLTFSerializer
{
    [Serializable()]
    public class GLTFHeaderInvalidException : Exception
    {
        public GLTFHeaderInvalidException() : base() { }
        public GLTFHeaderInvalidException(string message) : base(message) { }
        public GLTFHeaderInvalidException(string message, Exception inner) : base(message, inner) { }
#if !WINDOWS_UWP
        protected GLTFHeaderInvalidException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context)
        { }
#endif
    }

    // todo blgross unity - move over when doing unity layer
    [Serializable()]
    public class ShaderNotFoundException : Exception
    {
        public ShaderNotFoundException() : base() { }
        public ShaderNotFoundException(string message) : base(message) { }
        public ShaderNotFoundException(string message, Exception inner) : base(message, inner) { }
#if !WINDOWS_UWP
        protected ShaderNotFoundException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context)
        { }
#endif
    }
}

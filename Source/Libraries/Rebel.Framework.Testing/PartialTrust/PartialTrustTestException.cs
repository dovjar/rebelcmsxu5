using System;
using System.Runtime.Serialization;

namespace Rebel.Framework.Testing.PartialTrust
{
    [Serializable]
    public class PartialTrustTestException : Exception
    {
        protected PartialTrustTestException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public PartialTrustTestException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
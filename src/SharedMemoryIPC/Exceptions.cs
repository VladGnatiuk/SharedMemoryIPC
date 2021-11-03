using System;

namespace SharedMemoryIPC
{
    public class RemotePartyIsNotResponsiveException : Exception
    {
        public RemotePartyIsNotResponsiveException(string message) : base(message)
        {
        }

        public RemotePartyIsNotResponsiveException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
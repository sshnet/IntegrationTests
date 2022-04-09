using System;

namespace SshNet.TestTools.OpenSSH
{
    public sealed class NetworkConnectivityDisruptorException : Exception
    {
        public NetworkConnectivityDisruptorException(string message, Exception innerException) :
            base(message, innerException)
        {
        }
    }
}

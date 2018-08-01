using System;

namespace UChainDB.Example.Chain.Network
{
    [Serializable]
    public class ApiClientException : Exception
    {
        public ApiClientException()
        {
        }

        public ApiClientException(string message) : base(message)
        {
        }

        public ApiClientException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
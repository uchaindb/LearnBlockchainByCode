using System;

namespace UChainDB.Example.Chain.Network
{
    public interface IApiClientFactory : IDisposable
    {
        IPeer Produce();
    }
}
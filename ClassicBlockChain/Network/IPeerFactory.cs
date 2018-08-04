using System;

namespace UChainDB.Example.Chain.Network
{
    public interface IPeerFactory : IDisposable
    {
        IPeer Produce();
    }
}
using System;

namespace UChainDB.Example.Chain.Network
{
    public interface IListener:IDisposable
    {
        event EventHandler<IPeer> OnPeerConnected;
        void Start();
    }
}
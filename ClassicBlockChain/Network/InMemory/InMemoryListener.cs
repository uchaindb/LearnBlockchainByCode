using System;

namespace UChainDB.Example.Chain.Network.InMemory
{
    public class InMemoryListener : IListener
    {
        private readonly InMemoryConnectionCenter center;

        public InMemoryListener(InMemoryConnectionCenter center, string address)
        {
            this.center = center;
            this.Address = address;
        }

        public event EventHandler<IPeer> OnPeerConnected;

        public string Address { get; }

        public void Start()
        {
        }

        public void Dispose()
        {
        }

        internal bool Connect(ActiveInMemoryPeer peer)
        {
            var oppositePeer = new PassiveInMemoryPeer(this.center, peer);
            this.center.AddPeer(oppositePeer);
            this.OnPeerConnected?.Invoke(this, oppositePeer);
            return true;
        }
    }
}
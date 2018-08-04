using System;

namespace UChainDB.Example.Chain.Network.InMemory
{
    public class InMemoryListener : IListener
    {
        private readonly InMemoryClientServerCenter center;

        public InMemoryListener(InMemoryClientServerCenter center, string address)
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

        internal bool Connect(ActiveInMemoryClient client)
        {
            var peer = new PassiveInMemoryClient(this.center, client);
            this.center.AddPeer(peer);
            this.OnPeerConnected?.Invoke(this, peer);
            return true;
        }
    }
}
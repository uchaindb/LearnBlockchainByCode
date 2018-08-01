using System;
using System.Threading.Tasks;

namespace UChainDB.Example.Chain.Network.InMemory
{
    public class InMemoryListener : IListener
    {
        private readonly InMemoryClientServerCenter center;
        internal readonly string address;

        public event EventHandler<IPeer> OnPeerConnected;

        public InMemoryListener(InMemoryClientServerCenter center, string address)
        {
            this.center = center;
            this.address = address;
        }

        internal Task<bool> ConnectAsync(ActiveInMemoryClient client)
        {
            var peer = new PassiveInMemoryClient(this.center, client );
            this.OnPeerConnected?.Invoke(this, peer);
            return Task.FromResult(true);
        }

        public void Start()
        {
        }

        public void Dispose()
        {
        }
    }
}
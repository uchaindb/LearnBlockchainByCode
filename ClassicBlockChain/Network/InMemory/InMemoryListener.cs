using System;
using System.Threading.Tasks;

namespace UChainDB.Example.Chain.Network.InMemory
{
    public class InMemoryListener : IListener
    {
        private readonly InMemoryClientServerCenter center;

        public string Address { get;}

        public event EventHandler<IPeer> OnPeerConnected;

        public InMemoryListener(InMemoryClientServerCenter center, string address)
        {
            this.center = center;
            this.Address = address;
        }

        internal Task<bool> ConnectAsync(ActiveInMemoryClient client)
        {
            var peer = new PassiveInMemoryClient(this.center, client );
            this.center.AddPeer(peer);
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
namespace UChainDB.Example.Chain.Network.InMemory
{
    public class InMemoryPeerFactory : IPeerFactory
    {
        private readonly InMemoryConnectionCenter center;
        private readonly InMemoryListener server;

        public InMemoryPeerFactory(InMemoryConnectionCenter center, InMemoryListener server)
        {
            this.center = center;
            this.server = server;
        }

        public void Dispose()
        {
        }

        public IPeer Produce()
        {
            return new ActiveInMemoryPeer(this.center, this.server.Address);
        }
    }
}
namespace UChainDB.Example.Chain.Network.InMemory
{
    public class InMemoryClientFactory : IApiClientFactory
    {
        private readonly InMemoryClientServerCenter center;
        private readonly InMemoryListener server;

        public InMemoryClientFactory(InMemoryClientServerCenter center, InMemoryListener server)
        {
            this.center = center;
            this.server = server;
        }

        public void Dispose()
        {
        }

        public IPeer Produce()
        {
            var client = new ActiveInMemoryClient(this.center, this.server.Address);
            return client;
        }
    }
}
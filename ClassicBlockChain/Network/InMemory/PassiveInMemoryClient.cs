using System;
using System.Threading;
using System.Threading.Tasks;

namespace UChainDB.Example.Chain.Network.InMemory
{
    public class PassiveInMemoryClient : InMemoryClientBase
    {
        private readonly ActiveInMemoryClient client;

        public PassiveInMemoryClient(InMemoryClientServerCenter center, ActiveInMemoryClient client) : base(center)
        {
            this.client = client;
            this.baseAddress = client.targetAddress;
            this.targetAddress = client.baseAddress;
        }

        public override Task ConnectAsync(string connectionString, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotSupportedException();
        }
    }
}
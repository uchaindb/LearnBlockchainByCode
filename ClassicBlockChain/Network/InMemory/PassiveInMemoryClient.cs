using System;

namespace UChainDB.Example.Chain.Network.InMemory
{
    public class PassiveInMemoryClient : InMemoryClientBase
    {
        public PassiveInMemoryClient(InMemoryClientServerCenter center, ActiveInMemoryClient client) : base(center)
        {
            this.opposite = client;
            this.opposite.opposite = this;
            this.BaseAddress = client.TargetAddress;
            this.TargetAddress = client.BaseAddress;
        }

        public override void Connect(string connectionString)
        {
            throw new NotSupportedException();
        }
    }
}
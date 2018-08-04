using System;

namespace UChainDB.Example.Chain.Network.InMemory
{
    public class PassiveInMemoryPeer : InMemoryPeerBase
    {
        public PassiveInMemoryPeer(InMemoryConnectionCenter center, ActiveInMemoryPeer peer) : base(center)
        {
            this.opposite = peer;
            this.opposite.opposite = this;
            this.BaseAddress = peer.TargetAddress;
            this.TargetAddress = peer.BaseAddress;
        }

        public override void Connect(string connectionString)
        {
            throw new NotSupportedException();
        }
    }
}
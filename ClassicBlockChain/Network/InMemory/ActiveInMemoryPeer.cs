namespace UChainDB.Example.Chain.Network.InMemory
{
    public class ActiveInMemoryPeer : InMemoryPeerBase
    {
        public ActiveInMemoryPeer(InMemoryConnectionCenter center, string address) : base(center)
        {
            this.BaseAddress = address;
        }

        public override void Connect(string connectionString)
        {
            this.TargetAddress = connectionString;
            if (this.center.Connect(connectionString, this))
            {
                this.IsConnected = true;
                this.center.AddPeer(this);
            }
            else
            {
                this.IsConnected = false;
            }
        }
    }
}
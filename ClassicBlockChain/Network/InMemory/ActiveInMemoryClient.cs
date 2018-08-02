using System.Threading;
using System.Threading.Tasks;

namespace UChainDB.Example.Chain.Network.InMemory
{
    public class ActiveInMemoryClient : InMemoryClientBase
    {
        public ActiveInMemoryClient(InMemoryClientServerCenter center, string address) : base(center)
        {
            this.BaseAddress = address;
        }

        public override async Task ConnectAsync(string connectionString, CancellationToken cancellationToken = default(CancellationToken))
        {
            this.TargetAddress = connectionString;
            if (await this.center.ConnectAsync(connectionString, this))
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
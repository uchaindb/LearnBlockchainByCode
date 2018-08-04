using System.Diagnostics;

namespace UChainDB.Example.Chain.Network
{
    public enum ConnectionStatus
    {
        Initial,
        Connected,
        Disconnected,
        Dead,
    }

    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ", nq}")]
    public class ConnectionNode
    {
        public ConnectionNode(string address = null)
        {
            this.Address = address;
        }

        public string Address { get; set; }
        public ConnectionStatus Status { get; set; } = ConnectionStatus.Initial;
        public IPeer Peer { get; set; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        protected virtual string DebuggerDisplay => $"{this.Peer?.BaseAddress} -> {this.Peer?.TargetAddress}";
    }
}
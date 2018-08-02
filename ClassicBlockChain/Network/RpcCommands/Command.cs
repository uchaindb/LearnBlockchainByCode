using System.Diagnostics;
using UChainDB.Example.Chain.Core;

namespace UChainDB.Example.Chain.Network.RpcCommands
{
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ", nq}")]
    public abstract class Command
    {
        public abstract string CommandType { get; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        protected virtual string DebuggerDisplay => $"{CommandType}";

        public abstract void OnReceived(Node node, ConnectionNode connectionNode);
    }
}
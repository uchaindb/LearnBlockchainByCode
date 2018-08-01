using System.Diagnostics;

namespace UChainDB.Example.Chain.Network.RpcCommands
{
    public static class Commands
    {
        public const string Version = nameof(Version);
        public const string VersionAcknowledge = nameof(VersionAcknowledge);
        public const string GetBlocks = nameof(GetBlocks);
        public const string Inventory = nameof(Inventory);
        public const string GetHeaders = nameof(GetHeaders);
        public const string Headers = nameof(Headers);
        public const string GetAddress = nameof(GetAddress);
        public const string Address = nameof(Address);
        public const string GetData = nameof(GetData);
        public const string Block = nameof(Block);
        public const string Transaction = nameof(Transaction);
        public const string NotFound = nameof(NotFound);
        public const string Reject = nameof(Reject);
    }

    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ", nq}")]
    public abstract class Command
    {
        public abstract string CommandType { get;  }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        protected virtual string DebuggerDisplay => $"{CommandType}";
    }

    public class VersionCommand : Command
    {
        public override string CommandType => Commands.Version;
    }

    public class VersionAcknowledgeCommnad : Command
    {
        public override string CommandType => Commands.VersionAcknowledge;
    }
}
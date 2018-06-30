using System.Diagnostics;
using System.Linq;

namespace UChainDB.Example.BlockChain.Chain
{
    public class Transaction : HashBase
    {
        private byte version;
        private UInt256[] inputTransactions = new UInt256[] { };

        public byte Version
        {
            get => this.version;
            set => this.SetPropertyField(ref this.version, value);
        }

        public UInt256[] InputTransactions
        {
            get => this.inputTransactions;
            set => this.SetPropertyField(ref this.inputTransactions, value);
        }

        protected internal override string HashContent => $"{this.Version}{string.Join(",", this.InputTransactions?.Select(_ => _.ToBase64()) ?? new string[] { })}";

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        protected override string DebuggerDisplay => $"{this.InputTransactions.Length}: {this.Hash.ToShort()}";
    }
}
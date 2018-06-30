using System;
using System.Diagnostics;
using System.Linq;

namespace UChainDB.Example.BlockChain.Chain
{
    public class Block : HashBase
    {
        private byte version;
        private UInt256 previousBlockHash;
        private DateTime time;
        private Transaction[] transactions = new Transaction[] { };
        private ulong nonce;

        public byte Version
        {
            get => this.version;
            set => this.SetPropertyField(ref this.version, value);
        }

        public UInt256 PreviousBlockHash
        {
            get => this.previousBlockHash;
            set => this.SetPropertyField(ref this.previousBlockHash, value);
        }

        public DateTime Time
        {
            get => this.time;
            set => this.SetPropertyField(ref this.time, value);
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public Transaction[] Transactions
        {
            get => this.transactions;
            set => this.SetPropertyField(ref this.transactions, value);
        }

        public ulong Nonce
        {
            get => this.nonce;
            set => this.SetPropertyField(ref this.nonce, value);
        }

        public override string ToString()
        {
            return this.DebuggerDisplay;
        }

        protected internal override string HashContent => $"{this.Version}{this.Nonce}{this.PreviousBlockHash}{this.Time.ToUnixTimestamp()}{string.Join(",", this.Transactions?.Select(_ => _.Hash) ?? new UInt256[] { })}";

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        protected override string DebuggerDisplay => $"{this.Hash.ToShort()}" +
            $": (" +
            $"N: {this.Nonce,8}" +
            $", " +
            $"T: {this.Transactions.Length}" +
            $")\r\n" +
            $"  {string.Join<Transaction>(Environment.NewLine + "  ", this.Transactions ?? new Transaction[] { })}";
    }
}
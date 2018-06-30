﻿using System.Diagnostics;
using System.Linq;

namespace UChainDB.Example.BlockChain.Chain
{
    public class Transaction : HashBase
    {
        private byte version;
        private string metaData;
        private UInt256[] inputTransactions = new UInt256[] { };
        private TransactionOutput[] outputOwner = new TransactionOutput[] { };

        public byte Version
        {
            get => this.version;
            set => this.SetPropertyField(ref this.version, value);
        }

        public string MetaData
        {
            get => this.metaData;
            set => this.SetPropertyField(ref this.metaData, value);
        }

        public UInt256[] InputTransactions
        {
            get => this.inputTransactions;
            set => this.SetPropertyField(ref this.inputTransactions, value);
        }

        public TransactionOutput[] OutputOwners
        {
            get => this.outputOwner;
            set => this.SetPropertyField(ref this.outputOwner, value);
        }

        public override string ToString()
        {
            return this.DebuggerDisplay;
        }

        protected internal override string HashContent => $"{this.Version}" +
            $"|{this.MetaData}" +
            $"|{string.Join(",", this.InputTransactions?.Select(_ => _.ToHex()) ?? new string[] { })}" +
            $"|{string.Join(",", this.OutputOwners?.Select(_ => _.ToString()) ?? new string[] { })}";

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        protected override string DebuggerDisplay => $"" +
            $"{this.Hash.ToShort()}: " +
            $"({string.Join(",", this.OutputOwners?.Select(_ => _.ToString()) ?? new string[] { })}) <-- " +
            ((this.InputTransactions != null && this.InputTransactions.Length > 0)
                ? $"({string.Join(",", this.InputTransactions.Select(_ => _.ToShort()))})"
                : $"(Coin Base)");
    }

    public class TransactionOutput
    {
        public string Owner { get; set; }
        public int Value { get; set; }
        public override string ToString()
        {
            return $"{this.Owner}: {this.Value}";
        }
    }
}
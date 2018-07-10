using System.Diagnostics;
using System.Linq;
using UChainDB.Example.Chain.Utility;

namespace UChainDB.Example.Chain.Entity
{
    public class Transaction : HashBase
    {
        private byte version;
        private string metaData;
        private TransactionInput[] inputTransactions = new TransactionInput[] { };
        private TransactionOutput[] outputOwners = new TransactionOutput[] { };

        public byte Version
        {
            get => this.version;
            set => this.SetPropertyField(ref this.version, value);
        }

        public TransactionInput[] InputTransactions
        {
            get => this.inputTransactions;
            set => this.SetPropertyField(ref this.inputTransactions, value);
        }

        public TransactionOutput[] OutputOwners
        {
            get => this.outputOwners;
            set => this.SetPropertyField(ref this.outputOwners, value);
        }

        public override string ToString()
        {
            return this.DebuggerDisplay;
        }

        protected internal override string HashContent => $"{this.Version}" +
            $"|{string.Join(",", this.InputTransactions?.Select(_ => _.HashContent) ?? new string[] { })}" +
            $"|{string.Join(",", this.OutputOwners?.Select(_ => _.HashContent) ?? new string[] { })}";

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        protected override string DebuggerDisplay => $"" +
            $"{this.Hash.ToShort()}: " +
            $"({string.Join(",", this.OutputOwners?.Select(_ => _.ToString()) ?? new string[] { })}) <-- " +
            ((this.InputTransactions != null && this.InputTransactions.Length > 0)
                ? $"({string.Join(",", this.InputTransactions.Select(_ => _.ToString()))})"
                : $"(Coin Base)");
    }
}
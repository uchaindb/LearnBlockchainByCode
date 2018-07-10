using System.Diagnostics;
using System.Linq;
using UChainDB.Example.Chain.Utility;

namespace UChainDB.Example.Chain.Entity
{
    public class Transaction : HashBase
    {
        private byte version;
        private TxInput[] inputTxs = new TxInput[] { };
        private TxOutput[] outputs = new TxOutput[] { };

        public byte Version
        {
            get => this.version;
            set => this.SetPropertyField(ref this.version, value);
        }

        public TxInput[] InputTxs
        {
            get => this.inputTxs;
            set => this.SetPropertyField(ref this.inputTxs, value);
        }

        public TxOutput[] Outputs
        {
            get => this.outputs;
            set => this.SetPropertyField(ref this.outputs, value);
        }

        public override string ToString()
        {
            return this.DebuggerDisplay;
        }

        protected internal override string HashContent => $"{this.Version}" +
            $"|{string.Join(",", this.InputTxs?.Select(_ => _.HashContent) ?? new string[] { })}" +
            $"|{string.Join(",", this.Outputs?.Select(_ => _.HashContent) ?? new string[] { })}";

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        protected override string DebuggerDisplay => $"" +
            $"{this.Hash.ToShort()}: " +
            $"({string.Join(",", this.Outputs?.Select(_ => _.ToString()) ?? new string[] { })}) <-- " +
            ((this.InputTxs != null && this.InputTxs.Length > 0)
                ? $"({string.Join(",", this.InputTxs.Select(_ => _.ToString()))})"
                : $"(Coin Base)");
    }
}
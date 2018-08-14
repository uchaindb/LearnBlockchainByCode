using System.Diagnostics;
using System.Linq;

namespace UChainDB.Example.Chain.Entity
{
    public class Tx : HashBase
    {
        private byte version;
        private string metaData;
        private UInt256[] inputs = new UInt256[] { };
        private TxOutput[] outputs = new TxOutput[] { };

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

        public UInt256[] Inputs
        {
            get => this.inputs;
            set => this.SetPropertyField(ref this.inputs, value);
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
            $"|{this.MetaData}" +
            $"|{string.Join(",", this.Inputs?.Select(_ => _.ToHex()) ?? new string[] { })}" +
            $"|{string.Join(",", this.Outputs?.Select(_ => _.ToString()) ?? new string[] { })}";

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        protected override string DebuggerDisplay => $"" +
            $"{this.Hash.ToShort()}: " +
            $"({string.Join(",", this.Outputs?.Select(_ => _.ToString()) ?? new string[] { })}) <-- " +
            ((this.Inputs != null && this.Inputs.Length > 0)
                ? $"({string.Join(",", this.Inputs.Select(_ => _.ToShort()))})"
                : $"(Coin Base)");
    }
}
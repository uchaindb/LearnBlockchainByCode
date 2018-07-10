using System;
using UChainDB.Example.Chain.Utility;

namespace UChainDB.Example.Chain.Entity
{
    public class TxInput
    {
        public UInt256 PrevTxHash { get; set; }
        public int PrevTxIndex { get; set; }
        public Signature Signature { get; set; }

        internal string HashContent => $"{this.PrevTxHash.ToHex()}" +
            $"|{this.PrevTxIndex}" +
            $"|{(this.Signature == null ? "" : Convert.ToBase64String(this.Signature))}";

        public override string ToString() => $"{this.PrevTxHash.ToShort()}" +
            $"[{this.PrevTxIndex}]" +
            $": {(this.Signature == null ? "(no sig)" : Convert.ToBase64String(this.Signature).Substring(0, 12))}";
    }
}
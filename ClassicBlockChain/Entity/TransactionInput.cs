using System;
using UChainDB.Example.Chain.Utility;

namespace UChainDB.Example.Chain.Entity
{
    public class TransactionInput
    {
        public UInt256 PrevTransactionHash { get; set; }
        public int PrevTransactionIndex { get; set; }
        public Signature Signature { get; set; }

        internal string HashContent => $"{this.PrevTransactionHash.ToHex()}" +
            $"|{this.PrevTransactionIndex}" +
            $"|{(this.Signature == null ? "" : Convert.ToBase64String(this.Signature))}";

        public override string ToString() => $"{this.PrevTransactionHash.ToShort()}" +
            $"[{this.PrevTransactionIndex}]" +
            $": {(this.Signature == null ? "(no sig)" : Convert.ToBase64String(this.Signature).Substring(0, 12))}";
    }
}
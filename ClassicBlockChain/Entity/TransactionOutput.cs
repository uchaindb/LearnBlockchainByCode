using System;

namespace UChainDB.Example.Chain.Entity
{
    public class TransactionOutput
    {
        public PublicKey PublicKey { get; set; }
        public int Value { get; set; }

        internal string HashContent => $"{this.Value}" +
            $"|{Convert.ToBase64String(this.PublicKey)}";

        public override string ToString() => $"{this.Value}" +
            $": {this.PublicKey.ToBase58().Substring(0, 12)}";
    }
}
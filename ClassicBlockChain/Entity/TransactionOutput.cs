using System;

namespace UChainDB.Example.Chain.Entity
{
    public class TransactionOutput
    {
        public byte[] Address { get; set; }
        public int Value { get; set; }

        internal string HashContent => $"{this.Value}" +
            $"|{Convert.ToBase64String(this.Address)}";

        public override string ToString() =>
                    $"{this.Value}" +
            $": {Convert.ToBase64String(this.Address).Substring(0, 12)}";
    }
}
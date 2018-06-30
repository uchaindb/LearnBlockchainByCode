namespace UChainDB.Example.Chain.Entity
{
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
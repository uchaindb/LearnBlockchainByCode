namespace UChainDB.Example.Chain.Entity
{
    public class TxOutput
    {
        public string Owner { get; set; }
        public int Value { get; set; }

        public override string ToString()
        {
            return $"{this.Owner}: {this.Value}";
        }
    }
}
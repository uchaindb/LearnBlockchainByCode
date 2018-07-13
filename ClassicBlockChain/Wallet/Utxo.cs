using UChainDB.Example.Chain.Entity;

namespace UChainDB.Example.Chain.Wallet
{
    public class Utxo
    {
        public Utxo(Transaction tx, int index)
        {
            this.Tx = tx;
            this.Index = index;
        }

        public Transaction Tx { get; }
        public int Index { get; }
    }
}
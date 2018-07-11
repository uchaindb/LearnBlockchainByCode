using UChainDB.Example.Chain.Core;
using UChainDB.Example.Chain.Entity;

namespace UChainDB.Example.Chain
{
    public interface IWallet
    {
        string Name { get; }
        PrivateKey PrivateKey { get; }
        PublicKey PublicKey { get; }

        void SendMoney(Engine engine, Transaction utxo, int index, IWallet receiver, int value);

        void SendMoney(Engine engine, (Transaction utxo, int idx)[] utxos, params TxOutput[] outputs);

        void GenerateKeyPair();

        (Transaction tx, int index)[] GetUtxos(Engine engine);
    }
}
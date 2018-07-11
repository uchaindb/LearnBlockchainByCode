using UChainDB.Example.Chain.Core;
using UChainDB.Example.Chain.Entity;

namespace UChainDB.Example.Chain
{
    public interface IWallet
    {
        string Name { get; }
        PrivateKey PrivateKey { get; }
        PublicKey PublicKey { get; }

        Transaction SendMoney(Engine engine, Transaction utxo, int index, IWallet receiver, int value, int fee = 0);

        Transaction SendMoney(Engine engine, (Transaction utxo, int idx)[] utxos, params TxOutput[] outputs);

        void GenerateKeyPair();

        (Transaction tx, int index)[] GetUtxos(Engine engine);

        void SyncBlockHead(Engine engine);

        bool VerifyTx(Engine engine, Transaction tx);
    }
}
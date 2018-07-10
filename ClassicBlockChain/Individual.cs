using System.Text;
using UChainDB.Example.Chain.Core;
using UChainDB.Example.Chain.Entity;
using UChainDB.Example.Chain.Utility;

namespace UChainDB.Example.Chain
{
    public class Individual
    {
        ISignAlgorithm signAlgo = new ECDsaSignAlgorithm();
        public PublicKey PublicKey { get; }
        public PrivateKey PrivateKey { get; }
        public string Name { get; }

        public Individual(string name)
        {
            this.Name = name;
            this.PrivateKey = this.signAlgo.GenerateRandomPrivateKey();
            this.PublicKey = this.signAlgo.GetPublicKey(this.PrivateKey);
        }

        public void SendMoney(Engine engine, Transaction utxo, int index, Individual receiver, int value)
        {
            SendMoney(
                engine,
                new[] { new TransactionInput { PrevTransactionHash = utxo.Hash, PrevTransactionIndex = index } },
                new TransactionOutput { PublicKey = receiver.PublicKey, Value = value });
        }

        public void SendMoney(Engine engine, TransactionInput[] utxos, params TransactionOutput[] outputs)
        {
            var tran = new Transaction
            {
                InputTransactions = utxos,
                OutputOwners = outputs,
            };
            var sig = this.signAlgo.Sign(new[] { Encoding.UTF8.GetBytes(tran.HashContent) }, this.PrivateKey);
            foreach (var intx in tran.InputTransactions)
            {
                intx.Signature = sig;
            }
            engine.AttachTransaction(tran);
        }
    }
}
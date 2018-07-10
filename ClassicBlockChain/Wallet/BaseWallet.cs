using System.Collections.Generic;
using System.Linq;
using System.Text;
using UChainDB.Example.Chain.Core;
using UChainDB.Example.Chain.Entity;
using UChainDB.Example.Chain.Utility;

namespace UChainDB.Example.Chain
{
    public abstract class BaseWallet : IWallet
    {
        protected ISignAlgorithm signAlgo = new ECDsaSignAlgorithm();

        public BaseWallet(string name)
        {
            this.Name = name;
            this.GenerateKeyPair();
        }

        public PublicKey PublicKey { get; private set; }
        public PrivateKey PrivateKey { get; private set; }
        public string Name { get; }

        public void GenerateKeyPair()
        {
            this.PrivateKey = this.signAlgo.GenerateRandomPrivateKey();
            this.PublicKey = this.signAlgo.GetPublicKey(this.PrivateKey);
            this.AfterKeyPairGenerated();
        }

        public void SendMoney(Engine engine, Transaction utxo, int index, IWallet receiver, int value)
        {
            SendMoney(
                engine,
                new[] { (utxo, index) },
                new TransactionOutput { PublicKey = receiver.PublicKey, Value = value });
        }

        public void SendMoney(Engine engine, (Transaction utxo, int idx)[] utxos, params TransactionOutput[] outputs)
        {
            var inputTrans = utxos
                .Select(_ => new TransactionInput { PrevTransactionHash = _.utxo.Hash, PrevTransactionIndex = _.idx })
                .ToArray();
            var tran = new Transaction
            {
                InputTransactions = inputTrans,
                OutputOwners = outputs,
            };
            var sigList = new Signature[tran.InputTransactions.Length];
            for (int i = 0; i < tran.InputTransactions.Length; i++)
            {
                var utxoEnt = utxos[i];
                sigList[i] = this.signAlgo.Sign(
                    new[] { Encoding.UTF8.GetBytes(tran.HashContent) },
                    FindPrivateKey(utxoEnt.utxo.OutputOwners[utxoEnt.idx].PublicKey));
            }

            for (int i = 0; i < tran.InputTransactions.Length; i++)
            {
                tran.InputTransactions[i].Signature = sigList[i];
            }
            engine.AttachTransaction(tran);
        }

        protected virtual void AfterKeyPairGenerated()
        {
        }

        protected virtual PrivateKey FindPrivateKey(PublicKey publicKey)
        {
            if (publicKey != this.PublicKey)
                throw new KeyNotFoundException("corresponding public key is not right");
            return this.PrivateKey;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UChainDB.Example.Chain.Core;
using UChainDB.Example.Chain.Entity;
using UChainDB.Example.Chain.Utility;

namespace UChainDB.Example.Chain.Wallet
{
    public abstract class BaseWallet : IWallet
    {
        protected ISignAlgorithm signAlgo = new ECDsaSignAlgorithm();

        protected Dictionary<UInt256, BlockHead> blockHeads
            = new Dictionary<UInt256, BlockHead>();

        protected BaseWallet(string name)
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

        public Transaction SendMoney(Engine engine, Transaction utxo, int index, IWallet receiver, int value, int fee = 0)
        {
            var total = utxo.Outputs[index].Value;
            var change = total - value - fee;
            var mainOutput = new TxOutput { PublicKey = receiver.PublicKey, Value = value };
            var changeOutput = new TxOutput { PublicKey = this.PublicKey, Value = change };
            return this.SendMoney(engine, new[] { (utxo, index) }, mainOutput, changeOutput);
        }

        public Transaction SendMoney(Engine engine, (Transaction utxo, int idx)[] utxos, params TxOutput[] outputs)
        {
            var inputTxs = utxos
                .Select(_ => new TxInput { PrevTxHash = _.utxo.Hash, PrevTxIndex = _.idx })
                .ToArray();
            var tx = new Transaction
            {
                InputTxs = inputTxs,
                Outputs = outputs,
            };
            var sigList = new Signature[tx.InputTxs.Length];
            for (int i = 0; i < tx.InputTxs.Length; i++)
            {
                var utxoEnt = utxos[i];
                sigList[i] = this.signAlgo.Sign(
                    new[] { Encoding.UTF8.GetBytes(tx.HashContent) },
                    this.FindPrivateKey(utxoEnt.utxo.Outputs[utxoEnt.idx].PublicKey));
            }

            for (int i = 0; i < tx.InputTxs.Length; i++)
            {
                tx.InputTxs[i].Signature = sigList[i];
            }
            engine.AttachTx(tx);

            return tx;
        }

        public Utxo[] GetUtxos(Engine engine)
        {
            var txlist = engine.BlockChain.TxToBlockDictionary
                .Select(_ => engine.BlockChain.GetTx(_.Key))
                .SelectMany(_ => _.Outputs.Select((txo, i) => new { tx = _, txo, i }))
                .Where(_ => this.ContainPubKey(_.txo.PublicKey))
                .Where(_ => !engine.BlockChain.UsedTxDictionary.ContainsKey((_.tx.Hash, _.i)))
                .Select(_ => new Utxo(_.tx, _.i))
                .ToArray();
            return txlist;
        }

        public void SyncBlockHead(Engine engine)
        {
            var blocks = engine.BlockChain.BlockHeadDictionary.ToDictionary(_ => _.Key, _ => _.Value);
            var newBlockHash = blocks.Select(_ => _.Key).ToArray();
            var oldBlockHash = this.blockHeads.Select(_ => _.Key).ToArray();
            var excepts = oldBlockHash.Except(newBlockHash).ToArray();
            if (excepts.Length > 0)
            {
                Console.WriteLine($"found [{excepts.Length}] difference in sync block");
                return;
            }
            this.blockHeads = blocks;
        }

        public bool VerifyTx(Engine engine, Transaction tx)
        {
            var (hs, flags, txnum, block) = engine.GetMerkleBlock(tx.Hash);

            var merkleRoot = MerkleTree.GetPartialTreeRootHash(txnum, hs, flags);

            // response block not exist in local blockchain
            if (!this.blockHeads.ContainsKey(block.Hash)) return false;

            var localBlock = this.blockHeads[block.Hash];
            return merkleRoot == localBlock.MerkleRoot;
        }

        protected virtual void AfterKeyPairGenerated()
        {
        }

        protected abstract PrivateKey FindPrivateKey(PublicKey publicKey);

        protected abstract bool ContainPubKey(PublicKey publicKey);
    }
}
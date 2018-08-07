using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UChainDB.Example.Chain.Core;
using UChainDB.Example.Chain.Entity;
using UChainDB.Example.Chain.SmartContracts;
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

        public Transaction SendMoney(Engine engine, Transaction utxo, int index, IWallet receiver, int value, int fee = 0, uint lockTime = 0)
        {
            var total = utxo.Outputs[index].Value;
            var change = total - value - fee;
            var mainOutput = new TxOutput { LockScripts = receiver.PublicKey.ProduceSingleLockScript(), Value = value };
            var changeOutput = new TxOutput { LockScripts = this.PublicKey.ProduceSingleLockScript(), Value = change };
            return this.SendMoney(engine, lockTime, new[] { new Utxo(utxo, index) }, mainOutput, changeOutput);
        }

        public Transaction SendMoney(Engine engine, uint lockTime, Utxo[] utxos, params TxOutput[] outputs)
        {
            var inputTxs = utxos
                .Select(_ => new TxInput { PrevTxHash = _.Tx.Hash, PrevTxIndex = _.Index })
                .ToArray();
            var tx = new Transaction
            {
                InputTxs = inputTxs,
                Outputs = outputs,
                LockTime = lockTime,
            };
            var sigList = new Signature[tx.InputTxs.Length];
            for (int i = 0; i < tx.InputTxs.Length; i++)
            {
                var utxoEnt = utxos[i];
                sigList[i] = this.signAlgo.Sign(
                    new[] { (byte[])tx.GetLockHash() },
                    this.FindPrivateKey(utxoEnt.Tx.Outputs[utxoEnt.Index].LockScripts));
            }

            for (int i = 0; i < tx.InputTxs.Length; i++)
            {
                tx.InputTxs[i].UnlockScripts = sigList[i].ProduceSingleUnlockScript();
            }
            engine.AttachTx(tx);

            return tx;
        }

        public Utxo[] GetUtxos(Engine engine)
        {
            var txlist = engine.BlockChain.TxToBlockDictionary
                .Select(_ => engine.BlockChain.GetTx(_.Key))
                .SelectMany(_ => _.Outputs.Select((txo, i) => new { tx = _, txo, i }))
                .Where(_ => this.ContainPubKey(_.txo.LockScripts))
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

        protected abstract PrivateKey FindPrivateKey(LockScripts lockScripts);

        protected abstract bool ContainPubKey(LockScripts lockScripts);
    }
}
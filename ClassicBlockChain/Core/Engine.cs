using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using UChainDB.Example.Chain.Entity;
using UChainDB.Example.Chain.Utility;

namespace UChainDB.Example.Chain.Core
{
    public class Engine : IDisposable
    {
        public readonly BlockChain BlockChain;

        private readonly Thread thWorker;
        private bool disposing = false;
        public event EventHandler<BlockHead> OnNewBlockCreated;
        private readonly IWallet MinerWallet;
        private readonly ISignAlgorithm signAlgo = new ECDsaSignAlgorithm();

        public Engine(IWallet minerWallet)
        {
            this.MinerWallet = minerWallet;
            this.BlockChain = new BlockChain();
            this.thWorker = new Thread(this.GenerateBlockThread);
            this.thWorker.Start();
        }

        public UInt256 AttachTx(Transaction tx)
        {
            this.BlockChain.AddTx(tx);
            return tx.Hash;
        }

        internal (UInt256[] hashes, BitArray flags, int txnum, BlockHead block) GetMerkleBlock(UInt256 txhash)
        {
            var (blockHead, txidx) = this.BlockChain.TxToBlockDictionary[txhash];
            var block = this.BlockChain.GetBlock(blockHead.Hash);
            var tree = new MerkleTree(block.Txs.Select(_ => _.Hash).ToArray());
            var (hashes, flags) = tree.Trim(_ => _ == txhash);

            return (hashes, flags, block.Txs.Length, blockHead);
        }

        private void GenerateBlockThread(object state)
        {
            while (!this.disposing)
            {
                try
                {
                    var block = this.GenerateBlock();
                    this.OnNewBlockCreated?.Invoke(this, block);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error when generating new block[{ex.Message}]");
                }
            }
        }

        private BlockHead GenerateBlock()
        {
            var finalTxs = this.BlockChain.DequeueTxs()
                .Where(this.ValidateTx)
                .ToList();

            this.MinerWallet.GenerateKeyPair();
            var minerTx = new Transaction
            {
                Outputs = new[] { new TxOutput { PublicKey = this.MinerWallet.PublicKey, Value = this.BlockChain.RewardOfBlock } },
            };
            var allTxs = new[] { minerTx }.Concat(finalTxs).ToArray();

            var prevBlock = this.BlockChain.Tail;
            var merkleRoot = MerkleTree.GetMerkleRoot(allTxs.Select(_ => _.Hash).ToArray());
            var blockHead = new BlockHead
            {
                PreviousBlockHash = prevBlock.Hash,
                Time = DateTime.Now,
                MerkleRoot = merkleRoot,
            };
            var block = this.BlockChain.AddBlock(new Block
            {
                Head = blockHead,
                Txs = allTxs,
            });
            return block;
        }

        private bool ValidateTx(Transaction tx)
        {
            if (this.BlockChain.ContainTx(tx.Hash)) return false;
            if (this.BlockChain.ContainUsedTxs(tx.InputTxs)) return false;
            foreach (var intx in tx.InputTxs)
            {
                var output = this.BlockChain.GetTx(intx.PrevTxHash).Outputs[intx.PrevTxIndex];
                var verifyTx = new Transaction
                {
                    Version = tx.Version,
                    InputTxs = tx.InputTxs
                        .Select(_ => new TxInput { PrevTxHash = _.PrevTxHash, PrevTxIndex = _.PrevTxIndex })
                        .ToArray(),
                    Outputs = tx.Outputs.ToArray(),
                };
                if (!this.signAlgo.Verify(new[] { Encoding.UTF8.GetBytes(verifyTx.HashContent) }, output.PublicKey, intx.Signature))
                    return false;
            }
            return true;
        }

        public void Dispose()
        {
            this.disposing = true;
            this.thWorker.Join();
        }
    }
}
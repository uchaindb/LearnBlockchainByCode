using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using UChainDB.Example.Chain.Entity;
using UChainDB.Example.Chain.SmartContracts;
using UChainDB.Example.Chain.Utility;
using UChainDB.Example.Chain.Wallet;

namespace UChainDB.Example.Chain.Core
{
    public class Engine : IDisposable
    {
        private const uint LockTimeBreakPoint = 1_500_000_000; // = July 14, 2017 2:40:00 AM
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
                .Where(this.ValidateLockTime)
                .ToList();

            var fee = CollectFee(finalTxs);
            this.MinerWallet.GenerateKeyPair();
            var minerTxOut = new TxOutput
            {
                LockScripts = this.MinerWallet.PublicKey.ProduceSingleLockScript(),
                Value = this.BlockChain.RewardOfBlock + fee
            };
            var minerTx = new Transaction { Outputs = new[] { minerTxOut }, };
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
                    LockTime = tx.LockTime,
                };
                if (!verifyTx.CanUnlock(intx, output))
                    return false;
            }
            return true;
        }

        private bool ValidateLockTime(Transaction tx)
        {
            return tx.InputTxs
                .Select(_ => this.BlockChain.GetTx(_.PrevTxHash))
                .All(_ => this.ValidateLockTime(_.LockTime, DateTime.Now));
        }

        private bool ValidateLockTime(uint lockTime, DateTime time)
        {
            // field is ignored if it's 0
            if (lockTime == 0) return true;
            if (lockTime > LockTimeBreakPoint)
            {
                // it's unix time
                var lockdt = DateTimeOffset.FromUnixTimeSeconds(lockTime);
                return lockdt > time;
            }
            else
            {
                // it's block height
                return this.BlockChain.Height > lockTime;
            }
        }

        private int CollectFee(IEnumerable<Transaction> txs)
        {
            var input = 0;
            var output = 0;
            foreach (var tx in txs)
            {
                input += tx.InputTxs.Sum(_ => this.BlockChain.GetTx(_.PrevTxHash).Outputs[_.PrevTxIndex].Value);
                output += tx.Outputs.Sum(_ => _.Value);
            }

            return input - output;
        }

        public void Dispose()
        {
            this.disposing = true;
            this.thWorker.Join();
        }
    }
}
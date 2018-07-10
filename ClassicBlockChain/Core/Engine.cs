using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using UChainDB.Example.Chain.Entity;

namespace UChainDB.Example.Chain.Core
{
    public class Engine : IDisposable
    {
        public readonly BlockChain BlockChain;

        private readonly Thread thWorker;
        private bool disposing = false;
        public event EventHandler<BlockHead> OnNewBlockCreated;
        private readonly IWallet MinerWallet;

        public Engine(IWallet minerWallet)
        {
            this.MinerWallet = minerWallet;
            this.BlockChain = new BlockChain();
            this.thWorker = new Thread(this.GenerateBlockThread);
            this.thWorker.Start();
        }

        public UInt256 AttachTransaction(Transaction transaction)
        {
            this.BlockChain.AddTransaction(transaction);
            return transaction.Hash;
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
            var finalTrans = this.BlockChain.DequeueTransactions()
                .Where(this.ValidateTransaction)
                .ToList();

            this.MinerWallet.GenerateKeyPair();
            var minerTran = new Transaction
            {
                OutputOwners = new[] { new TransactionOutput { PublicKey = this.MinerWallet.PublicKey, Value = this.BlockChain.RewardOfBlock } },
            };
            var allTrans = new[] { minerTran }.Concat(finalTrans).ToArray();

            var prevBlock = this.BlockChain.Tail;
            var merkleRoot = MerkleTree.GetMerkleRoot(allTrans.Select(_ => _.Hash).ToArray());
            var blockHead = new BlockHead
            {
                PreviousBlockHash = prevBlock.Hash,
                Time = DateTime.Now,
                MerkleRoot = merkleRoot,
            };
            var block = this.BlockChain.AddBlock(new Block
            {
                Head = blockHead,
                Transactions = allTrans,
            });
            return block;
        }

        private bool ValidateTransaction(Transaction tran)
        {
            return !this.BlockChain.ContainTransaction(tran.Hash)
                && !this.BlockChain.ContainUsedTransactions(tran.InputTransactions);
        }

        public void Dispose()
        {
            this.disposing = true;
            this.thWorker.Join();
        }
    }
}
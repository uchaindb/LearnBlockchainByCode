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
        public event EventHandler<Block> OnNewBlockCreated;
        private readonly string MinerName;

        public Engine(string minerName)
        {
            this.MinerName = minerName;
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

        private Block GenerateBlock()
        {
            var finalTrans = this.BlockChain.DequeueTransactions()
                .Where(this.ValidateTransaction)
                .ToList();

            var minerTran = new Transaction
            {
                OutputOwners = new[] { new TransactionOutput { Owner = this.MinerName, Value = this.BlockChain.RewardOfBlock } },
                MetaData = DateTime.Now.Ticks.ToString(),
            };
            var allTrans = new[] { minerTran }.Concat(finalTrans).ToArray();

            var prevBlock = this.BlockChain.Tail;
            var block = this.BlockChain.AddBlock(new Block
            {
                PreviousBlockHash = prevBlock.Hash,
                Time = DateTime.Now,
                Transactions = allTrans,
            });
            return block;
        }

        private bool ValidateTransaction(Transaction tran)
        {
            return string.IsNullOrEmpty(tran.MetaData)
                && !this.BlockChain.ContainTransaction(tran.Hash);
        }

        public void Dispose()
        {
            this.disposing = true;
            this.thWorker.Join();
        }
    }
}
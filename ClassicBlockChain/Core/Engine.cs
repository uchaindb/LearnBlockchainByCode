using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UChainDB.Example.BlockChain.Chain;

namespace UChainDB.Example.BlockChain.Core
{
    public class Engine : IDisposable
    {
        public BlockChain BlockChain;

        private Thread thWorker;
        private bool disposing = false;
        public event EventHandler<Block> OnNewBlockCreated;
        private readonly string MinerName;

        public Engine(string minerName)
        {
            this.MinerName = minerName;
            this.BlockChain = new BlockChain();
            this.thWorker = new Thread(GenerateBlock);
            this.thWorker.Start();
        }

        public UInt256 AttachTransaction(Transaction transaction)
        {
            this.BlockChain.AddTransaction(transaction);
            return transaction.Hash;
        }

        internal void AppendBlocks(Block[] blocks)
        {
            this.InitBlocks(blocks);
        }

        private void GenerateBlock(object state)
        {
            while (true)
            {
                if (this.disposing) return;
                Thread.Sleep(10);
                try
                {
                    var vts = this.BlockChain.DequeueTransactions();

                    var finalTrans = new List<Transaction>();

                    foreach (var vt in vts)
                    {
                        var tran = vt;
                        if (!this.BlockChain.ContainTransaction(tran.Hash))
                        {
                            finalTrans.Add(tran);
                        }
                    }

                    var minerTran = new Transaction
                    {
                        OutputOwners = new[] { new TransactionOutput { Owner = MinerName, Value = BlockChain.RewardOfBlock } },
                        MetaData = DateTime.Now.ToString() + DateTime.Now.Ticks.ToString(),
                    };
                    var allTrans = new[] { minerTran }.Concat(finalTrans).ToArray();

                    var prevBlock = this.BlockChain.Tail;
                    var height = prevBlock.Nonce + 1;

                    var block = this.BlockChain.AddBlock(new Block
                    {
                        PreviousBlockHash = prevBlock.Hash,
                        Time = DateTime.Now,
                        Transactions = allTrans,
                    });

                    this.OnNewBlockCreated?.Invoke(this, block);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error when generating new block[{ex.Message}]");
                }
            }
        }

        private void InitBlocks(Block[] blocks)
        {
            this.BlockChain.InitBlocks(blocks);

            this.OnNewBlockCreated?.Invoke(this, null);
        }

        public void Dispose()
        {
            this.disposing = true;
            this.thWorker.Join();
        }
    }
}
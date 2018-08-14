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

        public UInt256 AttachTx(Tx tx)
        {
            this.BlockChain.AddTx(tx);
            return tx.Hash;
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
            var finalTrans = this.BlockChain.DequeueTxs()
                .Where(this.ValidateTx)
                .ToList();

            var minerTran = new Tx
            {
                Outputs = new[] { new TxOutput { Owner = this.MinerName, Value = this.BlockChain.RewardOfBlock } },
                MetaData = DateTime.Now.Ticks.ToString(),
            };
            var allTrans = new[] { minerTran }.Concat(finalTrans).ToArray();

            var prevBlock = this.BlockChain.Tail;
            var block = this.BlockChain.AddBlock(new Block
            {
                PreviousBlockHash = prevBlock.Hash,
                Time = DateTime.Now,
                Txs = allTrans,
            });
            return block;
        }

        private bool ValidateTx(Tx tran)
        {
            return !this.BlockChain.ContainTx(tran.Hash)
                && !this.BlockChain.ContainUsedTxs(tran.Inputs);
        }

        public void Dispose()
        {
            this.disposing = true;
            this.thWorker.Join();
        }
    }
}
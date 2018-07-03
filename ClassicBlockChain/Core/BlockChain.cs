using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using UChainDB.Example.Chain.Entity;

namespace UChainDB.Example.Chain.Core
{
    public class BlockChain
    {
        static BlockChain()
        {
            GenesisBlock = FindValidBlock(new Block
            {
                PreviousBlockHash = null,
                Time = new DateTime(2017, 6, 30, 9, 0, 0, DateTimeKind.Utc),
                Version = BlockChainVersion,
                Transactions = new Transaction[] { },
            }, Difficulty);
        }

        private const byte Difficulty = 2;
        private const byte BlockChainVersion = 1;

        public static readonly Block GenesisBlock;

        private readonly int MaxTransactionNumberInBlock = 1000;
        internal readonly int RewardOfBlock = 50;

        public BlockChain()
        {
            this.InitBlocks(GenesisBlock);
            this.Tail = GenesisBlock;
        }

        public ConcurrentDictionary<UInt256, Block> BlockDictionary { get; }
            = new ConcurrentDictionary<UInt256, Block>();
        internal ConcurrentDictionary<UInt256, (Block head, int index)> TransactionToBlockDictionary { get; }
            = new ConcurrentDictionary<UInt256, (Block, int)>();
        internal ConcurrentDictionary<UInt256, byte> UsedTransactionDictionary { get; }
            = new ConcurrentDictionary<UInt256, byte>();
        public int Height => this.BlockDictionary.Count;
        public Block Tail { get; set; }
        private ConcurrentQueue<Transaction> TransactionQueue { get; } = new ConcurrentQueue<Transaction>();

        internal bool ContainTransaction(UInt256 tranHash)
            => this.TransactionToBlockDictionary.ContainsKey(tranHash);

        internal void AddTransaction(Transaction transaction)
        {
            var (ret, error) = this.CheckQueueOfTransaction(transaction);
            if (!ret) throw new ArgumentException(error);

            this.TransactionQueue.Enqueue(transaction);
        }

        internal Block AddBlock(Block block)
        {
            block.Version = BlockChainVersion;
            block = FindValidBlock(block, Difficulty);

            this.InitBlocks(block);
            return block;
        }

        private (bool ret, string error) CheckQueueOfTransaction(Transaction transaction)
        {
            if (this.GetTransaction(transaction.Hash) != null)
            {
                return (false, "the transaction you submit already exist in chain.");
            }

            if (this.TransactionQueue.Any(_ => _.Hash == transaction.Hash))
            {
                return (false, "the transaction you submit already exist in queue.");
            }

            return (true, null);
        }

        internal bool ContainUsedTransactions(UInt256[] inputTransactions)
        {
            foreach (var tran in inputTransactions)
            {
                if (this.UsedTransactionDictionary.TryGetValue(tran, out var _))
                    return true;
            }

            return false;
        }

        internal void InitBlocks(params Block[] blocks)
        {
            foreach (var block in blocks)
            {
                this.BlockDictionary[block.Hash] = block;
            }

            this.MaintainBlockChain(blocks.Last());
        }

        private static Block FindValidBlock(Block originBlock, int difficulty)
        {
            var block = originBlock;
            block.Nonce = 0;
            while (true)
            {
                block.Nonce++;
                if (((byte[])block.Hash).Take(difficulty).All(_ => _ == 0))
                {
                    return block;
                }
            }
        }

        internal Transaction[] DequeueTransactions()
        {
            // don't allow duplicate transaction as they may 
            // just broadcast multiple time to ensure its execution
            var dict = new Dictionary<UInt256, Transaction>();
            while (this.TransactionQueue.TryDequeue(out var tran)
                && dict.Count < this.MaxTransactionNumberInBlock)
            {
                if (!dict.ContainsKey(tran.Hash))
                    dict.Add(tran.Hash, tran);
                else
                    dict[tran.Hash] = tran;
            }

            return dict.Select(_ => _.Value).ToArray();
        }

        internal Transaction GetTransaction(UInt256 hash)
        {
            if (!this.TransactionToBlockDictionary.TryGetValue(hash, out var tranref))
            {
                return null;
            }

            return this.BlockDictionary[tranref.head.Hash]?.Transactions[tranref.index];
        }

        private void MaintainBlockChain(Block newTail)
        {
            var prevTail = this.Tail;

            this.Tail = newTail;
            if (this.Tail != prevTail) this.MaintainChainDictionary(GenesisBlock, this.Tail);
        }

        private void MaintainChainDictionary(Block from, Block to)
        {
            var cursor = to;
            while (cursor.Hash != from.Hash)
            {
                var block = this.BlockDictionary[cursor.Hash];

                // update transaction dictionary
                if (block.Transactions != null)
                {
                    for (int i = 0; i < block.Transactions.Length; i++)
                    {
                        var tran = block.Transactions[i];
                        this.TransactionToBlockDictionary[tran.Hash] = (cursor, i);
                        foreach (var usedTx in tran.InputTransactions ?? new UInt256[] { })
                        {
                            this.UsedTransactionDictionary[usedTx] = 0;
                        }
                    }
                }

                cursor = this.BlockDictionary[cursor.PreviousBlockHash];
            }
        }
    }
}
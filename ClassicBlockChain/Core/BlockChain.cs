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
            GenesisBlockHead = FindValidBlock(new BlockHead
            {
                PreviousBlockHash = null,
                Time = new DateTime(2017, 6, 30, 9, 0, 0, DateTimeKind.Utc),
                Version = BlockChainVersion,
                MerkleRoot = UInt256.Zero,
            }, Difficulty);
            GenesisBlock = new Block
            {
                Head = GenesisBlockHead,
                Txs = new Transaction[] { },
            };
        }

        private const byte Difficulty = 2;
        private const byte BlockChainVersion = 1;

        public static readonly BlockHead GenesisBlockHead;
        public static readonly Block GenesisBlock;

        private readonly int MaxTxNumberInBlock = 1000;
        internal readonly int RewardOfBlock = 50;

        public BlockChain()
        {
            this.InitBlocks(GenesisBlockHead);
            this.Tail = GenesisBlockHead;
        }

        public ConcurrentDictionary<UInt256, BlockHead> BlockHeadDictionary { get; }
            = new ConcurrentDictionary<UInt256, BlockHead>();
        public ConcurrentDictionary<UInt256, Block> BlockDictionary { get; }
            = new ConcurrentDictionary<UInt256, Block>();
        internal ConcurrentDictionary<UInt256, (BlockHead head, int index)> TxToBlockDictionary { get; }
            = new ConcurrentDictionary<UInt256, (BlockHead, int)>();
        internal ConcurrentDictionary<(UInt256, int), byte> UsedTxDictionary { get; }
            = new ConcurrentDictionary<(UInt256, int), byte>();
        public int Height => this.BlockHeadDictionary.Count;
        public BlockHead Tail { get; set; }
        internal ConcurrentQueue<Transaction> TxQueue { get; } = new ConcurrentQueue<Transaction>();

        internal bool ContainTx(UInt256 txHash)
            => this.TxToBlockDictionary.ContainsKey(txHash);

        internal void AddTx(Transaction tx)
        {
            var (ret, error) = this.CheckQueueOfTx(tx);
            if (!ret) throw new ArgumentException(error);

            this.TxQueue.Enqueue(tx);
        }

        internal BlockHead AddBlock(Block block)
        {
            var blockhead = block.Head;

            blockhead.Version = BlockChainVersion;
            blockhead = FindValidBlock(blockhead, Difficulty);

            this.BlockDictionary[block.Hash] = block;
            this.InitBlocks(blockhead);
            return blockhead;
        }

        private (bool ret, string error) CheckQueueOfTx(Transaction tx)
        {
            if (this.GetTx(tx.Hash) != null)
            {
                return (false, "the transaction you submit already exist in chain.");
            }

            if (this.TxQueue.Any(_ => _.Hash == tx.Hash))
            {
                return (false, "the transaction you submit already exist in queue.");
            }

            return (true, null);
        }

        internal bool ContainUsedTxs(TxInput[] inputTxs)
        {
            foreach (var tx in inputTxs)
            {
                if (this.UsedTxDictionary.TryGetValue((tx.PrevTxHash, tx.PrevTxIndex), out var _))
                    return true;
            }

            return false;
        }

        internal void InitBlocks(params BlockHead[] blocks)
        {
            foreach (var block in blocks)
            {
                this.BlockHeadDictionary[block.Hash] = block;
            }

            this.MaintainBlockChain(blocks.Last());
        }

        private static BlockHead FindValidBlock(BlockHead originBlock, int difficulty)
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

        internal Transaction[] DequeueTxs()
        {
            // don't allow duplicate transaction as they may 
            // just broadcast multiple time to ensure its execution
            var dict = new Dictionary<UInt256, Transaction>();
            while (this.TxQueue.TryDequeue(out var tx)
                && dict.Count < this.MaxTxNumberInBlock)
            {
                if (!dict.ContainsKey(tx.Hash))
                    dict.Add(tx.Hash, tx);
                else
                    dict[tx.Hash] = tx;
            }

            return dict.Select(_ => _.Value).ToArray();
        }

        internal Transaction GetTx(UInt256 hash)
        {
            if (!this.TxToBlockDictionary.TryGetValue(hash, out var txRef))
            {
                return null;
            }

            return this.BlockDictionary[txRef.head.Hash]?.Txs[txRef.index];
        }

        internal Block GetBlock(UInt256 hash)
        {
            return this.BlockDictionary[hash];
        }

        private void MaintainBlockChain(BlockHead newTail)
        {
            var prevTail = this.Tail;

            this.Tail = newTail;
            if (this.Tail != prevTail) this.MaintainChainDictionary(GenesisBlockHead, this.Tail);
        }

        private void MaintainChainDictionary(BlockHead from, BlockHead to)
        {
            var cursor = to;
            while (cursor.Hash != from.Hash)
            {
                var block = this.BlockDictionary[cursor.Hash];

                // update transaction dictionary
                if (block.Txs != null)
                {
                    for (int i = 0; i < block.Txs.Length; i++)
                    {
                        var tx = block.Txs[i];
                        this.TxToBlockDictionary[tx.Hash] = (cursor, i);
                        foreach (var usedTx in tx.InputTxs ?? new TxInput[] { })
                        {
                            this.UsedTxDictionary[(usedTx.PrevTxHash, usedTx.PrevTxIndex)] = 0;
                        }
                    }
                }

                cursor = this.BlockHeadDictionary[cursor.PreviousBlockHash];
            }
        }
    }
}
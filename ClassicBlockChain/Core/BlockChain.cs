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
                Txs = new Tx[] { },
            }, Difficulty);
        }

        private const byte Difficulty = 2;
        private const byte BlockChainVersion = 1;

        public static readonly Block GenesisBlock;

        private readonly int MaxTxNumberInBlock = 1000;
        internal readonly int RewardOfBlock = 50;

        public BlockChain()
        {
            this.InitBlocks(GenesisBlock);
            this.Tail = GenesisBlock;
        }

        public ConcurrentDictionary<UInt256, Block> BlockDictionary { get; }
            = new ConcurrentDictionary<UInt256, Block>();
        internal ConcurrentDictionary<UInt256, (Block head, int index)> TxToBlockDictionary { get; }
            = new ConcurrentDictionary<UInt256, (Block, int)>();
        internal ConcurrentDictionary<UInt256, byte> UsedTxDictionary { get; }
            = new ConcurrentDictionary<UInt256, byte>();
        public int Height => this.BlockDictionary.Count;
        public Block Tail { get; set; }
        private ConcurrentQueue<Tx> TxQueue { get; } = new ConcurrentQueue<Tx>();

        internal bool ContainTx(UInt256 tranHash)
            => this.TxToBlockDictionary.ContainsKey(tranHash);

        internal void AddTx(Tx tx)
        {
            var (ret, error) = this.CheckQueueOfTx(tx);
            if (!ret) throw new ArgumentException(error);

            this.TxQueue.Enqueue(tx);
        }

        internal Block AddBlock(Block block)
        {
            block.Version = BlockChainVersion;
            block = FindValidBlock(block, Difficulty);

            this.InitBlocks(block);
            return block;
        }

        private (bool ret, string error) CheckQueueOfTx(Tx tx)
        {
            if (this.GetTx(tx.Hash) != null)
            {
                return (false, "the tx you submit already exist in chain.");
            }

            if (this.TxQueue.Any(_ => _.Hash == tx.Hash))
            {
                return (false, "the tx you submit already exist in queue.");
            }

            return (true, null);
        }

        internal bool ContainUsedTxs(UInt256[] inputTxs)
        {
            foreach (var tran in inputTxs)
            {
                if (this.UsedTxDictionary.TryGetValue(tran, out var _))
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

        internal Tx[] DequeueTxs()
        {
            // don't allow duplicate tx as they may 
            // just broadcast multiple time to ensure its execution
            var dict = new Dictionary<UInt256, Tx>();
            while (this.TxQueue.TryDequeue(out var tran)
                && dict.Count < this.MaxTxNumberInBlock)
            {
                if (!dict.ContainsKey(tran.Hash))
                    dict.Add(tran.Hash, tran);
                else
                    dict[tran.Hash] = tran;
            }

            return dict.Select(_ => _.Value).ToArray();
        }

        internal Tx GetTx(UInt256 hash)
        {
            if (!this.TxToBlockDictionary.TryGetValue(hash, out var tranref))
            {
                return null;
            }

            return this.BlockDictionary[tranref.head.Hash]?.Txs[tranref.index];
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

                // update tx dictionary
                if (block.Txs != null)
                {
                    for (int i = 0; i < block.Txs.Length; i++)
                    {
                        var tran = block.Txs[i];
                        this.TxToBlockDictionary[tran.Hash] = (cursor, i);
                        foreach (var usedTx in tran.Inputs ?? new UInt256[] { })
                        {
                            this.UsedTxDictionary[usedTx] = 0;
                        }
                    }
                }

                cursor = this.BlockDictionary[cursor.PreviousBlockHash];
            }
        }
    }
}
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using UChainDB.Example.Chain.Entity;

namespace UChainDB.Example.Chain.Core
{
    public class BlockChain : IDisposable
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

        private const byte Difficulty = 5;
        private const byte BlockChainVersion = 1;

        public static readonly BlockHead GenesisBlockHead;
        public static readonly Block GenesisBlock;

        private readonly int MaxTxNumberInBlock = 1000;
        internal readonly int RewardOfBlock = 50;
        private bool cancelSearchNonce = false;

        public BlockChain()
        {
            this.CacheBlock(GenesisBlock);
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
        public int Height => this.ReverseIterateBlockHeaders(GenesisBlockHead.Hash, this.Tail.Hash).Count();
        public BlockHead Tail { get; set; }
        internal ConcurrentQueue<Transaction> TxQueue { get; } = new ConcurrentQueue<Transaction>();

        internal bool ContainTx(UInt256 txHash)
            => this.TxToBlockDictionary.ContainsKey(txHash);

        internal void SyncTx(Transaction tx)
        {
            // no check for sync as its dependences may not come at this time
            this.TxQueue.Enqueue(tx);
        }

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
            this.cancelSearchNonce = false;
            blockhead = FindValidBlock(blockhead, Difficulty, ref this.cancelSearchNonce);
            if (blockhead == null) return null;

            CacheBlock(block);
            this.MaintainBlockChain(blockhead);
            return blockhead;
        }

        internal void AddSyncBlock(Block block)
        {
            CacheBlock(block);
            // just cache block if prevous block not exist
            if (!this.BlockHeadDictionary.ContainsKey(block.Head.PreviousBlockHash)) return;
            if (this.TryMoveSyncTail(block.Head))
            {
                this.cancelSearchNonce = true;
            }
        }

        private void CacheBlock(Block block)
        {
            this.BlockDictionary[block.Hash] = block;
            this.InitBlocks(block.Head);
        }

        private bool TryMoveSyncTail(BlockHead newTail)
        {
            var listnow = this.ReverseIterateBlockHeaders(GenesisBlockHead.Hash, this.Tail.Hash).ToArray();
            var listnew = this.ReverseIterateBlockHeaders(GenesisBlockHead.Hash, newTail.Hash).ToArray();
            // broken chain should not count
            if (listnew.LastOrDefault()?.Hash != GenesisBlockHead.Hash) return false;
            var cnow = listnow.Length;
            var cnew = listnew.Length;
            if (cnew > cnow)
            {
                MaintainBlockChain(newTail);
                return true;
            }

            return false;
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
        }

        private static BlockHead FindValidBlock(BlockHead originBlock, int difficulty)
        {
            var flag = false;
            return FindValidBlock(originBlock, difficulty, ref flag);
        }

        private static BlockHead FindValidBlock(BlockHead originBlock, int difficulty, ref bool stopFinding)
        {
            var block = originBlock;
            block.Nonce = 0;
            while (!stopFinding)
            {
                block.Nonce++;
                //if (((byte[])block.Hash).Take(difficulty).All(_ => _ == 0))
                if ((BitConverter.ToString((byte[])block.Hash).Replace("-", "")).Take(difficulty).All(_ => _ == '0'))
                {
                    return block;
                }
            }

            return null;
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

        public Block GetBlock(UInt256 hash)
        {
            return this.BlockDictionary[hash];
        }

        public IEnumerable<Block> GetBlocks(UInt256 startingHash)
        {
            return this.GetBlockHeaders(startingHash)
                .Select(_ => this.GetBlock(_.Hash));
        }

        public IEnumerable<BlockHead> GetBlockHeaders(UInt256 startingHash)
        {
            return this.ReverseIterateBlockHeaders(startingHash, this.Tail.Hash)
                .Reverse();
        }

        internal IEnumerable<BlockHead> ReverseIterateBlockHeaders(UInt256 from, UInt256 to)
        {
            var cursor = this.BlockHeadDictionary[to];
            while (cursor.Hash != from)
            {
                yield return cursor;
                if (!this.BlockHeadDictionary.TryGetValue(cursor.PreviousBlockHash, out cursor))
                    yield break;
            }

            yield return cursor;
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

        // ref https://en.bitcoin.it/wiki/Protocol_documentation#getblocks
        private static long[] GetBlockLocatorHashes(long height)
        {
            var indexes = new List<long>();
            var step = 1;
            var i = height;
            do
            {
                if (indexes.Count >= 10) step *= 2;
                indexes.Add(i);
                i -= step;
            } while (i > 0);

            indexes.Add(0);
            return indexes.ToArray();
        }

        public void Dispose()
        {
            this.cancelSearchNonce = true;
        }
    }
}
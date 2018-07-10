using System;
using System.Diagnostics;
using System.Linq;

namespace UChainDB.Example.Chain.Entity
{
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ", nq}")]
    public class Block : IHashObject
    {
        public Block()
        {
        }

        public Block(BlockHead head, Transaction[] txs)
        {
            this.Head = head;
            this.Txs = txs;

            // verification
            if (this.Head.MerkleRoot != null)
            {
                var txHashList = this.Txs.Select(_ => _.Hash).ToArray();
                var txRoot = MerkleTree.GetMerkleRoot(txHashList);
                if (txRoot != this.Head.MerkleRoot)
                    throw new ArgumentException($"hash of transaction list [{txRoot}] not equal to the one in header [{this.Head.MerkleRoot}]");
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public Transaction[] Txs { get; set; } = new Transaction[] { };

        public BlockHead Head { get; set; }

        public UInt256 Hash { get => this.Head?.Hash; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        protected string DebuggerDisplay =>
            $"{this.Head} [{this.Txs.Length} txs]\r\n" +
            $"{string.Join(Environment.NewLine, this.Txs?.Select(_ => _.ToString()) ?? new string[] { })}";

        public override string ToString() => this.DebuggerDisplay;
    }
}
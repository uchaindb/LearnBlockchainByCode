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

        public Block(BlockHead head, Transaction[] transactions)
        {
            this.Head = head;
            this.Transactions = transactions;

            // verification
            if (this.Head.MerkleRoot != null)
            {
                var tranHashList = this.Transactions.Select(_ => _.Hash).ToArray();
                var tranRoot = MerkleTree.GetMerkleRoot(tranHashList);
                if (tranRoot != this.Head.MerkleRoot)
                    throw new ArgumentException($"hash of transaction list [{tranRoot}] not equal to the one in header [{this.Head.MerkleRoot}]");
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public Transaction[] Transactions { get; set; } = new Transaction[] { };

        public BlockHead Head { get; set; }

        public UInt256 Hash { get => this.Head?.Hash; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        protected string DebuggerDisplay =>
            $"{this.Head} [{this.Transactions.Length} txs]\r\n" +
            $"{string.Join(Environment.NewLine, this.Transactions?.Select(_ => _.ToString()) ?? new string[] { })}";

        public override string ToString() => this.DebuggerDisplay;
    }
}
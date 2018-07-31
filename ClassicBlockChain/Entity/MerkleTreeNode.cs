namespace UChainDB.Example.Chain.Entity
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ", nq}")]
    internal class MerkleTreeNode
    {
        public MerkleTreeNode(MerkleTreeNode left, MerkleTreeNode right, MerkleTreeNode parent = null)
        {
            this.Left = left;
            this.Left.Parent = this;
            this.Right = right;
            if (this.Right != null) this.Right.Parent = this;
            this.Parent = parent;
            this.Hash = new Hash(new byte[][] { left.Hash, (right ?? left).Hash });
        }

        public MerkleTreeNode(UInt256 hash)
        {
            this.Hash = hash;
        }

        public UInt256 Hash { get; }
        public MerkleTreeNode Left { get; }
        public MerkleTreeNode Right { get; }

        public MerkleTreeNode Parent { get; private set; }
        public bool IsMarked { get; internal set; }
        public bool IsLeaf => this.Left == null && this.Right == null;

        public IEnumerable<MerkleTreeNode> Ancestors()
        {
            var n = Parent;
            while (n != null)
            {
                yield return n;
                n = n.Parent;
            }
        }

        public IEnumerable<MerkleTreeNode> EnumerateDescendants()
        {
            IEnumerable<MerkleTreeNode> result = new MerkleTreeNode[] { this };
            if (Right != null)
                result = Right.EnumerateDescendants().Concat(result);
            if (Left != null)
                result = Left.EnumerateDescendants().Concat(result);
            return result;
        }

        public IEnumerable<MerkleTreeNode> GetLeafs()
        {
            return EnumerateDescendants().Where(l => l.IsLeaf);
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        protected virtual string DebuggerDisplay => $"{Hash} ({Left?.Hash}, {Right?.Hash}) {(IsLeaf ? "L" : "")}{(IsMarked ? "M" : "")}";
    }
}
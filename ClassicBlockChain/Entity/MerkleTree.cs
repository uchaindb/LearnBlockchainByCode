namespace UChainDB.Example.Chain.Entity
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class MerkleTree
    {
        private MerkleTreeNode root;

        public MerkleTree(UInt256[] hashes)
        {
            if (hashes == null) throw new ArgumentNullException(nameof(hashes));
            if (hashes.Length == 0) throw new ArgumentOutOfRangeException("length greater than 0 is required", nameof(hashes));

            this.root = BuildTree(hashes.Select(p => new MerkleTreeNode(p)).ToArray());
        }

        public static UInt256 GetMerkleRoot(UInt256[] hashes)
        {
            var tree = new MerkleTree(hashes);
            return tree.root.Hash;
        }

        public UInt256[] ToArray()
        {
            return Traverse(root).Select(_ => _.Hash).ToArray();
        }

        /// <summary>
        /// when leaves = 3
        /// 3rd iteration:        x
        ///                      / \
        ///                     /   \
        ///                    /     \
        /// 2nd iteration:    a       b
        ///                  / \     / \
        /// 1st iteration:  1   2   3   3
        /// </summary>
        /// <param name="leaves">nodes</param>
        /// <returns>tree full with nodes</returns>
        private static MerkleTreeNode BuildTree(MerkleTreeNode[] leaves)
        {
            if (leaves.Length == 1) return leaves[0];
            var parentNumber = (leaves.Length + 1) / 2;
            var parents = new MerkleTreeNode[parentNumber];
            for (int i = 0; i < parents.Length; i++)
            {
                var left = leaves[i * 2];
                left.Parent = parents[i];

                var right = left;
                if (i * 2 + 1 < leaves.Length)
                {
                    right = leaves[i * 2 + 1];
                    right.Parent = parents[i];
                }

                parents[i] = new MerkleTreeNode(left, right);
            }

            return BuildTree(parents);
        }

        // DFS traverse
        private static IEnumerable<MerkleTreeNode> Traverse(MerkleTreeNode node)
        {
            if (node.Left == null)
            {
                // both child should appear null
                yield return node;
            }
            else
            {
                yield return node.Left;
                yield return node.Right;
            }
        }
    }

    internal class MerkleTreeNode
    {
        public MerkleTreeNode(MerkleTreeNode left, MerkleTreeNode right, MerkleTreeNode parent = null)
        {
            this.Left = left;
            this.Right = right;
            this.Parent = parent;
            this.Hash = new Hash(new byte[][] { this.Left.Hash, this.Right.Hash });
        }

        public MerkleTreeNode(UInt256 hash)
        {
            this.Hash = hash;
        }

        public UInt256 Hash { get; }
        public MerkleTreeNode Parent { get; internal set; }
        public MerkleTreeNode Left { get; }
        public MerkleTreeNode Right { get; }
    }
}
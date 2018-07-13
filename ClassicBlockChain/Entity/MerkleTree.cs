namespace UChainDB.Example.Chain.Entity
{
    using System;
    using System.Collections;
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

        public UInt256 RootHash => this.root.Hash;

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

                MerkleTreeNode right = null;
                if (i * 2 + 1 < leaves.Length)
                {
                    right = leaves[i * 2 + 1];
                }

                parents[i] = new MerkleTreeNode(left, right);

                if (right != null) right.Parent = parents[i];
                left.Parent = parents[i];
            }

            return BuildTree(parents);
        }

        public (UInt256[] hashes, BitArray flags) Trim(Predicate<UInt256> filter)
        {
            var flags = new List<bool>();
            var hashes = new List<UInt256>();
            Trim(root, hashes, flags, filter);

            return (hashes.ToArray(), new BitArray(flags.ToArray()));
        }

        private static void Trim(MerkleTreeNode root, List<UInt256> hashes, List<bool> flags, Predicate<UInt256> filter)
        {
            foreach (var leaf in root.GetLeafs().Where(_ => filter(_.Hash)))
            {
                MarkToTop(leaf, true);
            }

            TrimCore(root, hashes, flags);
        }

        private static void MarkToTop(MerkleTreeNode leaf, bool value)
        {
            leaf.IsMarked = value;
            foreach (var ancestor in leaf.Ancestors())
            {
                ancestor.IsMarked = value;
            }
        }

        private static void TrimCore(MerkleTreeNode node, List<UInt256> hashes, List<bool> flags)
        {
            if (node == null)
                return;
            flags.Add(node.IsMarked);
            if (node.IsLeaf || !node.IsMarked)
                hashes.Add(node.Hash);

            if (node.IsMarked)
            {
                TrimCore(node.Left, hashes, flags);
                TrimCore(node.Right, hashes, flags);
            }
        }

        public static UInt256 GetPartialTreeRootHash(int txNumber, UInt256[] hashes, BitArray flags)
        {
            var height = (int)Math.Ceiling(Math.Log(txNumber, 2)) + 1;
            var tree = BuildPartialTree(new Queue<UInt256>(hashes), height, new Queue<bool>(flags.OfType<bool>()));
            return tree.Hash;
        }

        private static MerkleTreeNode BuildPartialTree(Queue<UInt256> hashes, int height, Queue<bool> flags)
        {
            if (height == 0) return null;
            var flag = flags.Dequeue();
            if (flag)
            {
                if (height == 1)
                {
                    // For TXID, Use the next hash as this node’s TXID, and mark this transaction as matching the filter.	
                    var leaf = new MerkleTreeNode(hashes.Dequeue())
                    {
                        IsMarked = true
                    };
                    return leaf;
                }
                else
                {
                    // For Non-TXID, The hash needs to be computed.
                    // Process the left child node to get its hash;
                    // process the right child node to get its hash; 
                    // then concatenate the two hashes as 64 raw bytes and hash them to get this node’s hash.
                    var left = BuildPartialTree(hashes, height - 1, flags);
                    var right = BuildPartialTree(hashes, height - 1, flags);
                    var parent = new MerkleTreeNode(left, right);
                    return parent;
                }
            }
            else
            {
                if (height == 1)
                {
                    // For TXID, Use the next hash as this node’s TXID, but this transaction didn’t match the filter.
                    var leaf = new MerkleTreeNode(hashes.Dequeue());
                    return leaf;

                }
                else
                {
                    // For Non-TXID, Use the next hash as this node’s hash. Don’t process any descendant nodes.
                    var leaf = new MerkleTreeNode(hashes.Dequeue());
                    return leaf;
                }
            }
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
}
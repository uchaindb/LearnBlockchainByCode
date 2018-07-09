using System;
using System.Diagnostics;
using UChainDB.Example.Chain.Utility;

namespace UChainDB.Example.Chain.Entity
{
    public class BlockHead : HashBase
    {
        private byte version;
        private UInt256 previousBlockHash;
        private UInt256 merkleRoot ;
        private DateTime time;
        private uint nonce;

        public byte Version
        {
            get => this.version;
            set => this.SetPropertyField(ref this.version, value);
        }

        public UInt256 PreviousBlockHash
        {
            get => this.previousBlockHash;
            set => this.SetPropertyField(ref this.previousBlockHash, value);
        }

        public DateTime Time
        {
            get => this.time;
            set => this.SetPropertyField(ref this.time, value);
        }

        public UInt256 MerkleRoot
        {
            get => this.merkleRoot;
            set => this.SetPropertyField(ref this.merkleRoot, value);
        }

        public uint Nonce
        {
            get => this.nonce;
            set => this.SetPropertyField(ref this.nonce, value);
        }

        public override string ToString()
        {
            return this.DebuggerDisplay;
        }

        protected internal override string HashContent => $"{this.Version}" +
            $"|{this.Nonce}" +
            $"|{this.PreviousBlockHash}" +
            $"|{this.MerkleRoot}" +
            $"|{this.Time.ToUnixTimestamp()}";

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        protected override string DebuggerDisplay => $"{this.Hash.ToShort()}" +
            $": (" +
            $"N: {this.Nonce,8}" +
            $", " +
            $"MR: {this.MerkleRoot.ToShort()}" +
            $")";
    }
}
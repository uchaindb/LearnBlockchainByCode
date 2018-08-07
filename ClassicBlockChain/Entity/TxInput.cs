using System;
using UChainDB.Example.Chain.SmartContracts;
using UChainDB.Example.Chain.Utility;

namespace UChainDB.Example.Chain.Entity
{
    public class TxInput
    {
        public UInt256 PrevTxHash { get; set; }
        public int PrevTxIndex { get; set; }
        public UnlockScripts UnlockScripts { get; set; }

        internal string HashContent => this.LockHashContent
            + $"|{this.UnlockScripts}"
            ;

        internal string LockHashContent => $"{this.PrevTxHash.ToHex()}"
            + $"|{this.PrevTxIndex}"
            ;

        public override string ToString() => $"{this.PrevTxHash.ToShort()}"
            + $"[{this.PrevTxIndex}]"
            + $": {(this.UnlockScripts?.ToString().Substring(0, 12))}"
            ;
    }
}
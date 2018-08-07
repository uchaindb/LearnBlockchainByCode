using System;
using UChainDB.Example.Chain.SmartContracts;

namespace UChainDB.Example.Chain.Entity
{
    public class TxOutput
    {
        public LockScripts LockScripts { get; set; }
        public int Value { get; set; }

        internal string HashContent => this.LockHashContent
            + $"|{this.LockScripts}"
            ;

        internal string LockHashContent => $"{this.Value}"
            ;

        public override string ToString() => $"{this.Value}"
            + $": {this.LockScripts?.ToString().Substring(0, 12)}"
            ;
    }
}
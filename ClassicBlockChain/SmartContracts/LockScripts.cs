using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace UChainDB.Example.Chain.SmartContracts
{
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ", nq}")]
    public class LockScripts : ReadOnlyCollection<ScriptToken>
    {
        public LockScripts(IList<ScriptToken> list)
            : base(list)
        {
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        protected string DebuggerDisplay => string.Join(Environment.NewLine, this.Select(_ => _.ToString()) ?? new string[] { });

        public static implicit operator ScriptToken[] (LockScripts ls)
            => ls.ToArray();

        public static explicit operator LockScripts(ScriptToken[] s)
            => new LockScripts(s);

        public override string ToString() =>
            string.Join("\n", this.Select(_ => _.ToString()) ?? new string[] { });
    }
}
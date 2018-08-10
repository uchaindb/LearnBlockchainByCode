using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace UChainDB.Example.Chain.SmartContracts
{
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ", nq}")]
    public class UnlockScripts : ReadOnlyCollection<ScriptToken>
    {
        public UnlockScripts(IList<ScriptToken> list)
            : base(list)
        {
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        protected string DebuggerDisplay => string.Join(Environment.NewLine, this.Select(_ => _.ToString()) ?? new string[] { });

        public static WholeScripts operator +(UnlockScripts us, LockScripts ls)
            => new WholeScripts((us ?? new UnlockScripts(new ScriptToken[] { })).Concat(ls).ToList());

        public static implicit operator ScriptToken[] (UnlockScripts s)
            => s.ToArray();

        public static explicit operator UnlockScripts(ScriptToken[] s)
            => new UnlockScripts(s);

        public override string ToString() =>
            string.Join("\n", this.Select(_ => _.ToString()) ?? new string[] { });
    }
}
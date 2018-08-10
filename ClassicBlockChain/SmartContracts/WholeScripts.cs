using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace UChainDB.Example.Chain.SmartContracts
{
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ", nq}")]
    public class WholeScripts : ReadOnlyCollection<ScriptToken>
    {
        public WholeScripts(IList<ScriptToken> list)
            : base(list)
        {
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        protected string DebuggerDisplay => string.Join(Environment.NewLine, this.Select(_ => _.ToString()) ?? new string[] { });

        public static implicit operator ScriptToken[] (WholeScripts s)
            => s.ToArray();
    }
}
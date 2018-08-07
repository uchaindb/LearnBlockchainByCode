using System;
using System.Diagnostics;
using System.Linq;

namespace UChainDB.Example.Chain.SmartContracts
{
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ", nq}")]
    public class ScriptToken 
    {
        public const string TOKEN_PREFIX = "OC_";
        public bool IsOpCode { get => this.OpCode != OpCode.Object; }
        public OpCode OpCode { get; set; } = OpCode.Object;
        public string Object { get; set; }

        public ScriptToken(object obj)
        {
            if (obj is OpCode opcode)
            {
                this.OpCode = opcode;
            }
            else if (obj is string str)
            {
                if (str.StartsWith(TOKEN_PREFIX))
                {
                    this.OpCode = (OpCode)Enum.Parse(typeof(OpCode), str.Remove(0, TOKEN_PREFIX.Length));
                }
                else
                {
                    this.Object = str;
                }
            }
            else
            {
                this.Object = obj.ToString();
            }
        }

        public ScriptToken()
        {
        }

        public string GetValue()
        {
            return this.Object;
        }

        public T GetValue<T>()
        {
            return (T)Convert.ChangeType(this.Object, typeof(T));
        }

        public static ScriptToken CreateToken(object obj)
        {
            return new ScriptToken(obj);
        }

        public override string ToString() => this.IsOpCode
            ? $"{TOKEN_PREFIX}{this.OpCode}"
            : $"{this.Object}";

        public ScriptToken Clone()
        {
            return new ScriptToken(this.IsOpCode ? (object)this.OpCode : this.Object);
        }

        public override bool Equals(object obj)
        {
            var item = obj as ScriptToken;

            if (item == null)
            {
                return false;
            }

            return this.IsOpCode == item.IsOpCode
                && this.OpCode == item.OpCode
                && this.Object == item.Object;
        }

        public override int GetHashCode()
        {
            return this.IsOpCode ? this.OpCode.GetHashCode() : this.Object.GetHashCode();
        }

        public static ScriptToken[] Parse(string code)
        {
            var lines = code.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
            return lines
                .Select(_=>_.Trim())
                .Select(_ => new ScriptToken(_))
                .ToArray();
        }

        public static bool TryParse(string code, out ScriptToken[] lockScripts)
        {
            try
            {
                lockScripts = Parse(code);
            }
            catch (ArgumentException)
            {
                lockScripts = null;
                return false;
            }

            return true;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        protected string DebuggerDisplay => this.IsOpCode
            ? $"[OpCode]{this.OpCode}"
            : $"{this.Object}";
    }
}
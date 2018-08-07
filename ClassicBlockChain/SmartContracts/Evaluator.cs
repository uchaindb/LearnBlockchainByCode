using System;
using System.Collections.Generic;
using UChainDB.Example.Chain.Entity;
using UChainDB.Example.Chain.Utility;

namespace UChainDB.Example.Chain.SmartContracts
{
    public class Evaluator
    {
        public static T Evaluate<T>(ScriptToken[] tokens, Transaction transaction, ISignAlgorithm algorithm)
        {
            var ret = Evaluate(tokens, transaction, algorithm);
            if (typeof(T) == typeof(Boolean))
            {
                var s = ret.ToString().ToLower();
                return (T)(object)(s == "true");
            }

            return (T)Convert.ChangeType(ret, typeof(T));
        }

        public static object Evaluate(ScriptToken[] tokens, Transaction transaction, ISignAlgorithm algorithm)
        {
            if (tokens == null || tokens.Length == 0)
                return true;

            var stack = new Stack<ScriptToken>();

            foreach (var token in tokens)
            {
                if (!token.IsOpCode)
                {
                    stack.Push(token);
                    continue;
                }

                switch (token.OpCode)
                {
                    case OpCode.CheckSignature:
                        {
                            if (!stack.CanPop()) return false;
                            var pubKey = PublicKey.ParseBase58(stack.Pop().GetValue());

                            if (!stack.CanPop()) return false;
                            var sig = Signature.ParseBase58(stack.Pop().GetValue());

                            var ret = algorithm.Verify(new[] { (byte[])transaction.GetLockHash() }, pubKey, sig);
                            stack.Push(ScriptToken.CreateToken(ret));
                            break;
                        }
                    default:
                        Console.WriteLine($"unexpected opcode [{token.OpCode}]");
                        break;
                }
            }

            if (!stack.CanPop()) return false;
            return stack.Pop().GetValue();
        }
    }
}
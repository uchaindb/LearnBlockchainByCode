using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UChainDB.Example.Chain.Entity;
using UChainDB.Example.Chain.Utility;

namespace UChainDB.Example.Chain.SmartContracts
{
    public static class TokenScriptExtension
    {
        public static bool TryExecuteAsync(this WholeScripts scripts, Transaction trans, ISignAlgorithm signAlgorithm = null)
        {
            signAlgorithm = signAlgorithm ?? new ECDsaSignAlgorithm();
            var eval = Evaluator.Evaluate<bool>(scripts, trans, signAlgorithm);
            return eval;
        }

        public static bool CanPop(this Stack<ScriptToken> stack)
        {
            return stack.Count > 0;
        }

        /*
         * [sig, pubkey] -> Check Signature
         */

        public static UnlockScripts ProduceSingleUnlockScript(this Signature sig) => (UnlockScripts)new[] {
            ScriptToken.CreateToken(sig.ToBase58()),
        };

        public static LockScripts ProduceSingleLockScript(this PublicKey pubKey) => (LockScripts)new[] {
            ScriptToken.CreateToken(pubKey.ToBase58()),
            ScriptToken.CreateToken(OpCode.CheckSignature),
        };

        /*
         * [sig, | pubkey1, pubkey2, ..., pubkeyN, N] -> Check One Of Multiple Signatures
         */

        public static UnlockScripts ProduceMultiUnlockScript(this Signature sig) => (UnlockScripts)new[] {
            ScriptToken.CreateToken(sig.ToBase58()),
        };

        public static LockScripts ProduceMultiLockScript(this IEnumerable<PublicKey> pubKeyList) => (LockScripts)pubKeyList
            .Select(_ => ScriptToken.CreateToken(_.ToBase58()))
            .Concat(new[] { ScriptToken.CreateToken(pubKeyList.Count()) })
            .Concat(new[] { ScriptToken.CreateToken(OpCode.CheckOneOfMultiSignature) })
            .ToArray();

        public static bool CanUnlock(this Transaction tran, TxInput input, TxOutput output)
        {
            try
            {
                var scripts = input.UnlockScripts + output.LockScripts;
                var result = scripts.TryExecuteAsync(tran);
                if (!result) return false;
            }
            catch (AggregateException ex)
            {
                Debug.WriteLine("execute smart contract failed with aggregate exception: " + ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("execute smart contract failed: " + ex.Message);
                return false;
            }

            return true;
        }
    }
}
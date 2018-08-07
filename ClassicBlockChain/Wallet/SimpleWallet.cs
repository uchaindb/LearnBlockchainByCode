using System.Collections.Generic;
using UChainDB.Example.Chain.Entity;
using UChainDB.Example.Chain.SmartContracts;

namespace UChainDB.Example.Chain.Wallet
{
    public class SimpleWallet : BaseWallet
    {
        public SimpleWallet(string name)
            : base(name)
        {
        }

        protected override PrivateKey FindPrivateKey(LockScripts lockScripts)
        {
            if (!lockScripts.Contains(new ScriptToken(this.PublicKey.ToBase58())))
                throw new KeyNotFoundException("cannot find corresponding public key");
            return this.PrivateKey;
        }

        protected override bool ContainPubKey(LockScripts lockScripts)
        {
            return lockScripts.Contains(new ScriptToken(this.PublicKey.ToBase58()));
        }
    }
}
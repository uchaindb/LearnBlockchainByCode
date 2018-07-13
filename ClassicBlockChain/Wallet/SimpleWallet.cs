using System.Collections.Generic;
using UChainDB.Example.Chain.Entity;

namespace UChainDB.Example.Chain.Wallet
{
    public class SimpleWallet : BaseWallet
    {
        public SimpleWallet(string name)
            : base(name)
        {
        }

        protected override PrivateKey FindPrivateKey(PublicKey publicKey)
        {
            if (publicKey != this.PublicKey)
                throw new KeyNotFoundException("corresponding public key is not right");
            return this.PrivateKey;
        }

        protected override bool ContainPubKey(PublicKey publicKey)
        {
            return publicKey == this.PublicKey;
        }
    }
}
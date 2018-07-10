using System.Collections.Generic;
using UChainDB.Example.Chain.Entity;

namespace UChainDB.Example.Chain
{
    public class DeterministicWallet : BaseWallet
    {
        private List<PublicKey> usedPublicKeys = new List<PublicKey>();

        private List<PrivateKey> usedPrivateKeys = new List<PrivateKey>();

        public DeterministicWallet(string name)
                            : base(name)
        {
        }

        protected override void AfterKeyPairGenerated()
        {
            this.usedPrivateKeys.Add(this.PrivateKey);
            this.usedPublicKeys.Add(this.PublicKey);
        }

        protected override PrivateKey FindPrivateKey(PublicKey publicKey)
        {
            var idx = usedPublicKeys.FindIndex(_ => _ == publicKey);
            if (idx == -1)
                throw new KeyNotFoundException("cannot find corresponding public key");
            return usedPrivateKeys[idx];
        }
    }
}
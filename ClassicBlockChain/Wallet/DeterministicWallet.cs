using System.Collections.Generic;
using System.Linq;
using UChainDB.Example.Chain.Entity;
using UChainDB.Example.Chain.SmartContracts;

namespace UChainDB.Example.Chain.Wallet
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

        protected override PrivateKey FindPrivateKey(LockScripts lockScripts)
        {
            var idx = this.usedPublicKeys.FindIndex(_ => lockScripts.Contains(new ScriptToken(_.ToBase58())));
            if (idx == -1)
                throw new KeyNotFoundException("cannot find corresponding public key");
            return this.usedPrivateKeys[idx];
        }

        protected override bool ContainPubKey(LockScripts lockScripts)
        {
            return this.usedPublicKeys.Any(_ => lockScripts.Contains(new ScriptToken(_.ToBase58())));
        }
    }
}
using System.Collections.Generic;
using UChainDB.BingChain.Engine.Cryptography;
using UChainDB.Example.Chain.Entity;

namespace UChainDB.Example.Chain.Utility
{
    public class ECDsaSignAlgorithm : ISignAlgorithm
    {
        internal readonly Secp256k1 signAlgo = new Secp256k1();

        public PrivateKey GenerateRandomPrivateKey(long random = 0)
        {
            var privateKey = new byte[32];
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                rng.GetBytes(privateKey);
            }

            return new PrivateKey(privateKey);
        }

        public PublicKey GetPublicKey(PrivateKey privateKey)
        {
            var publicKey = this.signAlgo.SelectedCurve.G * privateKey;
            return new PublicKey(publicKey.EncodePoint(true));
        }

        public Signature Sign(IEnumerable<byte[]> data, PrivateKey privateKey)
        {
            return new Signature(this.signAlgo.Sign(privateKey, data));
        }

        public bool Verify(IEnumerable<byte[]> data, PublicKey publicKey, Signature sig)
        {
            return this.signAlgo.Verify(publicKey, sig, data);
        }
    }
}
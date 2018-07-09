using System.Collections.Generic;
using UChainDB.Example.Chain.Entity;

namespace UChainDB.Example.Chain.Utility
{
    public interface ISignAlgorithm
    {
        Signature Sign(IEnumerable<byte[]> data, PrivateKey privateKey);
        bool Verify(IEnumerable<byte[]> data, PublicKey publicKey, Signature sig);
        PublicKey GetPublicKey(PrivateKey privateKey);
        PrivateKey GenerateRandomPrivateKey(long random =0);
    }
}
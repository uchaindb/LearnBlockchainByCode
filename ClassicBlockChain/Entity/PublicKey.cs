using UChainDB.Example.Chain.Utility;

namespace UChainDB.Example.Chain.Entity
{
    public class PublicKey : ByteArray<PublicKeyDef>
    {
        public PublicKey(byte[] byteArray)
            : base(byteArray)
        {
        }

        public static PublicKey ParseBase58(string base58String)
        {
            return new PublicKey(Base58.Decode(base58String));
        }
    }

    public class PublicKeyDef : ByteArrayDef
    {
        public override int Length => 33;
    }
}
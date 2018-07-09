using UChainDB.Example.Chain.Utility;

namespace UChainDB.Example.Chain.Entity
{
    public class PrivateKey : ByteArray<PrivateKeyDef>
    {
        public PrivateKey(byte[] byteArray)
            : base(byteArray)
        {
        }

        public static PrivateKey ParseBase58(string base58String)
        {
            return new PrivateKey(Base58.Decode(base58String));
        }
    }

    public class PrivateKeyDef : ByteArrayDef
    {
        public override int Length => 32;
    }
}
using UChainDB.Example.Chain.Utility;

namespace UChainDB.Example.Chain.Entity
{
    public class Signature : ByteArray<SignatureDef>
    {
        public Signature(byte[] byteArray)
            : base(byteArray)
        {
        }

        public static Signature ParseBase58(string base58String)
        {
            return new Signature(Base58.Decode(base58String));
        }
    }

    public class SignatureDef : ByteArrayDef
    {
        public override int Length => 64;
    }
}
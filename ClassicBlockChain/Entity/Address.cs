using UChainDB.Example.Chain.Utility;

namespace UChainDB.Example.Chain.Entity
{
    public class Address : ByteArray<AddressDef>
    {
        public Address(byte[] byteArray)
            : base(byteArray)
        {
        }

        public static Address ParseBase58(string base58String)
        {
            return new Address(Base58.Decode(base58String));
        }
    }

    public class AddressDef : ByteArrayDef
    {
        public override int Length => 20;
    }
}
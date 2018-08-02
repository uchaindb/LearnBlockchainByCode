using UChainDB.Example.Chain.Entity;

namespace UChainDB.Example.Chain.Network.RpcCommands
{
    public enum InventoryType
    {
        Transaction,
        Block,
    }

    public class InventoryEntity
    {
        public InventoryEntity(InventoryType type, UInt256 hash)
        {
            this.Type = type;
            this.Hash = hash;
        }

        public InventoryType Type { get; set; }
        public UInt256 Hash { get; set; }
    }
}
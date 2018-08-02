using UChainDB.Example.Chain.Core;

namespace UChainDB.Example.Chain.Network.RpcCommands
{
    public class InventoryCommnad : Command
    {
        public override string CommandType => Commands.Inventory;

        public InventoryEntity[] Items { get; set; }

        public override void OnReceived(Node node, ConnectionNode connectionNode)
        {
        }
    }
}
using UChainDB.Example.Chain.Core;

namespace UChainDB.Example.Chain.Network.RpcCommands
{
    public class NotFoundCommnad : Command
    {
        public override string CommandType => Commands.NotFound;

        public InventoryEntity[] Items { get; set; }

        public override void OnReceived(Node node, ConnectionNode connectionNode)
        {
        }
    }
}
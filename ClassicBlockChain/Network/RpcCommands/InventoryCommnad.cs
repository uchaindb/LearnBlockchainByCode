using UChainDB.Example.Chain.Core;

namespace UChainDB.Example.Chain.Network.RpcCommands
{
    public class InventoryCommnad : Command
    {
        public override string CommandType => Commands.Inventory;

        public InventoryEntity[] Items { get; set; }

        public override void OnReceived(Node node, ConnectionNode connectionNode)
        {
            var engine = node.Engine;
            var bc = engine.BlockChain;

            var responseCmd = new GetDataCommnad { Items = this.Items };
            connectionNode.ApiClient.SendAsync(responseCmd);
        }
    }
}
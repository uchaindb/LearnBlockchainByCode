using UChainDB.Example.Chain.Core;

namespace UChainDB.Example.Chain.Network.RpcCommands
{
    public class GetDataCommnad : CommandBase
    {
        public override string CommandType => Commands.GetData;

        public InventoryEntity[] Items { get; set; }

        public override void OnReceived(Node node, ConnectionNode connectionNode)
        {
            var engine = node.Engine;
            var bc = engine.BlockChain;
            foreach (var item in this.Items)
            {
                switch (item.Type)
                {
                    case InventoryType.Transaction:
                        var tx = bc.GetTx(item.Hash);
                        if (tx != null)
                        {
                            var responseCmd = new TransactionCommnad { Transaction = tx };
                            connectionNode.Peer.Send(responseCmd);
                        }
                        break;
                    case InventoryType.Block:
                        var blk = bc.GetBlock(item.Hash);
                        if (blk != null)
                        {
                            var responseCmd = new BlockCommnad { Block = blk };
                            connectionNode.Peer.Send(responseCmd);
                        }
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
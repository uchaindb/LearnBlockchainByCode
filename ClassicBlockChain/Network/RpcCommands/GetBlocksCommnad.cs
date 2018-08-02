using System.Linq;
using UChainDB.Example.Chain.Core;
using UChainDB.Example.Chain.Entity;

namespace UChainDB.Example.Chain.Network.RpcCommands
{
    public class GetBlocksCommnad : BlockLocatorCommnadBase
    {
        private const int MaxBlockRetrivalNumber = 100;
        public override string CommandType => Commands.GetBlocks;

        public override void OnReceived(Node node, ConnectionNode connectionNode)
        {
            var engine = node.Engine;
            var recentValidHash = UInt256.Zero;

            for (int i = 0; i < this.BlockLocators.Length; i++)
            {
                var hash = this.BlockLocators[i];

                if (engine.BlockChain.BlockHeadDictionary.TryGetValue(hash, out var block))
                {
                    recentValidHash = block.Hash;
                    break;
                }
            }

            var items = engine.BlockChain.GetBlockHeaders(recentValidHash)
                .Take(MaxBlockRetrivalNumber)
                .Select(_ => new InventoryEntity(InventoryType.Block, _.Hash))
                .ToArray();

            var responseCmd = new InventoryCommnad { Items = items };
            connectionNode.ApiClient.SendAsync(responseCmd);
        }
    }
}
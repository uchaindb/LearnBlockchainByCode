using UChainDB.Example.Chain.Core;
using UChainDB.Example.Chain.Entity;

namespace UChainDB.Example.Chain.Network.RpcCommands
{
    public class BlockCommnad : Command
    {
        public override string CommandType => Commands.Block;

        public Block Block { get; set; }

        public override void OnReceived(Node node, ConnectionNode connectionNode)
        {
            var engine = node.Engine;
            var bc = engine.BlockChain;

            // nothing to process as it already exists
            if (bc.BlockHeadDictionary.ContainsKey(this.Block.Hash)) return;

            if (bc.BlockHeadDictionary.ContainsKey(this.Block.Head.PreviousBlockHash))
            {
                bc.AddSyncBlock(this.Block);
            }
        }
    }
}
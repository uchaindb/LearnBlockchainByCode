using UChainDB.Example.Chain.Core;
using UChainDB.Example.Chain.Entity;

namespace UChainDB.Example.Chain.Network.RpcCommands
{
    public class BlockCommand : CommandBase
    {
        public override string CommandType => Commands.Block;

        public Block Block { get; set; }

        public override void OnReceived(Node node, ConnectionNode connectionNode)
        {
            var engine = node.Engine;
            var bc = engine.BlockChain;

            // nothing to process as it already exists
            if (bc.BlockHeadDictionary.ContainsKey(this.Block.Hash)) return;

            bc.AddSyncBlock(this.Block);

            // initialize sync process if broken chain block received
            if (bc.BlockHeadDictionary.ContainsKey(this.Block.Head.PreviousBlockHash))
            {
                var getblkcmd = new GetBlocksCommand
                {
                    BlockLocators = engine.BlockChain.GetBlockLocatorHashes(),
                    LastBlockHash = bc.Tail.Hash,
                };
                connectionNode.Peer.Send(getblkcmd);
            }
        }
    }
}
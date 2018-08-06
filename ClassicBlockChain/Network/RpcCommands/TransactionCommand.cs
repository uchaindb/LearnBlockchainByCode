using UChainDB.Example.Chain.Core;
using UChainDB.Example.Chain.Entity;

namespace UChainDB.Example.Chain.Network.RpcCommands
{
    public class TransactionCommand : CommandBase
    {
        public override string CommandType => Commands.Transaction;

        public Transaction Transaction { get; set; }

        public override void OnReceived(Node node, ConnectionNode connectionNode)
        {
            var bc = node.Engine.BlockChain;
            bc.SyncTx(this.Transaction);
        }
    }
}
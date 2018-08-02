using UChainDB.Example.Chain.Core;
using UChainDB.Example.Chain.Entity;

namespace UChainDB.Example.Chain.Network.RpcCommands
{
    public class TransactionCommnad : Command
    {
        public override string CommandType => Commands.Transaction;

        public Transaction Transaction { get; set; }

        public override void OnReceived(Node node, ConnectionNode connectionNode)
        {
            var engine = node.Engine;
            var bc = engine.BlockChain;

            bc.AddTx(Transaction);
        }
    }
}
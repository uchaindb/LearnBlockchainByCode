using UChainDB.Example.Chain.Core;
using UChainDB.Example.Chain.Entity;

namespace UChainDB.Example.Chain.Network.RpcCommands
{
    public class HeadersCommnad : Command
    {
        public override string CommandType => Commands.Headers;

        public BlockHead[] Headers { get; set; }

        public override void OnReceived(Node node, ConnectionNode connectionNode)
        {
        }
    }
}
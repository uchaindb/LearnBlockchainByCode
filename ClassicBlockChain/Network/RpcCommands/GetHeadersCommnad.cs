using UChainDB.Example.Chain.Core;

namespace UChainDB.Example.Chain.Network.RpcCommands
{
    public class GetHeadersCommnad : BlockLocatorCommnadBase
    {
        public override string CommandType => Commands.GetHeaders;

        public override void OnReceived(Node node, ConnectionNode connectionNode)
        {
        }
    }
}
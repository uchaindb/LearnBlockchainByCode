using UChainDB.Example.Chain.Core;

namespace UChainDB.Example.Chain.Network.RpcCommands
{
    public class GetAddressCommnad : Command
    {
        public override string CommandType => Commands.GetAddress;

        public override void OnReceived(Node node, ConnectionNode connectionNode)
        {
        }
    }
}
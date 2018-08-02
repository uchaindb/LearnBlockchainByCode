using UChainDB.Example.Chain.Core;

namespace UChainDB.Example.Chain.Network.RpcCommands
{
    public class GetDataCommnad : Command
    {
        public override string CommandType => Commands.GetData;

        public override void OnReceived(Node node, ConnectionNode connectionNode)
        {
        }
    }
}
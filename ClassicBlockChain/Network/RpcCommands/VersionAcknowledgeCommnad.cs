using UChainDB.Example.Chain.Core;

namespace UChainDB.Example.Chain.Network.RpcCommands
{
    public class VersionAcknowledgeCommnad : CommandBase
    {
        public override string CommandType => Commands.VersionAcknowledge;

        public override void OnReceived(Node node, ConnectionNode connectionNode)
        {
            connectionNode.Status = ConnectionStatus.Connected;
        }
    }
}
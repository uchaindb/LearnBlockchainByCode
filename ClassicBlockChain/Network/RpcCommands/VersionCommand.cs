using UChainDB.Example.Chain.Core;

namespace UChainDB.Example.Chain.Network.RpcCommands
{
    public class VersionCommand : Command
    {
        public override string CommandType => Commands.Version;

        public override void OnReceived(Node node, ConnectionNode connectionNode)
        {
            connectionNode.Status = ConnectionStatus.Connected;
            connectionNode.ApiClient.SendAsync(new VersionAcknowledgeCommnad()).Wait();
        }
    }
}
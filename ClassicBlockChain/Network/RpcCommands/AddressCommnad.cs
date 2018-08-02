using UChainDB.Example.Chain.Core;

namespace UChainDB.Example.Chain.Network.RpcCommands
{
    public class AddressCommnad : Command
    {
        public override string CommandType => Commands.Address;

        public string Address { get; set; }

        public override void OnReceived(Node node, ConnectionNode connectionNode)
        {
        }
    }
}
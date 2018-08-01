using System;
using System.Threading.Tasks;

namespace UChainDB.Example.Chain.Network.InMemory
{
    public class InMemoryApiServer //: IApiServer
    {
        private INodeOperator nodeOperator;
        private InMemoryClientServerCenter center;

        private Action<IPeer> acceptFunc;

        public InMemoryApiServer(InMemoryClientServerCenter center, string address)
        {
            this.center = center;
            this.Address = address;
        }

        public string Address { get; }

        public void Dispose()
        {
        }

        public void Start(INodeOperator nodeOperator)
        {
            this.nodeOperator = nodeOperator;
        }

        public byte[] Execute(byte[] buffer)
        {
            return this.nodeOperator.ExecuteRpcRaw(buffer);
        }

        public void Start(Action<IPeer> acceptFunc)
        {
            this.acceptFunc = acceptFunc;
        }

        internal Task<bool> ConnectAsync(ActiveInMemoryClient client)
        {
            this.acceptFunc(new PassiveInMemoryClient(this.center, client));
            return Task.FromResult(true);
        }
    }
}
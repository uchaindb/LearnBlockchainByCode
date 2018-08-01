using System;
using System.Threading;
using System.Threading.Tasks;
using UChainDB.Example.Chain.Network.RpcCommands;

namespace UChainDB.Example.Chain.Network
{
    //public interface IApiServer : IDisposable
    //{
    //    void Start(INodeOperator nodeOperator);
    //    void Start(Func<IPeer> acceptFunc);
    //}

    public interface IConnectionPool : IDisposable
    {
    }
}

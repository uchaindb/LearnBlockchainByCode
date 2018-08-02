using System;
using System.Threading;
using System.Threading.Tasks;
using UChainDB.Example.Chain.Network.RpcCommands;

namespace UChainDB.Example.Chain.Network
{
    public interface IPeer : IDisposable
    {
        bool IsConnected { get; }

        string State { get; }
        string TargetAddress { get; }
        string BaseAddress { get; }

        Task SendAsync(Command command, CancellationToken cancellationToken = default(CancellationToken));

        Task<Command> ReceiveAsync(CancellationToken cancellationToken = default(CancellationToken));

        Task ConnectAsync(string connectionString, CancellationToken cancellationToken = default(CancellationToken));

        Task CloseAsync(object closingMessage = null, CancellationToken cancellationToken = default(CancellationToken));
    }
}
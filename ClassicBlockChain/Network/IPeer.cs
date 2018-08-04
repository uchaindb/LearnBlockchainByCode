using System;
using UChainDB.Example.Chain.Network.RpcCommands;

namespace UChainDB.Example.Chain.Network
{
    public interface IPeer : IDisposable
    {
        bool IsConnected { get; }

        string TargetAddress { get; }
        string BaseAddress { get; }

        void Send(CommandBase command);

        CommandBase Receive();

        void Connect(string connectionString);

        void Close(object closingMessage = null);
    }
}
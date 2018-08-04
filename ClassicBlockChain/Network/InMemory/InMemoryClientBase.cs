using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UChainDB.Example.Chain.Network.RpcCommands;

namespace UChainDB.Example.Chain.Network.InMemory
{
    public abstract class InMemoryClientBase : IPeer
    {
        protected readonly InMemoryClientServerCenter center;
        internal protected InMemoryClientBase opposite;

        public string TargetAddress { get; protected set; }
        public string BaseAddress { get; protected set; }

        protected Queue<CommandBase> receivedData = new Queue<CommandBase>();

        public InMemoryClientBase(InMemoryClientServerCenter center)
        {
            this.center = center;
        }

        public bool IsConnected { get; protected set; }

        public string State => "Normal";

        public virtual Task CloseAsync(object closingMessage = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            this.IsConnected = false;
            this.center.RemovePeer(this);
            return Task.CompletedTask;
        }

        public abstract Task ConnectAsync(string connectionString, CancellationToken cancellationToken = default(CancellationToken));

        public virtual void Dispose()
        {
        }

        public virtual Task<CommandBase> ReceiveAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (this.receivedData.TryDequeue(out var ret))
            {
                return Task.FromResult(ret);
            }
            return Task.FromResult<CommandBase>(null);
        }

        public virtual async Task SendAsync(CommandBase command, CancellationToken cancellationToken = default(CancellationToken))
        {
            //await this.center.SendAsync(this.TargetAddress, command);
            this.opposite.receivedData.Enqueue(command);
        }

        internal Task InternalSendAsync(CommandBase command)
        {
            //this.opposite.receivedData.Enqueue(command);
            this.receivedData.Enqueue(command);
            return Task.CompletedTask;
        }
    }
}
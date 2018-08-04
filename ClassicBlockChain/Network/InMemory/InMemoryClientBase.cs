using System.Collections.Generic;
using UChainDB.Example.Chain.Network.RpcCommands;

namespace UChainDB.Example.Chain.Network.InMemory
{
    public abstract class InMemoryClientBase : IPeer
    {
        protected internal InMemoryClientBase opposite;
        protected readonly InMemoryClientServerCenter center;
        protected Queue<CommandBase> receivedData = new Queue<CommandBase>();

        public InMemoryClientBase(InMemoryClientServerCenter center)
        {
            this.center = center;
        }

        public string TargetAddress { get; protected set; }
        public string BaseAddress { get; protected set; }
        public bool IsConnected { get; protected set; }

        public string State => "Normal";

        public abstract void Connect(string connectionString);

        public void Send(CommandBase command)
        {
            this.opposite.receivedData.Enqueue(command);
        }

        public CommandBase Receive()
        {
            if (this.receivedData.TryDequeue(out var ret))
            {
                return ret;
            }
            return null;
        }

        public void Close(object closingMessage = null)
        {
            this.IsConnected = false;
            this.center.RemovePeer(this);
        }

        public virtual void Dispose()
        {
        }

        internal void InternalSend(CommandBase command)
        {
            this.receivedData.Enqueue(command);
        }
    }
}
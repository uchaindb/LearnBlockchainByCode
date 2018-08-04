using System;
using System.Collections.Concurrent;
using UChainDB.Example.Chain.Entity;
using UChainDB.Example.Chain.Network;
using UChainDB.Example.Chain.Network.RpcCommands;
using UChainDB.Example.Chain.Utility;
using UChainDB.Example.Chain.Wallet;

namespace UChainDB.Example.Chain.Core
{
    public class Node : IDisposable
    {
        public Engine Engine { get; set; }

        private readonly IListener listener;
        private readonly NodeOptions options;
        private readonly IPeerFactory peerFactory;
        public ConnectionPool ConnPool { get; }

        public Node(IWallet miner, IListener listener, IPeerFactory peerFactory, NodeOptions options = null)
        {
            this.Engine = new Engine(miner);
            this.options = options ?? new NodeOptions();

            this.listener = listener;
            this.listener.Start();
            this.peerFactory = peerFactory;
            this.ConnPool = new ConnectionPool(this, this.options.WellKnownNodes, this.peerFactory, this.listener);
            this.ConnPool.Start();
        }

        public void Dispose()
        {
            this.Engine?.Dispose();
            this.listener.Dispose();
            this.ConnPool.Dispose();
            this.peerFactory.Dispose();
        }
    }
}
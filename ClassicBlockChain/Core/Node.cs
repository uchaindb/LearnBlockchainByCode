using System;
using UChainDB.Example.Chain.Entity;
using UChainDB.Example.Chain.Network;
using UChainDB.Example.Chain.Wallet;

namespace UChainDB.Example.Chain.Core
{
    public class Node : IDisposable
    {
        private readonly IListener listener;
        private readonly NodeOptions options;
        private readonly IPeerFactory peerFactory;

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

        public Engine Engine { get; }
        public ConnectionPool ConnPool { get; }

        public void Dispose()
        {
            this.Engine?.Dispose();
            this.listener.Dispose();
            this.ConnPool.Dispose();
            this.peerFactory.Dispose();
        }
    }
}
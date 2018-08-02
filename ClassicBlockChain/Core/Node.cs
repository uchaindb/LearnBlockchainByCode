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
        public INodeOperator NodeOperator { get; }

        private readonly IListener apiServer;
        private readonly NodeOptions options;
        private readonly IApiClientFactory apiClientFactory;
        public Guid NodeId { get; set; } = Guid.NewGuid();
        internal ConnectionPool pool;
        public int NetworkId { get; }

        public Node(IWallet miner, IListener apiServer, IApiClientFactory apiClientFactory, NodeOptions options = null)
        {
            this.Engine = new Engine(miner);
            this.options = options ?? new NodeOptions();

            var no = new NodeOperator();
            //no.Init(this);
            this.NodeOperator = no;
            this.apiServer = apiServer;
            //this.apiServer.Start(this.NodeOperator);
            this.apiServer.Start();
            this.apiClientFactory = apiClientFactory;
            this.pool = new ConnectionPool(this, this.options.NetworkId, this.options.WellKnownNodes, this.NodeId, this.apiClientFactory, this.apiServer);
            this.pool.Start();
            this.NetworkId = options.NetworkId;
            this.Engine.OnNewBlockCreated += Engine_OnNewBlockCreated;
            this.Engine.OnNewTxCreated += Engine_OnNewTxCreated;
        }

        private void Engine_OnNewTxCreated(object sender, Transaction e)
        {
            this.pool.BroadcastAsync(new TransactionCommnad { Transaction = e });
        }

        private void Engine_OnNewBlockCreated(object sender, BlockHead e)
        {
            var blk = this.Engine.BlockChain.GetBlock(e.Hash);
            this.pool.BroadcastAsync(new BlockCommnad { Block = blk });
        }

        public void Dispose()
        {
            this.Engine?.Dispose();
            this.apiServer.Dispose();
            this.pool.Dispose();
            this.apiClientFactory.Dispose();
        }
    }

    public class NodeOptions
    {
        public int IntervalInMilliseconds { get; set; } = 1000;
        public int NetworkId { get; set; } = 32423;
        public string[] WellKnownNodes { get; set; } = new[] {
            "localhost:3030",
            "localhost:3031",
            "localhost:3032",
            "localhost:3033",
            "localhost:3034",
            "localhost:3035",
        };
    }
}
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using UChainDB.Example.Chain.Entity;

namespace UChainDB.Example.Chain.Network.InMemory
{
    public class InMemoryConnectionCenter
    {
        private int number = 0;

        private ConcurrentDictionary<string, InMemoryListener> dicServers = new ConcurrentDictionary<string, InMemoryListener>();
        private ConcurrentDictionary<string, List<InMemoryPeerBase>> dicPeers = new ConcurrentDictionary<string, List<InMemoryPeerBase>>();

        public InMemoryConnectionCenter()
        {
        }

        public NodeOptions NodeOptions { get => new NodeOptions { WellKnownNodes = this.ProduceWellKnownNodes() }; }

        public string[] ProduceWellKnownNodes() => Enumerable.Range(0, this.number).Select(_ => _.ToString()).ToArray();

        public (IListener listener, IPeerFactory peerFactory) Produce()
        {
            var listener = this.ProduceListener();
            var peerFactory = this.ProducePeerFactory(listener);
            return (listener, peerFactory);
        }

        internal void AddPeer(InMemoryPeerBase peer)
        {
            var key = peer.TargetAddress;
            if (this.dicPeers.ContainsKey(key))
            {
                this.dicPeers[key].Add(peer);
            }
            else
            {
                this.dicPeers[key] = new List<InMemoryPeerBase>(new[] { peer });
            }
        }

        internal void RemovePeer(InMemoryPeerBase peer)
        {
            var key = peer.TargetAddress;
            if (this.dicPeers.ContainsKey(key))
            {
                this.dicPeers[key].Remove(peer);
            }
            else
            {
                // nothing to remove
            }
        }

        internal bool Connect(string baseAddress, ActiveInMemoryPeer peer)
        {
            return this.dicServers[baseAddress].Connect(peer);
        }

        private InMemoryListener ProduceListener()
        {
            var address = this.number.ToString();
            var server = new InMemoryListener(this, address);
            this.dicServers[address] = server;
            this.number++;
            return server;
        }

        private IPeerFactory ProducePeerFactory(InMemoryListener server)
        {
            return new InMemoryPeerFactory(this, server);
        }
    }
}
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UChainDB.Example.Chain.Core;
using UChainDB.Example.Chain.Network.RpcCommands;

namespace UChainDB.Example.Chain.Network.InMemory
{
    public class InMemoryClientServerCenter
    {
        //public struct PeerDirection
        //{
        //    public string Source { get; set; }
        //    public string Destination { get; set; }
        //}
        private int number = 0;

        private ConcurrentDictionary<string, InMemoryListener> dicServers = new ConcurrentDictionary<string, InMemoryListener>();
        private ConcurrentDictionary<string, List<InMemoryClientBase>> dicPeers = new ConcurrentDictionary<string, List<InMemoryClientBase>>();

        internal void AddPeer(InMemoryClientBase client)
        {
            var key = client.TargetAddress;
            if (this.dicPeers.ContainsKey(key))
            {
                this.dicPeers[key].Add(client);
            }
            else
            {
                this.dicPeers[key] = new List<InMemoryClientBase>(new[] { client });
            }
        }

        internal void RemovePeer(InMemoryClientBase client)
        {
            var key = client.TargetAddress;
            if (this.dicPeers.ContainsKey(key))
            {
                this.dicPeers[key].Remove(client);
            }
            else
            {
                // nothing to remove
            }
        }

        public InMemoryClientServerCenter()
        {
        }

        public NodeOptions NodeOptions { get => new NodeOptions { WellKnownNodes = this.ProduceWellKnownNodes(), IntervalInMilliseconds = 300 }; }

        public string[] ProduceWellKnownNodes() => Enumerable.Range(0, this.number).Select(_ => _.ToString()).ToArray();

        private InMemoryListener ProduceApiServer()
        {
            var address = this.number.ToString();
            var server = new InMemoryListener(this, address);
            this.dicServers[address] = server;
            this.number++;
            return server;
        }

        private IApiClientFactory ProduceApiClientFactory(InMemoryListener server)
        {
            return new InMemoryClientFactory(this, server);
        }

        public (IListener listener, IApiClientFactory clientFactory) Produce()
        {
            var server = this.ProduceApiServer();
            var clientFactory = this.ProduceApiClientFactory(server);
            return (server, clientFactory);
        }

        //internal async Task SendAsync(string targetAddress, Command command)
        //{
        //    if (this.dicPeers.TryGetValue(targetAddress, out var clients))
        //    {
        //        var cc = clients.ToArray();
        //        foreach (var client in cc)
        //        {
        //            await client.InternalSendAsync(command);
        //        }
        //    }
        //    //return Task.FromResult(this.dicServers[baseAddress].Execute(buffer));
        //}

        internal async Task<bool> ConnectAsync(string baseAddress, ActiveInMemoryClient client)
        {
            return await this.dicServers[baseAddress].ConnectAsync(client);
        }
    }
}
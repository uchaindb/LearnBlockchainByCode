using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UChainDB.Example.Chain.Core;
using UChainDB.Example.Chain.Entity;
using UChainDB.Example.Chain.Network.RpcCommands;

namespace UChainDB.Example.Chain.Network
{
    public class ConnectionPool : IConnectionPool
    {
        internal List<ConnectionNode> nodes;
        private int magicNumber;
        private Guid nodeId;
        private Node selfNode;
        private IApiClientFactory apiClientFactory;
        private readonly IListener listener;
        private Timer reconnectTimer;
        private bool isSyncing = false;
        private bool isReceiving = false;
        private Thread thReceive;
        public event EventHandler<Command> OnCommandReceived;

        public ConnectionPool(Node node, int magicNumber, string[] wellKnowns, Guid nodeId, IApiClientFactory apiClientFactory, IListener listener)
        {
            this.selfNode = node;
            this.magicNumber = magicNumber;
            this.nodes = wellKnowns.Select(_ => new ConnectionNode(_)).ToList();
            this.nodeId = nodeId;
            this.apiClientFactory = apiClientFactory;
            this.listener = listener;
            this.listener.OnPeerConnected += Listener_OnPeerConnected;
        }

        private void Listener_OnPeerConnected(object sender, IPeer e)
        {
            lock (this.nodes)
            {
                this.nodes.Add(new ConnectionNode("TODO")
                {
                    ApiClient = e,
                });
            }
        }

        public int ConnectionNumber { get; set; }

        public void Start()
        {
            this.reconnectTimer = new Timer(async (_) => await this.ConnectAllAsync(), null, new TimeSpan(0, 0, 1), new TimeSpan(0, 0, 20));
            this.thReceive = new Thread(Receive);
            this.thReceive.Start();
            this.isReceiving = true;
        }

        private void Receive()
        {
            try
            {
                while (this.isReceiving)
                {
                    ConnectionNode[] internalnodes;
                    lock (this.nodes)
                    {
                        internalnodes = this.nodes.ToArray();
                    }
                    foreach (var node in internalnodes)
                    {
                        if (node.ApiClient == null) continue;
                        var command = node.ApiClient.ReceiveAsync().Result;
                        if (command == null) continue;
                        OnCommandReceived?.Invoke(this, command);
                        command.OnReceived(this.selfNode, node);
                        if (!this.isReceiving) break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"unexpected exception in receive: {ex}");
            }
        }

        private async Task ConnectAllAsync()
        {
            ConnectionNode[] internalnodes;
            lock (this.nodes)
            {
                internalnodes = this.nodes
                    .Where(_ => _.Status == ConnectionStatus.Initial || _.Status == ConnectionStatus.Dead)
                    .ToArray();
            }
            foreach (var node in internalnodes)
            {
                await this.TryConnectAsync(node);
            }
        }

        public async void BroadcastAsync(Command command)
        {
            ConnectionNode[] internalnodes;
            lock (this.nodes)
            {
                internalnodes = this.nodes
                    .Where(_ => _.Status == ConnectionStatus.Connected)
                    .ToArray();
            }
            foreach (var node in internalnodes)
            {
                await node.ApiClient.SendAsync(command);
            }
        }

        public async void Dispose()
        {
            this.isReceiving = false;
            this.reconnectTimer?.Dispose();
            ConnectionNode[] internalnodes;
            lock (this.nodes)
            {
                internalnodes = this.nodes
                    .Where(_ => _.Status == ConnectionStatus.Connected)
                    .ToArray();
            }
            foreach (var node in internalnodes)
            {
                //await node.ApiClient.CloseAsync(WebSocketCloseStatus.ProtocolError, "close", CancellationToken.None);
                await node.ApiClient.CloseAsync(CancellationToken.None);
                node.ApiClient?.Dispose();
            }
            this.thReceive.Join();
        }

        private async Task TryConnectAsync(ConnectionNode node)
        {
            // dispose previous client if exist
            if (node.ApiClient != null)
            {
                node.ApiClient.Dispose();
                node.ApiClient = null;
            }

            var client = this.apiClientFactory.Produce();
            try
            {
                await client.ConnectAsync(node.Address);
            }
            catch (ApiClientException)
            {
                //log.LogError($"Cannot connect to server {node.Address}, due to {acex.Message}", acex);
                node.Status = ConnectionStatus.Dead;
            }

            if (!client.IsConnected)
            {
                //log.LogWarning("open api client channel failed");
                node.Status = ConnectionStatus.Dead;
                return;
            }

            try
            {
                await client.SendAsync(new VersionCommand());
            }
            catch (ApiClientException ex) when (ex.InnerException is WebException wex && wex.InnerException is HttpRequestException)
            {
                // close connection
                node.Status = ConnectionStatus.Dead;
            }
            catch (Exception ex)
            {
                //this.log.LogError($"something error trying connect to {node.Address}", ex);
                node.Status = ConnectionStatus.Dead;
            }
            finally
            {
                if (node.Status != ConnectionStatus.Connected)
                {
                    await client.CloseAsync();
                    client.Dispose();
                }
            }
        }
    }

    public enum ConnectionStatus
    {
        Initial,
        Self,
        DifferentNetwork,
        Connected,
        Disconnected,
        Dead,
    }

    public class ConnectionNode
    {
        public ConnectionNode(string address)
        {
            this.Address = address;
        }

        public string Address { get; set; }
        public ConnectionStatus Status { get; set; } = ConnectionStatus.Initial;
        public Guid NodeId { get; set; } = Guid.Empty;
        public IPeer ApiClient { get; set; }
        public BlockHead LatestBlock { get; set; }
        public ulong Height { get; set; }
    }
}
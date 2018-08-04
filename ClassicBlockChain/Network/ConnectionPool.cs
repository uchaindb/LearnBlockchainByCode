using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UChainDB.Example.Chain.Core;
using UChainDB.Example.Chain.Entity;
using UChainDB.Example.Chain.Network.InMemory;
using UChainDB.Example.Chain.Network.RpcCommands;

namespace UChainDB.Example.Chain.Network
{
    public class ConnectionPool : IDisposable
    {
        private readonly List<ConnectionNode> nodes;
        private readonly Node selfNode;
        private readonly IPeerFactory peerFactory;
        private readonly IListener listener;
        private Timer reconnectTimer;
        private bool isReceiving = false;
        private Thread thReceive;
        public event EventHandler<CommandBase> OnCommandReceived;

        public ConnectionPool(Node node, string[] wellKnowns, IPeerFactory peerFactory, IListener listener)
        {
            this.selfNode = node;
            this.nodes = wellKnowns
                .Where(_ => _ != listener.Address)
                .Select(_ => new ConnectionNode(_))
                .ToList();
            this.peerFactory = peerFactory;
            this.listener = listener;
            this.listener.OnPeerConnected += Listener_OnPeerConnected;
            this.selfNode.Engine.OnNewBlockCreated += Engine_OnNewBlockCreated;
            this.selfNode.Engine.OnNewTxCreated += Engine_OnNewTxCreated;
        }

        private void Engine_OnNewTxCreated(object sender, Transaction e)
        {
            this.Broadcast(new TransactionCommnad { Transaction = e });
        }

        private void Engine_OnNewBlockCreated(object sender, BlockHead e)
        {
            var blk = this.selfNode.Engine.BlockChain.GetBlock(e.Hash);
            this.Broadcast(new BlockCommnad { Block = blk });
        }

        private void Listener_OnPeerConnected(object sender, IPeer e)
        {
            lock (this.nodes)
            {
                var prev = this.nodes
                    .FirstOrDefault(_ => 
                        _.Peer.TargetAddress == e.TargetAddress 
                        && _.Peer.BaseAddress == e.BaseAddress);
                if (prev != null) this.nodes.Remove(prev);
                this.nodes.Add(new ConnectionNode()
                {
                    Peer = e,
                });
            }
        }

        public int ConnectionNumber { get; set; }

        public void Start()
        {
            this.reconnectTimer = new Timer((_) => this.ConnectAll(), null, new TimeSpan(0, 0, 0, 0, 100), new TimeSpan(0, 0, 20));
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
                        if (node.Peer == null) continue;
                        var command = node.Peer.Receive();
                        if (command == null) continue;
                        OnCommandReceived?.Invoke(this, command);
                        command.OnReceived(this.selfNode, node);
                        if (!this.isReceiving) break;
                    }

                    Thread.Sleep(500);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"unexpected exception in receive: {ex}");
            }
        }

        private void ConnectAll()
        {
            ConnectionNode[] internalnodes;
            lock (this.nodes)
            {
                internalnodes = this.nodes
                    .Where(_ => _.Status == ConnectionStatus.Initial || _.Status == ConnectionStatus.Dead)
                    .Where(_ => _.Address != null)
                    .ToArray();
            }
            foreach (var node in internalnodes)
            {
                this.TryConnect(node);
            }
        }

        public void Broadcast(CommandBase command)
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
                node.Peer.Send(command);
            }
        }

        public void Dispose()
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
                node.Peer.Close();
                node.Peer?.Dispose();
            }
            this.thReceive.Join();
        }

        private void TryConnect(ConnectionNode node)
        {
            // dispose previous client if exist
            if (node.Peer != null)
            {
                node.Peer.Dispose();
                node.Peer = null;
            }

            var peer = this.peerFactory.Produce();
            try
            {
                peer.Connect(node.Address);
                node.Peer = peer;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Cannot connect to server {node.Address}, due to {ex.Message}");
                node.Status = ConnectionStatus.Dead;
            }

            if (!peer.IsConnected)
            {
                Debug.WriteLine("open peer channel failed");
                node.Status = ConnectionStatus.Dead;
                return;
            }

            try
            {
                peer.Send(new VersionCommand());
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"something error trying connect to {node.Address}, ex:{ex}");
                node.Status = ConnectionStatus.Dead;
            }
            finally
            {
                if (node.Status != ConnectionStatus.Connected)
                {
                    peer.Close();
                    peer.Dispose();
                }
            }
        }
    }
}
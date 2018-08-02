using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UChainDB.Example.Chain.Core;
using UChainDB.Example.Chain.Entity;
using UChainDB.Example.Chain.Network.InMemory;
using UChainDB.Example.Chain.Network.RpcCommands;
using UChainDB.Example.Chain.Utility;
using UChainDB.Example.Chain.Wallet;

namespace DebugConsole
{
    public class ControlHub : Hub
    {
        public ControlHub(IHubContext<ControlHub> hubcontext)
        {
            this.hubcontext = hubcontext;
        }
        private Timer updateTimer;
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        public async Task Start()
        {
            this.updateTimer = new Timer(async (_) => await this.UpdateBlock(), null, new TimeSpan(0, 0, 1), new TimeSpan(0, 0, 1));
            this.clientData = new ClientEntity();
            this.StatusHelper = new StatusHelper(clientData);

            for (int i = 0; i < nodeNumber; i++)
            {
                this.clientData.Statuses.Add(new StatusEntity());
                var (listener, clientFactory) = center.Produce();
                var number = i;
                var miner = new DeterministicWallet($"{number}(Miner)");
                miners.Add(miner);
                var node = new Node(miner, listener, clientFactory, center.NodeOptions);
                nodes.Add(node);
                StatusHelper.Append($"[Node {number}]Genesis Block: {BlockChain.GenesisBlock}", number);
                AssignEvent(node, number);
            }
        }

        public async Task Stop()
        {
            this.updateTimer.Dispose();
            for (int i = 0; i < nodeNumber; i++)
            {
                var node = nodes[i];
                node.Dispose();
            }
        }

        private async Task UpdateBlock()
        {
            for (int i = 0; i < nodeNumber; i++)
            {
                var node = nodes[i];
                var number = i;
                var blocks = node.Engine.BlockChain.GetBlockHeaders(BlockChain.GenesisBlockHead.Hash)
                    .Select((_, h) => new BlockEntity { Height = h + 2, Hash = _.Hash.ToHex() })
                    .ToList();
                this.clientData.Statuses[i].Blocks = blocks;
            }

            //if (Clients != null)
            //{
            //    await Clients.All.SendAsync("Update", this.clientData);
            //}
            await this.hubcontext.Clients.All.SendAsync("Update", this.clientData);
        }

        private List<IWallet> miners = new List<IWallet>();
        private IWallet alice = new SimpleWallet("Alice");
        private IWallet bob = new SimpleWallet("Bob");

        private Utxo h2utxo;
        private Transaction h3tx;
        private bool bobVerified = false;
        private List<Node> nodes = new List<Node>();
        private InMemoryClientServerCenter center = new InMemoryClientServerCenter();
        private int nodeNumber = 2;
        private ClientEntity clientData;
        StatusHelper StatusHelper;
        private readonly IHubContext<ControlHub> hubcontext;

        private void AssignEvent(Node node, int number)
        {
            node.Engine.OnNewBlockCreated += (object sender, BlockHead block) =>
            {
                var engine = sender as Engine;
                var height = engine.BlockChain.Height;
                var tailBlock = engine.BlockChain.GetBlock(engine.BlockChain.Tail.Hash);
                StatusHelper.Append($"New block created at height[{height:0000}]: {tailBlock}", number);

                var me = miners[number];
                // take action only on first node
                if (number == 0)
                {
                    if (height == 2)
                    {
                        var utxos = me.GetUtxos(engine);
                        var utxo = utxos.First();
                        h2utxo = utxo;
                        me.SendMoney(engine, utxo.Tx, utxo.Index, alice, 30, 1);
                        return;
                    }

                    if (h3tx == null)
                    {
                        var utxos = alice.GetUtxos(engine);
                        var utxo = utxos.FirstOrDefault();

                        if (utxo != null)
                        {
                            alice.SyncBlockHead(engine);
                            var verify = alice.VerifyTx(engine, utxo.Tx);
                            StatusHelper.Append($"verify [{utxo.Tx.Hash.ToShort()}]: {verify}", number);
                            h3tx = alice.SendMoney(engine, utxo.Tx, utxo.Index, bob, 20);
                        }

                        return;
                    }

                    if (!bobVerified)
                    {
                        bob.SyncBlockHead(engine);
                        var verify = bob.VerifyTx(engine, h3tx);
                        StatusHelper.Append($"verify [{h3tx.Hash.ToShort()}]: {verify}", number);
                        bobVerified = verify;
                        if (bobVerified)
                        {
                            // try to use used transaction which cannot pass validation and ignored
                            me.SendMoney(engine, h2utxo.Tx, h2utxo.Index, bob, 50);
                        }
                    }
                }
            };

            node.pool.OnCommandReceived += (object sender, Command e) =>
            {
                StatusHelper.Append($"Command[{e.CommandType}] received", number);
            };
        }

    }

    public class StatusHelper
    {
        private readonly ClientEntity clientData;

        public StatusHelper(ClientEntity clientData)
        {
            this.clientData = clientData;
        }
        public void Append(string value, int number)
        {
            clientData.Statuses[number].Status.Add(value);
        }
    }

    public class BlockEntity
    {
        public string Hash { get; set; }
        public int Height { get; set; }
    }

    public class StatusEntity
    {
        public string Name { get; set; }
        public List<BlockEntity> Blocks { get; set; } = new List<BlockEntity>();
        public List<string> Status { get; set; } = new List<string>();
    }
    public class ClientEntity
    {
        public List<StatusEntity> Statuses { get; set; } = new List<StatusEntity>();
    }
}

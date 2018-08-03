using DebugConsole.Models;
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
    public class ControlService
    {
        private readonly IHubContext<ControlHub> hubcontext;
        private Timer updateTimer;
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

        public ControlService(IHubContext<ControlHub> hubcontext)
        {
            this.hubcontext = hubcontext;
        }

        public async Task Start()
        {
            this.h2utxo = null;
            this.h3tx = null;
            this.bobVerified = false;

            this.updateTimer = new Timer(async (_) => await this.UpdateBlock(), null, new TimeSpan(0, 0, 1), new TimeSpan(0, 0, 1));
            this.clientData = new ClientEntity();
            this.clientData.IsRunning = true;

            for (int i = 0; i < nodeNumber; i++)
            {
                var number = i;
                this.clientData.Nodes.Add(new NodeEntity { Name = number.ToString(), });
                var (listener, clientFactory) = center.Produce();
                var miner = new DeterministicWallet($"{number}(Miner)");
                miners.Add(miner);
                var node = new Node(miner, listener, clientFactory, center.NodeOptions);
                nodes.Add(node);
                Append(new BlockCreatedStatusEntity(BlockChain.GenesisBlock, "Genesis Block"), number);
                AssignEvent(node, number);
            }
        }

        public async Task Stop()
        {
            this.clientData.IsRunning = false;
            this.updateTimer.Dispose();
            await UpdateBlock();

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
                var blocks = node.Engine.BlockChain.GetBlocks(BlockChain.GenesisBlockHead.Hash)
                    .Select((_, h) => new BlockEntity { Height = h + 2, Block = _ })
                    .ToList();
                this.clientData.Nodes[i].Blocks = blocks;
            }

            await this.hubcontext.Clients.All.SendAsync("Update", this.clientData);
        }

        private void AssignEvent(Node node, int number)
        {
            node.Engine.OnNewBlockCreated += (object sender, BlockHead block) =>
            {
                var engine = sender as Engine;
                var height = engine.BlockChain.Height;
                var tailBlock = engine.BlockChain.GetBlock(engine.BlockChain.Tail.Hash);
                Append(new BlockCreatedStatusEntity(tailBlock, $"New block created at height[{height:0000}]"), number);

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
                        Append($"send money from me to alice: 30", number);
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
                            Append($"verify [{utxo.Tx.Hash.ToShort()}]: {verify}", number);
                            h3tx = alice.SendMoney(engine, utxo.Tx, utxo.Index, bob, 20);
                            Append($"send money from alice to bob: 20", number);
                        }

                        return;
                    }

                    if (!bobVerified)
                    {
                        bob.SyncBlockHead(engine);
                        var verify = bob.VerifyTx(engine, h3tx);
                        Append($"verify [{h3tx.Hash.ToShort()}]: {verify}", number);
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
                var blkcmd = e as BlockCommnad;
                Append(new CommandReceivedStatusEntity(e.CommandType, blkcmd?.Block?.Hash?.ToHex()), number);
            };
        }

        private void Append(string value, int number)
        {
            Append(new PlainStatusEntity(value), number);
        }

        private void Append(StatusEntity value, int number)
        {
            var list = clientData.Nodes[number].Status;
            list.Insert(0, value);
            var len = list.Count;
            if (len > 20) list.RemoveRange(20, len - 20);
        }
    }
}
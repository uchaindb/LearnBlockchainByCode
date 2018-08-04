using System;
using System.Collections.Generic;
using System.Linq;
using UChainDB.Example.Chain.Core;
using UChainDB.Example.Chain.Entity;
using UChainDB.Example.Chain.Network.InMemory;
using UChainDB.Example.Chain.Utility;
using UChainDB.Example.Chain.Wallet;

namespace UChainDB.Example.Chain
{
    internal class Program
    {
        private static List<IWallet> miners = new List<IWallet>();
        private static IWallet alice = new SimpleWallet("Alice");
        private static IWallet bob = new SimpleWallet("Bob");

        private static Utxo h2utxo;
        private static Transaction h3tx;
        private static bool bobVerified = false;
        private static List<Node> nodes = new List<Node>();
        private static InMemoryConnectionCenter center = new InMemoryConnectionCenter();
        private static int nodeNumber = 2;

        private static void Main(string[] args)
        {
            Console.ResetColor();
            Console.WriteLine($"Press any key to stop....");

            for (int i = 0; i < nodeNumber; i++)
            {
                var (listener, clientFactory) = center.Produce();
                var number = i;
                var miner = new DeterministicWallet($"{number}(Miner)");
                miners.Add(miner);
                var node = new Node(miner, listener, clientFactory, center.NodeOptions);
                nodes.Add(node);
                ConsoleHelper.WriteLine($"[Node {number}]Genesis Block: {BlockChain.GenesisBlock}", number);
                AssignEvent(node, number);
            }

            Console.ReadKey();
            Console.WriteLine($"===================================");

            for (int i = 0; i < nodeNumber; i++)
            {
                var node = nodes[i];
                var number = i;
                ConsoleHelper.WriteLine($"[Node {number}]++++++++++++++++++++++++++++++++++++++++", number);
                var blocks = node.Engine.BlockChain.GetBlockHeaders(BlockChain.GenesisBlockHead.Hash)
                    .Select(_ => _.Hash.ToShort())
                    .ToArray();
                for (int j = 0; j < blocks.Length; j++)
                {
                    ConsoleHelper.WriteLine($"[{j + 2:0000}]: {blocks[j]}", number);
                }

                node.Dispose();
            }

            Console.WriteLine($"Stopped, press any key to exit....");
            Console.ReadKey();
        }

        private static void AssignEvent(Node node, int number)
        {
            node.Engine.OnNewBlockCreated += (object sender, BlockHead block) =>
            {
                var engine = sender as Engine;
                var height = engine.BlockChain.Height;
                var tailBlock = engine.BlockChain.GetBlock(engine.BlockChain.Tail.Hash);
                ConsoleHelper.WriteLine($"New block created at height[{height:0000}]: {tailBlock}", number);

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
                            ConsoleHelper.WriteLine($"verify [{utxo.Tx.Hash.ToShort()}]: {verify}", number);
                            h3tx = alice.SendMoney(engine, utxo.Tx, utxo.Index, bob, 20);
                        }

                        return;
                    }

                    if (!bobVerified)
                    {
                        bob.SyncBlockHead(engine);
                        var verify = bob.VerifyTx(engine, h3tx);
                        ConsoleHelper.WriteLine($"verify [{h3tx.Hash.ToShort()}]: {verify}", number);
                        bobVerified = verify;
                        if (bobVerified)
                        {
                            // try to use used transaction which cannot pass validation and ignored
                            me.SendMoney(engine, h2utxo.Tx, h2utxo.Index, bob, 50);
                        }
                    }
                }
            };

            node.ConnPool.OnCommandReceived += (object sender, Network.RpcCommands.CommandBase e) =>
            {
                ConsoleHelper.WriteLine($"Command[{e.CommandType}] received", number);
            };
        }
    }
}
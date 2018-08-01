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
        private static IWallet me = new DeterministicWallet("Icer(Miner)");
        private static IWallet alice = new SimpleWallet("Alice");
        private static IWallet bob = new SimpleWallet("Bob");

        private static Utxo h2utxo;
        private static Transaction h3tx;
        private static List<Node> nodes = new List<Node>();
        private static InMemoryClientServerCenter center = new InMemoryClientServerCenter();
        private static int nodeNumber = 2;

        private static void Main(string[] args)
        {
            Console.ResetColor();
            Console.WriteLine($"Press any key to stop....");

            for (int i = 0; i < nodeNumber; i++)
            {
                var (listener, clientFactory) = center.Produce();
                var node = new Node(me, listener, clientFactory, center.NodeOptions);
                nodes.Add(node);
                var engine = node.Engine;
                ConsoleHelper.WriteLine($"Genesis Block: {BlockChain.GenesisBlock}", i + 1);
                AssignEngineEvent(engine, i + 1);
            }

            Console.ReadKey();

            for (int i = 0; i < nodeNumber; i++)
            {
                var node = nodes[i];
                var engine = node.Engine;
                engine.Dispose();
            }

            Console.WriteLine($"Stopped, press any key to exit....");
            Console.ReadKey();
        }

        private static void AssignEngineEvent(Engine engine, int number)
        {
            engine.OnNewBlockCreated += (object sender, BlockHead block) =>
            {
                var senderEngine = sender as Engine;
                var height = engine.BlockChain.Height;
                var tailBlock = engine.BlockChain.GetBlock(engine.BlockChain.Tail.Hash);
                ConsoleHelper.WriteLine($"New block created at height[{height:0000}]: {tailBlock}", number);

                // take action only on first node
                if (number == 1)
                {
                    if (height == 2)
                    {
                        var utxos = me.GetUtxos(engine);
                        var utxo = utxos.First();
                        h2utxo = utxo;
                        me.SendMoney(engine, utxo.Tx, utxo.Index, alice, 30, 1);
                    }
                    else if (height == 3)
                    {
                        var utxos = alice.GetUtxos(engine);
                        var utxo = utxos.First();
                        alice.SyncBlockHead(engine);
                        var verify = alice.VerifyTx(engine, utxo.Tx);
                        ConsoleHelper.WriteLine($"verify [{utxo.Tx.Hash.ToShort()}]: {verify}", number);
                        h3tx = alice.SendMoney(engine, utxo.Tx, utxo.Index, bob, 20);
                    }
                    else if (height == 4)
                    {
                        bob.SyncBlockHead(engine);
                        var verify = bob.VerifyTx(engine, h3tx);
                        ConsoleHelper.WriteLine($"verify [{h3tx.Hash.ToShort()}]: {verify}", number);
                        // try to use used transaction which cannot pass validation and ignored
                        me.SendMoney(engine, h2utxo.Tx, h2utxo.Index, bob, 50);
                    }
                }
            };
        }
    }
}
using System;
using System.Linq;
using UChainDB.Example.Chain.Core;
using UChainDB.Example.Chain.Entity;

namespace UChainDB.Example.Chain
{
    internal class Program
    {
        private static IWallet me = new DeterministicWallet("Icer(Miner)");
        private static IWallet alice = new SimpleWallet("Alice");
        private static IWallet bob = new SimpleWallet("Bob");

        private static (Transaction, int) h2utxo = (null, 0);

        private static void Main(string[] args)
        {
            Console.WriteLine($"Press any key to stop....");
            var engine = new Engine(me);
            Console.WriteLine($"Genesis Block: {BlockChain.GenesisBlock}");
            engine.OnNewBlockCreated += Engine_OnNewBlockCreated;

            Console.ReadKey();
            engine.Dispose();
            Console.WriteLine($"Stopped, press any key to exit....");
            Console.ReadKey();
        }

        private static void Engine_OnNewBlockCreated(object sender, BlockHead block)
        {
            var engine = sender as Engine;
            var height = engine.BlockChain.Height;
            var tailBlock = engine.BlockChain.GetBlock(engine.BlockChain.Tail.Hash);
            Console.WriteLine($"New block created at height[{height:0000}]: {tailBlock}");

            if (height == 2)
            {
                var utxos = me.GetUtxos(engine);
                var utxo = utxos.First();
                h2utxo = utxo;
                me.SendMoney(engine, utxo.tx, utxo.index, alice, 50);
            }
            else if (height == 3)
            {
                var utxos = alice.GetUtxos(engine);
                var utxo = utxos.First();
                alice.SendMoney(engine, utxo.tx, utxo.index, bob, 50);
            }
            else if (height == 4)
            {
                // try to use used transaction which cannot pass validation and ignored
                me.SendMoney(engine, h2utxo.Item1, h2utxo.Item2, bob, 50);
            }
        }
    }
}
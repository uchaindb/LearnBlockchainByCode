using System;
using System.Linq;
using UChainDB.Example.Chain.Core;
using UChainDB.Example.Chain.Entity;
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
                me.SendMoney(engine, utxo.Tx, utxo.Index, alice, 30, 1);
            }
            else if (height == 3)
            {
                var utxos = alice.GetUtxos(engine);
                var utxo = utxos.First();
                alice.SyncBlockHead(engine);
                var verify = alice.VerifyTx(engine, utxo.Tx);
                Console.WriteLine($"verify [{utxo.Tx.Hash.ToShort()}]: {verify}");
                h3tx = alice.SendMoney(engine, utxo.Tx, utxo.Index, bob, 20);
            }
            else if (height == 4)
            {
                bob.SyncBlockHead(engine);
                var verify = bob.VerifyTx(engine, h3tx);
                Console.WriteLine($"verify [{h3tx.Hash.ToShort()}]: {verify}");
                // try to use used transaction which cannot pass validation and ignored
                me.SendMoney(engine, h2utxo.Tx, h2utxo.Index, bob, 50);
            }
        }
    }
}
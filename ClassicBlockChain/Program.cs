﻿using System;
using System.Linq;
using UChainDB.Example.Chain.Core;
using UChainDB.Example.Chain.Entity;

namespace UChainDB.Example.Chain
{
    internal class Program
    {
        private const string myName = "Icer(Miner)";
        private const string AliceName = "Alice";
        private const string BobName = "Bob";

        private static Tx h2utxo = null;

        private static void Main(string[] args)
        {
            Console.WriteLine($"Press any key to stop....");
            var engine = new Engine(myName);
            Console.WriteLine($"Genesis Block: {BlockChain.GenesisBlock}");
            engine.OnNewBlockCreated += Engine_OnNewBlockCreated;

            Console.ReadKey();
            engine.Dispose();
            Console.WriteLine($"Stopped, press any key to exit....");
            Console.ReadKey();
        }

        private static void Engine_OnNewBlockCreated(object sender, Block block)
        {
            var engine = sender as Engine;
            var height = engine.BlockChain.Height;
            Console.WriteLine($"New block created at height[{height:0000}]: {engine.BlockChain.Tail}");

            if (height == 2)
            {
                var utxo = engine.BlockChain.Tail.Txs.First();
                h2utxo = utxo;
                SendMoney(engine, utxo, AliceName, 50);
            }
            else if (height == 3)
            {
                var utxo = engine.BlockChain.Tail.Txs
                    .First(txs => txs.Outputs.Any(_ => _.Owner == AliceName));
                SendMoney(engine, utxo, BobName, 50);
            }
            else if (height == 4)
            {
                // try to use used tx which cannot pass validation and ignored
                SendMoney(engine, h2utxo, BobName, 50);
            }
        }

        private static void SendMoney(Engine engine, Tx utxo, string receiver, int value)
        {
            SendMoney(engine, utxo, new TxOutput { Owner = receiver, Value = value });
        }

        private static void SendMoney(Engine engine, Tx utxo, params TxOutput[] outputs)
        {
            engine.AttachTx(new Tx
            {
                Inputs = new[] { utxo.Hash },
                Outputs = outputs,
            });
        }
    }
}
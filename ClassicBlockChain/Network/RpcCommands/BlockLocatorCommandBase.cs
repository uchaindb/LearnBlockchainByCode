﻿using UChainDB.Example.Chain.Entity;

namespace UChainDB.Example.Chain.Network.RpcCommands
{
    public abstract class BlockLocatorCommandBase : CommandBase
    {
        public UInt256 LastBlockHash { get; set; }
        public UInt256[] BlockLocators { get; set; }
    }
}
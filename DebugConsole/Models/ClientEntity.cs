using System.Collections.Generic;

namespace UChainDB.Example.Chain.DebugConsole.Models
{
    public class ClientEntity
    {
        public List<NodeEntity> Nodes { get; set; } = new List<NodeEntity>();
        public bool IsRunning { get; set; }
    }
}
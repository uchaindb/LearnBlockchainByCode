using System.Collections.Generic;

namespace UChainDB.Example.Chain.DebugConsole.Models
{
    public class NodeEntity
    {
        public string Name { get; set; }
        public List<BlockEntity> Blocks { get; set; } = new List<BlockEntity>();
        public List<StatusEntity> Status { get; set; } = new List<StatusEntity>();
    }
}
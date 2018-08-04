using System.Collections.Generic;

namespace DebugConsole.Models
{
    public class ClientEntity
    {
        public List<NodeEntity> Nodes { get; set; } = new List<NodeEntity>();
        public bool IsRunning { get; set; }
    }
}
using System.Collections.Generic;
using UChainDB.Example.Chain.Entity;

namespace DebugConsole.Models
{
    public class ClientEntity
    {
        public List<NodeEntity> Nodes { get; set; } = new List<NodeEntity>();
    }

    public class BlockEntity
    {
        public Block Block { get; set; }
        public int Height { get; set; }
    }

    public class NodeEntity
    {
        public string Name { get; set; }
        public List<BlockEntity> Blocks { get; set; } = new List<BlockEntity>();
        public List<StatusEntity> Status { get; set; } = new List<StatusEntity>();
    }

    public abstract class StatusEntity
    {
        public abstract StatusType Type { get; }
    }

    public class PlainStatusEntity : StatusEntity
    {
        public override StatusType Type => StatusType.Plain;

        public string Text { get; }
        public PlainStatusEntity(string text)
        {
            Text = text;
        }
    }

    public class CommandReceivedStatusEntity : StatusEntity
    {
        public override StatusType Type => StatusType.CommandReceived;

        public string Command { get; }

        public CommandReceivedStatusEntity(string command)
        {
            Command = command;
        }
    }

    public class BlockCreatedStatusEntity : PlainStatusEntity
    {
        public override StatusType Type => StatusType.BlockCreated;

        public Block Block { get; }

        public BlockCreatedStatusEntity(Block block, string text)
            : base(text)
        {
            Block = block;
        }
    }

    public enum StatusType
    {
        Plain,
        CommandReceived,
        BlockCreated,
    }
}
namespace UChainDB.Example.Chain.DebugConsole.Models
{
    public enum StatusType
    {
        Plain,
        CommandReceived,
        BlockCreated,
    }

    public abstract class StatusEntity
    {
        public abstract StatusType Type { get; }
    }
}
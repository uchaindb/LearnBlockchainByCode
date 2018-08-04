namespace UChainDB.Example.Chain.DebugConsole.Models
{
    public class CommandReceivedStatusEntity : PlainStatusEntity
    {
        public CommandReceivedStatusEntity(string command, string text = null)
            : base(text)
        {
            this.Command = command;
        }

        public override StatusType Type => StatusType.CommandReceived;

        public string Command { get; }
    }
}
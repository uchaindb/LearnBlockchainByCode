namespace DebugConsole.Models
{
    public class CommandReceivedStatusEntity : PlainStatusEntity
    {
        public CommandReceivedStatusEntity(string command, string text = null)
            : base(text)
        {
            Command = command;
        }

        public override StatusType Type => StatusType.CommandReceived;

        public string Command { get; }
    }
}
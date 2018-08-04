namespace UChainDB.Example.Chain.DebugConsole.Models
{
    public class PlainStatusEntity : StatusEntity
    {
        public PlainStatusEntity(string text)
        {
            this.Text = text;
        }

        public override StatusType Type => StatusType.Plain;

        public string Text { get; }
    }
}
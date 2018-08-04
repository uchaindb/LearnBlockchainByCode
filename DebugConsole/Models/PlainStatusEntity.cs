namespace DebugConsole.Models
{
    public class PlainStatusEntity : StatusEntity
    {
        public PlainStatusEntity(string text)
        {
            Text = text;
        }

        public override StatusType Type => StatusType.Plain;

        public string Text { get; }
    }
}
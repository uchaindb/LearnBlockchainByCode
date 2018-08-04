namespace UChainDB.Example.Chain.Entity
{
    public class NodeOptions
    {
        public string[] WellKnownNodes { get; set; } = new[] {
            "localhost:3030",
            "localhost:3031",
        };
    }
}
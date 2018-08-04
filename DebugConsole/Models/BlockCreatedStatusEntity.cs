using UChainDB.Example.Chain.Entity;

namespace UChainDB.Example.Chain.DebugConsole.Models
{
    public class BlockCreatedStatusEntity : PlainStatusEntity
    {
        public BlockCreatedStatusEntity(Block block, string text)
            : base(text)
        {
            this.Block = block;
        }

        public override StatusType Type => StatusType.BlockCreated;

        public Block Block { get; }
    }
}
using UChainDB.Example.Chain.Entity;

namespace DebugConsole.Models
{
    public class BlockCreatedStatusEntity : PlainStatusEntity
    {
        public BlockCreatedStatusEntity(Block block, string text)
            : base(text)
        {
            Block = block;
        }

        public override StatusType Type => StatusType.BlockCreated;

        public Block Block { get; }
    }
}
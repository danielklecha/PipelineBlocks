namespace PipelineBlocks.Models;

public interface IChildBlock : IBlock, IExecutableBlock
{
    bool SetParent(IParentBlock? parent);
}

public interface IChildBlock<T> : IChildBlock, IBlock<T>
{
}

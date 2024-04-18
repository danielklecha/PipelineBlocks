namespace PipelineBlocks.Models;

public interface IChildBlock : IBlock, IExecutableBlock
{
    /// <summary>
    /// Set parent of the block
    /// </summary>
    /// <param name="parent"></param>
    /// <returns></returns>
    bool SetParent(IParentBlock? parent);
}

public interface IChildBlock<T> : IChildBlock, IBlock<T>
{
}

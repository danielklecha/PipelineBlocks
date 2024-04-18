namespace PipelineBlocks.Models;

public interface IParentBlock : IBlock, IExecutableBlock
{
    /// <summary>
    /// Reset the block
    /// </summary>
    void Reset();
    /// <summary>
    /// Set child of the block using function
    /// </summary>
    /// <param name="setter"></param>
    /// <returns></returns>
    bool SetChild(Func<IBlock, IChildBlock?> setter);
    /// <summary>
    /// Set child of the block using instance
    /// </summary>
    /// <param name="block"></param>
    /// <returns></returns>
    bool SetChild(IChildBlock? block);
}

public interface IParentBlock<T> : IParentBlock, IBlock<T>
{
    /// <summary>
    /// Set child of the block using generic function
    /// </summary>
    /// <param name="setter"></param>
    /// <returns></returns>
    bool SetChild(Func<IBlock<T>, IChildBlock?> setter);
}
namespace PipelineBlocks;

public interface IParentBlock : IBlock, IExecutableBlock
{
    void ResetData();
    bool SetChild(Func<IBlock, IChildBlock?> setter);
    bool SetChild(IChildBlock? block);
}

public interface IParentBlock<T> : IParentBlock, IBlock<T>
{
    bool SetChild(Func<IBlock<T>, IChildBlock?> setter);
}
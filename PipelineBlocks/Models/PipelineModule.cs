namespace PipelineBlocks.Models;

public class PipelineModule(IChildBlock startBlock, IParentBlock endBlock) : IPipelineModule
{
    public object? Data => startBlock.Data;

    public string? Name => startBlock.Name;

    public string? Key => startBlock.Key;

    public bool IsCheckpoint => startBlock.IsCheckpoint;

    public bool HasExit => startBlock.HasExit;

    public IBlock? Parent => startBlock.Parent;

    public IBlock? Child => endBlock.Child;

    public bool IsCompleted => startBlock.IsCompleted;

    public Task<bool> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        return startBlock.ExecuteAsync(cancellationToken);
    }

    void IParentBlock.Reset()
    {
        endBlock.Reset();
    }

    bool IParentBlock.SetChild(Func<IBlock, IChildBlock?> setter)
    {
        return endBlock.SetChild(setter);
    }

    bool IParentBlock.SetChild(IChildBlock? block)
    {
        return endBlock.SetChild(block);
    }

    bool IChildBlock.SetParent(IParentBlock? parent)
    {
        return startBlock.SetParent(parent);
    }
}
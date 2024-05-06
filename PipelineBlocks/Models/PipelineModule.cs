using PipelineBlocks.Extensions;

namespace PipelineBlocks.Models;

public class PipelineModule(IChildBlock startBlock, IParentBlock endBlock) : IPipelineModule
{
    public object? Data => endBlock.Data;

    public string? Name => endBlock.Name;

    public string? Key => endBlock.Key;

    public bool IsCheckpoint => Enumerable.Repeat(endBlock, 1).Concat(endBlock.EnumerateAncestors()).TakeWhile(x => x != startBlock.Parent).Any(x => x.IsCheckpoint);

    public bool HasExit => Enumerable.Repeat(endBlock, 1).Concat(endBlock.EnumerateAncestors()).TakeWhile(x => x != startBlock.Parent).Any(x => x.HasExit);

    public IBlock? Parent => startBlock.Parent;

    public IBlock? Child => endBlock.Child;

    public bool IsCompleted => endBlock.IsCompleted;

    public string? StateMessage => endBlock.StateMessage;

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
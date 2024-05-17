using PipelineBlocks.Extensions;

namespace PipelineBlocks.Models;

public class PipelineBlock<T> : IPipelineBlock<T>
{
    private IParentBlock? _parent;
    public Func<IBlock<T>, IChildBlock?>? ChildCondition { private get; set; }
    public Func<IBlock<T>, CancellationToken, Task<BlockResult<T>>>? Job { private get; set; }
    public Func<IBlock<T>, string?>? KeyCondition { private get; set; }
    public Func<IBlock<T>, string?>? NameCondition { private get; set; }
    public Func<IBlock<T>, bool?>? CheckpointCondition { private get; set; }
    public Func<IBlock<T>, bool?>? ExitCondition { private get; set; }
    public T? Data { get; private set; }
    public bool HasExit => (ExitCondition?.Invoke(this) ?? false) || Child is null;

    public string? Name => NameCondition?.Invoke(this);

    public string? Key => KeyCondition?.Invoke(this);

    public bool IsCheckpoint => Parent is null || (CheckpointCondition?.Invoke(this) ?? false);

    public IBlock? Parent => _parent;

    public IBlock? Child => ChildCondition?.Invoke(this);

    public async Task<BlockResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        if (!this.IsActive())
            return BlockResult.Error("Block is not active");
        if (Job is null)
        {
            IsCompleted = true;
            return BlockResult.Error("No job");
        }
        var result = await Job.Invoke(this, cancellationToken);
        return result.ResultType switch
        {
            BlockResultType.Exit => await ExitAsync(result, cancellationToken),
            BlockResultType.Forward => await ForwardAsync(result, cancellationToken),
            BlockResultType.BackToCheckpoint => await BackToCheckpointAsync(result, cancellationToken),
            BlockResultType.BackToExit => await BackToExitAsync(result, cancellationToken),
            BlockResultType.Skip => await SkipAsync(cancellationToken),
            _ => result
        };
    }

    private Task<BlockResult> ExitAsync(BlockResult<T> result, CancellationToken cancellationToken)
    {
        if (!HasExit)
            return Task.FromResult(BlockResult.Error("No Exit"));
        Data = result.Data;
        IsCompleted = true;
        return Task.FromResult(BlockResult.Completed());
    }

    public bool IsCompleted { get; private set; }

    object? IBlock.Data => Data;

    private async Task<BlockResult> BackToCheckpointAsync(BlockResult result, CancellationToken cancellationToken)
    {
        var targetDescendant = this.EnumerateAncestors().OfType<IParentBlock>().FirstOrDefault(x => x.IsCheckpoint && (result.Key == null || string.Equals(result.Key, x.Key, StringComparison.OrdinalIgnoreCase)));
        if (targetDescendant == null)
            return BlockResult.Error("Unable to find checkpoint");
        (this as IParentBlock).Reset();
        foreach (var descendant in this.EnumerateAncestors().OfType<IParentBlock>().TakeWhile(x => x != targetDescendant))
            descendant.Reset();
        return await targetDescendant.ExecuteAsync(cancellationToken);
    }

    private Task<BlockResult> BackToExitAsync(BlockResult result, CancellationToken cancellationToken)
    {
        var targetAncestor = this.EnumerateAncestors().OfType<IParentBlock>().FirstOrDefault(x => x.HasExit && (result.Key == null || string.Equals(result.Key, x.Key, StringComparison.OrdinalIgnoreCase)));
        if (targetAncestor == null)
            return Task.FromResult(BlockResult.Error("Unable to find exit"));
        (this as IParentBlock).Reset();
        foreach (var descendant in this.EnumerateAncestors().OfType<IParentBlock>().TakeWhile(x => x != targetAncestor))
            descendant.Reset();
        return Task.FromResult(BlockResult.Completed());
    }

    private async Task<BlockResult> ForwardAsync(BlockResult<T> result, CancellationToken cancellationToken)
    {
        Data = result.Data;
        IsCompleted = true;
        var child = ChildCondition?.Invoke(this);
        if (child == null)
            return BlockResult.Completed();
        if (child == this)
            return BlockResult.Error("Child is current block");
        if (!child.SetParent(this))
            return BlockResult.Error("Unable to set child's parent");
        return await child.ExecuteAsync(cancellationToken);
    }

    void IParentBlock.Reset()
    {
        Data = default;
        IsCompleted = false;
    }

    bool IChildBlock.SetParent(IParentBlock? parent)
    {
        if (!this.IsActive())
            return false;
        _parent = parent;
        return true;
    }

    private async Task<BlockResult> SkipAsync(CancellationToken cancellationToken)
    {
        (this as IParentBlock).Reset();
        var child = ChildCondition?.Invoke(this);
        if (child == null)
            return BlockResult.Completed();
        if (child == this || !child.SetParent(_parent))
            return BlockResult.Error("");
        return await child.ExecuteAsync(cancellationToken);
    }

    bool IParentBlock.SetChild(Func<IBlock, IChildBlock?> setter)
    {
        if (!this.IsActive())
            return false;
        ChildCondition = x => setter.Invoke(this);
        return true;
    }

    public bool SetChild(IChildBlock? block)
    {
        if (!this.IsActive())
            return false;
        ChildCondition = x => block;
        return true;
    }

    public bool SetChild(Func<IBlock<T>, IChildBlock?> setter)
    {
        if (!this.IsActive())
            return false;
        ChildCondition = x => setter.Invoke(this);
        return true;
    }
}
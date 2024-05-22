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

    async Task<BlockResult> IExecutableBlock.ExecuteSelfAsync(CancellationToken cancellationToken)
    {
        if (!this.IsActive())
            return BlockResult.Error("Block is not active");
        if (Job is null)
        {
            IsCompleted = true;
            return BlockResult.Error("No job");
        }
        BlockResult<T>? result = await Job.Invoke(this, cancellationToken);
        return result is null
            ? BlockResult.Error("Job returned null")
            : result.ResultType switch
            {
                BlockResultType.Exit or BlockResultType.Completed => Exit(result),
                BlockResultType.Forward => Forward(result),
                BlockResultType.BackToCheckpoint => BackToCheckpoint(result),
                BlockResultType.BackToExit => BackToExit(result),
                BlockResultType.Skip => Skip(),
                _ => result
            };
    }

    public async Task<BlockResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        BlockResult? result = await (this as IExecutableBlock).ExecuteSelfAsync(cancellationToken);
        while (true)
        {
            if (result is null)
                return BlockResult.Error("Block returned null");
            switch (result.ResultType)
            {
                case BlockResultType.Execute when result.Data is IExecutableBlock block:
                    if (block == this)
                        return BlockResult.Error("Child is current block");
                    result = await block.ExecuteSelfAsync();
                    break;
                default:
                    return result;
            }
        }
    }

    private BlockResult Exit(BlockResult<T> result)
    {
        if (!HasExit)
            return BlockResult.Error("No Exit");
        Data = result.Data;
        IsCompleted = true;
        return BlockResult.Completed("Exit result");
    }

    public bool IsCompleted { get; private set; }

    object? IBlock.Data => Data;

    private BlockResult BackToCheckpoint(BlockResult result)
    {
        IParentBlock targetDescendant = this.EnumerateAncestors().OfType<IParentBlock>().FirstOrDefault(x => x.IsCheckpoint && (result.Key == null || string.Equals(result.Key, x.Key, StringComparison.OrdinalIgnoreCase)));
        if (targetDescendant == null)
            return BlockResult.Error("Unable to find checkpoint");
        (this as IParentBlock).Reset();
        foreach (IParentBlock? descendant in this.EnumerateAncestors().OfType<IParentBlock>().TakeWhile(x => x != targetDescendant))
            descendant.Reset();
        return BlockResult.Execute<IExecutableBlock>(targetDescendant);
    }

    private BlockResult BackToExit(BlockResult result)
    {
        IParentBlock targetAncestor = this.EnumerateAncestors().OfType<IParentBlock>().FirstOrDefault(x => x.HasExit && (result.Key == null || string.Equals(result.Key, x.Key, StringComparison.OrdinalIgnoreCase)));
        if (targetAncestor == null)
            return BlockResult.Error("Unable to find exit");
        (this as IParentBlock).Reset();
        foreach (IParentBlock? descendant in this.EnumerateAncestors().OfType<IParentBlock>().TakeWhile(x => x != targetAncestor))
            descendant.Reset();
        return BlockResult.Completed("Back to exit result");
    }

    private BlockResult Forward(BlockResult<T> result)
    {
        Data = result.Data;
        IsCompleted = true;
        IChildBlock? child = ChildCondition?.Invoke(this);
        return child == null
            ? BlockResult.Completed("Reached end of the pipeline")
            : child == this
            ? BlockResult.Error("Child is current block")
            : !child.SetParent(this)
            ? BlockResult.Error("Unable to set child's parent")
            : BlockResult.Execute<IExecutableBlock>(child);
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

    private BlockResult Skip()
    {
        (this as IParentBlock).Reset();
        IChildBlock? child = ChildCondition?.Invoke(this);
        return child == null
            ? BlockResult.Completed("Reached end of the pipeline")
            : child == this
            ? BlockResult.Error("Child is current block")
            : !child.SetParent(_parent)
            ? BlockResult.Error("Unable to set child's parent")
            : BlockResult.Execute<IExecutableBlock>(child);
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
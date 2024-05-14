using PipelineBlocks.Extensions;

namespace PipelineBlocks.Models;

public class PipelineBlock<T> : IPipelineBlock<T>, IActiveBlock<T>
{
    private IParentBlock? _parent;
    public Func<IBlock<T>, IChildBlock?>? ChildCondition { private get; set; }
    public Func<IActiveBlock<T>, CancellationToken, Task>? Job { private get; set; }
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

    public async Task<bool> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        if (!this.IsActive())
            return false;
        if (Job is null)
        {
            IsCompleted = true;
            return false;
        }
        await Job.Invoke(this, cancellationToken);
        return true;
    }

    Task<bool> IActiveBlock<T>.ExitAsync(T? data, CancellationToken cancellationToken)
    {
        if (!this.IsActive() || !HasExit)
            return Task.FromResult(false);
        Data = data;
        IsCompleted = true;
        return Task.FromResult(true);
    }

    public bool IsCompleted { get; private set; }

    object? IBlock.Data => Data;

    public string? StateMessage { get; private set; }

    async Task<bool> IActiveBlock.BackToCheckpointAsync(string? key, CancellationToken cancellationToken)
    {
        if (!this.IsActive())
            return false;
        var targetDescendant = this.EnumerateAncestors().OfType<IParentBlock>().FirstOrDefault(x => x.IsCheckpoint && (key == null || string.Equals(key, x.Key, StringComparison.OrdinalIgnoreCase)));
        if (targetDescendant == null)
            return false;
        (this as IParentBlock).Reset();
        foreach (var descendant in this.EnumerateAncestors().OfType<IParentBlock>().TakeWhile(x => x != targetDescendant))
            descendant.Reset();
        return await targetDescendant.ExecuteAsync(cancellationToken);
    }

    Task<bool> IActiveBlock.BackToExitAsync(string? key, CancellationToken cancellationToken)
    {
        if (!this.IsActive())
            return Task.FromResult(false);
        var targetAncestor = this.EnumerateAncestors().OfType<IParentBlock>().FirstOrDefault(x => x.HasExit && (key == null || string.Equals(key, x.Key, StringComparison.OrdinalIgnoreCase)));
        if (targetAncestor == null)
            return Task.FromResult(false);
        (this as IParentBlock).Reset();
        foreach (var descendant in this.EnumerateAncestors().OfType<IParentBlock>().TakeWhile(x => x != targetAncestor))
            descendant.Reset();
        return Task.FromResult(true);
    }

    async Task<bool> IActiveBlock<T>.ForwardAsync(T? data, CancellationToken cancellationToken)
    {
        if (!this.IsActive())
            return false;
        Data = data;
        IsCompleted = true;
        var child = ChildCondition?.Invoke(this);
        if (child == null)
            return true;
        if (child == this || !child.SetParent(this))
            return false;
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

    async Task<bool> IActiveBlock.SkipAsync(CancellationToken cancellationToken)
    {
        if (!this.IsActive())
            return false;
        (this as IParentBlock).Reset();
        var child = ChildCondition?.Invoke(this);
        if (child == null)
            return true;
        if (child == this || !child.SetParent(_parent))
            return false;
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

    public bool SetStateMessage(string? message)
    {
        if (!this.IsActive())
            return false;
        StateMessage = message;
        return true;
    }
}
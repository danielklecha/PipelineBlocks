using PipelineBlocks.Extensions;

namespace PipelineBlocks.Models;

public class PipelineBlock<T> : IPipelineBlock<T>
{
    private bool _isCompleted = false;
    private IParentBlock? _parent;
    private T? _data;
    private Func<IBlock<T>, IChildBlock?>? _childCondition;
    public Func<IBlock<T>, IChildBlock?>? ChildCondition { init => _childCondition = value; }
    public Func<IActiveBlock<T>, CancellationToken, Task>? Job { private get; init; }
    public Func<IBlock<T>, string?>? KeyCondition { private get; init; }
    public Func<IBlock<T>, string?>? NameCondition { private get; init; }
    public Func<IBlock<T>, bool?>? CheckpointCondition { private get; init; }
    public Func<IBlock<T>, bool?>? ExitCondition { private get; init; }
    public T? Data => _data;
    public bool HasExit => (ExitCondition?.Invoke(this) ?? false) || Child is null;

    public string? Name => NameCondition?.Invoke(this);

    public string? Key => KeyCondition?.Invoke(this);

    public bool IsCheckpoint => Parent is null || (CheckpointCondition?.Invoke(this) ?? false);

    public IBlock? Parent => _parent;

    public IBlock? Child => _childCondition?.Invoke(this);

    public async Task<bool> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        if (_isCompleted)
            return false;
        if (Job is null)
        {
            _isCompleted = true;
            return false;
        }
        await Job.Invoke(this, cancellationToken);
        return true;
    }

    Task<bool> IActiveBlock<T>.ExitAsync(T? data, CancellationToken cancellationToken)
    {
        if (_isCompleted || !HasExit)
            return Task.FromResult(false);
        _data = data;
        _isCompleted = true;
        return Task.FromResult(true);
    }

    public bool IsCompleted => _isCompleted;

    object? IBlock.Data => _data;

    async Task<bool> IActiveBlock.BackToCheckpointAsync(string? key, CancellationToken cancellationToken)
    {
        if (_isCompleted)
            return false;
        var targetDescendant = this.EnumerateDescendants().OfType<IParentBlock>().FirstOrDefault(x => x.IsCheckpoint && (key == null || string.Equals(key, x.Key, StringComparison.OrdinalIgnoreCase)));
        if (targetDescendant == null)
            return false;
        (this as IParentBlock).ResetData();
        foreach (var descendant in this.EnumerateDescendants().OfType<IParentBlock>().TakeWhile(x => x != targetDescendant))
            descendant.ResetData();
        return await targetDescendant.ExecuteAsync(cancellationToken);
    }

    Task<bool> IActiveBlock.BackToExitAsync(string? key, CancellationToken cancellationToken)
    {
        if (_isCompleted)
            return Task.FromResult(false);
        var targetDescendant = this.EnumerateDescendants().OfType<IParentBlock>().FirstOrDefault(x => x.HasExit && (key == null || string.Equals(key, x.Key, StringComparison.OrdinalIgnoreCase)));
        if (targetDescendant == null)
            return Task.FromResult(false);
        (this as IParentBlock).ResetData();
        foreach (var descendant in this.EnumerateDescendants().OfType<IParentBlock>().TakeWhile(x => x != targetDescendant))
            descendant.ResetData();
        return Task.FromResult(true);
    }

    async Task<bool> IActiveBlock<T>.ForwardAsync(T? data, CancellationToken cancellationToken)
    {
        if (_isCompleted)
            return false;
        _data = data;
        _isCompleted = true;
        var child = _childCondition?.Invoke(this);
        if (child == null)
            return true;
        if (child == this || child.IsCompleted)
            return false;
        child.SetParent(this);
        return await child.ExecuteAsync(cancellationToken);
    }

    void IParentBlock.ResetData()
    {
        _data = default;
        _isCompleted = false;
    }

    bool IChildBlock.SetParent(IParentBlock? parent)
    {
        if (_isCompleted)
            return false;
        _parent = parent;
        return true;
    }

    async Task<bool> IActiveBlock.SkipAsync(CancellationToken cancellationToken)
    {
        if (_isCompleted)
            return false;
        (this as IParentBlock).ResetData();
        var child = _childCondition?.Invoke(this);
        if (child == null)
            return true;
        child.SetParent(_parent);
        return await child.ExecuteAsync(cancellationToken);
    }

    bool IParentBlock.SetChild(Func<IBlock, IChildBlock?> setter)
    {
        if (_isCompleted)
            return false;
        _childCondition = x => setter.Invoke(this);
        return true;
    }

    public bool SetChild(IChildBlock? block)
    {
        if (_isCompleted)
            return false;
        _childCondition = x => block;
        return true;
    }

    public bool SetChild(Func<IBlock<T>, IChildBlock?> setter)
    {
        if (_isCompleted)
            return false;
        _childCondition = x => setter.Invoke(this);
        return true;
    }
}
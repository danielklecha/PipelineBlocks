namespace PipelineBlocks;

public class PipelineBlock : IPipelineBlock
{
    private bool _isCompleted = false;
    /// <summary>
    /// Name of current block
    /// </summary>
    public virtual string? Name => NameCondition?.Invoke( this );
    public virtual string? Key => KeyCondition?.Invoke( this );
    public virtual bool IsCheckpoint => !HasParent || (CheckpointCondition?.Invoke( this ) ?? false);
    public virtual Func<IReadOnlyPipelineBlock, string?>? KeyCondition { get; init; }
    public virtual Func<IReadOnlyPipelineBlock, string?>? NameCondition { get; init; }
    public virtual Func<IReadOnlyPipelineBlock, bool?>? CheckpointCondition { get; init; }
    public virtual Func<IReadOnlyPipelineBlock, bool?>? ExitCondition { get; init; }
    public virtual object? Data { get; internal set; }
    public virtual IPipelineBlock? Parent { get; set; }
    IReadOnlyPipelineBlock? IReadOnlyPipelineModule.Parent => Parent;
    public virtual Func<PipelineBlock, CancellationToken, Task>? Job { get; init; }
    public virtual bool HasParent => Parent != null;
    public virtual Func<IReadOnlyPipelineBlock, IPipelineModule?>? ChildCondition { get; set; }

    public PipelineBlock()
    {
    }

    public PipelineBlock( object data, Func<IReadOnlyPipelineBlock, IPipelineModule?>? childCondition ) : this( (x, c) => { x.Data = data; return Task.CompletedTask; }, childCondition )
    {

    }

    public PipelineBlock( Func<PipelineBlock, CancellationToken, Task> func, Func<IReadOnlyPipelineBlock, IPipelineModule?>? childCondition )
    {
        Job = func;
        ChildCondition = childCondition;
    }

    public virtual bool HasExit => (ExitCondition?.Invoke( this ) ?? false) || ChildCondition?.Invoke( this ) == null;

    public virtual async Task<bool> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        if (_isCompleted || Job == null)
            return false;
        await Job.Invoke(this, cancellationToken);
        //if (!HasParent && !IsCompleted)
        //    throw new Exception( "Pipeline completed with invalid state" );
        return true;
    }

    public virtual bool ResetData()
    {
        if (_isCompleted)
            return false;
        Data = default;
        return true;
    }

    public IEnumerable<IPipelineBlock> Descendants
    {
        get
        {
            var parent = Parent;
            while (parent != null)
            {
                yield return parent;
                parent = parent.Parent;
            }
        }
    }

    public virtual async Task<bool> GoForwardAsync(object? data = default, CancellationToken cancellationToken = default)
    {
        if (_isCompleted)
            return false;
        Data = data;
        var child = ChildCondition?.Invoke( this );
        if (child == null)
        {
            MarkAsCompleted();
            return true;
        }
        if (child != this && !Descendants.OfType<IPipelineModule>().Contains( child ))
            child.Parent = this;
        await child.ExecuteAsync(cancellationToken);
        return true;
    }

    public virtual Task<bool> ExitAsync(object? data = default, CancellationToken cancellationToken = default)
    {
        if (_isCompleted || !HasExit)
            return Task.FromResult( false );
        Data = data;
        MarkAsCompleted();
        return Task.FromResult( true );
    }

    public virtual async Task<bool> SkipAsync(CancellationToken cancellationToken = default)
    {
        if (_isCompleted)
            return false;
        ResetData();
        var child = ChildCondition?.Invoke( this );
        if (child == null)
        {
            Parent?.MarkAsCompleted();
            return true;
        }
        child.Parent = Parent;
        await child.ExecuteAsync(cancellationToken);
        return true;
    }

    public string? Path => string.Join( "\\", Descendants.Reverse().Concat( Enumerable.Repeat( this, 1 ) ).Select( x => x.Name ).Where( x => !string.IsNullOrEmpty( x ) ) );

    public bool MarkAsCompleted()
    {
        if (_isCompleted)
            return false;
        Parent?.MarkAsCompleted();
        _isCompleted = true;
        return true;
    }

    public virtual Task<bool> GoBackToExitAsync(string? key = default, CancellationToken cancellationToken = default)
    {
        if (_isCompleted)
            return Task.FromResult(false);
        var targetDescendant = Descendants.FirstOrDefault( x => x.HasExit && (x.Key?.Equals( key, StringComparison.OrdinalIgnoreCase ) ?? true) );
        if (targetDescendant == null)
            return Task.FromResult( false );
        ResetData();
        foreach (var descendant in Descendants.TakeWhile( x => x != targetDescendant ))
            descendant.ResetData();
        targetDescendant.MarkAsCompleted();
        return Task.FromResult( true );
    }

    public virtual async Task<bool> GoBackToCheckpointAsync(string? key = default, CancellationToken cancellationToken = default)
    {
        if (_isCompleted)
            return false;
        var targetDescendant = Descendants.FirstOrDefault( x => x.IsCheckpoint && (x.Key?.Equals( key, StringComparison.OrdinalIgnoreCase ) ?? true) );
        if (targetDescendant == null)
            return false;
        ResetData();
        foreach (var descendant in Descendants.TakeWhile( x => x != targetDescendant ))
            descendant.ResetData();
        await targetDescendant.ExecuteAsync(cancellationToken);
        return true;
    }

    public bool IsCompleted => _isCompleted;
}

public class PipelineBlock<T> : PipelineBlock, IPipelineBlock<T>
{
    public new Func<IReadOnlyPipelineBlock<T>, string?>? NameCondition
    {
        get => base.NameCondition;
        init => base.NameCondition = x => x is IReadOnlyPipelineBlock<T> t ? value?.Invoke( t ) : null;
    }

    public new Func<IReadOnlyPipelineBlock<T>, bool?>? CheckpointCondition
    {
        get => x => base.CheckpointCondition?.Invoke( x );
        init => base.CheckpointCondition = x => x is IReadOnlyPipelineBlock<T> t ? value?.Invoke( t ) : false;
    }

    public new Func<IReadOnlyPipelineBlock<T>, string?>? KeyCondition
    {
        get => x => base.KeyCondition?.Invoke( x );
        init => base.KeyCondition = x => x is IReadOnlyPipelineBlock<T> t ? value?.Invoke( t ) : null;
    }

    public new Func<IReadOnlyPipelineBlock<T>, bool?>? ExitCondition
    {
        get => x => base.ExitCondition?.Invoke( x );
        init => base.ExitCondition = x => x is IReadOnlyPipelineBlock<T> t ? value?.Invoke( t ) : null;
    }

    public new T? Data
    {
        get => base.Data is T data ? data : default;
        set => base.Data = value;
    }

    public new Func<PipelineBlock<T>, CancellationToken, Task>? Job
    {
        get => base.Job;
        init => base.Job = (x,c) => x is PipelineBlock<T> t && value != null ? value.Invoke( t, c ) : Task.CompletedTask;
    }

    public new Func<IReadOnlyPipelineBlock<T>, IPipelineModule?>? ChildCondition
    {
        get => base.ChildCondition;
        set => base.ChildCondition = x => x is IReadOnlyPipelineBlock<T> t ? value?.Invoke( t ) : null;
    }

    public PipelineBlock()
    {

    }

    public PipelineBlock( Func<PipelineBlock<T>, CancellationToken, Task> func )
    {
        ResetData();
        Job = func;
    }

    public PipelineBlock( T data ) : this( (x,c) => { x.Data = data; return Task.CompletedTask; } )
    {
    }

    public override bool ResetData()
    {
        if (IsCompleted)
            return false;
        Data = default;
        return true;
    }

    public override async Task<bool> GoForwardAsync(object? data = default, CancellationToken cancellationToken = default) => data is T && await base.GoForwardAsync( data,cancellationToken );

    public virtual Task<bool> GoForwardAsync(T? data = default, CancellationToken cancellationToken = default) => base.GoForwardAsync(data, cancellationToken);

    public override async Task<bool> ExitAsync(object? data = default, CancellationToken cancellationToken = default) => data is T && await base.ExitAsync( data,cancellationToken );

    public virtual Task<bool> ExitAsync(T? data = default, CancellationToken cancellationToken = default) => base.ExitAsync(data, cancellationToken);
}
namespace PipelineBlocks;

public class PipelineBlock : IPipelineBlock
{
    /// <summary>
    /// Name of current block
    /// </summary>
    public virtual string? Name => NameCondition?.Invoke( this );
    public virtual string? Key => KeyCondition?.Invoke( this );
    public virtual bool IsCheckpoint => !HasParent || (CheckpointCondition?.Invoke( this ) ?? false);
    public virtual Func<IReadOnlyPipelineBlock, string?>? KeyCondition { get; init; }
    public virtual Func<IReadOnlyPipelineBlock, string?>? NameCondition { get; init; }
    public virtual Func<IReadOnlyPipelineBlock, bool?>? CheckpointCondition { get; internal set; }
    public virtual Func<IReadOnlyPipelineBlock, bool?>? ExitCondition { get; init; }
    public virtual object? Data { get; internal set; }
    public virtual IPipelineBlock? Parent { get; set; }
    IReadOnlyPipelineBlock? IReadOnlyPipelineModule.Parent => Parent;
    public virtual Func<PipelineBlock, Task>? Job { get; init; }
    public virtual bool HasParent => Parent != null;
    public virtual Func<IReadOnlyPipelineBlock, IPipelineModule?>? ChildCondition { get; set; }

    public PipelineBlock()
    {
    }

    public PipelineBlock( object data, Func<IReadOnlyPipelineBlock, IPipelineModule?>? childCondition ) : this( x => { x.Data = data; return Task.CompletedTask; }, childCondition )
    {

    }

    public PipelineBlock( Func<PipelineBlock, Task> func, Func<IReadOnlyPipelineBlock, IPipelineModule?>? childCondition )
    {
        Job = func;
        ChildCondition = childCondition;
    }

    public virtual bool HasExit => (ExitCondition?.Invoke( this ) ?? false) || ChildCondition?.Invoke( this ) == null;

    public virtual async Task ExecuteAsync()
    {
        if (Job != null)
            await Job.Invoke( this );
        //if (!HasParent && !IsCompleted)
        //    throw new Exception( "Pipeline completed with invalid state" );
    }

    public virtual void ResetData()
    {
        Data = default;
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

    public virtual Task<bool> GoBackToExitAsync() => GoBackToExitAsync( null );

    public virtual Task<bool> GoBackToCheckpointAsync() => GoBackToCheckpointAsync( null );

    public virtual async Task<bool> GoForwardAsync() => await GoForwardAsync( default );

    public virtual async Task<bool> GoForwardAsync( object? data )
    {
        Data = data;
        var child = ChildCondition?.Invoke( this );
        if (child == null)
        {
            MarkAsCompleted();
            return true;
        }
        if (child != this && !Descendants.OfType<IPipelineModule>().Contains( child ))
            child.Parent = this;
        await child.ExecuteAsync();
        return true;
    }

    public virtual Task<bool> GoToExitAsync() => GoToExitAsync( default );

    public virtual Task<bool> GoToExitAsync( object? data )
    {
        if (!HasExit)
            return Task.FromResult( false );
        Data = data;
        MarkAsCompleted();
        return Task.FromResult( true );
    }

    public virtual async Task<bool> SkipAndGoForwardAsync()
    {
        ResetData();
        var child = ChildCondition?.Invoke( this );
        if (child == null)
        {
            Parent?.MarkAsCompleted();
            return true;
        }
        child.Parent = Parent;
        await child.ExecuteAsync();
        return true;
    }

    public string? Path => string.Join( "\\", Descendants.Reverse().Concat( Enumerable.Repeat( this, 1 ) ).Select( x => x.Name ).Where( x => !string.IsNullOrEmpty( x ) ) );

    public void MarkAsCompleted()
    {
        IsCompleted = true;
        foreach (var descendant in Descendants)
            descendant.IsCompleted = true;
    }

    public virtual Task<bool> GoBackToExitAsync( string? key )
    {
        var targetDescendant = Descendants.FirstOrDefault( x => x.HasExit && (x.Key?.Equals( key, StringComparison.OrdinalIgnoreCase ) ?? true) );
        if (targetDescendant == null)
            return Task.FromResult( false );
        ResetData();
        foreach (var descendant in Descendants.TakeWhile( x => x != targetDescendant ))
            descendant.ResetData();
        targetDescendant.MarkAsCompleted();
        return Task.FromResult( true );
    }

    public virtual async Task<bool> GoBackToCheckpointAsync( string? key )
    {
        var targetDescendant = Descendants.FirstOrDefault( x => x.IsCheckpoint && (x.Key?.Equals( key, StringComparison.OrdinalIgnoreCase ) ?? true) );
        if (targetDescendant == null)
            return false;
        ResetData();
        foreach (var descendant in Descendants.TakeWhile( x => x != targetDescendant ))
            descendant.ResetData();
        await targetDescendant.ExecuteAsync();
        return true;
    }

    public bool IsCompleted { get; set; }
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
        internal set => base.CheckpointCondition = x => x is IReadOnlyPipelineBlock<T> t ? value?.Invoke( t ) : false;
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

    public new Func<PipelineBlock<T>, Task>? Job
    {
        get => base.Job;
        init => base.Job = x => x is PipelineBlock<T> t && value != null ? value.Invoke( t ) : Task.CompletedTask;
    }

    public new Func<IReadOnlyPipelineBlock<T>, IPipelineModule?>? ChildCondition
    {
        get => base.ChildCondition;
        set => base.ChildCondition = x => x is IReadOnlyPipelineBlock<T> t ? value?.Invoke( t ) : null;
    }

    public PipelineBlock()
    {

    }

    public PipelineBlock( Func<PipelineBlock<T>, Task> func )
    {
        ResetData();
        Job = func;
    }

    public PipelineBlock( T data ) : this( x => { x.Data = data; return Task.CompletedTask; } )
    {
    }

    public override void ResetData()
    {
        Data = default;
    }

    public override Task<bool> GoForwardAsync() => GoForwardAsync( default );

    public override async Task<bool> GoForwardAsync( object? data ) => data is T && await base.GoForwardAsync( data );

    public virtual Task<bool> GoForwardAsync( T? data ) => base.GoForwardAsync( data );

    public override Task<bool> GoToExitAsync() => GoToExitAsync( default );

    public override async Task<bool> GoToExitAsync( object? data ) => data is T && await base.GoToExitAsync( data );

    public virtual Task<bool> GoToExitAsync( T? data ) => base.GoToExitAsync( data );
}
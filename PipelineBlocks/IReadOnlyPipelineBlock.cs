namespace PipelineBlocks;

public interface IReadOnlyPipelineBlock : IReadOnlyPipelineModule
{
    /// <summary>
    /// Name of current block
    /// </summary>
    string? Name { get; }
    string? Key { get; }
    /// <summary>
    /// Checkpoint allow to go back from ancestors blocks
    /// </summary>
    bool IsCheckpoint { get; }
    Func<IReadOnlyPipelineBlock, string?>? KeyCondition { get; }
    Func<IReadOnlyPipelineBlock, string?>? NameCondition { get; }
    Func<IReadOnlyPipelineBlock, bool?>? CheckpointCondition { get; }
    Func<IReadOnlyPipelineBlock, bool?>? ExitCondition { get; }
    object? Data { get; }
    string? Path { get; }
    /// <summary>
    /// Exit allow to escape pipeline before end
    /// </summary>
    public virtual bool HasExit => (ExitCondition?.Invoke( this ) ?? false) || ChildCondition?.Invoke( this ) == null;
}

public interface IReadOnlyPipelineBlock<T> : IReadOnlyPipelineBlock
{
    new T? Data { get; }
    new Func<IReadOnlyPipelineBlock<T>, string?>? NameCondition { get; }
    new Func<IReadOnlyPipelineBlock<T>, bool?>? CheckpointCondition { get; }
    new Func<IReadOnlyPipelineBlock<T>, IPipelineModule?>? ChildCondition { get; }
    new Func<IReadOnlyPipelineBlock<T>, string?>? KeyCondition { get; }
    new Func<IReadOnlyPipelineBlock<T>, bool?>? ExitCondition { get; }
}

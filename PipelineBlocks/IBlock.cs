namespace PipelineBlocks;

public interface IBlock<T> : IBlock
{
    /// <summary>
    /// Data associated with the block
    /// </summary>
    new T? Data { get; }
}

public interface IBlock
{
    object? Data { get; }
    /// <summary>
    /// Name of current block
    /// </summary>
    string? Name { get; }
    string? Key { get; }
    /// <summary>
    /// Checkpoint allow to go back from ancestors blocks
    /// </summary>
    bool IsCheckpoint { get; }
    /// <summary>
    /// Exit allow to escape pipeline before end
    /// </summary>
    bool HasExit { get; }
    IBlock? Parent { get; }
    IBlock? Child { get; }
    bool IsCompleted { get; }
    /// <summary>
    /// Full path
    /// </summary>
    string? Path { get; }
}

namespace PipelineBlocks.Models;

public interface IBlock<T> : IBlock
{
    /// <summary>
    /// Data associated with the block
    /// </summary>
    new T? Data { get; }
}

public interface IBlock
{
    /// <summary>
    /// Data associated with the block
    /// </summary>
    object? Data { get; }
    /// <summary>
    /// Name of the block
    /// </summary>
    string? Name { get; }
    /// <summary>
    /// Key of the block
    /// </summary>
    string? Key { get; }
    /// <summary>
    /// Checkpoint allow to go back to this block from ancestors blocks
    /// </summary>
    bool IsCheckpoint { get; }
    /// <summary>
    /// Exit allow to escape pipeline before end
    /// </summary>
    bool HasExit { get; }
    /// <summary>
    /// Parent of the block
    /// </summary>
    IBlock? Parent { get; }
    /// <summary>
    /// Child of the block
    /// </summary>
    IBlock? Child { get; }
    /// <summary>
    /// Completed block was processed and not skipped
    /// </summary>
    bool IsCompleted { get; }
}

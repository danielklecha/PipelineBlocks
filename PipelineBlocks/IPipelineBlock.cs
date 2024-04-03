namespace PipelineBlocks;

public interface IPipelineBlock : IReadOnlyPipelineBlock, IPipelineModule
{
    /// <summary>
    /// Reset block data
    /// </summary>
    void ResetData();
    /// <summary>
    /// Go back to exit with specific key
    /// </summary>
    /// <returns>information about success</returns>
    Task<bool> GoBackToExitAsync( string? key = default, CancellationToken cancellationToken = default);
    /// <summary>
    /// Go back to checkpoint with specific key
    /// </summary>
    /// <returns>information about success</returns>
    Task<bool> GoBackToCheckpointAsync(string? key = default, CancellationToken cancellationToken = default);
    /// <summary>
    /// Go forward with data
    /// </summary>
    /// <returns>information about success</returns>
    Task<bool> GoForwardAsync( object? data = default, CancellationToken cancellationToken = default);
    /// <summary>
    /// Go back to exit with specific key
    /// </summary>
    /// <returns>information about success</returns>
    Task<bool> GoBackToExitAsync( object? data = default, CancellationToken cancellationToken = default);
    /// <summary>
    /// Skip current block and go forward
    /// </summary>
    /// <returns>information about success</returns>
    Task<bool> SkipAndGoForwardAsync(CancellationToken cancellationToken = default);
    //new IPipelineBlock? Parent { get; set; }
    void MarkAsCompleted();
}

public interface IPipelineBlock<T> : IPipelineBlock, IReadOnlyPipelineBlock<T>
{
    Task<bool> GoForwardAsync(T? data = default, CancellationToken cancellationToken = default);
    Task<bool> GoBackToExitAsync(T? data = default, CancellationToken cancellationToken = default);
}

namespace PipelineBlocks.Models;

public interface IActiveBlock : IBlock
{
    /// <summary>
    /// Go back to exit with specific key
    /// </summary>
    /// <returns>information about success</returns>
    Task<bool> BackToExitAsync(string? key = default, CancellationToken cancellationToken = default);
    /// <summary>
    /// Go back to checkpoint with specific key
    /// </summary>
    /// <returns>information about success</returns>
    Task<bool> BackToCheckpointAsync(string? key = default, CancellationToken cancellationToken = default);
    /// <summary>
    /// Skip current block and go forward
    /// </summary>
    /// <returns>information about success</returns>
    Task<bool> SkipAsync(CancellationToken cancellationToken = default);
}

public interface IActiveBlock<T> : IActiveBlock, IBlock<T>
{
    /// <summary>
    /// Go forward with data
    /// </summary>
    /// <returns>information about success</returns>
    Task<bool> ForwardAsync(T? data = default, CancellationToken cancellationToken = default);
    /// <summary>
    /// Exit with data
    /// </summary>
    /// <returns>information about success</returns>
    Task<bool> ExitAsync(T? data = default, CancellationToken cancellationToken = default);
}
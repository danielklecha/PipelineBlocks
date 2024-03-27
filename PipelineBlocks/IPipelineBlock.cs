namespace PipelineBlocks;

public interface IPipelineBlock : IReadOnlyPipelineBlock, IPipelineModule
{
    /// <summary>
    /// Reset block data
    /// </summary>
    void ResetData();
    /// <summary>
    /// Go back to last exit
    /// </summary>
    /// <returns>information about success</returns>
    Task<bool> GoBackToExitAsync();
    /// <summary>
    /// Go back to exit with specific key
    /// </summary>
    /// <returns>information about success</returns>
    Task<bool> GoBackToExitAsync( string? key );
    /// <summary>
    /// Go back to last checkpoint
    /// </summary>
    /// <returns>information about success</returns>
    Task<bool> GoBackToCheckpointAsync();
    /// <summary>
    /// Go back to checkpoint with specific key
    /// </summary>
    /// <returns>information about success</returns>
    Task<bool> GoBackToCheckpointAsync( string? key );
    /// <summary>
    /// Go forward without data
    /// </summary>
    /// <returns>information about success</returns>
    Task<bool> GoForwardAsync();
    /// <summary>
    /// Go forward with data
    /// </summary>
    /// <returns>information about success</returns>
    Task<bool> GoForwardAsync( object? data );
    /// <summary>
    /// Go back to last exit
    /// </summary>
    /// <returns>information about success</returns>
    Task<bool> GoToExitAsync();
    /// <summary>
    /// Go back to exit with specific key
    /// </summary>
    /// <returns>information about success</returns>
    Task<bool> GoToExitAsync( object? data );
    /// <summary>
    /// Skip current block and go forward
    /// </summary>
    /// <returns>information about success</returns>
    Task<bool> SkipAndGoForwardAsync();
    new bool IsCompleted { get; set; }
    //new IPipelineBlock? Parent { get; set; }
    void MarkAsCompleted();
}

public interface IPipelineBlock<T> : IPipelineBlock, IReadOnlyPipelineBlock<T>
{
    Task<bool> GoForwardAsync( T? data );
    Task<bool> GoToExitAsync( T? data );
}

namespace PipelineBlocks.Models;

public interface IExecutableBlock
{
    /// <summary>
    /// Process the block and all desendants
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<BlockResult> ExecuteAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Process the block without descendants
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<BlockResult> ExecuteSelfAsync(CancellationToken cancellationToken = default);
}

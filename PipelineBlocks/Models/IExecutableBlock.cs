namespace PipelineBlocks.Models;

public interface IExecutableBlock
{
    /// <summary>
    /// Process the block
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<bool> ExecuteAsync(CancellationToken cancellationToken = default);
}

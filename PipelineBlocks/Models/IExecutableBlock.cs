namespace PipelineBlocks.Models;

public interface IExecutableBlock
{
    Task<bool> ExecuteAsync(CancellationToken cancellationToken = default);
}

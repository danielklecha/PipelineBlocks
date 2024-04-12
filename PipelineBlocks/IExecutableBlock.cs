namespace PipelineBlocks;

public interface IExecutableBlock
{
    Task<bool> ExecuteAsync(CancellationToken cancellationToken = default);
}

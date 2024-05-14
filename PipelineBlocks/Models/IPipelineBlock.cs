namespace PipelineBlocks.Models;

public interface IPipelineBlock : IPipelineModule
{
}

public interface IPipelineBlock<T> : IPipelineBlock, IParentBlock<T>, IChildBlock<T>
{
}

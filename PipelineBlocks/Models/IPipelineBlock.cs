namespace PipelineBlocks.Models;

public interface IPipelineBlock : IActiveBlock, IPipelineModule
{
}

public interface IPipelineBlock<T> : IPipelineBlock, IActiveBlock<T>, IParentBlock<T>, IChildBlock<T>
{
}

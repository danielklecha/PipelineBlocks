namespace PipelineBlocks.Models;

public interface IPipelineBlock : IActiveBlock, IChildBlock, IParentBlock, IPipelineModule
{
}

public interface IPipelineBlock<T> : IPipelineBlock, IActiveBlock<T>, IParentBlock<T>, IChildBlock<T>
{
}

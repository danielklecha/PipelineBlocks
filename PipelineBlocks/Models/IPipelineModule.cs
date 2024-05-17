namespace PipelineBlocks.Models;

public interface IPipelineModule : IChildBlock, IParentBlock
{
}

public interface IPipelineModule<T> : IPipelineModule, IChildBlock, IParentBlock<T>
{
}

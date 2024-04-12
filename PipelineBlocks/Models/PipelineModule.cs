namespace PipelineBlocks.Models;

public class PipelineModule(IPipelineBlock startBlock, IPipelineBlock endBlock)
{
    public IParentBlock EndBlock => endBlock;
    public IChildBlock StartBlock => startBlock;
}

public class PipelineModule<TStart, TEnd>(IPipelineBlock<TStart> startBlock, IPipelineBlock<TEnd> endBlock)
{
    public IParentBlock<TEnd> EndBlock => endBlock;
    public IChildBlock<TStart> StartBlock => startBlock;
}
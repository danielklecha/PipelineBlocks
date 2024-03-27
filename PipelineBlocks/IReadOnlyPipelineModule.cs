namespace PipelineBlocks;

public interface IReadOnlyPipelineModule
{
    Func<IReadOnlyPipelineBlock, IPipelineModule?>? ChildCondition { get; }
    IReadOnlyPipelineBlock? Parent { get; }
    bool HasParent => Parent != null;
    IEnumerable<IPipelineBlock> Descendants { get; }
    bool IsCompleted { get; }
}

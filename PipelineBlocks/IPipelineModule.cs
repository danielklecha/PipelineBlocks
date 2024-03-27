namespace PipelineBlocks;

public interface IPipelineModule : IReadOnlyPipelineModule
{
    new Func<IReadOnlyPipelineBlock, IPipelineModule?>? ChildCondition { get; set; }
    new IPipelineBlock? Parent { get; set; }
    Task ExecuteAsync();
}

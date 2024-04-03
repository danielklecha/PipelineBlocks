namespace PipelineBlocks;

public class PipelineModule(IPipelineModule startBlock, params IPipelineModule[] endBlocks) : IPipelineModule
{
    public bool HasParent => startBlock.HasParent;

    public Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        return startBlock.ExecuteAsync(cancellationToken);
    }

    public Func<IReadOnlyPipelineBlock, IPipelineModule?>? ChildCondition
    {
        get => endBlocks[ 0 ].ChildCondition;
        set => endBlocks[ 0 ].ChildCondition = value;
    }

    public IEnumerable<IPipelineBlock> Descendants => startBlock.Descendants;

    public IPipelineBlock? Parent
    {
        get => startBlock.Parent;
        set => startBlock.Parent = value;
    }

    public bool IsCompleted => startBlock.IsCompleted;

    IReadOnlyPipelineBlock? IReadOnlyPipelineModule.Parent => Parent;

    public bool SetChildCondition( int index, Func<IReadOnlyPipelineBlock, PipelineBlock?>? condition )
    {
        if (index < 0 || index >= endBlocks.Length)
            return false;
        endBlocks[ index ].ChildCondition = condition;
        return true;
    }

    public static PipelineModule Merge( params IPipelineModule[] modules )
    {
        if (modules.Length == 0)
            throw new ArgumentException( "Expected at least one element" );
        for (var i = 1; i < modules.Length; i++)
        {
            var currentModule = modules[ i ];
            modules[ i - 1 ].ChildCondition = x => currentModule;
        }
        return new PipelineModule( modules.First(), modules.Last() );
    }
}

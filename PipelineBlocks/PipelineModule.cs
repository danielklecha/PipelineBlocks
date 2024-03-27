namespace PipelineBlocks;

public class PipelineModule : IPipelineModule
{
    private readonly IPipelineModule _startBlock;
    private readonly IPipelineModule[] _endBlocks;

    public bool HasParent => _startBlock.HasParent;

    public Task ExecuteAsync()
    {
        return _startBlock.ExecuteAsync();
    }

    public PipelineModule( IPipelineModule startBlock, params IPipelineModule[] endBlocks )
    {
        _startBlock = startBlock;
        _endBlocks = endBlocks;
    }

    public Func<IReadOnlyPipelineBlock, IPipelineModule?>? ChildCondition
    {
        get => _endBlocks[ 0 ].ChildCondition;
        set => _endBlocks[ 0 ].ChildCondition = value;
    }

    public IEnumerable<IPipelineBlock> Descendants => _startBlock.Descendants;

    public IPipelineBlock? Parent
    {
        get => _startBlock.Parent;
        set => _startBlock.Parent = value;
    }

    public bool IsCompleted => _startBlock.IsCompleted;

    IReadOnlyPipelineBlock? IReadOnlyPipelineModule.Parent => Parent;

    public bool SetChildCondition( int index, Func<IReadOnlyPipelineBlock, PipelineBlock?>? condition )
    {
        if (index < 0 || index >= _endBlocks.Length)
            return false;
        _endBlocks[ index ].ChildCondition = condition;
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

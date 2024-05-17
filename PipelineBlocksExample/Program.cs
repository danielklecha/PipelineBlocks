// See https://aka.ms/new-console-template for more information
using PipelineBlocks.Extensions;
using PipelineBlocks.Models;

Console.WriteLine( "Hello, World!" );

PipelineBlock<object> GetPipelineBlock()
{
    return new PipelineBlock<object>()
    {
        Job = (x,c) =>
        {
            Console.WriteLine( x.GetPath() );
            switch (Convert.ToInt32( Console.ReadLine() ))
            {
                case -1:
                    Console.WriteLine( "GoBackToCheckpointAsync" );
                    return Task.FromResult(BlockResult<object>.BackToCheckpoint());
                case -2:
                    Console.WriteLine( "GoBackToExitAsync" );
                    return Task.FromResult(BlockResult<object>.BackToExit());
                case 1:
                    Console.WriteLine( "GoForwardAsync" );
                    return Task.FromResult(BlockResult.Forward<object>(123));
                case 2:
                    Console.WriteLine( "SkipAsync" );
                    return Task.FromResult(BlockResult<object>.Skip());
                default:
                    return Task.FromResult(BlockResult<object>.Error());
            };
        },
        NameCondition = x => "Block"
    };
}
var block = GetPipelineBlock();
block.SetChild(GetPipelineBlock());
block.SetChild(GetPipelineBlock());
block.SetChild(GetPipelineBlock());
await block.ExecuteAsync();
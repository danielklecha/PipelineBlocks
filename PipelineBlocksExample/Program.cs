// See https://aka.ms/new-console-template for more information
using PipelineBlocks.Extensions;
using PipelineBlocks.Models;

Console.WriteLine( "Hello, World!" );

PipelineBlock<object> GetPipelineBlock()
{
    return new PipelineBlock<object>()
    {
        Job = async (x,c) =>
        {
            Console.WriteLine( x.GetPath() );
            switch (Convert.ToInt32( Console.ReadLine() ))
            {
                case -1:
                    Console.WriteLine( "GoBackToCheckpointAsync" );
                    await x.BackToCheckpointAsync(cancellationToken: c);
                    break;
                case -2:
                    Console.WriteLine( "GoBackToExitAsync" );
                    await x.BackToExitAsync(cancellationToken: c);
                    break;
                case 1:
                    Console.WriteLine( "GoForwardAsync" );
                    await x.ForwardAsync(cancellationToken: c);
                    break;
                case 2:
                    Console.WriteLine( "SkipAsync" );
                    await x.SkipAsync(c);
                    break;
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
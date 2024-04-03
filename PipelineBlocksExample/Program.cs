// See https://aka.ms/new-console-template for more information
using PipelineBlocks;

Console.WriteLine( "Hello, World!" );

PipelineBlock GetPipelineBlock()
{
    return new PipelineBlock()
    {
        Job = async (x,c) =>
        {
            Console.WriteLine( x.Path );
            switch (Convert.ToInt32( Console.ReadLine() ))
            {
                case -1:
                    Console.WriteLine( "GoBackToCheckpointAsync" );
                    await x.GoBackToCheckpointAsync();
                    break;
                case -2:
                    Console.WriteLine( "GoBackToExitAsync" );
                    await x.GoBackToExitAsync();
                    break;
                case 1:
                    Console.WriteLine( "GoForwardAsync" );
                    await x.GoForwardAsync();
                    break;
                case 2:
                    Console.WriteLine( "SkipAndGoForwardAsync" );
                    await x.SkipAndGoForwardAsync();
                    break;
            };
        },
        NameCondition = x => "Block"
    };
}


var module = PipelineModule.Merge(
    GetPipelineBlock(),
    GetPipelineBlock(),
    GetPipelineBlock(),
    GetPipelineBlock() );
await module.ExecuteAsync();
using PipelineBlocks.Models;

namespace PipelineBlocks.Extensions;

public static class ParentBlockExtensions
{
    public static void SetDescendants(this IParentBlock block, params IPipelineBlock[] ancestors)
    {
        var parent = block;
        foreach (var ancestor in ancestors)
        {
            parent.SetChild(ancestor);
            parent = ancestor;
        }
    }
}

using PipelineBlocks.Models;

namespace PipelineBlocks.Extensions;

public static class PipelineModuleExtensions
{
    /// <summary>
    /// Set two-direction links
    /// </summary>
    /// <param name="block"></param>
    /// <param name="descendants"></param>
    /// <returns></returns>
    public static bool SetLinks(this IPipelineModule block, params IPipelineModule[] descendants)
    {
        return block.SetDescendants(descendants)
            && descendants.Last().SetAncestors(descendants.Reverse().Skip(1).Concat(Enumerable.Repeat(block, 1)).ToArray());
    }
}

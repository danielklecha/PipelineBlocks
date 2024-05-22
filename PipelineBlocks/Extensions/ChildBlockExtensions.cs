using PipelineBlocks.Models;

namespace PipelineBlocks.Extensions;

public static class ChildBlockExtensions
{
    /// <summary>
    /// Set one-direction links from child to parent
    /// </summary>
    /// <param name="block"></param>
    /// <param name="ancestors"></param>
    /// <returns></returns>
    public static bool SetAncestors(this IChildBlock block, params IPipelineModule[] ancestors)
    {
        IChildBlock child = block;
        foreach (IPipelineModule ancestor in ancestors)
        {
            if (!child.SetParent(ancestor))
                return false;
            child = ancestor;
        }
        return true;
    }
}

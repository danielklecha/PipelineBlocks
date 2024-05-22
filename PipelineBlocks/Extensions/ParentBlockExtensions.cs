using PipelineBlocks.Models;

namespace PipelineBlocks.Extensions;

public static class ParentBlockExtensions
{
    /// <summary>
    /// Set one-direction links from parent to child
    /// </summary>
    /// <param name="block"></param>
    /// <param name="descendants"></param>
    /// <returns></returns>
    public static bool SetDescendants(this IParentBlock block, params IPipelineModule[] descendants)
    {
        IParentBlock parent = block;
        foreach (IPipelineModule descendant in descendants)
        {
            if (!parent.SetChild(descendant))
                return false;
            parent = descendant;
        }
        return true;
    }
}

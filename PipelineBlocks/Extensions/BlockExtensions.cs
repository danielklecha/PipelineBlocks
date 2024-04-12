using PipelineBlocks.Models;

namespace PipelineBlocks.Extensions;

public static class BlockExtensions
{
    public static IEnumerable<IBlock> EnumerateDescendants(this IBlock block)
    {
        var parent = block.Parent;
        while (parent != null)
        {
            yield return parent;
            parent = parent.Parent;
        }
    }

    public static T1? GetDescendantData<T1>(this IBlock block, string? key = null)
    {
        return block
            .EnumerateDescendants()
            .Where(x => x is IBlock<T1> || x.Data is T1)
            .Where(x => key == null || string.Equals(key, x.Key, StringComparison.OrdinalIgnoreCase))
            .Take(1)
            .Select(x => x switch { IBlock<T1> block => block.Data, _ => x.Data })
            .OfType<T1>()
            .FirstOrDefault();
    }

    public static string GetPath(this IBlock block)
    {
        return string.Join("\\", block.EnumerateDescendants().Reverse().Concat(Enumerable.Repeat(block, 1)).Select(x => x.Name).Where(x => !string.IsNullOrEmpty(x)));
    }
}

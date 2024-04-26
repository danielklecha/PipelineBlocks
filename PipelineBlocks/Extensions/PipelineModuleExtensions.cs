using PipelineBlocks.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        if(!block.SetDescendants(descendants))
            return false;
        return descendants.Last().SetAncestors(descendants.Reverse().Skip(1).Concat(Enumerable.Repeat(block, 1)).ToArray());
    }
}

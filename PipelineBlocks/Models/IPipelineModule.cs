using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipelineBlocks.Models;

public interface IPipelineModule : IChildBlock, IParentBlock
{
}

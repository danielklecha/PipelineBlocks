using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PipelineBlocks.Models;
using System.Diagnostics.CodeAnalysis;

namespace PipelineBlocksTests.Models;

[TestClass()]
[ExcludeFromCodeCoverage]
public class BlockResultTests
{
    [TestMethod()]
    public void Factory_Forward_DataShouldBeSet()
    {
        BlockResult<int> result = BlockResult.Forward(0);
        result.Data.Should().Be(0);
        result.As<BlockResult>().Data.Should().Be(0);
    }
}

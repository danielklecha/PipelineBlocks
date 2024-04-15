using FluentAssertions.Execution;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PipelineBlocks.Models;
using System.Diagnostics.CodeAnalysis;

namespace PipelineBlocks.Extensions.Tests;

[TestClass()]
[ExcludeFromCodeCoverage]
public class ParentBlockExtensionsTests
{
    [TestMethod()]
    public void SetAncestors_TwoAncestors_ShouldBeSuccess()
    {
        // arrange
        var block1 = new Mock<IPipelineBlock>();
        var block2 = new Mock<IPipelineBlock>();
        var block3 = new Mock<IPipelineBlock>();
        // act
        block1.Object.SetAncestors(block2.Object, block3.Object);
        // assert
        using var _ = new AssertionScope();
        block1.Verify(x => x.SetChild(block2.Object), Times.Once());
        block2.Verify(x => x.SetChild(block3.Object), Times.Once());
    }
}
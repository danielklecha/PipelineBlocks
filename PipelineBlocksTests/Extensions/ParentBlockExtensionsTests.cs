using FluentAssertions;
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
    public void SetDescendants_TwoDescendants_ShouldBeSuccess()
    {
        // arrange
        Mock<IPipelineBlock> block1 = new();
        block1.Setup(x => x.SetChild(It.IsAny<IPipelineBlock>())).Returns(true);
        Mock<IPipelineBlock> block2 = new();
        block2.Setup(x => x.SetChild(It.IsAny<IPipelineBlock>())).Returns(true);
        Mock<IPipelineBlock> block3 = new();
        // act
        block1.Object.SetDescendants(block2.Object, block3.Object);
        using AssertionScope assertionScope = new();
        block1.Verify(x => x.SetChild(block2.Object), Times.Once());
        block2.Verify(x => x.SetChild(block3.Object), Times.Once());
    }

    [TestMethod()]
    public void SetDescendants_CompletedDescendant_ShouldBeSuccess()
    {
        // arrange
        Mock<IPipelineBlock> block1 = new();
        Mock<IPipelineBlock> block2 = new();
        block2.Setup(x => x.SetChild(It.IsAny<IPipelineBlock>())).Returns(false);
        // act
        Func<bool> act = () => block1.Object.SetDescendants(block2.Object);
        // assert
        act.Should().NotThrow().Which.Should().BeFalse();
    }
}
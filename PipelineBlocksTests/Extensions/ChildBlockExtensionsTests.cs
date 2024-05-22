using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PipelineBlocks.Models;
using System.Diagnostics.CodeAnalysis;

namespace PipelineBlocks.Extensions.Tests;

[TestClass()]
[ExcludeFromCodeCoverage]
public class ChildBlockExtensionsTests
{
    [TestMethod()]
    public void SetAncestors_TwoAncestor_ShouldBeSuccess()
    {
        // arrange
        Mock<IPipelineBlock> block1 = new();
        Mock<IPipelineBlock> block2 = new();
        block2.Setup(x => x.SetParent(It.IsAny<IPipelineBlock>())).Returns(true);
        Mock<IPipelineBlock> block3 = new();
        block3.Setup(x => x.SetParent(It.IsAny<IPipelineBlock>())).Returns(true);
        // act
        block3.Object.SetAncestors(block2.Object, block1.Object);
        using AssertionScope assertionScope = new();
        block3.Verify(x => x.SetParent(block2.Object), Times.Once());
        block2.Verify(x => x.SetParent(block1.Object), Times.Once());
    }

    [TestMethod()]
    public void SetAncestors_CompletedAncestor_ShouldBeSuccess()
    {
        // arrange
        Mock<IPipelineBlock> block1 = new();
        Mock<IPipelineBlock> block2 = new();
        block2.Setup(x => x.SetParent(It.IsAny<IPipelineBlock>())).Returns(false);
        // act
        Func<bool> act = () => block2.Object.SetAncestors(block1.Object);
        // assert
        act.Should().NotThrow().Which.Should().BeFalse();
    }
}
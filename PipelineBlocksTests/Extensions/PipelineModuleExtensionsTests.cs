using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PipelineBlocks.Models;
using System.Diagnostics.CodeAnalysis;

namespace PipelineBlocks.Extensions.Tests;

[TestClass()]
[ExcludeFromCodeCoverage]
public class PipelineModuleExtensionsTests
{
    [TestMethod()]
    public void SetLinks_TwoDescendants_ShouldBeSuccess()
    {
        // arrange
        Mock<IPipelineModule> block1 = new();
        block1.Setup(x => x.SetChild(It.IsAny<IPipelineModule>())).Returns(true);
        Mock<IPipelineModule> block2 = new();
        block2.Setup(x => x.SetChild(It.IsAny<IPipelineModule>())).Returns(true);
        block1.Setup(x => x.SetParent(It.IsAny<IPipelineModule>())).Returns(true);
        Mock<IPipelineModule> block3 = new();
        block3.Setup(x => x.SetParent(It.IsAny<IPipelineModule>())).Returns(true);
        // act
        block1.Object.SetLinks(block2.Object, block3.Object);
        using AssertionScope assertionScope = new();
        block1.Verify(x => x.SetChild(block2.Object), Times.Once());
        block2.Verify(x => x.SetChild(block3.Object), Times.Once());
        block2.Verify(x => x.SetParent(block1.Object), Times.Once());
        block3.Verify(x => x.SetParent(block2.Object), Times.Once());
    }

    [TestMethod()]
    public void SetLinks_CompletedDescendant_ShouldBeSuccess()
    {
        // arrange
        Mock<IPipelineModule> block1 = new();
        Mock<IPipelineModule> block2 = new();
        block2.Setup(x => x.SetChild(It.IsAny<IPipelineModule>())).Returns(false);
        // act
        Func<bool> act = () => block1.Object.SetLinks(block2.Object);
        // assert
        act.Should().NotThrow().Which.Should().BeFalse();
    }
}
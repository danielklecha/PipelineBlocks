using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PipelineBlocks.Extensions;
using PipelineBlocks.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipelineBlocks.Extensions.Tests;

[TestClass()]
[ExcludeFromCodeCoverage]
public class PipelineModuleExtensionsTests
{
    [TestMethod()]
    public void SetLinks_TwoDescendants_ShouldBeSuccess()
    {
        // arrange
        var block1 = new Mock<IPipelineModule>();
        block1.Setup(x => x.SetChild(It.IsAny<IPipelineModule>())).Returns(true);
        var block2 = new Mock<IPipelineModule>();
        block2.Setup(x => x.SetChild(It.IsAny<IPipelineModule>())).Returns(true);
        block1.Setup(x => x.SetParent(It.IsAny<IPipelineModule>())).Returns(true);
        var block3 = new Mock<IPipelineModule>();
        block3.Setup(x => x.SetParent(It.IsAny<IPipelineModule>())).Returns(true);
        // act
        block1.Object.SetLinks(block2.Object, block3.Object);
        // assert
        using var _ = new AssertionScope();
        block1.Verify(x => x.SetChild(block2.Object), Times.Once());
        block2.Verify(x => x.SetChild(block3.Object), Times.Once());
        block2.Verify(x => x.SetParent(block1.Object), Times.Once());
        block3.Verify(x => x.SetParent(block2.Object), Times.Once());
    }

    [TestMethod()]
    public void SetLinks_CompletedDescendant_ShouldBeSuccess()
    {
        // arrange
        var block1 = new Mock<IPipelineModule>();
        var block2 = new Mock<IPipelineModule>();
        block2.Setup(x => x.SetChild(It.IsAny<IPipelineModule>())).Returns(false);
        // act
        Func<bool> act = () => block1.Object.SetLinks(block2.Object);
        // assert
        act.Should().NotThrow().Which.Should().BeFalse();
    }
}
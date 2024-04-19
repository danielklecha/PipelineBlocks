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
public class ChildBlockExtensionsTests
{
    [TestMethod()]
    public void SetAncestors_TwoAncestor_ShouldBeSuccess()
    {
        // arrange
        var block1 = new Mock<IPipelineBlock>();
        var block2 = new Mock<IPipelineBlock>();
        block2.Setup(x => x.SetParent(It.IsAny<IPipelineBlock>())).Returns(true);
        var block3 = new Mock<IPipelineBlock>();
        block3.Setup(x => x.SetParent(It.IsAny<IPipelineBlock>())).Returns(true);
        // act
        block3.Object.SetAncestors(block2.Object, block1.Object);
        // assert
        using var _ = new AssertionScope();
        block3.Verify(x => x.SetParent(block2.Object), Times.Once());
        block2.Verify(x => x.SetParent(block1.Object), Times.Once());
    }

    [TestMethod()]
    public void SetAncestors_CompletedAncestor_ShouldBeSuccess()
    {
        // arrange
        var block1 = new Mock<IPipelineBlock>();
        var block2 = new Mock<IPipelineBlock>();
        block2.Setup(x => x.SetParent(It.IsAny<IPipelineBlock>())).Returns(false);
        // act
        Func<bool> act = () => block2.Object.SetAncestors(block1.Object);
        // assert
        act.Should().NotThrow().Which.Should().BeFalse();
    }
}
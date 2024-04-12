using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PipelineBlocks.Models;
using System.Diagnostics.CodeAnalysis;

namespace PipelineBlocks.Extensions.Tests;

[TestClass()]
[ExcludeFromCodeCoverage]
public class BlockExtensionsTests
{
    [TestMethod()]
    public void EnumerateDescendants_TwoDescendants_ShouldBeSuccess()
    {
        // arrange
        var block1 = new Mock<IParentBlock>();
        var block2 = new Mock<IParentBlock>();
        block2.Setup(x => x.Parent).Returns(block1.Object);
        var block3 = new PipelineBlock<object>();
        (block3 as IChildBlock).SetParent(block2.Object);
        // act & assert
        block3.EnumerateDescendants().Should().Equal(new List<IBlock> { block2.Object, block1.Object });
    }

    [TestMethod()]
    public void GetDescendantData_BaseType_ShouldBeSuccessed()
    {
        // arrange
        var block1 = new Mock<IParentBlock>();
        block1.Setup(x => x.Data).Returns(1);
        var block2 = new Mock<IParentBlock>();
        block2.Setup(x => x.Parent).Returns(block1.Object);
        block2.Setup(x => x.Data).Returns(2);
        // act & assert
        block2.Object.GetDescendantData<int>().Should().Be(1);
    }

    [TestMethod()]
    public void GetDescendantData_BaseTypeWithKey_ShouldBeSuccessed()
    {
        // arrange
        var block1 = new Mock<IParentBlock>();
        block1.Setup(x => x.Key).Returns("block1");
        block1.Setup(x => x.Data).Returns(1);
        var block2 = new Mock<IParentBlock>();
        block2.Setup(x => x.Data).Returns(2);
        block2.Setup(x => x.Parent).Returns(block1.Object);
        var block3 = new Mock<IParentBlock>();
        block3.Setup(x => x.Parent).Returns(block2.Object);
        // act & assert
        block3.Object.GetDescendantData<int>("block1").Should().Be(1);
    }

    [TestMethod()]
    public void GetDescendantData_GenericType_ShouldBeSuccessed()
    {
        // arrange
        var block1 = new Mock<IParentBlock<int>>();
        block1.Setup(x => x.Data).Returns(1);
        var block2 = new Mock<IParentBlock<int>>();
        block2.Setup(x => x.Parent).Returns(block1.Object);
        block2.Setup(x => x.Data).Returns(2);
        // act & assert
        block2.Object.GetDescendantData<int>().Should().Be(1);
    }

    [TestMethod()]
    public void GetDescendantData_GenericTypeWithKey_ShouldBeSuccessed()
    {
        // arrange
        var block1 = new Mock<IParentBlock<int>>();
        block1.Setup(x => x.Key).Returns("block1");
        block1.Setup(x => x.Data).Returns(1);
        var block2 = new Mock<IParentBlock<int>>();
        block2.Setup(x => x.Data).Returns(2);
        block2.Setup(x => x.Parent).Returns(block1.Object);
        var block3 = new Mock<IParentBlock<int>>();
        block3.Setup(x => x.Parent).Returns(block2.Object);
        // act & assert
        block3.Object.GetDescendantData<int>("block1").Should().Be(1);
    }

    [TestMethod()]
    public void GetPath_ThreeBlocks_ShouldBeSet()
    {
        // arrange
        var block1 = new Mock<IPipelineBlock>();
        var block2 = new Mock<IPipelineBlock>();
        var block3 = new Mock<IPipelineBlock>();
        block1.Setup(x => x.Name).Returns("block1");
        block2.Setup(x => x.Name).Returns("block2");
        block2.Setup(x => x.Parent).Returns(block1.Object);
        block3.Setup(x => x.Name).Returns("block3");
        block3.Setup(x => x.Parent).Returns(block2.Object);
        // act & assert
        block3.Object.GetPath().Should().Be("block1\\block2\\block3");
    }
}
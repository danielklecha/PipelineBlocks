using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Diagnostics.CodeAnalysis;

namespace PipelineBlocks.Models.Tests;

[TestClass()]
[ExcludeFromCodeCoverage]
public class PipelineModuleTests
{
    [TestMethod()]
    public void Data_EndBlock_ShouldBeSet()
    {
        // arrange
        int data = 123;
        Mock<IParentBlock> endBlock = new();
        _ = endBlock.Setup(x => x.Data).Returns(data);
        PipelineModule module = new(Mock.Of<IChildBlock>(), endBlock.Object);
        // act && assert
        _ = module.Data.Should().Be(data);
    }

    [TestMethod()]
    public void Data_GenericEndBlock_ShouldBeSet()
    {
        // arrange
        int data = 123;
        Mock<IParentBlock<object>> endBlock = new();
        _ = endBlock.Setup(x => x.Data).Returns(data);
        PipelineModule<object> module = new(Mock.Of<IChildBlock>(), endBlock.Object);
        // act && assert
        _ = module.Data.Should().Be(data);
    }

    [TestMethod()]
    public void Name_EndBlock_ShouldBeSet()
    {
        // arrange
        string name = "endBlock";
        Mock<IParentBlock<object>> endBlock = new();
        _ = endBlock.Setup(x => x.Name).Returns(name);
        PipelineModule<object> module = new(Mock.Of<IChildBlock>(), endBlock.Object);
        // act && assert
        _ = module.Name.Should().Be(name);
    }

    [TestMethod()]
    public void Key_EndBlock_ShouldBeSet()
    {
        // arrange
        string key = "endBlock";
        Mock<IParentBlock<object>> endBlock = new();
        _ = endBlock.Setup(x => x.Key).Returns(key);
        PipelineModule<object> module = new(Mock.Of<IChildBlock>(), endBlock.Object);
        // act && assert
        _ = module.Key.Should().Be(key);
    }

    [TestMethod()]
    public void IsCheckpoint_StartBlock_ShouldBeSet()
    {
        // arrange
        Mock<IChildBlock> startBlock = new();
        _ = startBlock.Setup(x => x.IsCheckpoint).Returns(true);
        Mock<IParentBlock<object>> endBlock = new();
        _ = endBlock.Setup(x => x.Parent).Returns(startBlock.Object);
        PipelineModule<object> module = new(startBlock.Object, endBlock.Object);
        // act && assert
        _ = module.IsCheckpoint.Should().BeTrue();
    }

    [TestMethod()]
    public void IsCheckpoint_EndBlock_ShouldBeSet()
    {
        // arrange
        Mock<IChildBlock> startBlock = new();
        Mock<IParentBlock<object>> endBlock = new();
        _ = endBlock.Setup(x => x.Parent).Returns(startBlock.Object);
        _ = endBlock.Setup(x => x.IsCheckpoint).Returns(true);
        PipelineModule<object> module = new(startBlock.Object, endBlock.Object);
        // act && assert
        _ = module.IsCheckpoint.Should().BeTrue();
    }

    [TestMethod()]
    public void HasExit_StartBlock_ShouldBeSet()
    {
        // arrange
        Mock<IChildBlock> startBlock = new();
        _ = startBlock.Setup(x => x.HasExit).Returns(true);
        Mock<IParentBlock<object>> endBlock = new();
        _ = endBlock.Setup(x => x.Parent).Returns(startBlock.Object);
        PipelineModule<object> module = new(startBlock.Object, endBlock.Object);
        // act && assert
        _ = module.HasExit.Should().BeTrue();
    }

    [TestMethod()]
    public void HasExit_EndBlock_ShouldBeSet()
    {
        // arrange
        Mock<IChildBlock> startBlock = new();
        Mock<IParentBlock<object>> endBlock = new();
        _ = endBlock.Setup(x => x.Parent).Returns(startBlock.Object);
        _ = startBlock.Setup(x => x.HasExit).Returns(true);
        PipelineModule<object> module = new(startBlock.Object, endBlock.Object);
        // act && assert
        _ = module.HasExit.Should().BeTrue();
    }

    [TestMethod()]
    public void Parent_StartBlock_ShouldBeSet()
    {
        // arrange
        IParentBlock parent = Mock.Of<IParentBlock>();
        Mock<IChildBlock> startBlock = new();
        _ = startBlock.Setup(x => x.Parent).Returns(parent);
        PipelineModule<object> module = new(startBlock.Object, Mock.Of<IParentBlock<object>>());
        // act && assert
        _ = module.Parent.Should().Be(parent);
    }

    [TestMethod()]
    public void IsCompleted_EndBlock_ShouldBeSet()
    {
        // arrange
        Mock<IParentBlock<object>> endBlock = new();
        _ = endBlock.Setup(x => x.IsCompleted).Returns(true);
        PipelineModule<object> module = new(Mock.Of<IChildBlock>(), endBlock.Object);
        // act && assert
        _ = module.IsCompleted.Should().BeTrue();
    }

    [TestMethod()]
    public void Child_EndBlock_ShouldBeSet()
    {
        // arrange
        IChildBlock child = Mock.Of<IChildBlock>();
        Mock<IParentBlock<object>> endBlock = new();
        _ = endBlock.Setup(x => x.Child).Returns(child);
        PipelineModule<object> module = new(Mock.Of<IChildBlock>(), endBlock.Object);
        // act && assert
        _ = module.Child.Should().Be(child);
    }

    [TestMethod()]
    public async Task ExecuteAsync_StartBlock_ShouldBeSuccess()
    {
        // arrange
        Mock<IChildBlock> startBlock = new();
        PipelineModule<object> module = new(startBlock.Object, Mock.Of<IParentBlock<object>>());
        // act
        _ = await module.ExecuteAsync();
        // assert
        startBlock.Verify(x => x.ExecuteAsync(It.IsAny<CancellationToken>()), Times.Once());
    }

    [TestMethod()]
    public async Task ExecuteSelfAsync_StartBlock_ShouldBeSuccess()
    {
        // arrange
        Mock<IChildBlock> startBlock = new();
        PipelineModule<object> module = new(startBlock.Object, Mock.Of<IParentBlock<object>>());
        // act
        _ = await (module as IExecutableBlock).ExecuteSelfAsync();
        // assert
        startBlock.Verify(x => x.ExecuteSelfAsync(It.IsAny<CancellationToken>()), Times.Once());
    }

    [TestMethod()]
    public void ResetData_EndBlock_ShouldBeSuccess()
    {
        // arrange
        Mock<IParentBlock<object>> endBlock = new();
        PipelineModule<object> module = new(Mock.Of<IChildBlock>(), endBlock.Object);
        // act
        (module as IParentBlock).Reset();
        // assert
        endBlock.Verify(x => x.Reset(), Times.Once());
    }

    [TestMethod()]
    public void SetChild_ChildBlock_ShouldBeSuccess()
    {
        // arrange
        IChildBlock child = Mock.Of<IChildBlock>();
        Mock<IParentBlock> endBlock = new();
        PipelineModule module = new(Mock.Of<IChildBlock>(), endBlock.Object);
        // act
        _ = module.SetChild(child);
        // assert
        endBlock.Verify(x => x.SetChild(child), Times.Once());
    }

    [TestMethod()]
    public void SetChild_Func_ShouldBeSuccess()
    {
        // arrange
        Func<IBlock, IChildBlock?> func = x => null;
        Mock<IParentBlock> endBlock = new();
        PipelineModule module = new(Mock.Of<IChildBlock>(), endBlock.Object);
        // act
        _ = module.SetChild(func);
        // assert
        endBlock.Verify(x => x.SetChild(func), Times.Once());
    }

    [TestMethod()]
    public void SetChild_GenericFunc_ShouldBeSuccess()
    {
        // arrange
        Func<IBlock, IChildBlock?> func = x => null;
        Mock<IParentBlock<object>> endBlock = new();
        PipelineModule<object> module = new(Mock.Of<IChildBlock>(), endBlock.Object);
        // act
        _ = module.SetChild(func);
        // assert
        endBlock.Verify(x => x.SetChild(func), Times.Once());
    }

    [TestMethod()]
    public void SetParent_ParentBlock_ShouldBeSet()
    {
        // arrange
        IParentBlock parent = Mock.Of<IParentBlock>();
        Mock<IChildBlock> startBlock = new();
        PipelineModule<object> module = new(startBlock.Object, Mock.Of<IParentBlock<object>>());
        // act
        _ = (module as IChildBlock).SetParent(parent);
        // assert
        startBlock.Verify(x => x.SetParent(parent), Times.Once());
    }
}
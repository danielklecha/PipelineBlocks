﻿using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PipelineBlocks.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipelineBlocks.Models.Tests;

[TestClass()]
[ExcludeFromCodeCoverage]
public class PipelineModuleTests
{
    [TestMethod()]
    public void Data_StartBlock_ShouldBeSet()
    {
        // arrange
        var data = 123;
        Mock<IChildBlock> startBlock = new();
        startBlock.Setup(x => x.Data).Returns(data);
        PipelineModule module = new(startBlock.Object, Mock.Of<IParentBlock>());
        // act && assert
        module.Data.Should().Be(data);
    }

    [TestMethod()]
    public void Name_StartBlock_ShouldBeSet()
    {
        // arrange
        var name = "startBlock";
        Mock<IChildBlock> startBlock = new();
        startBlock.Setup(x => x.Name).Returns(name);
        PipelineModule module = new(startBlock.Object, Mock.Of<IParentBlock>());
        // act && assert
        module.Name.Should().Be(name);
    }

    [TestMethod()]
    public void Key_StartBlock_ShouldBeSet()
    {
        // arrange
        var key = "startBlock";
        Mock<IChildBlock> startBlock = new();
        startBlock.Setup(x => x.Key).Returns(key);
        PipelineModule module = new(startBlock.Object, Mock.Of<IParentBlock>());
        // act && assert
        module.Key.Should().Be(key);
    }

    [TestMethod()]
    public void IsCheckpoint_StartBlock_ShouldBeSet()
    {
        // arrange
        Mock<IChildBlock> startBlock = new();
        startBlock.Setup(x => x.IsCheckpoint).Returns(true);
        PipelineModule module = new(startBlock.Object, Mock.Of<IParentBlock>());
        // act && assert
        module.IsCheckpoint.Should().BeTrue();
    }

    [TestMethod()]
    public void HasExit_StartBlock_ShouldBeSet()
    {
        // arrange
        Mock<IChildBlock> startBlock = new();
        startBlock.Setup(x => x.HasExit).Returns(true);
        PipelineModule module = new(startBlock.Object, Mock.Of<IParentBlock>());
        // act && assert
        module.HasExit.Should().BeTrue();
    }

    [TestMethod()]
    public void Parent_StartBlock_ShouldBeSet()
    {
        // arrange
        IParentBlock parent = Mock.Of<IParentBlock>();
        Mock<IChildBlock> startBlock = new();
        startBlock.Setup(x => x.Parent).Returns(parent);
        PipelineModule module = new(startBlock.Object, Mock.Of<IParentBlock>());
        // act && assert
        module.Parent.Should().Be(parent);
    }

    [TestMethod()]
    public void IsCompleted_StartBlock_ShouldBeSet()
    {
        // arrange
        Mock<IChildBlock> startBlock = new();
        startBlock.Setup(x => x.IsCompleted).Returns(true);
        PipelineModule module = new(startBlock.Object, Mock.Of<IParentBlock>());
        // act && assert
        module.IsCompleted.Should().BeTrue();
    }

    [TestMethod()]
    public void Child_EndBlock_ShouldBeSet()
    {
        // arrange
        IChildBlock child = Mock.Of<IChildBlock>();
        Mock<IParentBlock> endBlock = new();
        endBlock.Setup(x => x.Child).Returns(child);
        PipelineModule module = new(Mock.Of<IChildBlock>(), endBlock.Object);
        // act && assert
        module.Child.Should().Be(child);
    }

    [TestMethod()]
    public async Task ExecuteAsync_StartBlock_ShouldBeSuccess()
    {
        // arrange
        Mock<IChildBlock> startBlock = new();
        PipelineModule module = new(startBlock.Object, Mock.Of<IParentBlock>());
        // act
        await module.ExecuteAsync();
        // assert
        startBlock.Verify(x => x.ExecuteAsync(It.IsAny<CancellationToken>()), Times.Once());
    }

    [TestMethod()]
    public void ResetData_EndBlock_ShouldBeSuccess()
    {
        // arrange
        Mock<IParentBlock> endBlock = new();
        PipelineModule module = new(Mock.Of<IChildBlock>(), endBlock.Object);
        // act
        (module as IParentBlock).ResetData();
        // assert
        endBlock.Verify(x => x.ResetData(), Times.Once());
    }

    [TestMethod()]
    public void SetChild_ChildBlock_ShouldBeSuccess()
    {
        // arrange
        IChildBlock child = Mock.Of<IChildBlock>();
        Mock<IParentBlock> endBlock = new();
        PipelineModule module = new(Mock.Of<IChildBlock>(), endBlock.Object);
        // act
        (module as IParentBlock).SetChild(child);
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
        (module as IParentBlock).SetChild(func);
        // assert
        endBlock.Verify(x => x.SetChild(func), Times.Once());
    }

    [TestMethod()]
    public void SetParent_ParentBlock_ShouldBeSet()
    {
        // arrange
        IParentBlock parent = Mock.Of<IParentBlock>();
        Mock<IChildBlock> startBlock = new();
        PipelineModule module = new(startBlock.Object, Mock.Of<IParentBlock>());
        // act
        (module as IChildBlock).SetParent(parent);
        // assert
        startBlock.Verify(x => x.SetParent(parent), Times.Once());
    }
}
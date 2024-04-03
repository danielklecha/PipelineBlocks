using PipelineBlocks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace PipelineBlocks.Tests;

[TestClass()]
public class PipelineBlockTests
{
    [TestMethod()]
    public async Task GoForwardAsync_OneBlock_ShouldBeCompleted()
    {
        // arrange
        var block = new PipelineBlock { Job = (x,c) => x.GoForwardAsync(cancellationToken: c) };
        // act
        await block.ExecuteAsync();
        // assert
        block.IsCompleted.Should().BeTrue();
    }

    [TestMethod()]
    public async Task GoForwardAsync_TwoBlocks_ShouldBeCompleted()
    {
        // arrange
        var block2 = new PipelineBlock { Job = (x, c) => x.GoForwardAsync(cancellationToken: c) };
        var block1 = new PipelineBlock { Job = (x, c) => x.GoForwardAsync(cancellationToken: c), ChildCondition = x => block2 };
        // act
        await block1.ExecuteAsync();
        // assert
        block1.IsCompleted.Should().BeTrue();
        block2.IsCompleted.Should().BeTrue();
    }

    [TestMethod]
    public async Task SkipAndGoForwardAsync_SkipFirst_ShouldBeSkipped()
    {
        // arrange
        var block2 = new PipelineBlock { Job = (x, c) => x.GoForwardAsync(cancellationToken: c) };
        var block1 = new PipelineBlock { Job = (x, c) => x.SkipAsync(cancellationToken: c), ChildCondition = x => block2 };
        // act
        await block1.ExecuteAsync();
        // assert
        block1.IsCompleted.Should().BeFalse();
        block2.IsCompleted.Should().BeTrue();
    }

    [TestMethod]
    public async Task SkipAndGoForwardAsync_SkipMiddle_ShouldBeSkipped()
    {
        // arrange
        var block3 = new PipelineBlock { Job = (x, c) => x.GoForwardAsync(cancellationToken: c) };
        var block2 = new PipelineBlock { Job = (x, c) => x.SkipAsync(cancellationToken: c), ChildCondition = x => block3 };
        var block1 = new PipelineBlock { Job = (x, c) => x.GoForwardAsync(cancellationToken: c), ChildCondition = x => block2 };
        // act
        await block1.ExecuteAsync();
        // assert
        block1.IsCompleted.Should().BeTrue();
        block2.IsCompleted.Should().BeFalse();
        block3.IsCompleted.Should().BeTrue();
    }

    [TestMethod]
    public async Task SkipAndGoForwardAsync_SkipLast_ShouldBeSkipped()
    {
        // arrange
        var block2 = new PipelineBlock { Job = (x, c) => x.SkipAsync(cancellationToken: c) };
        var block1 = new PipelineBlock { Job = (x, c) => x.GoForwardAsync(cancellationToken: c), ChildCondition = x => block2 };
        // act
        await block1.ExecuteAsync();
        // assert
        block1.IsCompleted.Should().BeTrue();
        block2.IsCompleted.Should().BeFalse();
    }

    [TestMethod]
    public async Task GoBackToCheckpointAsync_BackFromSecondToFirst_ShouldBeDone()
    {
        // arrange
        var block1 = new Mock<IPipelineBlock>();
        block1.Setup(x => x.IsCheckpoint).Returns(true);
        var block2 = new PipelineBlock { Job = (x, c) => x.GoBackToCheckpointAsync(cancellationToken: c), Parent = block1.Object };
        // act
        await block2.ExecuteAsync();
        // assert
        block1.Verify(x => x.ExecuteAsync(CancellationToken.None), Times.Once());
    }

    [TestMethod]
    public async Task GoBackToCheckpointAsync_BackFromThirdToFirst_ShouldBeDone()
    {
        // arrange
        var block1 = new Mock<IPipelineBlock>();
        block1.Setup(x => x.IsCheckpoint).Returns(true);
        var block3 = new PipelineBlock { Job = (x, c) => x.GoBackToCheckpointAsync(cancellationToken: c) };
        var block2 = new PipelineBlock { Job = (x, c) => x.GoForwardAsync(cancellationToken: c), Parent = block1.Object, ChildCondition = x => block3 };
        // act
        await block2.ExecuteAsync();
        // assert
        block1.Verify(x => x.ExecuteAsync(CancellationToken.None), Times.Once());
    }

    [TestMethod()]
    public void IsCheckpoint_NoParent_ShouldBeTrue()
    {
        // arrange
        PipelineBlock block = new();
        // act & assert
        block.IsCheckpoint.Should().BeTrue();
    }

    [TestMethod()]
    public async Task IsCheckpoint_HasParentAndCheckpointCondition_ShouldBeTrue()
    {
        // arrange
        PipelineBlock block2 = new()
        {
            CheckpointCondition = x => true
        };
        PipelineBlock block1 = new()
        {
            Job = (x, c) => x.GoForwardAsync(cancellationToken: c),
            ChildCondition = x => block2
        };
        // act
        await block1.ExecuteAsync();
        // assert
        block2.IsCheckpoint.Should().BeTrue();
    }

    [TestMethod()]
    public async Task IsCheckpoint_HasParentButDoesntHaveCheckpointCondition_ShouldBeFalse()
    {
        // arrange
        PipelineBlock block2 = new();
        PipelineBlock block1 = new()
        {
            Job = (x, c) => x.GoForwardAsync(cancellationToken: c),
            CheckpointCondition = x => true,
            ChildCondition = x => block2
        };
        // act
        await block1.ExecuteAsync();
        // assert
        block2.IsCheckpoint.Should().BeFalse();
    }

    [TestMethod()]
    public void ExecuteAsyncTest()
    {
        Assert.Fail();
    }

    [TestMethod()]
    public void ResetDataTest()
    {
        Assert.Fail();
    }

    [TestMethod()]
    public void GoBackToExitAsyncTest()
    {
        Assert.Fail();
    }

    [TestMethod()]
    public void GoBackToCheckpointAsyncTest()
    {
        Assert.Fail();
    }

    [TestMethod()]
    public void GoForwardAsyncTest()
    {
        Assert.Fail();
    }

    [TestMethod()]
    public void GoForwardAsyncTest1()
    {
        Assert.Fail();
    }

    [TestMethod()]
    public void GoToExitAsyncTest()
    {
        Assert.Fail();
    }

    [TestMethod()]
    public void GoToExitAsyncTest1()
    {
        Assert.Fail();
    }

    [TestMethod()]
    public void SkipAndGoForwardAsyncTest()
    {
        Assert.Fail();
    }

    [TestMethod()]
    public void MarkAsCompletedTest()
    {
        Assert.Fail();
    }

    [TestMethod()]
    public void GoBackToExitAsyncTest1()
    {
        Assert.Fail();
    }

    [TestMethod()]
    public void GoBackToCheckpointAsyncTest1()
    {
        Assert.Fail();
    }

    [TestMethod()]
    public void Name_HasNameCondition_ShouldBeSet()
    {
        // arrange
        var name = "myblock";
        PipelineBlock block = new()
        {
            NameCondition = x => name
        };
        // act & assert
        block.Name.Should().Be(name);
    }

    [TestMethod()]
    public void Name_DoesntHaveNameCondition_ShouldBeNull()
    {
        // arrange
        PipelineBlock block = new();
        // act & assert
        block.Name.Should().BeNull();
    }

    [TestMethod()]
    public void Name_HasKeyCondition_ShouldBeSet()
    {
        // arrange
        var key = "myblock";
        PipelineBlock block = new()
        {
            KeyCondition = x => key
        };
        // act & assert
        block.Key.Should().Be(key);
    }

    [TestMethod()]
    public void Name_DoesntHaveKeyCondition_ShouldBeNull()
    {
        // arrange
        PipelineBlock block = new();
        // act & assert
        block.Key.Should().BeNull();
    }

    [TestMethod()]
    public void HasExit_HasExitCondition_ShouldBeTrue()
    {
        // arrange
        PipelineBlock block = new()
        {
            ExitCondition = x => true
        };
        // act & assert
        block.HasExit.Should().BeTrue();
    }

    [TestMethod()]
    public void HasExit_DoesntHaveExitConditionAndHasChild_ShouldBeFalse()
    {
        // arrange
        PipelineBlock block2 = new();
        PipelineBlock block1 = new()
        {
            ChildCondition = x => block2
        };
        // act & assert
        block1.HasExit.Should().BeFalse();
    }

    [TestMethod()]
    public void HasExit_DoesntHaveExitConditionAndDoesntHaveChild_ShouldBeFalse()
    {
        // arrange
        PipelineBlock block1 = new();
        // act & assert
        block1.HasExit.Should().BeFalse();
    }


    [TestMethod()]
    public void PipelineBlockTest1()
    {
        Assert.Fail();
    }

    [TestMethod()]
    public void PipelineBlockTest2()
    {
        Assert.Fail();
    }

    [TestMethod()]
    public void ExecuteAsyncTest1()
    {
        Assert.Fail();
    }

    [TestMethod()]
    public void ResetDataTest1()
    {
        Assert.Fail();
    }

    [TestMethod()]
    public void GoBackToExitAsyncTest2()
    {
        Assert.Fail();
    }

    [TestMethod()]
    public void GoBackToCheckpointAsyncTest2()
    {
        Assert.Fail();
    }

    [TestMethod()]
    public void GoForwardAsyncTest2()
    {
        Assert.Fail();
    }

    [TestMethod()]
    public void GoForwardAsyncTest3()
    {
        Assert.Fail();
    }

    [TestMethod()]
    public void GoToExitAsyncTest2()
    {
        Assert.Fail();
    }

    [TestMethod()]
    public void GoToExitAsyncTest3()
    {
        Assert.Fail();
    }

    [TestMethod()]
    public void SkipAndGoForwardAsyncTest1()
    {
        Assert.Fail();
    }

    [TestMethod()]
    public void MarkAsCompletedTest1()
    {
        Assert.Fail();
    }

    [TestMethod()]
    public void GoBackToExitAsyncTest3()
    {
        Assert.Fail();
    }

    [TestMethod()]
    public void GoBackToCheckpointAsyncTest3()
    {
        Assert.Fail();
    }
}
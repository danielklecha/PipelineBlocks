using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions.Execution;
using PipelineBlocks.Models;

namespace PipelineBlocksTests.Models;

[TestClass()]
[ExcludeFromCodeCoverage]
public class PipelineBlockTests
{
    [TestMethod()]
    public void HasExit_HasExitCondition_ShouldBeTrue()
    {
        // arrange
        PipelineBlock<object> block = new()
        {
            ExitCondition = x => true
        };
        // act & assert
        block.HasExit.Should().BeTrue();
    }

    [TestMethod()]
    public void HasExit_HasChild_ShouldBeFalse()
    {
        // arrange
        PipelineBlock<object> block = new();
        block.SetChild(new PipelineBlock<object>());
        // act & assert
        block.HasExit.Should().BeFalse();
    }

    [TestMethod()]
    public void HasExit_NoExitConditionNoChild_ShouldBeTrue()
    {
        // arrange
        PipelineBlock<object> block = new();
        // act & assert
        block.HasExit.Should().BeTrue();
    }

    [TestMethod()]
    public void Name_HasNameCondition_ShouldBeSet()
    {
        // arrange
        var name = "myblock";
        PipelineBlock<object> block = new()
        {
            NameCondition = x => name
        };
        // act & assert
        block.Name.Should().Be(name);
    }

    [TestMethod()]
    public void Name_NoNameCondition_ShouldBeNull()
    {
        // arrange
        PipelineBlock<object> block = new();
        // act & assert
        block.Name.Should().BeNull();
    }

    [TestMethod()]
    public void Key_HasKeyCondition_ShouldBeSet()
    {
        // arrange
        var key = "myblock";
        PipelineBlock<object> block = new()
        {
            KeyCondition = x => key
        };
        // act & assert
        block.Key.Should().Be(key);
    }

    [TestMethod()]
    public void Key_NoKeyCondition_ShouldBeNull()
    {
        // arrange
        PipelineBlock<object> block = new();
        // act & assert
        block.Key.Should().BeNull();
    }
    [TestMethod()]
    public void IsCheckpoint_NoParentNoCheckpointCondition_ShouldBeTrue()
    {
        // arrange
        PipelineBlock<object> block = new();
        // act & assert
        block.IsCheckpoint.Should().BeTrue();
    }

    [TestMethod()]
    public async Task IsCheckpoint_HasParentHasCheckpointCondition_ShouldBeTrue()
    {
        // arrange
        PipelineBlock<object> block2 = new()
        {
            CheckpointCondition = x => true
        };
        PipelineBlock<object> block1 = new()
        {
            Job = (x, c) => x.ForwardAsync(cancellationToken: c),
            ChildCondition = x => block2
        };
        // act
        await block1.ExecuteAsync();
        // assert
        block2.IsCheckpoint.Should().BeTrue();
    }

    [TestMethod()]
    public async Task IsCheckpoint_HasParentNoCheckpointCondition_ShouldBeFalse()
    {
        // arrange
        PipelineBlock<object> block2 = new()
        {
            Job = (x, c) => x.BackToExitAsync(cancellationToken: c)
        };
        PipelineBlock<object> block1 = new()
        {
            Job = (x, c) => x.ForwardAsync(cancellationToken: c),
            ChildCondition = x => block2
        };
        // act
        await block1.ExecuteAsync();
        // assert
        block2.IsCheckpoint.Should().BeFalse();
    }

    [TestMethod()]
    public async Task ExecuteAsync_IsCompleted_ShouldReturnFalse()
    {
        // arrange
        PipelineBlock<object> block = new()
        {
            Job = (x, c) => x.ForwardAsync(cancellationToken: c)
        };
        // act
        Func<Task<bool>> act = async () =>
        {
            await block.ExecuteAsync();
            return await block.ExecuteAsync();
        };
        // assert
        (await act.Should().NotThrowAsync()).Which.Should().BeFalse();
    }

    [TestMethod()]
    public async Task ExecuteAsync_NoJob_ShouldReturnFalse()
    {
        // arrange
        PipelineBlock<object> block = new();
        // act
        Func<Task<bool>> act = () => block.ExecuteAsync();
        // assert
        (await act.Should().NotThrowAsync()).Which.Should().BeFalse();
    }

    [TestMethod()]
    public async Task ExecuteAsync_HasJob_ShouldReturnTrue()
    {
        // arrange
        PipelineBlock<object> block = new()
        {
            Job = (x, c) => Task.CompletedTask
        };
        // act
        Func<Task<bool>> act = () => block.ExecuteAsync();
        // assert
        (await act.Should().NotThrowAsync()).Which.Should().BeTrue();
    }

    [TestMethod()]
    public void IsCompleted_NotExecuted_ShouldReturnFalse()
    {
        // arrange
        PipelineBlock<object> block = new();
        // act & assert
        block.IsCompleted.Should().BeFalse();
    }

    [TestMethod()]
    public async Task IsCompleted_AfterExecution_ShouldReturnTrue()
    {
        // arrange
        PipelineBlock<object> block = new();
        // act
        await block.ExecuteAsync();
        // assert
        block.IsCompleted.Should().BeTrue();
    }

    [TestMethod()]
    public async Task Data_AfterExecution_ShouldReturnTrue()
    {
        // arrange
        var result = 123;
        PipelineBlock<int> block = new()
        {
            Job = (x, c) => x.ForwardAsync(result, c)
        };
        // act
        await block.ExecuteAsync();
        // assert
        using (new AssertionScope())
        {
            block.Data.Should().Be(result);
            (block as IBlock).Data.Should().Be(result);
        }
    }

    [TestMethod()]
    public async Task ForwardAsync_IsCompleted_ShouldBeCompleted()
    {
        // arrange
        var block = new PipelineBlock<object>() as IActiveBlock<object>;
        // act
        Func<Task<bool>> act = async () =>
        {
            await block.ForwardAsync();
            return await block.ForwardAsync();
        };
        // assert
        (await act.Should().NotThrowAsync()).Which.Should().BeFalse();
    }

    [TestMethod()]
    public async Task ForwardAsync_OneBlock_ShouldBeCompleted()
    {
        // arrange
        var result = 123;
        var block = new PipelineBlock<object>
        {
            Job = (x, c) => x.ForwardAsync(result, c)
        };
        // act
        await block.ExecuteAsync();
        // assert
        using (new AssertionScope())
        {
            block.Data.Should().Be(result);
            block.IsCompleted.Should().BeTrue();
        }
    }

    [TestMethod()]
    public async Task ForwardAsync_TwoBlocks_ShouldBeCompleted()
    {
        // arrange
        var block2 = new PipelineBlock<object>
        {
            Job = (x, c) => x.ForwardAsync(cancellationToken: c)
        };
        var block1 = new PipelineBlock<object>
        {
            Job = (x, c) => x.ForwardAsync(cancellationToken: c),
            ChildCondition = x => block2
        };
        // act
        await block1.ExecuteAsync();
        // assert
        using (new AssertionScope())
        {
            block1.IsCompleted.Should().BeTrue();
            block2.Parent.Should().Be(block1);
            block2.IsCompleted.Should().BeTrue();
        }
    }

    [TestMethod()]
    public async Task ForwardAsync_ChildIsParent_ShouldBeFalse()
    {
        // arrange
        var block = new PipelineBlock<object>();
        block.SetChild(block);
        // act
        Func<Task<bool>> act = () => (block as IActiveBlock<object>).ForwardAsync();
        // assert
        (await act.Should().NotThrowAsync()).Which.Should().BeFalse();
    }

    [TestMethod()]
    public async Task ForwardAsync_ChildIsCompleted_ShouldBeFalse()
    {
        // arrange
        Mock<IChildBlock> block2 = new();
        block2.Setup(x => x.IsCompleted).Returns(true);
        var block1 = new PipelineBlock<object>();
        block1.SetChild(block2.Object);
        // act
        Func<Task<bool>> act = () => (block1 as IActiveBlock<object>).ForwardAsync();
        // assert
        (await act.Should().NotThrowAsync()).Which.Should().BeFalse();
    }

    [TestMethod()]
    public async Task SetParent_IsCompleted_ShouldBeFalse()
    {
        // arrange
        var block = new PipelineBlock<object>();
        // act
        await block.ExecuteAsync();
        Func<bool> act = () => (block as IChildBlock).SetParent(new PipelineBlock<object>());
        // assert
        act.Should().NotThrow().Which.Should().BeFalse();
    }

    [TestMethod()]
    public void SetParent_ParentBlock_ShouldBeTrue()
    {
        // arrange
        var block = new PipelineBlock<object>();
        // act
        Func<bool> act = () => (block as IChildBlock).SetParent(Mock.Of<IParentBlock>());
        // assert
        act.Should().NotThrow().Which.Should().BeTrue();
    }

    [TestMethod()]
    public async Task ExitAsync_IsCompleted_ShouldReturnFalse()
    {
        // arrange
        var block = new PipelineBlock<object>() as IActiveBlock<object>;
        // act
        Func<Task<bool>> act = async () =>
        {
            await block.ForwardAsync(null);
            return await block.ExitAsync(null);
        };
        // asset
        (await act.Should().NotThrowAsync()).Which.Should().BeFalse();
    }

    [TestMethod()]
    public async Task ExitAsync_NoExit_ShouldReturnFalse()
    {
        // arrange
        var block2 = new PipelineBlock<object>();
        var block1 = new PipelineBlock<object>()
        {
            ChildCondition = _ => block2
        };
        // act
        Func<Task<bool>> act = () => (block1 as IActiveBlock<object>).ExitAsync(null);
        // asset
        (await act.Should().NotThrowAsync()).Which.Should().BeFalse();
    }

    [TestMethod()]
    public async Task ExitAsync_HasExit_ShouldReturnSuccess()
    {
        // arrange
        var result = 123;
        var block = new PipelineBlock<object>() as IActiveBlock<object>;
        // act
        Func<Task<bool>> act = () => block.ExitAsync(result);
        // asset
        using (new AssertionScope())
        {
            (await act.Should().NotThrowAsync()).Which.Should().BeTrue();
            block.Data.Should().Be(result);
            block.IsCompleted.Should().BeTrue();
        }
    }

    [TestMethod()]
    public async Task ResetData_IsExecuted_ShouldReturnFalse()
    {
        // arrange
        var block = new PipelineBlock<object>()
        {
            Job = (x, c) => x.ForwardAsync(123, c)
        };
        // act
        await block.ExecuteAsync();
        (block as IParentBlock<object>).ResetData();
        // asset
        using var _ = new AssertionScope();
        block.IsCompleted.Should().BeFalse();
        block.Data.Should().BeNull();
    }

    [TestMethod()]
    public async Task BackToCheckpointAsync_IsCompleted_ShouldReturnFalse()
    {
        // arrange
        var block = new PipelineBlock<object>() as IActiveBlock<object>;
        // act
        Func<Task<bool>> act = async () =>
        {
            await block.ForwardAsync();
            return await block.BackToCheckpointAsync();
        };
        // asset
        (await act.Should().NotThrowAsync()).Which.Should().BeFalse();
    }

    [TestMethod()]
    public async Task BackToCheckpointAsync_OneBlock_ShouldReturnFalse()
    {
        // arrange
        var block = new PipelineBlock<object>() as IActiveBlock<object>;
        // act
        Func<Task<bool>> act = () => block.BackToCheckpointAsync();
        // asset
        (await act.Should().NotThrowAsync()).Which.Should().BeFalse();
    }

    [TestMethod()]
    public async Task BackToCheckpointAsync_NamedCheckpoint_ShouldReturnSuccess()
    {
        // arrange
        var block1 = new Mock<IPipelineBlock<object>>();
        var block2 = new Mock<IPipelineBlock<object>>();
        var block3 = new PipelineBlock<object>();
        block1.Setup(x => x.Key).Returns("block1");
        block1.Setup(x => x.IsCompleted).Returns(true);
        block1.Setup(x => x.Child).Returns(block2.Object);
        block1.Setup(x => x.IsCheckpoint).Returns(true);
        block2.Setup(x => x.Key).Returns("block2");
        block2.Setup(x => x.IsCompleted).Returns(true);
        block2.Setup(x => x.Parent).Returns(block1.Object);
        block2.Setup(x => x.Child).Returns(block3);
        (block3 as IChildBlock).SetParent(block2.Object);
        // act
        await (block3 as IActiveBlock).BackToCheckpointAsync("block1");
        // asset
        using var _ = new AssertionScope();
        block2.Verify(x => x.ResetData(), Times.Once());
        block1.Verify(x => x.ExecuteAsync(It.IsAny<CancellationToken>()), Times.Once());
    }

    [TestMethod()]
    public async Task BackToCheckpointAsync_UnnamedCheckpoint_ShouldReturnSuccess()
    {
        // arrange
        var block1 = new Mock<IPipelineBlock<object>>();
        var block2 = new Mock<IPipelineBlock<object>>();
        var block3 = new PipelineBlock<object>();
        block1.Setup(x => x.IsCompleted).Returns(true);
        block1.Setup(x => x.Child).Returns(block2.Object);
        block1.Setup(x => x.IsCheckpoint).Returns(true);
        block2.Setup(x => x.IsCompleted).Returns(true);
        block2.Setup(x => x.Parent).Returns(block1.Object);
        block2.Setup(x => x.Child).Returns(block3);
        (block3 as IChildBlock).SetParent(block2.Object);
        // act
        await (block3 as IActiveBlock).BackToCheckpointAsync();
        // asset
        using var _ = new AssertionScope();
        block2.Verify(x => x.ResetData(), Times.Once());
        block1.Verify(x => x.ExecuteAsync(It.IsAny<CancellationToken>()), Times.Once());
    }

    [TestMethod()]
    public async Task BackToExitAsync_IsCompleted_ShouldReturnFalse()
    {
        // arrange
        var block = new PipelineBlock<object>() as IActiveBlock<object>;
        // act
        Func<Task<bool>> act = async () =>
        {
            await block.ForwardAsync();
            return await block.BackToExitAsync();
        };
        // asset
        (await act.Should().NotThrowAsync()).Which.Should().BeFalse();
    }

    [TestMethod()]
    public async Task BackToExitAsync_OneBlock_ShouldReturnFalse()
    {
        // arrange
        var block = new PipelineBlock<object>() as IActiveBlock<object>;
        // act
        Func<Task<bool>> act = () => block.BackToExitAsync();
        // asset
        (await act.Should().NotThrowAsync()).Which.Should().BeFalse();
    }

    [TestMethod()]
    public async Task BackToExitAsync_NamedExit_ShouldReturnSuccess()
    {
        // arrange
        var block1 = new Mock<IPipelineBlock<object>>();
        var block2 = new Mock<IPipelineBlock<object>>();
        var block3 = new PipelineBlock<object>();
        block1.Setup(x => x.Key).Returns("block1");
        block1.Setup(x => x.IsCompleted).Returns(true);
        block1.Setup(x => x.Child).Returns(block2.Object);
        block1.Setup(x => x.HasExit).Returns(true);
        block2.Setup(x => x.Key).Returns("block2");
        block2.Setup(x => x.IsCompleted).Returns(true);
        block2.Setup(x => x.Parent).Returns(block1.Object);
        block2.Setup(x => x.Child).Returns(block3);
        (block3 as IChildBlock).SetParent(block2.Object);
        // act
        await (block3 as IActiveBlock).BackToExitAsync("block1");
        // asset
        using var _ = new AssertionScope();
        block2.Verify(x => x.ResetData(), Times.Once());
        block1.Verify(x => x.ResetData(), Times.Never);
    }

    [TestMethod()]
    public async Task BackToExitAsync_UnnamedExit_ShouldReturnSuccess()
    {
        // arrange
        var block1 = new Mock<IPipelineBlock<object>>();
        var block2 = new Mock<IPipelineBlock<object>>();
        var block3 = new PipelineBlock<object>();
        block1.Setup(x => x.IsCompleted).Returns(true);
        block1.Setup(x => x.Child).Returns(block2.Object);
        block1.Setup(x => x.HasExit).Returns(true);
        block2.Setup(x => x.IsCompleted).Returns(true);
        block2.Setup(x => x.Parent).Returns(block1.Object);
        block2.Setup(x => x.Child).Returns(block3);
        (block3 as IChildBlock).SetParent(block2.Object);
        // act
        await (block3 as IActiveBlock).BackToExitAsync();
        // asset
        using var _ = new AssertionScope();
        block2.Verify(x => x.ResetData(), Times.Once());
        block1.Verify(x => x.ResetData(), Times.Never);
    }

    [TestMethod()]
    public async Task SkipAsync_IsCompleted_ShouldReturnFalse()
    {
        // arrange
        var block = new PipelineBlock<object>() as IActiveBlock<object>;
        // act
        Func<Task<bool>> act = async () =>
        {
            await block.ForwardAsync();
            return await block.SkipAsync();
        };
        // asset
        (await act.Should().NotThrowAsync()).Which.Should().BeFalse();
    }

    [TestMethod()]
    public async Task SkipAsync_OneBlock_ShouldReturnTrue()
    {
        // arrange
        var block = new PipelineBlock<object>() as IActiveBlock<object>;
        // act
        Func<Task<bool>> act = () => block.SkipAsync();
        // asset
        (await act.Should().NotThrowAsync()).Which.Should().BeTrue();
    }

    [TestMethod()]
    public async Task SkipAsync_HasChild_ShouldReturnSuccess()
    {
        // arrange
        var block2 = new Mock<IPipelineBlock<object>>();
        var block1 = new PipelineBlock<object>();
        block1.SetChild(block2.Object);
        // act
        await (block1 as IActiveBlock<object>).SkipAsync();
        // asset
        using var _ = new AssertionScope();
        block1.IsCompleted.Should().BeFalse();
        block2.Verify(x => x.SetParent(null), Times.Once());
        block2.Verify(x => x.ExecuteAsync(It.IsAny<CancellationToken>()), Times.Once());
    }

    [TestMethod()]
    public void SetChild_BaseType_ShouldBeTrue()
    {
        // arrange
        var block1 = new PipelineBlock<object>();
        var block2 = Mock.Of<IChildBlock>();
        // act
        Func<bool> act = () => block1.SetChild(block2);
        // assert
        using var _ = new AssertionScope();
        act.Should().NotThrow().Which.Should().BeTrue();
        block1.Child.Should().Be(block2);
    }

    [TestMethod()]
    public async Task SetChild_BaseTypeIsCompleted_ShouldBeFalse()
    {
        // arrange
        var block = new PipelineBlock<object>();
        // act
        await block.ExecuteAsync();
        Func<bool> act = () => block.SetChild(Mock.Of<IChildBlock>());
        // assert
        act.Should().NotThrow().Which.Should().BeFalse();
    }

    [TestMethod()]
    public async Task SetChild_GenericFunc_ShouldBeSuccess()
    {
        // arrange
        var block1 = new PipelineBlock<object>
        {
            Job = (x, c) => x.ForwardAsync(cancellationToken: c)
        };
        var block2 = new Mock<IChildBlock>();
        // act
        block1.SetChild(x => block2.Object);
        await block1.ExecuteAsync();
        // assert
        block2.Verify(x => x.ExecuteAsync(It.IsAny<CancellationToken>()), Times.Once());
    }

    [TestMethod()]
    public async Task SetChild_GenericFuncIsCompleted_ShouldReturnFalse()
    {
        // arrange
        var block = new PipelineBlock<object>();
        // act
        await block.ExecuteAsync();
        Func<bool> act = () => block.SetChild(x => Mock.Of<IChildBlock>());
        // assert
        act.Should().NotThrow().Which.Should().BeFalse();
    }

    [TestMethod()]
    public async Task SetChild_BaseFunc_ShouldBeSuccess()
    {
        // arrange
        var block1 = new PipelineBlock<object>
        {
            Job = (x, c) => x.ForwardAsync(cancellationToken: c)
        };
        var block2 = new Mock<IChildBlock>();
        // act
        (block1 as IParentBlock).SetChild(x => block2.Object);
        await block1.ExecuteAsync();
        // assert
        block2.Verify(x => x.ExecuteAsync(It.IsAny<CancellationToken>()), Times.Once());
    }

    [TestMethod()]
    public async Task SetChild_BaseFuncIsCompleted_ShouldReturnFalse()
    {
        // arrange
        var block = new PipelineBlock<object>();
        // act
        await block.ExecuteAsync();
        Func<bool> act = () => (block as IParentBlock).SetChild(x => Mock.Of<IChildBlock>());
        // assert
        act.Should().NotThrow().Which.Should().BeFalse();
    }
}
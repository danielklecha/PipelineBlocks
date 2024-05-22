using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PipelineBlocks.Models;
using System.Diagnostics.CodeAnalysis;

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
        string name = "myblock";
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
        string key = "myblock";
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
        PipelineBlock<int> block2 = new()
        {
            CheckpointCondition = x => true
        };
        PipelineBlock<int> block1 = new()
        {
            Job = (x, _) => Task.FromResult(BlockResult.Forward(123)),
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
        PipelineBlock<int> block2 = new()
        {
            Job = (x, _) => Task.FromResult(BlockResult<int>.BackToExit())
        };
        PipelineBlock<int> block1 = new()
        {
            Job = (x, _) => Task.FromResult(BlockResult.Forward(123)),
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
        PipelineBlock<int> block = new()
        {
            Job = (x, _) => Task.FromResult(BlockResult.Forward(123))
        };
        // act
        Func<Task<BlockResult>> act = async () =>
        {
            await block.ExecuteAsync();
            return await block.ExecuteAsync();
        };
        // assert
        (await act.Should().NotThrowAsync()).Which.ResultType.Should().Be(BlockResultType.Error);
    }

    [TestMethod()]
    public async Task ExecuteAsync_NoJob_ShouldReturnFalse()
    {
        // arrange
        PipelineBlock<object> block = new();
        // act
        Func<Task<BlockResult>> act = () => block.ExecuteAsync();
        // assert
        (await act.Should().NotThrowAsync()).Which.ResultType.Should().Be(BlockResultType.Error);
    }

    [TestMethod()]
    public async Task ExecuteAsync_HasJob_ShouldReturnTrue()
    {
        // arrange
        PipelineBlock<int> block = new()
        {
            Job = (x, _) => Task.FromResult(BlockResult.Forward(123))
        };
        // act
        Func<Task<BlockResult>> act = () => block.ExecuteAsync();
        // assert
        (await act.Should().NotThrowAsync()).Which.ResultType.Should().Be(BlockResultType.Completed);
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
        int result = 123;
        PipelineBlock<int> block = new()
        {
            Job = (x, _) => Task.FromResult(BlockResult.Forward(result))
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
    public async Task Job_ForwardResultWithOneBlock_ShouldBeCompleted()
    {
        // arrange
        int result = 123;
        PipelineBlock<int> block = new()
        {
            Job = (x, _) => Task.FromResult(BlockResult.Forward(result))
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
    public async Task Job_ForwardResultWithTwoBlocks_ShouldBeCompleted()
    {
        // arrange
        PipelineBlock<int> block2 = new()
        {
            Job = (x, _) => Task.FromResult(BlockResult.Forward(0))
        };
        PipelineBlock<int> block1 = new()
        {
            Job = (x, _) => Task.FromResult(BlockResult.Forward(0)),
            ChildCondition = x => block2
        };
        // act
        await block1.ExecuteAsync();
        // assert
        using AssertionScope assertionScope = new();
        block1.IsCompleted.Should().BeTrue();
        block2.Parent.Should().Be(block1);
        block2.IsCompleted.Should().BeTrue();
    }

    [TestMethod()]
    public async Task Job_ForwardResultWhenChildIsParent_ShouldBeFalse()
    {
        // arrange
        PipelineBlock<int> block = new()
        {
            Job = (x, _) => Task.FromResult(BlockResult.Forward(0))
        };
        block.SetChild(block);
        // act
        Func<Task<BlockResult>> act = () => block.ExecuteAsync();
        // assert
        (await act.Should().NotThrowAsync()).Which.ResultType.Should().Be(BlockResultType.Error);
    }

    [TestMethod()]
    public async Task Job_ForwardResultWhenChildIsCompleted_ShouldBeFalse()
    {
        // arrange
        Mock<IChildBlock> block2 = new();
        block2.Setup(x => x.SetParent(It.IsAny<IParentBlock>())).Returns(false);
        PipelineBlock<int> block1 = new()
        {
            Job = (x, _) => Task.FromResult(BlockResult.Forward(0))
        };
        block1.SetChild(block2.Object);

        // act
        Func<Task<BlockResult>> act = () => block1.ExecuteAsync();
        // assert
        (await act.Should().NotThrowAsync()).Which.ResultType.Should().Be(BlockResultType.Error);
    }

    [TestMethod()]
    public async Task SetParent_IsCompleted_ShouldBeFalse()
    {
        // arrange
        PipelineBlock<object> block = new();
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
        PipelineBlock<object> block = new();
        // act
        Func<bool> act = () => (block as IChildBlock).SetParent(Mock.Of<IParentBlock>());
        // assert
        act.Should().NotThrow().Which.Should().BeTrue();
    }

    [TestMethod()]
    public async Task Job_ExitResultWhenNoExit_ShouldReturnFalse()
    {
        // arrange
        PipelineBlock<int> block1 = new()
        {
            Job = (x, _) => Task.FromResult(BlockResult.Exit<int>()),
            ChildCondition = x => Mock.Of<IChildBlock>()
        };
        // act
        Func<Task<BlockResult>> act = () => block1.ExecuteAsync();
        // asset
        (await act.Should().NotThrowAsync()).Which.ResultType.Should().Be(BlockResultType.Error);
    }

    [TestMethod()]
    public async Task Job_ExitResultWhenHasExit_ShouldReturnSuccess()
    {
        // arrange
        int result = 123;
        PipelineBlock<int> block = new()
        {
            ExitCondition = x => true,
            Job = (x, _) => Task.FromResult(BlockResult.Exit(result))
        };
        // act
        Func<Task<BlockResult>> act = () => block.ExecuteAsync();
        // asset
        using (new AssertionScope())
        {
            (await act.Should().NotThrowAsync()).Which.ResultType.Should().Be(BlockResultType.Completed);
            block.Data.Should().Be(result);
            block.IsCompleted.Should().BeTrue();
        }
    }

    [TestMethod()]
    public async Task Reset_IsExecuted_ShouldReturnFalse()
    {
        // arrange
        PipelineBlock<int?> block = new()
        {
            Job = (x, _) => Task.FromResult(BlockResult.Exit<int?>(null))
        };
        // act
        await block.ExecuteAsync();
        (block as IParentBlock<int?>).Reset();
        using AssertionScope assertionScope = new();
        block.IsCompleted.Should().BeFalse();
        block.Data.Should().BeNull();
    }

    [TestMethod()]
    public async Task Job_BackToCheckpointResultWithOneBlock_ShouldReturnFalse()
    {
        // arrange
        PipelineBlock<int> block = new()
        {
            Job = (x, _) => Task.FromResult(BlockResult<int>.BackToCheckpoint())
        };
        // act
        Func<Task<BlockResult>> act = () => block.ExecuteAsync();
        // asset
        (await act.Should().NotThrowAsync()).Which.ResultType.Should().Be(BlockResultType.Error);
    }

    [TestMethod()]
    public async Task Job_BackToCheckpointResultWithNamedCheckpoint_ShouldReturnSuccess()
    {
        // arrange
        Mock<IPipelineBlock<object>> block1 = new();
        Mock<IPipelineBlock<object>> block2 = new();
        PipelineBlock<int> block3 = new()
        {
            Job = (x, _) => Task.FromResult(BlockResult<int>.BackToCheckpoint("block1"))
        };
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
        await block3.ExecuteAsync();
        using AssertionScope assertionScope = new();
        block2.Verify(x => x.Reset(), Times.Once());
        block1.Verify(x => x.ExecuteSelfAsync(It.IsAny<CancellationToken>()), Times.Once());
    }

    [TestMethod()]
    public async Task Job_BackToCheckpointResultWithUnnamedCheckpoint_ShouldReturnSuccess()
    {
        // arrange
        Mock<IPipelineBlock<object>> block1 = new();
        Mock<IPipelineBlock<object>> block2 = new();
        PipelineBlock<int> block3 = new()
        {
            Job = (x, _) => Task.FromResult(BlockResult<int>.BackToCheckpoint())
        };
        block1.Setup(x => x.IsCompleted).Returns(true);
        block1.Setup(x => x.Child).Returns(block2.Object);
        block1.Setup(x => x.IsCheckpoint).Returns(true);
        block2.Setup(x => x.IsCompleted).Returns(true);
        block2.Setup(x => x.Parent).Returns(block1.Object);
        block2.Setup(x => x.Child).Returns(block3);
        (block3 as IChildBlock).SetParent(block2.Object);
        // act
        await block3.ExecuteAsync();
        using AssertionScope assertionScope = new();
        block2.Verify(x => x.Reset(), Times.Once());
        block1.Verify(x => x.ExecuteSelfAsync(It.IsAny<CancellationToken>()), Times.Once());
    }

    [TestMethod()]
    public async Task Job_BackToExitResultWithOneBlock_ShouldReturnError()
    {
        // arrange
        PipelineBlock<int> block = new()
        {
            Job = (x, _) => Task.FromResult(BlockResult<int>.BackToExit())
        };
        // act
        Func<Task<BlockResult>> act = () => block.ExecuteAsync();
        // asset
        (await act.Should().NotThrowAsync()).Which.ResultType.Should().Be(BlockResultType.Error);
    }

    [TestMethod()]
    public async Task Job_BackToExitResultWithNamedExit_ShouldReturnSuccess()
    {
        // arrange
        Mock<IPipelineBlock<object>> block1 = new();
        Mock<IPipelineBlock<object>> block2 = new();
        PipelineBlock<int> block3 = new()
        {
            Job = (x, _) => Task.FromResult(BlockResult<int>.BackToExit("block1"))
        };
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
        await block3.ExecuteAsync();
        using AssertionScope assertionScope = new();
        block2.Verify(x => x.Reset(), Times.Once());
        block1.Verify(x => x.Reset(), Times.Never);
    }

    [TestMethod()]
    public async Task Job_BackToExitResultWithUnnamedExit_ShouldReturnSuccess()
    {
        // arrange
        Mock<IPipelineBlock<object>> block1 = new();
        Mock<IPipelineBlock<object>> block2 = new();
        PipelineBlock<int> block3 = new()
        {
            Job = (x, _) => Task.FromResult(BlockResult<int>.BackToExit())
        };
        block1.Setup(x => x.IsCompleted).Returns(true);
        block1.Setup(x => x.Child).Returns(block2.Object);
        block1.Setup(x => x.HasExit).Returns(true);
        block2.Setup(x => x.IsCompleted).Returns(true);
        block2.Setup(x => x.Parent).Returns(block1.Object);
        block2.Setup(x => x.Child).Returns(block3);
        (block3 as IChildBlock).SetParent(block2.Object);
        // act
        await block3.ExecuteAsync();
        using AssertionScope assertionScope = new();
        block2.Verify(x => x.Reset(), Times.Once());
        block1.Verify(x => x.Reset(), Times.Never);
    }

    [TestMethod()]
    public async Task Job_SkipResultWhenChildIsCompleted_ShouldReturnFalse()
    {
        // arrange
        Mock<IPipelineBlock> block2 = new();
        block2.Setup(x => x.SetParent(It.IsAny<IParentBlock>())).Returns(false);
        PipelineBlock<int> block1 = new()
        {
            Job = (x, c) => Task.FromResult(BlockResult<int>.Skip()),
            ChildCondition = x => block2.Object
        };
        // act
        Func<Task<BlockResult>> act = () => block1.ExecuteAsync();
        // asset
        (await act.Should().NotThrowAsync()).Which.ResultType.Should().Be(BlockResultType.Error);
    }

    [TestMethod()]
    public async Task Job_SkipResultWithOneBlock_ShouldReturnTrue()
    {
        // arrange
        PipelineBlock<int> block = new()
        {
            Job = (x, c) => Task.FromResult(BlockResult<int>.Skip()),
        };
        // act
        Func<Task<BlockResult>> act = () => block.ExecuteAsync();
        // asset
        (await act.Should().NotThrowAsync()).Which.ResultType.Should().Be(BlockResultType.Completed);
    }

    [TestMethod()]
    public async Task Job_SkipResultWhenHasChild_ShouldReturnSuccess()
    {
        // arrange
        Mock<IPipelineBlock<object>> block2 = new();
        block2.Setup(x => x.SetParent(It.IsAny<IParentBlock>())).Returns(true);
        PipelineBlock<int> block1 = new()
        {
            Job = (x, c) => Task.FromResult(BlockResult<int>.Skip()),
            ChildCondition = x => block2.Object
        };
        // act
        await block1.ExecuteAsync();
        using AssertionScope assertionScope = new();
        block1.IsCompleted.Should().BeFalse();
        block2.Verify(x => x.SetParent(null), Times.Once());
        block2.Verify(x => x.ExecuteSelfAsync(It.IsAny<CancellationToken>()), Times.Once());
    }

    [TestMethod()]
    public async Task Job_SkipResultWhenChildIsParent_ShouldBeFalse()
    {
        // arrange
        PipelineBlock<int> block = new()
        {
            Job = (_, _) => Task.FromResult(BlockResult<int>.Skip()),
        };
        block.SetChild(block);
        // act
        Func<Task<BlockResult>> act = () => block.ExecuteAsync();
        // assert
        (await act.Should().NotThrowAsync()).Which.ResultType.Should().Be(BlockResultType.Error);
    }

    [TestMethod()]
    public void SetChild_BaseType_ShouldBeTrue()
    {
        // arrange
        PipelineBlock<object> block1 = new();
        IChildBlock block2 = Mock.Of<IChildBlock>();
        // act
        Func<bool> act = () => block1.SetChild(block2);
        using AssertionScope assertionScope = new();
        act.Should().NotThrow().Which.Should().BeTrue();
        block1.Child.Should().Be(block2);
    }

    [TestMethod()]
    public async Task SetChild_BaseTypeIsCompleted_ShouldBeFalse()
    {
        // arrange
        PipelineBlock<object> block = new();
        // act
        await block.ExecuteAsync();
        Func<bool> act = () => block.SetChild(Mock.Of<IChildBlock>());
        // assert
        act.Should().NotThrow().Which.Should().BeFalse();
    }

    [TestMethod()]
    public void SetChild_GenericFunc_ShouldBeSuccess()
    {
        // arrange
        PipelineBlock<int> block1 = new()
        {
            Job = (_, _) => Task.FromResult(BlockResult.Forward(0))
        };
        IChildBlock block2 = Mock.Of<IChildBlock>();
        // act
        Func<bool> act = () => block1.SetChild(x => block2);
        using AssertionScope assertionScope = new();
        act.Should().NotThrow().Which.Should().BeTrue();
        block1.Child.Should().Be(block2);
    }

    [TestMethod()]
    public async Task SetChild_GenericFuncIsCompleted_ShouldReturnFalse()
    {
        // arrange
        PipelineBlock<object> block = new();
        // act
        await block.ExecuteAsync();
        Func<bool> act = () => block.SetChild(x => Mock.Of<IChildBlock>());
        // assert
        act.Should().NotThrow().Which.Should().BeFalse();
    }

    [TestMethod()]
    public void SetChild_BaseFunc_ShouldBeSuccess()
    {
        // arrange
        PipelineBlock<int> block1 = new()
        {
            Job = (_, _) => Task.FromResult(BlockResult.Forward(0))
        };
        IChildBlock block2 = Mock.Of<IChildBlock>();
        // act
        Func<bool> act = () => (block1 as IParentBlock).SetChild(x => block2);
        using AssertionScope assertionScope = new();
        act.Should().NotThrow().Which.Should().BeTrue();
        block1.Child.Should().Be(block2);
    }

    [TestMethod()]
    public async Task SetChild_BaseFuncIsCompleted_ShouldReturnFalse()
    {
        // arrange
        PipelineBlock<object> block = new();
        // act
        await block.ExecuteAsync();
        Func<bool> act = () => (block as IParentBlock).SetChild(x => Mock.Of<IChildBlock>());
        // assert
        act.Should().NotThrow().Which.Should().BeFalse();
    }

    [TestMethod()]
    public async Task Job_ErrorResult_ShouldBeForwarded()
    {
        // arrange
        string message = "error";
        BlockResult<int> result = BlockResult<int>.Error(message);
        PipelineBlock<int> block = new()
        {
            Job = (_, _) => Task.FromResult(result)
        };
        // act
        Func<Task<BlockResult>> act = () => block.ExecuteAsync();
        // assert
        (await act.Should().NotThrowAsync()).Which.Should().Be(result);
        result.Message.Should().Be(message);
    }

    [TestMethod()]
    public async Task Job_CompletedResult_ShouldBeSuccess()
    {
        // arrange
        PipelineBlock<int> block = new()
        {
            Job = (_, _) => Task.FromResult(BlockResult<int>.Completed())
        };
        // act
        Func<Task<BlockResult>> act = () => block.ExecuteAsync();
        // assert
        (await act.Should().NotThrowAsync()).Which.ResultType.Should().Be(BlockResultType.Completed);
    }

    [TestMethod()]
    public async Task Job_ExecuteResult_ShouldReturnError()
    {
        // arrange
        PipelineBlock<IExecutableBlock> block = new()
        {
            Job = (x, _) => Task.FromResult<BlockResult<IExecutableBlock>>(BlockResult<IExecutableBlock>.Execute(x as IExecutableBlock))
        };
        // act
        Func<Task<BlockResult>> act = () => block.ExecuteAsync();
        // assert
        (await act.Should().NotThrowAsync()).Which.ResultType.Should().Be(BlockResultType.Error);
    }

    [TestMethod()]
    public async Task Job_Null_ShouldReturnError()
    {
        // arrange
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        PipelineBlock<int> block = new()
        {
            Job = (x, _) => Task.FromResult<BlockResult<int>>(null)
        };
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        // act
        Func<Task<BlockResult>> act = () => block.ExecuteAsync();
        // assert
        (await act.Should().NotThrowAsync()).Which.ResultType.Should().Be(BlockResultType.Error);
    }
}
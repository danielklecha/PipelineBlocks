using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PipelineBlocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipelineBlocks.Tests
{
    [TestClass()]
    public class PipelineBlockTests
    {
        [TestMethod()]
        public async Task GoForwardAsync_OneBlock_ShouldBeCompleted()
        {
            var block = new PipelineBlock { Job = x => x.GoForwardAsync() };
            //act
            await block.ExecuteAsync();
            //
            block.IsCompleted.Should().BeTrue();
        }

        [TestMethod()]
        public async Task GoForwardAsync_TwoBlocks_ShouldBeCompleted()
        {
            var block2 = new PipelineBlock { Job = x => x.GoForwardAsync() };
            var block1 = new PipelineBlock { Job = x => x.GoForwardAsync(), ChildCondition = x => block2 };
            //act
            await block1.ExecuteAsync();
            //
            block1.IsCompleted.Should().BeTrue();
            block2.IsCompleted.Should().BeTrue();
        }

        [TestMethod]
        public async Task SkipAndGoForwardAsync_SkipFirst_ShouldBeSkipped()
        {
            var block2 = new PipelineBlock { Job = x => x.GoForwardAsync() };
            var block1 = new PipelineBlock { Job = x => x.SkipAndGoForwardAsync(), ChildCondition = x => block2 };
            //act
            await block1.ExecuteAsync();

            block1.IsCompleted.Should().BeFalse();
            block2.IsCompleted.Should().BeTrue();
        }

        [TestMethod]
        public async Task SkipAndGoForwardAsync_SkipMiddle_ShouldBeSkipped()
        {
            var block3 = new PipelineBlock { Job = x => x.GoForwardAsync() };
            var block2 = new PipelineBlock { Job = x => x.SkipAndGoForwardAsync(), ChildCondition = x => block3 };
            var block1 = new PipelineBlock { Job = x => x.GoForwardAsync(), ChildCondition = x => block2 };
            //act
            await block1.ExecuteAsync();

            block1.IsCompleted.Should().BeTrue();
            block2.IsCompleted.Should().BeFalse();
            block3.IsCompleted.Should().BeTrue();
        }

        [TestMethod]
        public async Task SkipAndGoForwardAsync_SkipLast_ShouldBeSkipped()
        {
            var block2 = new PipelineBlock { Job = x => x.SkipAndGoForwardAsync() };
            var block1 = new PipelineBlock { Job = x => x.GoForwardAsync(), ChildCondition = x => block2 };
            //act
            await block1.ExecuteAsync();

            block1.IsCompleted.Should().BeTrue();
            block2.IsCompleted.Should().BeFalse();
        }

        [TestMethod]
        public async Task GoBackToCheckpointAsync_BackFromSecondToFirst_ShouldBeDone()
        {
            var block1 = new Mock<IPipelineBlock>();
            block1.Setup(x => x.IsCheckpoint).Returns(true);
            var block2 = new PipelineBlock { Job = x => x.GoBackToCheckpointAsync(), Parent = block1.Object };

            //act
            await block2.ExecuteAsync();

            block1.Verify( x => x.ExecuteAsync(), Times.Once() );
        }

        [TestMethod]
        public async Task GoBackToCheckpointAsync_BackFromThirdToFirst_ShouldBeDone()
        {
            var block1 = new Mock<IPipelineBlock>();
            block1.Setup( x => x.IsCheckpoint ).Returns( true );
            var block3 = new PipelineBlock { Job = x => x.GoBackToCheckpointAsync() };
            var block2 = new PipelineBlock { Job = x => x.GoForwardAsync(), Parent = block1.Object, ChildCondition = x => block3 };

            //act
            await block2.ExecuteAsync();

            block1.Verify( x => x.ExecuteAsync(), Times.Once() );
        }
    }
}
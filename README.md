# PipelineBlocks

[![NuGet](https://img.shields.io/nuget/v/PipelineBlocks.svg)](https://www.nuget.org/packages/PipelineBlocks)
[![NuGet downloads](https://img.shields.io/nuget/dt/PipelineBlocks.svg)](https://www.nuget.org/packages/PipelineBlocks)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](https://github.com/danielklecha/PipelineBlocks/blob/master/LICENSE.txt)
[![Coverage Status](https://coveralls.io/repos/github/danielklecha/PipelineBlocks/badge.svg?branch=master)](https://coveralls.io/github/danielklecha/PipelineBlocks?branch=master)

A .NET Standard library that can be used to create two-directional pipeline blocks.

## Strong-named version

[![NuGet](https://img.shields.io/nuget/v/PipelineBlocks.StrongName.svg)](https://www.nuget.org/packages/PipelineBlocks.StrongName)
[![NuGet downloads](https://img.shields.io/nuget/dt/PipelineBlocks.StrongName.svg)](https://www.nuget.org/packages/PipelineBlocks.StrongName)

## Features

- It's asynchronous.
- Allows moving forward and backward in the pipeline.
- Allows skipping the current block and removing it from the pipeline.
- Blocks can be marked as an exit, meaning the pipeline can be finished at that stage.
- Blocks can be marked as checkpoints, allowing the pipeline to revert to this stage from any descendant block.
- Child blocks can be dynamically determined based on the data of the current block.
- Blocks can be merged into modules for better organization and reuse.
- The library includes unit tests.

## Available actions in active block

```mermaid
sequenceDiagram
participant PA as Parent A (exit)
participant PB as Parent B (checkpoint)
participant A as Active block (exit)
participant CA as Child
A ->> CA: BlockResult.Forward<T>
Note left of CA: Continue pipeline
A ->> CA: BlockResult<T>.Skip
Note left of CA: Continue pipeline
A ->> PA: BlockResult<T>.BackToExit
Note right of PA: Finish pipeline
A ->> PB: BlockResult<T>.BackToCheckpoint
Note right of PB: Continue pipeline
A ->> A: BlockResult.Exit<T>
Note right of A: Finish pipeline
A ->> A: BlockResult<T>.Error
Note right of A: Finish pipeline
```

## Example - single blocks

```csharp
using PipelineBlocks.Extensions;
using PipelineBlocks.Models;

var block2 = new PipelineBlock<string>()
{
    Job = (x, c) => Task.FromResult(BlockResult.Forward("success")),
    KeyCondition = x => "block2",
    NameCondition = x => "block2",
    CheckpointCondition = x => false,
    ExitCondition = x => true,
    ChildCondition = x => null
};
var block1 = new PipelineBlock<int>()
{
    Job = (x, c) => Task.FromResult(BlockResult.Forward(123)),
    KeyCondition = x => "block1",
    NameCondition = x => "block1",
    CheckpointCondition = x => false,
    ExitCondition = x => true,
    ChildCondition = x => block2
};
await block1.ExecuteAsync();
```

## Example - module

```csharp
using PipelineBlocks.Extensions;
using PipelineBlocks.Models;

var endBlock = new PipelineBlock<int>()
{
    Job = (x, c) => Task.FromResult(BlockResult.Forward(123)),
    KeyCondition = x => "block1",
    NameCondition = x => "block1",
    CheckpointCondition = x => false,
    ExitCondition = x => true
};
var startblock = new PipelineBlock<int>()
{
    Job = (x, c) => Task.FromResult(BlockResult.Forward(123)),
    KeyCondition = x => "block1",
    NameCondition = x => "block1",
    CheckpointCondition = x => false,
    ExitCondition = x => true,
    ChildCondition = x => endBlock
};
var module = new PipelineModule<int>(startblock, endBlock);
await module.ExecuteAsync();
```

## License

`PipelineBlocks` is provided as-is under the MIT license.

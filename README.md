# PipelineBlocks

[![NuGet](https://img.shields.io/nuget/v/PipelineBlocks.svg)](https://www.nuget.org/packages/PipelineBlocks)
[![NuGet downloads](https://img.shields.io/nuget/dt/PipelineBlocks.svg)](https://www.nuget.org/packages/PipelineBlocks)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](https://github.com/danielklecha/PipelineBlocks/blob/master/LICENSE.txt)

A .NET library that can be used to create two-directional pipeline blocks.

## Example

```csharp
using PipelineBlocks.Extensions;
using PipelineBlocks.Models;

var block2 = new PipelineBlock<string>()
{
    Job = (x, c) => x.ForwardAsync("success", c),
    KeyCondition = x => "block2",
    NameCondition = x => "block2",
    CheckpointCondition = x => false,
    ExitCondition = x => true,
    ChildCondition = x => null
};
var block1 = new PipelineBlock<int>()
{
    Job = (x, c) => x.ForwardAsync(123, c),
    KeyCondition = x => "block1",
    NameCondition = x => "block1",
    CheckpointCondition = x => false,
    ExitCondition = x => true,
    ChildCondition = x => block2
};
await block1.ExecuteAsync();
```

## Avaliable actions in active block

```mermaid
sequenceDiagram
participant PA as Parent A (exit)
participant PB as Parent B (checkpoint)
participant A as Active block (exit)
participant CA as Child
A ->> CA: ForwardAsync
Note left of CA: Continuue pipeline
A ->> CA: SkipAsync
Note left of CA: Continuue pipeline
A ->> PA: BackToExitAsync
Note right of PA: Finish pipeline
A ->> PB: BackToCheckpointAsync
Note right of PB: Finish pipeline
A ->> A: ExitAsync
Note right of A: Finish pipeline
```

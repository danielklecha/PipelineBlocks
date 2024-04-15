# PipelineBlocks

A .NET Standard library that can be used to create two-directional pipeline blocks.

## Example

```csharp
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

## Avaliable actions inside active block

```mermaid
sequenceDiagram
participant PA as Parent A (exit)
participant PB as Parent B (checkpoint)
participant A as Active block
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

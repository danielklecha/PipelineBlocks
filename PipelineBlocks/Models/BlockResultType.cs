namespace PipelineBlocks.Models;

public enum BlockResultType
{
    Completed,
    Error,
    Skip,
    Forward,
    BackToCheckpoint,
    BackToExit,
    Exit
}
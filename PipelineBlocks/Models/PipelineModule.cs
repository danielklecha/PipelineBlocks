﻿using PipelineBlocks.Extensions;

namespace PipelineBlocks.Models;

public class PipelineModule(IChildBlock startBlock, IParentBlock endBlock) : IPipelineModule
{
    public object? Data => endBlock.Data;

    public string? Name => endBlock.Name;

    public string? Key => endBlock.Key;

    public bool IsCheckpoint => Enumerable.Repeat(endBlock, 1).Concat(endBlock.EnumerateAncestors()).TakeWhile(x => x != startBlock.Parent).Any(x => x.IsCheckpoint);

    public bool HasExit => Enumerable.Repeat(endBlock, 1).Concat(endBlock.EnumerateAncestors()).TakeWhile(x => x != startBlock.Parent).Any(x => x.HasExit);

    public IBlock? Parent => startBlock.Parent;

    public IBlock? Child => endBlock.Child;

    public bool IsCompleted => endBlock.IsCompleted;

    public Task<BlockResult> ExecuteAsync(CancellationToken cancellationToken = default) => startBlock.ExecuteAsync(cancellationToken);

    void IParentBlock.Reset() => endBlock.Reset();

    public bool SetChild(Func<IBlock, IChildBlock?> setter) => endBlock.SetChild(setter);

    public bool SetChild(IChildBlock? block) => endBlock.SetChild(block);

    bool IChildBlock.SetParent(IParentBlock? parent) => startBlock.SetParent(parent);

    Task<BlockResult> IExecutableBlock.ExecuteSelfAsync(CancellationToken cancellationToken) => startBlock.ExecuteSelfAsync(cancellationToken);
}

public class PipelineModule<T>(IChildBlock startBlock, IParentBlock<T> endBlock) : PipelineModule(startBlock, endBlock), IPipelineModule<T>
{
    public new T? Data => endBlock.Data;

    public bool SetChild(Func<IBlock<T>, IChildBlock?> setter) => endBlock.SetChild(setter);
}
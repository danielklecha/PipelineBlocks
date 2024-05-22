namespace PipelineBlocks.Models;

public class BlockResult
{
    public string? Key { get; }
    public object? Data { get; }
    public string? Message { get; }
    public BlockResultType ResultType { get; }

    internal BlockResult(object? data, string? key, string? message, BlockResultType resultType)
    {
        Data = data;
        Key = key;
        Message = message;
        ResultType = resultType;
    }

    public static BlockResult Completed(string? message = null) => new(default, null, message, BlockResultType.Completed);
    public static BlockResult Error(string? message = null) => new(default, null, message, BlockResultType.Error);
    public static BlockResult<T> Exit<T>(T? data = default) => new(data, null, null, BlockResultType.Exit);
    public static BlockResult<T> Forward<T>(T? data = default) => new(data, null, null, BlockResultType.Forward);
    public static BlockResult<T> Execute<T>(T? data = default) => new(data, null, null, BlockResultType.Execute);
}

public class BlockResult<T> : BlockResult
{
    public new T? Data { get; }

    internal BlockResult(T? data, string? key, string? message, BlockResultType resultType) : base(data, key, message, resultType) => Data = data;

    public static new BlockResult<T> Completed(string? message = null) => new(default, null, message, BlockResultType.Completed);
    public static BlockResult<T> Skip() => new(default, null, null, BlockResultType.Skip);
    public static new BlockResult<T> Error(string? message = null) => new(default, null, message, BlockResultType.Error);
    public static BlockResult<T> BackToCheckpoint(string? key = null) => new(default, key, null, BlockResultType.BackToCheckpoint);
    public static BlockResult<T> BackToExit(string? key = null) => new(default, key, null, BlockResultType.BackToExit);
}
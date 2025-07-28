namespace PipelineBlocks.Models;

public class BlockResult
{
    public string? Key { get; }
    public object? Data { get; }
    public string? Message { get; }
    public Exception? Exception { get; set; }
    public BlockResultType ResultType { get; }

    internal BlockResult(object? data, string? key, string? message, BlockResultType resultType, Exception? exception)
    {
        Data = data;
        Key = key;
        Message = message;
        ResultType = resultType;
        Exception = exception;
    }

    public static BlockResult Completed(string? message = null) => new(default, null, message, BlockResultType.Completed, null);
    public static BlockResult Error(string? message = null, Exception? exception = null) => new(default, null, message, BlockResultType.Error, exception);
    public static BlockResult<T> Exit<T>(T? data = default) => new(data, null, null, BlockResultType.Exit, null);
    public static BlockResult<T> Forward<T>(T? data = default) => new(data, null, null, BlockResultType.Forward, null);
    public static BlockResult<T> Execute<T>(T? data = default) => new(data, null, null, BlockResultType.Execute, null);
}

public class BlockResult<T> : BlockResult
{
    public new T? Data { get; }

    internal BlockResult(T? data, string? key, string? message, BlockResultType resultType, Exception? exception) : base(data, key, message, resultType, exception) => Data = data;

    public static new BlockResult<T> Completed(string? message = null) => new(default, null, message, BlockResultType.Completed, null);
    public static BlockResult<T> Skip() => new(default, null, null, BlockResultType.Skip, null);
    public static new BlockResult<T> Error(string? message = null, Exception? exception = null) => new(default, null, message, BlockResultType.Error, exception);
    public static BlockResult<T> BackToCheckpoint(string? key = null) => new(default, key, null, BlockResultType.BackToCheckpoint, null);
    public static BlockResult<T> BackToExit(string? key = null) => new(default, key, null, BlockResultType.BackToExit, null);
}
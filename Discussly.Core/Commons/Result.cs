using System.Diagnostics.CodeAnalysis;

namespace Discussly.Core.Commons
{
    public abstract class Result
    {
        [MemberNotNullWhen(true, nameof(Error))]
        public abstract bool IsSuccess { get; }
        [MemberNotNullWhen(true, nameof(Error))]
        public abstract bool IsFailure { get; }
        public abstract string? Error { get; }

        public static Result Success() => new Result<object?>(true, null, string.Empty);
        public static Result<T> Success<T>(T value) => Result<T>.Success(value);
        public static Result Failure(string error) => new Result<object?>(false, default, error);
        public static Result<T> Failure<T>(string error) => Result<T>.Failure(error);
        public Result Combine(Result nextResult)
            => IsSuccess ? nextResult : this;
    }

    public sealed class Result<T> : Result
    {
        [MemberNotNullWhen(true, nameof(Value))]
        [MemberNotNullWhen(false, nameof(Error))]
        public override bool IsSuccess { get; }
        [MemberNotNullWhen(false, nameof(Value))]
        [MemberNotNullWhen(true, nameof(Error))]
        public override bool IsFailure => !IsSuccess;
        public override string? Error { get; }
        public T? Value { get; }

        internal Result(bool isSuccess, T? value, string error)
        {
            IsSuccess = isSuccess;
            Error = error;
            Value = value;
        }

        public static Result<T> Success(T value) => new(true, value, string.Empty);
        public static new Result<T> Failure(string error) => new(false, default, error);
        public T GetValueOrThrow() => IsSuccess ? Value! : throw new InvalidOperationException(Error);

        public Result<TOut> Map<TOut>(Func<T, TOut> mapper)
        => IsSuccess ? Result<TOut>.Success(mapper(Value!)) : Result<TOut>.Failure(Error);

        public Result<TOut> Bind<TOut>(Func<T, Result<TOut>> binder)
            => IsSuccess ? binder(Value!) : Result<TOut>.Failure(Error);

        public TOut Match<TOut>(Func<T, TOut> onSuccess, Func<string, TOut> onFailure)
            => IsSuccess ? onSuccess(Value!) : onFailure(Error);
    }
}

namespace Discussly.Core.Commons
{
    public abstract class Result
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public string Error { get; } = string.Empty;

        protected Result(bool isSuccess, string error)
        {
            if (isSuccess && !string.IsNullOrEmpty(error))
                throw new InvalidOperationException("Successful result cannot have error");

            if (!isSuccess && string.IsNullOrEmpty(error))
                throw new InvalidOperationException("Failed result must have error");

            IsSuccess = isSuccess;
            Error = error;
        }

        public static Result Success() => new Result<object?>(true, null, string.Empty);
        public static Result<T> Success<T>(T value) => Result<T>.Success(value);
        public static Result Failure(string error) => new Result<object?>(false, default, error);
        public static Result<T> Failure<T>(string error) => Result<T>.Failure(error);
        public Result Combine(Result nextResult)
            => IsSuccess ? nextResult : this;
    }

    public sealed class Result<T> : Result
    {
        public T? Value { get; }

        internal Result(bool isSuccess, T? value, string error) : base(isSuccess, error)
        {
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

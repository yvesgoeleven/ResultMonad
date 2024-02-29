namespace Result
{
    public readonly struct Result<L, R>
    {
        private readonly R _right;
        private readonly L _left;
        private readonly bool _isRight;
        private readonly bool _isLeft;

        private Result(R right)
        {
            this._isRight = true;
            this._isLeft = false;
            this._right = right;
            this._left = default;
        }
        private Result(L left)
        {
            this._isLeft = true;
            this._isRight = false;
            this._left = left;
            this._right = default;
        }

        public static implicit operator Result<L, R>(R value) =>
            new Result<L, R>(value);

        public static implicit operator Result<L, R>(L value) =>
            new Result<L, R>(value);

        public static Result<L, R> From(R value) => new Result<L, R>(value);

        public Result<L, R2> Map<R2>(Func<R, R2> map) =>
            this._isRight
                ? new Result<L, R2>(map(_right))
                : new Result<L, R2>(_left);

        public Result<L, R2> Bind<R2>(Func<R, Result<L, R2>> f) =>
            this._isRight
                ? f(_right)
                : new Result<L, R2>(_left);
       
        public async Task<Result<L, R2>> BindAsync<R2>(Func<R, Task<Result<L, R2>>> bind) =>
            this._isRight
                ? await bind(this._right).ConfigureAwait(false)
                : new Result<L, R2>(_left);

        public T Fold<T>(Func<L, T> leftFunc, Func<R, T> rightFunc) =>
           this._isRight
               ? rightFunc(_right)
               : leftFunc(_left);

    }


    public static class ResultExtension
    {
        public static Result<L, V> SelectMany<U, V, L, R>(this Result<L, R> first, Func<R, Result<L, U>> second, Func<R, U, V> project)
        {
            return first.Bind(a => second(a).Map(b => project(a, b)));
        }
        public static Result<L, U> Select<U, L, R>(this Result<L, R> first, Func<R, U> map) => first.Map(map);

    }

    public static class TaskExtension
    {
        public static Task<Result<L, R>> ToAsync<L, R>(this Result<L, R> result)
        {
            return Task.FromResult(result);
        }

        public static Task<Result<L, R>> ToAsync<L, R>(this R value)
        {
            return Task.FromResult(Result<L, R>.From(value));
        }

        public static async Task<Result<L, U>> Select<U, L, R>(this Task<Result<L, R>> first, Func<R, U> map) => (await first).Map(map);
        public static async Task<Result<L, V>> SelectMany<U, V, L, R>(this Task<Result<L, R>> first, Func<R, Task<Result<L, U>>> second, Func<R, U, V> project)
        {
            return await (await first).BindAsync(async a => (await second(a)).Map(b => project(a, b)));
        }

        public static async Task<Result<L, V>> SelectMany<U, V, L, R>(this Result<L, R> first, Func<R, Task<Result<L, U>>> second, Func<R, U, V> project)
        {
            return await first.BindAsync(async a => (await second(a)).Map(b => project(a, b)));
        }

        public static async Task<Result<L, V>> SelectMany<U, V, L, R>(this Task<Result<L, R>> first, Func<R, Result<L, U>> second, Func<R, U, V> project)
        {
            return (await first).Bind(a => second(a).Map(b => project(a, b)));
        }
    }
}
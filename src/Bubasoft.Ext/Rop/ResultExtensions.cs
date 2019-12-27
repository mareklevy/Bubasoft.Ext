namespace Bubasoft.Ext.Rop
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    ///
    /// </summary>
    public static class ResultExtensions
    {
        /// <summary>
        /// Handle
        /// </summary>
        /// <typeparam name="TSuccess"></typeparam>
        /// <typeparam name="TFailure"></typeparam>
        /// <param name="result"></param>
        /// <param name="onSuccess"></param>
        /// <param name="onFailure"></param>
        public static void Handle<TSuccess, TFailure>(this Result<TSuccess, TFailure> result,
            Action<TSuccess> onSuccess,
            Action<TFailure> onFailure)
        {
            if (result.IsSuccess)
                onSuccess(result.Success);
            else
                onFailure(result.Failure);
        }

        /// <summary>
        /// Either
        /// </summary>
        /// <typeparam name="TSuccess"></typeparam>
        /// <typeparam name="TFailure"></typeparam>
        /// <typeparam name="TSuccess2"></typeparam>
        /// <typeparam name="TFailure2"></typeparam>
        /// <param name="x"></param>
        /// <param name="onSuccess"></param>
        /// <param name="onFailure"></param>
        /// <returns></returns>
        public static Result<TSuccess2, TFailure2> Either<TSuccess, TFailure, TSuccess2, TFailure2>(
            this Result<TSuccess, TFailure> x,
            Func<Result<TSuccess, TFailure>, Result<TSuccess2, TFailure2>> onSuccess,
            Func<Result<TSuccess, TFailure>, Result<TSuccess2, TFailure2>> onFailure)
        {
            return x.IsSuccess ? onSuccess(x) : onFailure(x);
        }

        /// <summary>
        /// Either
        /// </summary>
        /// <typeparam name="TSuccess"></typeparam>
        /// <typeparam name="TFailure"></typeparam>
        /// <typeparam name="TSuccess2"></typeparam>
        /// <typeparam name="TFailure2"></typeparam>
        /// <param name="x"></param>
        /// <param name="onSuccess"></param>
        /// <param name="onFailure"></param>
        /// <returns></returns>
        public static async Task<Result<TSuccess2, TFailure2>> EitherAsync<TSuccess, TFailure, TSuccess2, TFailure2>(
            this Task<Result<TSuccess, TFailure>> x,
            Func<Result<TSuccess, TFailure>, Task<Result<TSuccess2, TFailure2>>> onSuccess,
            Func<Result<TSuccess, TFailure>, Task<Result<TSuccess2, TFailure2>>> onFailure)
        {
            var result = await x;

            return result.IsSuccess ? await onSuccess(result) : await onFailure(result);
        }

        /// <summary>
        /// Whatever x is, make it a failure.
        /// The trick is that failure is an array type, can it can be made an empty array failure.
        /// </summary>
        /// <typeparam name="TSuccess"></typeparam>
        /// <typeparam name="TFailure"></typeparam>
        /// <param name="x"></param>
        /// <returns></returns>
        public static Result<TSuccess, TFailure[]> ToFailure<TSuccess, TFailure>(
            this Result<TSuccess, TFailure[]> x)
        {
            return x.Either(
                a => Result<TSuccess, TFailure[]>.Failed(new TFailure[0]),
                b => b
                );
        }

        /// <summary>
        /// Put accumulator and next together.
        /// If they are both successes, then put them together as a success.
        /// If either/both are failures, then put them together as a failure.
        /// Because success and failure is an array, they can be put together
        /// </summary>
        /// <typeparam name="TSuccess"></typeparam>
        /// <typeparam name="TFailure"></typeparam>
        /// <param name="accumulator"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        public static Result<TSuccess[], TFailure[]> Merge<TSuccess, TFailure>(
            this Result<TSuccess[], TFailure[]> accumulator,
            Result<TSuccess, TFailure[]> next)
        {
            if (accumulator.IsSuccess && next.IsSuccess)
            {
                return Result<TSuccess[], TFailure[]>
                    .Succeeded(accumulator.Success.Concat(new List<TSuccess>() { next.Success })
                        .ToArray());
            }
            return Result<TSuccess[], TFailure[]>
                .Failed(accumulator.ToFailure().Failure.Concat(next.ToFailure().Failure).ToArray());
        }

        /// <summary>
        //// Aggregate an array of results together.
        //// If any of the results fail, return combined failures
        //// Will only return success if all results succeed
        /// </summary>
        /// <typeparam name="TSuccess"></typeparam>
        /// <typeparam name="TFailure"></typeparam>
        /// <param name="accumulator"></param>
        /// <returns></returns>
        public static Result<TSuccess[], TFailure[]> Aggregate<TSuccess, TFailure>(
            this IEnumerable<Result<TSuccess, TFailure[]>> accumulator)
        {
            var emptySuccess = Result<TSuccess[], TFailure[]>.Succeeded(new TSuccess[0]);
            return accumulator.Aggregate(emptySuccess, (acc, o) => acc.Merge(o));
        }

        /// <summary>
        /// Map: functional map
        /// if x is a a success call f, otherwise pass it through as a failure
        /// </summary>
        /// <typeparam name="TSuccess"></typeparam>
        /// <typeparam name="TFailure"></typeparam>
        /// <typeparam name="TSuccessNew"></typeparam>
        /// <param name="x"></param>
        /// <param name="f"></param>
        /// <returns></returns>
        public static Result<TSuccessNew, TFailure> Map<TSuccess, TFailure, TSuccessNew>(
            this Result<TSuccess, TFailure> x,
            Func<TSuccess, TSuccessNew> f)
        {
            return x.IsSuccess
                ? Result<TSuccessNew, TFailure>.Succeeded(f(x.Success))
                : Result<TSuccessNew, TFailure>.Failed(x.Failure);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TSuccess"></typeparam>
        /// <typeparam name="TFailure"></typeparam>
        /// <typeparam name="TSuccessNew"></typeparam>
        /// <param name="x"></param>
        /// <param name="f"></param>
        /// <returns></returns>
        public static async Task<Result<TSuccessNew, TFailure>> MapAsync<TSuccess, TFailure, TSuccessNew>(
            this Task<Result<TSuccess, TFailure>> x,
            Func<TSuccess, Task<TSuccessNew>> f)
        {
            var result = await x;

            return result.IsSuccess
                ? Result<TSuccessNew, TFailure>.Succeeded(await f(result.Success).ConfigureAwait(false))
                : Result<TSuccessNew, TFailure>.Failed(result.Failure);
        }

        /// <summary>
        /// Bind: functional bind
        /// Monadize it!
        /// </summary>
        /// <typeparam name="TSuccess"></typeparam>
        /// <typeparam name="TFailure"></typeparam>
        /// <typeparam name="TSuccessNew"></typeparam>
        /// <param name="x"></param>
        /// <param name="f"></param>
        /// <returns></returns>
        public static Result<TSuccessNew, TFailure> Bind<TSuccess, TFailure, TSuccessNew>(
            this Result<TSuccess, TFailure> x,
            Func<TSuccess, Result<TSuccessNew, TFailure>> f)
        {
            return x.IsSuccess
                ? f(x.Success)
                : Result<TSuccessNew, TFailure>.Failed(x.Failure);
        }

        /// <summary>
        /// Bind: functional bind
        /// </summary>
        /// <typeparam name="TSuccess"></typeparam>
        /// <typeparam name="TFailure"></typeparam>
        /// <typeparam name="TSuccessNew"></typeparam>
        /// <param name="x"></param>
        /// <param name="f"></param>
        /// <returns></returns>
        public static async Task<Result<TSuccessNew, TFailure>> BindAsync<TSuccess, TFailure, TSuccessNew>(
           this Task<Result<TSuccess, TFailure>> x,
           Func<TSuccess, Task<Result<TSuccessNew, TFailure>>> f)
        {
            var result = await x;

            return result.IsSuccess
                ? await f.Invoke(result.Success).ConfigureAwait(continueOnCapturedContext: false)
                : Result<TSuccessNew, TFailure>.Failed(result.Failure);
        }

        /// <summary>
        /// Tee
        /// </summary>
        /// <typeparam name="TSuccess"></typeparam>
        /// <typeparam name="TFailure"></typeparam>
        /// <param name="x"></param>
        /// <param name="f"></param>
        /// <returns></returns>
        public static Result<TSuccess, TFailure> Tee<TSuccess, TFailure>(this Result<TSuccess, TFailure> x, Action<TSuccess> f)
        {
            if (x.IsSuccess)
            {
                f(x.Success);
            }

            return x;
        }

        /// <summary>
        /// Tee
        /// </summary>
        /// <typeparam name="TSuccess"></typeparam>
        /// <typeparam name="TFailure"></typeparam>
        /// <param name="x"></param>
        /// <param name="f"></param>
        /// <returns></returns>
        public static async Task<Result<TSuccess, TFailure>> TeeAsync<TSuccess, TFailure>(this Task<Result<TSuccess, TFailure>> x, Func<TSuccess, Task> f)
        {
            var result = await x;

            if (result.IsSuccess)
            {
                await f(result.Success);
            }

            return result;
        }

        /// <summary>
        /// Evaluates a specified action if Success is present.
        /// </summary>
        /// <typeparam name="TSuccess"></typeparam>
        /// <typeparam name="TFailure"></typeparam>
        /// <param name="x"></param>
        /// <param name="f"></param>
        public static void MatchSuccess<TSuccess, TFailure>(this Result<TSuccess, TFailure> x, Action<TSuccess> f)
        {
            if (x.IsFailure)
            {
                return;
            }

            f.Invoke(x.Success);
        }

        /// <summary>
        /// Evaluates a specified action if Success is present.
        /// </summary>
        /// <typeparam name="TSuccess"></typeparam>
        /// <typeparam name="TFailure"></typeparam>
        /// <param name="x"></param>
        /// <param name="f"></param>
        /// <returns></returns>
        public static async Task MatchSuccessAsync<TSuccess, TFailure>(this Task<Result<TSuccess, TFailure>> x, Func<TSuccess, Task> f)
        {
            var result = await x;

            if (result.IsFailure)
            {
                return;
            }

            await f.Invoke(result.Success).ConfigureAwait(continueOnCapturedContext: false); ;
        }

        /// <summary>
        /// Evaluates a specified action if Failure is present.
        /// </summary>
        /// <typeparam name="TSuccess"></typeparam>
        /// <typeparam name="TFailure"></typeparam>
        /// <param name="x"></param>
        /// <param name="f"></param>
        public static void MatchFailure<TSuccess, TFailure>(this Result<TSuccess, TFailure> x, Action<TFailure> f)
        {
            if (x.IsSuccess)
            {
                return;
            }

            f.Invoke(x.Failure);
        }

        /// <summary>
        /// Evaluates a specified action if Failure is present.
        /// </summary>
        /// <typeparam name="TSuccess"></typeparam>
        /// <typeparam name="TFailure"></typeparam>
        /// <param name="x"></param>
        /// <param name="f"></param>
        /// <returns></returns>
        public static async Task MatchFailureAsync<TSuccess, TFailure>(this Task<Result<TSuccess, TFailure>> x, Func<TFailure, Task> f)
        {
            var result = await x;

            if (result.IsSuccess)
            {
                return;
            }

            await f.Invoke(result.Failure).ConfigureAwait(continueOnCapturedContext: false); ;
        }
    }
}

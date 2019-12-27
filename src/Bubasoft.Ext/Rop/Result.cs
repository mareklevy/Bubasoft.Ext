using System;

namespace Bubasoft.Ext.Rop
{
    /// <summary>
    /// Railway Oriented Programming
    /// https://www.youtube.com/watch?v=uM906cqdFWE
    /// </summary>
    /// <typeparam name="TSuccess"></typeparam>
    /// <typeparam name="TFailure"></typeparam>
    public class Result<TSuccess, TFailure>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="success"></param>
        /// <returns></returns>
        public static Result<TSuccess, TFailure> Succeeded(TSuccess success)
        {
            if (success == null) throw new ArgumentNullException(nameof(success));

            return new Result<TSuccess, TFailure>
            {
                IsSuccessful = true,
                Success = success
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="failure"></param>
        /// <returns></returns>
        public static Result<TSuccess, TFailure> Failed(TFailure failure)
        {
            if (failure == null) throw new ArgumentNullException(nameof(failure));

            return new Result<TSuccess, TFailure>
            {
                IsSuccessful = false,
                Failure = failure
            };
        }

        private Result()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsSuccess => IsSuccessful;

        /// <summary>
        /// 
        /// </summary>
        public bool IsFailure => !IsSuccessful;

        /// <summary>
        /// 
        /// </summary>
        public TSuccess Success { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public TFailure Failure { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        private bool IsSuccessful { get; set; }
    }
}

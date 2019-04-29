﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace TReX.Kernel.Shared
{
    public static class Extensions
    {
        public static Maybe<T> ToMaybe<T>(this T subject)
        {
            return subject;
        }

        public static Maybe<T> FirstOrNothing<T>(this IEnumerable<T> enumerable)
        {
            return enumerable.FirstOrDefault();
        }

        public static Maybe<T> FirstOrNothing<T>(this IEnumerable<T> enumerable, Func<T, bool> func)
        {
            return enumerable.FirstOrDefault(func);
        }

        public static Result OnFailureCompensate<T>(this Result<T> result, Func<Result> compensation)
        {
            if (result.IsFailure)
            {
                return compensation();
            }

            return result;
        }

        public static Maybe<T> ToMaybe<T>(this Result<T> result)
        {
            if (result.IsSuccess)
            {
                return Maybe<T>.From(result.Value);
            }

            return Maybe<T>.None;
        }

        public static async Task<Maybe<T>> ToMaybe<T>(this Task<Result<T>> resultTask)
        {
            return (await resultTask).ToMaybe();
        }

        public static async Task<Result> TryAsync(Func<Task> func)
        {
            try
            {
                await func();
                return Result.Ok();
            }
            catch (Exception e)
            {
                return Result.Fail(e.Message);
            }
        }

        public static async Task<Result<T>> TryAsync<T>(Func<Task<T>> func)
        {
            try
            {
                return Result.Ok(await func());
            }
            catch (Exception e)
            {
                return Result.Fail<T>(e.Message);
            }
        }
    }
}
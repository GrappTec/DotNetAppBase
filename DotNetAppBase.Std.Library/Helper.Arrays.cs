using System;
using System.Threading;
using System.Threading.Tasks;
using DotNetAppBase.Std.Exceptions.Assert;
using DotNetAppBase.Std.Exceptions.Bussines;

// ReSharper disable UnusedMember.Global

namespace DotNetAppBase.Std.Library
{
    public partial class XHelper
    {
        public static class Flow
        {
            public static TData TryGet<TData>(Func<TData> funcGetData, TData failValue = default) => !TryGet(funcGetData, out var data, out _) ? failValue : data;

            public static bool TryGet<TData>(Func<TData> funcGetData, out TData data, out string error)
            {
                try
                {
                    error = null;
                    data = funcGetData();

                    return true;
                }
                catch (Exception ex)
                {
                    XDebug.OnException(ex);

                    error = Exceptions.GetMessageOnTopOfStack(ex);
                    data = default;

                    return false;
                }
            }

            public static async Task<T> TryGetWithAttempt<T>(Func<T> funcGet, CancellationToken cancellationToken, T defaultValue = default, int attempts = 1, int delayBetweenAttempts = 200)
            {
                try
                {
                    return funcGet();
                }
                catch (Exception)
                {
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        if (--attempts > 0)
                        {
                            await Task.Delay(delayBetweenAttempts, cancellationToken);

                            if (!cancellationToken.IsCancellationRequested)
                            {
                                return await TryGetWithAttempt(funcGet, cancellationToken, defaultValue, attempts, delayBetweenAttempts);
                            }
                        }
                    }

                    return defaultValue;
                }
            }

            public static async Task<bool> TryExecuteWithAttempt(Action action, CancellationToken cancellationToken, int attempts = 1, int delayBetweenAttempts = 200)
            {
                try
                {
                    action();

                    return true;
                }
                catch (Exception)
                {
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        if (--attempts > 0)
                        {
                            await Task.Delay(delayBetweenAttempts, cancellationToken);

                            if (!cancellationToken.IsCancellationRequested)
                            {
                                return await TryExecuteWithAttempt(action, cancellationToken, attempts, delayBetweenAttempts);
                            }
                        }
                    }

                    return false;
                }
            }

            public static bool Ensure(Action action)
            {
                try
                {
                    action();

                    return true;
                }
                catch (Exception e)
                {
                    XDebug.OnException(e);

                    return true;
                }
            }

            public static bool Ensure(Action action, out string error)
            {
                try
                {
                    action();

                    error = null;

                    return true;
                }
                catch (Exception e)
                {
                    XDebug.OnException(e);

                    error = Exceptions.GetMessageOnTopOfStack(e);
                    return true;
                }
            }

            public static T FlowMustBeNotNull<T>(T obj)
            {
                if (Obj.IsNull(obj))
                {
                    throw XFlowException.Create("Is expecting not null data, but it wasn't attended.");
                }

                return obj;
            }

            public static bool IfIs<T>(object obj, Action<T> isAction) where T : class
            {
                XContract.ArgIsNotNull(isAction, nameof(isAction));

                var cast = obj.As<T>();
                if (cast == null)
                {
                    return false;
                }

                isAction.Invoke(cast);
                return true;
            }

            public static TResult IfIsGet<T, TResult>(object obj, Func<T, TResult> getAction, TResult defaultValue) where T : class
            {
                XContract.ArgIsNotNull(getAction, nameof(getAction));

                var cast = obj.As<T>();
                if (cast == null)
                {
                    return defaultValue;
                }

                return getAction.Invoke(cast);
            }

            public static bool IfIsNot<T>(object obj, Action notIsAction) where T : class
            {
                XContract.ArgIsNotNull(notIsAction, nameof(notIsAction));

                var cast = obj.As<T>();
                if (cast != null)
                {
                    return false;
                }

                notIsAction.Invoke();

                return true;
            }

            public static void IfIsNotNull<T>(T obj, Action<T> notNullAction, Action nullAction = null) where T : class
            {
                XContract.ArgIsNotNull(notNullAction, nameof(notNullAction));

                if (obj == null)
                {
                    nullAction?.Invoke();

                    return;
                }

                notNullAction(obj);
            }

            public static TResult IfIsNotNull<T, TResult>(T obj, Func<T, TResult> notNullAction, Func<TResult> nullAction) where T : class
            {
                XContract.ArgIsNotNull(notNullAction, nameof(notNullAction));
                XContract.ArgIsNotNull(nullAction, nameof(nullAction));

                if (obj == null)
                {
                    return nullAction();
                }

                return notNullAction(obj);
            }

            public static TReturn IfIsNotNull<T, TReturn>(T obj, Func<T, TReturn> notNullFunc, Action nullAction = null) where T : class
            {
                XContract.ArgIsNotNull(notNullFunc, nameof(notNullFunc));

                if (obj == null)
                {
                    nullAction?.Invoke();

                    return default;
                }

                return notNullFunc(obj);
            }
        }
    }
}
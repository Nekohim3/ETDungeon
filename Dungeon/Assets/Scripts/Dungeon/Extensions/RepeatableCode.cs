﻿using System;

namespace Assets._Scripts.Extensions
{
    public static class RepeatableCode
    {
        public static Exception? Repeat<T>(Func<T> func, int count)
        {
            return null;
        }

        public static (T? result, Exception? exception) RepeatResultWithException<T>(Func<T?> func, int count) where T : class
        {
            Exception? exception = null;
            for (var i = 0; i < count; i++)
            {
                try
                {
                    var res = func.Invoke();
                    if (res != null)
                        return (res, null);
                }
                catch (Exception e)
                {
                    exception = e;
                }
            }

            return (null, exception);
        }

        public static T? RepeatResult<T>(Func<T?> func, int count) where T : class
        {
            for (var i = 0; i < count; i++)
            {
                var res = func.Invoke();
                if (res != null)
                    return res;
            }

            return null;
        }
        public static bool RepeatResult(Func<bool> func, int count)
        {
            for (var i = 0; i < count; i++)
            {
                var res = func.Invoke();
                if (res)
                    return res;
            }

            return false;
        }
    }
}
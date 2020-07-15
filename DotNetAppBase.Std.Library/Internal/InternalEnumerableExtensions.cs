﻿#region License

// Copyright(c) 2020 GrappTec
// 
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

#endregion

using System;
using System.Collections.Generic;
using DotNetAppBase.Std.Exceptions.Assert;
using DotNetAppBase.Std.Library;

// ReSharper disable CheckNamespace
internal static class InternalEnumerableExtensions
// ReSharper restore CheckNamespace
{
    public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
    {
        XContract.ArgIsNotNull(enumerable, nameof(enumerable));
        XContract.ArgIsNotNull(action, nameof(action));

        foreach (var item in enumerable)
        {
            action(item);
        }
    }

    public static bool IsNullOrEmpty<T>(this IEnumerable<T> enumerable) => XHelper.Enumerable.IsNullOrEmpty(enumerable);

    public static T[] ToArrayEfficient<T>(this IEnumerable<T> values) => XHelper.Enumerable.ToArrayEfficient(values);
}
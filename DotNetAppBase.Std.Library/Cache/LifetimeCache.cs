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

namespace DotNetAppBase.Std.Library.Cache
{
    public class LifetimeCache<TValue>
    {
        private readonly Func<TValue> _funcCreate;
        private readonly object _syncReadValue = new object();
        private DateTime _cached;

        private TValue _value;

        public LifetimeCache(long lifetime, Func<TValue> funcCreate)
        {
            _funcCreate = funcCreate;

            Lifetime = lifetime;
        }

        public long Lifetime { get; }

        public TValue Value
        {
            get
            {
                lock (_syncReadValue)
                {
                    var now = DateTime.Now;

                    if (_cached.AddMilliseconds(Lifetime) >= now)
                    {
                        return _value;
                    }

                    _value = _funcCreate();
                    _cached = now;

                    return _value;
                }
            }
        }
    }
}
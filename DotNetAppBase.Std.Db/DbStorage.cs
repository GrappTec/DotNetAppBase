#region License

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
using System.Linq;
using DotNetAppBase.Std.Db.Contract;

namespace DotNetAppBase.Std.Db
{
    public sealed class DbStorage : IDbStorage
    {
        public static readonly DbStorage Instance;

        private readonly Dictionary<string, IDbDatabase> _dbs;

        static DbStorage()
        {
            Instance = new DbStorage();
        }

        private DbStorage()
        {
            _dbs = new Dictionary<string, IDbDatabase>();
        }

        public IDbDatabase DefaultDatabase { get; set; }

        public bool Constains(string name) => !string.IsNullOrEmpty(name) && _dbs.ContainsKey(name);

        public IDbDatabase Restore(string name) => Constains(name) ? _dbs[name] : null;

        public bool Storage(IDbDatabase database)
        {
            if (Constains(database.Name))
            {
                return false;
            }

            _dbs.Add(database.Name, database);

            return true;
        }

        public bool UnStorage(DbDatabase dataBase)
        {
            if (!Constains(dataBase.Name) || Restore(dataBase.Name) != dataBase)
            {
                return false;
            }

            _dbs.Remove(dataBase.Name);

            return true;
        }
    }
}
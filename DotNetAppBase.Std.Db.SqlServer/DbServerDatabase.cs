﻿using System;
using System.ComponentModel;
using System.Data.Common;
using System.Security;
using DotNetAppBase.Std.Db.Contract;

#if NETFRAMEWORK
using System.Data.SqlClient;
#else
using Microsoft.Data.SqlClient;
#endif

namespace DotNetAppBase.Std.Db.SqlServer
{
    [Localizable(false)]
    public class DbServerDatabase : DbDatabase
    {
        private readonly SqlCredential _credential;
        private readonly Lazy<SqlConnectionStringBuilder> _lazyConnStrBuilder;

        private string _connectionString;

        public DbServerDatabase(string name, string connectionString, string user, SecureString pwd) : base(name)
        {
            _connectionString = connectionString;
            _credential = new SqlCredential(user, pwd);

            _lazyConnStrBuilder = new Lazy<SqlConnectionStringBuilder>(
                () => new SqlConnectionStringBuilder(_connectionString));
        }

        public DbServerDatabase(string name, string connectionString) : base(name)
        {
            _connectionString = connectionString;

            _lazyConnStrBuilder = new Lazy<SqlConnectionStringBuilder>(
                () => new SqlConnectionStringBuilder(_connectionString));
        }

        public override int ConnectionTimeout
        {
            get => Builder.ConnectTimeout;
            set
            {
                if (ConnectionTimeout == value)
                {
                    return;
                }

                Builder.ConnectTimeout = value;

                _connectionString = Builder.ToString();
            }
        }

        private SqlConnectionStringBuilder Builder => _lazyConnStrBuilder.Value;

        protected override DbConnection InternalCreateConnection(string connectionString) => new SqlConnection(connectionString, _credential);

        protected override IDbSession InternalCreateSqlSession() => new DbServerSession(this);

        protected override string InternalGetConnectionString() => _connectionString;

        protected override DateTime InternalGetDateTime(int connectionTimeout = 5)
        {
            var local = new SqlConnectionStringBuilder(_connectionString)
                {
                    ConnectTimeout = connectionTimeout
                };

            using var conn = InternalCreateConnection(local.ToString());
            using var comm = conn.CreateCommand();
            
            conn.Open();

            comm.CommandText = "SELECT CONVERT(datetimeoffset, GETDATE) AT TIME ZONE 'E. South America Standard Time'";
            var dt = (DateTime)comm.ExecuteScalar();

            return dt;
        }
    }
}
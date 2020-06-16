using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using DotNetAppBase.Std.Db.Contract;
using DotNetAppBase.Std.Db.Enums;
using DotNetAppBase.Std.Db.SqlTrace;
using DotNetAppBase.Std.Exceptions.Assert;
using DotNetAppBase.Std.Exceptions.Base;
using DotNetAppBase.Std.Library;

namespace DotNetAppBase.Std.Db
{
    public partial class DbAccess : IDbAccess
    {
        private readonly List<IDisposable> _disposableObjects = new List<IDisposable>();

        public EventHandler Disposed;
        private bool _disposed;

        private IDbSession _session;

        public DbAccess()
        {
            if(DbStorage.Instance.DefaultDatabase == null)
            {
                throw new XException("N�o foi poss�vel recuperar a base de dados default.");
            }

            if(DbStorage.Instance.DefaultDatabase.DefaultSession is DbSession session)
            {
                session.AddAccess(this);
            }
            else
            {
                Session = DbStorage.Instance.DefaultDatabase.DefaultSession;
            }
        }

        public DbAccess(IDbSession session) : this(session, session.BuildContext()) { }

        public DbAccess(IDbSession session, IDbContext context)
        {
            _session = session;

            Context = context;
        }

        public object Calller { get; set; }

        public int? CommandTimeout { get; set; }

        public IDbContext Context { get; private set; }

        public IDbSession Session
        {
            get => _session;
            internal set
            {
                if(_session == value)
                {
                    return;
                }

                _session = value;
                Context = _session.BuildContext();
            }
        }

        public EDbContextState TransactionState => Context.State;

        public TResult ExecFunc<TResult, TParam>(string funcName, params TParam[] parameters) where TParam : DbParameter
        {
            try
            {
                return InternalExecuteFunc<TResult, TParam>(funcName, parameters);
            }
            catch(DbException ex)
            {
                if(_session.RetryInteractionOnDbExcepion(ex))
                {
                    return InternalExecuteFunc<TResult, TParam>(funcName, parameters);
                }

                XDebug.OnException(ex);

                throw;
            }
        }

        public TResult ExecProc<TResult, TParam>(string procName, params TParam[] parameters) where TParam : DbParameter
        {
            try
            {
                return InternalExecuteProc<TResult, TParam>(procName, parameters);
            }
            catch(DbException ex)
            {
                if(_session.RetryInteractionOnDbExcepion(ex))
                {
                    return InternalExecuteProc<TResult, TParam>(procName, parameters);
                }

                XDebug.OnException(ex);

                throw;
            }
        }

        public int ExecProc<TParam>(string procName, params TParam[] parameters) where TParam : DbParameter => ExecProc<Count, TParam>(procName, parameters);

        public void ExecProcAndFill<TParam>(DataTable dataTable, string procName, params TParam[] parameters) where TParam : DbParameter
        {
            try
            {
                InternalExecuteProcAndFill(dataTable, procName, parameters);
            }
            catch(DbException ex)
            {
                if(_session.RetryInteractionOnDbExcepion(ex))
                {
                    InternalExecuteProcAndFill(dataTable, procName, parameters);

                    return;
                }

                XDebug.OnException(ex);

                throw;
            }
        }

        public int ExecText<TParam>(string sql, params TParam[] parameters) where TParam : DbParameter => ExecText<Count, TParam>(sql, parameters);

        public TResult ExecText<TResult, TParam>(string sql, params TParam[] parameters) where TParam : DbParameter => ExecText<TResult, TParam>(sql, CommandBehavior.Default, parameters);

        public TResult ExecText<TResult, TParam>(string sql, CommandBehavior behavior, params TParam[] parameters) where TParam : DbParameter
        {
            try
            {
                return InternalExecuteText<TResult, TParam>(sql, parameters, behavior);
            }
            catch(DbException ex)
            {
                if(_session.RetryInteractionOnDbExcepion(ex))
                {
                    return InternalExecuteText<TResult, TParam>(sql, parameters, behavior);
                }

                XDebug.OnException(ex);

                throw;
            }
        }

        public void OpenConnection()
        {
            Context.Open();
        }

        private static EReturnType GetReturnType(Type type)
        {
            if(type == typeof(DataSet))
            {
                return EReturnType.DataSet;
            }

            if(type == typeof(DbDataReader))
            {
                return EReturnType.DataReader;
            }

            if(type == typeof(Count))
            {
                return EReturnType.Count;
            }

            if(type == typeof(DataTable))
            {
                return EReturnType.DataTable;
            }

            if(type == typeof(DataRow))
            {
                return EReturnType.DataRow;
            }

            if(type == typeof(Return))
            {
                return EReturnType.ProcReturn;
            }

            if(type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ReturnAndData<>))
            {
                return EReturnType.ProcReturnAndValue;
            }

            return EReturnType.Unknown;
        }

        private void AddDisposableObject(params IDisposable[] objects)
        {
            foreach(var obj in objects)
            {
                _disposableObjects.Add(obj);
            }
        }

        private static void AddParameters<TParam>(DbCommand command, IReadOnlyCollection<TParam> parameters) where TParam : DbParameter
        {
            if(parameters == null || parameters.Count == 0)
            {
                return;
            }

            foreach(var param in parameters)
            {
                if(param == null)
                {
                    continue;
                }

                command.Parameters.Add(param);
            }
        }

        private void ConfigCommandTimeout(IDbCommand command)
        {
            command.CommandTimeout = CommandTimeout ?? Session.Database.CommandTimeout;
        }

        private TResult InternalExecuteProc<TResult, TParam>(string procName, IReadOnlyCollection<TParam> parameters) where TParam : DbParameter
        {
            OpenConnection();

            var command = Context.CreateCommand();
            command.CommandText = procName;
            command.CommandType = CommandType.StoredProcedure;

            ConfigCommandTimeout(command);

            AddParameters(command, parameters);

            try
            {
                DbTraceProvider.Instance.Trace(this, command);

                var returnType = typeof(TResult);
                return (TResult)ExecuteCommand(returnType, command);
            }
            finally
            {
                command.Parameters.Clear();
            }
        }

        private TResult InternalExecuteFunc<TResult, TParam>(string funcName, IEnumerable<TParam> parameters) where TParam : DbParameter
        {
            var returnParameter = _session.CreateReturnParameter().CastTo<TParam>();

            InternalExecuteProc<Count, TParam>(funcName, parameters.Union(new[] {returnParameter}).ToArray());

            return XHelper.Sql.GetFromDbValue<TResult>(returnParameter.Value);
        }

        private void InternalExecuteProcAndFill<TParam>(DataTable dataTable, string procName, IReadOnlyCollection<TParam> parameters) where TParam : DbParameter
        {
            OpenConnection();

            var cmd = Context.CreateCommand();
            cmd.CommandText = procName;
            cmd.CommandType = CommandType.StoredProcedure;

            ConfigCommandTimeout(cmd);

            AddParameters(cmd, parameters);
            try
            {
                DbTraceProvider.Instance.Trace(this, cmd);

                using var da = _session.CreateDataAtapter(cmd);
                da.Fill(dataTable);
            }
            finally
            {
                cmd.Parameters.Clear();
            }
        }

        private TResult InternalExecuteText<TResult, TParam>(string sql, IReadOnlyCollection<TParam> parameters, CommandBehavior behavior) where TParam : DbParameter
        {
            OpenConnection();

            var command = Context.CreateCommand();
            command.CommandText = sql;
            command.CommandType = CommandType.Text;

            ConfigCommandTimeout(command);

            AddParameters(command, parameters);

            try
            {
                DbTraceProvider.Instance.Trace(this, command);

                var returnType = typeof(TResult);
                return (TResult)ExecuteCommand(returnType, command, behavior);
            }
            finally
            {
                command.Parameters.Clear();
            }
        }

        private object ExecuteCommand(Type returnType, DbCommand command, CommandBehavior behavior = CommandBehavior.Default)
        {
            var enumReturnType = GetReturnType(returnType);

            switch(enumReturnType)
            {
                case EReturnType.DataReader:
                {
                    var reader = command.ExecuteReader(behavior);
                    AddDisposableObject(command, reader);

                    return reader;
                }

                case EReturnType.DataSet:
                {
                    using var da = _session.CreateDataAtapter(command);
                    var ds = new DataSet();
                    da.Fill(ds);

                    return ds;
                }

                case EReturnType.DataTable:
                {
                    using var da = _session.CreateDataAtapter(command);
                    var ds = new DataSet();
                    da.Fill(ds);

                    return ds.Tables[0];
                }

                case EReturnType.DataRow:
                {
                    using var da = _session.CreateDataAtapter(command);
                    var ds = new DataSet();
                    da.Fill(ds);

                    return ds.Tables[0].Rows.Count == 0 ? null : ds.Tables[0].Rows[0];
                }

                case EReturnType.Count:
                {
                    return (Count)command.ExecuteNonQuery();
                }

                case EReturnType.ProcReturn:
                {
                    var returnParameter = _session.CreateReturnParameter();
                    command.Parameters.Add(returnParameter);

                    command.ExecuteNonQuery();

                    return Activator.CreateInstance(returnType, returnParameter.Value);
                }

                // Na teoria isso n�o funciona!
                case EReturnType.ProcReturnAndValue:
                {
                    var secondDataType = returnType.GetGenericArguments()[0];

                    var returnParameter = _session.CreateReturnParameter();
                    command.Parameters.Add(returnParameter);

                    var returnData = ExecuteCommand(secondDataType, command);

                    return Activator.CreateInstance(returnType, returnParameter.Value, returnData);
                }

                default:
                {
                    var data = command.ExecuteScalar();

                    if(returnType != typeof(object))
                    {
                        data = Convert.ChangeType(data, returnType);
                    }

                    return data;
                }
            }
        }

        #region Dispose Handle

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if(_disposed)
            {
                return;
            }

            _disposed = true;

            PartialDispose(disposing);

            if(disposing)
            {
                GC.SuppressFinalize(this);
            }

            if(Context is DbContext context)
            {
                context.Dispose(disposing);
            }
            else
            {
                Context?.Dispose();
            }

            var session = _session as DbSession;
            session?.RemoveAccess(this);

            Disposed?.Invoke(this, EventArgs.Empty);
        }

        public void PartialDispose()
        {
            PartialDispose(true);
        }

        public void PartialDispose(bool disposing)
        {
            foreach(var disposable in _disposableObjects.Where(disposable => !(disposable is DbDataReader) || disposing))
            {
                disposable.Dispose();
            }

            _disposableObjects.Clear();
        }

        ~DbAccess()
        {
            Dispose(false);
        }

        #endregion Dispose Handle
    }
}
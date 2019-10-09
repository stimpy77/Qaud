using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Qaud.DbProvider
{
    public class DbProviderDataStore<T> : IDataStore<T>, IDisposable
    {

        private DbConnection _connection;
        private DbProviderFactory _providerFactory;
        private EntityMemberResolver<T> _memberResolver;
        private string _connName;

        public DbProviderDataStore(DbProviderFactory providerFactory, string connectionName)
        {
            _providerFactory = providerFactory;
            _connection = _providerFactory.CreateConnection();
            _connection.ConnectionString = connectionName;
            _connName = connectionName;
            _memberResolver = new EntityMemberResolver<T>();
        }

        private void OpenIfClosed()
        {
            if (string.IsNullOrEmpty(_connection.ConnectionString))
            {
                var connStringConfig = ConfigurationManager.ConnectionStrings[_connName];
                _connection.ConnectionString = connStringConfig == null 
                    ? _connName 
                    : connStringConfig.ConnectionString;
            }
            if (_connection.State == ConnectionState.Broken) 
                _connection.Close();
            if (_connection.State == ConnectionState.Closed)
            {
                try
                {
                    _connection.Open();
                }
                catch
                {
                    var connstring = _connection.ConnectionString;
                    _connection = _providerFactory.CreateConnection();
                    _connection.ConnectionString = connstring;
                    _connection.Open();
                }
            }
        }

        public virtual T Create()
        {
            return Activator.CreateInstance<T>();
        }

        private DbCommandBuilder CreateCommandBuilder()
        {
            var cmdbuilder = _providerFactory.CreateCommandBuilder();
            cmdbuilder.DataAdapter = InitializeDataAdapter();
            return cmdbuilder;
        }

        private DbDataAdapter InitializeDataAdapter()
        {
            var adapter = _providerFactory.CreateDataAdapter();
            adapter.SelectCommand = InitializeSelectCommand(_providerFactory.CreateCommand());
            adapter.InsertCommand = InitializeInsertCommand(_providerFactory.CreateCommand());
            adapter.DeleteCommand = InitializeDeleteCommand(_providerFactory.CreateCommand());
            adapter.UpdateCommand = InitializeUpdateCommand(_providerFactory.CreateCommand());
            return adapter;
        }

        private DbCommand InitializeSelectCommand(DbCommand cmd)
        {
            cmd.Connection = CreateConnection();
            if (cmd.Connection.GetType().FullName.ToLower().Contains("sql"))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "SELECT * FROM " + StoreName;
            }
            else
            {
                cmd.CommandType = CommandType.TableDirect;
                cmd.CommandText = StoreName;
            }

            return cmd;
        }

        private DbCommand InitializeInsertCommand(DbCommand cmd)
        {
            cmd.Connection = CreateConnection();
            return cmd;
        }

        private DbCommand InitializeUpdateCommand(DbCommand cmd)
        {
            cmd.Connection = CreateConnection();
            return cmd;
        }

        private DbCommand InitializeDeleteCommand(DbCommand cmd)
        {
            cmd.Connection = CreateConnection();
            cmd.CommandText = "DELETE FROM " + StoreName + " WHERE "
                              + (string.Join(" AND ",
                                  _memberResolver.KeyPropertyMembers.Select(m => "[" + m.Name + "] = @" + m.Name)));
            return cmd;
        }

        private DbConnection CreateConnection()
        {
            var conn = _providerFactory.CreateConnection();
            conn.ConnectionString = _connName;
            return conn;
        }

        public virtual void Add(T item)
        {
            var cmdbuilder = CreateCommandBuilder();
            var insertcmd = cmdbuilder.GetInsertCommand(true);
            var dic = _memberResolver.ConvertToDictionary(item);
            foreach (var kvp in dic)
            {
                var t = kvp.Value.GetType();
                if (!IsComplexType(t))
                {
                    var param = insertcmd.Parameters.Cast<DbParameter>()
                        .First(
                            p =>
                                p.ParameterName.ToLower() == kvp.Key.ToLower() ||
                                p.ParameterName.ToLower() == "@" + kvp.Key.ToLower());
                    param.Value = kvp.Value;
                    if (t == typeof(string))
                        param.Size = ((string) kvp.Value ?? "").Length;
                }
            }
            insertcmd.Connection = _connection;
            OpenIfClosed();
            insertcmd.Prepare();
            insertcmd.ExecuteNonQuery();
        }

        private bool IsComplexType(Type t)
        {
            return !(t.IsPrimitive || 
                    t == typeof (DateTime) || t == typeof (DateTimeOffset) ||
                    t == typeof(string));
        }

        public virtual void Add(T item, out T result)
        {
            Add(item);
            result = item; // keygen tracking not yet supported, though it might be feasible
        }

        public virtual void AddRange(IEnumerable<T> items)
        {
            foreach (var item in items) Add(item);
        }

        public virtual T Get(params object[] key)
        {
            if (key == null) key = new object[] {};
            var cmdbuilder = CreateCommandBuilder();
            var selectCmd = cmdbuilder.DataAdapter.SelectCommand;
            var keymembers = _memberResolver.KeyPropertyMembers.ToArray();
            if (keymembers.Length != key.Length)
            {
                throw new ArgumentException("Key component count mismatch. Should match [Key] attribute count.", "key");
            }
            for (var i=0; i<key.Length; i++)
            {
                var parameter = selectCmd.CreateParameter();
                parameter.ParameterName = keymembers[i].Name;
                parameter.Value = key[i];
            }
            OpenIfClosed();
            selectCmd.Prepare();
            using (var result = selectCmd.ExecuteReader())
            {
                if (result.HasRows && result.Read())
                {
                    var dictionary = _memberResolver.ConvertToDictionary(result);
                    var item = Create();
                    _memberResolver.HydrateFromDictionary(item, dictionary);
                    return item;
                }
                return default(T);
            }
        }

        public virtual T Get(T lookup)
        {
            return Get(_memberResolver.GetKeyPropertyValues(lookup));
        }

        public virtual void Update(T item)
        {
            var cmdbuilder = CreateCommandBuilder();
            var updatecmd = cmdbuilder.GetUpdateCommand(true);
            var dic = _memberResolver.ConvertToDictionary(item);
            foreach (var kvp in dic)
            {
                updatecmd.Parameters.Cast<DbParameter>()
                    .First(
                        p =>
                            p.ParameterName.ToLower() == kvp.Key.ToLower() ||
                            p.ParameterName.ToLower() == "@" + kvp.Key.ToLower())
                    .Value = kvp.Value;
            }
            updatecmd.Prepare();
            updatecmd.ExecuteNonQuery();
        }

        public virtual void UpdateRange(IEnumerable<T> items)
        {
            foreach (var item in items) Update(item);
        }

        public virtual T UpdatePartial(object item)
        {
            var key = _memberResolver.GetKeyPropertyValues(item);
            var orig = Get(key);
            _memberResolver.ApplyPartial(orig, item);
            Update(orig);
            return orig;
        }

        public virtual void Delete(params object[] key)
        {
            if (key == null) key = new object[] { };
            var cmdbuilder = CreateCommandBuilder();
            var deleteCmd = cmdbuilder.DataAdapter.DeleteCommand;
            var insertCmd = cmdbuilder.GetInsertCommand();
            var keymembers = _memberResolver.KeyPropertyMembers.ToArray();
            if (keymembers.Length != key.Length)
            {
                throw new ArgumentException("Key component count mismatch. Should match [Key] attribute count.", "key");
            }
            for (var i = 0; i < key.Length; i++)
            {
                var refparameter = insertCmd.Parameters.Cast<DbParameter>()
                    .First(p => p.SourceColumn.ToLower() == keymembers[i].Name.ToLower());
                var parameter = deleteCmd.CreateParameter();
                parameter.Value = key[i];
                parameter.DbType = refparameter.DbType;
                parameter.Size = refparameter.Size;
                parameter.ParameterName = "@" + refparameter.SourceColumn;
                deleteCmd.Parameters.Add(parameter);
            }
            if (deleteCmd.Connection.State == ConnectionState.Closed)
                deleteCmd.Connection.Open();
            deleteCmd.Prepare();
            deleteCmd.ExecuteNonQuery();
        }

        public virtual void DeleteItem(T item)
        {
            Delete(_memberResolver.GetKeyPropertyValues(item));
        }

        public virtual void DeleteRange(IEnumerable<T> items)
        {
            foreach (var item in items) Delete(item);
        }

        protected virtual List<T> GetAll()
        {

            var cmdbuilder = CreateCommandBuilder();
            var selectCmd = cmdbuilder.DataAdapter.SelectCommand;
            selectCmd.Connection = _connection;
            OpenIfClosed();
            selectCmd.Prepare();
            var reader = selectCmd.ExecuteReader();
            var items = new List<T>();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    var item = Create();
                    var row = _memberResolver.ConvertToDictionary(reader);
                    _memberResolver.HydrateFromDictionary(item, row);
                }
            }
            return items;
        }


        /// <summary>
        /// Client-side filter with LINQ-to-Objects unless overridden
        /// </summary>
        protected virtual IQueryable<T> Query
        {
            get
            {
                return GetAll().AsQueryable();
                //throw new NotImplementedException();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Query.GetEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return Query.GetEnumerator();
        }

        Expression IQueryable.Expression
        {
            get { return Query.Expression; }
        }

        Type IQueryable.ElementType
        {
            get { return Query.ElementType; }
        }

        IQueryProvider IQueryable.Provider
        {
            get { return Query.Provider; }
        }

        bool IDataStore<T>.AutoSave
        {
            get { return true; }
            set { throw new NotImplementedException(); }
        }

        void IDataStore<T>.SaveChanges()
        {
            // no-op
        }

        bool IDataStore<T>.CanQueueChanges
        {
            get { return false; }
        }

        bool IDataStore<T>.SupportsNestedRelationships
        {
            get { return false; }
        }

        bool IDataStore<T>.SupportsComplexStructures
        {
            get { return false; }
        }

        bool IDataStore<T>.SupportsTransactionScope
        {
            get { return true; }
        }

        bool IDataStore<T>.SupportsGeneratedKeys
        {
            get { return false; }
        }

        object IDataStore<T>.DataSet
        {
            get { return null; }
        }

        object IDataStore<T>.DataContext
        {
            get { return _providerFactory; }
        }

        public string StoreName
        {
            get { return typeof(T).Name; } // temporary implementation
        }

        public void Dispose()
        {
            _connection.Dispose();
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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
            var keymembers = _memberResolver.KeyPropertyMembers;
            var adapter = _providerFactory.CreateDataAdapter();
            adapter.SelectCommand = PreinitializeSelectCommand(_providerFactory.CreateCommand());
            var cmd = _providerFactory.CreateCommandBuilder();
            cmd.DataAdapter = adapter;
            adapter.InsertCommand = cmd.GetInsertCommand();
            CloneParameters(adapter.InsertCommand, adapter.SelectCommand);
            adapter.SelectCommand.CommandText = adapter.SelectCommand.CommandText.Replace("*", string.Join(",", adapter.SelectCommand.Parameters.Cast<DbParameter>().Select(p => p.SourceColumn)));
            adapter.SelectCommand.CommandText += GetIdWhereClause(adapter.SelectCommand);
            for (var p = adapter.SelectCommand.Parameters.Count - 1; p >= 0; p--)
            {
                if (!keymembers.Any(km => km.Name.ToLower() == adapter.SelectCommand.Parameters[p].SourceColumn.ToLower()))
                    adapter.SelectCommand.Parameters.RemoveAt(p);
            }
            adapter.DeleteCommand = InitializeDeleteCommand(cmd.GetDeleteCommand());
            adapter.UpdateCommand = InitializeUpdateCommand(cmd.GetUpdateCommand());
            return adapter;
        }

        private string GetIdWhereClause(DbCommand selectCommand)
        {
            return " WHERE " + string.Join(" AND ",
                            _memberResolver.KeyPropertyMembers.Select(k
                            =>
                            {
                                var param = selectCommand.Parameters.Cast<DbParameter>().First(p => p.SourceColumn.ToLower() == k.Name.ToLower());
                                return "(" + param.ParameterName + " IS NULL OR " + k.Name + " = " + param.ParameterName + ")";
                            }));
        }

        private void CloneParameters(DbCommand src, DbCommand target)
        {
            target.Parameters.AddRange(src.Parameters.Cast<DbParameter>().Select(rp =>
            {
                var param = target.CreateParameter();
                param.ParameterName = rp.ParameterName;
                param.SourceColumn = rp.SourceColumn;
                param.DbType = rp.DbType;
                param.Size = rp.Size;
                param.Precision = rp.Precision;
                param.SourceColumnNullMapping = rp.SourceColumnNullMapping;
                param.Value = rp.Value;
                return param;
            }).ToArray());
        }

        private DbCommand PreinitializeSelectCommand(DbCommand cmd)
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

        private DbCommand InitializeDeleteCommand(DbCommand cmd)
        {
            cmd.Connection = CreateConnection();
            cmd.CommandText = "DELETE FROM " + StoreName + GetIdWhereClause(cmd);
            return cmd;
        }
        private DbCommand InitializeUpdateCommand(DbCommand cmd)
        {
            cmd.Connection = CreateConnection();
            var refparams = cmd.Parameters.Cast<DbParameter>();
            cmd.CommandText = "UPDATE [" + StoreName + "] SET "
                + string.Join(", ", _memberResolver.NonKeyPropertyMembers.Where(nk => !IsComplexType(nk.PropertyType)).Select(p =>
                {
                    var param = refparams.First(rp => rp.SourceColumn.ToLower() == p.Name.ToLower());
                    return "[" + p.Name + "] = " + param.ParameterName;
                }))
                + GetIdWhereClause(cmd);
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
            using (var insertcmd = cmdbuilder.GetInsertCommand(true))
            {
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
                            param.Size = ((string)kvp.Value ?? "").Length;
                    }
                }
                insertcmd.Connection = _connection;
                if (insertcmd.Connection.State == ConnectionState.Closed)
                    insertcmd.Connection.Open();
                insertcmd.Prepare();
                insertcmd.ExecuteNonQuery();
            }            
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
                var parameter = selectCmd.Parameters.Cast<DbParameter>().First(p => p.SourceColumn.ToLower() == keymembers[i].Name.ToLower());
                parameter.Value = key[i];
            }
            var parameters = selectCmd.Parameters.Cast<DbParameter>().ToList();
            selectCmd.Parameters.PrepareParams();
            using (selectCmd.Connection)
            {
                if (selectCmd.Connection.State == ConnectionState.Closed)
                    selectCmd.Connection.Open();
                selectCmd.Prepare();
                using (var result = selectCmd.ExecuteReader())
                {
                    if (result.HasRows && result.Read())
                    {
                        var dic = result.RowAsDictionary();
                        var item = Create();
                        _memberResolver.HydrateFromDictionary(item, dic);
                        return item;
                    }
                    return default(T);
                }
            }
        }

        public virtual T Get(T lookup)
        {
            return Get(_memberResolver.GetKeyPropertyValues(lookup).ToArray());
        }

        public virtual void Update(T item)
        {
            var cmdbuilder = CreateCommandBuilder();
            var updatecmd = cmdbuilder.DataAdapter.UpdateCommand;
            var sourceDic = _memberResolver.ConvertToDictionary(item);
            var dic = _memberResolver.KeyPropertyMembers.Union(_memberResolver.NonKeyPropertyMembers.Where(p => !IsComplexType(p.PropertyType)))
                .ToDictionary(r => r.Name, v => sourceDic[v.Name]);
            foreach (var kvp in dic)
            {
                var param = updatecmd.Parameters.Cast<DbParameter>().First(rp => rp.SourceColumn.ToLower() == kvp.Key.ToLower());
                param.Value = kvp.Value;
            }
            updatecmd.Parameters.PrepareParams();
            using (updatecmd.Connection)
            {
                if (updatecmd.Connection.State == ConnectionState.Closed)
                    updatecmd.Connection.Open();
                updatecmd.Prepare();
                updatecmd.ExecuteNonQuery();
            }
        }

        public virtual void UpdateRange(IEnumerable<T> items)
        {
            foreach (var item in items) Update(item);
        }

        public virtual T UpdatePartial(object item)
        {
            var key = _memberResolver.GetKeyPropertyValues(item).ToArray();
            var orig = Get(key);
            _memberResolver.ApplyPartial(orig, item);
            Update(orig);
            return orig;
        }

        public virtual void Delete(params object[] key)
        {
            if (key.All(k => k is T)) key = _memberResolver.GetKeyPropertyValues(key.First()).ToArray();
            if (key == null) key = new object[] { };
            var cmdbuilder = CreateCommandBuilder();
            var deleteCmd = cmdbuilder.DataAdapter.DeleteCommand;
            var keymembers = _memberResolver.KeyPropertyMembers.ToArray();
            if (keymembers.Length != key.Length)
            {
                throw new ArgumentException("Key component count mismatch. Should match [Key] attribute count.", "key");
            }
            for (var i = 0; i < key.Length; i++)
            {
                var parameter = deleteCmd.Parameters.Cast<DbParameter>()
                    .First(p => p.SourceColumn.ToLower() == keymembers[i].Name.ToLower());
                parameter.Value = key[i];
            }
            deleteCmd.Parameters.PrepareParams();
            using (deleteCmd.Connection)
            {
                if (deleteCmd.Connection.State == ConnectionState.Closed)
                    deleteCmd.Connection.Open();
                deleteCmd.Prepare();
                deleteCmd.ExecuteNonQuery();
            }
        }

        public virtual void DeleteItem(T item)
        {
            Delete(_memberResolver.GetKeyPropertyValues(item).ToArray());
        }

        public virtual void DeleteRange(IEnumerable<T> items)
        {
            foreach (var item in items) Delete(item);
        }

        protected virtual List<T> GetAll()
        {

            var cmdbuilder = CreateCommandBuilder();
            var selectCmd = cmdbuilder.DataAdapter.SelectCommand;
            selectCmd.Parameters.PrepareParams();
            using (selectCmd.Connection)
            {
                if (selectCmd.Connection.State == ConnectionState.Closed)
                    selectCmd.Connection.Open();
                selectCmd.Prepare();
                var reader = selectCmd.ExecuteReader();
                var items = new List<T>();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        var item = Create();
                        var row = reader.RowAsDictionary();
                        _memberResolver.HydrateFromDictionary(item, row);
                        items.Add(item);
                    }
                }
                return items;
            }
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
            get
            {
                // temporary implementation
                var tableAttr = typeof(T).GetCustomAttribute<TableAttribute>();
                if (tableAttr != null) return tableAttr.Name;
                return typeof(T).Name;
            }
        }

        public void Dispose()
        {
            _connection.Dispose();
        }
    }
}

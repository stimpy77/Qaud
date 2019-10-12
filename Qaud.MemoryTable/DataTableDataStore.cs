using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Qaud.MemoryTable
{
    public class DataTableDataStore<T> : IDataStore<T>
    {
        private readonly DataTable _dataTable;
        private EntityMemberResolver<T> _memberResolver;

        public DataTableDataStore()
        {
            _memberResolver = new EntityMemberResolver<T>();
            _dataTable = new DataTable(typeof (T).Name);
            _dataTable.BeginInit();
            Dictionary<string, object> props = _memberResolver.ConvertToDictionary(Activator.CreateInstance<T>());
            foreach (var prop in props)
            {
                var column = new DataColumn(prop.Key, _memberResolver.TypeOfProperty(prop.Key));
                if (_memberResolver.IsAutoIdentity(prop.Key))
                {
                    column.AutoIncrement = true;
                }
                _dataTable.Columns.Add(column);
            }
            _dataTable.EndInit();
            AutoSave = true;
        }

        public virtual T Create()
        {
            return Activator.CreateInstance<T>();
        }

        public virtual void Add(T item)
        {
            Dictionary<string, object> dic = _memberResolver.ConvertToDictionary(item);
            DataRow row = _dataTable.NewRow();
            foreach (var kvp in dic)
            {
                row[kvp.Key] = kvp.Value;
            }
            _dataTable.Rows.Add(row);
            if (AutoSave) SaveChanges();
        }

        public virtual void Add(T item, out T result)
        {
            DataRow newRow = null;
            DataTableNewRowEventHandler tnr = (sender, args) => newRow = args.Row;
            _dataTable.TableNewRow += tnr;
            Add(item);
            _dataTable.TableNewRow -= tnr;
            result = Hydrate(newRow);
            SaveChanges();
        }

        public virtual void AddRange(IEnumerable<T> items)
        {
            foreach (T item in items)
            {
                Add(item);
            }
        }

        protected virtual IQueryable<T> Query
        {
            get
            {
                // TODO: OMG

                List<T> entireTableRehydrated = (
                    from DataRow row in _dataTable.Rows select Hydrate(row)
                    ).ToList();
                return entireTableRehydrated.AsEnumerable().AsQueryable();
            }
        }

        public virtual IEnumerator<T> GetEnumerator()
        {
            return this.Query.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        Type IQueryable.ElementType
        {
            get { return Query.ElementType; }
        }

        System.Linq.Expressions.Expression IQueryable.Expression
        {
            get { return Query.Expression; }
        }

        IQueryProvider IQueryable.Provider
        {
            get { return Query.Provider; }
        }

        public virtual T Get(T lookup)
        {
            var keyprops = _memberResolver.KeyPropertyMembers.ToArray();
            if (!keyprops.Any()) throw new InvalidOperationException("Type does not have key columns: " + typeof(T).FullName);
            var filter = FilterByProperties(keyprops, lookup);
            var rows = _dataTable.Select(filter);
            return Hydrate(rows.Single());
        }

        public virtual T Get(params object[] keyvalue)
        {
            var keyprops = _memberResolver.KeyPropertyMembers.ToArray();
            if (!keyprops.Any()) throw new InvalidOperationException("Type does not have key columns: " + typeof(T).FullName);
            var obj = Create();
            var i = 0;
            foreach (var prop in keyprops)
            {
                prop.SetValue(obj, keyvalue[i++]);
            }
            return Get(obj);
        }

        public virtual void Update(T item)
        {
            DataRow row = FindRow(item);
            Dictionary<string, object> dic = _memberResolver.ConvertToDictionary(item);
            foreach (var kvp in dic)
            {
                row[kvp.Key] = kvp.Value;
            }
            if (AutoSave) SaveChanges();
        }


        public virtual void UpdateRange(IEnumerable<T> items)
        {
            foreach (T item in items) Update(item);
        }

        public virtual T UpdatePartial(object item)
        {
            T current = Create();
            _memberResolver.ApplyPartial(current, item); // populate ID'd entity
            current = Hydrate(FindRow(current));         // load existing entity
            _memberResolver.ApplyPartial(current, item); // update existing entity
            Update(current);                             // save changes

            if (AutoSave) SaveChanges();

            return current;
        }

        public virtual void DeleteItem(T item)
        {
            _dataTable.Rows.Remove(FindRow(item));
            if (AutoSave) SaveChanges();
        }

        public virtual void Delete(params object[] keyvalue)
        {
            DeleteItem(Get(keyvalue));
        }

        public virtual void DeleteRange(IEnumerable<T> items)
        {
            foreach (T item in items) DeleteItem(item);
        }

        public virtual bool AutoSave { get; set; }

        bool IDataStore<T>.SupportsNestedRelationships
        {
            get { return false; }
        }

        bool IDataStore<T>.SupportsTransactionScope
        {
            get { return false; }
        }

        public virtual void SaveChanges()
        {
            _dataTable.AcceptChanges();
        }

        /// <remarks>This is "protected" for convenience not safety.</remarks>
        protected virtual DataTable DataSet
        {
            get { return _dataTable; }
        }

        object IDataStore<T>.DataSet
        {
            get { return DataSet; }
        }

        object IDataStore<T>.DataContext
        {
            get { return null; }
        }

        protected virtual T Hydrate(DataRow newRow)
        {
            if (newRow == null) return default(T);
            T ret = Create();
            var dic = new Dictionary<string, object>();
            foreach (DataColumn c in _dataTable.Columns)
            {
                dic[c.ColumnName] = newRow[c];
            }
            _memberResolver.HydrateFromDictionary(ret, dic);
            return ret;
        }

        protected virtual DataRow FindRow(T item)
        {
            IEnumerable<PropertyInfo> keyprops = _memberResolver.KeyPropertyMembers;
            string filter = "";
            if (!keyprops.Any())
            {
                filter = FilterByProperties(typeof (T).GetProperties(), item);
            }
            else filter = FilterByProperties(keyprops, item);
            DataRow[] result = _dataTable.Select(filter);
            return result.SingleOrDefault();
        }

        protected virtual string FilterByProperties(IEnumerable<PropertyInfo> keyprops, T item)
        {
            var expr = new StringBuilder();
            foreach (PropertyInfo prop in keyprops)
            {
                if (expr.Length > 0) expr.Append(" AND ");

                string name = prop.Name;
                DataColumn col = _dataTable.Columns[name];
                Type coltype = col.DataType;
                object val = typeof (T).GetProperty(name).GetValue(item);

                expr.Append(name + " = ");

                bool esc = coltype == typeof (string) || coltype == typeof (DateTime);
                if (esc) expr.Append("'");
                expr.Append(val.ToString().Replace("'", "''"));
                if (esc) expr.Append("'");
            }
            return expr.ToString();
        }

        bool IDataStore<T>.SupportsComplexStructures
        {
            get { return false; }
        }

        /// <summary>
        /// Gets whether the data store implementation supports 
        /// <see cref="System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedAttribute"/>, particularly
        /// <see cref="System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.Identity"/>
        /// </summary>
        bool IDataStore<T>.SupportsGeneratedKeys
        {
            get { return true; }
        }

        /// <summary>
        /// Indicates whether setting <see cref="AutoSave"/> to <value>false</value> has any effect.
        /// </summary>
        bool IDataStore<T>.CanQueueChanges
        {
            get { return true; }
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

    }
}
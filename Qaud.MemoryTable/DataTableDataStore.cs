using System;
using System.Collections.Generic;
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

        public virtual IQueryable<T> Query
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

        public virtual T FindMatch(T lookup)
        {
            var keyprops = _memberResolver.KeyPropertyMembers.ToArray();
            if (!keyprops.Any()) throw new InvalidOperationException("Type does not have key columns: " + typeof(T).FullName);
            var filter = FilterByProperties(keyprops, lookup);
            var rows = _dataTable.Select(filter);
            return Hydrate(rows.Single());
        }

        public virtual T Find(params object[] keyvalue)
        {
            var keyprops = _memberResolver.KeyPropertyMembers.ToArray();
            if (!keyprops.Any()) throw new InvalidOperationException("Type does not have key columns: " + typeof(T).FullName);
            var obj = Create();
            var i = 0;
            foreach (var prop in keyprops)
            {
                prop.SetValue(obj, keyvalue[i++]);
            }
            return FindMatch(obj);
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

        public virtual void Delete(T item)
        {
            _dataTable.Rows.Remove(FindRow(item));
            if (AutoSave) SaveChanges();
        }

        public virtual void DeleteByKey(params object[] keyvalue)
        {
            Delete(Find(keyvalue));
        }

        public virtual void DeleteRange(IEnumerable<T> items)
        {
            foreach (T item in items) Delete(item);
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
        protected virtual DataTable DataSetImplementation
        {
            get { return _dataTable; }
        }

        object IDataStore<T>.DataSetImplementation
        {
            get { return DataSetImplementation; }
        }

        object IDataStore<T>.DataContextImplementation
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
            get { return false; }
        }
    }
}
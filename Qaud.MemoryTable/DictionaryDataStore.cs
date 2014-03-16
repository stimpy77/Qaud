using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Qaud.MemoryTable
{
    public class DictionaryDataStore<T> : IDataStore<T>, ICollection<T>, IDictionary<string, T>
    {
        private readonly SortedDictionary<string, T> _dictionary;
        private readonly EntityMemberResolver<T> _memberResolver;

        public DictionaryDataStore()
        {
            _dictionary = new SortedDictionary<string, T>();
            _memberResolver = new EntityMemberResolver<T>();
            KeyJoinDelimeter = "|";
        }

        public DictionaryDataStore(params T[] items)
            : this()
        {
            foreach (var item in items)
            {
                Add(item);
            }
        }

        public DictionaryDataStore(IEnumerable<T> collection)
            : this()
        {
            foreach (var item in collection) Add(item);
        } 

        public DictionaryDataStore(IList<T> list)
            : this((IEnumerable<T>) list)
        {
            _memberResolver = new EntityMemberResolver<T>();
        }

        ///////////////////////////////////////////////////////
        
        public T Create()
        {
            return Activator.CreateInstance<T>();
        }

        public void Add(T item)
        {
            var key = GetItemKey(item);
            _dictionary.Add(key, item);
        }

        public void Add(T item, out T result)
        {
            var keys = _memberResolver.GetKeyPropertyValues(item);
            var key = string.Join(KeyJoinDelimeter, keys.Select(x => x.ToString()));
            _dictionary.Add(key, item);
            result = item;
        }

        public void AddRange(IEnumerable<T> items)
        {
            foreach (var item in items) Add(item);
        }

        public IQueryable<T> Query
        {
            get { return _dictionary.Select(i=>i.Value).AsQueryable(); }
        }

        public T Get(T lookup)
        {
            return Get(_memberResolver.GetKeyPropertyValues(lookup).ToArray());
        }

        public T Get(params object[] keyvalue)
        {
            var key = string.Join(KeyJoinDelimeter, keyvalue.Select(k => k.ToString()).ToArray());
            if (_dictionary.ContainsKey(key)) return _dictionary[key];
            return default(T);
        }

        public void Update(T item)
        {
            var matchingItem = Get(item);
            _memberResolver.HydrateFromDictionary(matchingItem, _memberResolver.ConvertToDictionary(item));
        }

        public void UpdateRange(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                Update(item);
            }
        }

        public T UpdatePartial(object item)
        {
            var keys = _memberResolver.GetKeyPropertyValues(item).ToArray();
            var current = Get(keys);
            _memberResolver.ApplyPartial(current, item);
            Update(current);
            return current;
        }

        public void DeleteItem(T item)
        {
            var matchingItem = Get(item);
            _dictionary.Remove(GetItemKey(matchingItem));
        }

        public void Delete(params object[] keyvalue)
        {
            var matchingItem = Get(keyvalue);
            _dictionary.Remove(GetItemKey(matchingItem));
        }

        public void DeleteRange(IEnumerable<T> items)
        {
            var matchingItems = items.Select(Get);
            foreach (var item in matchingItems)
            {
                _dictionary.Remove(GetItemKey(item));
            }
        }

        public bool AutoSave
        {
            get { return true; }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool SupportsNestedRelationships
        {
            get { return false; }
        }

        public bool SupportsComplexStructures
        {
            get { return true; }
        }

        public bool SupportsTransactionScope
        {
            get { return false; }
        }

        public void SaveChanges()
        {
            //throw new NotImplementedException();
        }

        object IDataStore<T>.DataSet
        {
            get { return _dictionary; }
        }

        object IDataStore<T>.DataContext
        {
            get { return null; }
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

        void ICollection<T>.Clear()
        {
            _dictionary.Clear();
        }

        bool ICollection<T>.Contains(T item)
        {
            return _dictionary.Select(x => x.Value).Contains(item);
        }

        void ICollection<T>.CopyTo(T[] array, int arrayIndex)
        {
            var items = _dictionary.Select(item => item.Value).ToList();
            items.CopyTo(array, arrayIndex);
        }

        int ICollection<T>.Count
        {
            get { return _dictionary.Count; }
        }

        bool ICollection<T>.IsReadOnly
        {
            get { return ((ICollection<T>)_dictionary).IsReadOnly; }
        }

        bool ICollection<T>.Remove(T item)
        {
            return _dictionary.Remove(GetItemKey(item));
        }

        void ICollection<T>.Add(T item)
        {
            Add(item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _dictionary.Select(items=>items.Value).GetEnumerator();
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

        void IDictionary<string, T>.Add(string key, T value)
        {
            _dictionary.Add(key, value);
        }

        bool IDictionary<string, T>.ContainsKey(string key)
        {
            return _dictionary.ContainsKey(key);
        }

        ICollection<string> IDictionary<string, T>.Keys
        {
            get { return _dictionary.Keys; }
        }

        bool IDictionary<string, T>.Remove(string key)
        {
            return _dictionary.Remove(key);
        }

        bool IDictionary<string, T>.TryGetValue(string key, out T value)
        {
            return _dictionary.TryGetValue(key, out value);
        }

        ICollection<T> IDictionary<string, T>.Values
        {
            get { return _dictionary.Values; }
        }

        T IDictionary<string, T>.this[string key]
        {
            get { return _dictionary[key]; }
            set { _dictionary[key] = value; }
        }

        void ICollection<KeyValuePair<string, T>>.Add(KeyValuePair<string, T> item)
        {
            _dictionary.Add(item.Key, item.Value);
        }

        void ICollection<KeyValuePair<string, T>>.Clear()
        {
            _dictionary.Clear();
        }

        bool ICollection<KeyValuePair<string, T>>.Contains(KeyValuePair<string, T> item)
        {
            return _dictionary.ContainsKey(item.Key) && (object)_dictionary[item.Key] == (object)item.Value;
        }

        void ICollection<KeyValuePair<string, T>>.CopyTo(KeyValuePair<string, T>[] array, int arrayIndex)
        {
            ((IDictionary<string, T>)_dictionary).CopyTo(array, arrayIndex);
        }

        int ICollection<KeyValuePair<string, T>>.Count
        {
            get { return _dictionary.Count; }
        }

        bool ICollection<KeyValuePair<string, T>>.IsReadOnly
        {
            get { return false; }
        }

        bool ICollection<KeyValuePair<string, T>>.Remove(KeyValuePair<string, T> item)
        {
            if (((ICollection<KeyValuePair<string, T>>) this).Contains(item))
            {
                return _dictionary.Remove(item.Key);
            }
            return false;
        }

        IEnumerator<KeyValuePair<string, T>> IEnumerable<KeyValuePair<string, T>>.GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        protected virtual string KeyJoinDelimeter { get; set; }

        protected virtual string GetItemKey(T item)
        {
            if (!_memberResolver.KeyPropertyMembers.Any())
            {
                throw new InvalidOperationException("The type does not have a KeyAttribute: " + item.GetType().FullName);
            }
            var keys = _memberResolver.GetKeyPropertyValues(item);
            var key = string.Join(KeyJoinDelimeter, keys);
            if (string.IsNullOrEmpty(key))
            {
                throw new MissingPrimaryKeyException("Invalid key in " + item.GetType().FullName);
            }
            return key;
        }

        /// <summary>
        /// Indicates whether setting <see cref="AutoSave"/> to <value>false</value> has any effect.
        /// </summary>
        bool IDataStore<T>.CanQueueChanges
        {
            get { return true; }
        }
    }
}

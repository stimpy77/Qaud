using System;
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

        public T FindMatch(T lookup)
        {
            return Find(_memberResolver.GetKeyPropertyValues(lookup).ToArray());
        }

        public T Find(params object[] keyvalue)
        {
            var key = string.Join(KeyJoinDelimeter, keyvalue.Select(k => k.ToString()).ToArray());
            if (_dictionary.ContainsKey(key)) return _dictionary[key];
            return default(T);
        }

        public void Update(T item)
        {
            var matchingItem = FindMatch(item);
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
            var current = Find(keys);
            _memberResolver.ApplyPartial(current, item);
            Update(current);
            return current;
        }

        public void Delete(T item)
        {
            var matchingItem = FindMatch(item);
            _dictionary.Remove(GetItemKey(matchingItem));
        }

        public void DeleteByKey(params object[] keyvalue)
        {
            var matchingItem = Find(keyvalue);
            _dictionary.Remove(GetItemKey(matchingItem));
        }

        public void DeleteRange(IEnumerable<T> items)
        {
            var matchingItems = items.Select(FindMatch);
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

        object IDataStore<T>.DataSetImplementation
        {
            get { return _dictionary; }
        }

        object IDataStore<T>.DataContextImplementation
        {
            get { return null; }
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

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        void ICollection<T>.Add(T item)
        {
            Add(item);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return _dictionary.Select(items=>items.Value).GetEnumerator();
        }

        void IDictionary<string, T>.Add(string key, T value)
        {
            throw new NotImplementedException();
        }

        bool IDictionary<string, T>.ContainsKey(string key)
        {
            throw new NotImplementedException();
        }

        ICollection<string> IDictionary<string, T>.Keys
        {
            get { throw new NotImplementedException(); }
        }

        bool IDictionary<string, T>.Remove(string key)
        {
            throw new NotImplementedException();
        }

        bool IDictionary<string, T>.TryGetValue(string key, out T value)
        {
            throw new NotImplementedException();
        }

        ICollection<T> IDictionary<string, T>.Values
        {
            get { throw new NotImplementedException(); }
        }

        T IDictionary<string, T>.this[string key]
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        void ICollection<KeyValuePair<string, T>>.Add(KeyValuePair<string, T> item)
        {
            throw new NotImplementedException();
        }

        void ICollection<KeyValuePair<string, T>>.Clear()
        {
            throw new NotImplementedException();
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

        public virtual string KeyJoinDelimeter { get; set; }

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
    }
}
